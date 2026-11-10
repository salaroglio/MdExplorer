using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>Che cosa scoprire (S1, S6): i semi, le potature dell'utente, il budget di sicurezza.</summary>
    public sealed class CodeClosureRequest
    {
        public IReadOnlyList<string> Seeds { get; init; } = Array.Empty<string>();
        public ISet<string> Pruned { get; init; } = new HashSet<string>(StringComparer.Ordinal);
        public int MaxFiles { get; init; } = 300;
        public long MaxLines { get; init; } = 1_000_000;
    }

    /// <summary>Un file della chiusura: perché c'è (da chi, a che riga, con quale riferimento) e che cosa se ne usa.</summary>
    public sealed class CodeClosureFile
    {
        public string Path { get; init; }
        public int Depth { get; init; }
        /// <summary>Il file e la riga del riferimento che l'ha portato; null per un seme.</summary>
        public string From { get; init; }
        /// <summary>Il riferimento com'è scritto (<c>file:x.bh</c>, <c>module:LIB</c>, <c>symbol:NOME</c>); null per un seme.</summary>
        public string Written { get; init; }
        /// <summary>Tutto il file (un seme) o solo i simboli raggiunti (un include, una libreria).</summary>
        public bool Whole { get; set; }
        public SortedSet<string> Symbols { get; } = new(StringComparer.Ordinal);
        public int Lines { get; set; }
    }

    /// <summary>
    /// Un riferimento che la scoperta non ha seguito — non trovato, ambiguo, potato dall'utente, oltre il budget — una volta per
    /// riferimento: quante volte compare e i primi punti, perché l'anteprima lo mostra una riga sola.
    /// </summary>
    public sealed class CodeClosureMiss
    {
        public string Kind { get; init; }   // unresolved, ambiguous, pruned, frontier
        public string Written { get; init; }
        public int Uses { get; set; }
        public List<string> From { get; } = new();
        public IReadOnlyList<string> Candidates { get; init; } = Array.Empty<string>();
    }

    public sealed class CodeClosure
    {
        public List<CodeClosureFile> Files { get; } = new();
        public List<CodeClosureMiss> Misses { get; } = new();
        public bool BudgetReached { get; set; }
        public long Lines => Files.Sum(f => (long)f.Lines);
    }

    /// <summary>
    /// Dove si dichiara un nome in un linguaggio a regole (S11-S12): dalle espressioni che la definizione ha già — il nome del
    /// modulo (<c>PROGRAM "X"</c>), l'inizio dei blocchi di primo livello, le dichiarazioni di primo livello che sono simboli.
    /// Solo candidati: l'estrattore conferma quando il file entra nella chiusura.
    /// </summary>
    public sealed class DeclarationIndex
    {
        private readonly Func<string, string> _norm;
        private readonly Dictionary<string, List<string>> _programs = new(StringComparer.Ordinal);
        private readonly Dictionary<string, List<string>> _symbols = new(StringComparer.Ordinal);

        private DeclarationIndex(bool ignoreCase) { _norm = ignoreCase ? n => n.ToUpperInvariant() : n => n; }

        /// <summary>Un <c>grep</c> per espressione: su un server tornano solo le righe che dichiarano qualcosa, non i file.</summary>
        public static DeclarationIndex Build(DeclarativeLanguage lang, ICodeWorkspace workspace)
        {
            var index = new DeclarationIndex(lang.IgnoreCase);
            // di primo livello: senza «in», o «in: [module]» (il valore che il lettore della definizione dà per default)
            var blocks = lang.Blocks.Where(b => b.In == null || b.In.Count == 0 || b.In.Contains("module")).ToList();
            var declarations = lang.Declarations.Where(d => d.Emit && (d.In == null || d.In.Count == 0 || d.In.Contains("module"))).ToList();
            var patterns = new List<(Regex Pattern, Dictionary<string, List<string>> Map, string Template)>();
            if (lang.ModuleName != null) patterns.Add((lang.ModuleName, index._programs, null));
            patterns.AddRange(blocks.Select(b => (b.Begin, index._symbols, b.NameTemplate)));
            patterns.AddRange(declarations.Select(d => (d.At, index._symbols, d.NameTemplate)));
            foreach (var (pattern, map, template) in patterns)
                foreach (var (path, _, text) in workspace.Grep(pattern, lang.Extensions.ToList()))
                    if (pattern.Match(text) is { Success: true } m && m.Groups["name"].Success)
                        index.Add(map, Apply(template, m.Groups["name"].Value), path);
            return index;
        }

        private static string Apply(string template, string name) => template == null ? name : template.Replace("{name}", name);

        private void Add(Dictionary<string, List<string>> map, string name, string path)
        {
            var key = _norm(name.Trim());
            if (!map.TryGetValue(key, out var list)) map[key] = list = new();
            if (!list.Contains(path)) list.Add(path);
        }

        public IReadOnlyList<string> Programs(string name) => _programs.TryGetValue(_norm(name), out var l) ? l : (IReadOnlyList<string>)Array.Empty<string>();
        public IReadOnlyList<string> Symbols(string name) => _symbols.TryGetValue(_norm(name), out var l) ? l : (IReadOnlyList<string>)Array.Empty<string>();
    }

    /// <summary>
    /// La scoperta della chiusura di un linguaggio a regole (F2a; S6, S8): dai semi verso il basso, ogni file una volta,
    /// leggendo ogni file da solo con l'estrattore (le sue dipendenze sono gli archi con il riferimento scritto, F1a). Non
    /// scrive niente: il grafo lo fa l'estrazione dopo l'anteprima.
    /// <para>
    /// Un seme entra intero; un <c>#INCLUDE</c> e una libreria entrano col loro codice di modulo, e dei loro simboli solo
    /// quelli chiamati (una macro di un header incluso è testo copiato, ma non gira finché nessuno la chiama: seguire tutte le
    /// macro di un header porterebbe dentro mezza libreria). Le chiamate si cercano dopo aver trovato tutti gli import, così la regola di F1a — prima i file
    /// importati, anche attraverso gli include, poi il nome unico — vede l'insieme intero. Budget: file e righe; oltre,
    /// i riferimenti restano nella frontiera.
    /// </para>
    /// </summary>
    public sealed class DeclarativeClosure
    {
        private readonly DeclarativeLanguage _lang;
        private readonly DeclarativeLanguageExtractor _extractor;
        private readonly ICodeWorkspace _ws;
        private readonly Func<string, string> _norm;

        public DeclarativeClosure(DeclarativeLanguage lang, ICodeWorkspace workspace)
        {
            _lang = lang; _ws = workspace;
            _extractor = new DeclarativeLanguageExtractor(lang);
            _norm = lang.IgnoreCase ? n => n.ToUpperInvariant() : n => n;
        }

        private sealed class State
        {
            public CodeClosureFile Node;
            public CodeScript Script;
            public Dictionary<string, ScriptSymbol> ById;
            public HashSet<string> Followed = new(StringComparer.Ordinal);
            public HashSet<string> Imports = new(StringComparer.Ordinal);
        }

        public CodeClosure Compute(CodeClosureRequest request)
        {
            var result = new CodeClosure();
            var files = _ws.Files().Where(p => _lang.Extensions.Contains(System.IO.Path.GetExtension(p), StringComparer.OrdinalIgnoreCase)).ToList();
            var byName = files.GroupBy(p => _norm(CodeLinkIndex.LastSegment(p))).ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);
            var texts = new Dictionary<string, string>(StringComparer.Ordinal);
            string Text(string p) => texts.TryGetValue(p, out var t) ? t : texts[p] = _ws.Read(p);
            // l'indice con un grep per espressione; per intero si leggono solo i file che entrano
            var index = DeclarationIndex.Build(_lang, _ws);

            var states = new Dictionary<string, State>(StringComparer.Ordinal);
            var misses = new Dictionary<string, CodeClosureMiss>(StringComparer.Ordinal);
            var pendingCalls = new List<(string File, string Name, string Written, string From, int Depth)>();
            var work = new Queue<(string Path, bool Whole, string Symbol, int Depth, string From, string Written)>();
            foreach (var seed in request.Seeds) work.Enqueue((seed, true, null, 0, null, null));

            void Miss(string kind, string written, string from, IReadOnlyList<string> candidates = null)
            {
                var key = $"{kind}|{(written == null ? null : _norm(written))}";
                if (!misses.TryGetValue(key, out var m)) misses[key] = m = new CodeClosureMiss { Kind = kind, Written = written, Candidates = candidates ?? Array.Empty<string>() };
                m.Uses++;
                if (from != null && m.From.Count < 5 && !m.From.Contains(from)) m.From.Add(from);
            }

            while (true)
            {
                while (work.Count > 0)
                {
                    var (path, whole, symbol, depth, from, written) = work.Dequeue();
                    if (request.Pruned.Contains(path)) { Miss("pruned", written, from, new[] { path }); continue; }
                    if (!states.TryGetValue(path, out var st))
                    {
                        var text = Text(path);
                        var lines = text.Count(c => c == '\n') + 1;
                        if (result.Files.Count + 1 > request.MaxFiles || result.Lines + lines > request.MaxLines)
                        {
                            result.BudgetReached = true;
                            Miss("frontier", written, from, new[] { path });
                            continue;
                        }
                        var extracted = _extractor.Extract(new CodeExtractionRequest { ProjectPath = "", AllFiles = new[] { _ws.Source(path) }, FilesToEmit = new[] { _ws.Source(path) },
                            Sources = new SingleText(path, text) });
                        var script = extracted.Scripts.Single().Script;
                        st = states[path] = new State
                        {
                            Script = script,
                            ById = script.Symbols.Where(s => s.Id != null).GroupBy(s => s.Id).ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal),
                            Node = new CodeClosureFile { Path = path, Depth = depth, From = from, Written = written, Lines = lines },
                        };
                        result.Files.Add(st.Node);
                        // il codice del modulo fuori dai blocchi: raggiunto con il file (S8)
                        var module = script.Symbols.FirstOrDefault(s => s.Kind == "module");
                        if (module != null) Follow(st, module.Id, path, depth, byName, index, pendingCalls, work, Miss);
                    }
                    if (whole && !st.Node.Whole)
                    {
                        st.Node.Whole = true;
                        foreach (var s in st.Script.Symbols.Where(s => s.Kind != "external" && s.Kind != "module").ToList())
                            Follow(st, s.Id, path, st.Node.Depth, byName, index, pendingCalls, work, Miss);
                    }
                    else if (symbol != null)
                    {
                        var module = st.Script.Symbols.FirstOrDefault(s => s.Kind == "module");
                        var targets = st.Script.Symbols.Where(s => s.Kind == "function" && s.ParentId == module?.Id && s.Name != null && _norm(s.Name) == _norm(symbol)).ToList();
                        if (targets.Count == 0) Miss("unresolved", "symbol:" + symbol, from, new[] { path });   // l'indice l'aveva proposto, l'estrattore non lo vede
                        foreach (var t in targets) Follow(st, t.Id, path, st.Node.Depth, byName, index, pendingCalls, work, Miss);
                    }
                }
                if (pendingCalls.Count == 0) break;
                // tutti gli import noti: ora le chiamate, prima fra i file importati (anche attraverso gli include)
                var calls = pendingCalls.ToList();
                pendingCalls.Clear();
                foreach (var (file, name, written, from, depth) in calls)
                {
                    var candidates = index.Symbols(name).Where(p => p != file).ToList();
                    var imported = ImportedClosure(file, states);
                    var via = candidates.Where(imported.Contains).ToList();
                    var chosen = via.Count > 0 ? via : candidates;
                    if (chosen.Count == 1) work.Enqueue((chosen[0], false, name, depth + 1, from, written));
                    else if (chosen.Count == 0) Miss("unresolved", written, from);
                    else Miss("ambiguous", written, from, chosen);
                }
            }
            result.Misses.AddRange(misses.Values.OrderByDescending(m => m.Uses).ThenBy(m => m.Written, StringComparer.Ordinal));
            return result;
        }

        private static HashSet<string> ImportedClosure(string file, Dictionary<string, State> states)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var queue = new Queue<string>(states.TryGetValue(file, out var s) ? s.Imports : Enumerable.Empty<string>());
            while (queue.Count > 0)
            {
                var p = queue.Dequeue();
                if (p == file || !seen.Add(p)) continue;
                if (states.TryGetValue(p, out var n)) foreach (var x in n.Imports) queue.Enqueue(x);
            }
            return seen;
        }

        /// <summary>Un simbolo raggiunto: i suoi figli (frammenti, etichette) e ciò che chiama nel file; fuori, gli import e le chiamate.</summary>
        private void Follow(State st, string symbolId, string path, int depth, Dictionary<string, List<string>> byName, DeclarationIndex index,
            List<(string, string, string, string, int)> pendingCalls,
            Queue<(string, bool, string, int, string, string)> work, Action<string, string, string, IReadOnlyList<string>> miss)
        {
            var todo = new Stack<string>(new[] { symbolId });
            while (todo.Count > 0)
            {
                var id = todo.Pop();
                if (id == null || !st.Followed.Add(id)) continue;
                var isModule = st.ById.TryGetValue(id, out var sym) && sym.Kind == "module";
                if (sym != null && !isModule && sym.Name != null) st.Node.Symbols.Add(sym.Name);
                // i pezzi di un blocco (frammenti, etichette) sono il blocco; i figli del modulo sono i simboli del file, non il suo codice
                if (!isModule) foreach (var child in st.Script.Symbols.Where(s => s.ParentId == id)) todo.Push(child.Id);
                foreach (var e in st.Script.Edges.Where(e => e.SourceId == id && e.Relation != "contains"))
                {
                    var target = e.TargetId != null && st.ById.TryGetValue(e.TargetId, out var t) ? t : null;
                    if (target != null && target.Kind != "external") { todo.Push(target.Id); continue; }
                    if (e.Written == null) continue;
                    var (what, name, _) = CodeCrossLinker.Parse(e.Written);
                    var from = $"{path}:{e.Line}";
                    switch (what)
                    {
                        case "file":
                            var hits = byName.TryGetValue(_norm(CodeLinkIndex.LastSegment(name)), out var l) ? l.Where(p => p != path).ToList() : new List<string>();
                            // un include: il suo codice di modulo e le sue macro disponibili; si seguono solo quelle chiamate
                            if (hits.Count == 1) { st.Imports.Add(hits[0]); work.Enqueue((hits[0], false, null, depth + 1, from, e.Written)); }
                            else if (hits.Count == 0) miss("unresolved", e.Written, from, null);
                            else miss("ambiguous", e.Written, from, hits);
                            break;
                        case "module":
                            var libs = index.Programs(name).Where(p => p != path).ToList();
                            if (libs.Count == 1) { st.Imports.Add(libs[0]); work.Enqueue((libs[0], false, null, depth + 1, from, e.Written)); }
                            else if (libs.Count == 0) miss("unresolved", e.Written, from, null);
                            else miss("ambiguous", e.Written, from, libs);
                            break;
                        case "symbol":
                            pendingCalls.Add((path, name, e.Written, from, depth));
                            break;
                    }
                }
            }
        }

        /// <summary>Il testo già letto, per l'estrattore (non rilegge dal disco o dal server).</summary>
        private sealed class SingleText : ICodeSources
        {
            private readonly string _path, _text;
            public SingleText(string path, string text) { _path = path; _text = text; }
            public string Read(CodeSourceFile file) => file.LogicalPath == _path ? _text : throw new InvalidOperationException(file.LogicalPath);
            public bool Exists(CodeSourceFile file) => file.LogicalPath == _path;
        }
    }
}
