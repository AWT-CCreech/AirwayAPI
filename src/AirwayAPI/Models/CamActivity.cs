namespace AirwayAPI.Models;

public partial class CamActivity
{
    public int Id { get; set; }

    public int? ContactId { get; set; }

    public string? ActivityOwner { get; set; }

    public DateTime? ActivityDate { get; set; }

    public DateTime? ActivityTime { get; set; }

    public int? DurationHours { get; set; }

    public int? DurationMins { get; set; }

    public string? ActivityType { get; set; }

    public string? Notes { get; set; }

    public string? ProjectCode { get; set; }

    public DateTime? EntryDate { get; set; }

    public string? EnteredBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public string? CompletedBy { get; set; }

    public DateTime? CompleteDate { get; set; }

    public byte? Ogm { get; set; }

    public byte? Reminder { get; set; }

    public DateTime? RemindBefore { get; set; }

    public int? RemindBeforeInMins { get; set; }

    public byte? ContactOverride { get; set; }

    public byte? IsPrivate { get; set; }

    public byte? IsFullDay { get; set; }

    public int? RepeatId { get; set; }

    public int? RepeatOrgId { get; set; }

    public string? Members { get; set; }

    public string? MembersHist { get; set; }

    public string? Attachments { get; set; }

    public string? LinkRecType { get; set; }

    public int? LinkRecId { get; set; }

    public byte? LeftMsg { get; set; }
}
