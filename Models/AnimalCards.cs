using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CW_hammer.Models
{
    public class AnimalCards
    {
        public int ID { get; set; }
        public int PetOwnerID { get; set; }

        [Required]
        public string Name { get; set; }

        public bool IsAlive { get; set; } = true;
        public string Breed { get; set; }
        public string Species { get; set; }
        public string Coloring { get; set; }
        public DateTime BirthDate { get; set; }
        public enum SexEnum
        {
            Male = 0,
            Female = 1,
            Undefined = 2
        }

        public SexEnum Sex { get; set; }
        public bool Sterile { get; set; }
        public PetOwner PetOwner { get; set; }
        public ICollection<Vaccination> Vaccinations { get; set; }
        public ICollection<AnimalPhoto> Photos { get; set; }
    }

}


