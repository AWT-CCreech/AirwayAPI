namespace AirwayAPI.Models;

public partial class PortalItem
{
    public int Id { get; set; }

    public int WorkspaceId { get; set; }

    public int? ParentId { get; set; }

    public string Label { get; set; } = null!;

    public string? IconName { get; set; }

    public string? Path { get; set; }

    public string ItemType { get; set; } = null!;

    public int Ordering { get; set; }

    public int ColumnGroup { get; set; }

    public virtual ICollection<PortalItem> InverseParent { get; set; } = new List<PortalItem>();

    public virtual PortalItem? Parent { get; set; }

    public virtual ICollection<PortalUserFavorite> PortalUserFavorites { get; set; } = new List<PortalUserFavorite>();

    public virtual PortalWorkspace Workspace { get; set; } = null!;
}
