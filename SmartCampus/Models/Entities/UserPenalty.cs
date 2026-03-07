namespace SmartCampus.Models;

public class UserPenalty
{
    public int PenaltyID { get; set; }
    public int UserID { get; set; }
    public User User { get; set; } = null!;
    public long ReservationID { get; set; }
    public Reservation Reservation { get; set; } = null!;
    public int PenaltyPoints { get; set; } = 1;
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
}
