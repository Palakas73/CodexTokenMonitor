using System.Text.Json;
namespace CodexTokenMonitor.Infrastructure;

public sealed class CodexRpcException(int? code, bool authentication) : Exception(authentication
    ? "Codex requires a compatible authenticated session. Sign in through Codex and refresh again."
    : $"Codex rejected the request (RPC {code?.ToString() ?? "unknown"}).")
{
    public int? Code { get; } = code;
    public bool IsAuthenticationFailure { get; } = authentication;
    public static CodexRpcException FromError(JsonElement error)
    {
        int? code = error.ValueKind == JsonValueKind.Object && error.TryGetProperty("code", out var c) && c.ValueKind == JsonValueKind.Number && c.TryGetInt32(out var number) ? number : null;
        var message = error.ValueKind == JsonValueKind.Object && error.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String ? m.GetString() ?? "" : "";
        var auth = new[] { "not logged in", "unauthorized", "authentication", "not authenticated", "requires chatgpt" }.Any(term => message.Contains(term, StringComparison.OrdinalIgnoreCase));
        return new(code, auth);
    }
}
