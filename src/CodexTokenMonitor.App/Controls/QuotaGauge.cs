using System.Windows;
using System.Windows.Media;
namespace CodexTokenMonitor.App.Controls;

public sealed class QuotaGauge : QuotaVisual
{
    protected override void OnRender(DrawingContext dc)
    {
        var radius = Math.Max(1, Math.Min(ActualWidth / 2 - 12, ActualHeight - 18));
        var center = new Point(ActualWidth / 2, ActualHeight - 10);
        var gradient = new LinearGradientBrush { MappingMode = BrushMappingMode.Absolute, StartPoint = new(center.X - radius, center.Y), EndPoint = new(center.X + radius, center.Y) };
        gradient.GradientStops.Add(new(Colors.IndianRed, 0)); gradient.GradientStops.Add(new(Colors.Gold, .5)); gradient.GradientStops.Add(new(Colors.SpringGreen, 1));
        dc.DrawGeometry(null, new Pen(new SolidColorBrush(Color.FromRgb(42, 55, 72)), 10) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round }, Arc(center, radius, 1));
        if (HasValue && Animated > .01) dc.DrawGeometry(null, new Pen(gradient, 10) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round }, Arc(center, radius, Animated / 100));
        Text(dc, Display, 30, ValueBrush, new(center.X, center.Y - 20));
    }
    private static Geometry Arc(Point center, double radius, double fraction)
    {
        var angle = Math.PI * (1 - Math.Clamp(fraction, 0, 1));
        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            context.BeginFigure(new(center.X - radius, center.Y), false, false);
            context.ArcTo(new(center.X + radius * Math.Cos(angle), center.Y - radius * Math.Sin(angle)), new(radius, radius), 0, false, SweepDirection.Clockwise, true, false);
        }
        geometry.Freeze(); return geometry;
    }
}
