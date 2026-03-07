namespace SmartCampus.Models;

public class User
{
    public int UserID { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? FullName { get; set; }   // computed, read-only
    public ICollection<UserRole> UserRoles { get; set; } = [];
    public ICollection<Reservation> Reservations { get; set; } = [];
    public ICollection<ReservationAttendee> Attendances { get; set; } = [];
    public ICollection<UserPenalty> Penalties { get; set; } = [];
}
