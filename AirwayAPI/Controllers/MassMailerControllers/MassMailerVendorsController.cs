using AirwayAPI.Data;
using AirwayAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            var queryForCamContacts = _context.CamContacts
                .Where(vendor => vendor.Email != null && vendor.Email.Contains('@') && vendor.ActiveStatus == 1);

            if (mfg.Trim().ToLower() != "all")
            {
                queryForCamContacts = queryForCamContacts.Where(vendor => 
                    vendor.Mfgs != null && 
                    vendor.Mfgs.ToLower().Contains(mfg.Trim().ToLower())
                );
            }

            if (anc == false && fne == false)
            {
                queryForCamContacts = queryForCamContacts.Where(vendor =>
                    (vendor.ContactType ?? string.Empty).Trim() == "Reseller" ||
                    (vendor.ContactType ?? string.Empty).Trim() == "FNE Vendor" ||
                    (vendor.ContactType ?? string.Empty).Trim() == "OEM" ||
                    (vendor.ContactType ?? string.Empty).Trim() == "Ancillary Vendor" ||
                    (vendor.ContactType ?? string.Empty).Trim() == "Central Office" ||
                    (vendor.ContactType ?? string.Empty).Trim() == "Service Vendor"
                );

            }
            else if (anc == true)
            {
                queryForCamContacts = queryForCamContacts.Where(vendor =>
                    (vendor.ContactType ?? string.Empty).Trim() == "OEM" ||
                    (vendor.ContactType ?? string.Empty).Trim() == "Ancillary Vendor" ||
                    (vendor.ContactType ?? string.Empty).Trim() == "Central Office" ||
                    (vendor.ContactType ?? string.Empty).Trim() == "Service Vendor"
                );
            }
            else if (fne == true)
            {
                queryForCamContacts = queryForCamContacts.Where(vendor =>
                    (vendor.ContactType ?? string.Empty).Trim() == "Reseller" ||
                    (vendor.ContactType ?? string.Empty).Trim() == "FNE Vendor"
                );
            }


            var queryForVendors = queryForCamContacts.Select(vendor => new MassMailerVendor
            {
                Id = vendor.Id,
                Contact = vendor.Contact ?? string.Empty,
                Email = vendor.Email ?? string.Empty,
                Company = vendor.Company ?? string.Empty,
                MainVendor = vendor.MainVendor
            }).OrderBy(vendor => vendor.Company).ToListAsync();


            return await queryForVendors;
        }
    }
}
