namespace AirwayAPI.Models;

public partial class PortalMenu
{
    public int Id { get; set; }

    public int? RootId { get; set; }

    public byte? Show { get; set; }

    public int? Al { get; set; }

    public string? ZOrder { get; set; }

    public string? MenuName { get; set; }

    public string? Caption { get; set; }

    public string? Link { get; set; }

    public string? Target { get; set; }

    public bool? ReportOption { get; set; }

    public bool? MostUsed { get; set; }

    public bool? MgrsRpt { get; set; }

    public string? AccessIds { get; set; }
}
