namespace AirwayAPI.Models;

public partial class ScanHistory
{
    public int RowId { get; set; }

    public DateTime? ScanDate { get; set; }

    public string? ScannerId { get; set; }

    public string? Direction { get; set; }

    public string? OrderType { get; set; }

    public string? UserName { get; set; }

    public int? PostId { get; set; }

    public string? SoNo { get; set; }

    public string? PoNo { get; set; }

    public string? Rmano { get; set; }

    public int? Rtvid { get; set; }

    public string? PartNo { get; set; }

    public string? PartNo2 { get; set; }

    public string? PartNoClean { get; set; }

    public string? SerialNo { get; set; }

    public string? SerialNoB { get; set; }

    public string? TrackNo { get; set; }

    public string? HeciCode { get; set; }

    public string? BinLocation { get; set; }

    public int? TrkEventId { get; set; }

    /// <summary>
    /// ReturnToVendor RMA No
    /// </summary>
    public string? RtvRmaNo { get; set; }

    public string? VendorName { get; set; }

    public string? MnsCompany { get; set; }

    public bool? MnsInventoried { get; set; }

    public string? Notes { get; set; }
}
