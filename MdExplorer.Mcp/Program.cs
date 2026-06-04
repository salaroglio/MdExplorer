using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

// ── cwd probe (temporary) ───────────────────────────────────────────
// Records the working directory each time this MCP server is spawned, so we
// can verify whether Copilot launches it with cwd = the open project folder
// (the premise of project→account cwd-resolution). Append-only, failsafe.
try
{
    var cwdLog = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MdExplorer", "mcp-cwd.log");
    var pid = System.Diagnostics.Process.GetCurrentProcess().Id;
    File.AppendAllText(cwdLog,
        $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] pid={pid} cwd={Directory.GetCurrentDirectory()}{Environment.NewLine}");
}
catch { /* a diagnostic must never break startup */ }
// ────────────────────────────────────────────────────────────────────

var builder = Host.CreateApplicationBuilder(args);

// Log to stderr only (stdout is reserved for MCP JSON-RPC)
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services
    .AddMcpServer(options =>
    {
        options.ServerInfo = new()
        {
            Name = "MdExplorer",
            Version = "1.0.0"
        };
    })
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

// Register HttpClient for calling MdExplorer API
builder.Services.AddHttpClient("MdExplorer", client =>
{
    var port = MdExplorer.Mcp.PortDiscovery.GetPort();
    client.BaseAddress = new Uri($"http://localhost:{port}");
    client.Timeout = TimeSpan.FromSeconds(30);
});

await builder.Build().RunAsync();
