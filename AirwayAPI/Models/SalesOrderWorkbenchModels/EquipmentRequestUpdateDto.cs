namespace AirwayAPI.Models.SalesOrderWorkbenchModels
{
    public class EquipmentRequestUpdateDto
    {
        public int Id { get; set; }
        public string RWSalesOrderNum { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Subject { get; set; }
        public string HtmlBody { get; set; }
    }
}