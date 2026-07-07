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
