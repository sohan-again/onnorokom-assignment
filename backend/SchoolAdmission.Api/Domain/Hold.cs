namespace SchoolAdmission.Api.Domain;

public class Hold
{
    public string Id { get; set; } = "h_" + Guid.NewGuid().ToString("N")[..12];
    public string ClassId { get; set; } = "";
    public SchoolClass Class { get; set; } = null!;
    public string StudentId { get; set; } = "";
    public string Status { get; set; } = HoldStatuses.Held;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}