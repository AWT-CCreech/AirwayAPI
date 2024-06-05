using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AirwayAPI.Models.TimeTrackerModels
{
    public class TimeTrackerReportEmailInput
    {
        public string Body { get; set; }
        public string SenderUserName { get; set; }
        public string Password { get; set; }
        public bool previousPeriod { get; set; }
    }
}
