using System;
using System.Collections.Generic;

namespace AirwayAPI.Models;

public partial class CompetitorCall
{
    public int CallId { get; set; }

    public int? RequestId { get; set; }

    public string? PartNum { get; set; }

    public string? MfgPartNum { get; set; }

    public string? ContactName { get; set; }

    public bool? EquipmentFound { get; set; }

    public bool? PartialFound { get; set; }

    public string? CompanyName { get; set; }

    public double? HowMany { get; set; }

    public double? OurCost { get; set; }

    public string? LeadTime { get; set; }

    public string? UrgencyRange { get; set; }

    public string? OfferPrice { get; set; }

    public string? FieldOrStock { get; set; }

    public bool? LeftAmessage { get; set; }

    public int? QuoteValidFor { get; set; }

    public string? Comments { get; set; }

    public double? ListPrice { get; set; }

    public string? ProductCode { get; set; }

    public string? Category { get; set; }

    public double? MfgDiscount { get; set; }

    public DateTime? EntryDate { get; set; }

    public int? EnteredBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public int? ModifiedBy { get; set; }

    public string? CallType { get; set; }

    public bool? QtyNotAvailable { get; set; }

    public int? ContactId { get; set; }

    public string? CurType { get; set; }

    public string? CurType2 { get; set; }

    public double? CurRate { get; set; }

    public DateTime? CurDate { get; set; }

    public decimal? CurOurCost { get; set; }

    public int? Warranty { get; set; }

    public byte? AvgCostFlag { get; set; }

    public string? Country { get; set; }

    public string? NewOrUsed { get; set; }

    public bool? MassMailing { get; set; }
}
