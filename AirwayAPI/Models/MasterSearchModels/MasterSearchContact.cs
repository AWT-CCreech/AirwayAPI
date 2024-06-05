using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AirwayAPI.Models.MasterSearchModels
{
    public class MasterSearchContact
    {
        public int Id { get; set; }
        public string Contact { get; set; }
        public string Company { get; set; }
        public string State { get; set; }
        public string PhoneMain { get; set; }
        public bool ActiveStatus { get; set; }
    }
}
