using System;
using System.Collections.Generic;

namespace AirwayAPI.Models;

public partial class TsoSalesOrder
{
    public int Sokey { get; set; }

    public DateTime? AckDate { get; set; }

    public int? BillToAddrKey { get; set; }

    public int BillToCustAddrKey { get; set; }

    public short BlnktRelNo { get; set; }

    public int? BlnktSokey { get; set; }

    public string? CccustCode { get; set; }

    public DateTime? ChngOrdDate { get; set; }

    public short ChngOrdNo { get; set; }

    public string? ChngReason { get; set; }

    public string? ChngUserId { get; set; }

    public DateTime? CloseDate { get; set; }

    public int? CntctKey { get; set; }

    public string CompanyId { get; set; } = null!;

    public string? ConfirmNo { get; set; }

    public DateTime? CreateDate { get; set; }

    public short CreateType { get; set; }

    public string? CreateUserId { get; set; }

    public decimal CreditApprovedAmt { get; set; }

    public string? CreditAuthUserId { get; set; }

    public short CrHold { get; set; }

    public string? CrmopportunityId { get; set; }

    public double CurrExchRate { get; set; }

    public int? CurrExchSchdKey { get; set; }

    public string CurrId { get; set; } = null!;

    public int CustClassKey { get; set; }

    public int CustKey { get; set; }

    public string? CustPono { get; set; }

    public int? CustQuoteKey { get; set; }

    public int? DfltAcctRefKey { get; set; }

    public int? DfltCommPlanKey { get; set; }

    public short DfltCreatePo { get; set; }

    public short DfltDeliveryMeth { get; set; }

    public int? DfltFobkey { get; set; }

    public DateTime? DfltPromDate { get; set; }

    public int? DfltPurchVaddrKey { get; set; }

    public DateTime? DfltRequestDate { get; set; }

    public DateTime? DfltShipDate { get; set; }

    public int? DfltShipMethKey { get; set; }

    public short? DfltShipPriority { get; set; }

    public int? DfltShipToAddrKey { get; set; }

    public int? DfltShipToCaddrKey { get; set; }

    public int? DfltShipZoneKey { get; set; }

    public int? DfltVendKey { get; set; }

    public int? DfltWhseKey { get; set; }

    public decimal? DutyAmount { get; set; }

    public DateTime? Expiration { get; set; }

    public short FixedCurrExchRate { get; set; }

    public decimal FreightAmt { get; set; }

    public short FreightMethod { get; set; }

    public short Hold { get; set; }

    public string? HoldReason { get; set; }

    public int? ImportLogKey { get; set; }

    public decimal? NationalTaxAmount { get; set; }

    public int NextLineNo { get; set; }

    public decimal OpenAmt { get; set; }

    public decimal OpenAmtHc { get; set; }

    public int? PmtTermsKey { get; set; }

    public int? PrimarySperKey { get; set; }

    public int? QuoteFormKey { get; set; }

    public int? RecurSokey { get; set; }

    public short RequireSoack { get; set; }

    public decimal SalesAmt { get; set; }

    public decimal SalesAmtHc { get; set; }

    public int? SalesSourceKey { get; set; }

    public int? SoackFormKey { get; set; }

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

    public string TranNoChngOrd { get; set; } = null!;

    public string TranNoRel { get; set; } = null!;

    public string TranNoRelChngOrd { get; set; } = null!;

    public int TranType { get; set; }

    public int UpdateCounter { get; set; }

    public DateTime? UpdateDate { get; set; }

    public string? UpdateUserId { get; set; }

    public string? UserFld1 { get; set; }

    public string? UserFld2 { get; set; }

    public string? UserFld3 { get; set; }

    public string? UserFld4 { get; set; }

    public string? VatinvoiceNumber { get; set; }

    public string? Vatnumber { get; set; }

    public decimal? VattaxAmount { get; set; }

    public decimal? VattaxRate { get; set; }
}
