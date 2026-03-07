namespace SmartCampus.Models;

public class NoShowReportVm
{
    public int UserID { get; set; }
    public string FullName { get; set; } = "";
    public int TotalBookings { get; set; }
    public int AttendedCount { get; set; }
    public int MissedCount { get; set; }
    public decimal NoShowRate { get; set; }
    public string RiskLabel { get; set; } = "";
}
