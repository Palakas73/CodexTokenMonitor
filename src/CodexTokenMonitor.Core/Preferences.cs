namespace CodexTokenMonitor.Core;

public enum VisualMode { Digital, Gauge, Rpg }
public enum QuotaSelection { FiveHours, Weekly, Both }
public sealed record Preferences
{
    public double Left { get; init; } = 80;
    public double Top { get; init; } = 80;
    public double Width { get; init; } = 340;
    public double Height { get; init; } = 410;
    public VisualMode Mode { get; init; }
    public QuotaSelection Quota { get; init; } = QuotaSelection.Both;
    public double Opacity { get; init; } = 1;
    public bool TransparentBackground { get; init; }
    public int RefreshSeconds { get; init; } = 120;
    public int TimeoutSeconds { get; init; } = 30;
    public bool AutoStart { get; init; }
    public Preferences Normalize() => this with {
        Left = double.IsFinite(Left) ? Left : 80, Top = double.IsFinite(Top) ? Top : 80,
        Width = double.IsFinite(Width) ? Math.Clamp(Width, 260, 900) : 340,
        Height = double.IsFinite(Height) ? Math.Clamp(Height, 220, 900) : 410,
        Opacity = double.IsFinite(Opacity) ? Math.Clamp(Opacity, .3, 1) : 1,
        Mode = Enum.IsDefined(Mode) ? Mode : VisualMode.Digital,
        Quota = Enum.IsDefined(Quota) ? Quota : QuotaSelection.Both,
        RefreshSeconds = Math.Clamp(RefreshSeconds, 15, 3600), TimeoutSeconds = Math.Clamp(TimeoutSeconds, 3, 120)
    };
}

public readonly record struct ScreenArea(double X, double Y, double Width, double Height);
public static class WindowPlacement
{
    public static ScreenArea Fit(ScreenArea window, IReadOnlyList<ScreenArea> screens)
    {
        if (screens.Count == 0) return window;
        var screen = screens.OrderByDescending(s => Math.Max(0, Math.Min(window.X + window.Width, s.X + s.Width) - Math.Max(window.X, s.X)) * Math.Max(0, Math.Min(window.Y + window.Height, s.Y + s.Height) - Math.Max(window.Y, s.Y)))
            .ThenBy(s => Math.Pow(window.X - s.X, 2) + Math.Pow(window.Y - s.Y, 2)).First();
        var width = Math.Min(window.Width, screen.Width); var height = Math.Min(window.Height, screen.Height);
        return new(Math.Clamp(window.X, screen.X, screen.X + screen.Width - width), Math.Clamp(window.Y, screen.Y, screen.Y + screen.Height - height), width, height);
    }
}
