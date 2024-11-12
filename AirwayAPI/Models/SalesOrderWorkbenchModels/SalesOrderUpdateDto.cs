using System.ComponentModel.DataAnnotations;

namespace AirwayAPI.Models.SalesOrderWorkbenchModels
{
    public class SalesOrderUpdateDto
    {
        [Required]
        public int SaleId { get; set; }

        [Required]
        public string RWSalesOrderNum { get; set; } = string.Empty;

        public bool DropShipment { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        public int QuoteId { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string HtmlBody { get; set; } = string.Empty;
    }
}