using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using MdExplorer.Mcp;

// `--list-groups`: il catalogo dei gruppi su stdout, e basta. Serve alle impostazioni di
// MdExplorer, che così non tengono una copia dell'elenco: lo chiedono a chi lo sa.
// Si legge dagli args a mano perché un flag senza valore non passa dal binding della
// configurazione, che pretende `--chiave valore`.
if (args.Contains("--list-groups"))
{
    Console.WriteLine(ToolGroups.DescribeAsJson());
    return 0;
}

// ContentRoot = la cartella dell'eseguibile, NON la cartella corrente. Di default è il cwd, e la configurazione di default
// sorveglia ricorsivamente il ContentRoot per ricaricare appsettings.json: lanciato da un CLI dentro un progetto, il server
// prendeva un watch inotify per OGNI sottocartella (misurato il 21/10/2026: cwd con 200 cartelle → 201 watch; tre istanze
// nella radice del repository ≈ 52.000 dei 65.536 dell'utente). Esaurito il limite, il FileSystemWatcher del servizio va
// in errore e l'albero si ricarica in ciclo.
var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings { Args = args, ContentRootPath = AppContext.BaseDirectory });

// Log to stderr only (stdout is reserved for MCP JSON-RPC)
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Quali gruppi di funzionalità esporre. Li decide chi lancia il processo, con
// `--groups core,plantuml,jira`: MdExplorer li scrive nelle configurazioni dei tre
// ambienti agentici a partire dalla scelta fatta per il progetto aperto. Senza
// l'argomento si registra tutto, come prima di questo sprint.
IReadOnlyList<ToolGroups.Group> groups;
try
{
    groups = ToolGroups.Resolve(builder.Configuration[ToolGroups.Argument]);
}
catch (ArgumentException ex)
{
    // Meglio un server che non parte dicendo perché, di un server che parte con metà
    // degli strumenti e lascia credere all'AI che quelli mancanti non siano mai esistiti.
    Console.Error.WriteLine($"[MdExplorer.Mcp] {ex.Message}");
    return 2;
}

Console.Error.WriteLine(
    $"[MdExplorer.Mcp] gruppi attivi: {string.Join(", ", groups.Select(g => g.Id))}");

var mcp = builder.Services
    .AddMcpServer(options =>
    {
        options.ServerInfo = new()
        {
            Name = "MdExplorer",
            Version = "1.0.0"
        };
    })
    .WithStdioServerTransport();

foreach (var group in groups)
{
    group.Register(mcp);
}

// Register HttpClient for calling MdExplorer API
builder.Services.AddHttpClient("MdExplorer", client =>
{
    var port = MdExplorer.Mcp.PortDiscovery.GetPort();
    client.BaseAddress = new Uri($"http://localhost:{port}");
    client.Timeout = TimeSpan.FromSeconds(30);
});

await builder.Build().RunAsync();
return 0;
