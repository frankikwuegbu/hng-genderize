namespace Domain.Entity;

public class Profile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Name { get; set; }
    public string? Gender { get; set; }
    public float? GenderProbability { get; set; }
    public int Sample_Size { get; set; }
    public int Age { get; set; }
    public string? AgeGroup { get; set; }
    public string? CountryId { get; set; }
    public float CountryProbability { get; set; }
    public DateTime? ProcessedAt { get; set; } = DateTime.UtcNow;
}
