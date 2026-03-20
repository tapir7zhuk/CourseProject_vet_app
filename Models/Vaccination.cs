using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CW_hammer.Models
{
    public class Vaccination
    {
        public int ID { get; set; } // унікальний номер щеплення

        public int AnimalCardID { get; set; } // id тварини
        public string Manufacturer { get; set; }
        public string Purpose { get; set; } // призанчення

        [RegularExpression(@"^\d{6}$")]
        public string SerialNumber { get; set; }

        public DateTime VaccinationDate { get; set; }
        public DateTime ValidUntil { get; set; }
        public virtual AnimalCards AnimalCard { get; set; }

    }
}
