using System;
using System.Collections.Generic;

namespace AirwayAPI.Models;

public partial class UserPortalMenu
{
    public int RowId { get; set; }

    public int? UserId { get; set; }

    public int? DeptId { get; set; }

    public int? AppId { get; set; }

    public string? MenuType { get; set; }

    public DateTime? EntryDate { get; set; }
}
