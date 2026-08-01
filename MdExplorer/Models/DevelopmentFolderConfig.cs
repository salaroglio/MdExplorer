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

        /// <summary>
        /// Auto-merge dei deliverable-doc degli agenti (Fase 7g): quando <c>true</c>, a un deliverable
        /// pushato (7d.2) che NON tocca il submodule-codice, un gate meccanico fonde il branch
        /// d'attività nel default e pusha (doc-CI leggera/assente → auto). <b>Opt-in, default false</b>:
        /// il merge in main resta manuale. Richiede <see cref="UseAgentWorktrees"/>. Il merge del
        /// CODICE resta umano (§7e). Conflitto → not-ready (l'agente rilavora).
        /// </summary>
        /// <summary>
        /// <c>null</c> = non deciso → default come sopra (git presente ⇒ acceso). Il merge del
        /// CODICE resta umano in ogni caso (§7e): questo riguarda solo i deliverable-doc.
        /// </summary>
        public bool? AutoMergeAgentDeliverables { get; set; }
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
