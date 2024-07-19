using AirwayAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AirwayAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MassMailerVendorsController : ControllerBase
    {
        private readonly eHelpDeskContext _context;

        public MassMailerVendorsController(eHelpDeskContext context)
        {
            _context = context;
        }

        // GET: api/MassMailerVendors
        [HttpGet("{mfg}/{anc}/{fne}")]
        public async Task<ActionResult<IEnumerable<MassMailerVendor>>> GetVendors(string mfg, bool anc, bool fne)
        {
            // Fetch data from the database first
            var vendors = await _context.CamContacts
                .Where(vendor => vendor.Email != null && vendor.ActiveStatus == 1)
                .ToListAsync();

            // Apply in-memory filtering for Email containing '@'
            vendors = vendors.Where(vendor => vendor.Email != null && vendor.Email.Contains('@')).ToList();

            // Apply in-memory filtering for Mfgs
            if (mfg.Trim().ToLower() != "all")
            {
                vendors = vendors.Where(vendor =>
                    vendor.Mfgs != null &&
                    vendor.Mfgs.ToLower().Contains(mfg.Trim().ToLower())
                ).ToList();
            }

            // Apply in-memory filtering for ContactType
            if (anc == false && fne == false)
            {
                vendors = vendors.Where(vendor =>
                    (vendor.ContactType ?? string.Empty).Trim() == "Reseller" ||
                    (vendor.ContactType ?? string.Empty).Trim() == "FNE Vendor" ||
                    (vendor.ContactType ?? string.Empty).Trim() == "OEM" ||
                    (vendor.ContactType ?? string.Empty).Trim() == "Ancillary Vendor" ||
                    (vendor.ContactType ?? string.Empty).Trim() == "Central Office" ||
                    (vendor.ContactType ?? string.Empty).Trim() == "Service Vendor"
                ).ToList();
            }
            else if (anc == true)
            {
                vendors = vendors.Where(vendor =>
                    (vendor.ContactType ?? string.Empty).Trim() == "OEM" ||
                    (vendor.ContactType ?? string.Empty).Trim() == "Ancillary Vendor" ||
                    (vendor.ContactType ?? string.Empty).Trim() == "Central Office" ||
                    (vendor.ContactType ?? string.Empty).Trim() == "Service Vendor"
                ).ToList();
            }
            else if (fne == true)
            {
                vendors = vendors.Where(vendor =>
                    (vendor.ContactType ?? string.Empty).Trim() == "Reseller" ||
                    (vendor.ContactType ?? string.Empty).Trim() == "FNE Vendor"
                ).ToList();
            }

            var queryForVendors = vendors.Select(vendor => new MassMailerVendor
            {
                Id = vendor.Id,
                Contact = vendor.Contact ?? string.Empty,
                Email = vendor.Email ?? string.Empty,
                Company = vendor.Company ?? string.Empty,
                MainVendor = vendor.MainVendor
            }).OrderBy(vendor => vendor.Company).ToList();

            return Ok(queryForVendors);
        }
    }
}
