namespace AirwayAPI.Models.UserListModels;

public class UserUpdateDto
{
    public int Id { get; set; }
    public string? Uname { get; set; }
    public string? Fname { get; set; }
    public string? Mname { get; set; }
    public string? Lname { get; set; }
    public string? JobTitle { get; set; }
    public string? Email { get; set; }
    public string? Extension { get; set; }
    public string? DirectPhone { get; set; }
    public string? MobilePhone { get; set; }
}
