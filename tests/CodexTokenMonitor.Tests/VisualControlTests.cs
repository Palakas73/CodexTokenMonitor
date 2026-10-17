using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CodexTokenMonitor.App.Controls;
using Xunit;
namespace CodexTokenMonitor.Tests;

public sealed class VisualControlTests
{
    [Fact]
    public void LowQuotaGaugeDoesNotStretchGradientIntoGreen()
    {
        Exception? failure = null;
        var thread = new Thread(() => {
            try
            {
                var gauge = new QuotaGauge { Remaining = 16 };
                gauge.Measure(new(280, 88)); gauge.Arrange(new Rect(0, 0, 280, 88));
                var bitmap = new RenderTargetBitmap(280, 88, 96, 96, PixelFormats.Pbgra32); bitmap.Render(gauge);
                var pixels = new byte[280 * 88 * 4]; bitmap.CopyPixels(pixels, 280 * 4, 0);
                for (var i = 0; i < pixels.Length; i += 4) Assert.False(pixels[i + 1] > 150 && pixels[i + 1] > pixels[i + 2] * 1.3, "A low-quota arc must not contain bright green.");
            }
            catch (Exception ex) { failure = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA); thread.Start(); Assert.True(thread.Join(TimeSpan.FromSeconds(10))); Assert.Null(failure);
    }
    [Theory] [InlineData(0)] [InlineData(9)] [InlineData(10)] [InlineData(29)] [InlineData(30)] [InlineData(69)] [InlineData(70)] [InlineData(100)] [InlineData(double.NaN)]
    public void ControlsRenderBoundaryAndUnavailableValues(double value)
    {
        Exception? failure = null;
        var thread = new Thread(() => {
            try
            {
                foreach (var control in new QuotaVisual[] { new DigitalDisplay(), new QuotaGauge(), new RpgBar() })
                {
                    control.Remaining = value; control.Measure(new(280, 88)); control.Arrange(new Rect(0, 0, 280, 88));
                    var bitmap = new RenderTargetBitmap(280, 88, 96, 96, PixelFormats.Pbgra32); bitmap.Render(control);
                    var pixels = new byte[280 * 88 * 4]; bitmap.CopyPixels(pixels, 280 * 4, 0);
                    Assert.Contains(pixels, b => b != 0);
                }
            }
            catch (Exception ex) { failure = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA); thread.Start(); Assert.True(thread.Join(TimeSpan.FromSeconds(10)));
        Assert.Null(failure);
    }
}
