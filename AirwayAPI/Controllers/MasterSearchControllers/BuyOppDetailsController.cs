using AirwayAPI.Data;
using AirwayAPI.Data.MasterSearch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.MasterSearch
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuyOppDetailsController : ControllerBase
    {
        private readonly eHelpDeskContext _context;

        public BuyOppDetailsController(eHelpDeskContext context)
        {
            this._context = context;
        }

        // GET: api/BuyOppDetails
        [HttpGet]
        public async Task<ActionResult<BuyOppDetail[]>> GetBuyOppDetails([FromQuery] SearchInput input)
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
                MS_Utils.InsertSearchQuery(_context, input, "Buy Opp", "Detail");
                var search = input.Search.ToLower();
                var buyOppDetails = await (from be in _context.BuyingOppEvents
                                           join bd in _context.BuyingOppDetails on be.EventId equals bd.EventId
                                           join cc in _context.CamContacts on be.ContactId equals cc.Id
                                           join us in _context.Users on be.EventOwner equals us.Id
                                           join es in _context.EquipmentSnapshots on be.EventId equals es.EventId into leftOuter
                                           from lo in leftOuter.DefaultIfEmpty()
                                           where (input.PartNo == true ? (bd.PartNum ?? string.Empty).ToLower().Contains(search) : false)
                                               || (input.PartDesc == true ? (bd.PartDesc ?? string.Empty).ToLower().Contains(search) : false)
                                               || (input.Company == true ? (cc.Company ?? string.Empty).ToLower().Contains(search) : false)
                                               || (input.ID == true && search.All(char.IsNumber) ? bd.DetailId.ToString() == search : false)
                                           select new
                                           {
                                                be.EventId,
                                                bd.DetailId,
                                                bd.PartNum,
                                                bd.PartDesc,
                                                bd.Quantity,
                                                be.StatusCash,
                                                be.StatusConsignment,
                                                be.EntryDate,
                                                cc.Company,
                                                us.Uname,
                                                bd.AskingPrice
                                            }).OrderByDescending(x => x.DetailId).ToListAsync();
                return Ok(buyOppDetails);
            }
            else
            {
                return Ok(Array.Empty<BuyOppDetail>());
            }
        }
    }
}