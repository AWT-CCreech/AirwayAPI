namespace AirwayAPI.Models;

public partial class TarInvoice
{
    public int InvcKey { get; set; }

    public decimal AuthOvrdAmt { get; set; }

    public decimal Balance { get; set; }

    public decimal BalanceHc { get; set; }

    public int BatchKey { get; set; }

    public int BillToAddrKey { get; set; }

    public int? BillToCustAddrKey { get; set; }

    public DateTime? ClosingPostDate { get; set; }

    public DateTime? ClosingTranDate { get; set; }

    public int? CommPlanKey { get; set; }

    public string CompanyId { get; set; } = null!;

    public int? ConfirmToCntctKey { get; set; }

    public decimal CostOfSales { get; set; }

    public DateTime? CreateDate { get; set; }

    public short CreateType { get; set; }

    public string? CreateUserId { get; set; }

    public double CurrExchRate { get; set; }

    public int? CurrExchSchdKey { get; set; }

    public string CurrId { get; set; } = null!;

    public int CustClassKey { get; set; }

    public int CustKey { get; set; }

    public string? CustPono { get; set; }

    public decimal DiscAmt { get; set; }

    public DateTime? DiscDate { get; set; }

    public decimal DiscTakenAmt { get; set; }

    public DateTime? DueDate { get; set; }

    public int? Fobkey { get; set; }

    public decimal HandlAmt { get; set; }

    public int? ImportLogKey { get; set; }

    public short InDispute { get; set; }

    public int? InvcFormKey { get; set; }

    public int? NextApplEntryNo { get; set; }

    public int? PmtTermsKey { get; set; }

    public DateTime PostDate { get; set; }

    public int? PrimarySperKey { get; set; }

    public short Printed { get; set; }

    public int? ReasonCodeKey { get; set; }

    public int? RecurInvoiceKey { get; set; }

    public decimal RetntAmt { get; set; }

    public decimal? RetntPct { get; set; }

    public decimal SalesAmt { get; set; }

    public decimal ShipAmt { get; set; }

    public int? ShipMethKey { get; set; }

    public int ShipToAddrKey { get; set; }

    public int? ShipToCustAddrKey { get; set; }

    public int? ShipZoneKey { get; set; }

    public short SourceModule { get; set; }

    public short Status { get; set; }

    public decimal StaxAmt { get; set; }

    public short StaxCalc { get; set; }

    public int? StaxTranKey { get; set; }

    public decimal TradeDiscAmt { get; set; }

    public decimal TranAmt { get; set; }

    public decimal TranAmtHc { get; set; }

    public string? TranCmnt { get; set; }

    public DateTime TranDate { get; set; }

    public string TranId { get; set; } = null!;

    public string TranNo { get; set; } = null!;

    public short Transmitted { get; set; }

    public int TranType { get; set; }

    public int UpdateCounter { get; set; }

    public DateTime? UpdateDate { get; set; }

    public string? UpdateUserId { get; set; }

    public int? VoucherKey { get; set; }
}
