using AirwayAPI.Data;
using AirwayAPI.Models.MasterSearch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace AirwayAPI.Controllers.MasterSearch
{
    [Route("api/[controller]")]
    [ApiController]
    public class SellOppDetailsController : ControllerBase
    {
        private readonly eHelpDeskContext _context;

        public SellOppDetailsController(eHelpDeskContext context)
        {
            this._context = context;
        }

        //GET: api/SellOppDetails
        [HttpGet]
        public async Task<ActionResult<SellOppDetail[]>> GetSellOppDetails([FromQuery] SearchInput input)
        {
            try
            {
                // Log input
                Console.WriteLine($"SearchInput: {JsonConvert.SerializeObject(input)}");

                if ((input.ID == false && input.SONo == false && input.PartNo == false && input.PartDesc == false
                    && input.PONo == false && input.Mfg == false && input.Company == false && input.InvNo == false))
                {
                    input.ID = true;
                }

                if (!string.IsNullOrEmpty(input.Search))
                {
                    MS_Utils.InsertSearchQuery(_context, input, "Sell Opp", "Detail");
                    var search = input.Search;

                    var sellOppDetails = await (from re in _context.RequestEvents
                                                join er in _context.EquipmentRequests on re.EventId equals er.EventId
                                                join cc in _context.CamContacts on re.ContactId equals cc.Id
                                                join us in _context.Users on er.EnteredBy equals us.Id
                                                join rp in _context.RequestPos on er.RequestId equals rp.RequestId into leftOuter1
                                                from lo1 in leftOuter1.DefaultIfEmpty()
                                                join cs in _context.CsQtSoToInvNos on re.EventId equals cs.OrgEventId into leftOuter2
                                                from lo2 in leftOuter2.DefaultIfEmpty()
                                                join qt in _context.QtQuotes on re.EventId equals qt.EventId into leftOuter3
                                                from lo3 in leftOuter3.DefaultIfEmpty()
                                                where (input.PartNo && ((er.PartNum ?? string.Empty).Contains(search) || (er.AltPartNum ?? string.Empty).Contains(search)))
                                                   || (input.PartDesc && (er.PartDesc ?? string.Empty).ToLower().Contains(search))
                                                   || (input.Company && (cc.Company ?? string.Empty).ToLower().Contains(search))
                                                   || (input.ID && search.All(char.IsNumber) && er.RequestId.ToString() == search)
                                                   || (input.SONo && (er.SalesOrderNum ?? string.Empty).Contains(search))
                                                   || (input.PONo && (lo1.Ponum ?? string.Empty).Contains(search))
                                                   || (input.InvNo && lo2 != null && lo2.InvoiceNo.HasValue && lo2.InvoiceNo.Value.ToString().Contains(search)) // Ensure lo2 and lo2.InvoiceNo are not null
                                                   || (input.Mfg && (re.EntryDate > DateTime.Now.AddDays(-730) && (re.Manufacturer ?? string.Empty).ToLower().Contains(search)))
                                                select new
                                                {
                                                    re.EventId,
                                                    re.ContactId,
                                                    er.EnteredBy,
                                                    er.RequestId,
                                                    er.Quantity,
                                                    Manufacturer = er.Manufacturer ?? string.Empty,
                                                    PartNum = er.PartNum ?? string.Empty,
                                                    AltPartNum = er.AltPartNum ?? string.Empty,
                                                    PartDesc = er.PartDesc ?? string.Empty,
                                                    er.EquipFound,
                                                    Contact = cc.Contact ?? string.Empty,
                                                    Company = cc.Company ?? string.Empty,
                                                    Uname = us.Uname ?? string.Empty,
                                                    re.EntryDate,
                                                    QuoteId = (int?)lo3.QuoteId,
                                                    Version = (int?)lo3.Version
                                                }).ToListAsync();

                    var results = sellOppDetails
                        .GroupBy(a => a.RequestId)
                        .Select(g => g.OrderByDescending(x => x.Version ?? 0).FirstOrDefault())
                        .Where(x => x != null)  // Filter out any potential null results
                        .Select(x => new SellOppDetail
                        {
                            EventId = x!.EventId, // Use null-forgiving operator
                            ContactId = x.ContactId,
                            EnteredBy = x.EnteredBy,
                            RequestId = x.RequestId,
                            Quantity = x.Quantity,
                            Manufacturer = x.Manufacturer,
                            PartNum = x.PartNum,
                            AltPartNum = x.AltPartNum,
                            PartDesc = x.PartDesc,
                            EquipFound = x.EquipFound,
                            QtFound = 0,
                            Contact = x.Contact,
                            Company = x.Company,
                            Uname = x.Uname,
                            EntryDate = x.EntryDate,
                            QuoteId = x.QuoteId ?? 0,
                            Version = x.Version ?? 0
                        })
                        .OrderByDescending(x => x.RequestId)
                        .ToArray();

                    // Log results before processing
                    Console.WriteLine($"SellOppDetails: {JsonConvert.SerializeObject(results)}");

                    var today = DateTime.Now.AddDays(-31);
                    for (int i = 0; i < results.Length; ++i)
                    {
                        var result = results[i];
                        if (result != null)
                        {
                            var partNum = result.PartNum?.Trim();
                            var altPartNum = result.AltPartNum?.Trim();

                            var equipFound = await _context.CompetitorCalls
                                    .Where(cc => cc.QtyNotAvailable == false && cc.HowMany > 0 && cc.EntryDate > today
                                        && ((partNum != null && partNum.Length > 0 && cc.PartNum == partNum)
                                        || (altPartNum != null && altPartNum.Length > 0 && cc.MfgPartNum == altPartNum)))
                                    .SumAsync(cc => cc.HowMany);

                            if (result != null)
                            {
                                result.QtFound = (int)equipFound;
                            }
                        }
                    }

                    return Ok(results);
                }
                else
                {
                    return Ok(Array.Empty<SellOppDetail>());
                }
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
    }
}
