using MdExplorer.Features.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MdExplorer.Features.Utilities;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>Lo stato di una radice di codice esterna su questa macchina (R1, sempre visibile nella UI).</summary>
    public enum CodeRootStatus
    {
        /// <summary>Collegata: la cartella c'è ed è quella dichiarata.</summary>
        Linked,
        /// <summary>Dichiarata nel progetto, ma su questa macchina nessuno ha detto dove sta.</summary>
        NotLinked,
        /// <summary>Collegata, ma la cartella (o la sottocartella dichiarata) non c'è più.</summary>
        NotFound,
        /// <summary>La cartella c'è ma non è il repository dichiarato (remote diverso, o non è più un repository).</summary>
        Mismatch,
    }

    public sealed class CodeRootState
    {
        public string Alias { get; init; }
        public string Repository { get; init; }
        public string Subpath { get; init; }
        public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
        public IReadOnlyList<string> Languages { get; init; } = Array.Empty<string>();
        /// <summary>La cartella collegata su questa macchina (radice del repository, o la cartella se non è git); null se non collegata.</summary>
        public string LocalPath { get; init; }
        /// <summary>La cartella che si analizza: <see cref="LocalPath"/> più <see cref="Subpath"/>. Null se non collegata.</summary>
        public string RootPath { get; init; }
        public CodeRootStatus Status { get; init; }
        /// <summary>Perché non è collegata, in italiano, con il rimedio; null se collegata.</summary>
        public string Reason { get; init; }
        /// <summary>I remote trovati nella cartella collegata (per dire «questa cartella è X, non Y»).</summary>
        public IReadOnlyList<string> ActualRemotes { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// La traduzione fra percorsi logici e percorsi di questa macchina, su una fotografia delle radici (sprint
    /// 2026-10-12-Radici-Di-Codice-Esterne, R2). Puro: lo crea <c>CodeRootService</c> una volta per operazione e lo si
    /// usa per migliaia di file senza ripetere le chiamate a git.
    /// <list type="bullet">
    /// <item><c>@alias/percorso</c> → dentro la radice collegata; un <c>..</c> che ne esce è rifiutato.</item>
    /// <item>un percorso senza <c>@</c> → relativo alla radice del progetto, come oggi.</item>
    /// </list>
    /// Una radice non collegata non si risolve: l'errore dice perché e come rimediare (mai un percorso inventato).
    /// </summary>
    public sealed class CodeRootResolver
    {
        public string ProjectPath { get; }
        public IReadOnlyList<CodeRootState> Roots { get; }
        private readonly Dictionary<string, CodeRootState> _byAlias;

        public CodeRootResolver(string projectPath, IEnumerable<CodeRootState> roots)
        {
            ProjectPath = Path.GetFullPath(projectPath);
            Roots = (roots ?? Enumerable.Empty<CodeRootState>()).ToList();
            _byAlias = Roots.ToDictionary(r => r.Alias, StringComparer.Ordinal);
        }

        /// <summary>Il percorso assoluto su questa macchina; eccezione con il motivo se non si può.</summary>
        public string Resolve(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("percorso vuoto");
            if (!CodeRootPaths.IsLogical(path))
            {
                var inProject = Path.GetFullPath(Path.Combine(ProjectPath, path.Replace('/', Path.DirectorySeparatorChar)));
                if (!PathContainment.IsInside(ProjectPath, inProject))
                    throw new InvalidOperationException(UiText.Say($"'{path}' esce dal progetto.", $"'{path}' goes outside the project."));
                return inProject;
            }
            var (alias, rest) = CodeRootPaths.Parse(path);
            if (!_byAlias.TryGetValue(alias, out var root))
                throw new InvalidOperationException(UiText.Say($"La radice «@{alias}» non è dichiarata nel .development.yml di questo progetto.", $"The root «@{alias}» is not declared in this project's .development.yml."));
            if (root.Status != CodeRootStatus.Linked)
                throw new InvalidOperationException(UiText.Say($"La radice «@{alias}» non è utilizzabile su questa macchina: {root.Reason}", $"The root «@{alias}» cannot be used on this machine: {root.Reason}"));
            var full = Path.GetFullPath(rest.Length == 0 ? root.RootPath : Path.Combine(root.RootPath, rest.Replace('/', Path.DirectorySeparatorChar)));
            if (!PathContainment.IsInside(root.RootPath, full))
                throw new InvalidOperationException(UiText.Say($"'{path}' esce dalla radice «@{alias}».", $"'{path}' goes outside the root «@{alias}»."));
            return full;
        }

        /// <summary>
        /// Il percorso logico di un file di questa macchina: relativo al progetto se ci sta dentro, <c>@alias/…</c> se sta
        /// in una radice collegata, null se non sta da nessuna parte che il progetto conosca.
        /// </summary>
        public string ToLogical(string absolutePath)
        {
            if (string.IsNullOrWhiteSpace(absolutePath)) return null;
            var inProject = PathContainment.RelativeOrNull(ProjectPath, absolutePath);
            if (inProject != null) return inProject == "." ? string.Empty : inProject;
            foreach (var root in Roots.Where(r => r.Status == CodeRootStatus.Linked))
            {
                var rel = PathContainment.RelativeOrNull(root.RootPath, absolutePath);
                if (rel != null) return CodeRootPaths.Format(root.Alias, rel == "." ? string.Empty : rel);
            }
            return null;
        }

        /// <summary>Le radici collegate: quelle a cui MarkAgent può accedere (F3) e che si possono analizzare.</summary>
        public IEnumerable<CodeRootState> Linked => Roots.Where(r => r.Status == CodeRootStatus.Linked);
    }
}
