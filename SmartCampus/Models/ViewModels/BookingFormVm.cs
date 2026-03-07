namespace SmartCampus.Models;

public class BookingFormVm
{
    public int FacilityID { get; set; }
    public string FacilityName { get; set; } = "";
    public DateTime SlotStart { get; set; }
    public DateTime SlotEnd { get; set; }
}
