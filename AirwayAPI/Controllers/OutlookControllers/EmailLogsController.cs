using AirwayAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace AirwayAPI.Controllers.OutlookControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailLogsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public EmailLogsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> LogEmail([FromBody] EmailLog emailLog)
        {
            if (emailLog == null)
            {
                return BadRequest("Invalid email log data.");
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                const string query = @"
                    INSERT INTO EmailLogs (Subject, Body, OrderType, OrderNumber, SenderEmail, Recipients, LoggedBy)
                    VALUES (@Subject, @Body, @OrderType, @OrderNumber, @SenderEmail, @Recipients, @LoggedBy)";

                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Subject", emailLog.Subject);
                command.Parameters.AddWithValue("@Body", emailLog.Body);
                command.Parameters.AddWithValue("@OrderType", emailLog.OrderType);
                command.Parameters.AddWithValue("@OrderNumber", emailLog.OrderNumber);
                command.Parameters.AddWithValue("@SenderEmail", emailLog.SenderEmail);
                command.Parameters.AddWithValue("@Recipients", emailLog.Recipients);
                command.Parameters.AddWithValue("@LoggedBy", emailLog.LoggedBy);

                connection.Open();
                await command.ExecuteNonQueryAsync();
            }

            return Ok(new { message = "Email log saved successfully." });
        }
    }
}