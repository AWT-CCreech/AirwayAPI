namespace AirwayAPI.Models.SalesOrderWorkbenchModels
{
    public class EquipmentRequestUpdateDto
    {
        public int Id { get; set; }
        public string RWSalesOrderNum { get; set; } = string.Empty;
        public bool DropShipment { get; set; } = false;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string HtmlBody { get; set; } = string.Empty;
    }
}