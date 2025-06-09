using System;
using System.Collections.Generic;

namespace AirwayAPI.Models;

public partial class RequestPohistory
{
    public int Id { get; set; }

    public int? Poid { get; set; }

    public string? Ponum { get; set; }

    public DateTime? DeliveryDate { get; set; }

    public int? QtyBought { get; set; }

    public int? EnteredBy { get; set; }

    public DateTime? EditDate { get; set; }
}
