using System.Diagnostics;
using System.Text.Json;
using CodexTokenMonitor.Core;
using CodexTokenMonitor.Infrastructure;
using CodexTokenMonitor.App;
using Xunit;
namespace CodexTokenMonitor.Tests;

public sealed class IntegrationTests
{
    private static CodexRateLimitProvider Fake(string mode, string? pidFile = null, double timeout = 5) => new(timeout: TimeSpan.FromSeconds(timeout), processFactory: () =>
    {
        var start = new ProcessStartInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet", "dotnet.exe"));
        start.ArgumentList.Add(Path.Combine(AppContext.BaseDirectory, "FakeServer", "CodexTokenMonitor.FakeServer.dll")); start.ArgumentList.Add(mode);
        if (pidFile is not null) start.ArgumentList.Add(pidFile); return start;
    });
    [Theory] [InlineData("auth", true)] [InlineData("error", false)]
    public async Task RpcErrorsAreSafeAndClassified(string mode, bool auth)
    {
        var ex = await Assert.ThrowsAsync<CodexRpcException>(() => Fake(mode).ReadAsync(CancellationToken.None));
        Assert.Equal(auth, ex.IsAuthenticationFailure); Assert.NotNull(ex.Code);
    }
    [Fact] public async Task DisconnectFails() => await Assert.ThrowsAsync<IOException>(() => Fake("disconnect").ReadAsync(CancellationToken.None));
    [Fact] public async Task IncompleteResponseFails() => await Assert.ThrowsAsync<InvalidDataException>(() => Fake("incomplete").ReadAsync(CancellationToken.None));
    [Fact] public async Task MalformedJsonFails() => await Assert.ThrowsAnyAsync<JsonException>(() => Fake("malformed").ReadAsync(CancellationToken.None));
    [Fact] public async Task ReconnectAfterFailureStartsFreshProcess()
    {
        var calls = 0;
        var provider = new CodexRateLimitProvider(processFactory: () => {
            var s = new ProcessStartInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet", "dotnet.exe"));
            s.ArgumentList.Add(Path.Combine(AppContext.BaseDirectory, "FakeServer", "CodexTokenMonitor.FakeServer.dll")); s.ArgumentList.Add(++calls == 1 ? "disconnect" : "ok"); return s;
        });
        await Assert.ThrowsAsync<IOException>(() => provider.ReadAsync(CancellationToken.None));
        Assert.Equal(78, (await provider.ReadAsync(CancellationToken.None)).Windows.Single().RemainingPercent);
    }
    [Fact] public async Task TimeoutKillsOwnedProcess()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".pid");
        try { await Assert.ThrowsAsync<TimeoutException>(() => Fake("hang", path, 1).ReadAsync(CancellationToken.None)); Assert.True(File.Exists(path)); AssertExited(int.Parse(File.ReadAllText(path))); }
        finally { if (File.Exists(path)) File.Delete(path); }
    }
    [Fact] public async Task CancellationKillsOwnedProcess()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".pid");
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Fake("hang", path).ReadAsync(cts.Token));
            Assert.True(File.Exists(path)); AssertExited(int.Parse(File.ReadAllText(path)));
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }
    [Fact] public async Task SuccessfulReadClosesProcess()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".pid");
        try { await Fake("ok", path).ReadAsync(CancellationToken.None); AssertExited(int.Parse(File.ReadAllText(path))); }
        finally { if (File.Exists(path)) File.Delete(path); }
    }
    private static void AssertExited(int pid) { try { using var p = Process.GetProcessById(pid); Assert.True(p.HasExited); } catch (ArgumentException) { } }

    [Fact] public async Task ConcurrentViewModelUpdatesAreSkipped()
    {
        var provider = new WaitingProvider(); var vm = new MonitorViewModel(provider, false);
        var first = vm.RefreshAsync(CancellationToken.None); await vm.RefreshAsync(CancellationToken.None);
        Assert.Equal(1, provider.Calls); provider.Result.SetResult(new([], DateTimeOffset.UtcNow)); await first;
    }
    [Fact] public async Task MissingQuotaShowsNoPercentage()
    {
        var vm = new MonitorViewModel(new SimulatedRateLimitProvider(), true) { Selection = QuotaSelection.Weekly };
        await vm.RefreshAsync(CancellationToken.None); Assert.Single(vm.Rows); Assert.Equal(52, vm.Rows.Single().Remaining);
        var empty = new WaitingProvider(); var missing = new MonitorViewModel(empty, false); var read = missing.RefreshAsync(CancellationToken.None);
        empty.Result.SetResult(new([], DateTimeOffset.UtcNow)); await read;
        Assert.All(missing.Rows, row => { Assert.Equal("--", row.Percentage); Assert.True(double.IsNaN(row.Remaining)); });
    }
    [Fact] public async Task OldSnapshotBecomesStaleWithoutLosingValues()
    {
        var provider = new WaitingProvider(); var vm = new MonitorViewModel(provider, false); var read = vm.RefreshAsync(CancellationToken.None);
        provider.Result.SetResult(new([new("codex", 300, 40, null)], DateTimeOffset.UtcNow.AddMinutes(-10))); await read;
        vm.CheckAge(TimeSpan.FromSeconds(120)); Assert.Contains("DESACTUALIZADO", vm.Status); Assert.Equal(60, vm.Rows.First().Remaining);
    }
    private sealed class WaitingProvider : IRateLimitProvider
    {
        public int Calls; public TaskCompletionSource<RateLimitSnapshot> Result { get; } = new();
        public Task<RateLimitSnapshot> ReadAsync(CancellationToken ct) { Calls++; return Result.Task; }
    }
}
