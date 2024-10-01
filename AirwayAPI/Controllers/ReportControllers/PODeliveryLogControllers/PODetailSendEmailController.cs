using AirwayAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirwayAPI.Controllers.ReportControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PODetailSendEmailController : ControllerBase
    {
        private readonly eHelpDeskContext _context;

        public PODetailSendEmailController(
            eHelpDeskContext context)
        {
            _context = context;
        }

    }
}
