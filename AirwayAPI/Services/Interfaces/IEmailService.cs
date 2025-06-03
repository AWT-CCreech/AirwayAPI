using AirwayAPI.Models.EmailModels;

namespace AirwayAPI.Services.Interfaces;

public interface IEmailService
{
    /// <summary>
    /// Sends an email using the provided EmailInputBase configuration.
    /// </summary>
    /// <param name="emailInput">The email input details, including recipients, subject, and body.</param>
    Task SendEmailAsync(EmailInputBase emailInput);

    /// <summary>
    /// Retrieves sender information based on the provided username.
    /// </summary>
    /// <param name="username">The username of the sender.</param>
    /// <returns>A tuple containing sender details (FullName, Email, JobTitle, DirectPhone, MobilePhone).</returns>
    Task<(string FullName, string Email, string JobTitle, string DirectPhone, string MobilePhone)> GetSenderInfoAsync(string username);
}
