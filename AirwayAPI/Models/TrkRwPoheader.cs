using System;
using System.Collections.Generic;

namespace AirwayAPI.Data;

public partial class TrkRwPoheader
{
    public string? PotrackNum { get; set; }

    public string? Ponum { get; set; }

    public string? CompanyId { get; set; }

    public string? VendorNum { get; set; }

    public string? IssuedBy { get; set; }

    public string? Podesc { get; set; }

    public DateTime? RequiredDate { get; set; }

    public DateTime? ActualCloseDate { get; set; }

    public DateTime? CrtDate { get; set; }

    public DateTime? ApprovalDate { get; set; }

    public DateTime? IssueDate { get; set; }

    public int? Postatus { get; set; }

    public string? CustomerNum { get; set; }

    public string? OrderNum { get; set; }

    public decimal? Poamt { get; set; }
}
