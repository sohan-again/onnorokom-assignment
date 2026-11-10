namespace SchoolAdmission.Api.Domain;

public class SchoolClass
{
    public string Id { get; set; } = "c_" + Guid.NewGuid().ToString("N")[..12];
    public string Name { get; set; } = "";
    public int TotalSeats { get; set; }
}