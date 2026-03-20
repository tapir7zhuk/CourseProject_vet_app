using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CW_hammer.Models
{
    public class DiseaseDirectory
    {
        public int ID { get; set; }

        [Required]
        public string Name { get; set; } = null!;   // захворювання

        public string? Category { get; set; } // інфекційна, травма і тощо

        public ICollection<MedicalHistory> MedicalHistories { get; set; }
    }
}
