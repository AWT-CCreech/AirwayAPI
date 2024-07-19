using System;
using System.Collections.Generic;

namespace AirwayAPI.Data;

public partial class MasterSearchQuery
{
    public int RowId { get; set; }

    public string? SearchText { get; set; }

    public string? SearchFor { get; set; }

    public string? SearchType { get; set; }

    public bool? EventId { get; set; }

    public bool? SoNo { get; set; }

    public bool? PoNo { get; set; }

    public bool? InvNo { get; set; }

    public bool? PartNo { get; set; }

    public bool? PartDesc { get; set; }

    public bool? Company { get; set; }

    public bool? Mfg { get; set; }

    public string? SearchBy { get; set; }

    public DateTime? SearchDate { get; set; }
}
