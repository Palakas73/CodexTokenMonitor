namespace CodexTokenMonitor.Infrastructure;

public static class CodexLocator
{
    public static string Find(string? explicitPath = null)
    {
        if (explicitPath is not null)
            return File.Exists(explicitPath) && Path.GetExtension(explicitPath).Equals(".exe", StringComparison.OrdinalIgnoreCase)
                ? Path.GetFullPath(explicitPath) : throw new FileNotFoundException("La ruta de Codex debe apuntar a un .exe existente.");
        foreach (var directory in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator))
        {
            var candidate = Path.Combine(directory.Trim('"'), "codex.exe");
            if (File.Exists(candidate)) return candidate;
        }
        var bundled = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenAI", "Codex", "bin");
        if (Directory.Exists(bundled))
        {
            var candidate = Directory.EnumerateFiles(bundled, "codex.exe", SearchOption.AllDirectories)
                .OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault();
            if (candidate is not null) return candidate;
        }
        throw new FileNotFoundException("No se encuentra codex.exe. Añádelo a PATH o usa --codex ruta.exe.");
    }
}
