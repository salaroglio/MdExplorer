using System;
using System.Collections.Generic;
using System.Linq;

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

    /// <summary>Quello che lo store legge dal grafo per costruire la pagina: così la costruzione è una funzione pura.</summary>
    public sealed class CodeGraphViewInput
    {
        public string FolderPrefix { get; set; }
        public List<(string Path, int Count)> Files { get; } = new();
        /// <summary>Ogni simbolo nel perimetro → il suo file.</summary>
        public Dictionary<string, string> SymbolFiles { get; } = new(StringComparer.Ordinal);
        public HashSet<string> Expanded { get; } = new(StringComparer.Ordinal);
        public Dictionary<string, List<CodeSymbolView>> Outlines { get; } = new(StringComparer.Ordinal);
        public List<EdgeInput> Edges { get; } = new();

        public sealed class EdgeInput
        {
            public string Source, Target, Relation, Confidence, TargetKind, TargetName, TargetQualifiedName;
        }
    }

    /// <summary>
    /// Costruisce il grafo della pagina: i file della cartella sono i nodi di primo livello; un file «aperto» mostra i
    /// suoi simboli; gli archi fra simboli si proiettano sui file quando il simbolo non è mostrato (un arco file→file
    /// «calls» con il conteggio). I nodi external compaiono solo se un simbolo mostrato li tocca.
    /// </summary>
    public static class CodeGraphViewData
    {
        public static CodeGraphViewModel Build(CodeGraphViewInput input)
        {
            var model = new CodeGraphViewModel { Folder = input.FolderPrefix };

            // 1) i file della cartella
            var fileIds = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var (path, count) in input.Files)
            {
                var id = "file:" + path;
                fileIds[path] = id;
                var folder = path.Contains('/') ? path.Substring(0, path.LastIndexOf('/')) : string.Empty;
                if (!model.Folders.Contains(folder)) model.Folders.Add(folder);
                model.Nodes.Add(new CodeGraphViewModel.ViewNode { Id = id, Kind = "file", Name = path.Substring(path.LastIndexOf('/') + 1), QualifiedName = path, Path = path, Folder = folder, Hidden = count, Expanded = input.Expanded.Contains(path) });
            }

            // 2) i simboli dei file aperti (non i campi, non i moduli: il grafo mostra ciò che si usa da fuori)
            var shownSymbols = new Dictionary<string, CodeGraphViewModel.ViewNode>(StringComparer.Ordinal);
            foreach (var path in input.Expanded.Where(fileIds.ContainsKey))
            {
                if (!input.Outlines.TryGetValue(path, out var outline)) continue;
                foreach (var s in outline)
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

            // 3) gli archi: fra simboli mostrati, altrimenti proiettati sui file; external solo se toccati da un simbolo mostrato
            var links = new Dictionary<string, CodeGraphViewModel.ViewLink>(StringComparer.Ordinal);
            var externals = new Dictionary<string, CodeGraphViewModel.ViewNode>(StringComparer.Ordinal);
            foreach (var e in input.Edges)
            {
                string a = Project(e.Source), b;
                if (e.TargetKind == "external")
                {
                    if (!shownSymbols.ContainsKey(e.Source)) continue;
                    if (!externals.ContainsKey(e.Target))
                        externals[e.Target] = new CodeGraphViewModel.ViewNode { Id = e.Target, Kind = "external", Name = e.TargetName, QualifiedName = e.TargetQualifiedName, External = true };
                    b = e.Target;
                }
                else b = Project(e.Target);
                if (a == null || b == null || a == b) continue;
                var key = a + "|" + b + "|" + e.Relation;
                if (links.TryGetValue(key, out var l))
                {
                    l.Count++;
                    if (Rank(e.Confidence) > Rank(l.Confidence)) l.Confidence = e.Confidence;   // il peggiore vince: ambiguous > inferred > extracted
                }
                else links[key] = new CodeGraphViewModel.ViewLink { Source = a, Target = b, Relation = e.Relation, Confidence = e.Confidence, Count = 1 };
            }
            model.Nodes.AddRange(externals.Values);
            model.Links.AddRange(links.Values);

            // 4) il «+» dei simboli: quanti archi li toccano
            foreach (var s in shownSymbols.Values)
                s.Hidden = model.Links.Count(l => l.Source == s.Id || l.Target == s.Id);
            return model;

            string Project(string symbolId)
            {
                if (shownSymbols.ContainsKey(symbolId)) return symbolId;
                return input.SymbolFiles.TryGetValue(symbolId, out var path) && fileIds.TryGetValue(path, out var fid) ? fid : null;
            }
        }

        private static int Rank(string confidence) => confidence switch { "ambiguous" => 2, "inferred" => 1, _ => 0 };
    }
}
