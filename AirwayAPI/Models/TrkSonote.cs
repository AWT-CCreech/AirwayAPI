using System;
using System.Collections.Generic;

namespace AirwayAPI.Data;

public partial class TrkSonote
{
    public int Id { get; set; }

    public string? OrderNo { get; set; }

    public string? PartNo { get; set; }

    public string? EnteredBy { get; set; }

    public DateTime? EntryDate { get; set; }

    public string? ModBy { get; set; }

    public DateTime? ModDate { get; set; }

    public string? NoteType { get; set; }

    public string? Notes { get; set; }

    public int? ContactId { get; set; }
}
