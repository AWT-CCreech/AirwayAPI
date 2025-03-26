using System;
using System.Collections.Generic;

namespace AirwayAPI.Models;

public partial class PortalWorkspace
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<PortalItem> PortalItems { get; set; } = new List<PortalItem>();

    public virtual ICollection<PortalUserConfig> PortalUserConfigs { get; set; } = new List<PortalUserConfig>();
}
