namespace AirwayAPI.Models;

public partial class User
{
    public int Id { get; set; }

    public int? CompanyId { get; set; }

    public int? GroupId { get; set; }

    public string? TeamGroup { get; set; }

    public byte? InProd { get; set; }

    public byte? Active { get; set; }

    public byte? ActiveSales { get; set; }

    public byte? CanSchedule { get; set; }

    public byte? CallReporting { get; set; }

    public byte? ActivityReporting { get; set; }

    public string? SalesTeam { get; set; }

    public string? SalesTeam2 { get; set; }

    public string? SalesTeam3 { get; set; }

    /// <summary>
    /// Uses for reporting purposes for reps associated with multiple accounts
    /// </summary>
    public string? SalesTeam4 { get; set; }

    public string? RwTeamName { get; set; }

    public string? RwUserId { get; set; }

    public string? Fname { get; set; }

    public string? Lname { get; set; }

    public string? Mname { get; set; }

    public string? Uname { get; set; }

    public string? Hdpw { get; set; }

    public string? JobTitle { get; set; }

    public int? DeptId { get; set; }

    public int? MgrId { get; set; }

    public string? SkillLevel { get; set; }

    public string? Email { get; set; }

    public string? Email2 { get; set; }

    public string? Email3 { get; set; }

    public int? LocationId { get; set; }

    public int? ZoomPref { get; set; }

    public string? ActivityPref { get; set; }

    public byte? IsMgr { get; set; }

    public byte? OnHdstaff { get; set; }

    public DateTime? CrtDate { get; set; }

    public DateTime? ModDate { get; set; }

    public DateTime? LastLogin { get; set; }

    public int? LoginCnt { get; set; }

    public byte? SavePw { get; set; }

    public string? MachineName { get; set; }

    public int? NewPortal { get; set; }

    public int? TimeClock { get; set; }

    public string? AccessIds { get; set; }

    public string? StartPage { get; set; }

    public string? StartPage2 { get; set; }

    public string? StartPage3 { get; set; }

    public string? StartPage4 { get; set; }

    public string? StartPage5 { get; set; }

    public string? Ipaddress { get; set; }

    public string? Extension { get; set; }

    public string? MobilePhone { get; set; }

    public string? DirectPhone { get; set; }

    public virtual ICollection<PortalUserConfig> PortalUserConfigs { get; set; } = new List<PortalUserConfig>();

    public virtual ICollection<PortalUserFavorite> PortalUserFavorites { get; set; } = new List<PortalUserFavorite>();
}
