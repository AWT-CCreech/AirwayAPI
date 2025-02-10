namespace AirwayAPI.Models;

public partial class BuyingOppDetail
{
    public int DetailId { get; set; }

    public int? EventId { get; set; }

    public int? Quantity { get; set; }

    public string? PartNum { get; set; }

    public string? AltPartNum { get; set; }

    public string? PartDesc { get; set; }

    public string? Manufacturer { get; set; }

    public string? StatusCash { get; set; }

    public string? StatusConsignment { get; set; }

    public string? AskingPrice { get; set; }

    public string? Notes { get; set; }

    public string? EquipmentType { get; set; }

    public double? BidPrice { get; set; }

    public string? CompanyLostTo { get; set; }

    public string? PriceLostTo { get; set; }

    public int? EnteredBy { get; set; }

    public DateTime? EntryDate { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public byte? ForecastRequired { get; set; }

    public int? Amsnoozed { get; set; }

    public DateTime? AmsnoozeDate { get; set; }
}
