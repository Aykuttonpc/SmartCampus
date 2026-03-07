namespace SmartCampus.Models;

public class AlternativeVm
{
    public int FacilityID { get; set; }
    public string FacilityName { get; set; } = "";
    public string ZoneName { get; set; } = "";
    public int MaxCapacity { get; set; }
    public int CurrentOccupancy { get; set; }
    public decimal OccupancyPct { get; set; }
    public long SuggestionRank { get; set; }
}
