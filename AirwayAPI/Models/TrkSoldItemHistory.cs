using System;
using System.Collections.Generic;

namespace AirwayAPI.Models;

public partial class TrkSoldItemHistory
{
    public int RowId { get; set; }

    public int? RequestId { get; set; }

    public DateTime? DateMarkedBought { get; set; }

    public string? Username { get; set; }

    public string? PageName { get; set; }
}
