using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CodexTokenMonitor.App.Controls;

public abstract class QuotaVisual : FrameworkElement
{
    public static readonly DependencyProperty RemainingProperty = DependencyProperty.Register(nameof(Remaining), typeof(double), typeof(QuotaVisual), new PropertyMetadata(double.NaN, Changed));
    public static readonly DependencyProperty AnimatedProperty = DependencyProperty.Register(nameof(Animated), typeof(double), typeof(QuotaVisual), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));
    public double Remaining { get => (double)GetValue(RemainingProperty); set => SetValue(RemainingProperty, value); }
    public double Animated { get => (double)GetValue(AnimatedProperty); set => SetValue(AnimatedProperty, value); }
    protected bool HasValue => double.IsFinite(Remaining);
    protected Brush ValueBrush => new SolidColorBrush(!HasValue ? Color.FromRgb(100, 116, 139) : Remaining >= 70 ? Color.FromRgb(74, 222, 128) : Remaining >= 30 ? Color.FromRgb(250, 204, 21) : Remaining >= 10 ? Color.FromRgb(251, 146, 60) : Color.FromRgb(248, 113, 113));
    protected string Display => HasValue ? Math.Clamp(Animated, 0, 100).ToString("0", CultureInfo.CurrentCulture) + "%" : "--";
    private static void Changed(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        var control = (QuotaVisual)obj;
        if (control.HasValue && control.IsVisible && SystemParameters.ClientAreaAnimation)
            control.BeginAnimation(AnimatedProperty, new DoubleAnimation(Math.Clamp(control.Remaining, 0, 100), TimeSpan.FromMilliseconds(350)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
        else { control.BeginAnimation(AnimatedProperty, null); control.Animated = control.HasValue ? Math.Clamp(control.Remaining, 0, 100) : 0; }
        control.InvalidateVisual();
    }
    protected void Text(DrawingContext dc, string value, double size, Brush brush, Point center)
    {
        var text = new FormattedText(value, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("Segoe UI Semibold"), size, brush, VisualTreeHelper.GetDpi(this).PixelsPerDip);
        dc.DrawText(text, new(center.X - text.Width / 2, center.Y - text.Height / 2));
    }
    protected override Size MeasureOverride(Size availableSize) => new(double.IsInfinity(availableSize.Width) ? 260 : availableSize.Width, 88);
}
