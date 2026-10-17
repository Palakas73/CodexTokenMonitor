using System.Windows;
using System.Windows.Media;
namespace CodexTokenMonitor.App.Controls;

public sealed class RpgBar : QuotaVisual
{
    protected override void OnRender(DrawingContext dc)
    {
        var box = new Rect(5, ActualHeight / 2 - 21, Math.Max(1, ActualWidth - 10), 42);
        dc.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(10, 17, 27)), new Pen(new SolidColorBrush(Color.FromRgb(174, 145, 83)), 2), box, 5, 5);
        if (HasValue && Animated > 0)
        {
            var color = ((SolidColorBrush)ValueBrush).Color;
            var fill = new LinearGradientBrush(color, Color.FromRgb((byte)(color.R / 3), (byte)(color.G / 3), (byte)(color.B / 3)), 90);
            dc.DrawRoundedRectangle(fill, null, new(box.X + 3, box.Y + 3, Math.Max(0, (box.Width - 6) * Animated / 100), box.Height - 6), 3, 3);
        }
        for (var i = 1; i < 10; i++) dc.DrawLine(new Pen(new SolidColorBrush(Color.FromArgb(55, 8, 15, 25)), 1), new(box.X + box.Width * i / 10, box.Y + 3), new(box.X + box.Width * i / 10, box.Bottom - 3));
        Text(dc, Display, 24, Brushes.White, new(ActualWidth / 2, ActualHeight / 2));
    }
}
