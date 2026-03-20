using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CW_hammer.Models
{
    public class AnimalPhoto
    {
        public int ID { get; set; }              // унікальний запис фото
        public int AnimalCardID { get; set; }    // FK на тварину
        public string FilePath { get; set; }     // шлях до файлу
        public AnimalCards AnimalCard { get; set; }
    }
}


