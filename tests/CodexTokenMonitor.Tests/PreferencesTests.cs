using CodexTokenMonitor.Core;
using CodexTokenMonitor.Infrastructure;
using Xunit;
namespace CodexTokenMonitor.Tests;
public sealed class PreferencesTests
{
    [Fact] public void PreferencesRoundTripAndAtomicReplacement()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        try { var store = new PreferencesStore(path); var expected = new Preferences { Left = -1500, Mode = VisualMode.Rpg, Quota = QuotaSelection.Weekly, Opacity = .6, RefreshSeconds = 60, AutoStart = true }; store.Save(new()); store.Save(expected); Assert.Equal(expected, store.Load()); Assert.False(File.Exists(path + ".tmp")); }
        finally { if (File.Exists(path)) File.Delete(path); }
    }
    [Fact] public void CorruptPreferencesArePreserved()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        try { File.WriteAllText(path, "broken"); Assert.Throws<InvalidDataException>(() => new PreferencesStore(path).Load()); Assert.Equal("broken", File.ReadAllText(path)); }
        finally { File.Delete(path); }
    }
    [Fact] public void UnsafePreferencesAreNormalized()
    {
        var p = new Preferences { Width = -2, Height = double.NaN, Opacity = 10, RefreshSeconds = 0, Mode = (VisualMode)99 }.Normalize();
        Assert.Equal(260, p.Width); Assert.Equal(410, p.Height); Assert.Equal(1, p.Opacity); Assert.Equal(15, p.RefreshSeconds); Assert.Equal(VisualMode.Digital, p.Mode);
    }
    [Theory] [InlineData(1)] [InlineData(1.5)] [InlineData(2)]
    public void RemovedMonitorAndDpiScalingKeepWidgetVisible(double dpi)
    {
        var fit = WindowPlacement.Fit(new(5000, 3000, 340 * dpi, 400 * dpi), [new(0, 0, 1920, 1080)]);
        Assert.InRange(fit.X, 0, 1920 - fit.Width); Assert.InRange(fit.Y, 0, 1080 - fit.Height);
    }
    [Fact] public void NegativeMonitorCoordinatesArePreserved()
    {
        var rect = new ScreenArea(-1400, 20, 340, 400);
        Assert.Equal(rect, WindowPlacement.Fit(rect, [new(-1920, 0, 1920, 1080), new(0, 0, 1920, 1080)]));
    }
    [Fact] public void SmallerMonitorConstrainsSize()
    {
        var fit = WindowPlacement.Fit(new(0, 0, 900, 900), [new(0, 0, 800, 600)]);
        Assert.Equal(new ScreenArea(0, 0, 800, 600), fit);
    }
}
