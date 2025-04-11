using System;
using System.Collections.Generic;
using AirwayAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Data;

public partial class eHelpDeskContext : DbContext
{
    public eHelpDeskContext()
    {
    }

    public eHelpDeskContext(DbContextOptions<eHelpDeskContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BuyingOppDetail> BuyingOppDetails { get; set; }

    public virtual DbSet<BuyingOppEvent> BuyingOppEvents { get; set; }

    public virtual DbSet<CamActivity> CamActivities { get; set; }

    public virtual DbSet<CamCannedEmail> CamCannedEmails { get; set; }

    public virtual DbSet<CamContact> CamContacts { get; set; }

    public virtual DbSet<CamFieldsList> CamFieldsLists { get; set; }

    public virtual DbSet<CompetitorCall> CompetitorCalls { get; set; }

    public virtual DbSet<CsQtSoToInvNo> CsQtSoToInvNos { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<EquipmentRequest> EquipmentRequests { get; set; }

    public virtual DbSet<EquipmentSnapshot> EquipmentSnapshots { get; set; }

    public virtual DbSet<MassMailHistory> MassMailHistories { get; set; }

    public virtual DbSet<MassMailer> MassMailers { get; set; }

    public virtual DbSet<MasterSearchQuery> MasterSearchQueries { get; set; }

    public virtual DbSet<OpenSoreport> OpenSoreports { get; set; }

    public virtual DbSet<PhoneNumber> PhoneNumbers { get; set; }

    public virtual DbSet<PortalItem> PortalItems { get; set; }

    public virtual DbSet<PortalMenu> PortalMenus { get; set; }

    public virtual DbSet<PortalRoute> PortalRoutes { get; set; }

    public virtual DbSet<PortalUserConfig> PortalUserConfigs { get; set; }

    public virtual DbSet<PortalUserFavorite> PortalUserFavorites { get; set; }

    public virtual DbSet<PortalWorkspace> PortalWorkspaces { get; set; }

    public virtual DbSet<QtQuote> QtQuotes { get; set; }

    public virtual DbSet<QtQuoteDetail> QtQuoteDetails { get; set; }

    public virtual DbSet<QtSalesOrder> QtSalesOrders { get; set; }

    public virtual DbSet<QtSalesOrderDetail> QtSalesOrderDetails { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<RequestEvent> RequestEvents { get; set; }

    public virtual DbSet<RequestPo> RequestPos { get; set; }

    public virtual DbSet<RequestPohistory> RequestPohistories { get; set; }

    public virtual DbSet<ScanHistory> ScanHistories { get; set; }

    public virtual DbSet<ScanTestLab> ScanTestLabs { get; set; }

    public virtual DbSet<SellOpCompetitor> SellOpCompetitors { get; set; }

    public virtual DbSet<ShorelineUser> ShorelineUsers { get; set; }

    public virtual DbSet<SimpleList> SimpleLists { get; set; }

    public virtual DbSet<SoEmailNotification> SoEmailNotifications { get; set; }

    public virtual DbSet<TcEntry> TcEntries { get; set; }

    public virtual DbSet<TcHistory> TcHistories { get; set; }

    public virtual DbSet<TcHoliday> TcHolidays { get; set; }

    public virtual DbSet<TcPayPeriod> TcPayPeriods { get; set; }

    public virtual DbSet<TcPto> TcPtos { get; set; }

    public virtual DbSet<TrkAdjustment> TrkAdjustments { get; set; }

    public virtual DbSet<TrkCompany> TrkCompanies { get; set; }

    public virtual DbSet<TrkInventory> TrkInventories { get; set; }

    public virtual DbSet<TrkPolog> TrkPologs { get; set; }

    public virtual DbSet<TrkPonote> TrkPonotes { get; set; }

    public virtual DbSet<TrkRwImItem> TrkRwImItems { get; set; }

    public virtual DbSet<TrkRwPoheader> TrkRwPoheaders { get; set; }

    public virtual DbSet<TrkRwSodetail> TrkRwSodetails { get; set; }

    public virtual DbSet<TrkRwSoheader> TrkRwSoheaders { get; set; }

    public virtual DbSet<TrkRwvendor> TrkRwvendors { get; set; }

    public virtual DbSet<TrkSoldItemHistory> TrkSoldItemHistories { get; set; }

    public virtual DbSet<TrkSonote> TrkSonotes { get; set; }

    public virtual DbSet<TrkUnshippedByItemNo> TrkUnshippedByItemNos { get; set; }

    public virtual DbSet<TrkUnshippedBySo> TrkUnshippedBySos { get; set; }

    public virtual DbSet<TrkUnshippedValue> TrkUnshippedValues { get; set; }

    public virtual DbSet<TrkUsage> TrkUsages { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserPortalMenu> UserPortalMenus { get; set; }

    public virtual DbSet<UserPref> UserPrefs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=AWTSQL19;Database=eHelpDesk;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BuyingOppDetail>(entity =>
        {
            entity.HasKey(e => e.DetailId);

            entity.ToTable("BuyingOppDetail");

            entity.HasIndex(e => e.EventId, "IX_BuyingOppDetail_EventID").HasFillFactor(80);

            entity.HasIndex(e => e.PartNum, "IX_BuyingOppDetail_PartNum").HasFillFactor(80);

            entity.Property(e => e.DetailId).HasColumnName("DetailID");
            entity.Property(e => e.AltPartNum)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.AmsnoozeDate)
                .HasDefaultValue(new DateTime(1990, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified))
                .HasColumnType("datetime")
                .HasColumnName("AMsnoozeDate");
            entity.Property(e => e.Amsnoozed)
                .HasDefaultValue(0)
                .HasColumnName("AMsnoozed");
            entity.Property(e => e.AskingPrice)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasDefaultValueSql("((0))");
            entity.Property(e => e.BidPrice).HasDefaultValue(0.0);
            entity.Property(e => e.CompanyLostTo)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasDefaultValue("n/a");
            entity.Property(e => e.EnteredBy).HasDefaultValue(0);
            entity.Property(e => e.EntryDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EquipmentType)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.ForecastRequired).HasDefaultValue((byte)1);
            entity.Property(e => e.Manufacturer)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedBy).HasDefaultValue(0);
            entity.Property(e => e.ModifiedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Notes)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.PartDesc)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.PartNum)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PriceLostTo)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasDefaultValue("n/a");
            entity.Property(e => e.Quantity).HasDefaultValue(0);
            entity.Property(e => e.StatusCash)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.StatusConsignment)
                .HasMaxLength(25)
                .IsUnicode(false);
        });

        modelBuilder.Entity<BuyingOppEvent>(entity =>
        {
            entity.HasKey(e => e.EventId);

            entity.ToTable("BuyingOppEvent");

            entity.HasIndex(e => new { e.ContactId, e.StatusCash }, "IX_BuyingOppEvent_Contact_StatusCash").HasFillFactor(80);

            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.BidDueDate).HasColumnType("datetime");
            entity.Property(e => e.BidProposed)
                .HasMaxLength(50)
                .HasDefaultValue(" ");
            entity.Property(e => e.BuyingOpp)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("Pending");
            entity.Property(e => e.CashCompanyLostTo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CashPriceLostTo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CompletedSites)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ConsignCompanyLostTo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ConsignPriceLostTo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ContactId).HasColumnName("ContactID");
            entity.Property(e => e.DateAvailable).HasColumnType("datetime");
            entity.Property(e => e.EnteredBy).HasDefaultValue(0);
            entity.Property(e => e.EntryDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EquipmentCondition)
                .HasMaxLength(50)
                .HasDefaultValue(" ");
            entity.Property(e => e.EquipmentType)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.Frequency)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.InitialCommentary)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.Manufacturer)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedBy).HasDefaultValue(0);
            entity.Property(e => e.ModifiedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PartialBuy)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.Platform)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.Rating)
                .HasMaxLength(2)
                .IsUnicode(false)
                .HasDefaultValue("NR");
            entity.Property(e => e.ResearchOwner).HasDefaultValue(0);
            entity.Property(e => e.RetailEstimateValue).HasDefaultValue(0);
            entity.Property(e => e.StatusCash)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasDefaultValue("Pending");
            entity.Property(e => e.StatusConsignment)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasDefaultValue("Pending");
            entity.Property(e => e.Technology)
                .HasMaxLength(25)
                .IsUnicode(false);
        });

        modelBuilder.Entity<CamActivity>(entity =>
        {
            entity.ToTable("camActivities");

            entity.HasIndex(e => new { e.CompleteDate, e.RepeatId }, "IX_camActivitiesCompleteDate").HasFillFactor(80);

            entity.HasIndex(e => new { e.ContactId, e.CompletedBy }, "IX_camActivitiesContactIDCompletedBy").HasFillFactor(80);

            entity.HasIndex(e => e.ContactId, "IX_camActivitiesContactId").HasFillFactor(80);

            entity.HasIndex(e => e.ActivityDate, "IX_camActivitiesDate").HasFillFactor(80);

            entity.HasIndex(e => new { e.ActivityOwner, e.ProjectCode, e.ActivityDate }, "IX_camActivities_ActOwnerProjCode_ActDate_More").HasFillFactor(80);

            entity.HasIndex(e => new { e.CompletedBy, e.Reminder }, "IX_camActivities_CompByReminder").HasFillFactor(80);

            entity.HasIndex(e => new { e.ContactId, e.CompletedBy }, "IX_camActivities_ContactIDCompBy_andMore").HasFillFactor(80);

            entity.Property(e => e.ActivityDate).HasColumnType("datetime");
            entity.Property(e => e.ActivityOwner).HasMaxLength(20);
            entity.Property(e => e.ActivityTime).HasColumnType("datetime");
            entity.Property(e => e.ActivityType).HasMaxLength(50);
            entity.Property(e => e.Attachments)
                .HasMaxLength(1024)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.CompleteDate).HasColumnType("datetime");
            entity.Property(e => e.CompletedBy).HasMaxLength(20);
            entity.Property(e => e.EnteredBy).HasMaxLength(20);
            entity.Property(e => e.EntryDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsFullDay)
                .HasDefaultValue((byte)0)
                .HasColumnName("isFullDay");
            entity.Property(e => e.IsPrivate)
                .HasDefaultValue((byte)0)
                .HasColumnName("isPrivate");
            entity.Property(e => e.LeftMsg).HasDefaultValue((byte)0);
            entity.Property(e => e.LinkRecId).HasColumnName("LinkRecID");
            entity.Property(e => e.LinkRecType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Members)
                .HasMaxLength(2048)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.MembersHist)
                .HasMaxLength(2048)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.ModifiedBy).HasMaxLength(20);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.Notes).HasColumnType("text");
            entity.Property(e => e.Ogm)
                .HasDefaultValue((byte)0)
                .HasColumnName("OGM");
            entity.Property(e => e.ProjectCode).HasMaxLength(12);
            entity.Property(e => e.RemindBefore).HasColumnType("datetime");
            entity.Property(e => e.RemindBeforeInMins).HasDefaultValue(15);
            entity.Property(e => e.Reminder).HasDefaultValue((byte)0);
            entity.Property(e => e.RepeatId)
                .HasDefaultValue(0)
                .HasColumnName("RepeatID");
            entity.Property(e => e.RepeatOrgId)
                .HasDefaultValue(0)
                .HasColumnName("RepeatOrgID");
        });

        modelBuilder.Entity<CamCannedEmail>(entity =>
        {
            entity.ToTable("camCannedEmails");

            entity.Property(e => e.Active).HasDefaultValue(false);
            entity.Property(e => e.DefaultMsg).HasDefaultValue(false);
            entity.Property(e => e.EmailBody).HasColumnType("text");
            entity.Property(e => e.EmailDesc)
                .HasMaxLength(25)
                .HasDefaultValue("");
            entity.Property(e => e.EmailSubject).HasMaxLength(50);
            entity.Property(e => e.EmailType).HasMaxLength(25);
            entity.Property(e => e.EnteredBy).HasMaxLength(25);
            entity.Property(e => e.EntryDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ModifiedBy).HasMaxLength(50);
            entity.Property(e => e.ModifiedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<CamContact>(entity =>
        {
            entity.ToTable("camContacts");

            entity.HasIndex(e => e.Company, "IX_camContactCompany").HasFillFactor(80);

            entity.HasIndex(e => e.Contact, "IX_camContactsContact").HasFillFactor(80);

            entity.HasIndex(e => e.Email, "IX_camContactsEmail").HasFillFactor(80);

            entity.HasIndex(e => e.AccountMgr, "IX_camContacts_AcctMgrAcctNum").HasFillFactor(80);

            entity.HasIndex(e => new { e.ActiveStatus, e.ContactType }, "IX_camContacts_ActiveType").HasFillFactor(80);

            entity.HasIndex(e => new { e.Company, e.ActiveStatus }, "IX_camContacts_CompanyActiveStatus").HasFillFactor(80);

            entity.HasIndex(e => new { e.EntryDate, e.EnteredBy }, "IX_camContacts_EnteredBy_EntryDate").HasFillFactor(80);

            entity.HasIndex(e => e.RwaccountNum, "IX_camContacts_RWAccountNum").HasFillFactor(80);

            entity.HasIndex(e => e.RwaccountNum, "IX_camContacts_RwAcctNo").HasFillFactor(80);

            entity.HasIndex(e => e.SalesTeam, "IX_camContacts_Team_Co").HasFillFactor(80);

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.AccountCoord).HasMaxLength(20);
            entity.Property(e => e.AccountExec).HasMaxLength(20);
            entity.Property(e => e.AccountMgr).HasMaxLength(20);
            entity.Property(e => e.AccountOther).HasMaxLength(20);
            entity.Property(e => e.AcctReps).HasMaxLength(512);
            entity.Property(e => e.Address1).HasMaxLength(50);
            entity.Property(e => e.Address2).HasMaxLength(50);
            entity.Property(e => e.Address3).HasMaxLength(50);
            entity.Property(e => e.Anccard)
                .HasDefaultValue(false)
                .HasColumnName("ANCCard");
            entity.Property(e => e.Assistant).HasMaxLength(30);
            entity.Property(e => e.BirthDate).HasColumnType("datetime");
            entity.Property(e => e.Birthday).HasMaxLength(50);
            entity.Property(e => e.Btscount)
                .HasDefaultValue(0)
                .HasColumnName("BTSCount");
            entity.Property(e => e.BtstimeStamp)
                .HasColumnType("datetime")
                .HasColumnName("BTSTimeStamp");
            entity.Property(e => e.City).HasMaxLength(30);
            entity.Property(e => e.ClauseId)
                .HasDefaultValue(0)
                .HasColumnName("ClauseID");
            entity.Property(e => e.Company).HasMaxLength(75);
            entity.Property(e => e.CompanyId)
                .HasMaxLength(10)
                .HasDefaultValue("AIR")
                .HasColumnName("CompanyID");
            entity.Property(e => e.Competitor).HasDefaultValue(false);
            entity.Property(e => e.Contact).HasMaxLength(50);
            entity.Property(e => e.ContactType).HasMaxLength(30);
            entity.Property(e => e.Country).HasMaxLength(30);
            entity.Property(e => e.Dear).HasMaxLength(20);
            entity.Property(e => e.EFlyer).HasColumnName("eFlyer");
            entity.Property(e => e.Email).HasMaxLength(128);
            entity.Property(e => e.EnteredBy).HasMaxLength(20);
            entity.Property(e => e.EntryDate).HasColumnType("datetime");
            entity.Property(e => e.Extension).HasMaxLength(10);
            entity.Property(e => e.Fax).HasMaxLength(30);
            entity.Property(e => e.Fnecard)
                .HasDefaultValue(false)
                .HasColumnName("FNECard");
            entity.Property(e => e.GeneralInfo)
                .HasDefaultValue("")
                .HasColumnType("ntext");
            entity.Property(e => e.Gift1).HasDefaultValue(false);
            entity.Property(e => e.Gift2).HasDefaultValue(false);
            entity.Property(e => e.GmaccountNum)
                .HasMaxLength(20)
                .HasColumnName("GMAccountNum");
            entity.Property(e => e.HolidayGift).HasMaxLength(255);
            entity.Property(e => e.Interests).HasMaxLength(255);
            entity.Property(e => e.Kids).HasMaxLength(128);
            entity.Property(e => e.Label1).HasMaxLength(30);
            entity.Property(e => e.Label2).HasMaxLength(30);
            entity.Property(e => e.Label3).HasMaxLength(30);
            entity.Property(e => e.Label4).HasMaxLength(30);
            entity.Property(e => e.Lastname).HasMaxLength(30);
            entity.Property(e => e.MainVendor).HasDefaultValue(false);
            entity.Property(e => e.Mfgs)
                .HasMaxLength(255)
                .HasDefaultValue("");
            entity.Property(e => e.ModifiedBy).HasMaxLength(20);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.NumOfSites).HasDefaultValue(0);
            entity.Property(e => e.OldCompanyId).HasColumnName("OldCompanyID");
            entity.Property(e => e.OldContactId).HasColumnName("OldContactID");
            entity.Property(e => e.Other1).HasMaxLength(30);
            entity.Property(e => e.Other2).HasMaxLength(30);
            entity.Property(e => e.Other3).HasMaxLength(30);
            entity.Property(e => e.Other4).HasMaxLength(30);
            entity.Property(e => e.PhoneCell).HasMaxLength(30);
            entity.Property(e => e.PhoneDirect).HasMaxLength(30);
            entity.Property(e => e.PhoneMain).HasMaxLength(30);
            entity.Property(e => e.PoaccountNum)
                .HasMaxLength(20)
                .HasDefaultValue("")
                .HasComment("the RW account number for PO's")
                .HasColumnName("POAccountNum");
            entity.Property(e => e.RwaccountNum)
                .HasMaxLength(20)
                .HasColumnName("RWAccountNum");
            entity.Property(e => e.SCompanyIds)
                .HasMaxLength(512)
                .HasColumnName("sCompanyIDs");
            entity.Property(e => e.SContactIds)
                .HasMaxLength(512)
                .HasColumnName("sContactIDs");
            entity.Property(e => e.SalesTeam).HasMaxLength(50);
            entity.Property(e => e.Source).HasMaxLength(50);
            entity.Property(e => e.Spouse).HasMaxLength(128);
            entity.Property(e => e.State).HasMaxLength(20);
            entity.Property(e => e.TempKatietoTimG).HasDefaultValue((byte)0);
            entity.Property(e => e.Title).HasMaxLength(50);
            entity.Property(e => e.VerizonArea)
                .HasMaxLength(25)
                .HasDefaultValue("");
            entity.Property(e => e.VerizonMarket)
                .HasMaxLength(25)
                .HasDefaultValue("");
            entity.Property(e => e.VerizonRegion)
                .HasMaxLength(25)
                .HasDefaultValue("");
            entity.Property(e => e.WarrantyTerms).HasDefaultValue(0);
            entity.Property(e => e.Website).HasMaxLength(128);
            entity.Property(e => e.Zip).HasMaxLength(12);
        });

        modelBuilder.Entity<CamFieldsList>(entity =>
        {
            entity.ToTable("camFieldsList");

            entity.HasIndex(e => e.FieldName, "IX_camFieldsListFldName").HasFillFactor(80);

            entity.HasIndex(e => e.FieldValue, "IX_camFieldsListFldValue1").HasFillFactor(80);

            entity.HasIndex(e => e.FieldValue2, "IX_camFieldsListFldValue2").HasFillFactor(80);

            entity.HasIndex(e => e.ListName, "IX_camFieldsListName").HasFillFactor(80);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(512)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.FieldName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FieldValue)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.FieldValue2)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.FieldValue3)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.FieldValue4)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.FieldValue5)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.FieldValue6)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.FourG)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ListName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.Property(e => e.ThreeG)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TwoG)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<CompetitorCall>(entity =>
        {
            entity.HasKey(e => e.CallId);

            entity.ToTable("CompetitorCall");

            entity.HasIndex(e => new { e.MfgPartNum, e.QtyNotAvailable, e.HowMany, e.ModifiedDate }, "IX_CompetCall_MfgPartNum_QtyNotAvail").HasFillFactor(80);

            entity.HasIndex(e => new { e.PartNum, e.QtyNotAvailable, e.HowMany, e.EntryDate }, "IX_CompetCall_PartNumQtyAvail").HasFillFactor(80);

            entity.HasIndex(e => new { e.RequestId, e.AvgCostFlag }, "IX_CompetCall_RequestID_AvgCostFlag").HasFillFactor(80);

            entity.HasIndex(e => e.PartNum, "IX_CompetitorCall_PartNum").HasFillFactor(80);

            entity.HasIndex(e => new { e.PartNum, e.HowMany }, "IX_CompetitorCall_PartNumHowMany").HasFillFactor(80);

            entity.Property(e => e.CallId).HasColumnName("CallID");
            entity.Property(e => e.AvgCostFlag).HasDefaultValue((byte)0);
            entity.Property(e => e.CallType)
                .HasMaxLength(20)
                .HasDefaultValue("Purchasing");
            entity.Property(e => e.Category)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.Comments)
                .HasMaxLength(1500)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.CompanyName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ContactId)
                .HasDefaultValue(0)
                .HasColumnName("ContactID");
            entity.Property(e => e.ContactName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Country)
                .HasMaxLength(25)
                .HasDefaultValue("Domestic");
            entity.Property(e => e.CurDate)
                .HasDefaultValueSql("(((1)/(1))/(1990))")
                .HasColumnType("datetime")
                .HasColumnName("curDate");
            entity.Property(e => e.CurOurCost)
                .HasDefaultValue(0m)
                .HasColumnType("money")
                .HasColumnName("curOurCost");
            entity.Property(e => e.CurRate)
                .HasDefaultValue(0.0)
                .HasColumnName("curRate");
            entity.Property(e => e.CurType)
                .HasMaxLength(5)
                .HasDefaultValue("")
                .HasColumnName("curType");
            entity.Property(e => e.CurType2)
                .HasMaxLength(5)
                .HasDefaultValue("")
                .HasColumnName("curType2");
            entity.Property(e => e.EnteredBy).HasDefaultValue(0);
            entity.Property(e => e.EntryDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EquipmentFound).HasDefaultValue(false);
            entity.Property(e => e.FieldOrStock)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasDefaultValue("Field");
            entity.Property(e => e.HowMany).HasDefaultValue(0.0);
            entity.Property(e => e.LeadTime)
                .HasMaxLength(75)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.LeftAmessage)
                .HasDefaultValue(false)
                .HasColumnName("LeftAMessage");
            entity.Property(e => e.ListPrice).HasDefaultValue(0.0);
            entity.Property(e => e.MassMailing).HasDefaultValue(false);
            entity.Property(e => e.MfgDiscount).HasDefaultValue(0.0);
            entity.Property(e => e.MfgPartNum)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.ModifiedBy).HasDefaultValue(0);
            entity.Property(e => e.ModifiedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NewOrUsed)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.OfferPrice)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasDefaultValueSql("((0))");
            entity.Property(e => e.OurCost).HasDefaultValue(0.0);
            entity.Property(e => e.PartNum)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.PartialFound).HasDefaultValue(false);
            entity.Property(e => e.ProductCode)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.QtyNotAvailable).HasDefaultValue(false);
            entity.Property(e => e.QuoteValidFor).HasDefaultValue(30);
            entity.Property(e => e.RequestId)
                .HasDefaultValue(0)
                .HasColumnName("RequestID");
            entity.Property(e => e.UrgencyRange)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.Warranty).HasDefaultValue(0);
        });

        modelBuilder.Entity<CsQtSoToInvNo>(entity =>
        {
            entity.HasKey(e => e.RowId);

            entity.ToTable("csQtSoToInvNo");

            entity.Property(e => e.RowId).HasColumnName("rowID");
            entity.Property(e => e.AccountExec)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.AccountMgr)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CustAcctNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OrgEventId).HasColumnName("OrgEventID");
            entity.Property(e => e.OrgQuoteId).HasColumnName("OrgQuoteID");
            entity.Property(e => e.SalesRep)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.SalesTeam)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id).IsClustered(false);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CompanyId).HasColumnName("CompanyID");
            entity.Property(e => e.DeptEmail).HasMaxLength(50);
            entity.Property(e => e.DeptName).HasMaxLength(50);
            entity.Property(e => e.Fax).HasMaxLength(50);
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.MgrId).HasColumnName("MgrID");
            entity.Property(e => e.OpHours).HasMaxLength(20);
            entity.Property(e => e.Phone).HasMaxLength(50);
        });

        modelBuilder.Entity<EquipmentRequest>(entity =>
        {
            entity.HasKey(e => e.RequestId).IsClustered(false);

            entity.ToTable("EquipmentRequest");

            entity.HasIndex(e => new { e.AltPartNum, e.Status, e.RequestId, e.EntryDate }, "IX_EquipRequest_AltPart_Stat_RequestID").HasFillFactor(80);

            entity.HasIndex(e => e.EventId, "IX_EquipRequest_EventID").HasFillFactor(80);

            entity.HasIndex(e => new { e.EventId, e.Status }, "IX_EquipRequest_EventID_Status").HasFillFactor(80);

            entity.HasIndex(e => new { e.PartNum, e.AltPartNum, e.Status, e.RequestId, e.EntryDate }, "IX_EquipRequest_PartNumAltStatus").HasFillFactor(80);

            entity.HasIndex(e => new { e.Status, e.RequestId, e.EntryDate }, "IX_EquipRequest_StatusRequestidEnterDate_More").HasFillFactor(80);

            entity.HasIndex(e => new { e.LostButOngoing, e.OnGoingDate }, "IX_EquipmentRequest_LostButOnGoingAndDate").HasFillFactor(80);

            entity.HasIndex(e => new { e.MassMailDate, e.MassMailSentBy }, "IX_EquipmentRequest_MailDateBy").HasFillFactor(80);

            entity.HasIndex(e => new { e.MassMailing, e.MassMailDate, e.MassMailSentBy }, "IX_EquipmentRequest_MassMailandDate").HasFillFactor(80);

            entity.Property(e => e.RequestId).HasColumnName("RequestID");
            entity.Property(e => e.AllPossBy).HasDefaultValue(0);
            entity.Property(e => e.AllPossDate).HasColumnType("datetime");
            entity.Property(e => e.AllPossibilities).HasDefaultValue(false);
            entity.Property(e => e.AltPartNum)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.AmsnoozeDate)
                .HasDefaultValue(new DateTime(1990, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified))
                .HasColumnType("datetime")
                .HasColumnName("AMsnoozeDate");
            entity.Property(e => e.Amsnoozed)
                .HasDefaultValue(0)
                .HasColumnName("AMsnoozed");
            entity.Property(e => e.Bought).HasDefaultValue(false);
            entity.Property(e => e.BuyingOppId)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("BuyingOppID");
            entity.Property(e => e.CancelDate).HasColumnType("datetime");
            entity.Property(e => e.CanceledBy).HasDefaultValue(0);
            entity.Property(e => e.Category)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.Comments)
                .HasMaxLength(1500)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.CustomerPricing)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.DateNeeded)
                .HasDefaultValueSql("(((1)/(1))/(1990))")
                .HasColumnType("datetime");
            entity.Property(e => e.DexterId)
                .HasDefaultValue(0)
                .HasColumnName("DexterID");
            entity.Property(e => e.DropShipment).HasDefaultValue(false);
            entity.Property(e => e.EntryDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EquipFound).HasDefaultValue(false);
            entity.Property(e => e.EquipmentType)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.FoundEmailSent).HasDefaultValue(false);
            entity.Property(e => e.Frequency)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.InBuyingOpp).HasDefaultValue(false);
            entity.Property(e => e.InvalidPartDate)
                .HasDefaultValueSql("(((1)/(1))/(1990))")
                .HasColumnType("datetime");
            entity.Property(e => e.InvalidPartNum).HasDefaultValue(false);
            entity.Property(e => e.LostButOngoing).HasDefaultValue(false);
            entity.Property(e => e.Manufacturer)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MarkedSoldDate).HasColumnType("datetime");
            entity.Property(e => e.MassMailDate).HasColumnType("datetime");
            entity.Property(e => e.MassMailSentBy).HasDefaultValue(0);
            entity.Property(e => e.MassMailing).HasDefaultValue(false);
            entity.Property(e => e.ModifiedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NeedToBuy).HasDefaultValue(0);
            entity.Property(e => e.NeedToBuyTs)
                .HasColumnType("datetime")
                .HasColumnName("NeedToBuyTS");
            entity.Property(e => e.OnGoingDate)
                .HasDefaultValue(new DateTime(2000, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified))
                .HasColumnType("datetime");
            entity.Property(e => e.OnHold).HasDefaultValue(false);
            entity.Property(e => e.PartDesc)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.PartNum)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PartialFound).HasDefaultValue(false);
            entity.Property(e => e.Platform)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.Porequired)
                .HasDefaultValue((byte)0)
                .HasColumnName("PORequired");
            entity.Property(e => e.ProcureRep).HasDefaultValue(0);
            entity.Property(e => e.Pwbflag)
                .HasDefaultValue(false)
                .HasColumnName("PWBFlag");
            entity.Property(e => e.QtySold).HasDefaultValue(0);
            entity.Property(e => e.QuoteDeadLine)
                .HasDefaultValue(new DateTime(1990, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified))
                .HasColumnType("datetime");
            entity.Property(e => e.QuoteFullQty).HasDefaultValue(false);
            entity.Property(e => e.QuoteNum)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.ReasonLost)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.RevDetails)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.RevSpecific).HasDefaultValue((byte)0);
            entity.Property(e => e.Rmaflag)
                .HasDefaultValue((byte)0)
                .HasColumnName("RMAFlag");
            entity.Property(e => e.RwpartNumFlag)
                .HasDefaultValue(false)
                .HasColumnName("RWPartNumFlag");
            entity.Property(e => e.RwqtyFlag)
                .HasDefaultValue(false)
                .HasColumnName("RWQtyFlag");
            entity.Property(e => e.SalePrice).HasDefaultValue(0.0);
            entity.Property(e => e.SalesOrderNum)
                .HasMaxLength(2048)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.SoldWborder)
                .HasDefaultValue(false)
                .HasColumnName("SoldWBOrder");
            entity.Property(e => e.SoldWorkbenchDate)
                .HasDefaultValue(new DateTime(1990, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified))
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasDefaultValue("Pending");
            entity.Property(e => e.TechWbreqForSo)
                .HasDefaultValue(false)
                .HasColumnName("TechWBReqForSO");
            entity.Property(e => e.Technology)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.UnitMeasure)
                .HasMaxLength(25)
                .HasDefaultValue("Each");
            entity.Property(e => e.Urgent).HasDefaultValue((byte)0);
            entity.Property(e => e.UsedPart).HasDefaultValue((byte)0);
            entity.Property(e => e.WbnotifyFlag)
                .HasDefaultValue(false)
                .HasColumnName("WBNotifyFlag");
            entity.Property(e => e.WorkbenchDate)
                .HasDefaultValue(new DateTime(1990, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified))
                .HasColumnType("datetime");
            entity.Property(e => e.ZeroLeftToFind)
                .HasDefaultValue(false)
                .HasComment("added this b/c of the new report showing calls made on requests but still having to find qty; once a req hits the WB needing qty found it still stays on there so they can make more calls but for reporting purposes we don't need to include these so this flag will help leave them off the report");
        });

        modelBuilder.Entity<EquipmentSnapshot>(entity =>
        {
            entity.HasKey(e => e.SnapshotId);

            entity.ToTable("EquipmentSnapshot");

            entity.Property(e => e.SnapshotId).HasColumnName("SnapshotID");
            entity.Property(e => e.Comments)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.CustomersCanUse)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.EntryDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.ForecastDue).HasColumnType("datetime");
            entity.Property(e => e.ForecastRequired).HasDefaultValue((byte)0);
            entity.Property(e => e.ModifiedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<MassMailHistory>(entity =>
        {
            entity.ToTable("MassMailHistory");

            entity.HasIndex(e => new { e.MassMailId, e.CompanyName }, "IX_MassMailHistory_MailID_CoName").HasFillFactor(80);

            entity.HasIndex(e => e.MassMailId, "IX_MassMailHistory_MailID_w_CoName_ConName").HasFillFactor(80);

            entity.Property(e => e.AltPartNum)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.CompanyName).HasMaxLength(50);
            entity.Property(e => e.ContactName)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.DateSent)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MassMailId)
                .HasDefaultValue(0)
                .HasColumnName("MassMailID");
            entity.Property(e => e.PartDesc)
                .HasMaxLength(125)
                .HasDefaultValue("");
            entity.Property(e => e.PartNum)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.Qty).HasDefaultValue(0);
            entity.Property(e => e.RequestId)
                .HasDefaultValue(0)
                .HasColumnName("RequestID");
            entity.Property(e => e.RespondedTo).HasDefaultValue(false);
        });

        modelBuilder.Entity<MassMailer>(entity =>
        {
            entity.HasKey(e => e.MassMailId);

            entity.ToTable("MassMailer");

            entity.Property(e => e.MassMailId).HasColumnName("MassMailID");
            entity.Property(e => e.DateSent)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MassMailDesc)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.SentBy).HasDefaultValue(38);
        });

        modelBuilder.Entity<MasterSearchQuery>(entity =>
        {
            entity.HasKey(e => e.RowId);

            entity.Property(e => e.RowId).HasColumnName("rowID");
            entity.Property(e => e.Company).HasDefaultValue(false);
            entity.Property(e => e.EventId)
                .HasDefaultValue(false)
                .HasColumnName("EventID");
            entity.Property(e => e.InvNo).HasDefaultValue(false);
            entity.Property(e => e.Mfg).HasDefaultValue(false);
            entity.Property(e => e.PartDesc).HasDefaultValue(false);
            entity.Property(e => e.PartNo).HasDefaultValue(false);
            entity.Property(e => e.PoNo).HasDefaultValue(false);
            entity.Property(e => e.SearchBy).HasMaxLength(50);
            entity.Property(e => e.SearchDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SearchFor).HasMaxLength(50);
            entity.Property(e => e.SearchText).HasMaxLength(50);
            entity.Property(e => e.SearchType).HasMaxLength(50);
            entity.Property(e => e.SoNo).HasDefaultValue(false);
        });

        modelBuilder.Entity<OpenSoreport>(entity =>
        {
            entity.HasKey(e => e.Id).IsClustered(false);

            entity.ToTable("OpenSOReport");

            entity.HasIndex(e => e.SalesRep, "NonClusteredIndex-20220118-135229");

            entity.Property(e => e.AccountNo)
                .HasMaxLength(25)
                .HasDefaultValue("");
            entity.Property(e => e.AccountTeam)
                .HasMaxLength(20)
                .HasDefaultValue("");
            entity.Property(e => e.AllHere).HasDefaultValue(false);
            entity.Property(e => e.AmountLeft)
                .HasDefaultValue(0m)
                .HasColumnType("money");
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.CompanyId)
                .HasMaxLength(10)
                .HasDefaultValue("AIR")
                .HasColumnName("CompanyID");
            entity.Property(e => e.CustPo)
                .HasMaxLength(50)
                .HasDefaultValue("")
                .HasColumnName("CustPO");
            entity.Property(e => e.CustomerName).HasMaxLength(100);
            entity.Property(e => e.EntryDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EventId)
                .HasDefaultValue(0)
                .HasColumnName("EventID");
            entity.Property(e => e.ExpectedDelivery)
                .HasDefaultValueSql("(((1)/(1))/(1990))")
                .HasColumnType("datetime");
            entity.Property(e => e.ItemNum).HasMaxLength(25);
            entity.Property(e => e.LeftToShip).HasDefaultValue(0);
            entity.Property(e => e.MfgNum)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.OrderDate).HasColumnType("datetime");
            entity.Property(e => e.OrgLeftToShip).HasDefaultValue(0);
            entity.Property(e => e.PoissueDate)
                .HasDefaultValueSql("(((1)/(1))/(1990))")
                .HasColumnType("datetime")
                .HasColumnName("POIssueDate");
            entity.Property(e => e.Ponote)
                .HasDefaultValue(false)
                .HasColumnName("PONote");
            entity.Property(e => e.PonoteDate)
                .HasColumnType("datetime")
                .HasColumnName("PONoteDate");
            entity.Property(e => e.Ponum)
                .HasMaxLength(250)
                .HasDefaultValue("")
                .HasColumnName("PONum");
            entity.Property(e => e.QtyOrdered).HasDefaultValue(0);
            entity.Property(e => e.QtyReceived).HasDefaultValue(0);
            entity.Property(e => e.ReceivedOnPothatDay)
                .HasDefaultValue(false)
                .HasColumnName("ReceivedOnPOThatDay");
            entity.Property(e => e.RequiredDate).HasColumnType("datetime");
            entity.Property(e => e.SalesRep)
                .HasMaxLength(25)
                .HasDefaultValue("");
            entity.Property(e => e.Sonum)
                .HasMaxLength(10)
                .HasColumnName("SONum");
            entity.Property(e => e.UnitPrice)
                .HasDefaultValue(0m)
                .HasColumnType("money");
        });

        modelBuilder.Entity<PhoneNumber>(entity =>
        {
            entity.HasKey(e => e.Id).IsClustered(false);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Note).HasMaxLength(255);
            entity.Property(e => e.PhoneName).HasMaxLength(50);
            entity.Property(e => e.PhoneNumber1)
                .HasMaxLength(50)
                .HasColumnName("PhoneNumber");
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<PortalItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__portal_i__3213E83F721A5A11");

            entity.ToTable("portal_items");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ColumnGroup)
                .HasDefaultValue(1)
                .HasColumnName("column_group");
            entity.Property(e => e.IconName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("icon_name");
            entity.Property(e => e.ItemType)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("item")
                .HasColumnName("item_type");
            entity.Property(e => e.Label)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("label");
            entity.Property(e => e.Ordering).HasColumnName("ordering");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.Path)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("path");
            entity.Property(e => e.WorkspaceId).HasColumnName("workspace_id");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("fk_portal_items_parent");

            entity.HasOne(d => d.Workspace).WithMany(p => p.PortalItems)
                .HasForeignKey(d => d.WorkspaceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_portal_items_workspace");
        });

        modelBuilder.Entity<PortalMenu>(entity =>
        {
            entity.ToTable("PortalMenu");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccessIds)
                .HasMaxLength(255)
                .HasDefaultValue("")
                .HasColumnName("AccessIDs");
            entity.Property(e => e.Al)
                .HasDefaultValue(0)
                .HasColumnName("AL");
            entity.Property(e => e.Caption)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.Link)
                .HasMaxLength(255)
                .HasDefaultValue("");
            entity.Property(e => e.MenuName)
                .HasMaxLength(50)
                .HasDefaultValue("Default");
            entity.Property(e => e.MgrsRpt).HasDefaultValue(false);
            entity.Property(e => e.MostUsed).HasDefaultValue(false);
            entity.Property(e => e.ReportOption).HasDefaultValue(false);
            entity.Property(e => e.RootId)
                .HasDefaultValue(0)
                .HasColumnName("rootID");
            entity.Property(e => e.Show)
                .HasDefaultValue((byte)1)
                .HasColumnName("show");
            entity.Property(e => e.Target)
                .HasMaxLength(50)
                .HasDefaultValue("")
                .HasColumnName("target");
            entity.Property(e => e.ZOrder)
                .HasMaxLength(4)
                .HasDefaultValue("")
                .HasColumnName("zOrder");
        });

        modelBuilder.Entity<PortalRoute>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__portal_r__3213E83FD4F9FA7A");

            entity.ToTable("portal_routes");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ComponentName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("component_name");
            entity.Property(e => e.IsPrivate)
                .HasDefaultValue(true)
                .HasColumnName("is_private");
            entity.Property(e => e.Ordering).HasColumnName("ordering");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.Path)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("path");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("fk_portal_routes_parent");
        });

        modelBuilder.Entity<PortalUserConfig>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__portal_u__3213E83F03E71F86");

            entity.ToTable("portal_user_config");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Config).HasColumnName("config");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.WorkspaceId).HasColumnName("workspace_id");

            entity.HasOne(d => d.User).WithMany(p => p.PortalUserConfigs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_portal_user_config_user");

            entity.HasOne(d => d.Workspace).WithMany(p => p.PortalUserConfigs)
                .HasForeignKey(d => d.WorkspaceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_portal_user_config_workspace");
        });

        modelBuilder.Entity<PortalUserFavorite>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__portal_u__3213E83F5D5147B8");

            entity.ToTable("portal_user_favorites");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.Ordering).HasColumnName("ordering");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Item).WithMany(p => p.PortalUserFavorites)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_portal_user_favorites_item");

            entity.HasOne(d => d.User).WithMany(p => p.PortalUserFavorites)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_portal_user_favorites_user");
        });

        modelBuilder.Entity<PortalWorkspace>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__portal_w__3213E83F8C231517");

            entity.ToTable("portal_workspaces");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
        });

        modelBuilder.Entity<QtQuote>(entity =>
        {
            entity.HasKey(e => e.QuoteId).HasName("PK_qtQuotes");

            entity.ToTable("qtQuote");

            entity.HasIndex(e => e.EventId, "IX_qtQuote_EventID").HasFillFactor(80);

            entity.Property(e => e.QuoteId).HasColumnName("QuoteID");
            entity.Property(e => e.AccountMgr).HasDefaultValue(0);
            entity.Property(e => e.Approved).HasDefaultValue(false);
            entity.Property(e => e.ApprovedBy).HasDefaultValue(0);
            entity.Property(e => e.ApprovedDate)
                .HasDefaultValueSql("(((1)/(1))/(1990))")
                .HasColumnType("datetime");
            entity.Property(e => e.ApprovedFirst).HasDefaultValue(false);
            entity.Property(e => e.BillToCompanyName)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.BillToCustNum)
                .HasMaxLength(25)
                .HasDefaultValue("");
            entity.Property(e => e.BtAddr1)
                .HasMaxLength(50)
                .HasDefaultValue("")
                .HasColumnName("btAddr1");
            entity.Property(e => e.BtAddr2)
                .HasMaxLength(50)
                .HasDefaultValue("")
                .HasColumnName("btAddr2");
            entity.Property(e => e.BtAddr3)
                .HasMaxLength(50)
                .HasDefaultValue("")
                .HasColumnName("btAddr3");
            entity.Property(e => e.BtAddr4)
                .HasMaxLength(50)
                .HasDefaultValue("")
                .HasColumnName("btAddr4");
            entity.Property(e => e.Comments).HasMaxLength(3000);
            entity.Property(e => e.CompetitorFlag).HasDefaultValue(false);
            entity.Property(e => e.CurDate)
                .HasDefaultValueSql("(((1)/(1))/(1990))")
                .HasColumnType("datetime")
                .HasColumnName("curDate");
            entity.Property(e => e.CurQuoteTotal)
                .HasDefaultValue(0m)
                .HasColumnType("money")
                .HasColumnName("curQuoteTotal");
            entity.Property(e => e.CurRate)
                .HasDefaultValue(0.0)
                .HasColumnName("curRate");
            entity.Property(e => e.CurShipping)
                .HasDefaultValue(0m)
                .HasColumnType("money")
                .HasColumnName("curShipping");
            entity.Property(e => e.CurTotalCost)
                .HasDefaultValue(0m)
                .HasColumnType("money")
                .HasColumnName("curTotalCost");
            entity.Property(e => e.CurType)
                .HasMaxLength(5)
                .HasDefaultValue("")
                .HasColumnName("curType");
            entity.Property(e => e.CustomerPo)
                .HasMaxLength(50)
                .HasDefaultValue("")
                .HasColumnName("CustomerPO");
            entity.Property(e => e.EntryDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.MgrNotes).HasColumnType("text");
            entity.Property(e => e.ProjectCode).HasMaxLength(25);
            entity.Property(e => e.QuoteTotal)
                .HasDefaultValue(0m)
                .HasColumnType("money");
            entity.Property(e => e.RequiredDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RwsalesOrderNum)
                .HasMaxLength(2048)
                .HasDefaultValueSql("((0))")
                .HasColumnName("RWSalesOrderNum");
            entity.Property(e => e.SaleDate)
                .HasDefaultValueSql("(((1)/(1))/(1990))")
                .HasColumnType("datetime");
            entity.Property(e => e.SalesTeam)
                .HasMaxLength(25)
                .HasDefaultValue("");
            entity.Property(e => e.ShipToCompanyName)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.ShipToCustNum)
                .HasMaxLength(25)
                .HasDefaultValue("");
            entity.Property(e => e.ShipVia)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.ShippingHandling)
                .HasDefaultValue(0m)
                .HasColumnType("money");
            entity.Property(e => e.StAddr1)
                .HasMaxLength(50)
                .HasDefaultValue("")
                .HasColumnName("stAddr1");
            entity.Property(e => e.StAddr2)
                .HasMaxLength(50)
                .HasDefaultValue("")
                .HasColumnName("stAddr2");
            entity.Property(e => e.StAddr3)
                .HasMaxLength(50)
                .HasDefaultValue("")
                .HasColumnName("stAddr3");
            entity.Property(e => e.StAddr4)
                .HasMaxLength(50)
                .HasDefaultValue("")
                .HasColumnName("stAddr4");
            entity.Property(e => e.Terms)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.TotalCost)
                .HasDefaultValue(0m)
                .HasColumnType("money");
            entity.Property(e => e.Warranty)
                .HasDefaultValue(365)
                .HasComment("warranty in days");
        });

        modelBuilder.Entity<QtQuoteDetail>(entity =>
        {
            entity.ToTable("qtQuoteDetail");

            entity.HasIndex(e => new { e.QuoteId, e.RequestId }, "IX_QuoteDetail_QuoteID_ReqID").HasFillFactor(80);

            entity.HasIndex(e => e.PartNum, "IX_qtQuoteDetail_PartNum").HasFillFactor(80);

            entity.HasIndex(e => e.RequestId, "idx_qtQuoteDetail_RequestID");

            entity.Property(e => e.Comments)
                .HasDefaultValue("")
                .HasColumnType("text");
            entity.Property(e => e.Cost)
                .HasDefaultValue(0m)
                .HasColumnType("money");
            entity.Property(e => e.CurCost)
                .HasDefaultValue(0m)
                .HasColumnType("money")
                .HasColumnName("curCost");
            entity.Property(e => e.CurExtPrice)
                .HasDefaultValue(0m)
                .HasColumnType("money")
                .HasColumnName("curExtPrice");
            entity.Property(e => e.CurUnitPrice)
                .HasDefaultValue(0m)
                .HasColumnType("money")
                .HasColumnName("curUnitPrice");
            entity.Property(e => e.ExtendedPrice).HasDefaultValue(0.0);
            entity.Property(e => e.MfgPartNum)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.PartDesc)
                .HasMaxLength(125)
                .HasDefaultValue("");
            entity.Property(e => e.PartNum)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.QtyFound).HasDefaultValue(0);
            entity.Property(e => e.QtyRequested).HasDefaultValue(0);
            entity.Property(e => e.QuoteId)
                .HasDefaultValue(0)
                .HasColumnName("QuoteID");
            entity.Property(e => e.QuoteOrder).HasDefaultValue(0);
            entity.Property(e => e.QuoteQty).HasDefaultValue(0);
            entity.Property(e => e.RequestId)
                .HasDefaultValue(0)
                .HasColumnName("RequestID");
            entity.Property(e => e.UnitMeasure)
                .HasMaxLength(20)
                .HasDefaultValue("Each");
            entity.Property(e => e.UnitPrice).HasDefaultValue(0.0);
        });

        modelBuilder.Entity<QtSalesOrder>(entity =>
        {
            entity.HasKey(e => e.SaleId);

            entity.ToTable("qtSalesOrder");

            entity.HasIndex(e => e.EventId, "IX_qtSalesOrder_EventID").HasFillFactor(80);

            entity.Property(e => e.SaleId).HasColumnName("SaleID");
            entity.Property(e => e.AccountMgr).HasDefaultValue(0);
            entity.Property(e => e.BillToCompanyName)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.BillToCustNum)
                .HasMaxLength(25)
                .HasDefaultValue("");
            entity.Property(e => e.BtAddr1)
                .HasMaxLength(50)
                .HasDefaultValue("")
                .HasColumnName("btAddr1");
            entity.Property(e => e.BtAddr2)
                .HasMaxLength(50)
                .HasDefaultValue("")
                .HasColumnName("btAddr2");
            entity.Property(e => e.BtAddr3)
                .HasMaxLength(50)
                .HasDefaultValue("")
                .HasColumnName("btAddr3");
            entity.Property(e => e.BtAddr4)
                .HasMaxLength(50)
                .HasDefaultValue("")
                .HasColumnName("btAddr4");
            entity.Property(e => e.Comments)
                .HasMaxLength(1500)
                .HasDefaultValue("");
            entity.Property(e => e.CompetitorFlag).HasDefaultValue(false);
            entity.Property(e => e.CurDate)
                .HasDefaultValueSql("(((1)/(1))/(1990))")
                .HasColumnType("datetime")
                .HasColumnName("curDate");
            entity.Property(e => e.CurRate)
                .HasDefaultValue(0.0)
                .HasColumnName("curRate");
            entity.Property(e => e.CurSalesTotal)
                .HasDefaultValue(0m)
                .HasColumnType("money")
                .HasColumnName("curSalesTotal");
            entity.Property(e => e.CurShipping)
                .HasDefaultValue(0m)
                .HasColumnType("money")
                .HasColumnName("curShipping");
            entity.Property(e => e.CurType)
                .HasMaxLength(5)
                .HasDefaultValue("")
                .HasColumnName("curType");
            entity.Property(e => e.CustomerPo)
                .HasMaxLength(50)
                .HasDefaultValue("")
                .HasColumnName("CustomerPO");
            entity.Property(e => e.Draft).HasDefaultValue(false);
            entity.Property(e => e.DropShipment).HasDefaultValue(false);
            entity.Property(e => e.EditDate)
                .HasDefaultValueSql("(((1)/(1))/(1990))")
                .HasColumnType("datetime");
            entity.Property(e => e.EnteredBy)
                .HasMaxLength(20)
                .HasDefaultValue("");
            entity.Property(e => e.EventId)
                .HasDefaultValue(0)
                .HasColumnName("EventID");
            entity.Property(e => e.QuoteId)
                .HasDefaultValue(0)
                .HasColumnName("QuoteID");
            entity.Property(e => e.RequiredDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RwsalesOrderNum)
                .HasMaxLength(2048)
                .HasDefaultValueSql("((0))")
                .HasColumnName("RWSalesOrderNum");
            entity.Property(e => e.SaleDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SaleTotal)
                .HasDefaultValue(0m)
                .HasColumnType("money");
            entity.Property(e => e.ShipToCompanyName)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.ShipToCustNum)
                .HasMaxLength(25)
                .HasDefaultValue("");
            entity.Property(e => e.ShipVia)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.ShippingHandling)
                .HasDefaultValue(0m)
                .HasColumnType("money");
            entity.Property(e => e.StAddr1)
                .HasMaxLength(100)
                .HasDefaultValue("")
                .HasColumnName("stAddr1");
            entity.Property(e => e.StAddr2)
                .HasMaxLength(100)
                .HasDefaultValue("")
                .HasColumnName("stAddr2");
            entity.Property(e => e.StAddr3)
                .HasMaxLength(100)
                .HasDefaultValue("")
                .HasColumnName("stAddr3");
            entity.Property(e => e.StAddr4)
                .HasMaxLength(100)
                .HasDefaultValue("")
                .HasColumnName("stAddr4");
            entity.Property(e => e.Terms)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.Version).HasDefaultValue(0);
            entity.Property(e => e.Warranty)
                .HasDefaultValue(365)
                .HasComment("warranty in days");
        });

        modelBuilder.Entity<QtSalesOrderDetail>(entity =>
        {
            entity.ToTable("qtSalesOrderDetail");

            entity.HasIndex(e => new { e.PartNum, e.UnitPrice }, "IX_qtSalesOrderDetail_3").HasFillFactor(80);

            entity.HasIndex(e => e.SaleId, "IX_qtSalesOrderDetail_SaleID").HasFillFactor(80);

            entity.HasIndex(e => new { e.SaleId, e.QtySold }, "IX_qtSalesOrderDetail_SalesID_QtySold").HasFillFactor(80);

            entity.Property(e => e.AutoSoflag)
                .HasDefaultValue(false)
                .HasColumnName("AutoSOFlag");
            entity.Property(e => e.CurExtPrice)
                .HasDefaultValue(0m)
                .HasColumnType("money")
                .HasColumnName("curExtPrice");
            entity.Property(e => e.CurUnitPrice)
                .HasDefaultValue(0m)
                .HasColumnType("money")
                .HasColumnName("curUnitPrice");
            entity.Property(e => e.ExtendedPrice).HasDefaultValue(0.0);
            entity.Property(e => e.PartDesc)
                .HasMaxLength(100)
                .HasDefaultValue("");
            entity.Property(e => e.PartNum)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.QtySold).HasDefaultValue(0);
            entity.Property(e => e.QuoteQty).HasDefaultValue(0);
            entity.Property(e => e.RequestId)
                .HasDefaultValue(0)
                .HasColumnName("RequestID");
            entity.Property(e => e.SaleId)
                .HasDefaultValue(0)
                .HasColumnName("SaleID");
            entity.Property(e => e.SaleOrder).HasDefaultValue(0);
            entity.Property(e => e.Soflag)
                .HasDefaultValue(false)
                .HasColumnName("SOFlag");
            entity.Property(e => e.UnitMeasure)
                .HasMaxLength(20)
                .HasDefaultValue("");
            entity.Property(e => e.UnitPrice).HasDefaultValue(0.0);
            entity.Property(e => e.UpdatedByAutoSo)
                .HasDefaultValue(false)
                .HasColumnName("UpdatedByAutoSO");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RefreshT__3214EC07F8EC3E63");

            entity.HasIndex(e => e.Username, "IX_RefreshTokens_Username");

            entity.HasIndex(e => e.Token, "UQ__RefreshT__1EB4F8172904FC20").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Token).HasMaxLength(200);
            entity.Property(e => e.Username).HasMaxLength(100);
        });

        modelBuilder.Entity<RequestEvent>(entity =>
        {
            entity.HasKey(e => e.EventId);

            entity.ToTable("RequestEvent");

            entity.HasIndex(e => new { e.SoldOrLost, e.EnteredBy, e.EntryDate }, "IX_RequestEvent_2").HasFillFactor(80);

            entity.HasIndex(e => new { e.ContactId, e.SoldOrLost }, "IX_RequestEvent_ContactID_SoldOrLost").HasFillFactor(80);

            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.BillingAccountNum)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.BillingOption).HasMaxLength(50);
            entity.Property(e => e.CommercialTerms).HasMaxLength(25);
            entity.Property(e => e.CompanyId)
                .HasMaxLength(10)
                .HasDefaultValue("AIR")
                .HasColumnName("CompanyID");
            entity.Property(e => e.CompetitorFlag).HasDefaultValue(false);
            entity.Property(e => e.ContactId).HasColumnName("ContactID");
            entity.Property(e => e.CtNotes)
                .HasMaxLength(100)
                .HasDefaultValue("")
                .HasColumnName("ctNotes");
            entity.Property(e => e.EntryDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EquipmentType)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.EventEndDate).HasColumnType("datetime");
            entity.Property(e => e.EventNotification).HasDefaultValue(false);
            entity.Property(e => e.EventOwner).HasDefaultValue(0);
            entity.Property(e => e.FiveDayReminder).HasDefaultValue(false);
            entity.Property(e => e.Frequency)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.HowToBill)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.Manufacturer)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Platform)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.ProjectName).HasMaxLength(200);
            entity.Property(e => e.QuoteDeadline)
                .HasDefaultValue(new DateTime(1990, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified))
                .HasColumnType("datetime");
            entity.Property(e => e.ReasonCode)
                .HasMaxLength(75)
                .IsUnicode(false);
            entity.Property(e => e.ResponseDate)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasDefaultValue("No final response to date");
            entity.Property(e => e.RipReplace).HasDefaultValue(false);
            entity.Property(e => e.SoldOrLost)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pending");
            entity.Property(e => e.Technology)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.TenDayReminder).HasDefaultValue(false);
        });

        modelBuilder.Entity<RequestPo>(entity =>
        {
            entity.ToTable("RequestPOs");

            entity.HasIndex(e => e.RequestId, "IX_RequestPOs_RequestID").HasFillFactor(80);

            entity.Property(e => e.ContactId)
                .HasDefaultValue(0)
                .HasColumnName("ContactID");
            entity.Property(e => e.DeliveryDate)
                .HasDefaultValueSql("(((1)/(1))/(1990))")
                .HasColumnType("datetime");
            entity.Property(e => e.EditDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EditedBy).HasDefaultValue(0);
            entity.Property(e => e.EntryDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Location)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Poalarm)
                .HasDefaultValue(false)
                .HasColumnName("POAlarm");
            entity.Property(e => e.Ponum)
                .HasMaxLength(50)
                .HasColumnName("PONum");
            entity.Property(e => e.PurchaseDate)
                .HasDefaultValueSql("(((1)/(1))/(1990))")
                .HasColumnType("datetime");
            entity.Property(e => e.PurchasedBy).HasDefaultValue(0);
            entity.Property(e => e.RequestId).HasColumnName("RequestID");
        });

        modelBuilder.Entity<RequestPohistory>(entity =>
        {
            entity.ToTable("RequestPOHistory");

            entity.Property(e => e.DeliveryDate).HasColumnType("datetime");
            entity.Property(e => e.EditDate).HasColumnType("datetime");
            entity.Property(e => e.Poid)
                .HasDefaultValue(0)
                .HasColumnName("POID");
            entity.Property(e => e.Ponum)
                .HasMaxLength(50)
                .HasColumnName("PONum");
            entity.Property(e => e.QtyBought).HasDefaultValue(0);
        });

        modelBuilder.Entity<ScanHistory>(entity =>
        {
            entity.HasKey(e => e.RowId);

            entity.ToTable("ScanHistory");

            entity.Property(e => e.RowId).HasColumnName("rowID");
            entity.Property(e => e.BinLocation)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Direction)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.HeciCode)
                .HasMaxLength(128)
                .IsUnicode(false);
            entity.Property(e => e.MnsCompany)
                .HasMaxLength(50)
                .HasColumnName("MNS_Company");
            entity.Property(e => e.MnsInventoried)
                .HasDefaultValue(false)
                .HasColumnName("MNS_Inventoried");
            entity.Property(e => e.Notes)
                .HasMaxLength(512)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.OrderType)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.PartNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PartNo2)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.PartNoClean)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PoNo)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.PostId)
                .HasDefaultValue(0)
                .HasColumnName("PostID");
            entity.Property(e => e.Rmano)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("RMANo");
            entity.Property(e => e.RtvRmaNo)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasComment("ReturnToVendor RMA No");
            entity.Property(e => e.Rtvid)
                .HasDefaultValue(0)
                .HasColumnName("RTVID");
            entity.Property(e => e.ScanDate).HasColumnType("datetime");
            entity.Property(e => e.ScannerId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("")
                .HasColumnName("ScannerID");
            entity.Property(e => e.SerialNo)
                .HasMaxLength(128)
                .IsUnicode(false);
            entity.Property(e => e.SerialNoB)
                .HasMaxLength(128)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.SoNo)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.TrackNo)
                .HasMaxLength(128)
                .IsUnicode(false);
            entity.Property(e => e.TrkEventId)
                .HasDefaultValue(0)
                .HasColumnName("TrkEventID");
            entity.Property(e => e.UserName)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.VendorName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasDefaultValue("");
        });

        modelBuilder.Entity<ScanTestLab>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("scan_TestLab");

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EditBy)
                .HasMaxLength(21)
                .IsUnicode(false);
            entity.Property(e => e.EditDate).HasColumnType("datetime");
            entity.Property(e => e.Fn).HasDefaultValue(true);
            entity.Property(e => e.HeciCode)
                .HasMaxLength(128)
                .IsUnicode(false);
            entity.Property(e => e.Notes)
                .HasMaxLength(512)
                .IsUnicode(false);
            entity.Property(e => e.OrderType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PartNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PartNo2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PartNoClean)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RedTagAction)
                .HasMaxLength(12)
                .IsUnicode(false)
                .HasDefaultValue("TBD");
            entity.Property(e => e.RedTagStatus)
                .HasMaxLength(12)
                .IsUnicode(false)
                .HasDefaultValue("Open");
            entity.Property(e => e.RowId)
                .ValueGeneratedOnAdd()
                .HasColumnName("rowID");
            entity.Property(e => e.ScanDate).HasColumnType("datetime");
            entity.Property(e => e.ScanHistId).HasColumnName("ScanHistID");
            entity.Property(e => e.ScannerId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("ScannerID");
            entity.Property(e => e.SerialNo)
                .HasMaxLength(128)
                .IsUnicode(false);
            entity.Property(e => e.SerialNoB)
                .HasMaxLength(128)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TestResult)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.UserName)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<SellOpCompetitor>(entity =>
        {
            entity.ToTable("SellOpCompetitor");

            entity.Property(e => e.CompType)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.Company)
                .HasMaxLength(255)
                .HasDefaultValue("");
            entity.Property(e => e.EntryDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EventId)
                .HasDefaultValue(0)
                .HasColumnName("EventID");
        });

        modelBuilder.Entity<ShorelineUser>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.Didnumber)
                .HasMaxLength(16)
                .HasColumnName("DIDNumber");
            entity.Property(e => e.Dntype).HasColumnName("DNType");
            entity.Property(e => e.PhoneName).HasMaxLength(50);
        });

        modelBuilder.Entity<SimpleList>(entity =>
        {
            entity.HasKey(e => e.Id).IsClustered(false);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ldesc)
                .HasMaxLength(255)
                .HasColumnName("LDesc");
            entity.Property(e => e.Litem)
                .HasMaxLength(50)
                .HasColumnName("LItem");
            entity.Property(e => e.Ltag)
                .HasMaxLength(50)
                .HasColumnName("LTag");
            entity.Property(e => e.Ltype)
                .HasMaxLength(50)
                .HasColumnName("LType");
            entity.Property(e => e.MetalCategory).HasMaxLength(50);
            entity.Property(e => e.ResTech)
                .HasDefaultValue(11)
                .HasColumnName("resTech");
        });

        modelBuilder.Entity<SoEmailNotification>(entity =>
        {
            entity.HasKey(e => e.RowId);

            entity.ToTable("SO_Email_Notifications");

            entity.Property(e => e.RowId).HasColumnName("rowID");
            entity.Property(e => e.BillToCompanyName).HasMaxLength(100);
            entity.Property(e => e.CustNum).HasMaxLength(50);
        });

        modelBuilder.Entity<TcEntry>(entity =>
        {
            entity.HasKey(e => e.RowId);

            entity.ToTable("tcEntries");

            entity.Property(e => e.RowId).HasColumnName("rowID");
            entity.Property(e => e.ApprovedBy).HasMaxLength(50);
            entity.Property(e => e.ApprovedDate).HasColumnType("datetime");
            entity.Property(e => e.HolidayId)
                .HasDefaultValue(0)
                .HasColumnName("HolidayID");
            entity.Property(e => e.Pto)
                .HasDefaultValue(0)
                .HasColumnName("PTO");
            entity.Property(e => e.TimeIn).HasColumnType("datetime");
            entity.Property(e => e.TimeOut).HasColumnType("datetime");
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        modelBuilder.Entity<TcHistory>(entity =>
        {
            entity.HasKey(e => e.RowId);

            entity.ToTable("tcHistory");

            entity.Property(e => e.RowId).HasColumnName("rowID");
            entity.Property(e => e.Employee).HasMaxLength(50);
            entity.Property(e => e.EnteredBy).HasMaxLength(50);
            entity.Property(e => e.EntryDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Ptohistory)
                .HasMaxLength(100)
                .HasColumnName("PTOhistory");
        });

        modelBuilder.Entity<TcHoliday>(entity =>
        {
            entity.ToTable("tcHoliday");

            entity.Property(e => e.Holiday).HasMaxLength(50);
            entity.Property(e => e.HolidayDate).HasColumnType("datetime");
            entity.Property(e => e.ImgFile).HasMaxLength(50);
        });

        modelBuilder.Entity<TcPayPeriod>(entity =>
        {
            entity.HasKey(e => e.RowId);

            entity.ToTable("tcPayPeriod");

            entity.Property(e => e.RowId).HasColumnName("rowID");
            entity.Property(e => e.Date1).HasColumnType("datetime");
            entity.Property(e => e.Date2).HasColumnType("datetime");
            entity.Property(e => e.PayPeriod).HasDefaultValue(0);
        });

        modelBuilder.Entity<TcPto>(entity =>
        {
            entity.HasKey(e => e.RowId);

            entity.ToTable("tcPTO");

            entity.Property(e => e.RowId).HasColumnName("rowID");
            entity.Property(e => e.DeptId)
                .HasDefaultValue(0)
                .HasColumnName("DeptID");
            entity.Property(e => e.StartBalance).HasDefaultValue(0);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        modelBuilder.Entity<TrkAdjustment>(entity =>
        {
            entity.ToTable("trkAdjustments");

            entity.HasIndex(e => new { e.ItemNum, e.EntryDate }, "IX_trkAdjustments_ItemNumEntryDate").HasFillFactor(80);

            entity.Property(e => e.EntryDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ItemNum)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Reason)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.ScanGroup)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("");
        });

        modelBuilder.Entity<TrkCompany>(entity =>
        {
            entity.HasKey(e => e.RowId);

            entity.ToTable("trkCompany");

            entity.Property(e => e.RowId).HasColumnName("rowID");
            entity.Property(e => e.CompanyId)
                .HasMaxLength(10)
                .HasColumnName("CompanyID");
            entity.Property(e => e.CompanyName).HasMaxLength(100);
        });

        modelBuilder.Entity<TrkInventory>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("trkInventory");

            entity.HasIndex(e => new { e.CycleCountFlag, e.CycleCountDate }, "IX_trkInventory_CycleCountAndDate").HasFillFactor(80);

            entity.HasIndex(e => new { e.ItemNum2, e.MfgDiscount, e.TrkListPrice, e.TrkUnitCost, e.EditDate }, "IX_trkInventory_ItemNum2").HasFillFactor(80);

            entity.HasIndex(e => e.ItemNum, "trkInventory_ItemNum").HasFillFactor(80);

            entity.Property(e => e.ActualDimension)
                .HasDefaultValue(true)
                .HasComment("Is the dimension Actual or for Packaging; default to actual = 1 and they'll update if it's packaging");
            entity.Property(e => e.Category)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.Comments).HasMaxLength(500);
            entity.Property(e => e.CycleActualCount).HasDefaultValue(0);
            entity.Property(e => e.CycleCountBy).HasMaxLength(25);
            entity.Property(e => e.CycleCountDate).HasColumnType("datetime");
            entity.Property(e => e.CycleCountFlag).HasDefaultValue((byte)0);
            entity.Property(e => e.DimensionsEditBy)
                .HasMaxLength(25)
                .HasDefaultValue("");
            entity.Property(e => e.DimensionsEditDate)
                .HasDefaultValueSql("(((1)/(1))/(1900))")
                .HasColumnType("datetime");
            entity.Property(e => e.Eccn)
                .HasMaxLength(25)
                .HasColumnName("ECCN");
            entity.Property(e => e.EditDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EditedBy).HasDefaultValue(23);
            entity.Property(e => e.Height).HasDefaultValue(0);
            entity.Property(e => e.HeightInches).HasDefaultValue(0);
            entity.Property(e => e.Htscode)
                .HasMaxLength(25)
                .HasColumnName("HTScode");
            entity.Property(e => e.ItemNum)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ItemNum2)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasDefaultValueSql("((0))");
            entity.Property(e => e.LeadTime)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Length).HasDefaultValue(0);
            entity.Property(e => e.LengthInches).HasDefaultValue(0);
            entity.Property(e => e.MetalPartType).HasMaxLength(50);
            entity.Property(e => e.MfgDiscontinued).HasDefaultValue(false);
            entity.Property(e => e.MfgDiscount).HasDefaultValue(0.0);
            entity.Property(e => e.PartType).HasMaxLength(50);
            entity.Property(e => e.PricingExpires).HasDefaultValue(0);
            entity.Property(e => e.ProductCode)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.ProductMgmt).HasDefaultValue(false);
            entity.Property(e => e.QtyThreshold).HasDefaultValue(0);
            entity.Property(e => e.ReplacePartNum)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RetainConfirmedBy).HasMaxLength(25);
            entity.Property(e => e.RetainConfirmedDate).HasColumnType("datetime");
            entity.Property(e => e.SalvageValue).HasDefaultValue(0.0);
            entity.Property(e => e.TrkListPrice)
                .HasDefaultValue(0.0)
                .HasColumnName("trkListPrice");
            entity.Property(e => e.TrkUnitCost)
                .HasDefaultValue(0.0)
                .HasColumnName("trkUnitCost");
            entity.Property(e => e.UnitMeasure)
                .HasMaxLength(25)
                .HasDefaultValue("Each");
            entity.Property(e => e.UseValue).HasMaxLength(50);
            entity.Property(e => e.Weight).HasDefaultValue(0.0);
            entity.Property(e => e.Width).HasDefaultValue(0);
            entity.Property(e => e.WidthInches).HasDefaultValue(0);
        });

        modelBuilder.Entity<TrkPolog>(entity =>
        {
            entity.ToTable("trkPOLog");

            entity.HasIndex(e => new { e.Ponum, e.ItemNum }, "IX_trkPOLog_PoNumItemNum").HasFillFactor(80);

            entity.Property(e => e.CompanyId)
                .HasMaxLength(10)
                .HasDefaultValue("AIR")
                .HasColumnName("CompanyID");
            entity.Property(e => e.ContactId)
                .HasDefaultValue(0)
                .HasColumnName("ContactID");
            entity.Property(e => e.DateDelivered).HasColumnType("datetime");
            entity.Property(e => e.Deleted).HasDefaultValue(false);
            entity.Property(e => e.DeliveryDateEmail).HasDefaultValue(false);
            entity.Property(e => e.EditDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EditedBy).HasDefaultValue(23);
            entity.Property(e => e.ExpDelEditDate)
                .HasMaxLength(75)
                .IsUnicode(false);
            entity.Property(e => e.ExpectedDelivery).HasColumnType("datetime");
            entity.Property(e => e.IssueDate).HasColumnType("datetime");
            entity.Property(e => e.IssuedBy)
                .HasMaxLength(75)
                .IsUnicode(false);
            entity.Property(e => e.ItemNum)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.NoteEditDate)
                .HasMaxLength(75)
                .IsUnicode(false);
            entity.Property(e => e.Notes)
                .HasMaxLength(2000)
                .IsUnicode(false);
            entity.Property(e => e.PolineKey)
                .HasDefaultValue(0)
                .HasColumnName("POLineKey");
            entity.Property(e => e.Ponum)
                .HasMaxLength(200)
                .HasColumnName("PONum");
            entity.Property(e => e.QtyReceived).HasDefaultValue(0);
            entity.Property(e => e.RequiredDate).HasColumnType("datetime");
            entity.Property(e => e.SalesOrderNum)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.SalesRep).HasMaxLength(50);
        });

        modelBuilder.Entity<TrkPonote>(entity =>
        {
            entity.HasKey(e => e.RowId);

            entity.ToTable("trkPONotes");

            entity.Property(e => e.RowId).HasColumnName("rowID");
            entity.Property(e => e.EnteredBy).HasMaxLength(50);
            entity.Property(e => e.EntryDate).HasColumnType("datetime");
            entity.Property(e => e.Notes).HasColumnType("ntext");
            entity.Property(e => e.Ponum)
                .HasDefaultValue(0)
                .HasColumnName("PONum");
        });

        modelBuilder.Entity<TrkRwImItem>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("trkRwImItem");

            entity.HasIndex(e => e.ItemNum, "IX_trkRwImItemNum").HasFillFactor(80);

            entity.Property(e => e.AltPartNum).HasMaxLength(50);
            entity.Property(e => e.Category).HasMaxLength(20);
            entity.Property(e => e.CompanyId)
                .HasMaxLength(10)
                .HasColumnName("CompanyID");
            entity.Property(e => e.ItemClassId)
                .HasDefaultValue(0)
                .HasColumnName("ItemClassID");
            entity.Property(e => e.ItemClassification).HasDefaultValue(0);
            entity.Property(e => e.ItemDesc).HasMaxLength(255);
            entity.Property(e => e.ItemNum).HasMaxLength(50);
            entity.Property(e => e.LastPurchAmt).HasColumnType("money");
            entity.Property(e => e.LastSaleAmt).HasColumnType("money");
            entity.Property(e => e.ListPrice).HasMaxLength(20);
            entity.Property(e => e.Mfg).HasMaxLength(50);
            entity.Property(e => e.ProductCode).HasMaxLength(10);
            entity.Property(e => e.SubType).HasMaxLength(15);
            entity.Property(e => e.UnitCost).HasColumnType("money");
            entity.Property(e => e.VendorNum).HasMaxLength(25);
            entity.Property(e => e.WarehouseId)
                .HasDefaultValue(24)
                .HasColumnName("WarehouseID");
        });

        modelBuilder.Entity<TrkRwPoheader>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("trkRwPOHeader");

            entity.HasIndex(e => e.PotrackNum, "IX_trkRwPOHeader").HasFillFactor(80);

            entity.HasIndex(e => e.ApprovalDate, "IX_trkRwPOHeader_ActDate").HasFillFactor(80);

            entity.HasIndex(e => e.ApprovalDate, "IX_trkRwPOHeader_ApprDate").HasFillFactor(80);

            entity.HasIndex(e => e.CrtDate, "IX_trkRwPOHeader_CrtDate").HasFillFactor(80);

            entity.HasIndex(e => e.IssueDate, "IX_trkRwPOHeader_IssDate").HasFillFactor(80);

            entity.HasIndex(e => e.Poamt, "IX_trkRwPOHeader_PoAMT").HasFillFactor(80);

            entity.HasIndex(e => e.Ponum, "IX_trkRwPOHeader_PoNum").HasFillFactor(80);

            entity.Property(e => e.ActualCloseDate).HasColumnType("datetime");
            entity.Property(e => e.ApprovalDate).HasColumnType("datetime");
            entity.Property(e => e.CompanyId)
                .HasMaxLength(10)
                .HasDefaultValue("AIR")
                .HasColumnName("CompanyID");
            entity.Property(e => e.CrtDate).HasColumnType("datetime");
            entity.Property(e => e.CustomerNum).HasMaxLength(30);
            entity.Property(e => e.IssueDate).HasColumnType("datetime");
            entity.Property(e => e.IssuedBy).HasMaxLength(16);
            entity.Property(e => e.OrderNum).HasMaxLength(30);
            entity.Property(e => e.Poamt)
                .HasColumnType("money")
                .HasColumnName("POAmt");
            entity.Property(e => e.Podesc)
                .HasMaxLength(50)
                .HasColumnName("PODesc");
            entity.Property(e => e.Ponum)
                .HasMaxLength(10)
                .HasColumnName("PONum");
            entity.Property(e => e.Postatus).HasColumnName("POStatus");
            entity.Property(e => e.PotrackNum)
                .HasMaxLength(10)
                .HasColumnName("POTrackNum");
            entity.Property(e => e.RequiredDate).HasColumnType("datetime");
            entity.Property(e => e.VendorNum).HasMaxLength(16);
        });

        modelBuilder.Entity<TrkRwSodetail>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("trkRwSODetail");

            entity.HasIndex(e => e.Sonum, "IX_trkRwSODetail").HasFillFactor(80);

            entity.HasIndex(e => new { e.ItemNum, e.QtyPicked, e.QtyOrdered, e.QtyShipped }, "IX_trkRwSODetail_4").HasFillFactor(80);

            entity.HasIndex(e => new { e.ItemNum, e.QtyOrdered, e.QtyShipped }, "IX_trkRwSODetail_ItemNum_QtyOrderedShipped").HasFillFactor(80);

            entity.HasIndex(e => new { e.Sonum, e.ItemNum }, "IX_trkRwSODetail_SOItemNum").HasFillFactor(80);

            entity.Property(e => e.AccountTeam).HasMaxLength(20);
            entity.Property(e => e.CompanyId)
                .HasMaxLength(10)
                .HasColumnName("CompanyID");
            entity.Property(e => e.ExtTotal).HasColumnType("money");
            entity.Property(e => e.ItemDesc).HasMaxLength(75);
            entity.Property(e => e.ItemNum).HasMaxLength(50);
            entity.Property(e => e.QtyOpenToShip).HasDefaultValue(0);
            entity.Property(e => e.RowId)
                .ValueGeneratedOnAdd()
                .HasColumnName("rowID");
            entity.Property(e => e.Sonum)
                .HasMaxLength(10)
                .HasColumnName("SONum");
            entity.Property(e => e.UnitPrice).HasColumnType("money");
        });

        modelBuilder.Entity<TrkRwSoheader>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("trkRwSOHeader");

            entity.HasIndex(e => e.CustNum, "IX_trkRwSOHeaderCustNum").HasFillFactor(80);

            entity.HasIndex(e => e.OrderDate, "IX_trkRwSOHeaderOrdDate").HasFillFactor(80);

            entity.HasIndex(e => e.OrderNum, "IX_trkRwSOHeaderOrdNo").HasFillFactor(80);

            entity.HasIndex(e => new { e.Type, e.Status, e.CustNum }, "IX_trkRwSOHeader_TypeStatusCustNum").HasFillFactor(80);

            entity.Property(e => e.AccountTeam).HasMaxLength(20);
            entity.Property(e => e.Comments).HasMaxLength(256);
            entity.Property(e => e.CompanyId)
                .HasMaxLength(10)
                .HasColumnName("CompanyID");
            entity.Property(e => e.CustNum).HasMaxLength(16);
            entity.Property(e => e.CustPo)
                .HasMaxLength(50)
                .HasColumnName("CustPO");
            entity.Property(e => e.CustomerName).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(50);
            entity.Property(e => e.OrderDate).HasColumnType("datetime");
            entity.Property(e => e.OrderNum).HasMaxLength(10);
            entity.Property(e => e.QuoteTotal).HasColumnType("money");
            entity.Property(e => e.RequiredDate).HasColumnType("datetime");
            entity.Property(e => e.Rmanum)
                .HasMaxLength(25)
                .HasColumnName("RMANum");
            entity.Property(e => e.ShipToNum).HasMaxLength(16);
            entity.Property(e => e.Terms).HasMaxLength(25);
        });

        modelBuilder.Entity<TrkRwvendor>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("trkRWVendors");

            entity.Property(e => e.Address1)
                .HasMaxLength(75)
                .IsUnicode(false);
            entity.Property(e => e.Address2)
                .HasMaxLength(75)
                .IsUnicode(false);
            entity.Property(e => e.Address3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Address4)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.CompanyId)
                .HasMaxLength(10)
                .HasDefaultValue("AIR")
                .HasColumnName("CompanyID");
            entity.Property(e => e.Country).HasMaxLength(50);
            entity.Property(e => e.State).HasMaxLength(50);
            entity.Property(e => e.Text2).HasMaxLength(16);
            entity.Property(e => e.VendorName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.VendorNum)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Zip).HasMaxLength(12);
        });

        modelBuilder.Entity<TrkSoldItemHistory>(entity =>
        {
            entity.HasKey(e => e.RowId);

            entity.ToTable("trkSoldItemHistory");

            entity.Property(e => e.RowId).HasColumnName("rowID");
            entity.Property(e => e.DateMarkedBought)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PageName)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.RequestId)
                .HasDefaultValue(0)
                .HasColumnName("RequestID");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasDefaultValue("");
        });

        modelBuilder.Entity<TrkSonote>(entity =>
        {
            entity.ToTable("trkSONotes");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ContactId)
                .HasDefaultValue(0)
                .HasColumnName("ContactID");
            entity.Property(e => e.EnteredBy).HasMaxLength(50);
            entity.Property(e => e.EntryDate).HasColumnType("datetime");
            entity.Property(e => e.ModBy).HasMaxLength(50);
            entity.Property(e => e.ModDate).HasColumnType("datetime");
            entity.Property(e => e.NoteType)
                .HasMaxLength(25)
                .HasDefaultValue("");
            entity.Property(e => e.Notes).HasColumnType("ntext");
            entity.Property(e => e.OrderNo).HasMaxLength(50);
            entity.Property(e => e.PartNo).HasMaxLength(50);
        });

        modelBuilder.Entity<TrkUnshippedByItemNo>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("trkUnshippedByItemNo");

            entity.HasIndex(e => e.DateRecorded, "IX_trkUnshippedByItemNo_DateRecorded").HasFillFactor(80);

            entity.Property(e => e.AccountNum)
                .HasMaxLength(25)
                .HasDefaultValue("");
            entity.Property(e => e.DateRecorded).HasColumnType("datetime");
            entity.Property(e => e.ItemNum)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.QtyUnshipped).HasComputedColumnSql("([QtyOrdered]-[QtyShipped])", false);
            entity.Property(e => e.RowId)
                .ValueGeneratedOnAdd()
                .HasColumnName("rowID");
            entity.Property(e => e.SalesTeam)
                .HasMaxLength(25)
                .HasDefaultValue("");
            entity.Property(e => e.SoNo)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.UnitPrice)
                .HasDefaultValue(0m)
                .HasColumnType("money");
            entity.Property(e => e.UnshippedValue)
                .HasDefaultValue(0m)
                .HasColumnType("money");
        });

        modelBuilder.Entity<TrkUnshippedBySo>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("trkUnshippedBySo");

            entity.HasIndex(e => e.DateRecorded, "IX_trkUnshippedBySo_DateRec");

            entity.Property(e => e.DateRecorded).HasColumnType("datetime");
            entity.Property(e => e.SoNo)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.UnshippedValue).HasColumnType("money");
        });

        modelBuilder.Entity<TrkUnshippedValue>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("trkUnshippedValues");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.ShipDate).HasColumnType("datetime");
            entity.Property(e => e.UnshippedValue).HasDefaultValue(0.0);
        });

        modelBuilder.Entity<TrkUsage>(entity =>
        {
            entity.HasKey(e => e.RowId);

            entity.ToTable("trkUsage");

            entity.Property(e => e.RowId).HasColumnName("rowID");
            entity.Property(e => e.AppId)
                .HasDefaultValue(0)
                .HasColumnName("appID");
            entity.Property(e => e.EntryDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(50)
                .HasColumnName("IPAddress");
            entity.Property(e => e.Uname)
                .HasMaxLength(25)
                .HasColumnName("UName");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).IsClustered(false);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccessIds)
                .HasMaxLength(255)
                .HasDefaultValue("")
                .HasColumnName("AccessIDs");
            entity.Property(e => e.Active).HasDefaultValue((byte)1);
            entity.Property(e => e.ActiveSales).HasDefaultValue((byte)0);
            entity.Property(e => e.ActivityPref).HasMaxLength(20);
            entity.Property(e => e.ActivityReporting).HasDefaultValue((byte)0);
            entity.Property(e => e.CallReporting).HasDefaultValue((byte)0);
            entity.Property(e => e.CanSchedule).HasDefaultValue((byte)1);
            entity.Property(e => e.CompanyId)
                .HasDefaultValue(0)
                .HasColumnName("companyID");
            entity.Property(e => e.CrtDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DeptId)
                .HasDefaultValue(0)
                .HasColumnName("DeptID");
            entity.Property(e => e.DirectPhone)
                .HasMaxLength(12)
                .HasDefaultValue("");
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Email2).HasMaxLength(50);
            entity.Property(e => e.Email3).HasMaxLength(50);
            entity.Property(e => e.Extension)
                .HasMaxLength(10)
                .HasDefaultValue("");
            entity.Property(e => e.Fname)
                .HasMaxLength(50)
                .HasDefaultValue("First")
                .HasColumnName("FName");
            entity.Property(e => e.GroupId)
                .HasDefaultValue(0)
                .HasColumnName("groupID");
            entity.Property(e => e.Hdpw)
                .HasMaxLength(20)
                .HasDefaultValue("1234")
                .HasColumnName("HDPW");
            entity.Property(e => e.InProd)
                .HasDefaultValue((byte)0)
                .HasColumnName("inProd");
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(20)
                .HasColumnName("IPAddress");
            entity.Property(e => e.IsMgr)
                .HasDefaultValue((byte)0)
                .HasColumnName("isMgr");
            entity.Property(e => e.JobTitle).HasMaxLength(50);
            entity.Property(e => e.LastLogin).HasColumnType("datetime");
            entity.Property(e => e.Lname)
                .HasMaxLength(50)
                .HasDefaultValue("Last")
                .HasColumnName("LName");
            entity.Property(e => e.LocationId)
                .HasDefaultValue(0)
                .HasColumnName("LocationID");
            entity.Property(e => e.LoginCnt).HasDefaultValue(0);
            entity.Property(e => e.MachineName).HasMaxLength(50);
            entity.Property(e => e.MgrId)
                .HasDefaultValue(0)
                .HasColumnName("MgrID");
            entity.Property(e => e.Mname)
                .HasMaxLength(50)
                .HasColumnName("MName");
            entity.Property(e => e.MobilePhone).HasMaxLength(12);
            entity.Property(e => e.ModDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NewPortal)
                .HasDefaultValue(0)
                .HasColumnName("newPortal");
            entity.Property(e => e.OnHdstaff)
                .HasDefaultValue((byte)0)
                .HasColumnName("onHDstaff");
            entity.Property(e => e.RwTeamName)
                .HasMaxLength(10)
                .HasColumnName("rwTeamName");
            entity.Property(e => e.RwUserId)
                .HasMaxLength(6)
                .HasDefaultValue("")
                .HasColumnName("rwUserID");
            entity.Property(e => e.SalesTeam)
                .HasMaxLength(50)
                .HasDefaultValue("n/a");
            entity.Property(e => e.SalesTeam2)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.SalesTeam3)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.SalesTeam4)
                .HasMaxLength(50)
                .HasComment("Uses for reporting purposes for reps associated with multiple accounts");
            entity.Property(e => e.SavePw)
                .HasDefaultValue((byte)0)
                .HasColumnName("SavePW");
            entity.Property(e => e.SkillLevel).HasMaxLength(50);
            entity.Property(e => e.StartPage)
                .HasMaxLength(255)
                .HasDefaultValue("");
            entity.Property(e => e.StartPage2)
                .HasMaxLength(255)
                .HasDefaultValue("");
            entity.Property(e => e.StartPage3)
                .HasMaxLength(255)
                .HasDefaultValue("");
            entity.Property(e => e.StartPage4)
                .HasMaxLength(255)
                .HasDefaultValue("");
            entity.Property(e => e.StartPage5)
                .HasMaxLength(255)
                .HasDefaultValue("");
            entity.Property(e => e.TeamGroup)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TimeClock).HasDefaultValue(0);
            entity.Property(e => e.Uname)
                .HasMaxLength(20)
                .HasColumnName("UName");
            entity.Property(e => e.ZoomPref).HasDefaultValue(100);
        });

        modelBuilder.Entity<UserPortalMenu>(entity =>
        {
            entity.HasKey(e => e.RowId);

            entity.ToTable("UserPortalMenu");

            entity.Property(e => e.RowId).HasColumnName("rowID");
            entity.Property(e => e.AppId)
                .HasDefaultValue(0)
                .HasColumnName("AppID");
            entity.Property(e => e.DeptId)
                .HasDefaultValue(0)
                .HasColumnName("DeptID");
            entity.Property(e => e.EntryDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MenuType).HasMaxLength(25);
            entity.Property(e => e.UserId)
                .HasDefaultValue(0)
                .HasColumnName("UserID");
        });

        modelBuilder.Entity<UserPref>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.DefaultPref).HasDefaultValue((byte)0);
            entity.Property(e => e.PrefData)
                .HasMaxLength(2048)
                .IsUnicode(false);
            entity.Property(e => e.PrefName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PrefType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RowId)
                .ValueGeneratedOnAdd()
                .HasColumnName("rowID");
            entity.Property(e => e.UserName)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
