using System;
using System.Collections.Generic;

namespace AirwayAPI.Models;

public partial class CamContact
{
    public int Id { get; set; }

    public int? OldContactId { get; set; }

    public int? OldCompanyId { get; set; }

    public string? GmaccountNum { get; set; }

    public string? Company { get; set; }

    public string? Contact { get; set; }

    public string? CompanyId { get; set; }

    public string? Lastname { get; set; }

    public string? Title { get; set; }

    public string? Source { get; set; }

    public string? Dear { get; set; }

    public string? Assistant { get; set; }

    public string? PhoneMain { get; set; }

    public string? Extension { get; set; }

    public string? PhoneDirect { get; set; }

    public string? PhoneCell { get; set; }

    public string? Fax { get; set; }

    public string? Email { get; set; }

    public string? Website { get; set; }

    public string? Other1 { get; set; }

    public string? Other2 { get; set; }

    public string? Other3 { get; set; }

    public string? Other4 { get; set; }

    public string? Label1 { get; set; }

    public string? Label2 { get; set; }

    public string? Label3 { get; set; }

    public string? Label4 { get; set; }

    public string? Birthday { get; set; }

    public DateTime? BirthDate { get; set; }

    public string? Spouse { get; set; }

    public string? Kids { get; set; }

    public string? Interests { get; set; }

    public string? Address1 { get; set; }

    public string? Address2 { get; set; }

    public string? Address3 { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Zip { get; set; }

    public string? Country { get; set; }

    public string? AccountExec { get; set; }

    public string? AccountMgr { get; set; }

    public string? AccountCoord { get; set; }

    public string? AccountOther { get; set; }

    public string? ContactType { get; set; }

    public string? SalesTeam { get; set; }

    public string? AcctReps { get; set; }

    public string? RwaccountNum { get; set; }

    /// <summary>
    /// the RW account number for PO&apos;s
    /// </summary>
    public string? PoaccountNum { get; set; }

    public byte? ActiveStatus { get; set; }

    public int? Terms { get; set; }

    public int? WarrantyTerms { get; set; }

    public byte? EFlyer { get; set; }

    public string? HolidayGift { get; set; }

    public bool? Gift1 { get; set; }

    public bool? Gift2 { get; set; }

    public bool? Competitor { get; set; }

    public bool? Fnecard { get; set; }

    public bool? Anccard { get; set; }

    public bool? MainVendor { get; set; }

    public int? NumOfSites { get; set; }

    public DateTime? EntryDate { get; set; }

    public string? EnteredBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public string? SContactIds { get; set; }

    public string? SCompanyIds { get; set; }

    public string? Mfgs { get; set; }

    public string? VerizonArea { get; set; }

    public string? VerizonRegion { get; set; }

    public string? VerizonMarket { get; set; }

    public int? Btscount { get; set; }

    public DateTime? BtstimeStamp { get; set; }

    public int? ClauseId { get; set; }

    public byte? TempKatietoTimG { get; set; }

    public string? GeneralInfo { get; set; }
}
