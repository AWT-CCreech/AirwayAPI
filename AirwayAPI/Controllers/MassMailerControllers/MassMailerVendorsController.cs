using AirwayAPI.Data;
using AirwayAPI.Models.MassMailerModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.MassMailerControllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MassMailerVendorsController(eHelpDeskContext context) : ControllerBase
{
    private readonly eHelpDeskContext _context = context;

    // GET: api/MassMailerVendors
    [HttpGet("{mfg}/{anc}/{fne}")]
    public async Task<ActionResult<IEnumerable<MassMailerVendor>>> GetVendors(string mfg, bool anc, bool fne)
    {
        // Prepare the base query asynchronously
        var query = _context.CamContacts
            .Where(vendor => vendor.Email != null && EF.Functions.Like(vendor.Email, "%@%") && vendor.ActiveStatus == 1);

        // Apply filtering for Mfgs
        var mfgLower = mfg.Trim().ToLower();
        if (mfgLower != "all")
        {
            query = query.Where(vendor => vendor.Mfgs != null && EF.Functions.Like(vendor.Mfgs.ToLower(), $"%{mfgLower}%"));
        }

        // Apply filtering for ContactType based on 'anc' and 'fne' flags
        if (!anc && !fne)
        {
            query = query.Where(vendor =>
                vendor.ContactType == "Reseller" ||
                vendor.ContactType == "FNE Vendor" ||
                vendor.ContactType == "OEM" ||
                vendor.ContactType == "Ancillary Vendor" ||
                vendor.ContactType == "Central Office" ||
                vendor.ContactType == "Service Vendor"
            );
        }
        else if (anc)
        {
            query = query.Where(vendor =>
                vendor.ContactType == "OEM" ||
                vendor.ContactType == "Ancillary Vendor" ||
                vendor.ContactType == "Central Office" ||
                vendor.ContactType == "Service Vendor"
            );
        }
        else if (fne)
        {
            query = query.Where(vendor =>
                vendor.ContactType == "Reseller" ||
                vendor.ContactType == "FNE Vendor"
            );
        }

        // Execute the query and transform the result to MassMailerVendor asynchronously
        var result = await query
            .Select(vendor => new MassMailerVendor
            {
                Id = vendor.Id,
                Contact = vendor.Contact ?? string.Empty,
                Email = vendor.Email ?? string.Empty,
                Company = vendor.Company ?? string.Empty,
                MainVendor = vendor.MainVendor
            })
            .OrderBy(vendor => vendor.Company) // Sort by Company
            .ToListAsync();

        return Ok(result);
    }
}
