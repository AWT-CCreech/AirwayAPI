using System;
using System.Collections.Generic;

namespace AirwayAPI.Models;

public partial class TcPto
{
    public int RowId { get; set; }

    public string? Username { get; set; }

    public int? StartBalance { get; set; }

    public int? DeptId { get; set; }
}
