using System;
using System.Collections.Generic;
using MdExplorer.Features.Configuration.Models;

namespace MdExplorer.Service.Models
{
    /// <summary>
    /// Root configuration model for .development.yml file
    /// Contains all folders marked with development tags and compatibility settings
    /// </summary>
    public class DevelopmentConfig
    {
        /// <summary>
        /// List of folders with their associated tags
        /// </summary>
        public List<DevelopmentFolder> Folders { get; set; } = new List<DevelopmentFolder>();

        /// <summary>
        /// Markdown compatibility mode configuration
        /// </summary>
        public CompatibilityConfig Compatibility { get; set; } = new CompatibilityConfig();

        /// <summary>
        /// YAML auto-generation configuration
        /// </summary>
        public YamlAutoGenerationConfig YamlAutoGeneration { get; set; } = new YamlAutoGenerationConfig();

        /// <summary>
        /// External browser configuration (reserved for future use)
        /// Allows backward compatibility with .development.yml files that contain this section
        /// </summary>
        public ExternalBrowserConfig ExternalBrowser { get; set; } = new ExternalBrowserConfig();

        /// <summary>
        /// Project metadata shared across users (e.g., description).
        /// Name is intentionally NOT stored here — it is a per-user local label in UserDB.
        /// </summary>
        public ProjectConfig Project { get; set; } = new ProjectConfig();

        /// <summary>
        /// Shared, non-secret Atlassian (Jira/Confluence) configuration. Null when
        /// the project does not use the integration (omitted from the YAML). The
        /// per-user API token is NEVER stored here — it lives encrypted in UserDB.
        /// </summary>
        public AtlassianConfig Atlassian { get; set; }

        /// <summary>
        /// Federation activation of the agent city (§12.4 Agent-Harness-A2A). Null/absent
        /// or <c>Enabled=false</c> → the city and federation stay OFF (full retrocompat).
        /// Travels with the repo via git so the whole team shares the same activation and
        /// the same room secret. MUST live in this typed model: a section not modelled here
        /// would be WIPED on the next participants/description write (they go through the
        /// typed round-trip).
        /// </summary>
        public AgentCityConfig AgentCity { get; set; }

        /// <summary>
        /// Harness agentico per cui MdExplorer installa skill, agent e prompt
        /// (vedi docs-internal/Sprints/2026-08-31-Opencode-Harness-Support.md).
        /// Null/assente = progetto creato prima che la scelta esistesse: la prima apertura la
        /// deduce dal disco una volta sola e la scrive qui. Da lì in poi questa è l'unica fonte
        /// di verità.
        /// <para>DEVE stare in questo modello tipizzato: una sezione non modellata qui viene
        /// CANCELLATA al primo salvataggio che passa dal round-trip (vedi la nota su
        /// <see cref="AgentCity"/>).</para>
        /// </summary>
        public HarnessConfig Harness { get; set; }

        /// <summary>
        /// Le radici di codice ESTERNE alla radice del progetto (sprint 2026-10-12-Radici-Di-Codice-Esterne, R1): qui solo
        /// l'IDENTITÀ (alias, repository, sottocartella), su Git e uguale per il team; dove sta la cartella su ogni
        /// macchina lo dice lo UserDB (<c>CodeRootBinding</c>), mai questo file. Null/assente = nessuna radice esterna.
        /// <para>DEVE stare nel modello tipizzato: una sezione non modellata viene cancellata al primo salvataggio.</para>
        /// </summary>
        public List<CodeRootConfig> CodeRoots { get; set; }
    }

    /// <summary>
    /// Una radice di codice esterna in <c>.development.yml</c>. Il percorso logico dei suoi file è <c>@alias/…</c>,
    /// relativo alla sottocartella <see cref="Subpath"/> del repository: lo stesso su ogni macchina.
    /// </summary>
    public class CodeRootConfig
    {
        /// <summary>Il nome della radice nei percorsi logici (<c>@backend/…</c>): minuscole, cifre, «.», «_», «-».</summary>
        public string Alias { get; set; }

