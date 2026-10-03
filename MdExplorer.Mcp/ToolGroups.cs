using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;

namespace MdExplorer.Mcp;

/// <summary>
/// I gruppi di funzionalità del server MCP: l'unità che si accende e si spegne per progetto.
/// <para>
/// Il motivo è il contesto. Ogni tool registrato viaggia in <c>tools/list</c> — nome, descrizione
/// e schema dei parametri — e finisce nel contesto della chat <b>prima</b> che l'utente scriva
/// qualcosa. Misurato il 22/09/2026 sui 33 tool di allora: ~8.009 token, di cui ~4.354 di solo
/// Jira. Su un progetto che Jira non ce l'ha, è tutto peso morto.
/// </para>
/// <para>
/// I gruppi accesi arrivano da chi lancia il processo (<c>--groups core,plantuml,jira</c>), non
/// da una chiamata al Service e nemmeno dal cwd: il cwd <b>non</b> è la cartella del progetto —
/// misurato, opencode lancia questo server in <c>/tmp/mde-opencode/&lt;guid&gt;</c>.
/// </para>
/// <para>Sprint: docs-internal/Sprints/2026-09-22-Gruppi-MCP-Per-Progetto.md.</para>
/// </summary>
public static class ToolGroups
{
    /// <param name="Id">Come si scrive in <c>--groups</c>.</param>
    /// <param name="ToolsType">La classe che porta i tool del gruppo.</param>
    /// <param name="Mandatory">Se <c>true</c> è acceso comunque: spegnerlo non è una scelta sensata.</param>
    /// <param name="Summary">Una riga per la UI delle impostazioni.</param>
    /// <param name="Register">
    /// Come si registrano i tool del gruppo.
    /// <para>
    /// ⚠️ È un delegato, e non un <see cref="Type"/> da passare a <c>WithTools(IEnumerable&lt;Type&gt;)</c>,
    /// perché in <c>ModelContextProtocol 2.0.0</c> <b>quell'overload non registra niente</b>: misurato il
    /// 22/09/2026 contando i servizi <c>McpServerTool</c> nel contenitore — 0 con l'elenco di tipi (sia con
    /// metodi di istanza sia statici), 1 con <c>WithTools&lt;T&gt;()</c>, 1 con <c>WithToolsFromAssembly</c>.
    /// Non fallisce e non avverte: il server parte, <c>tools/list</c> risponde
    /// «Method 'tools/list' is not available» e l'AI crede che MdExplorer non abbia strumenti.
    /// </para>
    /// </param>
    public sealed record Group(
        string Id, Type ToolsType, bool Mandatory, string Summary, Action<IMcpServerBuilder> Register);

    public const string Core = "core";
    public const string PlantUml = "plantuml";
    public const string Jira = "jira";
    public const string Confluence = "confluence";
    public const string KnowledgeGraph = "kg";
    public const string Agents = "agents";

    /// <summary>Il nome dell'argomento di riga di comando, e quindi anche la chiave di configurazione.</summary>
    public const string Argument = "groups";

    public static readonly IReadOnlyList<Group> All = new[]
    {
        new Group(Core, typeof(CoreTools), true,
            "Scoprire i progetti e cercare nei documenti. Sempre acceso: senza, l'AI non sa quali progetti esistono.",
            b => b.WithTools<CoreTools>()),
        new Group(PlantUml, typeof(PlantUmlTools), false,
            "Verificare un diagramma PlantUML prima di scriverlo in un documento.",
            b => b.WithTools<PlantUmlTools>()),
        new Group(Jira, typeof(JiraTools), false,
            "Leggere, creare e aggiornare le issue Jira, con commenti, transizioni e allegati.",
            b => b.WithTools<JiraTools>()),
        new Group(Confluence, typeof(ConfluenceTools), false,
            "Cercare, leggere e scrivere pagine Confluence.",
            b => b.WithTools<ConfluenceTools>()),
        new Group(KnowledgeGraph, typeof(KnowledgeGraphTools), false,
            "Interrogare il knowledge graph del progetto su Neo4j (concetti, relazioni, Cypher).",
            b => b.WithTools<KnowledgeGraphTools>()),
        new Group(Agents, typeof(AgentTools), false,
            "La città degli agenti: messaggi fra agenti, richieste di intervento e memoria dei fatti appresi.",
            b => b.WithTools<AgentTools>()),
    };

    /// <summary>Gli id validi, per i messaggi d'errore e per la documentazione.</summary>
    public static string AllIds => string.Join(", ", All.Select(g => g.Id));

    /// <summary>
    /// Traduce il valore di <c>--groups</c> nei gruppi da registrare.
    /// <para>
    /// Vuoto o assente = <b>tutti</b>, cioè il comportamento di prima di questo sprint: serve a non
    /// rompere chi ha registrato il server a mano o lo lancia da un terminale suo. I gruppi
    /// obbligatori ci sono comunque. Un id che non esiste <b>non</b> viene ignorato: si solleva,
    /// perché un gruppo sbagliato in una configurazione significa tool che spariscono senza che
    /// nessuno se ne accorga.
    /// </para>
    /// </summary>
    public static IReadOnlyList<Group> Resolve(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv))
            return All;

        var wanted = csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(s => s.ToLowerInvariant())
                        .ToHashSet();

        var unknown = wanted.Where(w => !All.Any(g => g.Id == w)).ToList();
        if (unknown.Count > 0)
        {
            throw new ArgumentException(
                $"--{Argument}: gruppo sconosciuto '{string.Join("', '", unknown)}'. Gli id validi sono: {AllIds}.");
        }

        return All.Where(g => g.Mandatory || wanted.Contains(g.Id)).ToList();
    }

    /// <summary>
    /// Il catalogo dei gruppi con quanto pesa ciascuno, in JSON su stdout (<c>--list-groups</c>).
    /// <para>
    /// Esiste perché la fonte di verità sia <b>una sola</b>. L'elenco dei gruppi e il loro costo
    /// vivono qui, dentro il server; le impostazioni di MdExplorer non ne tengono una copia da
    /// rimettere a mano ogni volta che si aggiunge un tool: lo chiedono a lui. Il peso non è una
    /// stima scritta a mano ma la misura del vero <c>tools/list</c> — si registra il gruppo in un
    /// contenitore usa e getta e si serializza ciò che ne esce.
    /// </para>
    /// </summary>
    public static string DescribeAsJson()
    {
        var described = All.Select(g =>
        {
            var services = new ServiceCollection();
            services.AddLogging();
            var builder = services.AddMcpServer();
            g.Register(builder);
            using var provider = services.BuildServiceProvider();

            var tools = provider.GetServices<McpServerTool>().ToList();
            var chars = tools.Sum(t => JsonSerializer.Serialize(t.ProtocolTool).Length);

            return new
            {
                id = g.Id,
                mandatory = g.Mandatory,
                summary = g.Summary,
                tools = tools.Select(t => t.ProtocolTool.Name).OrderBy(n => n).ToArray(),
                // Il rapporto carattere/token è quello che si usa di solito per l'inglese: serve a
                // dare un ordine di grandezza in UI, non a fatturare.
                approxTokens = chars / 4
            };
        }).ToList();

        return JsonSerializer.Serialize(new { groups = described },
            new JsonSerializerOptions { WriteIndented = false });
    }
}
