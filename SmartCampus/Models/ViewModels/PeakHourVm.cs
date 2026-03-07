namespace SmartCampus.Models;

public class PeakHourVm
{
    public int HourOfDay { get; set; }
    public int AvgOccupancy { get; set; }
    public int PeakOccupancy { get; set; }
    public int BusiestRank { get; set; }
}