        /// <summary>
        /// Il <c>remote</c> del repository, così come l'ha dato chi ha collegato per primo: è l'identità che fa
        /// riconoscere la cartella giusta sulle altre macchine (R7). Null per una cartella fuori da git.
        /// </summary>
        public string Repository { get; set; }

        /// <summary>La sottocartella del repository che si analizza, con «/»; vuota = tutto il repository.</summary>
        public string Subpath { get; set; }

        public string Description { get; set; }

        /// <summary>Come per le cartelle interne: <c>program</c> = perimetro del grafo del codice.</summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>I linguaggi da analizzare, come per le cartelle interne.</summary>
        public FolderCodeGraphConfig CodeGraph { get; set; }
    }

    /// <summary>
    /// Sezione <c>harness:</c> di <c>.development.yml</c>. Committata col repository: la scelta
    /// dell'harness è una caratteristica del progetto condivisa dal team, non una preferenza
    /// della singola macchina.
    /// </summary>
    public class HarnessConfig
    {
        /// <summary>
        /// <c>copilot</c>, <c>opencode</c> oppure <c>none</c>. Resta uno scalare: la scelta è
        /// esclusiva, e modellarla come lista prima che serva davvero sarebbe prematuro.
        /// </summary>
        public string Target { get; set; }
    }

    /// <summary>
    /// Per-project federation activation, committed in <c>.development.yml</c> (§12.4).
    /// Shared across the team via git; the room secret is a shared credential (room key +
    /// payload-encryption key), NOT the common MdChat API key.
    /// </summary>
    public class AgentCityConfig
    {
        /// <summary>Master switch: when false/absent, the city and federation are OFF.</summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Relative path (from project root) of the ownership markdown doc
        /// (<c>mde_type: ownership</c>) — routing hint "who owns which scope, with which
        /// agents". Optional: absent → no ownership table injected.
        /// </summary>
        public string OwnershipDoc { get; set; }

        /// <summary>
        /// Percorso (dalla radice del progetto) del documento del workflow degli agenti (<c>mde_type: workflow</c>), che
        /// nel front matter nomina il suo <c>*.workflow.json</c> (<c>workflow:</c>). Quando c'è, MdExplorer applica il
        /// workflow: per esempio un incarico con <c>start: ask-owner</c> aspetta che il responsabile lo avvii.
        /// Assente → nessun workflow: gli incarichi partono come sempre.
        /// </summary>
        public string WorkflowDoc { get; set; }

        /// <summary>
        /// Per-project room secret, generated on first activation. Doubles as the relay
        /// room credential and the seed of the payload-encryption key (HKDF, §12.6/6b).
        /// Shared via git so every city on the same repo derives the same key.
        /// </summary>
        public string RoomSecret { get; set; }

        /// <summary>
        /// Optional override of the federation relay URL. Null → the built-in default
        /// (errantia.net) is used. Kept here so a team can point at a self-hosted relay.
        /// </summary>
        public string RelayUrl { get; set; }

        /// <summary>
        /// Agenti in <b>manutenzione (WIP)</b>, per <c>a2a.name</c> (§12.5 coda differita). È
        /// una condizione da segnalare a TUTTO il team, quindi vive qui (git) e NON nel blocco
        /// <c>a2a:</c> del file agente — modificarlo lì cambierebbe l'A2ABlockHash e farebbe
        /// decadere il trust (R3). Un agente in questa lista → le richieste per lui sono
        /// <c>deferred:maintenance</c> (parcheggiate, non fallite) finché non esce dalla lista.
        /// </summary>
        public List<string> Maintenance { get; set; }

        /// <summary>
        /// Isolamento d'esecuzione per-agente (Fase 7c): quando <c>true</c>, ogni risveglio LLM
        /// gira in un <b>worktree git persistente</b> fuori dal progetto (branch fresco per
        /// attività, reset "prepara-prima-di-eseguire"), invece che nella working tree dell'umano.
        /// <b>Opt-in, default false</b>: assente/false → comportamento storico (cwd = progetto).
        /// Richiede che il progetto sia un repo git con remote <c>origin</c>.
        /// </summary>
        /// <summary>
        /// <c>null</c> = <b>non deciso</b>: si applica il default, che è <c>true</c> quando il
        /// progetto è un repo git (l'isolamento serve, e può funzionare) e <c>false</c> quando
        /// non lo è (senza git non esistono worktree). Distinguere "assente" da "false" è ciò
        /// che permette di avere un default sensato senza impedire di spegnerlo a mano.
        /// </summary>
        public bool? UseAgentWorktrees { get; set; }
    }

