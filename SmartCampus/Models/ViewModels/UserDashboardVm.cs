namespace SmartCampus.Models;

public class UserDashboardVm
{
    public List<UpcomingReservationVm> Upcoming { get; set; } = [];
    public int TotalBookings { get; set; }
    public int Attended { get; set; }
    public int Missed { get; set; }
    public decimal AttendanceRate { get; set; }
    public int ActivePenalties { get; set; }
    public int TotalPenaltyPoints { get; set; }
    public DateTime? EarliestPenaltyExpiry { get; set; }
    public List<LiveOccupancyVm> TopAvailable { get; set; } = [];
}
