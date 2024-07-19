using System;
using System.Collections.Generic;

namespace AirwayAPI.Models;

public partial class TcHistory
{
    public int RowId { get; set; }

    public string? Employee { get; set; }

    public DateTime? EntryDate { get; set; }

    public string? EnteredBy { get; set; }

    public string? Ptohistory { get; set; }
}
