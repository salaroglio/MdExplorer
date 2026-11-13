using System;
using System.Collections.Generic;
using System.Linq;
using MdExplorer.Features.Services;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Dalla chiamata che legge una query di un catalogo XML alla query (13/11/2026): per ogni lettore del pacchetto
    /// (<see cref="DeclarativeLanguage.XmlLoaderRule"/>) i punti di chiamata del grafo con quel metodo danno, dagli argomenti per
    /// posizione, il gruppo, il nome e i frammenti; l'arco è <c>dispatches</c> (una stringa nomina quello che viene eseguito).
    /// Gli argomenti sono quelli che il grafo conosce: scritti, o passati per una variabile o per rami diversi (un punto per
    /// valore). Un argomento che non si conosce non filtra (il file, di solito: viene dalla configurazione).
    /// <list type="bullet">
    /// <item>frammenti scritti (<c>fragment</c>/<c>fragments</c>): quei frammenti, tutti eseguiti;</item>
    /// <item>senza, con <c>defaultFragment</c>: quel frammento;</item>
    /// <item>senza, e la query ha frammenti: tutti i frammenti, <c>ambiguous</c> (uno di loro, non si sa quale);</item>
    /// <item>una query senza frammenti: la query.</item>
    /// </list>
    /// Più query con quel gruppo e nome (in file diversi): verso tutte, <c>ambiguous</c>. Nessuna: un mancato, da dire.
    /// </summary>
    public static class XmlCatalogLinker
    {
        /// <summary>Una query o un frammento del grafo: il file, il gruppo (o null), il nome della query, il frammento (null per la query).</summary>
        public sealed record Entry(string Id, string Path, string Group, string Query, string Fragment);

        public sealed record Miss(string Path, int Line, string Text);

        /// <summary>
        /// Le query e i frammenti dai simboli dei linguaggi a cataloghi: nome qualificato <c>file:gruppo.nome[.frammento]</c>
        /// (con <c>@riga</c> per un doppione), tipo del contenitore per sapere che cosa è che cosa.
        /// </summary>
        public static List<Entry> Entries(IEnumerable<CodeOwnerMember> symbols)
        {
            var list = new List<Entry>();
            foreach (var s in symbols.Where(s => s.MemberKind == "query" && s.MemberQualifiedName != null))
            {
                var qn = s.MemberQualifiedName;
                var colon = qn.LastIndexOf(':');
                if (colon <= 0) continue;
                var path = qn[..colon];
                var parts = qn[(colon + 1)..].Split('.').Select(p => { var at = p.IndexOf('@'); return at < 0 ? p : p[..at]; }).ToList();
                switch (s.ParentKind)
                {
                    case "namespace" when parts.Count == 2: list.Add(new Entry(s.MemberId, path, parts[0], parts[1], null)); break;
                    case "module" when parts.Count == 1: list.Add(new Entry(s.MemberId, path, null, parts[0], null)); break;
                    case "query" when parts.Count == 3: list.Add(new Entry(s.MemberId, path, parts[0], parts[1], parts[2])); break;
                    case "query" when parts.Count == 2: list.Add(new Entry(s.MemberId, path, null, parts[0], parts[1])); break;
                }
            }
            return list;
        }

        public static List<ScriptEdge> Link(IReadOnlyList<DeclarativeLanguage.XmlLoaderRule> loaders, Func<string, IReadOnlyList<CodeCallSiteView>> sitesOf,
            IReadOnlyList<Entry> entries, List<Miss> misses)
        {
            var edges = new Dictionary<string, ScriptEdge>(StringComparer.Ordinal);
            void Add(CodeCallSiteView site, string target, string confidence, string guard)
            {
                var key = $"{site.CallerId}|{target}";
                if (edges.TryGetValue(key, out var existing))
                {
                    if (Rank(confidence) > Rank(existing.Confidence)) { existing.Confidence = confidence; existing.Line = site.Line; existing.Guard = guard; }
                    return;
                }
                edges[key] = new ScriptEdge { SourceId = site.CallerId, TargetId = target, Relation = "dispatches", Confidence = confidence, Line = site.Line, Path = site.Path, Guard = guard };
            }

            foreach (var loader in loaders)
                foreach (var site in sitesOf(loader.Method))
                {
                    if (loader.Receiver != null && !(site.Receiver ?? string.Empty).EndsWith(loader.Receiver, StringComparison.OrdinalIgnoreCase)) continue;
                    // la classe del lettore: un metodo con lo stesso nome in un'altra classe non è lui (13/11/2026, dal cliente: un
                    // lettore C# omonimo, per un altro XML); una chiamata che il compilatore non risolve non si può dire sua
                    if (loader.Class != null && !IsClass(site.CalleeType, loader.Class)) continue;
                    string Arg(int? position) => position is { } p && site.Arguments.TryGetValue(p, out var v) && !string.IsNullOrWhiteSpace(v) ? v.Trim() : null;
                    var name = loader.NameValue ?? Arg(loader.Name);
                    if (name == null) continue;   // il nome non si conosce: nessun arco, e il grafo non inventa
                    var group = loader.GroupValue ?? Arg(loader.Group);
                    // il file: il valore scritto, o solo il nome con cui finisce (MapPath, configurazione + "\ordini.xml")
                    var file = Arg(loader.File)
                        ?? (loader.File is { } fp && site.FileNames.TryGetValue(fp, out var fn) && !string.IsNullOrWhiteSpace(fn) ? fn.Trim() : null);
                    if (file != null) file = file.Replace('\\', '/')[(file.Replace('\\', '/').LastIndexOf('/') + 1)..];
                    var guard = $"{loader.Method}({string.Join(", ", new[] { file == null ? null : $"file = \"{file}\"", group == null ? null : $"group = \"{group}\"", $"name = \"{name}\"" }.Where(x => x != null))})";
                    var queries = entries.Where(e => e.Fragment == null && e.Query == name && (group == null || e.Group == group)
                        && (file == null || e.Path.EndsWith("/" + file, StringComparison.OrdinalIgnoreCase) || e.Path.Equals(file, StringComparison.OrdinalIgnoreCase))).ToList();
                    if (queries.Count == 0)
                    {
                        misses.Add(new Miss(site.Path, site.Line, (group == null ? "" : group + ".") + name));
                        continue;
                    }
                    var sure = queries.Count == 1;
                    List<Entry> FragmentsOf(Entry q) => entries.Where(e => e.Fragment != null && e.Path == q.Path && e.Group == q.Group && e.Query == q.Query).ToList();
                    var written = loader.Fragments.Select(p => Arg(p)).Where(f => f != null).ToList();
                    foreach (var q in queries)
                    {
                        var fragments = FragmentsOf(q);
                        if (written.Count > 0 || loader.DefaultFragment != null)
                        {
                            foreach (var f in written.Count > 0 ? written : new List<string> { loader.DefaultFragment })
                            {
                                var hit = fragments.FirstOrDefault(x => x.Fragment == f);
                                if (hit == null) { misses.Add(new Miss(site.Path, site.Line, (q.Group == null ? "" : q.Group + ".") + q.Query + "." + f)); continue; }
                                Add(site, hit.Id, sure ? "inferred" : "ambiguous", guard);
                            }
                        }
                        else if (fragments.Count > 0)
                            foreach (var f in fragments) Add(site, f.Id, sure && fragments.Count == 1 ? "inferred" : "ambiguous", guard);
                        else Add(site, q.Id, sure ? "inferred" : "ambiguous", guard);
                    }
                }
            return edges.Values.OrderBy(e => e.Path, StringComparer.Ordinal).ThenBy(e => e.Line).ThenBy(e => e.SourceId, StringComparer.Ordinal).ThenBy(e => e.TargetId, StringComparer.Ordinal).ToList();
        }

        /// <summary>La classe risolta è quella del lettore: uguale, o con quella fine dopo un punto (<c>Lettore</c> per <c>Demo.Dati.Lettore</c>).</summary>
        private static bool IsClass(string calleeType, string wanted)
            => calleeType != null && (calleeType.Equals(wanted, StringComparison.OrdinalIgnoreCase) || calleeType.EndsWith("." + wanted, StringComparison.OrdinalIgnoreCase));

        private static int Rank(string confidence) => confidence switch { "extracted" => 2, "inferred" => 1, _ => 0 };
    }
}
