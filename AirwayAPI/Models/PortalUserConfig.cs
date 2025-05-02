namespace AirwayAPI.Models;

public partial class PortalUserConfig
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int WorkspaceId { get; set; }

    public string? Config { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual PortalWorkspace Workspace { get; set; } = null!;
}
