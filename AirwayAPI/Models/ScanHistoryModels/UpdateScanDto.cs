namespace AirwayAPI.Models.ScanHistoryModels
{
    public class UpdateScanDto
    {
        public int RowId { get; set; }
        public DateTime? ScanDate { get; set; }
        public string? UserName { get; set; }
        /// <summary>
        /// This value indicates the order type (SO, PO, RMA, RTV/C). Your update logic can decide which order number to change.
        /// </summary>
        public string? OrderType { get; set; }
        public string? OrderNum { get; set; }
        public string? PartNo { get; set; }
        public string? SerialNo { get; set; }
        public string? HeciCode { get; set; }
        // Add additional fields as needed.
    }
}
