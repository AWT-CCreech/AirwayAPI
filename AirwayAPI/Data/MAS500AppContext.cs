using AirwayAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Data;

public partial class MAS500AppContext : DbContext
{
    public MAS500AppContext()
    {
    }

    public MAS500AppContext(DbContextOptions<MAS500AppContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TarCustomer> TarCustomers { get; set; }

    public virtual DbSet<TarInvoice> TarInvoices { get; set; }

    public virtual DbSet<TsoSalesOrder> TsoSalesOrders { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TarCustomer>(entity =>
        {
            entity.HasKey(e => e.CustKey).HasName("PK__tarCusto__8CCAE2DB8E5CC056");

            entity.ToTable("tarCustomer", tb =>
                {
                    tb.HasTrigger("tD_tarCustomer");
                    tb.HasTrigger("tI_tarCustomer");
                    tb.HasTrigger("tR_tarCustomer_AppAudit");
                    tb.HasTrigger("tR_tarCustomer_DBAudit");
                    tb.HasTrigger("tU_tarCustomer");
                });

            entity.HasIndex(e => new { e.CompanyId, e.CustId }, "XAK1tarCustomer").IsUnique();

            entity.HasIndex(e => e.SalesSourceKey, "XIF102tarCustomer");

            entity.HasIndex(e => e.DfltItemKey, "XIF239tarCustomer");

            entity.HasIndex(e => e.NationalAcctLevelKey, "XIF255tarCustomer");

            entity.HasIndex(e => e.CustClassKey, "XIF44tarCustomer");

            entity.HasIndex(e => e.DfltSalesAcctKey, "XIF46tarCustomer");

            entity.HasIndex(e => e.VendKey, "XIF47tarCustomer");

            entity.HasIndex(e => e.StmtCycleKey, "XIF49tarCustomer");

            entity.HasIndex(e => e.PrimaryAddrKey, "XIF50tarCustomer");

            entity.HasIndex(e => e.DfltBillToAddrKey, "XIF51tarCustomer");

            entity.HasIndex(e => e.DfltShipToAddrKey, "XIF52tarCustomer");

            entity.HasIndex(e => e.StmtFormKey, "XIF53tarCustomer");

            entity.HasIndex(e => e.PrimaryCntctKey, "XIF55tarCustomer");

            entity.HasIndex(e => e.CurrExchSchdKey, "XIF56tarCustomer");

            entity.HasIndex(e => e.DfltSalesReturnAcctKey, "XIF57tarCustomer");

            entity.Property(e => e.CustKey).ValueGeneratedNever();
            entity.Property(e => e.Abano)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("ABANo");
            entity.Property(e => e.BillingType).HasDefaultValue((short)1);
            entity.Property(e => e.CompanyId)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("CompanyID");
            entity.Property(e => e.CreateDate).HasColumnType("datetime");
            entity.Property(e => e.CreateUserId)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("CreateUserID");
            entity.Property(e => e.CreditLimit).HasColumnType("decimal(15, 3)");
            entity.Property(e => e.CrmcustId)
                .HasMaxLength(32)
                .IsUnicode(false)
                .HasColumnName("CRMCustID");
            entity.Property(e => e.CustId)
                .HasMaxLength(12)
                .IsUnicode(false)
                .HasColumnName("CustID");
            entity.Property(e => e.CustName)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.CustRefNo)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.DateEstab).HasColumnType("datetime");
            entity.Property(e => e.DfltMaxUpCharge).HasColumnType("decimal(15, 3)");
            entity.Property(e => e.FinChgFlatAmt).HasColumnType("decimal(15, 3)");
            entity.Property(e => e.FinChgPct)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 4)");
            entity.Property(e => e.ReqCreditLimit)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(15, 3)");
            entity.Property(e => e.ReqPo).HasColumnName("ReqPO");
            entity.Property(e => e.RetntPct)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 4)");
            entity.Property(e => e.ShipPriority).HasDefaultValue((short)3);
            entity.Property(e => e.Status).HasDefaultValue((short)1);
            entity.Property(e => e.StdIndusCodeId)
                .HasMaxLength(7)
                .IsUnicode(false)
                .HasColumnName("StdIndusCodeID");
            entity.Property(e => e.TradeDiscPct)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 4)");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateUserId)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("UpdateUserID");
            entity.Property(e => e.UserFld1)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.UserFld2)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.UserFld3)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.UserFld4)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TarInvoice>(entity =>
        {
            entity.HasKey(e => e.InvcKey).HasName("PK__tarInvoi__12691F6089BE8C75");

            entity.ToTable("tarInvoice", tb =>
                {
                    tb.HasTrigger("tD_tarInvoice");
                    tb.HasTrigger("tI_tarInvoice");
                    tb.HasTrigger("tR_tarInvoice_DBAudit");
                    tb.HasTrigger("tU_tarInvoice");
                });

            entity.HasIndex(e => new { e.CompanyId, e.TranId }, "XAK1tarInvoice").IsUnique();

            entity.HasIndex(e => new { e.CompanyId, e.Status, e.CustKey }, "XIE1tarInvoice");

            entity.HasIndex(e => new { e.CustKey, e.TranDate }, "XIE2tarInvoice");

            entity.HasIndex(e => e.StaxTranKey, "XIE3tarInvoice");

            entity.HasIndex(e => e.Fobkey, "XIF136tarInvoice");

            entity.HasIndex(e => e.SourceModule, "XIF139tarInvoice");

            entity.HasIndex(e => e.CustClassKey, "XIF140tarInvoice");

            entity.HasIndex(e => e.BatchKey, "XIF143tarInvoice");

            entity.HasIndex(e => e.RecurInvoiceKey, "XIF144tarInvoice");

            entity.HasIndex(e => e.ShipToAddrKey, "XIF145tarInvoice");

            entity.HasIndex(e => e.BillToAddrKey, "XIF146tarInvoice");

            entity.HasIndex(e => e.CurrId, "XIF148tarInvoice");

            entity.HasIndex(e => e.ReasonCodeKey, "XIF149tarInvoice");

            entity.HasIndex(e => e.ShipMethKey, "XIF150tarInvoice");

            entity.HasIndex(e => e.ConfirmToCntctKey, "XIF151tarInvoice");

            entity.HasIndex(e => e.BillToCustAddrKey, "XIF153tarInvoice");

            entity.HasIndex(e => e.ShipToCustAddrKey, "XIF154tarInvoice");

            entity.HasIndex(e => e.TranType, "XIF155tarInvoice");

            entity.HasIndex(e => e.PrimarySperKey, "XIF156tarInvoice");

            entity.HasIndex(e => e.InvcFormKey, "XIF157tarInvoice");

            entity.HasIndex(e => e.CommPlanKey, "XIF158tarInvoice");

            entity.HasIndex(e => e.PmtTermsKey, "XIF159tarInvoice");

            entity.HasIndex(e => e.CurrExchSchdKey, "XIF160tarInvoice");

            entity.HasIndex(e => e.VoucherKey, "XIF161tarInvoice");

            entity.Property(e => e.InvcKey).ValueGeneratedNever();
            entity.Property(e => e.AuthOvrdAmt).HasColumnType("decimal(15, 3)");
            entity.Property(e => e.Balance).HasColumnType("decimal(15, 3)");
            entity.Property(e => e.BalanceHc)
                .HasColumnType("decimal(15, 3)")
                .HasColumnName("BalanceHC");
            entity.Property(e => e.ClosingPostDate).HasColumnType("datetime");
            entity.Property(e => e.ClosingTranDate).HasColumnType("datetime");
            entity.Property(e => e.CompanyId)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("CompanyID");
            entity.Property(e => e.CostOfSales).HasColumnType("decimal(15, 3)");
            entity.Property(e => e.CreateDate).HasColumnType("datetime");
            entity.Property(e => e.CreateUserId)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("CreateUserID");
            entity.Property(e => e.CurrExchRate).HasDefaultValue(1.0);
            entity.Property(e => e.CurrId)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("CurrID");
            entity.Property(e => e.CustPono)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("CustPONo");
            entity.Property(e => e.DiscAmt).HasColumnType("decimal(15, 3)");
            entity.Property(e => e.DiscDate).HasColumnType("datetime");
            entity.Property(e => e.DiscTakenAmt).HasColumnType("decimal(15, 3)");
            entity.Property(e => e.DueDate).HasColumnType("datetime");
            entity.Property(e => e.Fobkey).HasColumnName("FOBKey");
            entity.Property(e => e.HandlAmt).HasColumnType("decimal(15, 3)");
            entity.Property(e => e.NextApplEntryNo).HasDefaultValue(1);
            entity.Property(e => e.PostDate).HasColumnType("datetime");
            entity.Property(e => e.Printed).HasDefaultValue((short)1);
            entity.Property(e => e.RetntAmt).HasColumnType("decimal(15, 3)");
            entity.Property(e => e.RetntPct)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 4)");
            entity.Property(e => e.SalesAmt).HasColumnType("decimal(15, 3)");
            entity.Property(e => e.ShipAmt).HasColumnType("decimal(15, 3)");
            entity.Property(e => e.SourceModule).HasDefaultValue((short)5);
            entity.Property(e => e.Status).HasDefaultValue((short)1);
            entity.Property(e => e.StaxAmt)
                .HasColumnType("decimal(15, 3)")
                .HasColumnName("STaxAmt");
            entity.Property(e => e.StaxCalc)
                .HasDefaultValue((short)1)
                .HasColumnName("STaxCalc");
            entity.Property(e => e.StaxTranKey).HasColumnName("STaxTranKey");
            entity.Property(e => e.TradeDiscAmt).HasColumnType("decimal(15, 3)");
            entity.Property(e => e.TranAmt).HasColumnType("decimal(15, 3)");
            entity.Property(e => e.TranAmtHc)
                .HasColumnType("decimal(15, 3)")
                .HasColumnName("TranAmtHC");
            entity.Property(e => e.TranCmnt)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TranDate).HasColumnType("datetime");
            entity.Property(e => e.TranId)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasColumnName("TranID");
            entity.Property(e => e.TranNo)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateUserId)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("UpdateUserID");
        });

        modelBuilder.Entity<TsoSalesOrder>(entity =>
        {
            entity.HasKey(e => e.Sokey).HasName("PK__tsoSales__83F931CE092D58F9");

            entity.ToTable("tsoSalesOrder", tb =>
                {
                    tb.HasTrigger("SalesOrderCreditHoldEmail");
                    tb.HasTrigger("tD_tsoSalesOrder");
                    tb.HasTrigger("tI_tsoSalesOrder");
                    tb.HasTrigger("tR_tsoSalesOrder_DBAudit");
                    tb.HasTrigger("tU_tsoSalesOrder");
                });

            entity.HasIndex(e => new { e.CompanyId, e.TranType, e.TranNoRel }, "XAK1tsoSalesOrder").IsUnique();

            entity.HasIndex(e => new { e.BlnktSokey, e.CreateDate }, "XIE1tsoSalesOrder");

            entity.HasIndex(e => new { e.CompanyId, e.TranType, e.TranNoRelChngOrd }, "XIE2tsoSalesOrder");

            entity.HasIndex(e => e.Status, "XIE3tsoSalesOrder");

            entity.HasIndex(e => e.StaxTranKey, "XIE4tsoSalesOrder");

            entity.HasIndex(e => e.QuoteFormKey, "XIF1216tsoSalesOrder");

            entity.HasIndex(e => e.CreditAuthUserId, "XIF2868tsoSalesOrder");

            entity.HasIndex(e => e.CustQuoteKey, "XIF872tsoSalesOrder");

            entity.HasIndex(e => e.RecurSokey, "XIF873tsoSalesOrder");

            entity.HasIndex(e => e.CustKey, "XIF876tsoSalesOrder");

            entity.HasIndex(e => e.PrimarySperKey, "XIF877tsoSalesOrder");

            entity.HasIndex(e => e.CntctKey, "XIF879tsoSalesOrder");

            entity.HasIndex(e => e.CustClassKey, "XIF880tsoSalesOrder");

            entity.HasIndex(e => e.CurrId, "XIF881tsoSalesOrder");

            entity.HasIndex(e => e.CurrExchSchdKey, "XIF882tsoSalesOrder");

            entity.HasIndex(e => e.DfltAcctRefKey, "XIF883tsoSalesOrder");

            entity.HasIndex(e => e.DfltShipZoneKey, "XIF884tsoSalesOrder");

            entity.HasIndex(e => e.DfltShipMethKey, "XIF885tsoSalesOrder");

            entity.HasIndex(e => e.DfltShipToAddrKey, "XIF886tsoSalesOrder");

            entity.HasIndex(e => e.DfltShipToCaddrKey, "XIF887tsoSalesOrder");

            entity.HasIndex(e => e.DfltWhseKey, "XIF888tsoSalesOrder");

            entity.HasIndex(e => e.PmtTermsKey, "XIF892tsoSalesOrder");

            entity.HasIndex(e => e.SoackFormKey, "XIF893tsoSalesOrder");

            entity.HasIndex(e => e.BillToCustAddrKey, "XIF894tsoSalesOrder");

            entity.HasIndex(e => e.TranType, "XIF896tsoSalesOrder");

            entity.HasIndex(e => e.SalesSourceKey, "XIF897tsoSalesOrder");

            entity.HasIndex(e => e.DfltPurchVaddrKey, "XIF900tsoSalesOrder");

            entity.HasIndex(e => e.DfltVendKey, "XIF901tsoSalesOrder");

            entity.HasIndex(e => e.BillToAddrKey, "XIF902tsoSalesOrder");

            entity.Property(e => e.Sokey)
                .ValueGeneratedNever()
                .HasColumnName("SOKey");
            entity.Property(e => e.AckDate).HasColumnType("datetime");
            entity.Property(e => e.BlnktSokey).HasColumnName("BlnktSOKey");
            entity.Property(e => e.CccustCode)
                .HasMaxLength(17)
                .IsUnicode(false)
                .HasColumnName("CCCustCode");
            entity.Property(e => e.ChngOrdDate).HasColumnType("datetime");
            entity.Property(e => e.ChngReason)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.ChngUserId)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("ChngUserID");
            entity.Property(e => e.CloseDate).HasColumnType("datetime");
            entity.Property(e => e.CompanyId)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("CompanyID");
            entity.Property(e => e.ConfirmNo)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CreateDate).HasColumnType("datetime");
            entity.Property(e => e.CreateUserId)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("CreateUserID");
            entity.Property(e => e.CreditApprovedAmt).HasColumnType("decimal(15, 3)");
            entity.Property(e => e.CreditAuthUserId)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("CreditAuthUserID");
            entity.Property(e => e.CrmopportunityId)
                .HasMaxLength(32)
                .IsUnicode(false)
                .HasColumnName("CRMOpportunityID");
            entity.Property(e => e.CurrExchRate).HasDefaultValue(1.0);
            entity.Property(e => e.CurrId)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("CurrID");
            entity.Property(e => e.CustPono)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("CustPONo");
            entity.Property(e => e.DfltCreatePo).HasColumnName("DfltCreatePO");
            entity.Property(e => e.DfltDeliveryMeth).HasDefaultValue((short)1);
            entity.Property(e => e.DfltFobkey).HasColumnName("DfltFOBKey");
            entity.Property(e => e.DfltPromDate).HasColumnType("datetime");
            entity.Property(e => e.DfltPurchVaddrKey).HasColumnName("DfltPurchVAddrKey");
            entity.Property(e => e.DfltRequestDate).HasColumnType("datetime");
            entity.Property(e => e.DfltShipDate).HasColumnType("datetime");
            entity.Property(e => e.DfltShipPriority).HasDefaultValue((short)3);
            entity.Property(e => e.DfltShipToCaddrKey).HasColumnName("DfltShipToCAddrKey");
            entity.Property(e => e.DutyAmount).HasColumnType("decimal(15, 3)");
            entity.Property(e => e.Expiration).HasColumnType("datetime");
            entity.Property(e => e.FreightAmt).HasColumnType("decimal(15, 3)");
            entity.Property(e => e.FreightMethod).HasDefaultValue((short)2);
            entity.Property(e => e.HoldReason)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.NationalTaxAmount).HasColumnType("decimal(15, 3)");
            entity.Property(e => e.NextLineNo).HasDefaultValue(1);
            entity.Property(e => e.OpenAmt).HasColumnType("decimal(15, 3)");
            entity.Property(e => e.OpenAmtHc)
                .HasColumnType("decimal(15, 3)")
                .HasColumnName("OpenAmtHC");
            entity.Property(e => e.RecurSokey).HasColumnName("RecurSOKey");
            entity.Property(e => e.RequireSoack).HasColumnName("RequireSOAck");
            entity.Property(e => e.SalesAmt).HasColumnType("decimal(15, 3)");
            entity.Property(e => e.SalesAmtHc)
                .HasColumnType("decimal(15, 3)")
                .HasColumnName("SalesAmtHC");
            entity.Property(e => e.SoackFormKey).HasColumnName("SOAckFormKey");
            entity.Property(e => e.StaxAmt)
                .HasColumnType("decimal(15, 3)")
                .HasColumnName("STaxAmt");
            entity.Property(e => e.StaxCalc)
                .HasDefaultValue((short)1)
                .HasColumnName("STaxCalc");
            entity.Property(e => e.StaxTranKey).HasColumnName("STaxTranKey");
            entity.Property(e => e.TradeDiscAmt).HasColumnType("decimal(15, 3)");
            entity.Property(e => e.TranAmt).HasColumnType("decimal(15, 3)");
            entity.Property(e => e.TranAmtHc)
                .HasColumnType("decimal(15, 3)")
                .HasColumnName("TranAmtHC");
            entity.Property(e => e.TranCmnt)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TranDate).HasColumnType("datetime");
            entity.Property(e => e.TranId)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasColumnName("TranID");
            entity.Property(e => e.TranNo)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.TranNoChngOrd)
                .HasMaxLength(14)
                .IsUnicode(false);
            entity.Property(e => e.TranNoRel)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.TranNoRelChngOrd)
                .HasMaxLength(19)
                .IsUnicode(false);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateUserId)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("UpdateUserID");
            entity.Property(e => e.UserFld1)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.UserFld2)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.UserFld3)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.UserFld4)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.VatinvoiceNumber)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("VATInvoiceNumber");
            entity.Property(e => e.Vatnumber)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasColumnName("VATNumber");
            entity.Property(e => e.VattaxAmount)
                .HasColumnType("decimal(15, 3)")
                .HasColumnName("VATTaxAmount");
            entity.Property(e => e.VattaxRate)
                .HasColumnType("decimal(12, 6)")
                .HasColumnName("VATTaxRate");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
