namespace AirwayAPI.Models;

public partial class TrkUnshippedBySo
{
    public string SoNo { get; set; } = null!;

    public DateTime DateRecorded { get; set; }

    public decimal? UnshippedValue { get; set; }
}
