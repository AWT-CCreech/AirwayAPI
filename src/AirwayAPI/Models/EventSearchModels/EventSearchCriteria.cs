namespace AirwayAPI.Models.EventSearchModels;

public class EventSearchCriteria
{
    public string? Company { get; set; }
    public string? Contact { get; set; }
    public string? ProjectName { get; set; }
    public DateTime? FromDate { get; set; } = DateTime.Today.AddDays(-30);
    public DateTime? ToDate { get; set; } = DateTime.Today;
    public string? SalesRep { get; set; }
    public string? Status { get; set; } = "Pending";
}
