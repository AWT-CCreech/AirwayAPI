using System;
using System.Collections.Generic;

namespace AirwayAPI.Models;

public partial class FreightQuote
{
    public int Id { get; set; }

    public int? EventId { get; set; }

    public int? QuoteId { get; set; }

    public string? ShipFrom { get; set; }

    public string? ShipFromNum { get; set; }

    public string? ShipFromAddress1 { get; set; }

    public string? ShipFromAddress2 { get; set; }

    public string? ShipFromAddress3 { get; set; }

    public string? ShipFromAddress4 { get; set; }

    public string? ShipFromContact { get; set; }

    public string? ShipFromPhone { get; set; }

    public string? ShipTo { get; set; }

    public string? ShipToNum { get; set; }

    public string? ShipToAddress1 { get; set; }

    public string? ShipToAddress2 { get; set; }

    public string? ShipToAddress3 { get; set; }

    public string? ShipToAddress4 { get; set; }

    public string? ShipToContact { get; set; }

    public string? ShipToPhone { get; set; }

    public string? VendorContact { get; set; }

    public string? VendorPhone { get; set; }

    public bool? HelpRequested { get; set; }

    public decimal? ShipmentValue { get; set; }

    public string? TypeOfService { get; set; }

    public string? TypeOfService2 { get; set; }

    public string? TypeOfService3 { get; set; }

    public bool? LoadingDock { get; set; }

    public string? LiftGate { get; set; }

    public string? Straps { get; set; }

    public string? Crating { get; set; }

    public string? AirwayPo { get; set; }

    public string? BillOfLading { get; set; }

    public string? TrackNum { get; set; }

    public int? TotalPieces { get; set; }

    public int? BoxTotal { get; set; }

    public DateTime? ShipDate { get; set; }

    public int? ActualWeight { get; set; }

    public bool? InOutBound { get; set; }

    public string? ShipRep { get; set; }

    public string? ServiceUsed { get; set; }

    public string? CarrierUsed { get; set; }

    public DateTime? Fqdeadline { get; set; }

    public DateTime? EntryDate { get; set; }

    public string? EnteredBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ShipInfoModDate { get; set; }

    public string? ShipInfoModBy { get; set; }

    public string? ShipmentNotes { get; set; }

    public bool? FreightChangeNpu { get; set; }

    public bool? Priced { get; set; }

    public string? CommercialTerms { get; set; }

    public bool? Cancelled { get; set; }

    public string? ReasonCancelled { get; set; }

    public bool? FreightSheet { get; set; }

    public string? Notes { get; set; }
}
