namespace AirwayAPI.Models.ScanHistoryModels
{
    public class CopyScansDto
    {
        public string FromCompany { get; set; } = "AirWay";
        public string ToCompany { get; set; } = "AirWay";
        public string FromOrderType { get; set; } = "PO"; // default as in your legacy code
        public string ToOrderType { get; set; } = "SO";     // default as in your legacy code
        public string FromOrderNum { get; set; } = string.Empty;
        public string ToOrderNum { get; set; } = string.Empty;
        public string RequestedBy { get; set; } = string.Empty;
        public IEnumerable<int> SelectedIDs { get; set; } = Enumerable.Empty<int>();
    }
}