    /// <summary>
    /// Shared Atlassian config committed in .development.yml. Travels with the
    /// repo so a colleague who clones the project only has to add their own token.
    /// </summary>
    public class AtlassianConfig
    {
        /// <summary>Jira/Confluence Cloud site, e.g. https://acme.atlassian.net.</summary>
        public string JiraBaseUrl { get; set; }

        /// <summary>Project keys the triage search is scoped to, e.g. ["BCO", "OFELIA"].</summary>
        public List<string> JiraProjectKeys { get; set; } = new List<string>();

        /// <summary>
        /// Optional override for the Confluence base URL. On Atlassian Cloud
        /// Confluence lives on the same site as Jira under /wiki, so when this is
        /// null the base is derived as {JiraBaseUrl}/wiki. Set it explicitly only
        /// for the rare case where Confluence sits on a different site.
        /// </summary>
        public string ConfluenceBaseUrl { get; set; }

        /// <summary>
        /// Confluence space keys the search/browse is scoped to, e.g. ["DEV", "ARCH"].
        /// Optional — used as a hint for the agent; CQL queries may target any space.
        /// </summary>
        public List<string> ConfluenceSpaceKeys { get; set; } = new List<string>();
    }

    /// <summary>
    /// Project-level metadata committed with the repository (.development.yml).
    /// </summary>
    public class ProjectConfig
    {
        /// <summary>
        /// Shared description explaining purpose, context, participants and goals.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Team participants — shared across users so that MdE Team gems and
        /// Teams chat shortcuts are consistent regardless of who opens the project.
        /// The canonical key is GitEmail (lowercased); ChatEmail may differ when
        /// the git commit email is not the company Teams address.
        /// </summary>
        public List<ProjectParticipant> Participants { get; set; } = new List<ProjectParticipant>();

        /// <summary>
        /// Optional custom project icon. The PNG sits beside the project (relative
        /// path stored in File). UpdatedAt drives URL cache busting on the client.
        /// </summary>
        public ProjectIconConfig Icon { get; set; }
    }

    /// <summary>
    /// Reference to a custom project icon stored on disk. The PNG file is
    /// committed with the repository so the icon follows the project across users.
    /// </summary>
    public class ProjectIconConfig
    {
        /// <summary>
        /// Relative path of the PNG from project root, e.g. ".md/project-icon.png".
        /// </summary>
        public string File { get; set; }

        /// <summary>
        /// ISO-8601 UTC timestamp of the last save. Used as cache-busting query
        /// parameter when the client requests the icon.
        /// </summary>
        public string UpdatedAt { get; set; }
    }

    /// <summary>
    /// A single project participant. GitEmail is the stable identity key;
    /// ChatEmail is what the MdE Team button opens in MS Teams.
    /// </summary>
    public class ProjectParticipant
    {
        /// <summary>
        /// Canonical identity (lowercased). For manual entries it may coincide
        /// with ChatEmail when there is no matching git author.
        /// </summary>
        public string GitEmail { get; set; }

        /// <summary>
        /// Last known display name from git log (informational only).
        /// </summary>
        public string GitName { get; set; }

        /// <summary>
        /// User-editable display name for the participant strip / popovers.
        /// Falls back to GitName when empty.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Email used to open the Teams chat (msteams://.../users=...).
        /// Defaults to GitEmail until the user overrides it with the
        /// actual company email.
        /// </summary>
        public string ChatEmail { get; set; }

        /// <summary>
        /// True when the entry was added manually (no matching git author).
        /// Manual entries can be removed from the UI; git-matched entries
        /// stay because the commit history keeps carrying them.
        /// </summary>
        public bool Manual { get; set; }
    }

