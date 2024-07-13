using Hl7.Fhir.Model;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
namespace NetCrudApp.Entity
{
    public class PatientEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = Guid.NewGuid().ToString();


        [Required]
        public string FamilyName { get; set; } = string.Empty;

        [Required]
        public string GivenName { get; set; } = string.Empty;

        [Required]
        public string Gender { get; set; } = string.Empty;

        [Required]
        public DateTime BirthDate { get; set; } = DateTime.MinValue;

        public byte[]? PhotoData { get; set; }

        public static PatientEntity FromFhirPatient(Patient patient)
        {
            byte[]? photoData = null;
            if(patient.Photo != null && patient.Photo.Count > 0 && patient.Photo[0].Data != null)
            {
                photoData = patient.Photo[0].Data;
            }
            DateTime birthDate = patient.BirthDateElement?.ToDateTimeOffset()?.DateTime ?? DateTime.MinValue;

            return new PatientEntity
            {
                Id = patient.Id ?? Guid.NewGuid().ToString(),
                FamilyName = patient.Name.FirstOrDefault()?.Family ?? string.Empty,
                GivenName = patient.Name.FirstOrDefault()?.Given.FirstOrDefault()?? string.Empty,
                Gender = patient.Gender.HasValue ? patient.Gender.Value.ToString(): string.Empty,
                BirthDate = birthDate,
                PhotoData = photoData
            };
        }

        public Patient ToFhirPatient()
        {
            return new Patient
            {
                Id = Id,
                Name = new List<HumanName>
                {
                    new HumanName
                    {
                        Family = FamilyName,
                        Given = new List<string> { GivenName }
                    }
                },
                Gender = !string.IsNullOrEmpty(Gender) ? (AdministrativeGender)Enum.Parse(typeof(AdministrativeGender), Gender, true) : null,
                BirthDate = BirthDate.ToString("yyyy-MM-dd"),
                Photo = PhotoData != null ? new List<Attachment> { new Attachment { Data = PhotoData } } : null
            };
        }
    }
}
