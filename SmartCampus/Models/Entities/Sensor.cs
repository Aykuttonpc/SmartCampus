namespace SmartCampus.Models;

public class Sensor
{
    public int SensorID { get; set; }
    public int FacilityID { get; set; }
    public Facility Facility { get; set; } = null!;
    public string SensorMac { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public ICollection<OccupancyLog> OccupancyLogs { get; set; } = [];
}
