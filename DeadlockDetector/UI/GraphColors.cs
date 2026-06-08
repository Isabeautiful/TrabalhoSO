using Color = Cairo.Color;

namespace DeadlockDetector.UI;

public static class GraphColors
{
    public static Color ProcessColor { get; } = new Color(0.13, 0.59, 0.95);
    public static Color ResourceColor { get; } = new Color(0.30, 0.69, 0.31);
    public static Color DeadlockColor { get; } = new Color(0.96, 0.26, 0.21);
    public static Color RequestColor { get; } = new Color(0.96, 0.26, 0.21);
    public static Color AllocationColor { get; } = new Color(0.30, 0.69, 0.31);
    public static Color TextColor { get; } = new Color(0.86, 0.86, 0.86);
    public static Color BackgroundColor { get; } = new Color(0.12, 0.12, 0.12);
}