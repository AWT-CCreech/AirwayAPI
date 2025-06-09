using System;
using System.Collections.Generic;

namespace AirwayAPI.Models;

public partial class TrkInventory
{
    public string? ItemNum { get; set; }

    public string? ItemNum2 { get; set; }

    public double? MfgDiscount { get; set; }

    public double? TrkListPrice { get; set; }

    public double? TrkUnitCost { get; set; }

    public string? ProductCode { get; set; }

    public string? Category { get; set; }

    public bool? MfgDiscontinued { get; set; }

    public string? ReplacePartNum { get; set; }

    public string? LeadTime { get; set; }

    public bool? ProductMgmt { get; set; }

    public DateTime? EditDate { get; set; }

    public int? EditedBy { get; set; }

    public int? PricingExpires { get; set; }

    public int? QtyThreshold { get; set; }

    public byte? CycleCountFlag { get; set; }

    public DateTime? CycleCountDate { get; set; }

    public int? CycleActualCount { get; set; }

    public string? CycleCountBy { get; set; }

    public DateTime? RetainConfirmedDate { get; set; }

    public string? RetainConfirmedBy { get; set; }

    public double? Weight { get; set; }

    public int? Height { get; set; }

    public int? HeightInches { get; set; }

    public int? Length { get; set; }

    public int? LengthInches { get; set; }

    public int? Width { get; set; }

    public int? WidthInches { get; set; }

    public double? SalvageValue { get; set; }

    public string? PartType { get; set; }

    public string? MetalPartType { get; set; }

    public string? UseValue { get; set; }

    /// <summary>
    /// Is the dimension Actual or for Packaging; default to actual = 1 and they&apos;ll update if it&apos;s packaging
    /// </summary>
    public bool? ActualDimension { get; set; }

    public DateTime? DimensionsEditDate { get; set; }

    public string? DimensionsEditBy { get; set; }

    public string? UnitMeasure { get; set; }

    public string? Eccn { get; set; }

    public string? Htscode { get; set; }

    public string? Comments { get; set; }
}
