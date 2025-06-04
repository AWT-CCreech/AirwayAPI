namespace AirwayAPI.Models;

public partial class CamCannedEmail
{
    public int Id { get; set; }

    public string? EmailType { get; set; }

    public string? EmailDesc { get; set; }

    public string? EmailSubject { get; set; }

    public string? EmailBody { get; set; }

    public bool? Active { get; set; }

    public bool? DefaultMsg { get; set; }

    public string? EnteredBy { get; set; }

    public DateTime? EntryDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }
}
