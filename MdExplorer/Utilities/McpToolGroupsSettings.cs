using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Service;

namespace MdExplorer.Utilities
{
    /// <summary>
    /// Quali gruppi di funzionalità MCP accendere per un progetto, e quanto pesa ciascuno.
    /// <para>
    /// Il problema che risolve è il contesto della chat: i tool del server MCP entrano tutti in
    /// <c>tools/list</c> — nome, descrizione, schema dei parametri — <b>prima</b> che l'utente
    /// scriva qualcosa. Misurati il 22/09/2026: ~8.000 token per tutti e 33, di cui ~4.350 di solo
    /// Jira. Su un progetto che Jira non ce l'ha sono token comprati e mai usati.
    /// </para>
    /// <para>
    /// L'elenco dei gruppi <b>non è scritto qui</b>: lo dichiara il server con <c>--list-groups</c>,
    /// pesi compresi. Una copia in questo progetto sarebbe una seconda verità da riallineare a mano
    /// ogni volta che nasce un tool, e nessuno se ne accorgerebbe finché la casella non manca in UI.
    /// </para>
    /// <para>Sprint: docs-internal/Sprints/2026-09-22-Gruppi-MCP-Per-Progetto.md, fase F2.</para>
    /// </summary>
    public static class McpToolGroupsSettings
    {
        /// <summary>Un gruppo come lo racconta il server.</summary>
        public sealed record GroupInfo(string Id, bool Mandatory, string Summary, string[] Tools, int ApproxTokens);

        private const int CatalogTimeoutMs = 20_000;

        private static readonly object _catalogLock = new();
        private static IReadOnlyList<GroupInfo> _catalog;
        private static string _catalogFrom;
        private static DateTime _catalogStamp;

        /// <summary>
        /// Il catalogo dei gruppi, chiesto a <c>MdExplorer.Mcp --list-groups</c>.
        /// <para>
        /// Tenuto in cache per la durata del processo, ma invalidato se l'eseguibile cambia
        /// (percorso o data): in sviluppo si ricompila il server di continuo, e un catalogo vecchio
        /// mostrerebbe gruppi che non esistono più.
        /// </para>
        /// <para>
        /// Se il server non si trova o non risponde <b>si solleva</b>. Un elenco vuoto verrebbe
        /// disegnato come «questo progetto non ha gruppi», che è falso e porterebbe a salvare una
        /// scelta che spegne tutto.
        /// </para>
        /// </summary>
        public static IReadOnlyList<GroupInfo> Catalog()
        {
            var exe = ProjectsManager.ResolveMcpExecutable(AppDomain.CurrentDomain.BaseDirectory);
            if (string.IsNullOrWhiteSpace(exe))
            {
                throw new InvalidOperationException(
                    MdExplorer.Features.Services.UiText.Say("MdExplorer.Mcp non trovato accanto al servizio né nella sua build: non si può sapere quali gruppi di funzionalità MCP esistono. Ricompila o reinstalla MdExplorer.", "MdExplorer.Mcp not found next to the service or in its build: there is no way to know which MCP feature groups exist. Rebuild or reinstall MdExplorer."));
            }

            var stamp = File.GetLastWriteTimeUtc(exe);
            lock (_catalogLock)
            {
                if (_catalog != null && _catalogFrom == exe && _catalogStamp == stamp)
                    return _catalog;

                _catalog = ReadCatalog(exe);
                _catalogFrom = exe;
                _catalogStamp = stamp;
                return _catalog;
            }
        }

        private static IReadOnlyList<GroupInfo> ReadCatalog(string exe)
        {
            var psi = new ProcessStartInfo(exe)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            psi.ArgumentList.Add("--list-groups");

            using var process = Process.Start(psi)
                ?? throw new InvalidOperationException(MdExplorer.Features.Services.UiText.Say($"Non si riesce ad avviare '{exe} --list-groups'.", $"Cannot start '{exe} --list-groups'."));

            var stdout = process.StandardOutput.ReadToEnd();
            var stderr = process.StandardError.ReadToEnd();
            if (!process.WaitForExit(CatalogTimeoutMs))
            {
                try { process.Kill(entireProcessTree: true); } catch { }
                throw new InvalidOperationException(
                    MdExplorer.Features.Services.UiText.Say($"'{exe} --list-groups' non ha risposto entro {CatalogTimeoutMs / 1000} secondi.", $"'{exe} --list-groups' did not answer within {CatalogTimeoutMs / 1000} seconds."));
            }

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    MdExplorer.Features.Services.UiText.Say($"'{exe} --list-groups' è uscito con codice {process.ExitCode}. {stderr}", $"'{exe} --list-groups' exited with code {process.ExitCode}. {stderr}").Trim());
            }

