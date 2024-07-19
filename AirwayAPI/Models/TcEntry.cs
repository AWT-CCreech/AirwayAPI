using System;
using System.Collections.Generic;

namespace AirwayAPI.Data;

public partial class TcEntry
{
    public int RowId { get; set; }

    public string? Username { get; set; }

    public DateTime? TimeIn { get; set; }

    public DateTime? TimeOut { get; set; }

    public int? Pto { get; set; }

    public int? HolidayId { get; set; }

    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedDate { get; set; }
}
