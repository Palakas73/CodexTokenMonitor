using System.Windows;
using System.Windows.Media.Imaging;
using Xunit;

namespace CodexTokenMonitor.Tests;

public sealed class ApplicationIconTests
{
    [Fact]
    public void EmbeddedIconLoadsForNotificationAreaAndHighDpiSizes()
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                // Exercise the actual packaged resource and native ICO decoder, not a loose source file.
                var uri = new Uri("/CodexTokenMonitor.App;component/Assets/CodexTokenMonitor.ico", UriKind.Relative);
                using (var stream = Application.GetResourceStream(uri)!.Stream)
                {
                    var decoder = new IconBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                    Assert.Equal(new[] { 16, 20, 24, 32, 40, 48, 64, 128, 256 }, decoder.Frames.Select(frame => frame.PixelWidth));
                }
                // The native tray decoder selects small/high-DPI frames; WPF also verifies the 256px Explorer frame above.
                foreach (var size in new[] { 16, 20, 24, 32, 40, 48, 64, 128 })
                {
                    using var stream = Application.GetResourceStream(uri)!.Stream;
                    using var icon = new System.Drawing.Icon(stream, new System.Drawing.Size(size, size));
                    Assert.Equal(size, icon.Width);
                    Assert.Equal(size, icon.Height);
                    using var bitmap = icon.ToBitmap();
                    Assert.True(bitmap.GetPixel(size / 2, size / 2).A > 0);
                }
            }
            catch (Exception ex) { failure = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(10)));
        Assert.Null(failure);
    }
}
