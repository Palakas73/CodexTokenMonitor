using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using CodexTokenMonitor.Core;
using CodexTokenMonitor.Infrastructure;
using Forms = System.Windows.Forms;
namespace CodexTokenMonitor.App;
public partial class MainWindow : Window
{
    private readonly CancellationTokenSource lifetime = new();
    private readonly MonitorViewModel viewModel;
    private readonly PreferencesStore store = new(PreferencesStore.DefaultPath);
    private Preferences preferences = new();
    private readonly DispatcherTimer ageTimer = new() { Interval = TimeSpan.FromSeconds(5) };
    private readonly DispatcherTimer saveTimer = new() { Interval = TimeSpan.FromMilliseconds(400) };
    private readonly SemaphoreSlim refreshGate = new(1, 1);
    private readonly Forms.NotifyIcon tray;
    private readonly System.Drawing.Icon trayIcon;
    private readonly List<Task> manualRefreshes = [];
    private Task? loop;
    private bool closing, readyToClose;
    public MainWindow()
    {
        InitializeComponent();
        try { preferences = store.Load(); }
        catch (Exception ex) when (ex is IOException or InvalidDataException or UnauthorizedAccessException) { MessageBox.Show(ex is InvalidDataException ? ex.Message : "Could not load preferences. Check the settings file and folder permissions.", "Preferences"); }
        preferences = preferences with { AutoStart = StartupShortcut.Enabled };
        var args = Environment.GetCommandLineArgs(); var simulated = args.Contains("--simulate");
        var index = Array.IndexOf(args, "--codex"); var executable = index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
        var timeoutIndex = Array.IndexOf(args, "--timeout-seconds");
        if (timeoutIndex >= 0 && timeoutIndex + 1 < args.Length && int.TryParse(args[timeoutIndex + 1], out var seconds)) preferences = (preferences with { TimeoutSeconds = seconds }).Normalize();
        IRateLimitProvider provider = simulated ? new SimulatedRateLimitProvider() : new CodexRateLimitProvider(executable, TimeSpan.FromSeconds(preferences.TimeoutSeconds));
        DataContext = viewModel = new(provider, simulated); ApplyAppearance();
        Left = preferences.Left; Top = preferences.Top; Width = preferences.Width; Height = preferences.Height;
        trayIcon = ApplicationIcon.LoadTrayIcon();
        tray = new Forms.NotifyIcon { Icon = trayIcon, Text = "Codex Token Monitor", Visible = true };
        tray.DoubleClick += (_, _) => Dispatcher.Invoke(RestoreWidget);
        var trayMenu = new Forms.ContextMenuStrip();
        trayMenu.Items.Add("Show", null, (_, _) => Dispatcher.Invoke(RestoreWidget));
        trayMenu.Items.Add("Hide", null, (_, _) => Dispatcher.Invoke(Hide));
        trayMenu.Items.Add("Refresh", null, (_, _) => Dispatcher.Invoke(StartManualRefresh));
        trayMenu.Items.Add("Exit", null, (_, _) => Dispatcher.Invoke(Close)); tray.ContextMenuStrip = trayMenu;
        SourceInitialized += (_, _) => { HwndSource.FromHwnd(new WindowInteropHelper(this).Handle)?.AddHook(WindowMessages); FitToMonitors(); };
        LocationChanged += (_, _) => ScheduleSave(); SizeChanged += (_, _) => ScheduleSave();
        PreviewMouseWheel += (_, e) => { Change(preferences with { Quota = e.Delta > 0 ? QuotaSelection.FiveHours : QuotaSelection.Weekly }); e.Handled = true; };
        ContextMenu = BuildMenu();
        ageTimer.Tick += (_, _) => viewModel.CheckAge(TimeSpan.FromSeconds(preferences.RefreshSeconds + preferences.TimeoutSeconds + 5));
        saveTimer.Tick += (_, _) => { saveTimer.Stop(); SavePlacement(); };
        Loaded += (_, _) => { loop ??= RunAsync(); ageTimer.Start(); if (args.Contains("--start-hidden")) Hide(); }; Closing += OnClosing;
    }
    private void ApplyAppearance()
    {
        Opacity = preferences.Opacity;
        Surface.Background = new SolidColorBrush(preferences.TransparentBackground ? Color.FromArgb(48, 15, 23, 35) : Color.FromArgb(255, 15, 23, 35));
        viewModel.Mode = preferences.Mode; viewModel.Selection = preferences.Quota;
    }
    private void Change(Preferences value) { preferences = value.Normalize(); ApplyAppearance(); ContextMenu = BuildMenu(); ScheduleSave(); }
    private ContextMenu BuildMenu()
    {
        var menu = new ContextMenu();
        MenuItem Item(string title, Action action, bool? selected = null) { var item = new MenuItem { Header = title, IsCheckable = selected.HasValue, IsChecked = selected ?? false }; item.Click += (_, _) => action(); return item; }
        var visual = new MenuItem { Header = "Visualization" };
        foreach (var mode in Enum.GetValues<VisualMode>()) visual.Items.Add(Item(mode switch { VisualMode.Digital => "Digital display", VisualMode.Gauge => "Semicircular gauge", _ => "RPG bar" }, () => Change(preferences with { Mode = mode }), preferences.Mode == mode)); menu.Items.Add(visual);
        var quota = new MenuItem { Header = "Quota" };
        foreach (var selection in Enum.GetValues<QuotaSelection>()) quota.Items.Add(Item(selection switch { QuotaSelection.FiveHours => "Five hours", QuotaSelection.Weekly => "Weekly", _ => "Both" }, () => Change(preferences with { Quota = selection }), preferences.Quota == selection)); menu.Items.Add(quota);
        var opacity = new MenuItem { Header = "Opacity" };
        foreach (var value in new[] { .4, .6, .8, 1 }) opacity.Items.Add(Item($"{value:P0}", () => Change(preferences with { Opacity = value }), preferences.Opacity == value)); menu.Items.Add(opacity);
        menu.Items.Add(Item("Transparent background", () => Change(preferences with { TransparentBackground = !preferences.TransparentBackground }), preferences.TransparentBackground));
        var size = new MenuItem { Header = "Size" };
        foreach (var entry in new[] { ("Compact", 280d, 330d), ("Normal", 340d, 410d), ("Large", 460d, 540d) }) size.Items.Add(Item(entry.Item1, () => { Width = entry.Item2; Height = entry.Item3; FitToMonitors(); ScheduleSave(); })); menu.Items.Add(size);
        var interval = new MenuItem { Header = "Refresh interval" };
        foreach (var seconds in new[] { 15, 60, 120, 300, 600 }) interval.Items.Add(Item($"{seconds} seconds", () => Change(preferences with { RefreshSeconds = seconds }), preferences.RefreshSeconds == seconds)); menu.Items.Add(interval);
        menu.Items.Add(Item("Start with Windows", () => { try { StartupShortcut.Set(!preferences.AutoStart); Change(preferences with { AutoStart = StartupShortcut.Enabled }); } catch (Exception) { MessageBox.Show("Could not update startup settings. Check access to your Startup folder.", "Start with Windows"); ContextMenu = BuildMenu(); } }, preferences.AutoStart));
        menu.Items.Add(new Separator()); menu.Items.Add(Item("Refresh now", StartManualRefresh)); menu.Items.Add(Item("Show", RestoreWidget)); menu.Items.Add(Item("Hide", Hide)); menu.Items.Add(Item("Exit", Close)); return menu;
    }
    private void DragWidget(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is System.Windows.Controls.Primitives.Thumb || e.OriginalSource is Button) return;
        if (e.ClickCount == 2) Change(preferences with { Mode = (VisualMode)(((int)preferences.Mode + 1) % 3) });
        else if (e.LeftButton == MouseButtonState.Pressed) { DragMove(); FitToMonitors(); }
    }
    private void RestoreWidget() { Show(); FitToMonitors(); }
    public void RestoreFromTray() => RestoreWidget();
    private void ScheduleSave() { if (!IsLoaded || closing) return; saveTimer.Stop(); saveTimer.Start(); }
    private void SavePlacement()
    {
        if (!IsLoaded || WindowState != WindowState.Normal) return;
        preferences = preferences with { Left = Left, Top = Top, Width = ActualWidth, Height = ActualHeight, Opacity = Opacity };
        try { store.Save(preferences); } catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { viewModel.ReportError("Could not save preferences. Check access to the settings folder."); }
    }
    private IntPtr WindowMessages(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled) { if (message is 0x007E or 0x02E0) Dispatcher.BeginInvoke(FitToMonitors); return IntPtr.Zero; }
    private void FitToMonitors()
    {
        var handle = new WindowInteropHelper(this).Handle; if (!GetWindowRect(handle, out var rect)) return;
        var fit = WindowPlacement.Fit(new(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top), Forms.Screen.AllScreens.Select(s => new ScreenArea(s.WorkingArea.X, s.WorkingArea.Y, s.WorkingArea.Width, s.WorkingArea.Height)).ToArray());
        SetWindowPos(handle, IntPtr.Zero, (int)fit.X, (int)fit.Y, (int)fit.Width, (int)fit.Height, 0x0014);
    }
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)] private struct NativeRect { public int Left, Top, Right, Bottom; }
    [System.Runtime.InteropServices.DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr hwnd, out NativeRect rect);
    [System.Runtime.InteropServices.DllImport("user32.dll")] private static extern bool SetWindowPos(IntPtr hwnd, IntPtr after, int x, int y, int width, int height, uint flags);
    private async Task RunAsync()
    {
        try { await RefreshAsync(); var last = DateTimeOffset.UtcNow; while (!lifetime.IsCancellationRequested) { await Task.Delay(1000, lifetime.Token); if (DateTimeOffset.UtcNow - last < TimeSpan.FromSeconds(preferences.RefreshSeconds)) continue; await RefreshAsync(); last = DateTimeOffset.UtcNow; } }
        catch (OperationCanceledException) when (lifetime.IsCancellationRequested) { }
    }
    private async Task RefreshAsync() { if (!await refreshGate.WaitAsync(0, lifetime.Token)) return; try { await viewModel.RefreshAsync(lifetime.Token); } finally { refreshGate.Release(); } }
    private void StartManualRefresh() { if (closing) return; manualRefreshes.RemoveAll(t => t.IsCompleted); manualRefreshes.Add(SafeRefreshAsync()); }
    private async Task SafeRefreshAsync() { try { await RefreshAsync(); } catch (OperationCanceledException) when (lifetime.IsCancellationRequested) { } }
    private void RefreshClick(object sender, RoutedEventArgs e) => StartManualRefresh();
    private async void OnClosing(object? sender, CancelEventArgs e)
    {
        if (readyToClose) return; e.Cancel = true; if (closing) return; closing = true;
        ageTimer.Stop(); saveTimer.Stop(); SavePlacement(); await lifetime.CancelAsync(); if (loop is not null) await loop; await Task.WhenAll(manualRefreshes);
        tray.Visible = false; tray.ContextMenuStrip?.Dispose(); tray.Dispose(); trayIcon.Dispose(); lifetime.Dispose(); readyToClose = true; Close();
    }
}
