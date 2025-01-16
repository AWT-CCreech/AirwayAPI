namespace AirwayAPI.Models.DTOs
{
    public class SalesOrderUpdateDto
    {
        public int SaleId { get; set; }
        public int EventId { get; set; }
        public int QuoteId { get; set; }
        public string SalesOrderNum { get; set; } = string.Empty;
        public bool DropShipment { get; set; } = false;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string HtmlBody { get; set; } = string.Empty;
    }
}