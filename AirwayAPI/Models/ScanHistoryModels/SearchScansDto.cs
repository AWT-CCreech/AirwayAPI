namespace AirwayAPI.Models.ScanHistoryModels;

public class SearchScansDto
{
    public DateTime ScanDateRangeStart { get; set; } = DateTime.Today.AddYears(-1);
    public DateTime ScanDateRangeEnd { get; set; } = DateTime.Today;
    public string OrderNum { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    public string PartNo { get; set; } = string.Empty;
    public string SerialNo { get; set; } = string.Empty;
    public string SNField { get; set; } = "SerialNo";
    public string MNSCo { get; set; } = string.Empty;
    public string ScanUser { get; set; } = string.Empty;
    public int Limit { get; set; } = 1000;
}
