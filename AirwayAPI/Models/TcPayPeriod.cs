using System;
using System.Collections.Generic;

namespace AirwayAPI.Models;

public partial class TcPayPeriod
{
    public int RowId { get; set; }

    public int? PayPeriod { get; set; }

    public DateTime? Date1 { get; set; }

    public DateTime? Date2 { get; set; }
}
