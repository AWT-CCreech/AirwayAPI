using AirwayAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.ReportControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SalesOrderWorkbenchController : ControllerBase
    {
        private readonly eHelpDeskContext _context;

        public SalesOrderWorkbenchController(eHelpDeskContext context)
        {
            _context = context;
        }

    }
}
