using AirwayAPI.Models.EmailModels;

namespace AirwayAPI.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailInputBase emailInput);
    }
}
