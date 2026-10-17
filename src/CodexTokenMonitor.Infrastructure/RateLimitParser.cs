using System.Text.Json;
using CodexTokenMonitor.Core;

namespace CodexTokenMonitor.Infrastructure;

public static class RateLimitParser
{
    public static RateLimitSnapshot Parse(JsonElement result)
    {
        if (result.ValueKind != JsonValueKind.Object) throw new InvalidDataException("Respuesta de cuotas incompleta.");
        var windows = new List<QuotaWindow>();
        if (result.TryGetProperty("rateLimitsByLimitId", out var buckets) && buckets.ValueKind == JsonValueKind.Object && buckets.EnumerateObject().Any())
        {
            foreach (var bucket in buckets.EnumerateObject()) ReadBucket(bucket.Value, bucket.Name, windows);
        }
        else if (result.TryGetProperty("rateLimits", out var legacy) && legacy.ValueKind == JsonValueKind.Object)
        {
            var id = legacy.TryGetProperty("limitId", out var name) && name.ValueKind == JsonValueKind.String ? name.GetString()! : "codex";
            ReadBucket(legacy, id, windows);
        }
        else throw new InvalidDataException("Falta rateLimits en la respuesta.");
        return new(windows, DateTimeOffset.UtcNow);
    }

    private static void ReadBucket(JsonElement bucket, string id, List<QuotaWindow> windows)
    {
        if (bucket.ValueKind == JsonValueKind.Null) return;
        if (bucket.ValueKind != JsonValueKind.Object) throw new InvalidDataException("Bucket inválido.");
        foreach (var slot in new[] { "primary", "secondary" })
        {
            if (!bucket.TryGetProperty(slot, out var window) || window.ValueKind == JsonValueKind.Null) continue;
            if (window.ValueKind != JsonValueKind.Object || !window.TryGetProperty("usedPercent", out var used) ||
                used.ValueKind != JsonValueKind.Number || !used.TryGetDouble(out var percent) || !double.IsFinite(percent))
                throw new InvalidDataException("Ventana sin porcentaje válido.");
            int? minutes = window.TryGetProperty("windowDurationMins", out var duration) && duration.ValueKind == JsonValueKind.Number && duration.TryGetInt32(out var m) ? m : null;
            DateTimeOffset? reset = null;
            if (window.TryGetProperty("resetsAt", out var time) && time.ValueKind != JsonValueKind.Null)
            {
                if (!time.TryGetInt64(out var seconds)) throw new InvalidDataException("Restablecimiento inválido.");
                try { reset = DateTimeOffset.FromUnixTimeSeconds(seconds); }
                catch (ArgumentOutOfRangeException ex) { throw new InvalidDataException("Restablecimiento fuera de rango.", ex); }
            }
            windows.Add(new(id, minutes, percent, reset));
        }
    }
}
