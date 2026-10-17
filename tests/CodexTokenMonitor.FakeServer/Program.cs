using System.Text.Json;

var mode = args[0];
if (args.Length > 1) File.WriteAllText(args[1], Environment.ProcessId.ToString());
while (await Console.In.ReadLineAsync() is { } line)
{
    using var doc = JsonDocument.Parse(line);
    var root = doc.RootElement;
    if (!root.TryGetProperty("id", out var id)) continue;
    if (id.GetInt32() == 1) { Console.WriteLine("{\"id\":1,\"result\":{}}"); continue; }
    if (mode == "disconnect") return;
    if (mode == "hang") { await Task.Delay(TimeSpan.FromMinutes(5)); return; }
    if (mode == "malformed") { Console.WriteLine("not-json"); continue; }
    if (mode == "incomplete") { Console.WriteLine("{\"id\":2}"); continue; }
    if (mode == "auth") { Console.WriteLine("{\"id\":2,\"error\":{\"code\":-32600,\"message\":\"Not logged in\"}}"); continue; }
    if (mode == "error") { Console.WriteLine("{\"id\":2,\"error\":{\"code\":-32603,\"message\":\"Internal error\"}}"); continue; }
    Console.WriteLine("{\"method\":\"account/rateLimits/updated\",\"params\":{}}");
    Console.WriteLine("{\"id\":2,\"result\":{\"rateLimits\":{\"primary\":{\"usedPercent\":22,\"windowDurationMins\":300,\"resetsAt\":null}}}}");
}
