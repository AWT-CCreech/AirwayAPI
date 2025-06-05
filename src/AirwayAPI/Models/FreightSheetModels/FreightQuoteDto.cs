// DTOs/FreightQuoteDto.cs
namespace AirwayAPI.Models.FreightSheetModels
{
    /// <summary>
    /// Captures exactly the “header” fields which the ASP page posted (and also the array of line‐items).
    /// </summary>
    public class FreightQuoteDto
    {
        // When frmAction="Save", FreightQuoteId will be 0. When frmAction="Update" or "AddRow", FreightQuoteId>0.
        public int FreightQuoteId { get; set; }

        public int? EventId { get; set; }          // Comes from hidden field
        public int? SolinesCount { get; set; }     // “SOLines” hidden field

        // Header‐level fields:
        public string ShipFrom { get; set; }
        public string ShipFromNum { get; set; }
        public string ShipFromAddress1 { get; set; }
        public string ShipFromAddress2 { get; set; }
        public string ShipFromAddress3 { get; set; }
        public string ShipFromAddress4 { get; set; }

        public string ShipTo { get; set; }
        public string ShipToNum { get; set; }
        public string ShipToAddress1 { get; set; }
        public string ShipToAddress2 { get; set; }
        public string ShipToAddress3 { get; set; }
        public string ShipToAddress4 { get; set; }

        public string Sonum { get; set; }          // SO Number (always posted as string)
        public string SalesRep { get; set; }       // SalesRep username

        public decimal? ShipmentValue { get; set; }
        public DateTime? ShipDate { get; set; }
        public string TrackNum { get; set; }
        public string ServiceUsed { get; set; }
        public string CarrierUsed { get; set; }
        public string ShipmentNotes { get; set; }
        public string BillOfLading { get; set; }

        public int? TotalPieces { get; set; }
        public int? TotalWeight { get; set; }

        /// <summary>
        /// A flat list of all the FreightSO line‐items.  When “AddRow” was clicked, ASP would render one more row,
        /// so we still get a FreightSolineDto for each row, some of which may have Id==0 (new insert).
        /// </summary>
        public List<FreightSoLineDto> Lines { get; set; } = new();
    }
}
