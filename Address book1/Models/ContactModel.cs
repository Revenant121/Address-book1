using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Address_book1.Models
{
    public class ContactModel
    {
        public Guid contactid { get; set; }
        public string FirstName {  get; set; }
        public string LastName { get; set; }
        public string address { get; set; }
        public string phone_number { get; set; } 

        public double Latitude {  get; set; }

        public double Longitude { get; set; }

    }
}
