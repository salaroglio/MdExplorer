using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    public class Project
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Path { get; set; }
        public virtual DateTime LastUpdate { get; set; }
        public virtual IList<Bookmark> Bookmarks { get; set; }
        public virtual int? SidenavWidth {  get; set; }
        public virtual string SelectedIde { get; set; }
        public virtual bool LinkIndexingEnabled { get; set; } = true;
        public virtual bool PlantUmlKeepOriginalColorsInDarkMode { get; set; } = false;
        public virtual bool UseCopilotCliAsDefault { get; set; } = true;
        public virtual bool ExecutionTrusted { get; set; } = false;
        public virtual bool ExcludeSubmodulesFromGitStatus { get; set; } = true;

        /// <summary>
        /// Isolamento worktree per-agente su QUESTA macchina. <c>null</c> = non deciso → default
        /// (acceso se il progetto è git con remoto <c>origin</c>). Vive qui e non nel
        /// <c>.development.yml</c> perché costa spazio disco locale: è una scelta della macchina,
        /// non una regola del repo.
        /// </summary>
        public virtual bool? UseAgentWorktrees { get; set; }

        /// <summary>
        /// Posti di lavoro (worktree) che gli agenti possono occupare contemporaneamente su
        /// questa macchina. <c>null</c> = non deciso → <b>2</b>. È anche il tetto fisico di
        /// quanti agenti girano insieme: il pool sostituisce il semaforo separato, così non
        /// esistono due limiti che possono dire cose diverse.
        /// </summary>
        public virtual int? AgentWorktreeSlots { get; set; }

        /// <summary>
        /// When ON, a SEPARATE background index (side-car FTS + TextFile table) is
        /// built and maintained for non-markdown text files. Completely additive:
        /// the markdown world (MarkdownFile / MdEngineFts) is never touched.
        /// Default OFF so existing projects behave exactly as before.
        /// </summary>
        public virtual bool IndexAllTextFiles { get; set; } = false;

        /// <summary>
        /// Per-project allow-list of extensions considered "text" for the text
        /// index, as a comma-separated list (e.g. ".txt,.csv,.json"). When null or
        /// empty the central default (<see cref="Services.TextFileClassifier"/>)
        /// applies. Deterministic by design: no binary sniffing.
        /// </summary>
        public virtual string TextFileExtensions { get; set; }

    }
}
