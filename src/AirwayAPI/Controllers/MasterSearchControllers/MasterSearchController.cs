﻿using AirwayAPI.Data;
using AirwayAPI.Models.MasterSearchModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.MasterSearchControllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MasterSearchController(eHelpDeskContext context) : ControllerBase
{
    private readonly eHelpDeskContext _context = context;

    #region BuyOppDetails

    // GET: api/MasterSearch/BuyOppDetails
    [HttpGet("BuyOppDetails")]
    public async Task<ActionResult<BuyOppDetail[]>> GetBuyOppDetails([FromQuery] SearchInput input)
    {
        // If no searching option is selected
        if (!input.ID && !input.PartNo && !input.PartDesc && !input.Company)
        {
            input.ID = input.PartNo = input.PartDesc = input.Company = true;
        }

        if (!string.IsNullOrWhiteSpace(input.Search))
        {
            MS_Utils.InsertSearchQuery(_context, input, "Buy Opp", "Detail");
            var search = input.Search.ToLower();

            var buyOppDetails = await (from be in _context.BuyingOppEvents
                                       join bd in _context.BuyingOppDetails on be.EventId equals bd.EventId
                                       join cc in _context.CamContacts on be.ContactId equals cc.Id
                                       join us in _context.Users on be.EventOwner equals us.Id
                                       join es in _context.EquipmentSnapshots on be.EventId equals es.EventId into leftOuter
                                       from lo in leftOuter.DefaultIfEmpty()
                                       where
                                           (input.PartNo && (EF.Functions.Like(bd.PartNum ?? "", $"%{search}%")
                                               || EF.Functions.Like(bd.AltPartNum ?? "", $"%{search}%")))
                                           || (input.PartDesc && EF.Functions.Like(bd.PartDesc ?? "", $"%{search}%"))
                                           || (input.Company && EF.Functions.Like(cc.Company ?? "", $"%{search}%"))
                                           || (input.ID && search.All(char.IsNumber) && bd.DetailId.ToString() == search)
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

    #endregion

    #region BuyOppEvents

    // GET: api/MasterSearch/BuyOppEvents
    [HttpGet("BuyOppEvents")]
    public async Task<ActionResult<BuyOppEvent[]>> GetBuyOppEvents([FromQuery] SearchInput input)
    {
        // If no searching option is selected
        if (!input.ID && !input.PartNo && !input.PartDesc && !input.Company)
        {
            input.ID = input.PartNo = input.PartDesc = input.Company = true;
        }

        if (!string.IsNullOrWhiteSpace(input.Search))
        {
            MS_Utils.InsertSearchQuery(_context, input, "Buy Opp", "Event");
            var search = input.Search.ToLower(); // Optional: Convert to lower if needed

            var buyOppEvents = await (from be in _context.BuyingOppEvents
                                      join cc in _context.CamContacts on be.ContactId equals cc.Id
                                      join us in _context.Users on be.EventOwner equals us.Id
                                      join bd in _context.BuyingOppDetails on be.EventId equals bd.EventId into leftOuter1
                                      from lo1 in leftOuter1.DefaultIfEmpty()
                                      join es in _context.EquipmentSnapshots on be.EventId equals es.EventId into leftOuter2
                                      from lo2 in leftOuter2.DefaultIfEmpty()
                                      where
                                          (input.PartNo && (EF.Functions.Like(lo1.PartNum ?? "", $"%{search}%")
                                              || EF.Functions.Like(lo1.PartDesc ?? "", $"%{search}%")))
                                          || (input.PartDesc && EF.Functions.Like(lo1.PartDesc ?? "", $"%{search}%"))
                                          || (input.Company && EF.Functions.Like(cc.Company ?? "", $"%{search}%"))
                                          || (input.ID && search.All(char.IsNumber) && be.EventId.ToString() == search)
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

    #endregion

    #region MasterSearchContacts

    // GET: api/MasterSearch/Contacts
    [HttpGet("Contacts")]
    public async Task<ActionResult<MasterSearchContact[]>> GetContacts([FromQuery] string searchValue, [FromQuery] bool active)
    {
        var query = _context.CamContacts.Where(cc =>
            cc.Contact != null && EF.Functions.Like(cc.Contact.Trim(), $"%{searchValue.Trim()}%")
        );

        if (active)
        {
            query = query.Where(cc => cc.ActiveStatus == 1);
        }

        var contacts = await query.Select(cc => new MasterSearchContact
        {
            Id = cc.Id,
            Contact = cc.Contact ?? "",
            Company = cc.Company ?? "",
            State = cc.State ?? "",
            PhoneMain = cc.PhoneMain ?? "",
            ActiveStatus = cc.ActiveStatus == 1
        }).ToListAsync();

        return Ok(contacts);
    }

    #endregion

    #region SellOppDetails

    // GET: api/MasterSearch/SellOppDetails
    [HttpGet("SellOppDetails")]
    public async Task<ActionResult<SellOppDetail[]>> GetSellOppDetails([FromQuery] SearchInput input)
    {
        try
        {
            if (!input.ID && !input.SONo && !input.PartNo && !input.PartDesc && !input.PONo && !input.Mfg && !input.Company && !input.InvNo)
            {
                input.ID = true;
            }

            if (!string.IsNullOrWhiteSpace(input.Search))
            {
                MS_Utils.InsertSearchQuery(_context, input, "Sell Opp", "Detail");
                var search = input.Search.ToLower();

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
                                            where
                                                (input.PartNo && (EF.Functions.Like(er.PartNum ?? "", $"%{search}%")
                                                    || EF.Functions.Like(er.AltPartNum ?? "", $"%{search}%")))
                                                || (input.PartDesc && EF.Functions.Like(er.PartDesc ?? "", $"%{search}%"))
                                                || (input.Company && EF.Functions.Like(cc.Company ?? "", $"%{search}%"))
                                                || (input.ID && search.All(char.IsNumber) && er.RequestId.ToString() == search)
                                                || (input.SONo && EF.Functions.Like(er.SalesOrderNum ?? "", $"%{search}%"))
                                                || (input.PONo && EF.Functions.Like(lo1.Ponum ?? "", $"%{search}%"))
                                                || (input.InvNo && lo2 != null && lo2.InvoiceNo.HasValue && lo2.InvoiceNo.Value.ToString().Contains(search))
                                                || (input.Mfg && re.EntryDate > DateTime.Now.AddDays(-730) && EF.Functions.Like(re.Manufacturer ?? "", $"%{search}%"))
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
                                                lo3.Version
                                            }).ToListAsync();

                var results = sellOppDetails
                    .GroupBy(a => a.RequestId)
                    .Select(g => g.OrderByDescending(x => x.Version ?? 0).FirstOrDefault())
                    .Where(x => x != null)  // Filter out any potential null results
                    .Select(x => new SellOppDetail
                    {
                        EventId = x!.EventId,
                        ContactId = x.ContactId,
                        EnteredBy = x.EnteredBy,
                        RequestId = x.RequestId,
                        Quantity = x.Quantity,
                        Manufacturer = x.Manufacturer,
                        PartNum = x.PartNum,
                        AltPartNum = x.AltPartNum,
                        PartDesc = x.PartDesc,
                        EquipFound = x.EquipFound,
                        QtFound = 0, // Assuming default value; adjust as needed
                        Contact = x.Contact,
                        Company = x.Company,
                        Uname = x.Uname,
                        EntryDate = x.EntryDate,
                        QuoteId = x.QuoteId ?? 0,
                        Version = x.Version ?? 0
                    })
                    .OrderByDescending(x => x.RequestId)
                    .ToArray();

                // Calculate QtFound
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
            // Log exception (consider using a logging framework)
            Console.WriteLine($"Exception: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    #endregion

    #region SellOppEvents

    // GET: api/MasterSearch/SellOppEvents
    [HttpGet("SellOppEvents")]
    public async Task<ActionResult<SellOppEvent[]>> GetSellOppEvents([FromQuery] SearchInput input)
    {
        if (!input.ID && !input.SONo && !input.PartNo && !input.PartDesc && !input.PONo && !input.Mfg && !input.Company && !input.InvNo)
        {
            input.ID = true;
        }

        if (!string.IsNullOrWhiteSpace(input.Search))
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
                                       where
                                           (input.PartNo && (EF.Functions.Like(er.PartNum ?? "", $"%{search}%")
                                               || EF.Functions.Like(er.AltPartNum ?? "", $"%{search}%")))
                                           || (input.PartDesc && EF.Functions.Like(er.PartDesc ?? "", $"%{search}%"))
                                           || (input.Company && EF.Functions.Like(cc.Company ?? "", $"%{search}%"))
                                           || (input.ID && search.All(char.IsNumber) && re.EventId.ToString() == search)
                                           || (input.ID && search.All(char.IsNumber) && input.PartNo && re.EventId.ToString().Contains(search))
                                           || (input.SONo && EF.Functions.Like(er.SalesOrderNum ?? "", $"%{search}%"))
                                           || (input.PONo && EF.Functions.Like(lo1.Ponum ?? "", $"%{search}%"))
                                           || (input.InvNo && lo2 != null && lo2.InvoiceNo.HasValue && lo2.InvoiceNo.Value.ToString().Contains(search))
                                           || (input.Mfg && re.EntryDate > DateTime.Now.AddDays(-730) && EF.Functions.Like(re.Manufacturer ?? "", $"%{search}%"))
                                       select new
                                       {
                                           re.EventId,
                                           re.ContactId,
                                           re.SoldOrLost,
                                           re.EnteredBy,
                                           Manufacturer = re.Manufacturer ?? string.Empty,
                                           re.Platform,
                                           re.EntryDate,
                                           Contact = cc.Contact ?? string.Empty,
                                           Company = cc.Company ?? string.Empty,
                                           Uname = us.Uname ?? string.Empty,
                                           QuoteId = (int?)lo3.QuoteId,
                                           lo3.Version
                                       }).Distinct().ToListAsync();

            var results = sellOppEvents
                          .GroupBy(a => a.EventId)
                          .Select(g => g.OrderByDescending(x => x?.Version ?? 0).FirstOrDefault())
                          .Where(x => x != null) // Ensure no null elements
                          .OrderByDescending(x => x!.EventId) // Sorting by EventId
                          .ToArray();

            return Ok(results);
        }
        else
        {
            return Ok(Array.Empty<SellOppEvent>());
        }
    }

    #endregion
}
