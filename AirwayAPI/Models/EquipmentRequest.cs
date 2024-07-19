using System;
using System.Collections.Generic;

namespace AirwayAPI.Data;

public partial class EquipmentRequest
{
    public int RequestId { get; set; }

    public int? EventId { get; set; }

    public string? PartDesc { get; set; }

    public string? PartNum { get; set; }

    public string? AltPartNum { get; set; }

    public double? Quantity { get; set; }

    public byte? RevSpecific { get; set; }

    public string? RevDetails { get; set; }

    public string? UnitMeasure { get; set; }

    public string? CustomerPricing { get; set; }

    public DateTime? DateNeeded { get; set; }

    public string? Manufacturer { get; set; }

    public string? Platform { get; set; }

    public string? Technology { get; set; }

    public string? Frequency { get; set; }

    public bool? RwpartNumFlag { get; set; }

    public bool? RwqtyFlag { get; set; }

    public string? QuoteNum { get; set; }

    public string? Comments { get; set; }

    public bool? EquipFound { get; set; }

    public bool? PartialFound { get; set; }

    public bool? AllPossibilities { get; set; }

    public DateTime? AllPossDate { get; set; }

    public int? AllPossBy { get; set; }

    public string? SalesOrderNum { get; set; }

    public double? SalePrice { get; set; }

    public DateTime? MarkedSoldDate { get; set; }

    public bool? Bought { get; set; }

    public bool? QuoteFullQty { get; set; }

    public bool? MassMailing { get; set; }

    public DateTime? MassMailDate { get; set; }

    public int? MassMailSentBy { get; set; }

    public string? Status { get; set; }

    public string? ReasonLost { get; set; }

    public DateTime? CancelDate { get; set; }

    public int? CanceledBy { get; set; }

    public string? EquipmentType { get; set; }

    public string? Category { get; set; }

    public int? QtySold { get; set; }

    public bool? LostButOngoing { get; set; }

    public DateTime? OnGoingDate { get; set; }

    public bool? InBuyingOpp { get; set; }

    public string? BuyingOppId { get; set; }

    public bool? OnHold { get; set; }

    public int? ProcureRep { get; set; }

    public DateTime? EntryDate { get; set; }

    public int? EnteredBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? QuoteDeadLine { get; set; }

    public byte? Urgent { get; set; }

    public DateTime? WorkbenchDate { get; set; }

    public DateTime? SoldWorkbenchDate { get; set; }

    public bool? WbnotifyFlag { get; set; }

    public byte? UsedPart { get; set; }

    public bool? InvalidPartNum { get; set; }

    public DateTime? InvalidPartDate { get; set; }

    public int? Amsnoozed { get; set; }

    public DateTime? AmsnoozeDate { get; set; }

    public bool? FoundEmailSent { get; set; }

    public int? DexterId { get; set; }

    public bool? Pwbflag { get; set; }

    public byte? Rmaflag { get; set; }

    public bool? TechWbreqForSo { get; set; }

    /// <summary>
    /// added this b/c of the new report showing calls made on requests but still having to find qty; once a req hits the WB needing qty found it still stays on there so they can make more calls but for reporting purposes we don&apos;t need to include these so this flag will help leave them off the report
    /// </summary>
    public bool? ZeroLeftToFind { get; set; }

    public bool? DropShipment { get; set; }

    public byte? Porequired { get; set; }

    public int? NeedToBuy { get; set; }

    public DateTime? NeedToBuyTs { get; set; }

    public bool? SoldWborder { get; set; }
}
