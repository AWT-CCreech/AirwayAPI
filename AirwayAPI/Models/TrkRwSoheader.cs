using System;
using System.Collections.Generic;

namespace AirwayAPI.Data;

public partial class TrkRwSoheader
{
    public string? OrderNum { get; set; }

    public int? Type { get; set; }

    public string? Description { get; set; }

    public int? Status { get; set; }

    public DateTime? OrderDate { get; set; }

    public DateTime? RequiredDate { get; set; }

    public string? CustNum { get; set; }

    public string? CustomerName { get; set; }

    public string? CustPo { get; set; }

    public string? Terms { get; set; }

    public string? ShipToNum { get; set; }

    public string? AccountTeam { get; set; }

    public decimal? QuoteTotal { get; set; }

    public string? Comments { get; set; }

    public string? Rmanum { get; set; }

    public double? ReturnAmt { get; set; }
}
