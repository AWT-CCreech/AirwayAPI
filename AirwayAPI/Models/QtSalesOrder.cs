namespace AirwayAPI.Models;

public partial class QtSalesOrder
{
    public int SaleId { get; set; }

    public int? QuoteId { get; set; }

    public int? EventId { get; set; }

    public int? Version { get; set; }

    public string? BillToCompanyName { get; set; }

    public string? BillToCustNum { get; set; }

    public string? BtAddr1 { get; set; }

    public string? BtAddr2 { get; set; }

    public string? BtAddr3 { get; set; }

    public string? BtAddr4 { get; set; }

    public string? ShipToCompanyName { get; set; }

    public string? ShipToCustNum { get; set; }

    public string? StAddr1 { get; set; }

    public string? StAddr2 { get; set; }

    public string? StAddr3 { get; set; }

    public string? StAddr4 { get; set; }

    public DateTime? RequiredDate { get; set; }

    public int? AccountMgr { get; set; }

    public string? CustomerPo { get; set; }

    public string? Terms { get; set; }

    public string? ShipVia { get; set; }

    public string? Comments { get; set; }

    public decimal? ShippingHandling { get; set; }

    public decimal? SaleTotal { get; set; }

    public DateTime? SaleDate { get; set; }

    public string? CurType { get; set; }

    public double? CurRate { get; set; }

    public DateTime? CurDate { get; set; }

    public decimal? CurShipping { get; set; }

    public decimal? CurSalesTotal { get; set; }

    public string? RwsalesOrderNum { get; set; }

    /// <summary>
    /// warranty in days
    /// </summary>
    public int? Warranty { get; set; }

    public bool? Draft { get; set; }

    public DateTime? EditDate { get; set; }

    public bool? CompetitorFlag { get; set; }

    public bool? DropShipment { get; set; }

    public string? EnteredBy { get; set; }
}
