namespace SmartCampus.Models;

public class ReservationAttendee
{
    public long ReservationID { get; set; }
    public Reservation Reservation { get; set; } = null!;
    public int UserID { get; set; }
    public User User { get; set; } = null!;
    public bool IsAttended { get; set; }
    public DateTime? CheckedInAt { get; set; }
}
