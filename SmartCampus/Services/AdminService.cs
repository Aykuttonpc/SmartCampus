using Microsoft.EntityFrameworkCore;
using SmartCampus.Data;
using SmartCampus.Data.DAL;
using SmartCampus.Models;

namespace SmartCampus.Services;

public class AdminService(AppDbContext db, AdminDal dal)
{
    public async Task<List<NoShowReportVm>> GetNoShowReportAsync() => await dal.GetNoShowReportAsync();

    public async Task<List<Facility>> GetAllFacilitiesAsync() => 
        await db.Facilities.Include(f => f.Zone).Include(f => f.FacilityType).OrderBy(f => f.FacilityName).ToListAsync();

    public async Task UpsertFacilityAsync(Facility f)
    {
        if (f.FacilityID == 0) db.Facilities.Add(f);
        else db.Facilities.Update(f);
        await db.SaveChangesAsync();
    }

    public async Task DeleteFacilityAsync(int id) => await dal.DeleteFacilityAsync(id);

    public async Task<List<UserRoleVm>> GetAllUsersWithRolesAsync() => await dal.GetAllUsersWithRolesAsync();

    public async Task UpdateUserRoleAsync(int userId, string newRoleName) => await dal.UpdateUserRoleAsync(userId, newRoleName);
}
