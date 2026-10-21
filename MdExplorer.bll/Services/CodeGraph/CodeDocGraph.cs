using System;
using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Una domanda all'LLM nella descrizione dal basso (F12a): i simboli di un file allo stesso livello, oppure un ciclo che
    /// attraversa più file. Parte quando le domande in <see cref="DependsOn"/> sono finite.
    /// </summary>
    public sealed class CodeDocGroup
    {
        public int Index { get; init; }
        public int Level { get; init; }
        /// <summary>I percorsi logici dei sorgenti, ordinati: uno, o più per un ciclo fra file.</summary>
        public IReadOnlyList<string> Sources { get; init; }
        public IReadOnlyList<CodeDocCandidate> Candidates { get; init; }
        public IReadOnlyList<int> DependsOn { get; init; }
        public bool IsCrossFileCycle => Sources.Count > 1;
    }

    /// <summary>Una funzione chiamata, per il prompt di chi la chiama (F12a, punto 2).</summary>
    public sealed class CodeDocCall
    {
        public int? Line { get; init; }
        public string QualifiedName { get; init; }
        public string Signature { get; init; }
        public string Summary { get; init; }
        public bool External { get; init; }
        /// <summary>Arco <c>ambiguous</c>: le candidate, una delle quali è quella vera; null altrimenti.</summary>
        public IReadOnlyList<CodeDocCall> Candidates { get; init; }
    }

    /// <summary>
    /// La parte deterministica della descrizione dal basso (D39-D41): dagli script AST di una cartella program il grafo
    /// delle chiamate fra i simboli da descrivere, i cicli raggruppati (Tarjan), i livelli — 0 chi non chiama niente da
    /// descrivere, sennò 1 + il più alto dei chiamati, rigoroso per funzione anche dentro lo stesso file (D40) — e le
    /// domande con le loro dipendenze. Una classe (interfaccia, enum, record) viene dopo i suoi membri. Gli archi
    /// <c>ambiguous</c> non danno ordine: entrano nel prompt come candidate.
    /// </summary>
    public sealed class CodeDocGraph
    {
        private readonly Dictionary<string, (ScriptSymbol Symbol, string Source)> _symbols = new(StringComparer.Ordinal);
        private readonly Dictionary<string, List<ScriptEdge>> _calls = new(StringComparer.Ordinal);
        private readonly Dictionary<string, List<string>> _members = new(StringComparer.Ordinal);
        private readonly Dictionary<string, ScriptSymbol> _externals = new(StringComparer.Ordinal);

        private static readonly HashSet<string> TypeKinds = new(StringComparer.Ordinal)
            { "class", "interface", "record", "struct", "enum", "type", "component", "service", "directive", "pipe", "ngmodule" };

        /// <summary>Archi verso simboli che nessuno script della cartella dichiara: non dovrebbero esserci (l'estrattore li fa esterni).</summary>
        public List<string> Warnings { get; } = new();

        public CodeDocGraph(IEnumerable<CodeScript> astScripts)
        {
            var scripts = astScripts.ToList();
            foreach (var script in scripts)
                foreach (var s in script.Symbols)
                {
                    if (s.IsExternal) _externals.TryAdd(s.Id, s);
                    else _symbols[s.Id] = (s, script.Header.Source);
                }
            foreach (var e in scripts.SelectMany(sc => sc.Edges))
            {
                if (e.Relation == "calls")
                {
                    if (!_calls.TryGetValue(e.SourceId, out var list)) _calls[e.SourceId] = list = new List<ScriptEdge>();
                    list.Add(e);
                }
                else if (e.Relation == "contains")
                {
                    if (!_members.TryGetValue(e.SourceId, out var list)) _members[e.SourceId] = list = new List<string>();
                    list.Add(e.TargetId);
                }
            }
        }

        /// <summary>
        /// Le domande, in un ordine in cui ognuna viene dopo quelle da cui dipende (per livello, poi per file): stesso
        /// risultato a parità di script, così una ripresa con <c>force</c> riparte dallo stesso punto.
        /// </summary>
        public List<CodeDocGroup> Plan(IEnumerable<CodeDocCandidate> candidates)
        {
            var byId = new Dictionary<string, CodeDocCandidate>(StringComparer.Ordinal);
            foreach (var c in candidates) byId[c.Symbol.Id] = c;
            var nodes = byId.Keys.OrderBy(k => byId[k].Source, StringComparer.Ordinal).ThenBy(k => byId[k].Symbol.LineStart ?? 0).ThenBy(k => k, StringComparer.Ordinal).ToList();

            // chi deve venire prima di chi: le funzioni chiamate (non ambigue) e, per un tipo, i suoi membri
            var adj = nodes.ToDictionary(n => n, n => new List<string>(), StringComparer.Ordinal);
            foreach (var n in nodes)
            {
                var deps = new HashSet<string>(StringComparer.Ordinal);
                if (_calls.TryGetValue(n, out var calls))
                    foreach (var e in calls)
                        if (e.Confidence != "ambiguous" && e.TargetId != n && byId.ContainsKey(e.TargetId)) deps.Add(e.TargetId);
                if (TypeKinds.Contains(byId[n].Symbol.Kind) && _members.TryGetValue(n, out var members))
                    foreach (var m in members)
                        if (m != n && byId.ContainsKey(m)) deps.Add(m);
                adj[n].AddRange(deps.OrderBy(d => d, StringComparer.Ordinal));
            }

            var (component, components) = Tarjan(nodes, adj);
            // Tarjan chiude prima i pozzi: in quell'ordine i livelli dei chiamati sono già noti
            var level = new int[components.Count];
            for (var c = 0; c < components.Count; c++)
            {
                var l = 0;
                foreach (var v in components[c])
                    foreach (var w in adj[v])
                        if (component[w] != c) l = Math.Max(l, level[component[w]] + 1);
                level[c] = l;
            }

            // una domanda = (livello, file); un ciclo che attraversa più file = una domanda sua
            var keyOf = new Dictionary<string, string>(StringComparer.Ordinal);
            var groupMembers = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            var groupLevel = new Dictionary<string, int>(StringComparer.Ordinal);
            for (var c = 0; c < components.Count; c++)
            {
                var files = components[c].Select(v => byId[v].Source).Distinct(StringComparer.Ordinal).OrderBy(f => f, StringComparer.Ordinal).ToList();
                var key = files.Count == 1 ? $"{level[c]:D6}|{files[0]}" : $"{level[c]:D6}|~ciclo|{string.Join("|", files)}|{components[c].Min(StringComparer.Ordinal)}";
                if (!groupMembers.TryGetValue(key, out var list)) { groupMembers[key] = list = new List<string>(); groupLevel[key] = level[c]; }
                foreach (var v in components[c]) { list.Add(v); keyOf[v] = key; }
            }
            var orderedKeys = groupMembers.Keys.OrderBy(k => k, StringComparer.Ordinal).ToList();
            var indexOf = orderedKeys.Select((k, i) => (k, i)).ToDictionary(x => x.k, x => x.i, StringComparer.Ordinal);
            var groups = new List<CodeDocGroup>();
            foreach (var key in orderedKeys)
            {
                var members = groupMembers[key].OrderBy(v => byId[v].Source, StringComparer.Ordinal).ThenBy(v => byId[v].Symbol.LineStart ?? 0).ThenBy(v => v, StringComparer.Ordinal).ToList();
                var deps = members.SelectMany(v => adj[v]).Select(w => keyOf[w]).Where(k => k != key).Distinct(StringComparer.Ordinal).Select(k => indexOf[k]).OrderBy(i => i).ToList();
                groups.Add(new CodeDocGroup
                {
                    Index = indexOf[key], Level = groupLevel[key],
                    Sources = members.Select(v => byId[v].Source).Distinct(StringComparer.Ordinal).OrderBy(f => f, StringComparer.Ordinal).ToList(),
                    Candidates = members.Select(v => byId[v]).ToList(),
                    DependsOn = deps,
                });
            }
            return groups;
        }

        /// <summary>
        /// Le funzioni che <paramref name="symbolId"/> chiama, per il prompt: nome qualificato, firma, riga e la sintesi che
        /// c'è adesso (<paramref name="summaryOf"/>: null = non ancora descritta). Gli archi ambigui della stessa riga
        /// diventano una voce con le candidate; un esterno è marcato tale.
        /// </summary>
        public List<CodeDocCall> CallsOf(string symbolId, Func<string, string> summaryOf)
        {
            var result = new List<CodeDocCall>();
            if (!_calls.TryGetValue(symbolId, out var calls)) return result;
            foreach (var e in calls.Where(e => e.Confidence != "ambiguous" && e.TargetId != symbolId).OrderBy(e => e.Line ?? 0).ThenBy(e => e.TargetId, StringComparer.Ordinal))
            {
                var call = Call(e.TargetId, e.Line, summaryOf);
                if (call != null) result.Add(call);
            }
            foreach (var line in calls.Where(e => e.Confidence == "ambiguous").GroupBy(e => e.Line).OrderBy(g => g.Key ?? 0))
            {
                var options = line.Select(e => Call(e.TargetId, null, summaryOf)).Where(c => c != null).ToList();
                if (options.Count == 0) continue;
                result.Add(new CodeDocCall { Line = line.Key, Candidates = options });
            }
            return result;
        }

        /// <summary>I membri descrivibili di un tipo con la loro sintesi: la classe si descrive mettendoli insieme.</summary>
        public List<CodeDocCall> MembersOf(string symbolId, Func<string, string> summaryOf)
        {
            var result = new List<CodeDocCall>();
            if (!_members.TryGetValue(symbolId, out var members)) return result;
            foreach (var m in members.Distinct(StringComparer.Ordinal))
                if (_symbols.TryGetValue(m, out var s) && CodeDocPlanner.IsDescribable(s.Symbol.Kind))
                    result.Add(new CodeDocCall { Line = s.Symbol.LineStart, QualifiedName = s.Symbol.QualifiedName, Signature = s.Symbol.Signature, Summary = summaryOf(m) });
            return result.OrderBy(c => c.Line ?? 0).ToList();
        }

        private CodeDocCall Call(string targetId, int? line, Func<string, string> summaryOf)
        {
            if (_symbols.TryGetValue(targetId, out var s))
                return new CodeDocCall { Line = line, QualifiedName = s.Symbol.QualifiedName, Signature = s.Symbol.Signature, Summary = summaryOf(targetId) };
            if (_externals.TryGetValue(targetId, out var x))
                return new CodeDocCall { Line = line, QualifiedName = x.QualifiedName, External = true };
            Warnings.Add($"arco verso '{targetId}', che nessuno script della cartella dichiara");
            return null;
        }

        /// <summary>Tarjan iterativo (i 40.000 simboli di PlantUML fanno saltare lo stack della versione ricorsiva).</summary>
        private static (Dictionary<string, int> Component, List<List<string>> Components) Tarjan(List<string> nodes, Dictionary<string, List<string>> adj)
        {
            var index = new Dictionary<string, int>(StringComparer.Ordinal);
            var low = new Dictionary<string, int>(StringComparer.Ordinal);
            var onStack = new HashSet<string>(StringComparer.Ordinal);
            var stack = new Stack<string>();
            var component = new Dictionary<string, int>(StringComparer.Ordinal);
            var components = new List<List<string>>();
            var next = 0;
            foreach (var root in nodes)
            {
                if (index.ContainsKey(root)) continue;
                var work = new Stack<(string Node, int Edge)>();
                index[root] = low[root] = next++; stack.Push(root); onStack.Add(root);
                work.Push((root, 0));
                while (work.Count > 0)
                {
                    var (v, i) = work.Pop();
                    if (i < adj[v].Count)
                    {
                        work.Push((v, i + 1));
                        var w = adj[v][i];
                        if (!index.ContainsKey(w))
                        {
                            index[w] = low[w] = next++; stack.Push(w); onStack.Add(w);
                            work.Push((w, 0));
                        }
                        else if (onStack.Contains(w)) low[v] = Math.Min(low[v], index[w]);
                        continue;
                    }
                    if (work.Count > 0) { var parent = work.Peek().Node; low[parent] = Math.Min(low[parent], low[v]); }
                    if (low[v] != index[v]) continue;
                    var comp = new List<string>();
                    string x;
                    do { x = stack.Pop(); onStack.Remove(x); component[x] = components.Count; comp.Add(x); } while (x != v);
                    components.Add(comp);
                }
            }
            return (component, components);
        }
    }
}
