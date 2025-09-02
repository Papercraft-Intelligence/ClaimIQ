public class RolloutStrategy
{
    // incremental strategy
    public int Percentage { get; set; } = 100;

    // user-based strategy
    public List<string> UserIds { get; set; } = new();

    // segment
    public List<string> Segments { get; set; } = new();

    // geography
    public List<string> Geographies { get; set; } = new();

    // custom
    public Dictionary<string, object> CustomRules { get; set; } = new();

    // temporal
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }  
}
