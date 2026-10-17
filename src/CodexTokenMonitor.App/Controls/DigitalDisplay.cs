using System.Windows;
using System.Windows.Media;
namespace CodexTokenMonitor.App.Controls;

public sealed class DigitalDisplay : QuotaVisual
{
    private static readonly int[] Masks = [0x3f, 0x06, 0x5b, 0x4f, 0x66, 0x6d, 0x7d, 0x07, 0x7f, 0x6f];
    protected override void OnRender(DrawingContext dc)
    {
        var digits = HasValue ? Math.Clamp(Animated, 0, 100).ToString("0") : "--";
        var width = digits.Length * 48 + 36;
        var scale = Math.Min(ActualWidth / width, ActualHeight / 82);
        dc.PushTransform(new TranslateTransform((ActualWidth - width * scale) / 2, (ActualHeight - 82 * scale) / 2));
        dc.PushTransform(new ScaleTransform(scale, scale));
        for (var i = 0; i < digits.Length; i++)
        {
            var mask = digits[i] == '-' ? 0x40 : Masks[digits[i] - '0'];
            Rect[] segments = [new(8,2,26,6),new(34,8,6,28),new(34,43,6,28),new(8,71,26,6),new(2,43,6,28),new(2,8,6,28),new(8,36,26,6)];
            dc.PushTransform(new TranslateTransform(i * 48, 0));
            for (var s = 0; s < 7; s++) dc.DrawRoundedRectangle((mask & (1 << s)) != 0 ? ValueBrush : new SolidColorBrush(Color.FromArgb(22, 148, 163, 184)), null, segments[s], 2, 2);
            dc.Pop();
        }
        if (HasValue) Text(dc, "%", 24, ValueBrush, new(digits.Length * 48 + 14, 55));
        dc.Pop(); dc.Pop();
    }
}
