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
        private readonly Dictionary<string, HashSet<string>> _imports = new(StringComparer.Ordinal);

        private CodeLinkIndex(bool ignoreCase) { _norm = ignoreCase ? n => n.ToUpperInvariant() : n => n; }

        public static CodeLinkIndex Build(bool ignoreCase, IEnumerable<(string Path, CodeScript Script)> scripts)
        {
            var index = new CodeLinkIndex(ignoreCase);
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
            foreach (var s in script.Symbols.Where(s => s.Kind == "function" && s.ParentId == module.Id && !string.IsNullOrEmpty(s.Name)))
            {
                if (!_symbols.TryGetValue(_norm(s.Name), out var list)) _symbols[_norm(s.Name)] = list = new();
                list.Add((path, s.Id));
            }
            SetImports(path, script);
        }

        /// <summary>Gli import di un file come sono adesso nel suo script (dopo il ricollegamento degli import).</summary>
        internal void SetImports(string path, CodeScript script)
            => _imports[path] = new HashSet<string>(script.Edges.Where(e => e.Relation == "imports" && _pathOfModule.ContainsKey(e.TargetId ?? ""))
                .Select(e => _pathOfModule[e.TargetId]), StringComparer.Ordinal);

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
        internal IReadOnlyList<(string Path, string Id)> Symbols(string name) => _symbols.TryGetValue(_norm(name), out var l) ? l : (IReadOnlyList<(string, string)>)Array.Empty<(string, string)>();

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
            IReadOnlyList<(string Path, CodeScript Script)> others)
        {
            var all = own.Concat(others).ToList();
            var index = CodeLinkIndex.Build(ignoreCase, all);
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
            var local = new HashSet<string>(script.Symbols.Where(s => s.Kind == "function" && module != null && s.ParentId == module.Id && s.Name != null)
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
                if (what == null || isCall == importsOnly) { result.AddRange(group.Select(x => x.Edge)); continue; }
                // nello stesso file l'estrattore ha già scelto, e vince
                if (isCall && local.Contains(index.Normalize(name))) { result.AddRange(group.Select(x => x.Edge)); continue; }
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

            // un arco per (sorgente, destinazione, relazione), come l'estrattore: vince la confidenza migliore
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
