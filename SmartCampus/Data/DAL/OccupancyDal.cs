using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SmartCampus.Models;

namespace SmartCampus.Data.DAL;

public class OccupancyDal(AppDbContext db)
{
    public async Task<List<LiveOccupancyVm>> GetLiveHeatmapAsync()
    {
        return await db.Set<LiveOccupancyVm>()
            .FromSqlRaw("EXEC sp_GetLiveHeatmap")
            .ToListAsync();
    }

    public async Task<List<PeakHourVm>> GetPeakHoursAsync(int facilityId)
    {
        return await db.Set<PeakHourVm>()
            .FromSqlInterpolated($"EXEC sp_GetPeakHours {facilityId}")
            .ToListAsync();
    }
}
