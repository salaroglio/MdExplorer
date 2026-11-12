using System;
using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// L'SQL scritto nel codice e gli oggetti del database analizzati (12/11/2026, utente: «già che c'è guarda se riesce a fare
    /// match con i nodi sconosciuti»). Una tabella o una procedura che l'SQL di un metodo C# o Visual Basic nomina è un nodo
    /// esterno del linguaggio <c>sql</c> (<see cref="RoslynExtractorBase.SqlLanguage"/>); quando un linguaggio database
    /// (<see cref="DeclarativeLanguage.Database"/>, il PL/SQL) ha un simbolo con quel nome, la freccia va a lui. Se poi quel
    /// simbolo sparisce, o il nome diventa ambiguo, la freccia torna al nodo esterno: il riferimento scritto
    /// (<see cref="ScriptEdge.Written"/>, <c>sql:table:NOME</c>) dice da dove ripartire.
    /// <para>
    /// Si ricollegano solo questi esterni, «trovabili dopo»: le classi di libreria non compariranno mai, e cercarle per nome
    /// semplice inventerebbe collegamenti. Il nome si confronta senza maiuscole, intero o senza lo schema davanti
    /// (<c>GEST.PKG.PROC</c> → <c>PKG.PROC</c>); con più di un simbolo la freccia resta all'esterno.
    /// </para>
    /// </summary>
    public static class CodeSqlLinker
    {
        public const string WrittenPrefix = "sql:";

        /// <summary>Il nodo esterno di una tabella (<c>table</c>) o di una procedura (<c>symbol</c>), come lo fa l'estrattore.</summary>
        public static string ExternalId(string kind, string name)
            => RoslynCSharpExtractor.StableId(RoslynExtractorBase.SqlLanguage, "external", kind + ":" + name.ToUpperInvariant());

        /// <summary>Che cosa nomina un nodo esterno dell'SQL: <c>table</c> o <c>symbol</c>; null per il nodo «?» o per un altro esterno.</summary>
        public static string KindOf(ScriptSymbol external)
        {
            if (external?.Kind != "external" || external.Language != RoslynExtractorBase.SqlLanguage || string.IsNullOrEmpty(external.QualifiedName)) return null;
            foreach (var kind in new[] { "table", "symbol" })
                if (external.Id == ExternalId(kind, external.QualifiedName)) return kind;
            return null;
        }

        public static (string Kind, string Name) ParseWritten(string written)
        {
            if (written == null || !written.StartsWith(WrittenPrefix, StringComparison.Ordinal)) return (null, null);
            var rest = written[WrittenPrefix.Length..];
            var colon = rest.IndexOf(':');
            return colon <= 0 ? (null, null) : (rest[..colon], rest[(colon + 1)..]);
        }

        /// <summary>
        /// L'indice dei simboli dei linguaggi database: per nome e per nome qualificato dopo il file
        /// (<c>db/ordini.pkb:PKG_ORDINI.ELENCO</c> → <c>PKG_ORDINI.ELENCO</c>), senza maiuscole.
        /// </summary>
        public static Func<string, string> Resolver(IEnumerable<CodeOwnerMember> databaseSymbols)
        {
            var index = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            void Add(string key, string id)
            {
                if (string.IsNullOrWhiteSpace(key) || id == null) return;
                if (!index.TryGetValue(key, out var ids)) index[key] = ids = new HashSet<string>(StringComparer.Ordinal);
                ids.Add(id);
            }
            foreach (var s in databaseSymbols)
            {
                Add(s.MemberName, s.MemberId);
                var qn = s.MemberQualifiedName ?? string.Empty;
                Add(qn[(qn.LastIndexOf(':') + 1)..], s.MemberId);
            }
            string One(string key) => index.TryGetValue(key, out var ids) && ids.Count == 1 ? ids.First() : null;
            return name =>
            {
                if (string.IsNullOrWhiteSpace(name)) return null;
                var n = name.Trim();
                if (index.ContainsKey(n)) return One(n);
                var dot = n.IndexOf('.');
                return dot > 0 && dot < n.Length - 1 ? One(n[(dot + 1)..]) : null;   // senza lo schema davanti
            };
        }

        /// <summary>
        /// Le frecce dell'SQL di uno script verso il simbolo che <paramref name="resolve"/> trova, o verso il nodo esterno se non lo
        /// trova. Torna quante frecce hanno cambiato bersaglio (0 = script invariato).
        /// </summary>
        public static int Relink(CodeScript script, Func<string, string> resolve)
        {
            var byId = new Dictionary<string, ScriptSymbol>(StringComparer.Ordinal);
            foreach (var s in script.Symbols.Where(s => s.Id != null)) byId.TryAdd(s.Id, s);
            var moved = 0;
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var result = new List<ScriptEdge>();
            foreach (var e in script.Edges)
            {
                var (kind, name) = ParseWritten(e.Written);
                if (kind == null && e.TargetId != null && byId.TryGetValue(e.TargetId, out var target) && KindOf(target) is { } k)
                    (kind, name) = (k, target.QualifiedName);
                if (kind != null)
                {
                    var found = resolve(name);
                    var to = found ?? ExternalId(kind, name);
                    if (found == null && !byId.ContainsKey(to))
                    {
                        var x = new ScriptSymbol { Id = to, Kind = "external", Name = name.Split('.').Last(), QualifiedName = name, Language = RoslynExtractorBase.SqlLanguage };
                        byId[to] = x; script.Symbols.Add(x);
                    }
                    if (to != e.TargetId) moved++;
                    e.TargetId = to;
                    e.Written = found == null ? null : WrittenPrefix + kind + ":" + name;
                }
                if (seen.Add($"{e.SourceId}|{e.Relation}|{e.TargetId}|{e.Line}")) result.Add(e);
            }
            if (moved == 0) return 0;
            script.Edges.Clear();
            script.Edges.AddRange(result);
            var targeted = new HashSet<string>(script.Edges.Select(e => e.TargetId).Where(t => t != null), StringComparer.Ordinal);
            script.Symbols.RemoveAll(s => s.Kind == "external" && s.Language == RoslynExtractorBase.SqlLanguage && !targeted.Contains(s.Id));
            return moved;
        }
    }
}
