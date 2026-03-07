namespace SmartCampus.Models;

public class FacilityType
{
    public int TypeID { get; set; }
    public string TypeName { get; set; } = "";
    public ICollection<Facility> Facilities { get; set; } = [];
}