    /// <summary>
    /// Configuration for a single folder
    /// </summary>
    public class DevelopmentFolder
    {
        /// <summary>
        /// Relative path of the folder from project root
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// List of tags associated with this folder (e.g., "program", "tests", "docs")
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// Optional description of the folder's purpose
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Optional Knowledge Graph configuration for this folder. Preserved on
        /// .development.yml round-trips so ProjectMetadataService writes
        /// (description, participants, icon) do not strip the KG namespace.
        /// </summary>
        public FolderKnowledgeGraphConfig KnowledgeGraph { get; set; }

        /// <summary>
        /// Il grafo del codice (sprint 2026-10-09, D16): i linguaggi scelti nella finestra «Ambiente di sviluppo».
        /// Null = la cartella è <c>program</c> ma nessun linguaggio è stato scelto: niente analisi.
        /// </summary>
        public FolderCodeGraphConfig CodeGraph { get; set; }
    }

    /// <summary>Per-folder code graph settings stored in .development.yml.</summary>
    public class FolderCodeGraphConfig
    {
        /// <summary>Gli id del catalogo (<c>csharp</c>, <c>typescript</c>, …): solo questi linguaggi si analizzano.</summary>
        public List<string> Languages { get; set; } = new List<string>();

        /// <summary>
        /// La descrizione del codice (punto 2b del 14/10/2026): `codeGraph.describe`: <c>all</c> = parte da sola dopo una scansione che ha riletto
        /// dei file, o finché la cartella non è descritta tutta; <c>onDemand</c> = solo quando la chiede l'utente. Assente =
        /// <c>onDemand</c>, il comportamento di prima. Vedi <see cref="CodeDescriptionModes"/>.
        /// </summary>
        public string Describe { get; set; }
    }

    public static class CodeDescriptionModes
    {
        public const string All = "all";
        public const string OnDemand = "onDemand";

        /// <summary>Il modo scritto nel .development.yml; un valore sconosciuto è un errore che dice che cosa scrivere.</summary>
        public static string Parse(string value, string where)
        {
            if (string.IsNullOrWhiteSpace(value)) return OnDemand;
            if (value == All || value == OnDemand) return value;
            throw new System.InvalidOperationException($"{where}: codeGraph.describe vale «{value}», ma può essere solo «{All}» o «{OnDemand}». Correggilo nel .development.yml o dalla finestra «Ambiente di sviluppo».");
        }
    }

    /// <summary>
    /// Per-folder Knowledge Graph settings stored in .development.yml.
    /// </summary>
    public class FolderKnowledgeGraphConfig
    {
        /// <summary>
        /// Neo4j graph namespace the folder's .kg.cypher files ingest into.
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Whether KG sync is enabled for this folder.
        /// </summary>
        public bool Enabled { get; set; }
    }

    /// <summary>
    /// Configuration for YAML front matter auto-generation.
    /// NOTA: L'auto-generazione YAML ora avviene on-demand (Export Word, Document Settings)
    /// e non più automaticamente alla visualizzazione del documento.
    /// Questa configurazione è mantenuta per backward compatibility e usi futuri.
    /// </summary>
    public class YamlAutoGenerationConfig
    {
        /// <summary>
        /// Se true, abilita globalmente l'auto-generazione YAML.
        /// Default: false (on-demand). Impostare a true per abilitare sempre.
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Lista di path esclusi dall'auto-generazione (legacy, mantenuto per backward compatibility).
        /// </summary>
        public List<string> ExcludePaths { get; set; } = new List<string>();
    }

    /// <summary>
    /// Configuration for external browser URL handling (reserved for future use)
    /// Allows backward compatibility with .development.yml files that contain this section
    /// </summary>
    public class ExternalBrowserConfig
    {
        /// <summary>
        /// Enable or disable external browser handling
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// If true, ALL external https:// links open in system browser
        /// </summary>
        public bool OpenAllExternal { get; set; } = false;

        /// <summary>
        /// URL patterns to open in external browser
        /// </summary>
        public List<string> UrlPatterns { get; set; } = new List<string>();
    }
}
