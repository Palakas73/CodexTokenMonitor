using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using CodexTokenMonitor.Core;
using CodexTokenMonitor.Infrastructure;
using System.Text.Json;

namespace CodexTokenMonitor.App;

public sealed record QuotaRow(string Label, string Percentage, string Reset, double Remaining = double.NaN);

public sealed class MonitorViewModel(IRateLimitProvider provider, bool simulated) : INotifyPropertyChanged
{
    private readonly MonitorState state = new();
    private readonly SemaphoreSlim gate = new(1, 1);
    public ObservableCollection<QuotaRow> Rows { get; } = [new("Quotas", "--", "")];
    public string Status { get; private set; } = "No data";
    public string LastUpdate => state.LastGood is { } snapshot ? $"Last successful update: {snapshot.ReadAt.ToLocalTime():g}" : "Last successful update: --";
    public event PropertyChangedEventHandler? PropertyChanged;
    public void ReportError(string message) { Status = message; Notify(); }
    private VisualMode mode;
    public VisualMode Mode { get => mode; set { mode = value; PropertyChanged?.Invoke(this, new(nameof(Mode))); } }
    private QuotaSelection selection = QuotaSelection.Both;
    public QuotaSelection Selection { get => selection; set { selection = value; RenderRows(); } }
    public void CheckAge(TimeSpan maxAge)
    {
        if (state.LastGood is { } good && DateTimeOffset.UtcNow - good.ReadAt > maxAge && !state.IsStale)
        { state.Fail("The last reading has expired"); Status = "STALE · waiting for a new reading"; Notify(); }
    }

    private void RenderRows()
    {
        Rows.Clear();
        var wanted = Selection switch { QuotaSelection.FiveHours => new[] { QuotaKind.FiveHours }, QuotaSelection.Weekly => [QuotaKind.Weekly], _ => [QuotaKind.FiveHours, QuotaKind.Weekly] };
        foreach (var kind in wanted)
        {
            var name = kind == QuotaKind.FiveHours ? "Five hours" : "Weekly";
            var windows = state.LastGood?.Windows.Where(w => w.Kind == kind).ToArray() ?? [];
            if (windows.Length == 0) Rows.Add(new(name, "--", "Quota unavailable"));
            foreach (var w in windows) Rows.Add(new($"{w.BucketId} · {name}", w.RemainingPercent.ToString("0.#", CultureInfo.CurrentCulture) + "%", w.ResetsAt is { } reset ? $"Resets: {reset.ToLocalTime():g}" : "Resets: --", w.RemainingPercent));
        }
    }

    public async Task RefreshAsync(CancellationToken ct)
    {
        if (!await gate.WaitAsync(0, ct)) return;
        try
        {
            Status = "Refreshing…";
            Notify();
            var snapshot = await provider.ReadAsync(ct);
            state.Accept(snapshot);
            RenderRows();
            Status = simulated ? "SIMULATION · demo data" : "Live Codex data";
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        catch (Exception ex)
        {
            state.Fail(ex switch
            {
                FileNotFoundException => "Codex CLI could not be found. Check its installation or use --codex.",
                Win32Exception => "Could not start Codex. Check permissions and the CLI installation.",
                JsonException => "Codex returned an invalid JSON response.",
                CodexRpcException or TimeoutException or InvalidDataException => ex.Message,
                IOException => "The connection to Codex failed. Try refreshing again.",
                _ => "Unable to read usage limits. Check Codex and try again."
            });
            Status = $"{(state.LastGood is null ? "No data" : "STALE")}: {state.Error}";
        }
        finally { Notify(); gate.Release(); }
    }

    private void Notify()
    {
        PropertyChanged?.Invoke(this, new(nameof(Status)));
        PropertyChanged?.Invoke(this, new(nameof(LastUpdate)));
    }
}
