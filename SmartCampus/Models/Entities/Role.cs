namespace SmartCampus.Models;

public class Role
{
    public int RoleID { get; set; }
    public string RoleName { get; set; } = "";
    public ICollection<UserRole> UserRoles { get; set; } = [];
}
