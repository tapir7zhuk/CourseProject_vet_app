using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace CW_hammer.Models
{
    public class PetOwner
    {
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [RegularExpression(@"^\+380\d{9}$",
            ErrorMessage = "Телефон має бути у форматі +380XXXXXXXXX")]
        public string Phone { get; set; }
        public string Address { get; set; }

        [RegularExpression(@"^\d{5}$")]
        public string ZipCode { get; set; }

        public string City { get; set; }
        public ICollection<AnimalCards> AnimalCards { get; set; }
    }
}
