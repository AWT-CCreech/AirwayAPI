namespace AirwayAPI.Models.SalesOrderWorkbenchModels
{
    public class SalesOrderUpdateDto
    {
        public int SaleId { get; set; }
        public string RWSalesOrderNum { get; set; }
        public bool DropShipment { get; set; }
        public int EventId { get; set; }
        public int QuoteId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Subject { get; set; }
        public string HtmlBody { get; set; }
    }
}