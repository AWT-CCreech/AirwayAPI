using System;
using System.Collections.Generic;

namespace AirwayAPI.Models;

public partial class PortalRoute
{
    public int Id { get; set; }

    public int? ParentId { get; set; }

    public string Path { get; set; } = null!;

    public string ComponentName { get; set; } = null!;

    public bool IsPrivate { get; set; }

    public int Ordering { get; set; }

    public virtual ICollection<PortalRoute> InverseParent { get; set; } = [];

    public virtual PortalRoute? Parent { get; set; }
}
