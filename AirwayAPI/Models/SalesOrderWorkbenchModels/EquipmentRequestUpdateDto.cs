public class EquipmentRequestUpdateDto
{
    public int Id { get; set; } // qtSalesOrderDetail Id
    public string RWSalesOrderNum { get; set; }
    public bool DropShipment { get; set; }
    public string Username { get; set; } // For email purposes
    public string Password { get; set; } // For email purposes
}