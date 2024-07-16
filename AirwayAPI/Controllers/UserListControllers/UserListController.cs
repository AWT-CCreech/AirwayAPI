using AirwayAPI.Data;
using AirwayAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserListController : ControllerBase
    {
        private readonly eHelpDeskContext _context;

        public UserListController(eHelpDeskContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a list of active users with their details, including uname, first name, last name,
        /// extension, direct phone number, cell phone number, and other relevant details.
        /// </summary>
        /// <returns>A list of active users with their details.</returns>
        [HttpGet("GetUserList")]
        public async Task<IActionResult> GetUserList()
        {
            var users = await _context.Users
                .Where(u => u.Active.HasValue && u.Active.Value == 1) // Checking for null and value
                .Join(
                    _context.PhoneNumbers,
                    user => user.Id,
                    phone => phone.UserId,
                    (user, phone) => new { User = user, Phone = phone })
                .Where(up => up.Phone.PhoneName == "ext") // Ensure 'PhoneName' is a simple property
                .GroupJoin( // Adding GroupJoin for camContacts
                    _context.CamContacts,
                    up => (up.User.Fname ?? string.Empty).Trim() + " " + (up.User.Lname ?? string.Empty).Trim(), // Key from Users
                    cc => (cc.Contact ?? string.Empty).Trim(), // Key from camContacts
                    (up, camContacts) => new { up, CamContact = camContacts.FirstOrDefault(c => c.ActiveStatus == 1 && c.Company == "AirWay Technologies") })
                .Select(upcc => new
                {
                    id = upcc.up.User.Id,
                    uname = upcc.up.User.Uname,
                    fname = upcc.up.User.Fname,
                    mname = upcc.up.User.Mname,
                    lname = upcc.up.User.Lname,
                    jobTitle = upcc.up.User.JobTitle,
                    email = upcc.up.User.Email,
                    extension = upcc.up.Phone.PhoneNumber1, // Assuming 'PhoneNumber' is a simple property
                    directPhone = "859.689.6" + (upcc.up.Phone.PhoneNumber1 ?? "223"),
                    mobilePhone = upcc.CamContact != null ? upcc.CamContact.PhoneCell : upcc.up.User.MobilePhone,
                })
                .OrderBy(upcc => upcc.lname) // Adding order by Last Name
                .ToListAsync();

            return Ok(users);
        }


        /// <summary>
        /// Adds a new user to the system.
        /// </summary>
        /// <param name="newUser">The user data to add.</param>
        /// <returns>The added user data, or an error message if the operation fails.</returns>
        [HttpPost("AddUser")]
        public async Task<IActionResult> AddUser([FromBody] User newUser)
        {
            if (newUser == null)
            {
                return BadRequest("User data must be provided.");
            }

            if (string.IsNullOrEmpty(newUser.Fname) || string.IsNullOrEmpty(newUser.Lname))
            {
                return BadRequest("Both first name and last name must be provided.");
            }

            try
            {
                // Set default values
                string firstLetterOfFname = string.IsNullOrEmpty(newUser.Fname) ? string.Empty : char.ToUpper(newUser.Fname[0]).ToString();
                newUser.Uname = $"{firstLetterOfFname}{newUser.Lname}";
                newUser.CompanyId = 2;
                newUser.TeamGroup = "Closed";
                newUser.SkillLevel = "Beginner";
                newUser.Email = $"{newUser.Uname}@airway.com";
                newUser.LocationId = 3;
                newUser.NewPortal = 1;

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();
                return Ok(newUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while adding the user: " + ex.Message);
            }
        }


        /// <summary>
        /// Updates the details of an existing user.
        /// </summary>
        /// <param name="id">The ID of the user to update.</param>
        /// <param name="userData">The updated user data.</param>
        /// <returns>The updated user data, or an error message if the operation fails.</returns>
        [HttpPut("UpdateUser/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDto userData)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Update User table
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
                if (user == null)
                {
                    return NotFound($"User with ID {id} not found.");
                }


                user.Uname = userData.Uname;
                user.Fname = userData.Fname;
                user.Mname = userData.Mname;
                user.Lname = userData.Lname;
                user.JobTitle = userData.JobTitle;
                user.Email = userData.Email;
                user.ModDate = DateTime.Now;
                user.Extension = userData.Extension;
                user.MobilePhone = userData.MobilePhone;
                user.DirectPhone = userData.DirectPhone;

                _context.Users.Update(user);

                // Update PhoneNumber table
                var phoneNumber = await _context.PhoneNumbers.FirstOrDefaultAsync(pn => pn.UserId == id && pn.PhoneName == "ext");
                if (phoneNumber != null)
                {
                    phoneNumber.PhoneNumber1 = userData.Extension;
                    _context.PhoneNumbers.Update(phoneNumber);
                }

                // Update CamContact table
                if (!string.IsNullOrWhiteSpace(userData.Fname) && !string.IsNullOrWhiteSpace(userData.Lname))
                {
                    var camContact = await _context.CamContacts.FirstOrDefaultAsync(cc =>
                        (cc.Contact ?? string.Empty).Trim() == $"{userData.Fname.Trim()} {userData.Lname.Trim()}");

                    if (camContact != null)
                    {
                        camContact.PhoneCell = userData.MobilePhone;
                        _context.CamContacts.Update(camContact);
                    }
                }

                // Save all changes
                await _context.SaveChangesAsync();

                // Commit transaction
                await transaction.CommitAsync();

                return Ok(userData);
            }
            catch (Exception ex)
            {
                // Rollback transaction if any error occurs
                await transaction.RollbackAsync();
                return StatusCode(500, "An error occurred while updating the user: " + ex.Message);
            }
        }

        /// <summary>
        /// Sets the active status of an existing user to 0, effectively deactivating the user.
        /// </summary>
        /// <param name="id">The ID of the user to deactivate.</param>
        /// <returns>A message indicating the result of the deactivate operation.</returns>
        [HttpDelete("DeleteUser/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            try
            {
                user.Active = 0; // Set the active status to 0 to deactivate the user
                user.ModDate = DateTime.Now;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return Ok($"User with ID {id} has been deactivated.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while deactivating the user: " + ex.Message);
            }
        }

    }
}