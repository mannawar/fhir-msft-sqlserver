using Hl7.Fhir.Model;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace NetCrudApp.Entity
{
    public class PatientEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        public string? FamilyName { get; set; }
        public string? GivenName { get; set; }
        public string? Gender { get; set; }
        public DateTime? BirthDate { get; set; }

        [NotMapped]
        public object? Base64BinaryObjectValue { get; set; }

        public byte[]? PhotoData { get; set; }
        public static PatientEntity FromFhirPatient(Patient patient)
        {
            byte[]? photoData = null;
            if(patient.Photo != null && patient.Photo.Count > 0 && patient.Photo[0].Data != null)
            {
                photoData = patient.Photo[0].Data;
            }
            return new PatientEntity
            {
                Id = patient.Id,
                FamilyName = patient.Name.FirstOrDefault()?.Family,
                GivenName = patient.Name.FirstOrDefault()?.Given.FirstOrDefault(),
                Gender = patient.Gender.HasValue ? patient.Gender.Value.ToString() : null,
                BirthDate = patient.BirthDateElement?.ToDateTimeOffset()?.DateTime,
                Base64BinaryObjectValue = "hi Mannawar",
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
                Gender = Gender != null ? (AdministrativeGender)Enum.Parse(typeof(AdministrativeGender), Gender, true) : null,
                BirthDate = BirthDate?.ToString("yyyy-MM-dd"),
                Photo = PhotoData != null ? new List<Attachment> { new Attachment { Data = PhotoData } } : null
            };
        }

        public void UpdateFromFhirPatient(Patient patient)
        {
            FamilyName = patient.Name.FirstOrDefault()?.Family;
            GivenName = patient.Name.FirstOrDefault()?.Given.FirstOrDefault();
            Gender = patient.Gender.HasValue ? patient.Gender.Value.ToString().ToLower() : null;
            BirthDate = patient.BirthDateElement?.ToDateTimeOffset()?.DateTime;
        }
    }
}
