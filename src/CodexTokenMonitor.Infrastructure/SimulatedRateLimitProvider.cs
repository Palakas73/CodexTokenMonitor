using CodexTokenMonitor.Core;
namespace CodexTokenMonitor.Infrastructure;

public sealed class SimulatedRateLimitProvider : IRateLimitProvider
{
    public Task<RateLimitSnapshot> ReadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new RateLimitSnapshot([
            new("SIMULATED", 300, 22, DateTimeOffset.UtcNow.AddHours(2)),
            new("SIMULATED", 10080, 48, DateTimeOffset.UtcNow.AddDays(3))], DateTimeOffset.UtcNow));
    }
}
