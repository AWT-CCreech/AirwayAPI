﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AirwayAPI.Models.TimeTrackerModels
{
    public class ApprovalList
    {
        public int[] UserIds { get; set; }
        public bool[] PreviousPeriods { get; set; }
    }
}
