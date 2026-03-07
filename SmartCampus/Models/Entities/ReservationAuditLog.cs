namespace SmartCampus.Models;

public class ReservationAuditLog
{
    public long AuditID { get; set; }
    public long ReservationID { get; set; }
    public Reservation Reservation { get; set; } = null!;
    public int? OldStatusID { get; set; }
    public int? NewStatusID { get; set; }
    public DateTime ChangedAt { get; set; }
}
