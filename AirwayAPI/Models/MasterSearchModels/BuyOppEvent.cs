using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AirwayAPI.Models.MasterSearch
{
    public class BuyOppEvent
    {
        public int EventId { get; set; }
        public string Manufacturer { get; set; }
        public string Platform { get; set; }
        public string Frequency { get; set; }
        public DateTime? BidDueDate { get; set; }
        public string StatusCash { get; set; }
        public string StatusConsignment { get; set; }
        public DateTime? EntryDate { get; set; }
        public string Company { get; set; }
        public string Lname { get; set; }
        public string Technology { get; set; }
        public string Comments { get; set; }
    }
}
