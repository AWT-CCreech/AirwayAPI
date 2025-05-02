namespace AirwayAPI.Models;

public partial class PortalRoute
{
    public int Id { get; set; }

    public int? ParentId { get; set; }

    public string Path { get; set; } = null!;

    public string ComponentName { get; set; } = null!;

    public bool IsPrivate { get; set; }

    public int Ordering { get; set; }

    public virtual ICollection<PortalRoute> InverseParent { get; set; } = new List<PortalRoute>();

    public virtual PortalRoute? Parent { get; set; }
}
