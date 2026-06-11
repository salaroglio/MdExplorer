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
