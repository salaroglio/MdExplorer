using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

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
