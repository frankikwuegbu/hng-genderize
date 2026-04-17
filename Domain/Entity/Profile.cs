namespace Domain.Entity;

public class Profile
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Name { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public float GenderProbability { get; set; }
    public int SampleSize { get; set; }
    public int Age { get; set; }
    public string AgeGroup { get; set; } = string.Empty;
    public string CountryId { get; set; } = string.Empty;
    public float CountryProbability { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
