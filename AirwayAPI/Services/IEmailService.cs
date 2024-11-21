using AirwayAPI.Models.DTOs;
using System.Threading.Tasks;

namespace AirwayAPI.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailInputDto emailInput);
    }
}
