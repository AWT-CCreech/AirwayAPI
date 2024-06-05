using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AirwayAPI.Models.MasterSearch
{
    public class SearchInput
    {
        public string Search { get; set; }
        public bool ID { get; set; }
        public bool SONo { get; set; }
        public bool PartNo { get; set; }
        public bool PartDesc { get; set; }
        public bool PONo { get; set; }
        public bool Mfg { get; set; }
        public bool Company { get; set; }
        public bool InvNo { get; set; }
        public string Uname { get; set; }
    }
}