            using var doc = JsonDocument.Parse(stdout);
            var groups = doc.RootElement.GetProperty("groups").EnumerateArray()
                .Select(g => new GroupInfo(
                    g.GetProperty("id").GetString(),
                    g.GetProperty("mandatory").GetBoolean(),
                    g.GetProperty("summary").GetString(),
                    g.GetProperty("tools").EnumerateArray().Select(t => t.GetString()).ToArray(),
                    g.GetProperty("approxTokens").GetInt32()))
                .ToList();

            if (groups.Count == 0)
            {
                throw new InvalidOperationException(
                    MdExplorer.Features.Services.UiText.Say($"'{exe} --list-groups' non ha dichiarato nessun gruppo: il server è stato costruito male.", $"'{exe} --list-groups' declared no groups: the server was built wrongly."));
            }

            return groups;
        }

        /// <summary>
        /// I gruppi accesi per il progetto, come id.
        /// <para>
        /// Se il progetto ha una scelta salvata vale quella — depurata dai gruppi che nel frattempo
        /// non esistono più, e sempre con i gruppi obbligatori. Se non ce l'ha valgono
        /// <see cref="DefaultFor"/>, cioè le integrazioni che il progetto ha davvero configurato.
        /// </para>
        /// </summary>
        public static IReadOnlyList<string> Resolve(Project project, IUserSettingsDB db, out bool chosen)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));
            chosen = !string.IsNullOrWhiteSpace(project.McpToolGroups);
            return Resolve(project.McpToolGroups, chosen ? null : ReadDefaultInputs(project, db), project.Path);
        }

        /// <summary>
        /// <see cref="Resolve(Project, IUserSettingsDB, out bool)"/> in due tempi, per chi tiene una transazione:
        /// <paramref name="chosenGroups"/> e <paramref name="inputs"/> si leggono dentro (<see cref="ReadDefaultInputs"/>),
        /// questo si chiama DOPO il commit, perché legge <c>.development.yml</c> dal disco.
        /// <paramref name="inputs"/> serve solo quando non c'è una scelta salvata.
        /// </summary>
        public static IReadOnlyList<string> Resolve(string chosenGroups, DefaultInputs inputs, string projectPath, PerfTrace.PhaseTimer perf = null)
        {
            var wanted = !string.IsNullOrWhiteSpace(chosenGroups)
                ? chosenGroups
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(s => s.ToLowerInvariant())
                    .ToHashSet()
                : DefaultFor(inputs ?? throw new ArgumentNullException(nameof(inputs),
                        "Senza una scelta salvata servono le righe del DB utente: leggile con ReadDefaultInputs nella transazione."),
                    projectPath, perf).ToHashSet();

            return Catalog()
                .Where(g => g.Mandatory || wanted.Contains(g.Id))
                .Select(g => g.Id)
                .ToList();
        }

        /// <summary>
        /// I gruppi da proporre a chi non ha mai scelto: quelli la cui integrazione è configurata
        /// nel progetto.
        /// <para>
        /// Non è un fallback nascosto — è la fotografia del progetto, e la UI la mostra come caselle
        /// già spuntate che si possono cambiare. Dal primo salvataggio vale solo la scelta esplicita.
        /// </para>
        /// </summary>
        public static IReadOnlyList<string> DefaultFor(Project project, IUserSettingsDB db)
            => DefaultFor(ReadDefaultInputs(project, db), project.Path);

        /// <summary>
        /// Quello che <see cref="DefaultFor(Project, IUserSettingsDB)"/> legge dal DB utente. Separato dai file perché
        /// il DB si apre con BEGIN IMMEDIATE: il 06/11/2026, primo avvio dopo un aggiornamento su Windows, la lettura
        /// di gruppi + .development.yml ha tenuto il lock 6,5 s e fermato l'apertura del progetto e AgentMessageDispatcher.
        /// </summary>
        public sealed class DefaultInputs
        {
            public bool Atlassian { get; init; }
            public bool Redmine { get; init; }
            public bool Neo4j { get; init; }
            public bool HasLlmAgents { get; init; }
            public IReadOnlyList<string> BindingLocalPaths { get; init; } = Array.Empty<string>();
        }

        /// <summary>Solo letture del DB utente, da chiamare dentro la transazione. Nessun file.</summary>
        public static DefaultInputs ReadDefaultInputs(Project project, IUserSettingsDB db, PerfTrace.PhaseTimer perf = null)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));

            var atlassian = db.GetDal<ProjectAtlassianSettings>().GetList()
                .FirstOrDefault(s => s.Project != null && s.Project.Id == project.Id);
            perf?.Mark("Atlassian (DB)");
            // Redmine (sprint 2026-10-27-Redmine, RM3): acceso quando la persona l'ha abilitato per il progetto.
            var redmine = db.GetDal<ProjectRedmineSettings>().GetList()
                .FirstOrDefault(s => s.Project != null && s.Project.Id == project.Id);
            perf?.Mark("Redmine (DB)");
            var neo4j = db.GetDal<ProjectNeo4jSettings>().GetList()
                .FirstOrDefault(s => s.Project != null && s.Project.Id == project.Id);
            perf?.Mark("Neo4j (DB)");

            // ⚠️ `.ToList()` prima del confronto: NHibernate non sa tradurre in SQL né
            // `string.Equals(a, b, StringComparison)` né `TrimEnd`, e solleva NotSupportedException
            // invece di restituire niente (misurato il 22/09/2026). La tabella degli agenti è piccola.
            var projectPath = (project.Path ?? string.Empty).TrimEnd('/', '\\');
            // ⚠️ Solo gli agenti `llm`, cioè quelli che qualcuno ha scritto: MdExplorer registra da
            // sé l'algoritmico `a2a-ping` in OGNI progetto, quindi «ha agenti» sarebbe sempre vero e
            // il gruppo resterebbe acceso ovunque (misurato il 22/09/2026 su un progetto appena nato).
            var hasLlmAgents = db.GetDal<AgentIdentity>().GetList().ToList()
                .Any(a => string.Equals(a.Kind, "llm", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(
                        (a.ProjectPath ?? string.Empty).TrimEnd('/', '\\'),
                        projectPath,
                        StringComparison.OrdinalIgnoreCase));
            perf?.Mark("AgentIdentity (DB)");

            // Le radici di codice collegate su QUESTA macchina (UserDB, non .development.yml).
            // ⚠️ `.ToList()` prima del confronto per percorso (vedi sopra).
            var bindingLocalPaths = db.GetDal<CodeRootBinding>().GetList().ToList()
                .Where(b => string.Equals((b.ProjectPath ?? string.Empty).TrimEnd('/', '\\'), projectPath, StringComparison.OrdinalIgnoreCase))
                .Select(b => b.LocalPath ?? string.Empty)
                .ToList();
            perf?.Mark("CodeRootBinding (DB)");

            return new DefaultInputs
            {
                Atlassian = atlassian?.Enabled == true,
                Redmine = redmine?.Enabled == true,
                Neo4j = neo4j?.Enabled == true,
                HasLlmAgents = hasLlmAgents,
                BindingLocalPaths = bindingLocalPaths,
            };
        }

        /// <summary>I gruppi proposti da <paramref name="inputs"/> e dal <c>.development.yml</c>. Legge file: fuori dalla transazione.</summary>
        public static IReadOnlyList<string> DefaultFor(DefaultInputs inputs, string projectPath, PerfTrace.PhaseTimer perf = null)
        {
            if (inputs == null) throw new ArgumentNullException(nameof(inputs));
            var groups = new List<string> { "core", "plantuml" };

            if (inputs.Atlassian)
            {
                groups.Add("jira");
                groups.Add("confluence");
            }
            if (inputs.Redmine)
            {
                groups.Add("redmine");
            }
            if (inputs.Neo4j)
            {
                groups.Add("kg");
            }

            // La città degli agenti si accende nel .development.yml (committato, vale per il team):
            // è il segnale giusto anche quando nessun agente è ancora stato registrato. Gli agenti
            // già registrati valgono comunque, per un progetto acceso prima che la voce esistesse.
            var agentCity = AgentCityEnabled(projectPath);
            perf?.Mark("AgentCityEnabled (.development.yml)");
            if (agentCity || inputs.HasLlmAgents)
            {
                groups.Add("agents");
            }

            // Il grafo del codice (sprint 2026-10-09, D6): acceso di default quando una cartella `program`
            // ha dei linguaggi scelti nella finestra «Ambiente di sviluppo» (codeGraph.languages), o quando il progetto ha
            // una radice esterna collegata (26/10, come «remote» per le ssh://): senza, MarkAgent riceve la cartella ma non
            // code_roots, e non sa a quale @alias corrisponde.
            var codeGraph = CodeGraphConfigured(projectPath);
            perf?.Mark("CodeGraphConfigured (.development.yml)");
            if (codeGraph || inputs.BindingLocalPaths.Count > 0)
            {
                groups.Add("code");
            }

            // Le radici remote (sprint 2026-10-21-Cartella-Remota-Via-SSH, F4): la posizione ssh:// sta nello UserDB di
            // QUESTA macchina, non nel .development.yml — un collega con il codice in locale non riceve i remote_*.
            if (inputs.BindingLocalPaths.Any(p => p.StartsWith(MdExplorer.Features.Services.RemoteFolders.RemoteLocation.Scheme, StringComparison.Ordinal)))
            {
                groups.Add("remote");
            }

            return groups;
        }

        /// <summary>
        /// Se il progetto ha acceso la città degli agenti nel suo <c>.development.yml</c>.
        /// <para>
        /// Letto dalla stessa fonte e con lo stesso deserializzatore di <c>HarnessSettings.Read</c>.
        /// Un file illeggibile vale «spenta»: qui si sta solo proponendo delle caselle, e proporne
        /// una in meno è meno dannoso che far fallire l'apertura delle impostazioni.
        /// </para>
        /// </summary>
        /// <summary>Se il progetto ha almeno una cartella <c>program</c> con <c>codeGraph.languages</c> (D16).</summary>
        private static bool CodeGraphConfigured(string projectPath)
        {
            var cfg = ReadDevelopmentConfigOrNull(projectPath);
            return cfg?.Folders?.Any(f => f.CodeGraph?.Languages?.Count > 0
                && f.Tags?.Any(t => string.Equals(t, "program", StringComparison.OrdinalIgnoreCase)) == true) == true;
        }

        private static MdExplorer.Service.Models.DevelopmentConfig ReadDevelopmentConfigOrNull(string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath)) return null;
            var filePath = Path.Combine(projectPath, ".development.yml");
            if (!File.Exists(filePath)) return null;
            try
            {
                var yaml = MdExplorer.Features.Configuration.DevelopmentConfigText.ReadFile(filePath).Text;
                if (string.IsNullOrWhiteSpace(yaml)) return null;
                var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                    .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
                    .IgnoreUnmatchedProperties()
                    .Build();
                return deserializer.Deserialize<MdExplorer.Service.Models.DevelopmentConfig>(yaml);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[McpToolGroupsSettings] {filePath} non leggibile ({ex.Message}): il grafo del codice lo considero spento per la proposta dei gruppi.");
                return null;
            }
        }

        private static bool AgentCityEnabled(string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath)) return false;

            var filePath = Path.Combine(projectPath, ".development.yml");
            if (!File.Exists(filePath)) return false;

            try
            {
                var yaml = MdExplorer.Features.Configuration.DevelopmentConfigText.ReadFile(filePath).Text;
                if (string.IsNullOrWhiteSpace(yaml)) return false;

                var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                    .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
                    .IgnoreUnmatchedProperties()
                    .Build();

                return deserializer.Deserialize<MdExplorer.Service.Models.DevelopmentConfig>(yaml)?.AgentCity?.Enabled == true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[McpToolGroupsSettings] {filePath} non leggibile ({ex.Message}): " +
                                  "la città degli agenti la considero spenta per la proposta dei gruppi.");
                return false;
            }
        }

        /// <summary>
        /// Il valore da passare a <c>--groups</c>. Vuoto quando i gruppi accesi sono tutti: così la
        /// riga di comando resta quella di sempre e non si scrive un elenco che è solo rumore.
        /// </summary>
        public static string ToArgument(IReadOnlyList<string> groups)
        {
            if (groups == null || groups.Count == 0) return null;
            return groups.Count == Catalog().Count ? null : string.Join(",", groups);
        }

        /// <summary>
        /// The <c>--groups</c> of an agent's turn: the project's, plus the agents' city (messages, interventions,
        /// memory) — the tools a RunToken exists for. Without it, on a project that turned the group off for the
        /// chat, an agent launched by hand did not see them (seen on 29/09/2026, sprint 2026-09-29-Motore-LLM-Unico).
        /// Null stays null: every group.
        /// </summary>
        public static string WithAgents(string groupsArgument)
        {
            if (string.IsNullOrWhiteSpace(groupsArgument)) return groupsArgument;
            var groups = groupsArgument.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return groups.Contains("agents", StringComparer.OrdinalIgnoreCase) ? groupsArgument : string.Join(",", groups.Append("agents"));
        }
    }
}
