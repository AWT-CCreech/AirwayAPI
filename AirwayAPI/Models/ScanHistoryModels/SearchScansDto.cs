namespace AirwayAPI.Models.ScanHistoryModels;
public class SearchScansDto
{
    // Default to one year ago
    public DateTime ScanDateRangeStart { get; set; } = DateTime.Today.AddYears(-1);
    // Default to today
    public DateTime ScanDateRangeEnd { get; set; } = DateTime.Today;

    public string SoNo { get; set; } = string.Empty;
    public string PoNo { get; set; } = string.Empty;
    public string Rmano { get; set; } = string.Empty;
    public int? RTVID { get; set; }
    public string PartNo { get; set; } = string.Empty;
    public string SerialNo { get; set; } = string.Empty;
    public string SNField { get; set; } = "SerialNo";
    public string MNSCo { get; set; } = string.Empty;
    public string ScanUser { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    public int Limit { get; set; } = 1000;
}