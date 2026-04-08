using Microsoft.EntityFrameworkCore;
using SmartCampus.Models;

namespace SmartCampus.Data.DAL;

public class AdminDal(AppDbContext db)
{
    public async Task<List<NoShowReportVm>> GetNoShowReportAsync()
    {
        return await db.Set<NoShowReportVm>()
            .FromSqlRaw("EXEC sp_GetNoShowReport")
            .ToListAsync();
    }

    public async Task DeleteFacilityAsync(int id)
    {
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM Facilities WHERE FacilityID = {id}");
    }

    public async Task<PaginatedList<UserRoleVm>> GetAllUsersWithRolesAsync(int pageIndex, int pageSize)
    {
        // Get all users and their single assigned role. (System assumes 1 role per user).
        var query = db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .OrderBy(u => u.FirstName)
            .Select(u => new UserRoleVm
            {
                UserID = u.UserID,
                FullName = string.IsNullOrEmpty(u.FullName) ? u.FirstName + " " + u.LastName : u.FullName,
                Email = u.Email,
                RoleName = u.UserRoles.FirstOrDefault()!.Role.RoleName
            });
            
        return await PaginatedList<UserRoleVm>.CreateAsync(query, pageIndex, pageSize);
    }

    public async Task UpdateUserRoleAsync(int userId, string newRoleName)
    {
        var roleId = await db.Roles.Where(r => r.RoleName == newRoleName).Select(r => r.RoleID).FirstOrDefaultAsync();
        if (roleId == 0) return;

        // Remove old roles
        var existingRoles = await db.UserRoles.Where(ur => ur.UserID == userId).ToListAsync();
        db.UserRoles.RemoveRange(existingRoles);
        
        // Add new role
        db.UserRoles.Add(new UserRole { UserID = userId, RoleID = roleId });
        await db.SaveChangesAsync();
    }
}
