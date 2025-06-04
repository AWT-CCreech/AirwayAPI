namespace AirwayAPI.Models;

public partial class TrkPonote
{
    public int RowId { get; set; }

    public int? Ponum { get; set; }

    public string? EnteredBy { get; set; }

    public DateTime? EntryDate { get; set; }

    public string? Notes { get; set; }
}
