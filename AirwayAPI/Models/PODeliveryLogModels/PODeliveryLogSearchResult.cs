namespace AirwayAPI.Models.PODeliveryLogModels;

public class PODeliveryLogSearchResult
{
    public int Id { get; set; }
    public string Ponum { get; set; }
    public DateTime? IssueDate { get; set; }
    public string ItemNum { get; set; }
    public int? QtyOrdered { get; set; }
    public int? QtyReceived { get; set; }
    public int? ReceiverNum { get; set; }
    public bool NotesExist { get; set; }
    public string? NoteEditDate { get; set; }
    public DateTime? PORequiredDate { get; set; }
    public DateTime? DateDelivered { get; set; }
    public DateTime? EditDate { get; set; }
    public DateTime? ExpectedDelivery { get; set; }
    public string Sonum { get; set; }
    public string IssuedBy { get; set; }
    public string VendorName { get; set; }
    public int? ItemClassId { get; set; }
    public string AltPartNum { get; set; }
    public int? Postatus { get; set; }
    public string CompanyId { get; set; }
    public int? ContactId { get; set; }

    // Sales Order details
    public string CustomerName { get; set; }
    public DateTime? SORequiredDate { get; set; }
    public string SalesRep { get; set; }
    public bool IsDropShipment { get; set; }
}