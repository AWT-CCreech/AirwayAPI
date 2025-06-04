namespace AirwayAPI.Models;

public partial class EquipmentSnapshot
{
    public int SnapshotId { get; set; }

    public int? EventId { get; set; }

    public int? EstimatedCost { get; set; }

    public int? EstimatedSellPrice { get; set; }

    public string? CustomersCanUse { get; set; }

    public DateTime? ForecastDue { get; set; }

    public int? MfgListPrice { get; set; }

    public string? Comments { get; set; }

    public DateTime? EntryDate { get; set; }

    public int? EnteredBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public int? ModifiedBy { get; set; }

    public byte? ForecastRequired { get; set; }
}
