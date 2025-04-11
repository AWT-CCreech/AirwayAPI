using AirwayAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.UtilityControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ScanHistoryController(eHelpDeskContext context, ILogger<ScanHistoryController> logger) : ControllerBase
    {
        private readonly eHelpDeskContext _context = context;
        private readonly ILogger<ScanHistoryController> _logger = logger;


    }
}
