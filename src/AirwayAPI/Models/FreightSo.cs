using System;
using System.Collections.Generic;

namespace AirwayAPI.Models;

public partial class FreightSo
{
    public int Id { get; set; }

    public int? FreightQuoteId { get; set; }

    public int? Sonum { get; set; }

    public decimal? FreightCharge { get; set; }

    public decimal? Markup { get; set; }

    public decimal? PackageHandling { get; set; }

    public decimal? TotalFreight { get; set; }

    public DateTime? DeliveredDate { get; set; }

    public DateTime? Eta { get; set; }

    public string? DeliveryNote { get; set; }

    public DateTime? EntryDate { get; set; }

    public string? EnteredBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? ModifiedBy { get; set; }
}
