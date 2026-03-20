using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CW_hammer.Models
{
    public class MedicalHistory
    {
        public int ID { get; set; }  // унікальний ID випадку захворювання 

        public int AnimalCardID { get; set; }
        public int DiseaseDirectoryID { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool DiseaseState { get; set; } = true;

        public string? Complaint { get; set; }     // звернення
        public string? Prescription { get; set; }  // рецепт  від ветеренара

        public AnimalCards AnimalCard { get; set; }
        public DiseaseDirectory DiseaseDirectory { get; set; }
    }
}
