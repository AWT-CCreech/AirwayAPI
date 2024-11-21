namespace AirwayAPI.Models.UtilityModels
{
    public class SalesOrderUpdateDto
    {
        public int SaleId { get; set; }
        public int EventId { get; set; }
        public int QuoteId { get; set; }
        public string RWSalesOrderNum { get; set; } = string.Empty;
        public bool DropShipment { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; internal set; }
    }
}