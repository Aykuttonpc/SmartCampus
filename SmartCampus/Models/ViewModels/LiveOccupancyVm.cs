namespace SmartCampus.Models;

public class LiveOccupancyVm
{
    public int FacilityID { get; set; }
    public string FacilityName { get; set; } = "";
    public string ZoneName { get; set; } = "";
    public string FacilityType { get; set; } = "";
    public int MaxCapacity { get; set; }
    public int LiveCount { get; set; }
    public decimal OccupancyPct { get; set; }
    public DateTime? LastUpdated { get; set; }
    public string CrowdLevel { get; set; } = "";
    public bool IsReservable { get; set; }
}
