using System.Text.Json;
using CodexTokenMonitor.Core;

namespace CodexTokenMonitor.Infrastructure;

public static class RateLimitParser
{
    public static RateLimitSnapshot Parse(JsonElement result)
    {
        if (result.ValueKind != JsonValueKind.Object) throw new InvalidDataException("Incomplete rate limit response.");
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
        else throw new InvalidDataException("The response is missing rateLimits.");
        return new(windows, DateTimeOffset.UtcNow);
    }

    private static void ReadBucket(JsonElement bucket, string id, List<QuotaWindow> windows)
    {
        if (bucket.ValueKind == JsonValueKind.Null) return;
        if (bucket.ValueKind != JsonValueKind.Object) throw new InvalidDataException("Invalid rate limit bucket.");
        foreach (var slot in new[] { "primary", "secondary" })
        {
            if (!bucket.TryGetProperty(slot, out var window) || window.ValueKind == JsonValueKind.Null) continue;
            if (window.ValueKind != JsonValueKind.Object || !window.TryGetProperty("usedPercent", out var used) ||
                used.ValueKind != JsonValueKind.Number || !used.TryGetDouble(out var percent) || !double.IsFinite(percent))
                throw new InvalidDataException("Rate limit window has no valid percentage.");
            int? minutes = window.TryGetProperty("windowDurationMins", out var duration) && duration.ValueKind == JsonValueKind.Number && duration.TryGetInt32(out var m) ? m : null;
            DateTimeOffset? reset = null;
            if (window.TryGetProperty("resetsAt", out var time) && time.ValueKind != JsonValueKind.Null)
            {
                if (!time.TryGetInt64(out var seconds)) throw new InvalidDataException("Invalid reset timestamp.");
                try { reset = DateTimeOffset.FromUnixTimeSeconds(seconds); }
                catch (ArgumentOutOfRangeException ex) { throw new InvalidDataException("Reset timestamp is out of range.", ex); }
            }
            windows.Add(new(id, minutes, percent, reset));
        }
    }
}
