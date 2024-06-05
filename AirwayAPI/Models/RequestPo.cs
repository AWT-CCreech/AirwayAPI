namespace AirwayAPI.Models;

public partial class RequestPo
{
    public int Id { get; set; }

    public int? RequestId { get; set; }

    public string Ponum { get; set; }

    public DateTime? DeliveryDate { get; set; }

    public DateTime? PurchaseDate { get; set; }

    public int? PurchasedBy { get; set; }

    public int? QtyBought { get; set; }

    public bool? Poalarm { get; set; }

    public string? Location { get; set; }

    public DateTime? EntryDate { get; set; }

    public DateTime? EditDate { get; set; }

    public int? EditedBy { get; set; }

    public int? ContactId { get; set; }
}
