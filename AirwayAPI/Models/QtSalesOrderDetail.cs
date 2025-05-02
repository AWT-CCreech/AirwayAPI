namespace AirwayAPI.Models;

public partial class QtSalesOrderDetail
{
    public int Id { get; set; }

    public int? SaleId { get; set; }

    public int? RequestId { get; set; }

    public int? SaleOrder { get; set; }

    public int? QuoteQty { get; set; }

    public int? QtySold { get; set; }

    public string? UnitMeasure { get; set; }

    public string? PartNum { get; set; }

    public string? PartDesc { get; set; }

    public double? UnitPrice { get; set; }

    public double? ExtendedPrice { get; set; }

    public decimal? CurUnitPrice { get; set; }

    public decimal? CurExtPrice { get; set; }

    public bool? AutoSoflag { get; set; }

    public bool? UpdatedByAutoSo { get; set; }

    public bool? Soflag { get; set; }
}
