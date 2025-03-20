using AirwayAPI.Data;
using AirwayAPI.Models;
using AirwayAPI.Models.PortalModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PortalController(eHelpDeskContext context) : ControllerBase
    {
        private readonly eHelpDeskContext _context = context;

        // Get available workspaces
        [HttpGet("workspaces")]
        public async Task<ActionResult<List<WorkspaceDto>>> GetWorkspaces()
        {
            var workspaces = await _context.PortalWorkspaces
                .OrderBy(w => w.Name)
                .Select(w => new WorkspaceDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    Description = w.Description
                })
                .ToListAsync();

            return Ok(workspaces);
        }

        // Get menu for workspace including user favorites
        [HttpGet("{workspaceId}/menu/{userId}")]
        public async Task<ActionResult<List<PortalMenuItemDto>>> GetMenu(int workspaceId, int userId)
        {
            var items = await _context.PortalItems
                .Where(i => i.WorkspaceId == workspaceId)
                .Include(i => i.InverseParent)
                .OrderBy(i => i.Ordering)
                .ToListAsync();

            var favorites = await _context.PortalUserFavorites
                .Where(f => f.UserId == userId && f.Item.WorkspaceId == workspaceId)
                .Select(f => f.ItemId)
                .ToListAsync();

            var menuTree = BuildMenuTree(items, null, favorites);
            return Ok(menuTree);
        }

        // Add item to favorites
        [HttpPost("favorites/{userId}/{itemId}")]
        public async Task<IActionResult> AddFavorite(int userId, int itemId)
        {
            if (!await _context.PortalUserFavorites.AnyAsync(f => f.UserId == userId && f.ItemId == itemId))
            {
                var maxOrdering = await _context.PortalUserFavorites
                    .Where(f => f.UserId == userId)
                    .MaxAsync(f => (int?)f.Ordering) ?? 0;

                var favorite = new PortalUserFavorite
                {
                    UserId = userId,
                    ItemId = itemId,
                    Ordering = maxOrdering + 1
                };

                _context.PortalUserFavorites.Add(favorite);
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        // Remove item from favorites
        [HttpDelete("favorites/{userId}/{itemId}")]
        public async Task<IActionResult> RemoveFavorite(int userId, int itemId)
        {
            var favorite = await _context.PortalUserFavorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ItemId == itemId);

            if (favorite == null)
                return NotFound();

            _context.PortalUserFavorites.Remove(favorite);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // Get routes for a workspace
        [HttpGet("{workspaceId}/routes")]
        public async Task<ActionResult<List<PortalRouteDto>>> GetRoutes(int workspaceId)
        {
            var routes = await _context.PortalRoutes
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

            return Ok(routes);
        }

        private List<PortalMenuItemDto> BuildMenuTree(List<PortalItem> items, int? parentId, List<int> favoriteIds)
        {
            return [.. items
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
    }
}
