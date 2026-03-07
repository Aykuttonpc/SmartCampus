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
}
