using AirwayAPI.Models.PortalModels;

namespace AirwayAPI.Services.Interfaces
{
    public interface IPortalService
    {
        Task<List<WorkspaceDto>> GetWorkspacesAsync();
        Task<List<PortalMenuItemDto>> GetMenuAsync(int workspaceId, int userId);
        Task AddFavoriteAsync(int userId, int itemId);
        Task RemoveFavoriteAsync(int userId, int itemId);
        Task<List<PortalRouteDto>> GetRoutesAsync(int workspaceId);
    }
}