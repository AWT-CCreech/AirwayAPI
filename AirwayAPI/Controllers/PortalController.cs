using AirwayAPI.Models.PortalModels;
using AirwayAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirwayAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PortalController(IPortalService service) : ControllerBase
    {
        private readonly IPortalService _service = service;

        [HttpGet("workspaces")]
        public async Task<ActionResult<List<WorkspaceDto>>> GetWorkspaces()
        {
            var workspaces = await _service.GetWorkspacesAsync();
            return Ok(workspaces);
        }

        [HttpGet("{workspaceId}/menu/{userId}")]
        public async Task<ActionResult<List<PortalMenuItemDto>>> GetMenu(
            int workspaceId,
            int userId)
        {
            var menu = await _service.GetMenuAsync(workspaceId, userId);
            return Ok(menu);
        }

        [HttpPost("favorites/{userId}/{itemId}")]
        public async Task<IActionResult> AddFavorite(
            int userId,
            int itemId)
        {
            await _service.AddFavoriteAsync(userId, itemId);
            return Ok();
        }

        [HttpDelete("favorites/{userId}/{itemId}")]
        public async Task<IActionResult> RemoveFavorite(
            int userId,
            int itemId)
        {
            await _service.RemoveFavoriteAsync(userId, itemId);
            return Ok();
        }

        [HttpGet("{workspaceId}/routes")]
        public async Task<ActionResult<List<PortalRouteDto>>> GetRoutes(
            int workspaceId)
        {
            var routes = await _service.GetRoutesAsync(workspaceId);
            return Ok(routes);
        }
    }
}