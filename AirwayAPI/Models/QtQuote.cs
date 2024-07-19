using System;
using System.Collections.Generic;

namespace AirwayAPI.Data;

public partial class QtQuote
{
    public int QuoteId { get; set; }

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

    public string? SalesTeam { get; set; }

    public string? CustomerPo { get; set; }

    public string? Terms { get; set; }

    public string? ShipVia { get; set; }

    public string? Comments { get; set; }

    public decimal? ShippingHandling { get; set; }

    public decimal? QuoteTotal { get; set; }

    public decimal? TotalCost { get; set; }

    public DateTime? SaleDate { get; set; }

    public DateTime? EntryDate { get; set; }

    public string? RwsalesOrderNum { get; set; }

    public string? ProjectCode { get; set; }

    public bool? Approved { get; set; }

    public bool? ApprovedFirst { get; set; }

    public int? ApprovedBy { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public bool? CompetitorFlag { get; set; }

    /// <summary>
    /// warranty in days
    /// </summary>
    public int? Warranty { get; set; }

    public string? CurType { get; set; }

    public double? CurRate { get; set; }

    public DateTime? CurDate { get; set; }

    public decimal? CurShipping { get; set; }

    public decimal? CurTotalCost { get; set; }

    public decimal? CurQuoteTotal { get; set; }

    public string? MgrNotes { get; set; }
}
