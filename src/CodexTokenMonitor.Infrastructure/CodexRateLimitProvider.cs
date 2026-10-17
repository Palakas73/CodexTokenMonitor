using System.Diagnostics;
using System.Text;
using System.Text.Json;
using CodexTokenMonitor.Core;

namespace CodexTokenMonitor.Infrastructure;

public sealed class CodexRateLimitProvider(string? executable = null, TimeSpan? timeout = null, Func<ProcessStartInfo>? processFactory = null) : IRateLimitProvider
{
    private readonly SemaphoreSlim gate = new(1, 1);

    public async Task<RateLimitSnapshot> ReadAsync(CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            deadline.CancelAfter(timeout ?? TimeSpan.FromSeconds(30));
            try { return await ReadProcessAsync(deadline.Token); }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            { throw new TimeoutException("Codex did not respond within the configured timeout."); }
        }
        finally { gate.Release(); }
    }

    private async Task<RateLimitSnapshot> ReadProcessAsync(CancellationToken ct)
    {
        var start = processFactory?.Invoke() ?? new ProcessStartInfo(CodexLocator.Find(executable));
        start.UseShellExecute = false; start.CreateNoWindow = true;
        start.RedirectStandardInput = true; start.RedirectStandardOutput = true; start.RedirectStandardError = true;
        start.StandardOutputEncoding = Encoding.UTF8; start.StandardErrorEncoding = Encoding.UTF8;
        if (processFactory is null)
        {
            start.ArgumentList.Add("app-server"); start.ArgumentList.Add("--listen"); start.ArgumentList.Add("stdio://");
        }
        using var process = Process.Start(start) ?? throw new IOException("Could not start Codex.");
        // Drain diagnostics without retaining potentially sensitive server output.
        var drain = DrainAsync(process.StandardError);
        try
        {
            await SendAsync(process, new { id = 1, method = "initialize", @params = new { clientInfo = new { name = "codex_token_monitor", title = "Codex Token Monitor", version = "1.0.2" } } }, ct);
            await ReceiveAsync(process, 1, ct);
            await SendAsync(process, new { method = "initialized" }, ct);
            await SendAsync(process, new { id = 2, method = "account/rateLimits/read" }, ct);
            return RateLimitParser.Parse(await ReceiveAsync(process, 2, ct));
        }
        finally
        {
            process.StandardInput.Close();
            using var shutdown = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            try { await process.WaitForExitAsync(shutdown.Token); }
            catch (OperationCanceledException) { if (!process.HasExited) process.Kill(entireProcessTree: true); await process.WaitForExitAsync(); }
            try { await drain.WaitAsync(TimeSpan.FromSeconds(2)); }
            catch (TimeoutException) { process.StandardError.Close(); }
        }
    }

    private static async Task DrainAsync(StreamReader reader)
    {
        var buffer = new char[2048];
        while (await reader.ReadAsync(buffer.AsMemory()) > 0) { }
    }

    private static async Task SendAsync(Process process, object message, CancellationToken ct)
    {
        await process.StandardInput.WriteLineAsync(JsonSerializer.Serialize(message).AsMemory(), ct);
        await process.StandardInput.FlushAsync(ct);
    }

    private static async Task<JsonElement> ReceiveAsync(Process process, int id, CancellationToken ct)
    {
        while (true)
        {
            var line = await process.StandardOutput.ReadLineAsync(ct) ?? throw new IOException("Codex closed the connection.");
            using var doc = JsonDocument.Parse(line);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object) throw new InvalidDataException("Invalid RPC message.");
            if (root.TryGetProperty("method", out _) && root.TryGetProperty("id", out var requestId))
            {
                await SendAsync(process, new { id = requestId.Clone(), error = new { code = -32601, message = "Unsupported client request" } }, ct);
                continue;
            }
            if (!root.TryGetProperty("id", out var responseId) || responseId.ValueKind != JsonValueKind.Number || !responseId.TryGetInt32(out var number) || number != id) continue;
            if (root.TryGetProperty("error", out var error)) throw CodexRpcException.FromError(error);
            if (!root.TryGetProperty("result", out var result)) throw new InvalidDataException("RPC response is missing result.");
            return result.Clone();
        }
    }
}
