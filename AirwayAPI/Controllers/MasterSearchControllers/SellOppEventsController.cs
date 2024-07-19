using AirwayAPI.Data;
using AirwayAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.MasterSearch
{
    [Route("api/[controller]")]
    [ApiController]
    public class SellOppEventsController : ControllerBase
    {
        private readonly eHelpDeskContext _context;

        public SellOppEventsController(eHelpDeskContext context)
        {
            _context = context;
        }

        // GET: api/SellOppEvents
        [HttpGet]
        public async Task<ActionResult<SellOppEvent[]>> GetSellOppEvents([FromQuery] SearchInput input)
        {
            // if no searching option is selected, search by ID
            if ((input.ID == false && input.SONo == false && input.PartNo == false && input.PartDesc == false
                && input.PONo == false && input.Mfg == false && input.Company == false && input.InvNo == false))
            {
                input.ID = true;
            }

            if (input.Search != null && input.Search.Trim() != "")
            {
                MS_Utils.InsertSearchQuery(_context, input, "Sell Opp", "Event");
                var search = input.Search.ToLower();
                var sellOppEvents = await (from re in _context.RequestEvents
                                           join er in _context.EquipmentRequests on re.EventId equals er.EventId
                                           join cc in _context.CamContacts on re.ContactId equals cc.Id
                                           join us in _context.Users on er.EnteredBy equals us.Id
                                           join rp in _context.RequestPos on er.RequestId equals rp.RequestId into leftOuter1
                                           from lo1 in leftOuter1.DefaultIfEmpty()
                                           join cs in _context.CsQtSoToInvNos on re.EventId equals cs.OrgEventId into leftOuter2
                                           from lo2 in leftOuter2.DefaultIfEmpty()
                                           join qt in _context.QtQuotes on re.EventId equals qt.EventId into leftOuter3
                                           from lo3 in leftOuter3.DefaultIfEmpty()
                                           where (input.PartNo == true && (er.PartNum.ToLower().Contains(search) 
                                                || er.AltPartNum.ToLower().Contains(search)))
                                                || (input.PartDesc == true && er.PartDesc.ToLower().Contains(search))
                                                || (input.Company == true && cc.Company.ToLower().Contains(search))
                                                || (input.ID == true && search.All(char.IsNumber) && re.EventId.ToString() == search)
                                                || (input.ID == true && search.All(char.IsNumber) && input.PartNo == true && re.EventId.ToString().Contains(search))
                                                || (input.SONo == true && er.SalesOrderNum.ToLower().Contains(search))
                                                || (input.PONo == true && lo1.Ponum.ToLower().Contains(search))
                                                || (input.InvNo == true && lo2.InvoiceNo.ToString().Contains(search))
                                                || (input.Mfg == true && re.EntryDate > DateTime.Now.AddDays(-730) && re.Manufacturer.ToLower().Contains(search))
                                           select new
                                           {
                                               re.EventId,
                                               re.ContactId,
                                               re.SoldOrLost,
                                               re.EnteredBy,
                                               re.Manufacturer,
                                               re.Platform,
                                               re.EntryDate,
                                               cc.Contact,
                                               cc.Company,
                                               us.Uname,
                                               QuoteId = (int?)lo3.QuoteId,
                                               Version = (int?)lo3.Version
                                           }).Distinct().ToListAsync();

                var results = sellOppEvents
                              .GroupBy(a => a.EventId)
                              .Select(g => g.OrderByDescending(x => x?.Version ?? 0).FirstOrDefault())
                              .Where(x => x != null) // Ensure no null elements
                              .OrderByDescending(x => x!.EventId); // Sorting by EventId

                return Ok(results);
            }
            else
            {
                return Ok(Array.Empty<SellOppEvent>());
            }
        }
    }
}
