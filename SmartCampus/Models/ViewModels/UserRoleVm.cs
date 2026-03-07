namespace SmartCampus.Models;

public class UserRoleVm
{
    public int UserID { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string RoleName { get; set; } = "";
}
