using AirwayAPI.Data;
using AirwayAPI.Data.MasterSearch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.MasterSearch
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuyOppEventsController : ControllerBase
    {
        private readonly eHelpDeskContext _context;

        public BuyOppEventsController(eHelpDeskContext context)
        {
            this._context = context;
        }

        // GET: api/BuyOppEvents
        [HttpGet]
        public async Task<ActionResult<BuyOppEvent[]>> GetBuyOppEvents([FromQuery] SearchInput input)
        {
            // if no searching option is selected
            if (input.ID == false && input.PartNo == false && input.PartDesc == false && input.Company == false)
            {
                input.ID = true;
                input.PartNo = true;
                input.PartDesc = true;
                input.Company = true;
            }

            if (input.Search != null && input.Search.Trim() != "")
            {
                MS_Utils.InsertSearchQuery(_context, input, "Buy Opp", "Event");
                var search = input.Search.ToLower();
                var buyOppEvents = await (from be in _context.BuyingOppEvents
                                          join cc in _context.CamContacts on be.ContactId equals cc.Id
                                          join us in _context.Users on be.EventOwner equals us.Id
                                          join bd in _context.BuyingOppDetails on be.EventId equals bd.EventId into leftOuter1
                                          from lo1 in leftOuter1.DefaultIfEmpty()
                                          join es in _context.EquipmentSnapshots on be.EventId equals es.EventId into leftOuter2
                                          from lo2 in leftOuter2.DefaultIfEmpty()
                                          where (input.PartNo == true ? (lo1.PartNum ?? string.Empty).ToLower().Contains(search) : false)
                                              || (input.PartDesc == true ? (lo1.PartDesc ?? string.Empty).ToLower().Contains(search) : false)
                                              || (input.Company == true ? (cc.Company ?? string.Empty).ToLower().Contains(search) : false)
                                              || (input.ID == true && search.All(char.IsNumber) ? be.EventId.ToString() == search : false)
                                          select new
                                          {
                                              be.EventId,
                                              be.Manufacturer,
                                              be.Platform,
                                              be.Frequency,
                                              be.BidDueDate,
                                              be.StatusCash,
                                              be.StatusConsignment,
                                              be.EntryDate,
                                              Company = cc.Company ?? string.Empty,
                                              Uname = us.Uname ?? string.Empty,
                                              be.Technology,
                                              Comments = lo2.Comments ?? string.Empty
                                          }).Distinct().OrderByDescending(x => x.EventId).ToListAsync();
                return Ok(buyOppEvents);
            }
            else
            {
                return Ok(Array.Empty<BuyOppEvent>());
            }
        }
    }
}