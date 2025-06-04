namespace AirwayAPI.Models;

public partial class TarCustomer
{
    public int CustKey { get; set; }

    public string? Abano { get; set; }

    public short AllowCustRefund { get; set; }

    public short AllowWriteOff { get; set; }

    public short BillingType { get; set; }

    public short BillToNationalAcctParent { get; set; }

    public string CompanyId { get; set; } = null!;

    public short ConsolidatedStatement { get; set; }

    public DateTime? CreateDate { get; set; }

    public short CreateType { get; set; }

    public string? CreateUserId { get; set; }

    public decimal CreditLimit { get; set; }

    public short CreditLimitAgeCat { get; set; }

    public short CreditLimitUsed { get; set; }

    public string? CrmcustId { get; set; }

    public int? CurrExchSchdKey { get; set; }

    public int CustClassKey { get; set; }

    public string CustId { get; set; } = null!;

    public string CustName { get; set; } = null!;

    public string? CustRefNo { get; set; }

    public DateTime? DateEstab { get; set; }

    public int DfltBillToAddrKey { get; set; }

    public int? DfltItemKey { get; set; }

    public decimal DfltMaxUpCharge { get; set; }

    public short DfltMaxUpChargeType { get; set; }

    public int? DfltSalesAcctKey { get; set; }

    public int? DfltSalesReturnAcctKey { get; set; }

    public int DfltShipToAddrKey { get; set; }

    public decimal FinChgFlatAmt { get; set; }

    public decimal? FinChgPct { get; set; }

    public short Hold { get; set; }

    public int? ImportLogKey { get; set; }

    public int? NationalAcctLevelKey { get; set; }

    public short PmtByNationalAcctParent { get; set; }

    public int PrimaryAddrKey { get; set; }

    public int? PrimaryCntctKey { get; set; }

    public short PrintDunnMsg { get; set; }

    public decimal? ReqCreditLimit { get; set; }

    public short ReqPo { get; set; }

    public decimal? RetntPct { get; set; }

    public int? SalesSourceKey { get; set; }

    public short ShipPriority { get; set; }

    public short Status { get; set; }

    public string? StdIndusCodeId { get; set; }

    public int? StmtCycleKey { get; set; }

    public int? StmtFormKey { get; set; }

    public decimal? TradeDiscPct { get; set; }

    public int UpdateCounter { get; set; }

    public DateTime? UpdateDate { get; set; }

    public string? UpdateUserId { get; set; }

    public string? UserFld1 { get; set; }

    public string? UserFld2 { get; set; }

    public string? UserFld3 { get; set; }

    public string? UserFld4 { get; set; }

    public int? VendKey { get; set; }
}
