using AirwayAPI.Data;
using AirwayAPI.Models;
using AirwayAPI.Models.PortalModels;
using AirwayAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Services;

public class PortalService(eHelpDeskContext context) : IPortalService
{
    private readonly eHelpDeskContext _context = context;

    public async Task<List<WorkspaceDto>> GetWorkspacesAsync()
    {
        return await _context.PortalWorkspaces
            .OrderBy(w => w.Name)
            .Select(w => new WorkspaceDto
            {
                Id = w.Id,
                Name = w.Name,
                Description = w.Description
            })
            .ToListAsync();
    }

    public async Task<List<PortalMenuItemDto>> GetMenuAsync(int workspaceId, int userId)
    {
        var items = await _context.PortalItems
            .Where(i => i.WorkspaceId == workspaceId)
            .Include(i => i.InverseParent)
            .OrderBy(i => i.Ordering)
            .ToListAsync();

        var favoriteIds = await _context.PortalUserFavorites
            .Where(f => f.UserId == userId && f.Item.WorkspaceId == workspaceId)
            .Select(f => f.ItemId)
            .ToListAsync();

        return BuildMenuTree(items, null, favoriteIds);
    }

    public async Task AddFavoriteAsync(int userId, int itemId)
    {
        var exists = await _context.PortalUserFavorites
            .AnyAsync(f => f.UserId == userId && f.ItemId == itemId);

        if (exists)
            return;

        var maxOrdering = await _context.PortalUserFavorites
            .Where(f => f.UserId == userId)
            .MaxAsync(f => (int?)f.Ordering) ?? 0;

        _context.PortalUserFavorites.Add(new PortalUserFavorite
        {
            UserId = userId,
            ItemId = itemId,
            Ordering = maxOrdering + 1
        });

        await _context.SaveChangesAsync();
    }

    public async Task RemoveFavoriteAsync(int userId, int itemId)
    {
        var fav = await _context.PortalUserFavorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.ItemId == itemId);

        if (fav == null)
            return;

        _context.PortalUserFavorites.Remove(fav);
        await _context.SaveChangesAsync();
    }

    public async Task<List<PortalRouteDto>> GetRoutesAsync(int workspaceId)
    {
        // Note: workspaceId is unused here to match original behavior
        return await _context.PortalRoutes
            .OrderBy(r => r.Ordering)
            .Select(r => new PortalRouteDto
            {
                Id = r.Id,
                Path = r.Path,
                ComponentName = r.ComponentName,
                IsPrivate = r.IsPrivate,
                Ordering = r.Ordering
            })
            .ToListAsync();
    }

    private static List<PortalMenuItemDto> BuildMenuTree(List<PortalItem> items, int? parentId, List<int> favoriteIds) => [.. items
            .Where(i => i.ParentId == parentId)
            .OrderBy(i => i.Ordering)
            .Select(i => new PortalMenuItemDto
            {
                Id = i.Id,
                Label = i.Label,
                IconName = i.IconName,
                Path = i.Path,
                ItemType = i.ItemType,
                Ordering = i.Ordering,
                ColumnGroup = i.ColumnGroup,
                IsFavorite = favoriteIds.Contains(i.Id),
                Children = BuildMenuTree(items, i.Id, favoriteIds)
            })];
}