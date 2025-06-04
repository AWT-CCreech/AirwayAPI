namespace AirwayAPI.Models;

public partial class BuyingOppEvent
{
    public int EventId { get; set; }

    public DateTime? BidDueDate { get; set; }

    public string? BidProposed { get; set; }

    public int? ContactId { get; set; }

    public int? RetailEstimateValue { get; set; }

    public string? Manufacturer { get; set; }

    public string? Platform { get; set; }

    public string? Frequency { get; set; }

    public string? CompletedSites { get; set; }

    public string? InitialCommentary { get; set; }

    public int? EventOwner { get; set; }

    public int? ResearchOwner { get; set; }

    public string? PartialBuy { get; set; }

    public string? BuyingOpp { get; set; }

    public string? StatusCash { get; set; }

    public string? StatusConsignment { get; set; }

    public DateTime? DateAvailable { get; set; }

    public bool? RipNeeded { get; set; }

    public string? EquipmentType { get; set; }

    public string? EquipmentCondition { get; set; }

    public bool? Consignment { get; set; }

    public string? Technology { get; set; }

    public string? CashCompanyLostTo { get; set; }

    public string? ConsignCompanyLostTo { get; set; }

    public string? CashPriceLostTo { get; set; }

    public string? ConsignPriceLostTo { get; set; }

    public string? Rating { get; set; }

    public DateTime? EntryDate { get; set; }

    public int? EnteredBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public int? ModifiedBy { get; set; }
}
