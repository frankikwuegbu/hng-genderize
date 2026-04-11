namespace hng_genderizeApp.Entity;

public class ClassifiedName
{
    public string? Name { get; set; }
    public string? Gender { get; set; }
    public float? Probability { get; set; }
    public int Sample_Size { get; set; }
    public bool IsConfident { get; set; }
    public DateTime? ProcessedAt { get; set; } = DateTime.UtcNow;
}
