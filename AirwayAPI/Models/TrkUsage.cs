using System;
using System.Collections.Generic;

namespace AirwayAPI.Data;

public partial class TrkUsage
{
    public int RowId { get; set; }

    public int? AppId { get; set; }

    public string? Uname { get; set; }

    public string? Ipaddress { get; set; }

    public DateTime? EntryDate { get; set; }
}
