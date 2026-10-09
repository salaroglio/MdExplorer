using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>Il grafo per la pagina del visualizzatore (D17): file come nodi, simboli dei file aperti, archi fra nodi visibili.</summary>
    public sealed class CodeGraphViewModel
    {
        public string Folder { get; set; }
        public List<ViewNode> Nodes { get; } = new();
        public List<ViewLink> Links { get; } = new();
        public List<string> Folders { get; } = new();

        public sealed class ViewNode
        {
            public string Id { get; set; }
            /// <summary><c>file</c> oppure il kind del simbolo.</summary>
            public string Kind { get; set; }
            public string Name { get; set; }
            public string QualifiedName { get; set; }
            public string Path { get; set; }
            public string Folder { get; set; }
            public int? Line { get; set; }
            public string Signature { get; set; }
            public string Summary { get; set; }
            public string ParentId { get; set; }
            /// <summary>Per un file: quanti simboli contiene (per il «+»); per un simbolo: quanti archi verso nodi non mostrati.</summary>
            public int Hidden { get; set; }
            public bool Expanded { get; set; }
            public bool External { get; set; }
        }

        public sealed class ViewLink
        {
            public string Source { get; set; }
            public string Target { get; set; }
            public string Relation { get; set; }
            public string Confidence { get; set; }
            public int Count { get; set; }
        }
    }

    /// <summary>
    /// Costruisce il grafo della pagina: i file della cartella sono i nodi di primo livello; un file «aperto» mostra i
    /// suoi simboli; gli archi fra simboli si proiettano sui file quando il simbolo non è mostrato (un arco file→file
    /// «calls» con il conteggio). I nodi external compaiono solo se un simbolo mostrato li tocca.
    /// </summary>
    public static class CodeGraphViewData
    {
        public static CodeGraphViewModel Build(SqliteConnection c, string folderPrefix, IReadOnlyCollection<string> expandedFiles, string focusSymbolId, int maxFiles = 400)
        {
            var model = new CodeGraphViewModel { Folder = folderPrefix };
            var prefix = string.IsNullOrEmpty(folderPrefix) ? string.Empty : folderPrefix.TrimEnd('/') + "/";

            // 1) i file della cartella
            var files = new List<(string Path, int Count)>();
            using (var cmd = c.CreateCommand())
            {
                cmd.CommandText = "SELECT Path, SymbolCount FROM code_file WHERE Path LIKE $p ORDER BY Path LIMIT $n";
                cmd.Parameters.AddWithValue("$p", prefix + "%");
                cmd.Parameters.AddWithValue("$n", maxFiles);
                using var r = cmd.ExecuteReader();
                while (r.Read()) files.Add((r.GetString(0), r.GetInt32(1)));
            }
            var expanded = new HashSet<string>(expandedFiles ?? Array.Empty<string>(), StringComparer.Ordinal);
            if (focusSymbolId != null)
            {
                var focus = CodeGraphQueries.GetSymbol(c, focusSymbolId);
                if (focus?.Path != null) expanded.Add(focus.Path);
            }
            var fileIds = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var (path, count) in files)
            {
                var id = "file:" + path;
                fileIds[path] = id;
                var folder = path.Contains('/') ? path.Substring(0, path.LastIndexOf('/')) : string.Empty;
                if (!model.Folders.Contains(folder)) model.Folders.Add(folder);
                model.Nodes.Add(new CodeGraphViewModel.ViewNode { Id = id, Kind = "file", Name = path.Substring(path.LastIndexOf('/') + 1), QualifiedName = path, Path = path, Folder = folder, Hidden = count, Expanded = expanded.Contains(path) });
            }

            // 2) i simboli dei file aperti (non i parametri, non i campi privati: il grafo mostra ciò che si usa da fuori)
            var shownSymbols = new Dictionary<string, CodeGraphViewModel.ViewNode>(StringComparer.Ordinal);
            var symbolFile = new Dictionary<string, string>(StringComparer.Ordinal);   // ogni simbolo in perimetro → il suo file
            using (var cmd = c.CreateCommand())
            {
                cmd.CommandText = "SELECT Id, Path FROM code_symbol WHERE Path LIKE $p";
                cmd.Parameters.AddWithValue("$p", prefix + "%");
                using var r = cmd.ExecuteReader();
                while (r.Read()) symbolFile[r.GetString(0)] = r.GetString(1);
            }
            foreach (var path in expanded.Where(fileIds.ContainsKey))
            {
                foreach (var s in CodeGraphQueries.Outline(c, path))
                {
                    if (s.Kind is "field" or "module") continue;
                    var n = new CodeGraphViewModel.ViewNode
                    {
                        Id = s.Id, Kind = s.Kind, Name = s.Name, QualifiedName = s.QualifiedName, Path = s.Path, Line = s.LineStart, Signature = s.Signature,
                        Summary = s.Summary, ParentId = s.ParentId != null && shownSymbols.ContainsKey(s.ParentId) ? s.ParentId : fileIds[path],
                        Folder = path.Contains('/') ? path.Substring(0, path.LastIndexOf('/')) : string.Empty,
                    };
                    shownSymbols[s.Id] = n;
                    model.Nodes.Add(n);
                }
            }

            // 3) gli archi: fra simboli mostrati, altrimenti proiettati sui file; i nodi external appaiono solo se toccati da un simbolo mostrato
            var links = new Dictionary<string, CodeGraphViewModel.ViewLink>(StringComparer.Ordinal);
            var externals = new Dictionary<string, CodeGraphViewModel.ViewNode>(StringComparer.Ordinal);
            using (var cmd = c.CreateCommand())
            {
                cmd.CommandText =
                    "SELECT e.SourceId, e.TargetId, e.Relation, e.Confidence, t.Kind, t.Name, t.QualifiedName FROM code_edge e JOIN code_symbol t ON t.Id = e.TargetId " +
                    "WHERE e.Relation <> 'contains' AND e.Path LIKE $p";
                cmd.Parameters.AddWithValue("$p", prefix + "%");
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    var src = r.GetString(0); var tgt = r.GetString(1); var rel = r.GetString(2); var conf = r.GetString(3);
                    var tKind = r.GetString(4);
                    string a = Project(src), b;
                    if (tKind == "external")
                    {
                        if (!shownSymbols.ContainsKey(src)) continue;      // external solo da un simbolo mostrato
                        if (!externals.TryGetValue(tgt, out var ext))
                            externals[tgt] = ext = new CodeGraphViewModel.ViewNode { Id = tgt, Kind = "external", Name = r.GetString(5), QualifiedName = r.GetString(6), External = true };
                        b = tgt;
                    }
                    else b = Project(tgt);
                    if (a == null || b == null || a == b) continue;
                    var key = a + "|" + b + "|" + rel;
                    if (links.TryGetValue(key, out var l))
                    {
                        l.Count++;
                        if (Rank(conf) > Rank(l.Confidence)) l.Confidence = conf;   // il peggiore vince: ambiguous > inferred > extracted
                    }
                    else links[key] = new CodeGraphViewModel.ViewLink { Source = a, Target = b, Relation = rel, Confidence = conf, Count = 1 };
                }
            }
            model.Nodes.AddRange(externals.Values);
            model.Links.AddRange(links.Values);

            // 4) il «+» dei simboli: quanti vicini non sono mostrati
            foreach (var s in shownSymbols.Values)
                s.Hidden = model.Links.Count(l => (l.Source == s.Id || l.Target == s.Id));
            return model;

            string Project(string symbolId)
            {
                if (shownSymbols.ContainsKey(symbolId)) return symbolId;
                return symbolFile.TryGetValue(symbolId, out var path) && fileIds.TryGetValue(path, out var fid) ? fid : null;
            }
        }

        private static int Rank(string confidence) => confidence switch { "ambiguous" => 2, "inferred" => 1, _ => 0 };
    }
}
