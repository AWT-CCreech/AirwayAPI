using AirwayAPI.Data;
using AirwayAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MassMailerPartItemsController : ControllerBase
    {
        private readonly eHelpDeskContext _context;

        public MassMailerPartItemsController(eHelpDeskContext context)
        {
            _context = context;
        }

        // GET: api/MassMailerPartItems
        [HttpGet("{userId}")]
        public async Task<ActionResult<IEnumerable<MassMailerPartItem>>> GetEquipmentRequest(int userId)
        {
            List<MassMailerPartItem> items = new();
            /*-----------  Query for showing Today's Marked Mass Mailings  -----------*
             *----------- for that user - THAT HAVE PART NUMBERS (grouped) -----------*/
            string company = "";
            double? quantity = 0.0;
            string partNum = "";
            string altPartNum = "";
            string xPartNum = "";
            int requestID = 0;
            string revision = "";
            string mfg = "";
            string partDesc = "";

            var markedPartsWithPartNum = await _context.EquipmentRequests
                .Where(er => er.MassMailing == true &&
                    er.MassMailDate.HasValue && // Check if MassMailDate is not null
                    EF.Functions.DateDiffDay(er.MassMailDate.Value, DateTime.Now) == 0 &&
                    er.MassMailSentBy == userId &&
                    ((er.PartNum ?? string.Empty).Trim().Length > 1 ||
                     (er.AltPartNum ?? string.Empty).Trim().Length > 1)) // Check if PartNum and AltPartNum are not null
                .Select(er => new { PartNum = er.PartNum ?? string.Empty, AltPartNum = er.AltPartNum ?? string.Empty }) // Ensure non-null values
                .Distinct()
                .OrderBy(er => er.PartNum)
                .ToListAsync();


            for (int i = 0; i < markedPartsWithPartNum.Count; ++i)
            {
                var part = markedPartsWithPartNum[i];
                company = "";
                //string partNumTemp = part.PartNum.Trim().Length > 0 ? part.PartNum.Trim() : part.AltPartNum.Length > 0 ? part.AltPartNum.Trim() : "";
                if (part.PartNum.Trim().Length > 0 || part.AltPartNum.Trim().Length > 0)
                {
                    /*----------- QUERY FOR GETTING PART DESC AND MFG -----------*/

                    var temp = await _context.EquipmentRequests
                        .Where(er => er.MassMailing == true &&
                            er.MassMailDate.HasValue && // Check if MassMailDate is not null
                            EF.Functions.DateDiffDay(er.MassMailDate.Value, DateTime.Now) == 0 &&
                            ((part.PartNum.Trim().Length > 0 && er.PartNum == part.PartNum) ||
                             (part.AltPartNum.Trim().Length > 0 && er.AltPartNum == part.AltPartNum)))
                        .Select(er => new
                        {
                            er.RequestId,
                            er.PartDesc,
                            er.Manufacturer,
                            er.RevDetails
                        })
                        .FirstOrDefaultAsync();



                    int RequestId = temp?.RequestId ?? 0; // Default to 0 if temp or RequestId is null
                    string PartDesc = temp?.PartDesc ?? string.Empty; // Default to empty string if temp or PartDesc is null
                    string Manufacturer = temp?.Manufacturer ?? string.Empty; // Default to empty string if temp or Manufacturer is null
                    string RevDetails = temp?.RevDetails ?? string.Empty; // Default to empty string if temp or RevDetails is null

                    double? Quantity = await _context.EquipmentRequests
                        .Where(er => er.MassMailing == true &&
                            er.MassMailDate.HasValue && // Check if MassMailDate is not null
                            EF.Functions.DateDiffDay(er.MassMailDate.Value, DateTime.Now) == 0 &&
                            er.MassMailSentBy == userId &&
                            ((part.PartNum.Trim().Length > 0 && er.PartNum == part.PartNum) ||
                             (part.AltPartNum.Trim().Length > 0 && er.AltPartNum == part.AltPartNum)))
                        .Select(er => er.Quantity ?? 0) // Ensure Quantity is not null
                        .SumAsync();


                    // Here we find the companies for each part num request and their qty
                    // ***CAM UPDATE***

                    var query = from r in _context.EquipmentRequests
                                join e in _context.RequestEvents on r.EventId equals e.EventId
                                join c in _context.CamContacts on e.ContactId equals c.Id
                                where r.MassMailing == true &&
                                      r.MassMailDate.HasValue && // Check if MassMailDate is not null
                                      EF.Functions.DateDiffDay(r.MassMailDate.Value, DateTime.Now) == 0 &&
                                      r.MassMailSentBy == userId &&
                                      ((part.PartNum != null && part.PartNum.Trim().Length > 0 && r.PartNum == part.PartNum) ||
                                       (part.AltPartNum != null && part.AltPartNum.Trim().Length > 0 && r.AltPartNum == part.AltPartNum))
                                select new { c.Company, r.Quantity };


                    var companies = await query.ToListAsync();
                    companies.ForEach(c =>
                    {
                        company = company + c.Company + "(" + c.Quantity + "),";
                    });

                    company = company.Trim();
                    company = company[..^1];
                    quantity = Quantity;
                    partNum = part.PartNum.Trim();
                    altPartNum = part.PartNum.Trim().Length > 0 ? part.AltPartNum.Trim() :
                                part.AltPartNum.Trim().Length > 0 ? part.AltPartNum.Trim().Replace(",", " ") : "";
                    xPartNum = part.PartNum.Trim().Length > 0 ? part.PartNum.Trim() :
                                part.AltPartNum.Trim().Length > 0 ? part.AltPartNum.Replace(",", " ") : "";
                    requestID = RequestId;
                    revision = RevDetails;
                    mfg = Manufacturer;
                    partDesc = PartDesc.Replace(',', ' ').Replace("\"", "");

                    // put the value 'none' into the part num field in case they want to send a blank part num in the mailer
                    if (xPartNum.Length == 0)
                        xPartNum = "NONE";
                    if (altPartNum.Length == 0)
                        altPartNum = "x";

                    items.Add(new MassMailerPartItem
                    {
                        Id = requestID,
                        PartNum = xPartNum.Trim() ?? "",
                        AltPartNum = altPartNum.Trim() ?? "",
                        PartDesc = partDesc ?? "",
                        Qty = quantity ?? 0,
                        Company = company ?? "",
                        Manufacturer = mfg ?? "",
                        Revision = revision ?? ""
                    });
                }
            }


            /*-----------      Query for showing Today's Marked Mass Mailings     -----------
             *----------- for that user - THAT HAVE NO PART NUMBERS (not grouped) -----------*/
            var markedPartsWithoutPartNum =
                await (from r in _context.EquipmentRequests
                       join e in _context.RequestEvents on r.EventId equals e.EventId
                       join c in _context.CamContacts on e.ContactId equals c.Id
                       where r.MassMailing == true &&
                             r.MassMailDate.HasValue && // Check if MassMailDate is not null
                             EF.Functions.DateDiffDay(r.MassMailDate.Value, DateTime.Now) == 0 &&
                             r.MassMailSentBy == userId &&
                             (r.PartNum == null || r.PartNum.Trim() == "") && // Check if PartNum is null or empty
                             (r.AltPartNum == null || r.AltPartNum.Trim() == "") // Check if AltPartNum is null or empty
                       orderby r.PartNum
                       select new
                       {
                           r.RequestId,
                           r.PartDesc,
                           r.Quantity,
                           r.Manufacturer,
                           PartNum = r.PartNum ?? string.Empty, // Handle potential null values
                           AltPartNum = r.AltPartNum ?? string.Empty, // Handle potential null values
                           c.Company
                       }).ToListAsync();


            for (var i = 0; i < markedPartsWithoutPartNum.Count; ++i)
            {
                Console.WriteLine(i);
                var part = markedPartsWithoutPartNum[i];
                items.Add(new MassMailerPartItem
                {
                    Id = part.RequestId,
                    PartNum = part.PartNum ?? "",
                    AltPartNum = "",
                    PartDesc = part.PartDesc ?? "",
                    Qty = part.Quantity ?? 0,
                    Company = part.Company ?? "",
                    Manufacturer = part.Manufacturer ?? "",
                    Revision = ""
                });
            }

            return Ok(items);
        }
    }
}
