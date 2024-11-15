using System.ComponentModel.DataAnnotations;

namespace AirwayAPI.Models.SalesOrderWorkbenchModels
{
    public class SalesOrderUpdateDto
    {
        public int SaleId { get; set; }
        public int EventId { get; set; }
        public int QuoteId { get; set; }
        public string RWSalesOrderNum { get; set; }
        public bool DropShipment { get; set; }
        public string Username { get; set; } // For email purposes
        public string Password { get; set; } // For email purposes
    }

}