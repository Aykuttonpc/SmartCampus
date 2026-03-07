namespace SmartCampus.Models;

public class UpcomingReservationVm
{
    public long ReservationID { get; set; }
    public string FacilityName { get; set; } = "";
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string StatusName { get; set; } = "";
}
