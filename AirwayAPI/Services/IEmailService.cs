using AirwayAPI.Models.EmailModels;

namespace AirwayAPI.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailInputBase emailInput);
    }
}
