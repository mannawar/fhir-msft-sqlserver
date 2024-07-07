using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
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
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                PatientEntity patientEntity = PatientEntity.FromFhirPatient(patient);
                _context.Patients.Add(patientEntity);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetPatient), new { id = patientEntity.Id }, patientEntity.ToFhirPatient());   
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred", Details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(string id, [FromBody]Patient patient)
        {
            if(id == null)
            {
                return BadRequest();
            }
            try
            {
                PatientEntity? patientEntity = await _context.Patients.FindAsync(id);
                if(patientEntity == null)
                {
                    return NotFound(new { Message = "Patient not found" });
                }

                patientEntity.UpdateFromFhirPatient(patient);
                _context.Entry(patientEntity).State = EntityState.Modified;

                await _context.SaveChangesAsync();
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
                return StatusCode(500, new { Message = "An error occurred", Details = ex.Message });
            }
            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatient(string id)
        {
            try
            {
                var patientEntity = await _context.Patients.FindAsync(id);
                if(patientEntity == null)
                {
                    return NotFound(new { Message = "Patient not found" });
                }
                _context.Patients.Remove(patientEntity);
                await _context.SaveChangesAsync();
                return NoContent();
            }catch(Exception ex)
            {
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

                var patients = await query.ToListAsync();
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
