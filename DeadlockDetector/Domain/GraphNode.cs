namespace DeadlockDetector.Domain;

public class GraphNode
{
    public string Id { get; set; } = "";
    public string Label { get; set; } = "";
    public string Type { get; set; } = "";
    public float X { get; set; }
    public float Y { get; set; }
    public bool IsDeadlocked { get; set; }
}