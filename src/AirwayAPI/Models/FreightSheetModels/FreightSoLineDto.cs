// DTOs/FreightSolineDto.cs
namespace AirwayAPI.Models.FreightSheetModels
{
    /// <summary>
    /// Mirrors the ASP array fields: aryShipmentID, arySONum, aryFreightCharge, aryMU, aryPackageHandling, aryTotalFreight
    /// </summary>
    public class FreightSoLineDto
    {
        /// <summary>
        /// When updating an existing line, Id>0. When adding a brand new row, Id==0.
        /// </summary>
        public int Id { get; set; }

        public int? Sonum { get; set; }
        public decimal? FreightCharge { get; set; }
        public decimal? Markup { get; set; }
        public decimal? PackageHandling { get; set; }
        public decimal? TotalFreight { get; set; }
    }
}
