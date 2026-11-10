using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// I file di una radice di codice per la scoperta della chiusura (F2, piano 2026-11-10-Analisi-Di-Programmi-Singoli):
    /// l'elenco e il testo, senza analizzare niente. Su disco (<see cref="LocalCodeWorkspace"/>) o, in F2c, su un server via
    /// MDE Remote. I percorsi sono logici (come negli script: <c>program/…</c>, <c>@alias/…</c>).
    /// </summary>
    public interface ICodeWorkspace
    {
        /// <summary>Tutti i file della radice (percorsi logici), senza le cartelle che lo scanner salta.</summary>
        IReadOnlyList<string> Files();
        string Read(string logicalPath);
        /// <summary>Il file su disco (o l'indirizzo sul server) di un percorso logico, per l'estrattore.</summary>
        CodeSourceFile Source(string logicalPath);
    }

    /// <summary>Una cartella su disco: <paramref name="logicalPrefix"/> è il suo percorso logico (<c>""</c> = la radice del progetto).</summary>
    public sealed class LocalCodeWorkspace : ICodeWorkspace
    {
        private readonly string _folder, _prefix;
        private readonly IReadOnlyDictionary<string, string> _encodings;
        private readonly ISet<string> _skip;
        private List<string> _files;

        public LocalCodeWorkspace(string folder, string logicalPrefix, IReadOnlyDictionary<string, string> encodings = null, IEnumerable<string> skipFolders = null)
        {
            _folder = Path.GetFullPath(folder ?? throw new ArgumentNullException(nameof(folder)));
            _prefix = (logicalPrefix ?? string.Empty).Replace('\\', '/').Trim('/');
            _encodings = encodings;
            _skip = new HashSet<string>(skipFolders ?? new[] { ".git", ".svn", ".mde-code", ".mde-languages", ".md", "node_modules", "bin", "obj", "target" }, StringComparer.OrdinalIgnoreCase);
        }

        public IReadOnlyList<string> Files()
        {
            if (_files != null) return _files;
            var list = new List<string>();
            var stack = new Stack<string>(new[] { _folder });
            while (stack.Count > 0)
            {
                var dir = stack.Pop();
                foreach (var f in Directory.EnumerateFiles(dir)) list.Add(Logical(f));
                foreach (var d in Directory.EnumerateDirectories(dir))
                    if (!_skip.Contains(Path.GetFileName(d))) stack.Push(d);
            }
            list.Sort(StringComparer.Ordinal);
            return _files = list;
        }

        private string Logical(string absolute)
        {
            var rel = Path.GetRelativePath(_folder, absolute).Replace('\\', '/');
            return _prefix.Length == 0 ? rel : _prefix + "/" + rel;
        }

        private string Absolute(string logical)
        {
            var rel = _prefix.Length == 0 ? logical : logical.StartsWith(_prefix + "/", StringComparison.Ordinal) ? logical[(_prefix.Length + 1)..]
                : throw new ArgumentException(UiText.Say($"{logical}: non sta in {_prefix}", $"{logical}: not inside {_prefix}"));
            return Path.Combine(_folder, rel.Replace('/', Path.DirectorySeparatorChar));
        }

        public string Read(string logicalPath) => CodeSourceText.Read(Absolute(logicalPath), _encodings);

        public CodeSourceFile Source(string logicalPath) => new(Absolute(logicalPath), logicalPath);
    }
}
