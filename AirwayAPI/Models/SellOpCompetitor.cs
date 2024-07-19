using System;
using System.Collections.Generic;

namespace AirwayAPI.Data;

public partial class SellOpCompetitor
{
    public int Id { get; set; }

    public int? EventId { get; set; }

    public string? Company { get; set; }

    public string? CompType { get; set; }

    public DateTime? EntryDate { get; set; }
}
