using System;
using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// La scoperta della chiusura di un seme Java (F2b, 10/11/2026; S6, S8): come <see cref="DeclarativeClosure"/>, verso il
    /// basso e per simbolo, leggendo ogni file da solo con l'estrattore Java. Le dipendenze sono gli esterni del file
    /// (<c>type:a.b.C</c>, e <c>method:a.b.C.run/1@9</c> con quanti argomenti, F1a).
    /// <para>
    /// Dove sta un tipo: per convenzione di percorso — <c>a.b.C</c> è il file che finisce per <c>a/b/C.java</c>, qualunque sia
    /// la cartella dei sorgenti; per una classe annidata si risale (<c>a.b.C.D</c> → <c>a/b/C.java</c>). Di un tipo raggiunto
    /// entrano la classe, i costruttori, i campi e le madri; dei metodi solo quelli chiamati, per numero di argomenti, nel
    /// tipo o nelle madri (si cercano dopo aver trovato le madri). Le implementazioni di un'interfaccia sono F4.
    /// </para>
    /// </summary>
    public sealed class JavaClosure
    {
        private readonly ICodeExtractor _extractor;
        private readonly ICodeWorkspace _ws;

        public JavaClosure(ICodeExtractor javaExtractor, ICodeWorkspace workspace) { _extractor = javaExtractor; _ws = workspace; }

        /// <summary>Quante volte al più si rileggono insieme i file della chiusura (F4b): di solito bastano due o tre giri.</summary>
        public const int MaxRounds = 5;

        private sealed class State
        {
            public CodeClosureFile Node;
            public string Text;
            public CodeScript Script;
            public Dictionary<string, ScriptSymbol> ById;
            public HashSet<string> Followed = new(StringComparer.Ordinal);
        }

        public CodeClosure Compute(CodeClosureRequest request)
        {
            var result = new CodeClosure();
            var javaFiles = _ws.Files().Where(p => p.EndsWith(".java", StringComparison.OrdinalIgnoreCase)).ToList();
            var states = new Dictionary<string, State>(StringComparer.Ordinal);
            var misses = new Dictionary<string, CodeClosureMiss>(StringComparer.Ordinal);
            var work = new Queue<(string Path, string TypeFqn, bool Whole, int Depth, string From, string Written)>();
            var pendingMethods = new List<(string Owner, string Name, int Argc, string Written, string From, int Depth)>();
            var typeOf = new Dictionary<string, (string Path, string Id)>(StringComparer.Ordinal);   // fqn → file e simbolo, dei file letti
            foreach (var seed in request.Seeds) work.Enqueue((seed, null, true, 0, null, null));

            void Miss(string kind, string written, string from, IReadOnlyList<string> candidates = null)
            {
                var key = kind + "|" + written;
                if (!misses.TryGetValue(key, out var m)) misses[key] = m = new CodeClosureMiss { Kind = kind, Written = written, Candidates = candidates ?? Array.Empty<string>() };
                m.Uses++;
                if (from != null && m.From.Count < 5 && !m.From.Contains(from)) m.From.Add(from);
            }

            // il file di un tipo per convenzione di percorso; null = non trovato, più di uno = ambiguo (detto da Miss)
            string FileOfType(string fqn, string written, string from)
            {
                if (typeOf.TryGetValue(fqn, out var known)) return known.Path;
                var parts = fqn.Split('.');
                for (var n = parts.Length; n >= 2; n--)
                {
                    var suffix = string.Join("/", parts.Take(n)) + ".java";
                    var hits = javaFiles.Where(p => p == suffix || p.EndsWith("/" + suffix, StringComparison.Ordinal)).ToList();
                    if (hits.Count == 1) return hits[0];
                    if (hits.Count > 1) { Miss("ambiguous", written, from, hits); return null; }
                }
                Miss("unresolved", written, from);
                return null;
            }

            State Visit(string path, int depth, string from, string written)
            {
                if (states.TryGetValue(path, out var st)) return st;
                if (request.Pruned.Contains(path)) { Miss("pruned", written, from, new[] { path }); return null; }
                var text = _ws.Read(path);
                var lines = text.Count(c => c == '\n') + 1;
                if (result.Files.Count + 1 > request.MaxFiles || result.Lines + lines > request.MaxLines)
                {
                    result.BudgetReached = true;
                    Miss("frontier", written, from, new[] { path });
                    return null;
                }
                var source = _ws.Source(path);
                var extracted = _extractor.Extract(new CodeExtractionRequest { ProjectPath = "", AllFiles = new[] { source }, FilesToEmit = new[] { source }, Sources = new OneText(path, text) });
                var script = extracted.Scripts.Single().Script;
                st = states[path] = new State
                {
                    Text = text,
                    Script = script,
                    ById = script.Symbols.Where(s => s.Id != null).GroupBy(s => s.Id).ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal),
                    Node = new CodeClosureFile { Path = path, Depth = depth, From = from, Written = written, Lines = lines },
                };
                foreach (var t in script.Symbols.Where(CodeCrossLinker.IsJavaType)) typeOf.TryAdd(t.QualifiedName, (path, t.Id));
                result.Files.Add(st.Node);
                return st;
            }

            // simbolo → il file che lo dichiara, fra quelli già letti insieme (giri «tutti insieme»); null nella discesa file per file
            Dictionary<string, State> owners = null;
            // F4c: per ogni metodo di interfaccia raggiunto, le implementazioni viste nei file letti insieme (file → metodo)
            var contracts = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
            var decided = new Dictionary<string, CodeClosureImplementations>(StringComparer.Ordinal);
            var implementorsCache = new Dictionary<string, List<(string Path, string TypeFqn)>>(StringComparer.Ordinal);
            // le righe con implements/extends di tutta la radice, lette UNA volta (un grep): poi ogni tipo si cerca in memoria
            List<(string Path, string Text)> heritage = null;
            Discover();
            // F4b (10/11/2026): letti da soli, i file non sanno il tipo di un ricevente che viene da un altro file (a().b()). Ora i
            // file della chiusura si rileggono INSIEME, come un piccolo contesto: l'estrattore deduce i riceventi, le madri, le
            // implementazioni che stanno nel perimetro; gli id dei simboli non cambiano, quindi il già raggiunto resta raggiunto.
            // Si ripassano gli archi dei simboli raggiunti, entra ciò che è nuovo, e si rifà finché non entra più niente.
            for (var round = 0; round < MaxRounds && result.Files.Count > 1; round++)
            {
                var paths = result.Files.Select(f => f.Path).ToList();
                var sources = paths.Select(_ws.Source).ToList();
                var together = _extractor.Extract(new CodeExtractionRequest { ProjectPath = "", AllFiles = sources, FilesToEmit = sources,
                    Sources = new Texts(states.ToDictionary(kv => kv.Key, kv => kv.Value.Text, StringComparer.Ordinal)) });
                foreach (var (file, script) in together.Scripts)
                {
                    var st = states[file.LogicalPath];
                    st.Script = script;
                    st.ById = script.Symbols.Where(x => x.Id != null).GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
                }
                owners = new Dictionary<string, State>(StringComparer.Ordinal);
                foreach (var st in states.Values)
                    foreach (var x in st.Script.Symbols.Where(x => x.Id != null && x.Kind != "external")) owners.TryAdd(x.Id, st);
                var filesBefore = result.Files.Count;
                var followedBefore = states.Values.Sum(v => v.Followed.Count);
                // i non trovati si ricalcolano sugli script di adesso; potature e frontiera restano
                foreach (var key in misses.Where(kv => kv.Value.Kind is "unresolved" or "ambiguous").Select(kv => kv.Key).ToList()) misses.Remove(key);
                foreach (var st in states.Values.ToList())
                    foreach (var id in st.Followed.ToList()) Follow(st, id, rescan: true);
                Discover();
                // F4c: le implementazioni — una sola nella radice o scelta dall'utente si segue, le altre si propongono
                DecideImplementations();
                Discover();
                if (result.Files.Count == filesBefore && states.Values.Sum(v => v.Followed.Count) == followedBefore) break;
            }
            result.Implementations.AddRange(decided.Values.Where(d => d.Candidates.Count > 0).OrderBy(d => d.Contract, StringComparer.Ordinal));
            result.Misses.AddRange(misses.Values.OrderByDescending(m => m.Uses).ThenBy(m => m.Written, StringComparer.Ordinal));
            return result;

            // le classi della radice che implementano (o estendono) il tipo: il nome semplice nelle righe, poi il parser conferma il nome qualificato
            List<(string Path, string TypeFqn)> ImplementorsInRoot(string contractType)
            {
                if (implementorsCache.TryGetValue(contractType, out var known)) return known;
                heritage ??= _ws.Grep(new System.Text.RegularExpressions.Regex(@"\b(implements|extends)\b"), new[] { ".java" }).Select(h => (h.Path, h.Text)).ToList();
                var simple = contractType[(contractType.LastIndexOf('.') + 1)..];
                var pattern = new System.Text.RegularExpressions.Regex(@"\b(implements|extends)\b[^{]*\b" + System.Text.RegularExpressions.Regex.Escape(simple) + @"\b");
                var found = new List<(string, string)>();
                foreach (var path in heritage.Where(h => pattern.IsMatch(h.Text)).Select(h => h.Path).Distinct())
                {
                    string text;
                    try { text = states.TryGetValue(path, out var st) ? st.Text : _ws.Read(path); } catch (Exception) { continue; }
                    var source = _ws.Source(path);
                    var one = _extractor.Extract(new CodeExtractionRequest { ProjectPath = "", AllFiles = new[] { source }, FilesToEmit = new[] { source }, Sources = new OneText(path, text) });
                    var script = one.Scripts.FirstOrDefault().Script;
                    if (script == null) continue;
                    var byId = script.Symbols.Where(x => x.Id != null).GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
                    foreach (var e in script.Edges.Where(e => e.Relation is "inherits" or "implements"))
                        if (byId.TryGetValue(e.TargetId ?? "", out var sup) && sup.QualifiedName == contractType && byId.TryGetValue(e.SourceId ?? "", out var sub) && CodeCrossLinker.IsJavaType(sub))
                            found.Add((path, sub.QualifiedName));
                }
                return implementorsCache[contractType] = found;
            }

            void DecideImplementations()
            {
                // una copia: seguire un'implementazione può far scoprire altri metodi di interfaccia, decisi al giro dopo
                foreach (var (contract, seen) in contracts.ToList())
                {
                    var dot = contract.LastIndexOf('.');
                    if (dot <= 0) continue;
                    var type = contract[..dot];
                    var name = contract[(dot + 1)..];
                    if (!decided.TryGetValue(contract, out var d)) decided[contract] = d = new CodeClosureImplementations { Contract = contract };
                    var inRoot = ImplementorsInRoot(type);
                    var candidates = seen.Keys.Concat(inRoot.Select(i => i.Path)).Distinct(StringComparer.Ordinal).OrderBy(p => p, StringComparer.Ordinal).ToList();
                    d.Candidates.Clear(); d.Candidates.AddRange(candidates);
                    d.Auto = candidates.Count == 1;
                    foreach (var path in candidates.Where(p => (d.Auto || request.Included.Contains(p)) && !d.Followed.Contains(p)))
                    {
                        d.Followed.Add(path);
                        if (seen.TryGetValue(path, out var methodId) && states.TryGetValue(path, out var st)) { Follow(st, methodId); continue; }
                        var impl = inRoot.FirstOrDefault(i => i.Path == path);
                        if (impl.TypeFqn == null) continue;
                        work.Enqueue((path, impl.TypeFqn, false, 1, "impl:" + contract, CodeCrossLinker.ImplementationPrefix + contract));
                        pendingMethods.Add((impl.TypeFqn, name, -1, "method:" + impl.TypeFqn + "." + name, "impl:" + contract, 1));
                    }
                }
            }

            void Discover()
            {
            while (true)
            {
                while (work.Count > 0)
                {
                    var (path, typeFqn, whole, depth, from, written) = work.Dequeue();
                    var st = Visit(path, depth, from, written);
                    if (st == null) continue;
                    if (whole && !st.Node.Whole)
                    {
                        st.Node.Whole = true;
                        foreach (var s in st.Script.Symbols.Where(s => s.Kind != "external").ToList()) Follow(st, s.Id);
                    }
                    else if (typeFqn != null)
                    {
                        var type = st.Script.Symbols.FirstOrDefault(s => CodeCrossLinker.IsJavaType(s) && s.QualifiedName == typeFqn);
                        if (type == null) { Miss("unresolved", written, from, new[] { path }); continue; }   // il file c'è, il tipo no
                        Follow(st, type.Id);
                        // di un tipo raggiunto: costruttori e campi (S8); le madri arrivano dagli archi della classe
                        foreach (var m in st.Script.Symbols.Where(s => s.ParentId == type.Id && (s.Kind == "constructor" || s.Kind == "field"))) Follow(st, m.Id);
                    }
                }
                if (pendingMethods.Count == 0) break;
                // le madri sono state cercate: ora i metodi, per numero di argomenti nel tipo e nelle madri
                var todo = pendingMethods.ToList();
                pendingMethods.Clear();
                var progress = false;
                foreach (var (owner, name, argc, w, from, depth) in todo)
                {
                    var found = FindMethod(owner, name, argc, states, typeOf, out var waiting);
                    if (found.Count > 0)
                    {
                        progress = true;
                        foreach (var (p, id) in found) { Follow(states[p], id); }
                    }
                    else if (waiting) pendingMethods.Add((owner, name, argc, w, from, depth));
                    else Miss("unresolved", w, from);
                }
                if (!progress && work.Count == 0)
                {
                    foreach (var (_, _, _, w, from, _) in pendingMethods) Miss("unresolved", w, from);
                    pendingMethods.Clear();
                    break;
                }
            }
            }

            // un simbolo raggiunto: i suoi figli che sono codice suo (lambda, classi anonime no: i figli metodo restano per chiamata),
            // ciò che usa nel file, e fuori i tipi e i metodi
            // rescan: il simbolo è già raggiunto, ma il suo script è nuovo (giro «tutti insieme»): se ne ripassano gli archi
            void Follow(State st, string symbolId, bool rescan = false)
            {
                var stack = new Stack<(string Id, bool Force)>(new[] { (symbolId, rescan) });
                while (stack.Count > 0)
                {
                    var (id, force) = stack.Pop();
                    if (id == null || (!st.Followed.Add(id) && !force)) continue;
                    if (st.ById.TryGetValue(id, out var sym) && sym.Name != null && sym.Kind != "module") st.Node.Symbols.Add(sym.Name);
                    // F4c: un metodo di interfaccia raggiunto: il codice vero sta in chi la implementa, da cercare anche fuori dalla chiusura
                    if (sym?.Kind == "method" && sym.ParentId != null && st.ById.TryGetValue(sym.ParentId, out var holder) && holder.Kind == "interface" && holder.QualifiedName != null)
                        contracts.TryAdd(holder.QualifiedName + "." + sym.Name, new Dictionary<string, string>(StringComparer.Ordinal));
                    // un import Java da solo non usa niente: contano i riferimenti veri (campi, new, chiamate, madri)
                    foreach (var e in st.Script.Edges.Where(e => e.SourceId == id && e.Relation != "contains" && e.Relation != "imports"))
                    {
                        // F4c: un'implementazione possibile non si segue da sola: si raccoglie, e la decide DecideImplementations
                        if (e.Written != null && e.Written.StartsWith(CodeCrossLinker.ImplementationPrefix, StringComparison.Ordinal))
                        {
                            var contract = e.Written[CodeCrossLinker.ImplementationPrefix.Length..];
                            if (owners != null && e.TargetId != null && owners.TryGetValue(e.TargetId, out var implOwner))
                            {
                                if (!contracts.TryGetValue(contract, out var seen)) contracts[contract] = seen = new Dictionary<string, string>(StringComparer.Ordinal);
                                seen.TryAdd(implOwner.Node.Path, e.TargetId);
                            }
                            continue;
                        }
                        var target = e.TargetId != null && st.ById.TryGetValue(e.TargetId, out var t) ? t : null;
                        if (target == null)
                        {
                            // letti insieme: un arco verso un simbolo di un altro file della chiusura (ricevente dedotto, implementazione)
                            if (owners != null && e.TargetId != null && owners.TryGetValue(e.TargetId, out var other) && other != st) Follow(other, e.TargetId);
                            continue;
                        }
                        var from = $"{st.Node.Path}:{e.Line}";
                        if (target.Kind != "external") { stack.Push((target.Id, false)); continue; }
                        if (target.QualifiedName == null) continue;
                        if (e.Relation == "calls")
                        {
                            var arities = CodeCrossLinker.ParseArities(e.Written);
                            var dot = target.QualifiedName.LastIndexOf('.');
                            if (dot <= 0 || target.QualifiedName.StartsWith("?.", StringComparison.Ordinal))
                            {
                                Miss("unresolved", "method:" + target.QualifiedName, from);   // il ricevente non si sa: lo dirà F4/S10
                                continue;
                            }
                            var owner = target.QualifiedName[..dot];
                            var name = target.QualifiedName[(dot + 1)..];
                            var ownerFile = FileOfType(owner, "type:" + owner, from);
                            if (ownerFile == null) continue;
                            work.Enqueue((ownerFile, owner, false, st.Node.Depth + 1, from, "type:" + owner));
                            foreach (var (argc, _) in arities.Count == 0 ? new[] { (-1, 0) } : arities)
                                pendingMethods.Add((owner, name, argc, "method:" + target.QualifiedName + "/" + argc, from, st.Node.Depth + 1));
                        }
                        else
                        {
                            var file = FileOfType(target.QualifiedName, "type:" + target.QualifiedName, from);
                            if (file != null) work.Enqueue((file, target.QualifiedName, false, st.Node.Depth + 1, from, "type:" + target.QualifiedName));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Il metodo nel tipo o nelle madri già lette, per numero di argomenti come <c>PerimeterIndex.ByArity</c>. <paramref name="waiting"/>:
        /// non trovato, ma una madre è ancora da leggere (il giro dopo).
        /// </summary>
        private static List<(string Path, string Id)> FindMethod(string fqn, string name, int argc, Dictionary<string, State> states,
            Dictionary<string, (string Path, string Id)> typeOf, out bool waiting)
        {
            waiting = false;
            var visited = new HashSet<string>(StringComparer.Ordinal);
            var queue = new Queue<string>(new[] { fqn });
            while (queue.Count > 0)
            {
                var t = queue.Dequeue();
                if (!visited.Add(t)) continue;
                if (!typeOf.TryGetValue(t, out var at) || !states.TryGetValue(at.Path, out var st)) { waiting = true; continue; }
                var methods = st.Script.Symbols.Where(s => s.Kind == "method" && s.ParentId == at.Id && s.Name == name).ToList();
                if (methods.Count > 0)
                {
                    var arity = st.Script.Parameters.GroupBy(p => p.SymbolId).ToDictionary(g => g.Key ?? "", g => g.Count(), StringComparer.Ordinal);
                    var fit = argc < 0 ? methods : methods.Where(m =>
                    {
                        var a = arity.TryGetValue(m.Id, out var n) ? n : 0;
                        return a == argc || (m.Signature?.Contains("...") == true && argc >= a - 1);
                    }).ToList();
                    waiting = false;
                    return (fit.Count > 0 ? fit : methods).Select(m => (at.Path, m.Id)).ToList();
                }
                // le madri della classe: gli archi inherits/implements verso tipi (interni al file o esterni)
                foreach (var e in st.Script.Edges.Where(e => e.SourceId == at.Id && (e.Relation == "inherits" || e.Relation == "implements")))
                    if (st.ById.TryGetValue(e.TargetId ?? "", out var sup) && sup.QualifiedName != null) queue.Enqueue(sup.QualifiedName);
            }
            return new List<(string, string)>();
        }

        private sealed class Texts : ICodeSources
        {
            private readonly Dictionary<string, string> _texts;
            public Texts(Dictionary<string, string> texts) { _texts = texts; }
            public string Read(CodeSourceFile file) => _texts.TryGetValue(file.LogicalPath, out var t) ? t : throw new InvalidOperationException(file.LogicalPath);
            public bool Exists(CodeSourceFile file) => _texts.ContainsKey(file.LogicalPath);
        }

        private sealed class OneText : ICodeSources
        {
            private readonly string _path, _text;
            public OneText(string path, string text) { _path = path; _text = text; }
            public string Read(CodeSourceFile file) => file.LogicalPath == _path ? _text : throw new InvalidOperationException(file.LogicalPath);
            public bool Exists(CodeSourceFile file) => file.LogicalPath == _path;
        }
    }
}
