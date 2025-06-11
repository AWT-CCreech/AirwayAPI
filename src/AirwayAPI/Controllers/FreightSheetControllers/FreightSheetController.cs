using AirwayAPI.Models;
using AirwayAPI.Models.EmailModels;
using AirwayAPI.Models.FreightSheetModels;
using AirwayAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirwayAPI.Controllers.FreightSheetControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FreightSheetController(
        ILogisticsService logisticsService,
        IEmailService emailService,
        IConfiguration configuration) : ControllerBase
    {
        private readonly ILogisticsService _logisticsService = logisticsService;
        private readonly IEmailService _emailService = emailService;
        private readonly IConfiguration _configuration = configuration;

        /// <summary>
        /// POST /api/FreightSheet/save
        /// Creates a new FreightQuote header plus one default FreightSo line.
        /// Then sends the “Sales Order … has Shipped” email to the SalesRep.
        /// Returns the newly‐created FreightQuote.Id.
        /// </summary>
        [HttpPost("save")]
        [Authorize]
        public async Task<ActionResult<int>> Save([FromBody] FreightQuoteDto dto)
        {
            if (dto == null)
                return BadRequest("Payload cannot be null.");

            // 1) Call the logistics service to insert header + default line
            var currentUser = User.Identity?.Name ?? "system";
            var newQuoteId = await _logisticsService.CreateFreightQuoteAsync(dto, currentUser);

            // 2) Craft and send the “has shipped” email
            //    In the old ASP code, the “SalesRepEmail” was simply SalesRep + "@airway.com".
            var salesRepEmail = $"{dto.SalesRep}@airway.com";
            var subject = $"Sales Order {dto.Sonum} has Shipped";

            // Build a minimal HTML or plain‐text body. If you have a full template,
            // you can use placeholders / templates just as EmailService expects.
            //
            // Here, we just set Body = plain summary; EmailService will inject it into %%EMAILBODY%%.
            // If you have a richer template, you can add more placeholders (e.g. %%CUSTOMERPO%%, etc.)
            var body = $"<p><b>Sales Order #{dto.Sonum} has shipped.</b></p>" +
                       (dto.ShipmentValue != null
                           ? $"<p><b>Shipment Value:</b> {dto.ShipmentValue:C}</p>"
                           : string.Empty) +
                       $"<p><b>Ship To:</b> {dto.ShipTo}</p>" +
                       $"<p><b>Carrier:</b> {dto.CarrierUsed}</p>" +
                       $"<p><b>Ship Date:</b> {dto.ShipDate:MM/dd/yyyy}</p>" +
                       $"<p><b>Tracking Number:</b> {dto.TrackNum}</p>";

            // 3) Prepare EmailInputBase (use your actual EmailInputBase structure)
            var emailInput = new EmailInputBase
            {
                // Recipients
                ToEmails = [salesRepEmail],

                // If you want to CC yourself or others, add them here
                CCEmails = [],

                Subject = subject,

                // Body goes into %%EMAILBODY%% placeholder by default
                Body = body,

                // You can pass placeholders dictionary if your template uses any other tokens.
                Placeholders = new Dictionary<string, string>
                {
                    // EmailService will automatically set %%NAME%%, %%JOBTITLE%%, etc. if UserName & Password are provided.
                    // If you have additional tokens in your company template, set them here:
                    // { "%%CUSTOMERPO%%", customerPo ?? string.Empty },
                    // { "%%SHIPTO%%", dto.ShipTo },
                    // etc.
                },

                // The template (Email.EmailTemplate) likely expects %%EMAILBODY%% to be replaced.
                //    So we leave Body = actual HTML snippet; EmailService will inject it.

                // USERNAME / PASSWORD are needed to look up placeholder info (FullName, JobTitle, etc.)
                // If you have service‐account credentials in configuration, you can read them:
                UserName = _configuration["EmailSettings:SmtpUserName"],
                Password = _configuration["EmailSettings:SmtpEncryptedPassword"]
                // (Assuming you store the encrypted password in config and EmailService will decrypt.)
            };

            // 4) Send the email
            try
            {
                await _emailService.SendEmailAsync(emailInput);
            }
            catch (Exception ex)
            {
                // Log or swallow, depending on your tolerance. We still return OK since shipping created succeeded.
                // You might want to return an error if email is critical.
                // For now, we’ll just log to console:
                Console.Error.WriteLine($"Error sending freight‐shipped email: {ex.Message}");
            }

            return Ok(newQuoteId);
        }

        /// <summary>
        /// POST /api/FreightSheet/update
        /// Updates the FreightQuote header and all FreightSo line‐items.
        /// After update, it also re‐sends the “has shipped” email (if desired).
        /// </summary>
        [HttpPost("update")]
        [Authorize]
        public async Task<IActionResult> Update([FromBody] FreightQuoteDto dto)
        {
            if (dto == null || dto.FreightQuoteId <= 0)
                return BadRequest("FreightQuoteId must be provided and > 0 when updating.");

            var currentUser = User.Identity?.Name ?? "system";
            await _logisticsService.UpdateFreightQuoteAsync(dto, currentUser);

            // Optional: re‐send email on update (the old ASP only sent on Save, but if you want to notify on update, un‐comment below)
            /*
            var salesRepEmail = $"{dto.SalesRep}@airway.com";
            var subject = $"Sales Order {dto.Sonum} has Updated Freight Info";
            var body = $"<p>Your freight sheet for Sales Order #{dto.Sonum} has been updated.</p>";
            var emailInput = new EmailInputBase
            {
                ToEmails = new List<string> { salesRepEmail },
                Subject = subject,
                Body = body,
                Placeholders = new Dictionary<string, string>(),
                UserName = _configuration["EmailSettings:SmtpUserName"],
                Password = _configuration["EmailSettings:SmtpEncryptedPassword"]
            };
            await _emailService.SendEmailAsync(emailInput);
            */

            return NoContent();
        }

        /// <summary>
        /// POST /api/FreightSheet/{freightQuoteId}/addrow
        /// Inserts one blank FreightSo row (all numeric fields = 0) under the specified FreightQuote.
        /// Returns the newly‐inserted FreightSo.Id.
        /// </summary>
        [HttpPost("{freightQuoteId:int}/addrow")]
        [Authorize]
        public async Task<ActionResult<int>> AddRow([FromRoute] int freightQuoteId)
        {
            if (freightQuoteId <= 0)
                return BadRequest("Invalid FreightQuoteId.");

            var currentUser = User.Identity?.Name ?? "system";
            var newLineId = await _logisticsService.AddFreightSoAsync(freightQuoteId, currentUser);
            return Ok(newLineId);
        }

        /// <summary>
        /// GET /api/FreightSheet/{id}
        /// Retrieves the FreightQuote header for the given Id.
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<ActionResult<FreightQuote>> GetFreightQuote(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid FreightQuoteId.");

            var quote = await _logisticsService.GetFreightQuoteByIdAsync(id);
            return Ok(quote);
        }

        /// <summary>
        /// GET /api/FreightSheet/{id}/lines
        /// Retrieves all FreightSo lines associated with the given FreightQuote.Id.
        /// </summary>
        [HttpGet("{id:int}/lines")]
        [Authorize]
        public async Task<ActionResult<List<FreightSo>>> GetFreightSoLines(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid FreightQuoteId.");

            var lines = await _logisticsService.GetFreightSoByQuoteIdAsync(id);
            return Ok(lines);
        }
    }
}