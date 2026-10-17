using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using CodexTokenMonitor.Core;

namespace CodexTokenMonitor.App;

public sealed record QuotaRow(string Label, string Percentage, string Reset, double Remaining = double.NaN);

public sealed class MonitorViewModel(IRateLimitProvider provider, bool simulated) : INotifyPropertyChanged
{
    private readonly MonitorState state = new();
    private readonly SemaphoreSlim gate = new(1, 1);
    public ObservableCollection<QuotaRow> Rows { get; } = [new("Cuotas", "--", "")];
    public string Status { get; private set; } = "Sin datos";
    public string LastUpdate => state.LastGood is { } snapshot ? $"Última lectura correcta: {snapshot.ReadAt.ToLocalTime():g}" : "Última lectura correcta: --";
    public event PropertyChangedEventHandler? PropertyChanged;
    public void ReportError(string message) { Status = message; Notify(); }
    private VisualMode mode;
    public VisualMode Mode { get => mode; set { mode = value; PropertyChanged?.Invoke(this, new(nameof(Mode))); } }
    private QuotaSelection selection = QuotaSelection.Both;
    public QuotaSelection Selection { get => selection; set { selection = value; RenderRows(); } }
    public void CheckAge(TimeSpan maxAge)
    {
        if (state.LastGood is { } good && DateTimeOffset.UtcNow - good.ReadAt > maxAge && !state.IsStale)
        { state.Fail("La última lectura ha caducado"); Status = "DESACTUALIZADO · esperando nueva lectura"; Notify(); }
    }

    private void RenderRows()
    {
        Rows.Clear();
        var wanted = Selection switch { QuotaSelection.FiveHours => new[] { QuotaKind.FiveHours }, QuotaSelection.Weekly => [QuotaKind.Weekly], _ => [QuotaKind.FiveHours, QuotaKind.Weekly] };
        foreach (var kind in wanted)
        {
            var name = kind == QuotaKind.FiveHours ? "Cinco horas" : "Semanal";
            var windows = state.LastGood?.Windows.Where(w => w.Kind == kind).ToArray() ?? [];
            if (windows.Length == 0) Rows.Add(new(name, "--", "Cuota no disponible"));
            foreach (var w in windows) Rows.Add(new($"{w.BucketId} · {name}", w.RemainingPercent.ToString("0.#", CultureInfo.CurrentCulture) + "%", w.ResetsAt is { } reset ? $"Restablecimiento: {reset.ToLocalTime():g}" : "Restablecimiento: --", w.RemainingPercent));
        }
    }

    public async Task RefreshAsync(CancellationToken ct)
    {
        if (!await gate.WaitAsync(0, ct)) return;
        try
        {
            Status = "Consultando…";
            Notify();
            var snapshot = await provider.ReadAsync(ct);
            state.Accept(snapshot);
            RenderRows();
            Status = simulated ? "SIMULACIÓN · datos de demostración" : "Datos reales de Codex";
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        catch (Exception ex)
        {
            state.Fail(ex.Message);
            Status = $"{(state.LastGood is null ? "Sin datos" : "DESACTUALIZADO")}: {state.Error}";
        }
        finally { Notify(); gate.Release(); }
    }

    private void Notify()
    {
        PropertyChanged?.Invoke(this, new(nameof(Status)));
        PropertyChanged?.Invoke(this, new(nameof(LastUpdate)));
    }
}
