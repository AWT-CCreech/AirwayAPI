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

        [HttpGet("{workspaceId}/menu")]
        public async Task<ActionResult<List<PortalMenuItemDto>>> GetMenu(int workspaceId)
        {
            var items = await _context.PortalItems
                .Where(i => i.WorkspaceId == workspaceId)
                .Include(i => i.InverseParent)
                .OrderBy(i => i.Ordering)
                .ToListAsync();

            var menuTree = BuildMenuTree(items, null);
            return Ok(menuTree);
        }

        [HttpGet("{workspaceId}/routes")]
        public async Task<ActionResult<List<PortalRouteDto>>> GetRoutes(int workspaceId)
        {
            var routes = await _context.PortalRoutes
                .OrderBy(r => r.Ordering)
                .ToListAsync();

            var routeDtos = routes.Select(r => new PortalRouteDto
            {
                Id = r.Id,
                Path = r.Path,
                ComponentName = r.ComponentName,
                IsPrivate = r.IsPrivate,
                Ordering = r.Ordering
            });

            return Ok(routeDtos);
        }

        private List<PortalMenuItemDto> BuildMenuTree(List<PortalItem> items, int? parentId)
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
                    Children = BuildMenuTree(items, i.Id)
                })];
        }
    }
}