using System;
using System.Collections.Generic;

namespace AirwayAPI.Models;

public partial class TrkAdjustment
{
    public int Id { get; set; }

    public string? ItemNum { get; set; }

    public int? QtyAdjustment { get; set; }

    public string? Reason { get; set; }

    public DateTime? EntryDate { get; set; }

    public int? EnteredBy { get; set; }

    public string? ScanGroup { get; set; }
}
