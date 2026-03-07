namespace SmartCampus.Models;

public class Zone
{
    public int ZoneID { get; set; }
    public string ZoneName { get; set; } = "";
    public string? Building { get; set; }
    public int? FloorNo { get; set; }
    public int? ParentZoneID { get; set; }
    public Zone? Parent { get; set; }
    public ICollection<Zone> Children { get; set; } = [];
    public ICollection<Facility> Facilities { get; set; } = [];
}
