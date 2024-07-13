using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetCrudApp.Data;
using NetCrudApp.Entity;

namespace NetCrudApp.Controllers
{
    [Route("fhir/patient")]
    [ApiController]
    public class PatientResourceProvider : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PatientResourceProvider(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPatient(string id)
        {
            try
            {
                //https://stackoverflow.com/questions/62899915/converting-null-literal-or-possible-null-value-to-non-nullable-type
                PatientEntity? patientEntity = await _context.Patients.FindAsync(id);
                if(patientEntity != null)
                {
                    return Ok(patientEntity);
                }else
                {
                    return NotFound(new { Message = "Patient not found" });
                }

            }catch(FhirOperationException ex) when (ex.Status == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound(new { Message = "Patient not found" });
            }catch(Exception ex)
            {
                return StatusCode(500, new { Message = "Ann error occurred", Details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePatient([FromBody] Patient patient)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                PatientEntity patientEntity = PatientEntity.FromFhirPatient(patient);
                _context.Patients.Add(patientEntity);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetPatient), new { id = patientEntity.Id }, patientEntity.ToFhirPatient());
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();  
                return StatusCode(500, new { Message = "An error occurred", Details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(string id, [FromBody] Patient patient)
        {
            if(id == null)
            {
                return BadRequest(new {Message = "Patient id cannot be null"});
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                PatientEntity? patientEntity = await _context.Patients.FindAsync(id);
                if(patientEntity == null)
                {
                    return NotFound(new { Message = "Patient not found" });
                }

                if(patient.Name == null || patient.Name.Count == 0 || string.IsNullOrEmpty(patient.Name[0].Family)) {
                    return BadRequest(new { Message = "Patient name is required" });
                }

                patientEntity.FamilyName = patient.Name.FirstOrDefault()?.Family ?? string.Empty; 
                patientEntity.GivenName = patient.Name.FirstOrDefault()?.Given.FirstOrDefault() ?? string.Empty;
                patientEntity.Gender = patient.Gender.HasValue ? patient.Gender.Value.ToString().ToLower() : string.Empty;
                patientEntity.BirthDate = patient.BirthDateElement?.ToDateTimeOffset()?.DateTime ?? DateTime.MinValue;

                _context.Entry(patientEntity).State = EntityState.Modified;

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return NoContent();
            }catch(DbUpdateConcurrencyException) 
            {
                if(!_context.Patients.Any(e => e.Id == id))
                {
                    return NotFound(new { Message = "Patient not found" });
                }
                else
                {
                    throw;
                }
            }catch(Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { Message = "An error occurred", Details = ex.Message });
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatient(string id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var patientEntity = await _context.Patients.FindAsync(id);
                if(patientEntity == null)
                {
                    return NotFound(new { Message = "Patient not found" });
                }
                _context.Patients.Remove(patientEntity);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return NoContent();
            }catch(Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { Message = "An error occurred", Details = ex.Message });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchPatient([FromQuery] string? name = null, string? id = null)
        {
            try
            {
                IQueryable<PatientEntity> query = _context.Patients;
                if (!string.IsNullOrEmpty(name))
                {
                    query = query.Where(n =>  n.FamilyName == name || n.GivenName == name);
                }
                if (!string.IsNullOrEmpty(id))
                {
                    query = query.Where(p => p.Id == id);
                }

                List<PatientEntity> patients = await query.ToListAsync();
                if (!patients.Any())
                {
                    return NotFound(new { Message = "No Patient found" });
                }
                return Ok(patients);
            }catch(Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred", Details = ex.Message });
            }
        }

    }
}
