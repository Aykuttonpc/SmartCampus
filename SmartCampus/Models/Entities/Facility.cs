namespace SmartCampus.Models;

public class Facility
{
    public int FacilityID { get; set; }
    public int ZoneID { get; set; }
    public Zone Zone { get; set; } = null!;
    public int TypeID { get; set; }
    public FacilityType FacilityType { get; set; } = null!;
    public string FacilityName { get; set; } = "";
    public int MaxCapacity { get; set; }
    public bool IsReservable { get; set; } = true;
    public bool IsOperational { get; set; } = true;
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }
    public ICollection<Sensor> Sensors { get; set; } = [];
    public ICollection<Reservation> Reservations { get; set; } = [];
}
