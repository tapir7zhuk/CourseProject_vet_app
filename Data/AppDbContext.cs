using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CW_hammer.Models;
using Microsoft.EntityFrameworkCore;

using CW_hammer.Models;
using Microsoft.EntityFrameworkCore;

namespace CW_hammer.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<AnimalCards> AnimalCards { get; set; }
        public DbSet<PetOwner> PetOwner { get; set; }
        public DbSet<MedicalHistory> MedicalHistories { get; set; }
        public DbSet<DiseaseDirectory> DiseaseDirectories { get; set; }
        public DbSet<AnimalPhoto> AnimalPhotos { get; set; }
        public DbSet<Vaccination> Vaccinations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Vaccination>().ToTable("Vaccination");
        }
    }
}
