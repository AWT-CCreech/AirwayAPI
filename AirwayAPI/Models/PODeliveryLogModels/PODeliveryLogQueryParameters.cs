namespace AirwayAPI.Models.PODeliveryLogModels;

public class PODeliveryLogQueryParameters
{
    public string? PONum { get; set; }
    public string? Vendor { get; set; }
    public string? PartNum { get; set; }
    public string? IssuedBy { get; set; }
    public string? SONum { get; set; }
    public string? xSalesRep { get; set; }
    public string HasNotes { get; set; } = "All";
    public string POStatus { get; set; } = "Not Complete";
    public string EquipType { get; set; } = "All";
    public string CompanyID { get; set; } = "AIR";
    public int YearRange { get; set; } = 0;
}