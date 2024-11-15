using System.ComponentModel.DataAnnotations;

namespace AirwayAPI.Models.UtilityModels
{
    public class InventoryRulesRequestDto
    {
        [Required]
        public string PartNum { get; set; }

        [Required]
        public string AltPartNum { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "QtySold cannot be negative.")]
        public decimal QtySold { get; set; }

        [Range(1, 365, ErrorMessage = "CallDateRange must be between 1 and 365 days.")]
        public int CallDateRange { get; set; }

        [Required]
        public string RequestStatus { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "RequestID must be greater than zero.")]
        public int RequestID { get; set; }
    }
}
