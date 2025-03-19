namespace AirwayAPI.Models.SalesOrderWorkbenchModels
{
    public class DetailLevelUpdateDto
    {
        public int EventId { get; set; }
        public int RequestId { get; set; }
        public string SalesOrderNum { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string HtmlBody { get; set; } = string.Empty;
    }
}