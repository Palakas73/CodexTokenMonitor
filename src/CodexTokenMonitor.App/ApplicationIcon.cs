using System.Windows;

namespace CodexTokenMonitor.App;

internal static class ApplicationIcon
{
    public static System.Drawing.Icon LoadTrayIcon()
    {
        var resource = Application.GetResourceStream(new Uri("/CodexTokenMonitor.App;component/Assets/CodexTokenMonitor.ico", UriKind.Relative))
            ?? throw new IOException("The embedded application icon could not be loaded.");
        using var stream = resource.Stream;
        // Select a crisp frame for the notification-area DPI; own the cloned icon independently of the stream.
        using var icon = new System.Drawing.Icon(stream, System.Windows.Forms.SystemInformation.SmallIconSize);
        return (System.Drawing.Icon)icon.Clone();
    }
}
