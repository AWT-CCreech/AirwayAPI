namespace AirwayAPI.Controllers.UtilityControllers
{
    public class InventoryRulesResponseDto
    {
        public decimal QtyOnHand { get; set; }
        public decimal QtyInPick { get; set; }
        public decimal DDQty { get; set; }
        public decimal Adjustments { get; set; }
        public decimal QtyFound { get; set; }
        public decimal NeedToFind { get; set; }
        public decimal NeedToBuy { get; set; }
        public decimal QtyAvail { get; set; }
        public decimal QtyBought { get; set; }
        public decimal QtyOnHandCost { get; set; }
        public decimal QtyInStock { get; set; }
        public decimal QtyToBuyNew { get; set; }
        public decimal QtySoldToday { get; set; }
        public decimal QtyAvailToSell { get; set; }
    }
}
