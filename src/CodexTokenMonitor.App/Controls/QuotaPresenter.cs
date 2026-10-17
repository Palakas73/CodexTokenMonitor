using System.Windows;
using System.Windows.Controls;
using CodexTokenMonitor.Core;
namespace CodexTokenMonitor.App.Controls;

public sealed class QuotaPresenter : ContentControl
{
    public static readonly DependencyProperty ModeProperty = DependencyProperty.Register(nameof(Mode), typeof(VisualMode), typeof(QuotaPresenter), new PropertyMetadata(VisualMode.Digital, Update));
    public static readonly DependencyProperty RemainingProperty = DependencyProperty.Register(nameof(Remaining), typeof(double), typeof(QuotaPresenter), new PropertyMetadata(double.NaN, Update));
    public VisualMode Mode { get => (VisualMode)GetValue(ModeProperty); set => SetValue(ModeProperty, value); }
    public double Remaining { get => (double)GetValue(RemainingProperty); set => SetValue(RemainingProperty, value); }
    private static void Update(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        var presenter = (QuotaPresenter)obj;
        if (e.Property == ModeProperty || presenter.Content is not QuotaVisual)
            presenter.Content = presenter.Mode switch { VisualMode.Gauge => new QuotaGauge(), VisualMode.Rpg => new RpgBar(), _ => new DigitalDisplay() };
        ((QuotaVisual)presenter.Content).Remaining = presenter.Remaining;
    }
    public QuotaPresenter() { Content = new DigitalDisplay(); HorizontalContentAlignment = HorizontalAlignment.Stretch; }
}
