using System;
using System.Collections.Generic;

namespace AirwayAPI.Models;

public partial class ShipmentWorkbench
{
    public int RowId { get; set; }

    public string? PickupFrom { get; set; }

    public string? DeliverTo { get; set; }

    public string? Rep { get; set; }

    public string? Sonum { get; set; }

    public string Ponum { get; set; } = null!;

    public string? Carrier { get; set; }

    public string? ShipStatus { get; set; }

    public DateTime? DeliveryDate { get; set; }

    public string? TrackingNo { get; set; }

    public int? FqshipmentId { get; set; }

    public bool? FreightIn { get; set; }

    public decimal? FreightAmount { get; set; }

    public int? Urgent { get; set; }

    public DateTime? EditDate { get; set; }

    public string? EditBy { get; set; }

    public DateTime? EntryDate { get; set; }

    public string? EnteredBy { get; set; }
}
