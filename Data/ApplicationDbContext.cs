using Hl7.Fhir.Model;
using Microsoft.EntityFrameworkCore;
using NetCrudApp.Entity;

namespace NetCrudApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        
        public DbSet<PatientEntity> Patients { get; set; }
   
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PatientEntity>().Ignore(p => p.Base64BinaryObjectValue);
            base.OnModelCreating(modelBuilder);
        }
    }
}
