namespace AirwayAPI.Models.PODeliveryLogModels;

public class PODetailUpdateDto
{
    public int Id { get; set; }
    public string PONum { get; set; }
    public string SONum { get; set; }
    public string PartNum { get; set; }
    public int? QtyOrdered { get; set; }
    public int? QtyReceived { get; set; }
    public int? ReceiverNum { get; set; }
    public DateTime? ExpectedDelivery { get; set; }
    public int? ContactID { get; set; }
    public string IssuedBy { get; set; }
    public DateTime? DateDelivered { get; set; }
    public DateTime? EditDate { get; set; }
    public string? EditedBy { get; set; }
    public string ExpDelEditDate { get; set; }
    public List<string> NotesList { get; set; }
    public string NewNote { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public bool UpdateAllDates { get; set; }
    public bool UrgentEmail { get; set; }
    public string ContactName { get; set; }
    public string Company { get; set; }
    public string Title { get; set; }
    public string Phone { get; set; }
}
