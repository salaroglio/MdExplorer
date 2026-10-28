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
        /// <summary>L'id del simbolo: solo per le candidate di un arco ambiguo, che l'LLM indica per id nella sua scelta (F12c).</summary>
        public string Id { get; init; }
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
    /// domande con le loro dipendenze. Una classe (interfaccia, enum, record) viene dopo i suoi membri. Una chiamata
    /// <c>ambiguous</c> fa aspettare TUTTE le sue candidate (utente, 22/10/2026: «qualunque chiamata, ambigua o meno, rende
    /// una funzione NON foglia»); non aspettano gli esterni, i metodi banali e chi è già descritto. Un blocco spezzato in
    /// frammenti si descrive come un tipo, dopo i suoi pezzi; i salti fra i pezzi (<c>jumps</c>) NON sono dipendenze:
    /// i GOTO all'indietro farebbero un ciclo grande quanto il blocco, cioè di nuovo una domanda sola.
    /// </summary>
    public sealed class CodeDocGraph
    {
        private readonly Dictionary<string, (ScriptSymbol Symbol, string Source)> _symbols = new(StringComparer.Ordinal);
        private readonly Dictionary<string, List<ScriptEdge>> _calls = new(StringComparer.Ordinal);
        private readonly Dictionary<string, List<string>> _members = new(StringComparer.Ordinal);
        private readonly Dictionary<string, List<ScriptEdge>> _jumps = new(StringComparer.Ordinal);
        private readonly Dictionary<string, ScriptSymbol> _externals = new(StringComparer.Ordinal);

        private static readonly HashSet<string> TypeKinds = new(StringComparer.Ordinal)
            { "class", "interface", "record", "struct", "enum", "type", "component", "service", "directive", "pipe", "ngmodule" };

        /// <summary>
        /// Chi si descrive mettendo insieme i suoi pezzi, già descritti: un tipo (i membri) e un blocco spezzato in frammenti
        ///. Il suo estratto toglie i corpi dei pezzi, e le loro sintesi arrivano in <c>members</c>.
        /// </summary>
        public static bool FromParts(string kind, IEnumerable<string> childKinds)
            => TypeKinds.Contains(kind) || childKinds.Contains(DeclarativeLanguage.FragmentKind);

        private bool FromParts(string id)
            => _symbols.TryGetValue(id, out var me) && FromParts(me.Symbol.Kind,
                _members.TryGetValue(id, out var ms) ? ms.Where(_symbols.ContainsKey).Select(m => _symbols[m].Symbol.Kind) : Enumerable.Empty<string>());

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
                else if (e.Relation == DeclarativeLanguage.JumpsRelation)
                {
                    if (!_jumps.TryGetValue(e.SourceId, out var list)) _jumps[e.SourceId] = list = new List<ScriptEdge>();
                    list.Add(e);
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
                        // anche le candidate di una chiamata ambigua (22/10/2026): per scegliere quella vera l'LLM deve
                        // conoscerle; prima non davano ordine e il chiamante passava prima di loro, «foglia» per sbaglio
                        if (e.TargetId != n && byId.ContainsKey(e.TargetId)) deps.Add(e.TargetId);
                if (FromParts(n) && _members.TryGetValue(n, out var members))
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
                var options = line.Select(e => Call(e.TargetId, null, summaryOf, withId: true)).Where(c => c != null).ToList();
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

        /// <summary>
        /// Dove passa il codice di un frammento: i frammenti verso cui salta o prosegue, con la riga del salto. Solo i
        /// nomi: i salti non sono dipendenze, quindi la sintesi di chi sta dall'altra parte di solito non c'è ancora.
        /// </summary>
        public List<CodeDocCall> JumpsOf(string symbolId)
        {
            var result = new List<CodeDocCall>();
            if (!_jumps.TryGetValue(symbolId, out var jumps)) return result;
            foreach (var e in jumps.OrderBy(e => e.Line ?? 0).ThenBy(e => e.TargetId, StringComparer.Ordinal))
                if (_symbols.TryGetValue(e.TargetId, out var s))
                    result.Add(new CodeDocCall { Line = e.Line, QualifiedName = s.Symbol.QualifiedName, Signature = s.Symbol.Signature });
            return result;
        }

        private Dictionary<string, int> _cycleOf;

        /// <summary>
        /// L'impronta di quello che una descrizione sa intorno al simbolo (F12b): le sintesi delle funzioni chiamate (anche
        /// le candidate di un arco ambiguo, i nomi degli esterni) e, per un tipo, dei membri. Le funzioni dello stesso ciclo
        /// di chiamate restano fuori: si descrivono nella stessa domanda e la loro sintesi non c'è ancora quando si scrive.
        /// Stessa <paramref name="summaryOf"/> del prompt, così l'impronta è quella di quello che l'LLM ha visto.
        /// </summary>
        public string ContextHash(string symbolId, Func<string, string> summaryOf)
        {
            _cycleOf ??= CyclesOfAll();
            _cycleOf.TryGetValue(symbolId, out var mine);
            bool SameCycle(string id) => _cycleOf.TryGetValue(id, out var c) && c == mine && mine != 0;
            var tokens = new List<string>();
            string Token(string id) => _symbols.TryGetValue(id, out var s) ? $"{s.Symbol.QualifiedName}={summaryOf(id) ?? "∅"}"
                : _externals.TryGetValue(id, out var x) ? $"ext:{x.QualifiedName}" : $"?:{id}";
            if (_calls.TryGetValue(symbolId, out var calls))
                foreach (var e in calls)
                    if (e.TargetId != symbolId && !SameCycle(e.TargetId)) tokens.Add((e.Confidence == "ambiguous" ? "amb:" : "call:") + Token(e.TargetId));
            if (FromParts(symbolId) && _members.TryGetValue(symbolId, out var members))
                foreach (var m in members)
                    if (_symbols.TryGetValue(m, out var ms) && CodeDocPlanner.IsDescribable(ms.Symbol.Kind)) tokens.Add("member:" + Token(m));
            tokens.Sort(StringComparer.Ordinal);
            return CodeBodyHash.Hash(string.Join("\n", tokens.Distinct(StringComparer.Ordinal)));
        }

        /// <summary>I cicli di chiamate fra TUTTI i simboli descrivibili non banali: id → numero del ciclo (1…), assente se non è in un ciclo.</summary>
        private Dictionary<string, int> CyclesOfAll()
        {
            var nodes = _symbols.Where(kv => CodeDocPlanner.IsDescribable(kv.Value.Symbol.Kind) && kv.Value.Symbol.Trivial == null).Select(kv => kv.Key).OrderBy(k => k, StringComparer.Ordinal).ToList();
            var set = new HashSet<string>(nodes, StringComparer.Ordinal);
            var adj = nodes.ToDictionary(n => n, n => _calls.TryGetValue(n, out var c)
                ? c.Where(e => e.TargetId != n && set.Contains(e.TargetId)).Select(e => e.TargetId).Distinct(StringComparer.Ordinal).OrderBy(t => t, StringComparer.Ordinal).ToList()
                : new List<string>(), StringComparer.Ordinal);
            var (_, components) = Tarjan(nodes, adj);
            var result = new Dictionary<string, int>(StringComparer.Ordinal);
            var n = 0;
            foreach (var comp in components.Where(c => c.Count > 1)) { n++; foreach (var v in comp) result[v] = n; }
            return result;
        }

        /// <summary>
        /// Le candidate di una chiamata ambigua di <paramref name="callerId"/> alla riga <paramref name="line"/> (F12c): per
        /// accettare la scelta dell'LLM solo fra queste, e per sapere quali scarta.
        /// </summary>
        public List<string> AmbiguousAt(string callerId, int? line)
            => _calls.TryGetValue(callerId, out var calls)
                ? calls.Where(e => e.Confidence == "ambiguous" && e.Line == line).Select(e => e.TargetId).Distinct(StringComparer.Ordinal).ToList()
                : new List<string>();

        private CodeDocCall Call(string targetId, int? line, Func<string, string> summaryOf, bool withId = false)
        {
            if (_symbols.TryGetValue(targetId, out var s))
                return new CodeDocCall { Id = withId ? targetId : null, Line = line, QualifiedName = s.Symbol.QualifiedName, Signature = s.Symbol.Signature, Summary = summaryOf(targetId) };
            if (_externals.TryGetValue(targetId, out var x))
                return new CodeDocCall { Id = withId ? targetId : null, Line = line, QualifiedName = x.QualifiedName, External = true };
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
