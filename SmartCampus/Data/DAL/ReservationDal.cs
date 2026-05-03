using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SmartCampus.Models;

namespace SmartCampus.Data.DAL;

public class ReservationDal(AppDbContext db)
{
    public async Task<PaginatedList<TodayReservationVm>> GetTodayReservationsAsync(int pageIndex = 1, int pageSize = 10, string? searchString = null, string? statusFilter = null)
    {
        var query = db.Set<TodayReservationVm>().AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            query = query.Where(r => r.FacilityName.Contains(searchString) || r.Organizer.Contains(searchString));
        }

        if (!string.IsNullOrEmpty(statusFilter))
        {
            query = query.Where(r => r.StatusName == statusFilter);
        }

        return await PaginatedList<TodayReservationVm>.CreateAsync(query, pageIndex, pageSize);
    }

    public async Task<List<FreeSlotVm>> GetFreeSlotsAsync(int facilityId, DateOnly date)
    {
        var slots = await db.Set<FreeSlotVm>()
            .FromSqlInterpolated($"EXEC sp_FindFreeSlots {facilityId}, {date.ToString("yyyy-MM-dd")}")
            .ToListAsync();

        // Bugünün geçmiş saatlerini filtrele
        if (date == DateOnly.FromDateTime(DateTime.Today))
            slots = slots.Where(s => s.SlotStart > DateTime.Now).ToList();

        return slots;
    }

    public async Task<List<AlternativeVm>> GetAlternativesAsync(string typeName, DateTime start, DateTime end)
    {
        var typeId = await db.FacilityTypes
            .Where(t => t.TypeName == typeName)
            .Select(t => t.TypeID)
            .FirstOrDefaultAsync();

        return await db.Set<AlternativeVm>()
            .FromSqlInterpolated($"EXEC sp_SuggestAlternatives {typeId}, {start}, {end}")
            .ToListAsync();
    }

    public async Task<(bool ok, string error)> BookAsync(int facilityId, int userId, DateTime start, DateTime end)
    {
        // Geçmiş saate rezervasyon engeli
        if (start <= DateTime.Now)
            return (false, "Geçmiş bir saate rezervasyon yapamazsınız.");

        try
        {
            var pendingId = await db.Statuses.Where(s => s.StatusName == "Pending").Select(s => s.StatusID).FirstAsync();

            // Rezervasyonu oluştur ve organizatörü otomatik katılımcı olarak ekle
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                DECLARE @NewID BIGINT;
                INSERT INTO Reservations (FacilityID, OrganizerUserID, StatusID, StartTime, EndTime, CreatedAt)
                VALUES ({facilityId}, {userId}, {pendingId}, {start}, {end}, GETDATE());
                SET @NewID = SCOPE_IDENTITY();
                INSERT INTO ReservationAttendees (ReservationID, UserID, IsAttended, CheckedInAt)
                VALUES (@NewID, {userId}, 0, NULL);
                """);

            return (true, "");
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException?.Message ?? ex.Message;
            return (false, msg.Contains("blocked") ? "Çok fazla no-show kaydınız var, yeni rezervasyon yapamayabilirsiniz." : msg);
        }
    }

    public async Task CancelAsync(long reservationId, int? userId)
    {
        var cancelledId = await db.Statuses.Where(s => s.StatusName == "Cancelled").Select(s => s.StatusID).FirstAsync();
        await db.Reservations
            .Where(r => r.ReservationID == reservationId && (userId == null || r.OrganizerUserID == userId))
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.StatusID, cancelledId).SetProperty(r => r.CancelledAt, DateTime.Now));
    }

    public async Task ApproveAsync(long reservationId)
    {
        var approvedId = await db.Statuses.Where(s => s.StatusName == "Approved").Select(s => s.StatusID).FirstAsync();
        await db.Reservations.Where(r => r.ReservationID == reservationId)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.StatusID, approvedId));
    }

    public async Task<string?> GetFacilityNameAsync(int id) =>
        await db.Facilities.Where(f => f.FacilityID == id).Select(f => f.FacilityName).FirstOrDefaultAsync();

    public async Task<UserDashboardVm> GetUserDashboardAsync(int userId)
    {
        var vm = new UserDashboardVm();
        using var cmd = db.Database.GetDbConnection().CreateCommand();
        cmd.CommandText = "EXEC sp_GetUserDashboard @UserID";
        cmd.Parameters.Add(new SqlParameter("@UserID", userId));

        if (cmd.Connection!.State == ConnectionState.Closed) await cmd.Connection.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();

        // 1. Result: Upcoming Reservations
        while (await reader.ReadAsync())
        {
            vm.Upcoming.Add(new UpcomingReservationVm
            {
                ReservationID = reader.GetInt64("ReservationID"),
                FacilityName = reader.GetString("FacilityName"),
                StartTime = reader.GetDateTime("StartTime"),
                EndTime = reader.GetDateTime("EndTime"),
                StatusName = reader.GetString("StatusName")
            });
        }

        // 2. Result: Stats
        if (await reader.NextResultAsync() && await reader.ReadAsync())
        {
            vm.TotalBookings = reader.GetInt32("TotalBookings");
            vm.Attended = reader.IsDBNull("Attended") ? 0 : reader.GetInt32("Attended");
            vm.Missed = reader.IsDBNull("Missed") ? 0 : reader.GetInt32("Missed");
            vm.AttendanceRate = reader.IsDBNull("AttendanceRate") ? 0 : reader.GetDecimal("AttendanceRate");
        }

        // 3. Result: Penalties
        if (await reader.NextResultAsync() && await reader.ReadAsync())
        {
            vm.ActivePenalties = reader.GetInt32("ActivePenalties");
            vm.TotalPenaltyPoints = reader.IsDBNull("TotalPoints") ? 0 : reader.GetInt32("TotalPoints");
            if (!reader.IsDBNull("EarliestExpiry"))
                vm.EarliestPenaltyExpiry = reader.GetDateTime("EarliestExpiry");
        }

        // 4. Result: Top Available
        if (await reader.NextResultAsync())
        {
            while (await reader.ReadAsync())
            {
                vm.TopAvailable.Add(new LiveOccupancyVm
                {
                    FacilityName = reader.GetString("FacilityName"),
                    ZoneName = reader.GetString("ZoneName"),
                    OccupancyPct = reader.GetDecimal("OccupancyPct"),
                    CrowdLevel = reader.GetString("CrowdLevel")
                });
            }
        }

        return vm;
    }
}
