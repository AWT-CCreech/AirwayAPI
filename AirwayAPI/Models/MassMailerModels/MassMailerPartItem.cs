using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AirwayAPI.Data
{
    public class MassMailerPartItem
    {
        public int Id { get; set; }
        public string PartNum { get; set; }
        public string AltPartNum { get; set; }
        public string PartDesc { get; set; }
        public double? Qty { get; set; }
        public string Company { get; set; }
        public string Manufacturer { get; set; }
        public string Revision { get; set; }
    }
}
