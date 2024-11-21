namespace AirwayAPI.Models.DTOs
{
    public class EquipmentRequestUpdateDto
    {
        public int Id { get; set; }
        public string RWSalesOrderNum { get; set; } = string.Empty;
        public bool DropShipment { get; set; }
        public string Username { get; set; } = string.Empty;
    }
}