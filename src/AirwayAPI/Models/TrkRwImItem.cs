using System;
using System.Collections.Generic;

namespace AirwayAPI.Models;

public partial class TrkRwImItem
{
    public string? ItemNum { get; set; }

    public int? WarehouseId { get; set; }

    public string? ProductCode { get; set; }

    public string? ItemDesc { get; set; }

    public string? AltPartNum { get; set; }

    public int? ItemType { get; set; }

    public int? QtyOnHand { get; set; }

    public string? VendorNum { get; set; }

    public decimal? LastPurchAmt { get; set; }

    public decimal? LastSaleAmt { get; set; }

    public int? ItemClassId { get; set; }

    public int? ItemClassification { get; set; }

    public string? CompanyId { get; set; }

    public string? SubType { get; set; }

    public double? QtyCost { get; set; }

    public string? Mfg { get; set; }

    public string? Category { get; set; }

    public string? ListPrice { get; set; }

    public decimal? UnitCost { get; set; }

    public int? NoteId { get; set; }

    public int? QtyInPick { get; set; }
}
