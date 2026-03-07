namespace SmartCampus.Models;

public class OccupancyLog
{
    public long LogID { get; set; }
    public int SensorID { get; set; }
    public Sensor Sensor { get; set; } = null!;
    public DateTime LoggedAt { get; set; }
    public int CurrentCount { get; set; }
}
