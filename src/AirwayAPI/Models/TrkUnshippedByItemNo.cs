namespace AirwayAPI.Models;

public partial class TrkUnshippedByItemNo
{
    public int RowId { get; set; }

    public string? SoNo { get; set; }

    public DateTime? DateRecorded { get; set; }

    public string? ItemNum { get; set; }

    public int? QtyOrdered { get; set; }

    public int? QtyShipped { get; set; }

    public int? QtyUnshipped { get; set; }

    public string? AccountNum { get; set; }

    public string? SalesTeam { get; set; }

    public decimal? UnitPrice { get; set; }

    public decimal? UnshippedValue { get; set; }
}
