using System;
using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>Il grafo per la pagina del visualizzatore (D17): file come nodi, simboli in cascata dai punti d'ingresso (D24).</summary>
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
            /// <summary>Il nodo da cui questo è comparso (il file per un punto d'ingresso, chi lo chiama per gli altri): dove nasce nel disegno.</summary>
            public string ParentId { get; set; }
            /// <summary>Per un file: quanti simboli contiene; per un simbolo: quanti simboli del perimetro mostrerebbe aprendolo (il «+»).</summary>
            public int Hidden { get; set; }
            /// <summary>Un file aperto, o un simbolo di cui si vedono le chiamate.</summary>
            public bool Expanded { get; set; }
            public bool External { get; set; }
            /// <summary>Una funzione o un metodo che nessun simbolo del perimetro chiama: codice morto o un punto d'ingresso.</summary>
            public bool Uncalled { get; set; }
            /// <summary>Ciò che il simbolo tocca FUORI dal perimetro: non sono nodi del disegno, ma si elencano nella scheda.</summary>
            public List<ViewExternal> Externals { get; set; }
            /// <summary>I simboli che chiama e che l'utente ha nascosto (tasto destro): non si disegnano, ma la scheda li dice.</summary>
            public List<string> HiddenCalls { get; set; }
            /// <summary>Il livello nel disegno dall'alto in basso: 0 i file, 1 i punti d'ingresso, poi chi è chiamato, un livello più giù.</summary>
            public int Depth { get; set; }
        }

        public sealed class ViewExternal
        {
            public string Name { get; set; }
            public string QualifiedName { get; set; }
            public string Relation { get; set; }
            public string Confidence { get; set; }
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
        /// <summary>I file aperti: mostrano i loro punti d'ingresso.</summary>
        public HashSet<string> Expanded { get; } = new(StringComparer.Ordinal);
        /// <summary>I simboli aperti: mostrano chi chiamano (e i membri, se sono tipi).</summary>
        public HashSet<string> Opened { get; } = new(StringComparer.Ordinal);
        /// <summary>I simboli che l'utente ha tolto dal disegno (es. Write-Log, chiamata da ovunque): non compaiono, non si contano nel «+».</summary>
        public HashSet<string> HiddenSymbols { get; } = new(StringComparer.Ordinal);
        /// <summary>Il simbolo da mettere in evidenza: si mostra anche se nessuna cascata ci arriva.</summary>
        public string Focus { get; set; }
        public Dictionary<string, List<CodeSymbolView>> Outlines { get; } = new(StringComparer.Ordinal);
        public List<EdgeInput> Edges { get; } = new();
        /// <summary>Per file: a che cosa serve (la sua <c>code:FileDescription</c>), anche per i file chiusi — l'accordion del box.</summary>
        public Dictionary<string, string> FileSummaries { get; } = new(StringComparer.Ordinal);

        public sealed class EdgeInput
        {
            public string Source, Target, Relation, Confidence, TargetKind, TargetName, TargetQualifiedName;
        }
    }

    /// <summary>
    /// Costruisce il grafo della pagina «a cascata» (D24, 12/10). I file della cartella sono i nodi di primo livello. Un file
    /// aperto mostra solo i suoi <b>punti d'ingresso</b>: i simboli che nessun altro simbolo dello stesso file chiama né
    /// contiene (per uno script PowerShell lo script stesso, il codice fuori dalle funzioni; le funzioni che nessuno chiama
    /// restano radici e sono segnate). Un simbolo aperto mostra i simboli del perimetro che chiama o istanzia, anche in
    /// altri file, e ciò che contiene (i membri di un tipo, le funzioni annidate). Niente sparisce in silenzio: il «+» dice quanti simboli si aprirebbero, i nodi
    /// external non si disegnano ma ogni simbolo li elenca, e le funzioni/metodi che nessuno chiama sono segnati.
    /// Gli archi fra file non aperti restano, sommati, come vista d'insieme.
    /// </summary>
    public static class CodeGraphViewData
    {
        /// <summary>Le relazioni che fanno la cascata: chi si esegue da qui.</summary>
        private static readonly HashSet<string> CascadeRelations = new(StringComparer.Ordinal) { "calls", "instantiates" };
        private static readonly HashSet<string> CallableKinds = new(StringComparer.Ordinal) { "function", "method" };

        public static CodeGraphViewModel Build(CodeGraphViewInput input)
        {
            var model = new CodeGraphViewModel { Folder = input.FolderPrefix };

            // 1) i file della cartella
            var fileIds = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var (path, count) in input.Files)
            {
                var id = "file:" + path;
                fileIds[path] = id;
                var folder = FolderOf(path);
                if (!model.Folders.Contains(folder)) model.Folders.Add(folder);
                input.FileSummaries.TryGetValue(path, out var summary);
                model.Nodes.Add(new CodeGraphViewModel.ViewNode { Id = id, Kind = "file", Name = path.Substring(path.LastIndexOf('/') + 1), QualifiedName = path, Path = path, Folder = folder, Hidden = count, Expanded = input.Expanded.Contains(path), Summary = summary });
            }

            // 2) i simboli conosciuti (dai file letti) e gli archi per sorgente
            var symbols = new Dictionary<string, CodeSymbolView>(StringComparer.Ordinal);
            foreach (var outline in input.Outlines.Values)
                foreach (var s in outline) symbols[s.Id] = s;
            var outgoing = input.Edges.GroupBy(e => e.Source).ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);
            var called = new HashSet<string>(input.Edges.Where(e => CascadeRelations.Contains(e.Relation) && e.Source != e.Target).Select(e => e.Target), StringComparer.Ordinal);

            bool InPerimeter(string id) => input.SymbolFiles.ContainsKey(id);

            List<string> Children(string id)
            {
                var list = new List<string>();
                if (outgoing.TryGetValue(id, out var edges))
                    foreach (var e in edges)
                        if (CascadeRelations.Contains(e.Relation) && e.TargetKind != "external" && InPerimeter(e.Target) && e.Target != id && !list.Contains(e.Target)) list.Add(e.Target);
                // i membri di un tipo, le funzioni annidate: ciò che il simbolo contiene (non i campi)
                if (symbols.TryGetValue(id, out var s) && s.Kind != "module" && input.SymbolFiles.TryGetValue(id, out var path) && input.Outlines.TryGetValue(path, out var outline))
                    foreach (var m in outline)
                        if (m.ParentId == id && m.Kind != "field" && !list.Contains(m.Id)) list.Add(m.Id);
                return list;
            }

            // 3) chi si mostra: i punti d'ingresso dei file aperti, poi la cascata dei simboli aperti
            var shown = new Dictionary<string, string>(StringComparer.Ordinal);   // id → da chi è comparso
            var depth = new Dictionary<string, int>(StringComparer.Ordinal);
            var queue = new Queue<string>();
            void Show(string id, string from)
            {
                if (shown.ContainsKey(id) || !symbols.ContainsKey(id) || input.HiddenSymbols.Contains(id)) return;
                shown[id] = from;
                depth[id] = from.StartsWith("file:", StringComparison.Ordinal) ? 1 : depth[from] + 1;   // in ampiezza: il livello più alto
                queue.Enqueue(id);
            }
            foreach (var path in input.Expanded.Where(fileIds.ContainsKey))
                if (input.Outlines.TryGetValue(path, out var outline))
                    foreach (var root in EntryPoints(outline, outgoing, symbols)) Show(root.Id, fileIds[path]);
            if (input.Focus != null && input.SymbolFiles.TryGetValue(input.Focus, out var focusPath) && fileIds.TryGetValue(focusPath, out var focusFile))
                Show(input.Focus, focusFile);
            while (queue.Count > 0)
            {
                var id = queue.Dequeue();
                if (!input.Opened.Contains(id)) continue;
                foreach (var c in Children(id)) Show(c, id);
            }

            // 3b) un punto d'ingresso di un file aperto che un simbolo APERTO chiama sta sotto chi lo chiama, non sotto il
            //     suo file: la cascata è delle chiamate (D29). Mai sotto un proprio discendente (niente cicli).
            bool IsAncestor(string candidate, string of)
            {
                for (var cur = of; cur != null && shown.TryGetValue(cur, out var up); cur = up)
                    if (cur == candidate) return true;
                return false;
            }
            foreach (var id in shown.Keys.Where(k => shown[k].StartsWith("file:", StringComparison.Ordinal)).ToList())
            {
                var caller = input.Edges
                    .Where(e => e.Target == id && CascadeRelations.Contains(e.Relation) && e.Source != id && input.Opened.Contains(e.Source) && shown.ContainsKey(e.Source))
                    .Select(e => e.Source)
                    .FirstOrDefault(c => !IsAncestor(id, c));
                if (caller != null) shown[id] = caller;
            }
            // il livello segue chi è il padre adesso
            int DepthOf(string id) => shown[id].StartsWith("file:", StringComparison.Ordinal) ? 1 : DepthOf(shown[id]) + 1;
            foreach (var id in shown.Keys.ToList()) depth[id] = DepthOf(id);

            // 4) i nodi dei simboli mostrati
            foreach (var (id, from) in shown)
            {
                var s = symbols[id];
                var allChildren = Children(id);
                var children = allChildren.Where(c => !input.HiddenSymbols.Contains(c)).ToList();
                model.Nodes.Add(new CodeGraphViewModel.ViewNode
                {
                    Id = s.Id, Kind = s.Kind, Name = s.Name, QualifiedName = s.QualifiedName, Path = s.Path, Line = s.LineStart, Signature = s.Signature,
                    // lo script stesso (module) è il file: la sua descrizione è quella del file (D36)
                    Summary = s.Summary ?? (s.Kind == "module" && s.Path != null && input.FileSummaries.TryGetValue(s.Path, out var fileSummary) ? fileSummary : null),
                    ParentId = from, Folder = FolderOf(s.Path ?? string.Empty),
                    Expanded = input.Opened.Contains(id),
                    Depth = depth[id],
                    HiddenCalls = allChildren.Where(input.HiddenSymbols.Contains).Select(c => symbols.TryGetValue(c, out var hs) ? hs.Name : c).ToList(),
                    Hidden = children.Count(c => !shown.ContainsKey(c)),
                    Uncalled = CallableKinds.Contains(s.Kind) && !called.Contains(id),
                    Externals = outgoing.TryGetValue(id, out var edges)
                        ? edges.Where(e => e.TargetKind == "external" || !InPerimeter(e.Target))
                               .Select(e => new CodeGraphViewModel.ViewExternal { Name = e.TargetName, QualifiedName = e.TargetQualifiedName, Relation = e.Relation, Confidence = e.Confidence })
                               .OrderBy(x => x.Relation, StringComparer.Ordinal).ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToList()
                        : new List<CodeGraphViewModel.ViewExternal>(),
                });
            }

            // 5) gli archi: fra simboli mostrati; il legame con chi li ha fatti comparire; fra file non aperti, sommati
            var links = new Dictionary<string, CodeGraphViewModel.ViewLink>(StringComparer.Ordinal);
            void Link(string a, string b, string relation, string confidence)
            {
                if (a == null || b == null || a == b) return;
                var key = a + "|" + b + "|" + relation;
                if (links.TryGetValue(key, out var l))
                {
                    l.Count++;
                    if (Rank(confidence) > Rank(l.Confidence)) l.Confidence = confidence;   // il peggiore vince: ambiguous > llm-resolved > inferred > extracted
                }
                else links[key] = new CodeGraphViewModel.ViewLink { Source = a, Target = b, Relation = relation, Confidence = confidence, Count = 1 };
            }
            foreach (var e in input.Edges)
            {
                if (e.TargetKind == "external") continue;
                if (input.HiddenSymbols.Contains(e.Source) || input.HiddenSymbols.Contains(e.Target)) continue;   // nascosto: neanche negli archi fra file
                var sourceShown = shown.ContainsKey(e.Source);
                var targetShown = shown.ContainsKey(e.Target);
                if (sourceShown && targetShown) { Link(e.Source, e.Target, e.Relation, e.Confidence); continue; }
                if (sourceShown || targetShown) continue;                            // è nel «+» del simbolo mostrato
                if (!input.SymbolFiles.TryGetValue(e.Source, out var pa) || !input.SymbolFiles.TryGetValue(e.Target, out var pb)) continue;
                if (input.Expanded.Contains(pa) || input.Expanded.Contains(pb)) continue;   // un file aperto parla con i suoi simboli
                if (fileIds.TryGetValue(pa, out var fa) && fileIds.TryGetValue(pb, out var fb)) Link(fa, fb, e.Relation, e.Confidence);
            }
            foreach (var (id, from) in shown)
            {
                // un punto d'ingresso pende dal suo file; un membro dal suo tipo (le chiamate hanno già il loro arco)
                // (un membro chiamato dal suo contenitore ha già l'arco calls: niente contains doppio)
                if (from.StartsWith("file:", StringComparison.Ordinal)) Link(from, id, "contains", "extracted");
                else if (symbols[id].ParentId == from && !links.Values.Any(l => l.Source == from && l.Target == id)) Link(from, id, "contains", "extracted");
            }
            model.Links.AddRange(links.Values);
            return model;
        }

        /// <summary>
        /// I punti d'ingresso di un file: i simboli che nessun altro simbolo dello stesso file chiama, istanzia o contiene.
        /// Un modulo conta solo se esegue qualcosa (uno script con codice fuori dalle funzioni); i campi no.
        /// </summary>
        public static List<CodeSymbolView> EntryPoints(IReadOnlyList<CodeSymbolView> outline, IReadOnlyDictionary<string, List<CodeGraphViewInput.EdgeInput>> outgoing, IReadOnlyDictionary<string, CodeSymbolView> symbols)
        {
            var inFile = new HashSet<string>(outline.Select(s => s.Id), StringComparer.Ordinal);
            var reachedInFile = new HashSet<string>(StringComparer.Ordinal);
            foreach (var s in outline)
            {
                if (!outgoing.TryGetValue(s.Id, out var edges)) continue;
                foreach (var e in edges)
                    if (CascadeRelations.Contains(e.Relation) && e.Target != s.Id && inFile.Contains(e.Target)) reachedInFile.Add(e.Target);
            }
            return outline.Where(s =>
            {
                if (s.Kind is "field" or "external") return false;
                if (s.Kind == "module") return outgoing.TryGetValue(s.Id, out var me) && me.Any(e => CascadeRelations.Contains(e.Relation));
                if (reachedInFile.Contains(s.Id)) return false;
                // dentro un tipo o una funzione dello stesso file: ci si arriva aprendo il contenitore
                if (s.ParentId != null && inFile.Contains(s.ParentId) && symbols.TryGetValue(s.ParentId, out var parent) && parent.Kind != "module") return false;
                return true;
            }).ToList();
        }

        private static string FolderOf(string path) => path.Contains('/') ? path.Substring(0, path.LastIndexOf('/')) : string.Empty;

        private static int Rank(string confidence) => confidence switch { "ambiguous" => 3, CodeOntology.LlmResolved => 2, "inferred" => 1, _ => 0 };   // llm-resolved: una deduzione, fra le due
    }
}
