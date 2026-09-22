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
                    "MdExplorer.Mcp non trovato accanto al servizio né nella sua build: non si può sapere " +
                    "quali gruppi di funzionalità MCP esistono. Ricompila o reinstalla MdExplorer.");
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
                ?? throw new InvalidOperationException($"Non si riesce ad avviare '{exe} --list-groups'.");

            var stdout = process.StandardOutput.ReadToEnd();
            var stderr = process.StandardError.ReadToEnd();
            if (!process.WaitForExit(CatalogTimeoutMs))
            {
                try { process.Kill(entireProcessTree: true); } catch { }
                throw new InvalidOperationException(
                    $"'{exe} --list-groups' non ha risposto entro {CatalogTimeoutMs / 1000} secondi.");
            }

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"'{exe} --list-groups' è uscito con codice {process.ExitCode}. {stderr}".Trim());
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
                    $"'{exe} --list-groups' non ha dichiarato nessun gruppo: il server è stato costruito male.");
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

            var catalog = Catalog();
            chosen = !string.IsNullOrWhiteSpace(project.McpToolGroups);

            var wanted = chosen
                ? project.McpToolGroups
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(s => s.ToLowerInvariant())
                    .ToHashSet()
                : DefaultFor(project, db).ToHashSet();

            return catalog
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
        {
            var groups = new List<string> { "core", "plantuml" };

            var atlassian = db.GetDal<ProjectAtlassianSettings>().GetList()
                .FirstOrDefault(s => s.Project != null && s.Project.Id == project.Id);
            if (atlassian?.Enabled == true)
            {
                groups.Add("jira");
                groups.Add("confluence");
            }

            var neo4j = db.GetDal<ProjectNeo4jSettings>().GetList()
                .FirstOrDefault(s => s.Project != null && s.Project.Id == project.Id);
            if (neo4j?.Enabled == true)
            {
                groups.Add("kg");
            }

            // La città degli agenti si accende nel .development.yml (committato, vale per il team):
            // è il segnale giusto anche quando nessun agente è ancora stato registrato. Gli agenti
            // già registrati valgono comunque, per un progetto acceso prima che la voce esistesse.
            // ⚠️ `.ToList()` prima del confronto: NHibernate non sa tradurre in SQL né
            // `string.Equals(a, b, StringComparison)` né `TrimEnd`, e solleva NotSupportedException
            // invece di restituire niente (misurato il 22/09/2026). La tabella degli agenti è piccola.
            var projectPath = (project.Path ?? string.Empty).TrimEnd('/', '\\');
            // ⚠️ Solo gli agenti `llm`, cioè quelli che qualcuno ha scritto: MdExplorer registra da
            // sé l'algoritmico `a2a-ping` in OGNI progetto, quindi «ha agenti» sarebbe sempre vero e
            // il gruppo resterebbe acceso ovunque (misurato il 22/09/2026 su un progetto appena nato).
            var hasAgents = AgentCityEnabled(project.Path)
                || db.GetDal<AgentIdentity>().GetList().ToList()
                    .Any(a => string.Equals(a.Kind, "llm", StringComparison.OrdinalIgnoreCase)
                        && string.Equals(
                            (a.ProjectPath ?? string.Empty).TrimEnd('/', '\\'),
                            projectPath,
                            StringComparison.OrdinalIgnoreCase));
            if (hasAgents)
            {
                groups.Add("agents");
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
        private static bool AgentCityEnabled(string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath)) return false;

            var filePath = Path.Combine(projectPath, ".development.yml");
            if (!File.Exists(filePath)) return false;

            try
            {
                var yaml = File.ReadAllText(filePath);
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
    }
}
