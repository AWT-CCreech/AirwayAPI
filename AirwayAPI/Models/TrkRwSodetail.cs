using System;
using System.Collections.Generic;

namespace AirwayAPI.Models;

public partial class TrkRwSodetail
{
    public int RowId { get; set; }

    public string? Sonum { get; set; }

    public string? CompanyId { get; set; }

    public int? OrderType { get; set; }

    public int? ItemLineNo { get; set; }

    public string? ItemNum { get; set; }

    public string? ItemDesc { get; set; }

    public string? AccountTeam { get; set; }

    public int? QtyOrdered { get; set; }

    public int? QtyShipped { get; set; }

    public int? QtyPicked { get; set; }

    public int? QtyOpenToShip { get; set; }

    public decimal? UnitPrice { get; set; }

    public decimal? ExtTotal { get; set; }
}
