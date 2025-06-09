using System;
using System.Collections.Generic;

namespace AirwayAPI.Models;

public partial class TrkUnshippedValue
{
    public int Id { get; set; }

    public DateTime? ShipDate { get; set; }

    public double? UnshippedValue { get; set; }
}
