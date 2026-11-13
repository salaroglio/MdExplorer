using System;
using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Che cosa dichiarano gli script già analizzati di un linguaggio, di tutti i contesti (F1a, 10/11/2026): file per nome,
    /// moduli per nome di programma, funzioni di primo livello per nome, e gli import di ogni file. Si costruisce dagli script
    /// (con gli id per file di G4), non dai sorgenti: serve a ricollegare senza rileggere niente.
    /// </summary>
    public sealed class CodeLinkIndex
    {
        private readonly Func<string, string> _norm;
        private readonly Dictionary<string, List<string>> _files = new(StringComparer.Ordinal);
        private readonly Dictionary<string, List<string>> _programs = new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _moduleOf = new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _pathOfModule = new(StringComparer.Ordinal);
        private readonly Dictionary<string, List<(string Path, string Id)>> _symbols = new(StringComparer.Ordinal);
        /// <summary>I simboli dichiarati dentro un'interfaccia (la specifica di un package): cedono all'implementazione.</summary>
        private readonly HashSet<string> _declarations = new(StringComparer.Ordinal);
        private readonly Dictionary<string, HashSet<string>> _imports = new(StringComparer.Ordinal);
        /// <summary>
        /// I nomi scritti con il contenitore (<c>PKG_A.SAVE</c>, 13/11/2026): i simboli per «contenitore.nome», quelli di primo
        /// livello per nome, e i nomi dei contenitori — se un contenitore con quel nome c'è, mai il SAVE di un altro.
        /// </summary>
        private readonly Dictionary<string, List<(string Path, string Id)>> _owned = new(StringComparer.Ordinal);
        private readonly Dictionary<string, List<(string Path, string Id)>> _topLevel = new(StringComparer.Ordinal);
        private readonly HashSet<string> _containers = new(StringComparer.Ordinal);

        private CodeLinkIndex(bool ignoreCase, bool allSymbols) { _norm = ignoreCase ? n => n.ToUpperInvariant() : n => n; AllSymbols = allSymbols; }

        /// <summary>
        /// Un linguaggio database (<see cref="DeclarativeLanguage.Database"/>, 12/11/2026): si cercano per nome TUTTI i suoi simboli,
        /// non solo le funzioni di primo livello — le tabelle e le viste di un altro file, le procedure dentro un package.
        /// </summary>
        public bool AllSymbols { get; }

        internal static bool Indexed(ScriptSymbol s) => s.Kind is not ("module" or "external" or DeclarativeLanguage.FragmentKind) && !string.IsNullOrEmpty(s.Name);

        public static CodeLinkIndex Build(bool ignoreCase, IEnumerable<(string Path, CodeScript Script)> scripts, bool allSymbols = false)
        {
            var index = new CodeLinkIndex(ignoreCase, allSymbols);
            foreach (var (path, script) in scripts) index.Add(path, script);
            return index;
        }

        private void Add(string path, CodeScript script)
        {
            var module = script.Symbols.FirstOrDefault(s => s.Kind == "module");
            Push(_files, _norm(LastSegment(path)), path);
            if (module?.Id == null) return;
            _moduleOf[path] = module.Id;
            _pathOfModule[module.Id] = path;
            if (!string.IsNullOrEmpty(module.Name)) Push(_programs, _norm(module.Name), path);
            foreach (var s in script.Symbols.Where(s => AllSymbols ? Indexed(s) : s.Kind == "function" && s.ParentId == module.Id && !string.IsNullOrEmpty(s.Name)))
            {
                if (!_symbols.TryGetValue(_norm(s.Name), out var list)) _symbols[_norm(s.Name)] = list = new();
                list.Add((path, s.Id));
                if (s.ParentId != null && script.Symbols.FirstOrDefault(p => p.Id == s.ParentId)?.Kind == "interface") _declarations.Add(s.Id);
            }
            foreach (var s in script.Symbols.Where(Indexed))
            {
                var parent = s.ParentId == null ? null : script.Symbols.FirstOrDefault(p => p.Id == s.ParentId);
                if (parent == null || parent.Kind == "module") { Push(_topLevel, _norm(s.Name), (path, s.Id)); continue; }
                if (string.IsNullOrEmpty(parent.Name)) continue;
                _containers.Add(_norm(parent.Name));
                Push(_owned, _norm(parent.Name + "." + s.Name), (path, s.Id));
                if (parent.Kind == "interface") _declarations.Add(s.Id);
            }
            SetImports(path, script);
        }

        /// <summary>Gli import di un file come sono adesso nel suo script (dopo il ricollegamento degli import).</summary>
        internal void SetImports(string path, CodeScript script)
            => _imports[path] = new HashSet<string>(script.Edges.Where(e => e.Relation == "imports" && _pathOfModule.ContainsKey(e.TargetId ?? ""))
                .Select(e => _pathOfModule[e.TargetId]), StringComparer.Ordinal);

        private static void Push(Dictionary<string, List<(string Path, string Id)>> map, string key, (string Path, string Id) value)
        {
            if (!map.TryGetValue(key, out var list)) map[key] = list = new();
            if (!list.Contains(value)) list.Add(value);
        }

        private static void Push(Dictionary<string, List<string>> map, string key, string value)
        {
            if (!map.TryGetValue(key, out var list)) map[key] = list = new();
            if (!list.Contains(value)) list.Add(value);
        }

        internal static string LastSegment(string path)
        {
            var p = (path ?? string.Empty).Replace('\\', '/');
            return p[(p.LastIndexOf('/') + 1)..];
        }

        internal string Normalize(string name) => _norm(name);
        internal IReadOnlyList<string> Files(string fileName) => _files.TryGetValue(_norm(LastSegment(fileName)), out var l) ? l : (IReadOnlyList<string>)Array.Empty<string>();
        internal IReadOnlyList<string> Programs(string name) => _programs.TryGetValue(_norm(name), out var l) ? l : (IReadOnlyList<string>)Array.Empty<string>();
        internal string ModuleOf(string path) => _moduleOf.TryGetValue(path, out var id) ? id : null;
        internal IReadOnlyList<(string Path, string Id)> Symbols(string name)
        {
            List<(string Path, string Id)> l;
            var dot = name.LastIndexOf('.');
            if (dot > 0 && dot < name.Length - 1)
            {
                // «contenitore.nome»: dentro quel contenitore, se c'è; altrimenti il nome davanti è lo schema
                var found = HasContainer(name[..dot]) ? _owned.TryGetValue(_norm(name), out l) : _topLevel.TryGetValue(_norm(name[(dot + 1)..]), out l);
                if (!found) return Array.Empty<(string, string)>();
            }
            else if (!_symbols.TryGetValue(_norm(name), out l)) return Array.Empty<(string, string)>();
            return AllSymbols || dot > 0 ? CodeOwnerMember.PreferImplementations(l, c => _declarations.Contains(c.Id) ? "interface" : null) : l;
        }

        internal bool HasContainer(string name) => _containers.Contains(_norm(name));

        /// <summary>I file che <paramref name="path"/> importa, anche attraverso i file importati (gli include di un include).</summary>
        internal HashSet<string> ImportedClosure(string path)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var queue = new Queue<string>(_imports.TryGetValue(path, out var first) ? first : Enumerable.Empty<string>());
            while (queue.Count > 0)
            {
                var p = queue.Dequeue();
                if (p == path || !seen.Add(p)) continue;
                if (_imports.TryGetValue(p, out var next)) foreach (var n in next) queue.Enqueue(n);
            }
            return seen;
        }
    }

    /// <summary>
    /// Il ricollegamento fra contesti di un linguaggio a regole (F1a del piano 2026-11-10-Analisi-Di-Programmi-Singoli, S13):
    /// un riferimento per nome (<see cref="ScriptEdge.Written"/>) si risolve sugli script di TUTTI i contesti già analizzati,
    /// non solo sui file dell'estrazione, così lo script di un file viene uguale qualunque contesto lo analizzi.
    /// <para>
    /// Prima gli import (file per nome, libreria per nome di programma), poi le chiamate: nello stesso file vince l'estrattore;
    /// altrimenti fra i file che il chiamante importa (anche attraverso gli include: utente 10/11, «risolvere per path»), poi
    /// per nome unico nel progetto (<c>inferred</c>), altrimenti ambiguo con i candidati, o esterno «?.nome» se sono troppi.
    /// Un riferimento che non trova niente torna esterno con il suo nome; un nodo esterno rimasto senza archi si toglie.
    /// </para>
    /// </summary>
    public static class CodeCrossLinker
    {
        // ------------------------------------------------------------------ Java (nome qualificato esatto)

        /// <summary>
        /// Il riferimento di una chiamata a un metodo esterno con quanti argomenti e da che riga: <c>method:a.b.C.nome/1@12,2@13</c>.
        /// Più chiamate dello stesso chiamante si uniscono in un arco, ma dentro un contesto andrebbero a metodi diversi (un
        /// sovraccarico per numero di argomenti), ognuno con la sua riga: le si ricorda tutte, la prima riga per ognuna.
        /// </summary>
        public static string WithArity(string written, string qualifiedName, int argc, int line)
        {
            var arities = new SortedDictionary<int, int>();
            foreach (var (a, l) in ParseArities(written)) arities[a] = l;
            if (!arities.TryGetValue(argc, out var first) || line < first) arities[argc] = line;
            return "method:" + qualifiedName + "/" + string.Join(",", arities.Select(kv => $"{kv.Key}@{kv.Value}"));
        }

        internal static IReadOnlyList<(int Argc, int Line)> ParseArities(string written)
        {
            if (written == null || !written.StartsWith("method:", StringComparison.Ordinal)) return Array.Empty<(int, int)>();
            var slash = written.LastIndexOf('/');
            if (slash < 0) return Array.Empty<(int, int)>();
            return written[(slash + 1)..].Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x =>
            {
                var at = x.IndexOf('@');
                var argc = int.TryParse(at < 0 ? x : x[..at], out var n) ? n : -1;
                var line = at > 0 && int.TryParse(x[(at + 1)..], out var l) ? l : 0;
                return (argc, line);
            }).ToList();
        }

        /// <summary>Un simbolo che c'era nello script di prima di un file e non c'è più: per riportare a esterno chi lo usava.</summary>
        public sealed record Vanished(string Kind, string Name, string QualifiedName, string OwnerQualifiedName, int Arity);

        private static readonly HashSet<string> MemberKinds = new(StringComparer.Ordinal) { "method", "constructor", "field", "module", "external", "fragment", "function", "namespace", "package", "file", "parameter" };
        internal static bool IsJavaType(ScriptSymbol s) => s.Kind != null && !MemberKinds.Contains(s.Kind) && s.QualifiedName != null && !s.QualifiedName.Contains('(');

        private sealed class JavaIndex
        {
            public readonly Dictionary<string, List<string>> TypesByFqn = new(StringComparer.Ordinal);
            public readonly Dictionary<string, string> FqnOfType = new(StringComparer.Ordinal);
            public readonly Dictionary<string, Dictionary<string, List<(string Id, int Arity, bool Varargs)>>> Methods = new(StringComparer.Ordinal);
            public readonly Dictionary<string, List<string>> Supers = new(StringComparer.Ordinal);

            public JavaIndex(IEnumerable<CodeScript> scripts)
            {
                var list = scripts.ToList();
                foreach (var script in list)
                    foreach (var t in script.Symbols.Where(IsJavaType))
                    {
                        if (!TypesByFqn.TryGetValue(t.QualifiedName, out var ids)) TypesByFqn[t.QualifiedName] = ids = new();
                        ids.Add(t.Id); FqnOfType[t.Id] = t.QualifiedName;
                    }
                foreach (var script in list)
                {
                    var arity = script.Parameters.GroupBy(p => p.SymbolId).ToDictionary(g => g.Key ?? "", g => g.Count(), StringComparer.Ordinal);
                    foreach (var m in script.Symbols.Where(m => m.Kind == "method" && m.ParentId != null && FqnOfType.ContainsKey(m.ParentId)))
                    {
                        var owner = FqnOfType[m.ParentId];
                        if (!Methods.TryGetValue(owner, out var byName)) Methods[owner] = byName = new(StringComparer.Ordinal);
                        if (!byName.TryGetValue(m.Name ?? "", out var ms)) byName[m.Name ?? ""] = ms = new();
                        ms.Add((m.Id, arity.TryGetValue(m.Id, out var a) ? a : 0, m.Signature?.Contains("...") == true));
                    }
                    var byId = script.Symbols.Where(x => x.Id != null).GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
                    foreach (var e in script.Edges.Where(e => (e.Relation == "inherits" || e.Relation == "implements") && FqnOfType.ContainsKey(e.SourceId ?? "")))
                    {
                        var super = FqnOfType.TryGetValue(e.TargetId ?? "", out var f) ? f
                            : byId.TryGetValue(e.TargetId ?? "", out var x) && x.Kind == "external" ? x.QualifiedName : null;
                        if (super == null) continue;
                        var sub = FqnOfType[e.SourceId];
                        if (!Supers.TryGetValue(sub, out var ss)) Supers[sub] = ss = new();
                        if (!ss.Contains(super)) ss.Add(super);
                    }
                }
            }

            /// <summary>Come <c>PerimeterIndex.FindMethod</c> dell'estrattore: il tipo, poi le madri; il primo che ha il nome, per numero di argomenti.</summary>
            public List<(string Id, int Arity, bool Varargs)> FindMethod(string fqn, string name, int argc)
            {
                var visited = new HashSet<string>(StringComparer.Ordinal);
                var queue = new Queue<string>(new[] { fqn });
                while (queue.Count > 0)
                {
                    var t = queue.Dequeue();
                    if (!visited.Add(t)) continue;
                    if (Methods.TryGetValue(t, out var byName) && byName.TryGetValue(name, out var ms) && ms.Count > 0)
                    {
                        if (argc < 0) return ms;
                        var fit = ms.Where(m => m.Arity == argc || (m.Varargs && argc >= m.Arity - 1)).ToList();
                        return fit.Count > 0 ? fit : ms;
                    }
                    if (Supers.TryGetValue(t, out var ss)) foreach (var x in ss) queue.Enqueue(x);
                }
                return null;
            }
        }

        private static string Weaker(string a, string b) => Rank(a) <= Rank(b) ? a : b;

        /// <summary>
        /// Java (F1a, 10/11/2026): si ricollegano solo gli ESTERNI, perché gli archi che l'estrattore ha risolto dentro il suo
        /// contesto sanno di più (tipo del ricevente, implementazioni). Un tipo esterno va alla classe con quel nome qualificato,
        /// se è una sola; una chiamata esterna al metodo del tipo (o delle madri già analizzate) con lo stesso numero di
        /// argomenti, con la confidenza che darebbe l'estrattore. Chi puntava a un simbolo sparito (<paramref name="vanished"/>)
        /// torna esterno. Le implementazioni di un'interfaccia di un altro contesto non si aggiungono (F4). Torna i percorsi di
        /// <paramref name="others"/> cambiati.
        /// </summary>
        public static List<string> LinkAllJava(string languageId, IReadOnlyList<(string Path, CodeScript Script)> own,
            IReadOnlyList<(string Path, CodeScript Script)> others, IReadOnlyDictionary<string, Vanished> vanished)
        {
            var all = own.Concat(others).ToList();
            var index = new JavaIndex(all.Select(a => a.Script));
            var before = others.ToDictionary(o => o.Path, o => Fingerprint(o.Script), StringComparer.Ordinal);
            string Ext(string what, string qn) => RoslynCSharpExtractor.StableId(languageId, "external", what + ":" + qn);
            var known = new HashSet<string>(all.SelectMany(a => a.Script.Symbols).Where(x => x.Id != null).Select(x => x.Id), StringComparer.Ordinal);
            foreach (var (_, script) in all) RelinkJava(script, index, Ext, known, vanished ?? new Dictionary<string, Vanished>());
            return others.Where(o => Fingerprint(o.Script) != before[o.Path]).Select(o => o.Path).ToList();
        }

        private static void RelinkJava(CodeScript script, JavaIndex index, Func<string, string, string> ext, HashSet<string> known, IReadOnlyDictionary<string, Vanished> vanished)
        {
            var byId = new Dictionary<string, ScriptSymbol>(StringComparer.Ordinal);
            foreach (var x in script.Symbols.Where(x => x.Id != null)) byId.TryAdd(x.Id, x);
            var result = new List<ScriptEdge>();
            var newExternals = new List<ScriptSymbol>();
            ScriptSymbol External(string what, string qn, string name)
            {
                var id = ext(what, qn);
                if (!byId.TryGetValue(id, out var x))
                {
                    x = new ScriptSymbol { Id = id, Kind = "external", Name = name, QualifiedName = qn };
                    byId[id] = x; newExternals.Add(x);
                }
                return x;
            }
            foreach (var e in script.Edges)
            {
                var target = e.TargetId == null ? null : byId.TryGetValue(e.TargetId, out var t) ? t : null;
                if (target?.Kind == "external" && target.QualifiedName != null)
                {
                    if (target.Id == ext("type", target.QualifiedName) && index.TypesByFqn.TryGetValue(target.QualifiedName, out var types) && types.Count == 1)
                    {
                        result.Add(new ScriptEdge { SourceId = e.SourceId, TargetId = types[0], Relation = e.Relation, Confidence = e.Confidence, Line = e.Line, Guard = e.Guard });
                        continue;
                    }
                    if (e.Relation == "calls" && target.Id == ext("method", target.QualifiedName))
                    {
                        var dot = target.QualifiedName.LastIndexOf('.');
                        var owner = dot > 0 ? target.QualifiedName[..dot] : null;
                        var name = dot > 0 ? target.QualifiedName[(dot + 1)..] : null;
                        var arities = ParseArities(e.Written);
                        var linked = new List<ScriptEdge>();
                        if (owner != null && index.TypesByFqn.TryGetValue(owner, out var ot) && ot.Count == 1)
                            foreach (var (argc, at) in arities.Count == 0 ? new[] { (-1, e.Line ?? 0) } : arities)
                            {
                                var ms = index.FindMethod(owner, name, argc);
                                if (ms == null) continue;
                                // come CallOnType + MethodEdges dell'estrattore
                                var confidence = Weaker(e.Confidence, ms.Count == 1 ? "extracted" : "inferred");
                                if (ms.Count > 1) confidence = Weaker(confidence, "inferred");
                                linked.AddRange(ms.Select(m => new ScriptEdge { SourceId = e.SourceId, TargetId = m.Id, Relation = e.Relation, Confidence = confidence, Line = at > 0 ? at : e.Line, Guard = e.Guard }));
                            }
                        if (linked.Count > 0) { result.AddRange(linked); continue; }
                    }
                    result.Add(e);
                    continue;
                }
                // la destinazione non c'è più in nessuno script: era un simbolo del file appena rianalizzato, torna esterno
                if (target == null && e.TargetId != null && !known.Contains(e.TargetId) && vanished.TryGetValue(e.TargetId, out var gone))
                {
                    if (gone.Kind == "method" && gone.OwnerQualifiedName != null)
                    {
                        var qn = gone.OwnerQualifiedName + "." + gone.Name;
                        var x = External("method", qn, gone.Name);
                        result.Add(new ScriptEdge { SourceId = e.SourceId, TargetId = x.Id, Relation = e.Relation, Confidence = e.Confidence, Line = e.Line, Guard = e.Guard, Written = WithArity(null, qn, gone.Arity, e.Line ?? 0) });
                        continue;
                    }
                    if (gone.QualifiedName != null && gone.Kind != "method")
                    {
                        var x = External("type", gone.QualifiedName, gone.Name);
                        result.Add(new ScriptEdge { SourceId = e.SourceId, TargetId = x.Id, Relation = e.Relation, Confidence = e.Confidence, Line = e.Line, Guard = e.Guard });
                        continue;
                    }
                }
                result.Add(e);
            }
            Finish(script, result, newExternals);
        }

        // ------------------------------------------------------------------ linguaggi a regole (per nome, con gli import)

        /// <summary>Il segno di un arco verso un'implementazione possibile (F4c): <c>impl:a.b.Interfaccia.metodo</c>.</summary>
        public const string ImplementationPrefix = "impl:";

        public static string Written(string what, string name, string maxConfidence)
            => what + ":" + name + (maxConfidence == null ? string.Empty : "|" + maxConfidence);

        internal static (string What, string Name, string Max) Parse(string written)
        {
            var colon = written.IndexOf(':');
            if (colon <= 0) return (null, null, null);
            var rest = written[(colon + 1)..];
            var bar = rest.LastIndexOf('|');
            return bar < 0 ? (written[..colon], rest, null) : (written[..colon], rest[..bar], rest[(bar + 1)..]);
        }

        /// <summary>
        /// Ricollega <paramref name="own"/> (gli script di questa analisi) e <paramref name="others"/> (gli altri del linguaggio,
        /// dal disco) sull'indice di tutti. Torna i percorsi degli script di <paramref name="others"/> che sono cambiati.
        /// </summary>
        public static List<string> LinkAll(string languageId, bool ignoreCase, IReadOnlyList<(string Path, CodeScript Script)> own,
            IReadOnlyList<(string Path, CodeScript Script)> others, bool allSymbols = false)
        {
            var all = own.Concat(others).ToList();
            var index = CodeLinkIndex.Build(ignoreCase, all, allSymbols);
            var before = others.ToDictionary(o => o.Path, o => Fingerprint(o.Script), StringComparer.Ordinal);
            string Ext(string what, string name) => RoslynCSharpExtractor.StableId(languageId, "external", what + ":" + (ignoreCase ? name.ToUpperInvariant() : name));
            // prima tutti gli import, poi le chiamate: chi importa chi serve a scegliere fra le funzioni con lo stesso nome
            foreach (var (path, script) in all) { Relink(script, path, index, Ext, importsOnly: true); index.SetImports(path, script); }
            foreach (var (path, script) in all) Relink(script, path, index, Ext, importsOnly: false);
            return others.Where(o => Fingerprint(o.Script) != before[o.Path]).Select(o => o.Path).ToList();
        }

        private static string Fingerprint(CodeScript s)
            => string.Join("\n", s.Edges.Select(e => $"{e.SourceId}|{e.TargetId}|{e.Relation}|{e.Confidence}|{e.Line}|{e.Written}"))
               + "\n#" + string.Join(",", s.Symbols.Where(x => x.Kind == "external").Select(x => x.Id));

        private static int Rank(string confidence) => confidence switch { "extracted" => 2, "inferred" => 1, _ => 0 };

        private static void Relink(CodeScript script, string path, CodeLinkIndex index, Func<string, string, string> ext, bool importsOnly)
        {
            var byId = new Dictionary<string, ScriptSymbol>(StringComparer.Ordinal);
            foreach (var s in script.Symbols.Where(s => s.Id != null)) byId.TryAdd(s.Id, s);
            var module = script.Symbols.FirstOrDefault(s => s.Kind == "module");
            var local = new HashSet<string>(script.Symbols.Where(s => index.AllSymbols ? CodeLinkIndex.Indexed(s) : s.Kind == "function" && module != null && s.ParentId == module.Id && s.Name != null)
                .Select(s => index.Normalize(s.Name)), StringComparer.Ordinal);

            // il riferimento di ogni arco: quello scritto, o (script di prima del 10/11) quello di un esterno
            string WrittenOf(ScriptEdge e)
            {
                if (e.Written != null) return e.Written;
                if (!byId.TryGetValue(e.TargetId ?? "", out var t) || t.Kind != "external" || t.Name == null) return null;
                var name = t.Name.StartsWith("?.", StringComparison.Ordinal) ? t.Name[2..] : t.Name;
                foreach (var what in new[] { "file", "module", "symbol" })
                    if (t.Id == ext(what, t.Name) || t.Id == ext(what, "?." + name)) return Written(what, name, null);
                return null;
            }

            // «contenitore.nome» nello stesso file: il simbolo dentro il contenitore, o di primo livello se il contenitore non c'è
            bool LocalQualified(string name)
            {
                var dot = name.LastIndexOf('.');
                if (dot <= 0 || dot == name.Length - 1) return false;
                var owner = index.Normalize(name[..dot]);
                var member = index.Normalize(name[(dot + 1)..]);
                return script.Symbols.Any(s => CodeLinkIndex.Indexed(s) && index.Normalize(s.Name) == member
                    && (s.ParentId != null && byId.TryGetValue(s.ParentId, out var p) && p.Kind != "module"
                        ? index.Normalize(p.Name ?? "") == owner
                        : !index.HasContainer(name[..dot])));
            }

            var result = new List<ScriptEdge>();
            var done = new HashSet<string>(StringComparer.Ordinal);
            var groups = script.Edges.Select(e => (Edge: e, W: WrittenOf(e))).Where(x => x.W != null)
                .GroupBy(x => $"{x.Edge.SourceId}|{x.Edge.Relation}|{x.W}").ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);
            var newExternals = new List<ScriptSymbol>();
            foreach (var e in script.Edges)
            {
                var w = WrittenOf(e);
                if (w == null) { result.Add(e); continue; }
                var key = $"{e.SourceId}|{e.Relation}|{w}";
                if (!done.Add(key)) continue;
                var group = groups[key];
                var (what, name, max) = Parse(w);
                var isCall = what == "symbol";
                // un punto di uscita per nome in una variabile (F4e) non si ricollega: non si sa dove va
                if (what is not ("symbol" or "file" or "module") || isCall == importsOnly) { result.AddRange(group.Select(x => x.Edge)); continue; }
                // nello stesso file l'estrattore ha già scelto, e vince
                if (isCall && (name.Contains('.') ? LocalQualified(name) : local.Contains(index.Normalize(name)))) { result.AddRange(group.Select(x => x.Edge)); continue; }
                var line = group.Min(x => x.Edge.Line);

                List<string> targets; string confidence;
                if (isCall)
                {
                    var candidates = index.Symbols(name).Where(c => c.Path != path).ToList();
                    var imported = index.ImportedClosure(path);
                    var viaImports = candidates.Where(c => imported.Contains(c.Path)).ToList();
                    if (viaImports.Count > 0) { targets = viaImports.Select(c => c.Id).ToList(); confidence = viaImports.Count == 1 ? "extracted" : "ambiguous"; }
                    else { targets = candidates.Select(c => c.Id).ToList(); confidence = candidates.Count == 1 ? "inferred" : "ambiguous"; }
                }
                else
                {
                    var paths = (what == "file" ? index.Files(name) : index.Programs(name)).Where(p => p != path).ToList();
                    targets = paths.Select(index.ModuleOf).Where(id => id != null).ToList();
                    confidence = targets.Count == 1 ? "extracted" : "ambiguous";
                }

                if (targets.Count == 0 || targets.Count > DeclarativeLanguageExtractor.MaxAmbiguousCandidates)
                {
                    var many = targets.Count > 0;
                    var extName = many ? "?." + name : name;
                    var extKind = many ? "symbol" : what;
                    var id = ext(extKind, extName);
                    if (!byId.ContainsKey(id))
                    {
                        var x = new ScriptSymbol { Id = id, Kind = "external", Name = extName, QualifiedName = extName };
                        byId[id] = x; newExternals.Add(x);
                    }
                    targets = new List<string> { id };
                    confidence = many ? "ambiguous" : isCall ? "inferred" : "extracted";
                }
                if (max != null && Rank(max) < Rank(confidence)) confidence = max;
                foreach (var t in targets)
                    result.Add(new ScriptEdge { SourceId = e.SourceId, TargetId = t, Relation = e.Relation, Confidence = confidence, Line = line, Written = w, Guard = e.Guard });
            }

            Finish(script, result, newExternals);
        }

        /// <summary>Un arco per (sorgente, destinazione, relazione), come gli estrattori; i nuovi esterni dentro, quelli senza archi fuori.</summary>
        private static void Finish(CodeScript script, List<ScriptEdge> result, List<ScriptSymbol> newExternals)
        {
            var merged = new List<ScriptEdge>();
            var seen = new Dictionary<string, ScriptEdge>(StringComparer.Ordinal);
            foreach (var e in result)
            {
                var k = $"{e.SourceId}|{e.TargetId}|{e.Relation}";
                if (seen.TryGetValue(k, out var existing))
                {
                    if (Rank(e.Confidence) > Rank(existing.Confidence)) { existing.Confidence = e.Confidence; existing.Line = e.Line; }
                    existing.Written ??= e.Written;
                    continue;
                }
                seen[k] = e; merged.Add(e);
            }
            script.Edges.Clear();
            script.Edges.AddRange(merged);

            script.Symbols.AddRange(newExternals);
            var targeted = new HashSet<string>(script.Edges.Select(e => e.TargetId).Where(t => t != null), StringComparer.Ordinal);
            script.Symbols.RemoveAll(s => s.Kind == "external" && !targeted.Contains(s.Id));
        }
    }
}
