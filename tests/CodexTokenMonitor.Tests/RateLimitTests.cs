using System.Text.Json;
using CodexTokenMonitor.Core;
using CodexTokenMonitor.Infrastructure;
using Xunit;

namespace CodexTokenMonitor.Tests;

public sealed class RateLimitTests
{
    [Theory]
    [InlineData(22, 78)] [InlineData(-10, 100)] [InlineData(130, 0)]
    public void RemainingIsClamped(double used, double expected) => Assert.Equal(expected, Percent.Remaining(used));

    [Fact]
    public void NonFiniteIsRejected() => Assert.Throws<ArgumentOutOfRangeException>(() => Percent.Remaining(double.NaN));

    [Fact]
    public void DurationIdentifiesWindowRegardlessOfSlot()
    {
        using var json = JsonDocument.Parse("""{"rateLimits":{"primary":{"usedPercent":60,"windowDurationMins":10080},"secondary":{"usedPercent":20,"windowDurationMins":300}}}""");
        var result = RateLimitParser.Parse(json.RootElement);
        Assert.Equal(QuotaKind.Weekly, result.Windows[0].Kind);
        Assert.Equal(QuotaKind.FiveHours, result.Windows[1].Kind);
    }

    [Theory]
    [InlineData("{}")] [InlineData("null")]
    [InlineData("{\"rateLimits\":{\"primary\":{}}}")]
    public void IncompleteResultsAreRejected(string input)
    {
        using var json = JsonDocument.Parse(input);
        Assert.Throws<InvalidDataException>(() => RateLimitParser.Parse(json.RootElement));
    }

    [Fact]
    public void NullDurationDoesNotBecomeFiveHours()
    {
        using var json = JsonDocument.Parse("""{"rateLimits":{"primary":{"usedPercent":5,"windowDurationMins":null,"resetsAt":null}}}""");
        Assert.Equal(QuotaKind.Other, RateLimitParser.Parse(json.RootElement).Windows.Single().Kind);
    }

    [Fact]
    public void MultipleBucketsTakePrecedence()
    {
        using var json = JsonDocument.Parse("""{"rateLimits":{"primary":{"usedPercent":99}},"rateLimitsByLimitId":{"codex":{"secondary":{"usedPercent":25,"windowDurationMins":300}},"other":{"primary":{"usedPercent":80}}}}""");
        var result = RateLimitParser.Parse(json.RootElement);
        Assert.Equal(2, result.Windows.Count);
        Assert.Equal(75, result.Windows.Single(w => w.BucketId == "codex").RemainingPercent);
    }

    [Fact]
    public void FailureRetainsLastGoodAndMarksStale()
    {
        var state = new MonitorState();
        var snapshot = new RateLimitSnapshot([], DateTimeOffset.UtcNow);
        state.Accept(snapshot);
        state.Fail("Sin conexión");
        Assert.Same(snapshot, state.LastGood);
        Assert.True(state.IsStale);
        state.Accept(snapshot);
        Assert.False(state.IsStale);
    }

    [Fact]
    public void InitialFailureHasNoInventedData()
    {
        var state = new MonitorState(); state.Fail("Sin autenticación");
        Assert.Null(state.LastGood);
    }

    [Fact]
    public async Task MissingExecutableReportsCommunicationFailure()
    {
        var provider = new CodexRateLimitProvider(Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".exe"));
        await Assert.ThrowsAsync<FileNotFoundException>(() => provider.ReadAsync(CancellationToken.None));
    }
}
