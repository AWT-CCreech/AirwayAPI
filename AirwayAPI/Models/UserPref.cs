using System;
using System.Collections.Generic;

namespace AirwayAPI.Models;

public partial class UserPref
{
    public int RowId { get; set; }

    public byte? DefaultPref { get; set; }

    public string? UserName { get; set; }

    public string? PrefType { get; set; }

    public string? PrefName { get; set; }

    public string? PrefData { get; set; }

    public int? PortalMenuLeft { get; set; }

    public int? PortalMenuTop { get; set; }
}
