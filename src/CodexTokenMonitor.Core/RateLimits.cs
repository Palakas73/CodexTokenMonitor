namespace CodexTokenMonitor.Core;

public enum QuotaKind { FiveHours, Weekly, Other }

public sealed record QuotaWindow(string BucketId, int? DurationMinutes, double UsedPercent, DateTimeOffset? ResetsAt)
{
    public double RemainingPercent => Percent.Remaining(UsedPercent);
    public QuotaKind Kind => DurationMinutes switch { 300 => QuotaKind.FiveHours, 10080 => QuotaKind.Weekly, _ => QuotaKind.Other };
}

public static class Percent
{
    public static double Remaining(double used)
    {
        if (!double.IsFinite(used)) throw new ArgumentOutOfRangeException(nameof(used));
        return Math.Clamp(100 - used, 0, 100);
    }
}

public sealed record RateLimitSnapshot(IReadOnlyList<QuotaWindow> Windows, DateTimeOffset ReadAt);

public interface IRateLimitProvider
{
    Task<RateLimitSnapshot> ReadAsync(CancellationToken cancellationToken);
}

public sealed class MonitorState
{
    public RateLimitSnapshot? LastGood { get; private set; }
    public bool IsStale { get; private set; }
    public string? Error { get; private set; }
    public void Accept(RateLimitSnapshot snapshot) { LastGood = snapshot; IsStale = false; Error = null; }
    public void Fail(string error) { IsStale = true; Error = error; }
}
