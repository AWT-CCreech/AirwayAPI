namespace AirwayAPI.Models;

public partial class QtQuoteDetail
{
    public int Id { get; set; }

    public int? QuoteId { get; set; }

    public int? RequestId { get; set; }

    public int? QuoteOrder { get; set; }

    public int? QtyRequested { get; set; }

    public int? QtyFound { get; set; }

    public int? QuoteQty { get; set; }

    public string? UnitMeasure { get; set; }

    public string? PartNum { get; set; }

    public string? MfgPartNum { get; set; }

    public string? PartDesc { get; set; }

    public decimal? Cost { get; set; }

    public double? UnitPrice { get; set; }

    public double? ExtendedPrice { get; set; }

    public decimal? CurCost { get; set; }

    public decimal? CurUnitPrice { get; set; }

    public decimal? CurExtPrice { get; set; }

    public string? Comments { get; set; }

    public int? QtyAvailable { get; set; }
}
