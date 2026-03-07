namespace SmartCampus.Models;

public class Status
{
    public int StatusID { get; set; }
    public string StatusName { get; set; } = "";
    public ICollection<Reservation> Reservations { get; set; } = [];
}
