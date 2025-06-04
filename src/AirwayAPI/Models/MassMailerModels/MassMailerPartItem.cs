namespace AirwayAPI.Models.MassMailerModels;

public class MassMailerPartItem
{
    public int RequestId { get; set; }
    public string PartNum { get; set; }
    public string AltPartNum { get; set; }
    public string PartDesc { get; set; }
    public double? Qty { get; set; }
    public string Company { get; set; }
    public string Manufacturer { get; set; }
    public string Revision { get; set; }
}
