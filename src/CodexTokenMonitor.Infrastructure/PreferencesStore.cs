using System.Text.Json;
using CodexTokenMonitor.Core;
namespace CodexTokenMonitor.Infrastructure;

public sealed class PreferencesStore(string path)
{
    public static string DefaultPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CodexTokenMonitor", "preferences.json");
    public Preferences Load()
    {
        if (!File.Exists(path)) return new();
        try { return (JsonSerializer.Deserialize<Preferences>(File.ReadAllText(path)) ?? new()).Normalize(); }
        catch (JsonException) { throw new InvalidDataException("Preferencias inválidas. El archivo original se conserva."); }
    }
    public void Save(Preferences preferences)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        var temporary = path + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(preferences.Normalize(), new JsonSerializerOptions { WriteIndented = true }));
        File.Move(temporary, path, true);
    }
}
