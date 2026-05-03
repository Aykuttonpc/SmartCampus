using Microsoft.EntityFrameworkCore;
using SmartCampus.Data;
using SmartCampus.Data.DAL;
using SmartCampus.Models;

namespace SmartCampus.Services;

public class AdminService(AppDbContext db, AdminDal dal)
{
    public async Task<List<NoShowReportVm>> GetNoShowReportAsync() => await dal.GetNoShowReportAsync();

    public async Task<PaginatedList<Facility>> GetAllFacilitiesAsync(int pageIndex, int pageSize, string? searchString) 
    {
        var query = db.Facilities.Include(f => f.Zone).Include(f => f.FacilityType).AsQueryable();
        if (!string.IsNullOrEmpty(searchString))
        {
            query = query.Where(f => f.FacilityName.Contains(searchString));
        }

        var ordered = query.OrderBy(f => f.FacilityName);
        return await PaginatedList<Facility>.CreateAsync(ordered, pageIndex, pageSize);
    }

    public async Task UpsertFacilityAsync(Facility f)
    {
        if (f.FacilityID == 0) db.Facilities.Add(f);
        else db.Facilities.Update(f);
        await db.SaveChangesAsync();
    }

    public async Task DeleteFacilityAsync(int id) => await dal.DeleteFacilityAsync(id);

    public async Task<PaginatedList<UserRoleVm>> GetAllUsersWithRolesAsync(int pageIndex, int pageSize, string? searchString, string? roleFilter) => await dal.GetAllUsersWithRolesAsync(pageIndex, pageSize, searchString, roleFilter);

    public async Task UpdateUserRoleAsync(int userId, string newRoleName) => await dal.UpdateUserRoleAsync(userId, newRoleName);
}
