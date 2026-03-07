namespace SmartCampus.Models;

public class TodayReservationVm
{
    public long ReservationID { get; set; }
    public string FacilityName { get; set; } = "";
    public string Organizer { get; set; } = "";
    public string StatusName { get; set; } = "";
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int DurationMin { get; set; }
    public int AttendeeCount { get; set; }
    public int OrganizerUserID { get; set; }
}
