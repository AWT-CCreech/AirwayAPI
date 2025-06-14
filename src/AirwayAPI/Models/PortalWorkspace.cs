﻿namespace AirwayAPI.Models;

public partial class PortalWorkspace
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<PortalItem> PortalItems { get; set; } = [];

    public virtual ICollection<PortalUserConfig> PortalUserConfigs { get; set; } = [];
}
