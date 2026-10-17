using System.Windows;
using System.Globalization;
namespace CodexTokenMonitor.App;
public partial class App : Application
{
    private Mutex? instance;
    private EventWaitHandle? restore;
    private System.Windows.Threading.DispatcherTimer? timer;
    protected override void OnStartup(StartupEventArgs e)
    {
        // App text and framework resources default to English; keep regional date/number formats.
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
        instance = new Mutex(true, "Local\\CodexTokenMonitor.Widget", out var created);
        restore = new EventWaitHandle(false, EventResetMode.AutoReset, "Local\\CodexTokenMonitor.Restore");
        if (!created) { restore.Set(); Shutdown(); return; }
        base.OnStartup(e);
        var window = new MainWindow(); MainWindow = window;
        timer = new() { Interval = TimeSpan.FromMilliseconds(500) };
        timer.Tick += (_, _) => { if (restore.WaitOne(0)) window.RestoreFromTray(); };
        timer.Start(); window.Show();
    }
    protected override void OnExit(ExitEventArgs e)
    {
        timer?.Stop(); restore?.Dispose(); instance?.Dispose(); base.OnExit(e);
    }
}
