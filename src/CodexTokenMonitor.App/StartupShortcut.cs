using System.Runtime.InteropServices;
namespace CodexTokenMonitor.App;
internal static class StartupShortcut
{
    private static string ShortcutPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "CodexTokenMonitor.lnk");
    public static bool Enabled => File.Exists(ShortcutPath);
    public static void Set(bool enabled)
    {
        if (!enabled) { if (Enabled) File.Delete(ShortcutPath); return; }
        var executable = Environment.ProcessPath ?? throw new IOException("Could not locate the application.");
        if (Path.GetFileNameWithoutExtension(executable).Equals("dotnet", StringComparison.OrdinalIgnoreCase)) throw new IOException("Enable startup from the published executable.");
        var type = Type.GetTypeFromProgID("WScript.Shell") ?? throw new IOException("Could not create the startup shortcut.");
        var shell = Activator.CreateInstance(type)!; object? shortcut = null;
        try { dynamic s = shell; shortcut = s.CreateShortcut(ShortcutPath); dynamic link = shortcut; link.TargetPath = executable; link.Arguments = "--start-hidden"; link.WorkingDirectory = Path.GetDirectoryName(executable)!; link.Description = "Codex Token Monitor"; link.Save(); }
        finally { if (shortcut is not null) Marshal.FinalReleaseComObject(shortcut); Marshal.FinalReleaseComObject(shell); }
    }
}
