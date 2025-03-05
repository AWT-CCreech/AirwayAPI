using System;
using System.Collections.Generic;

namespace AirwayAPI.Models;

public partial class MassMailHistory
{
    public int Id { get; set; }

    public int? MassMailId { get; set; }

    public string? CompanyName { get; set; }

    public string? ContactName { get; set; }

    public int? RequestId { get; set; }

    public string? PartNum { get; set; }

    public string? AltPartNum { get; set; }

    public string? PartDesc { get; set; }

    public int? Qty { get; set; }

    public DateTime? DateSent { get; set; }

    public bool? RespondedTo { get; set; }
}
