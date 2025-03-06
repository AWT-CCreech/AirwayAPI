using System;
using System.Collections.Generic;
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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=AWTSQL19;Database=mas500_app;Trusted_Connection=True;TrustServerCertificate=True;");

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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
