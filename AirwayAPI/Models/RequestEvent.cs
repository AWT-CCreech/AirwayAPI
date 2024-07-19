using System;
using System.Collections.Generic;

namespace AirwayAPI.Data;

public partial class RequestEvent
{
    public int EventId { get; set; }

    public int? ContactId { get; set; }

    public string? CompanyId { get; set; }

    public string? ProjectName { get; set; }

    public string? SoldOrLost { get; set; }

    public DateTime? EventEndDate { get; set; }

    public string? ReasonCode { get; set; }

    public string? ResponseDate { get; set; }

    public bool? InOutBound { get; set; }

    public string? Manufacturer { get; set; }

    public string? Platform { get; set; }

    public string? Technology { get; set; }

    public string? Frequency { get; set; }

    public string? EquipmentType { get; set; }

    public int? EventOwner { get; set; }

    public bool? EventNotification { get; set; }

    public DateTime? EntryDate { get; set; }

    public int? EnteredBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public int? ModifiedBy { get; set; }

    public string? BillingOption { get; set; }

    public string? CommercialTerms { get; set; }

    public string? BillingAccountNum { get; set; }

    public string? HowToBill { get; set; }

    public string? CtNotes { get; set; }

    public DateTime? QuoteDeadline { get; set; }

    public bool? RipReplace { get; set; }

    public bool? CompetitorFlag { get; set; }

    public bool? FiveDayReminder { get; set; }

    public bool? TenDayReminder { get; set; }
}
