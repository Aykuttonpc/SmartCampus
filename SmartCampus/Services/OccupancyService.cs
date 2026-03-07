using SmartCampus.Data.DAL;
using SmartCampus.Models;

namespace SmartCampus.Services;

public class OccupancyService(OccupancyDal dal)
{
    public async Task<List<LiveOccupancyVm>> GetLiveHeatmapAsync() => await dal.GetLiveHeatmapAsync();

    public async Task<List<PeakHourVm>> GetPeakHoursAsync(int facilityId) => await dal.GetPeakHoursAsync(facilityId);
}
