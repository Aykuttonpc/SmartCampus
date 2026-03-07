namespace SmartCampus.Models;

public class Reservation
{
    public long ReservationID { get; set; }
    public int FacilityID { get; set; }
    public Facility Facility { get; set; } = null!;
    public int OrganizerUserID { get; set; }
    public User Organizer { get; set; } = null!;
    public int StatusID { get; set; }
    public Status Status { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public ICollection<ReservationAttendee> Attendees { get; set; } = [];
    public ICollection<ReservationAuditLog> AuditLogs { get; set; } = [];
}
