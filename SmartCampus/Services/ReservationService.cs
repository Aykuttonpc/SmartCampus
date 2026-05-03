using SmartCampus.Data.DAL;
using SmartCampus.Models;

namespace SmartCampus.Services;

public class ReservationService(ReservationDal dal)
{
    public async Task<PaginatedList<TodayReservationVm>> GetTodayReservationsAsync(int pageIndex = 1, int pageSize = 10, string? searchString = null, string? statusFilter = null) => await dal.GetTodayReservationsAsync(pageIndex, pageSize, searchString, statusFilter);

    public async Task<List<FreeSlotVm>> GetFreeSlotsAsync(int facilityId, DateOnly date) => await dal.GetFreeSlotsAsync(facilityId, date);

    public async Task<List<AlternativeVm>> GetAlternativesAsync(string typeName, DateTime start, DateTime end) => await dal.GetAlternativesAsync(typeName, start, end);

    public async Task<(bool ok, string error)> BookAsync(int facilityId, int userId, DateTime start, DateTime end) => await dal.BookAsync(facilityId, userId, start, end);

    public async Task CancelAsync(long reservationId, int? userId) => await dal.CancelAsync(reservationId, userId);

    public async Task ApproveAsync(long reservationId) => await dal.ApproveAsync(reservationId);

    public async Task<UserDashboardVm> GetUserDashboardAsync(int userId) => await dal.GetUserDashboardAsync(userId);
    public async Task<string?> GetFacilityNameAsync(int id) => await dal.GetFacilityNameAsync(id);
}
