using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static MdExplorer.Features.Services.CodeGraph.CodeGraphIris;
using static MdExplorer.Features.Services.CodeGraph.SparqlResults;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// La mappa per livelli (vista guidata, passo 2 del 14/10/2026). Un livello alla volta: i programmi del progetto, oppure
    /// le sottocartelle di una cartella; le frecce sono SOMMATE fra le parti, e la somma la fa Fuseki (GROUP BY sulla
    /// prima cartella sotto quella corrente), così anche con 15.000 file arrivano poche righe. Senza <c>contains</c>
    /// (struttura) né <c>imports</c> (rumore: due parti lontane «collegate» da un import comune).
    /// </summary>
    public sealed partial class CodeGraphStore
    {
        /// <summary>Quante parti si disegnano al massimo: le altre vanno in «+N altre».</summary>
        public const int MapMaxGroups = 24;
        private const string RelationFilter = "?rel != \"contains\" && ?rel != \"imports\"";
        private const string OutsideMarker = "__OUT__";

        public async Task<CodeGraphMap> MapAsync(string projectPath, string folder, IReadOnlyList<string> programs, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            folder = (folder ?? string.Empty).Replace('\\', '/').Trim('/');
            return folder.Length == 0 ? await ProgramsMapAsync(projectPath, g, programs, ct) : await FolderMapAsync(g, folder, ct);
        }

        private async Task<CodeGraphMap> ProgramsMapAsync(string projectPath, ProjectGraphs g, IReadOnlyList<string> programs, CancellationToken ct)
        {
            var map = new CodeGraphMap { Level = "programs", Folder = string.Empty };
            foreach (var p in programs)
            {
                var c = await ProgramCountsAsync(projectPath, p, ct);
                map.Groups.Add(new CodeGraphMapGroup { Id = p, Label = p, Kind = "program", Folder = p, Files = c.Files, Symbols = c.Symbols });
                map.Files += c.Files; map.Symbols += c.Symbols;
            }
            if (programs.Count > 1)
            {
                // un passaggio solo sugli archi: a quale programma appartiene ciascun capo lo dice un IF annidato, prima i
                // prefissi più lunghi («src/Acme.Core» prima di «src»); con VALUES × VALUES ogni arco si provava con ogni
                // coppia (5,7 s su 385 file, 14/10/2026)
                string Owner(string variable) => programs.OrderByDescending(p => p.Length)
                    .Aggregate("\"\"", (inner, p) => $"IF(STRSTARTS({variable}, {Lit(p.TrimEnd('/') + "/")}), {Lit(p)}, {inner})");
                var rows = await RowsAsync(
                    $"SELECT ?a ?b ?rel ?conf (COUNT(?e) AS ?n) WHERE {{ GRAPH {g.DataIri} {{ " +
                    "?e a code:Edge ; code:relation ?rel ; code:confidence ?conf0 ; code:path ?sp ; code:target ?t . ?t code:path ?tp . " + EffectiveConfidence +
                    $"FILTER({RelationFilter}) BIND({Owner("?sp")} AS ?a) BIND({Owner("?tp")} AS ?b) " +
                    "FILTER(?a != \"\" && ?b != \"\" && ?a != ?b) } } GROUP BY ?a ?b ?rel ?conf", ct);
                foreach (var r in rows)
                    AddLink(map, Str(r, "a"), Str(r, "b"), Str(r, "rel"), Str(r, "conf"), Count(r, "n"));
            }
            return map;
        }

        private async Task<CodeGraphMap> FolderMapAsync(ProjectGraphs g, string folder, CancellationToken ct)
        {
            // i file sotto la cartella: da qui la compattazione e i gruppi (percorso, simboli)
            var files = (await RowsAsync(
                $"SELECT ?p ?n WHERE {{ GRAPH {g.DataIri} {{ ?f a code:File ; code:path ?p ; code:symbolCount ?n . FILTER(STRSTARTS(?p, {Lit(folder + "/")})) }} }}", ct))
                .Select(r => (Path: Str(r, "p"), Symbols: Count(r, "n"))).ToList();

            // una catena di cartelle con un figlio solo e nessun file (src/main/java/com/acme) si attraversa da sola
            var current = folder;
            while (files.Count > 0)
            {
                var rels = files.Select(f => f.Path.Substring(current.Length + 1)).ToList();
                if (rels.Any(r => !r.Contains('/'))) break;
                var first = rels.Select(r => r.Substring(0, r.IndexOf('/'))).Distinct().ToList();
                if (first.Count != 1) break;
                current += "/" + first[0];
            }
            var map = new CodeGraphMap { Level = "folders", Folder = current, Files = files.Count, Symbols = files.Sum(f => f.Symbols) };
            if (files.Count == 0) return map;

            // un gruppo per sottocartella (anche lei compattata: «main/java/com/acme»), più i file sciolti qui
            var segOf = new Dictionary<string, string>(StringComparer.Ordinal);   // primo segmento → id del gruppo
            var groups = new Dictionary<string, CodeGraphMapGroup>(StringComparer.Ordinal);
            foreach (var bySeg in files.GroupBy(f => FirstSegment(f.Path.Substring(current.Length + 1))))
            {
                CodeGraphMapGroup group;
                if (bySeg.Key.Length == 0)
                    group = new CodeGraphMapGroup { Id = "__files", Kind = "files", Folder = null, Label = string.Empty };
                else
                {
                    var deep = CommonDirectory(bySeg.Select(f => f.Path.Substring(current.Length + 1)).ToList());
                    group = new CodeGraphMapGroup { Id = current + "/" + deep, Kind = "folder", Folder = current + "/" + deep, Label = deep };
                }
                group.Files = bySeg.Count(); group.Symbols = bySeg.Sum(f => f.Symbols);
                groups[group.Id] = group;
                segOf[bySeg.Key] = group.Id;
            }

            // le più grandi si disegnano, le altre vanno insieme in «+N altre»
            var kept = groups.Values.OrderByDescending(x => x.Kind == "files" ? int.MaxValue : x.Files).ThenBy(x => x.Id, StringComparer.Ordinal).Take(MapMaxGroups).ToList();
            var keptIds = kept.Select(x => x.Id).ToHashSet(StringComparer.Ordinal);
            var rest = groups.Values.Where(x => !keptIds.Contains(x.Id)).ToList();
            map.Groups.AddRange(kept);
            if (rest.Count > 0)
            {
                map.HiddenGroups = rest.Count;
                map.More = rest.OrderByDescending(x => x.Files).ThenBy(x => x.Id, StringComparer.Ordinal).ToList();
                map.Groups.Add(new CodeGraphMapGroup { Id = "__more", Kind = "more", Label = rest.Count.ToString(), Files = rest.Sum(x => x.Files), Symbols = rest.Sum(x => x.Symbols) });
            }
            string IdOf(string seg)
            {
                if (seg == OutsideMarker) return "__out";
                if (!segOf.TryGetValue(seg, out var id)) return null;
                return keptIds.Contains(id) ? id : "__more";
            }

            var prefix = Lit(current + "/");
            var bind = "BIND(IF(CONTAINS(?ra, \"/\"), STRBEFORE(?ra, \"/\"), \"\") AS ?ga) ";
            // dall'interno: verso un'altra parte o fuori
            var outgoing = await RowsAsync(
                $"SELECT ?ga ?gb ?rel ?conf (COUNT(?e) AS ?n) WHERE {{ GRAPH {g.DataIri} {{ " +
                "?e a code:Edge ; code:relation ?rel ; code:confidence ?conf0 ; code:path ?sp ; code:target ?t . " + EffectiveConfidence +
                $"FILTER({RelationFilter} && STRSTARTS(?sp, {prefix})) ?t code:path ?tp . " +
                $"BIND(STRAFTER(?sp, {prefix}) AS ?ra) {bind}" +
                $"BIND(IF(STRSTARTS(?tp, {prefix}), STRAFTER(?tp, {prefix}), \"{OutsideMarker}/\") AS ?rb) " +
                "BIND(IF(CONTAINS(?rb, \"/\"), STRBEFORE(?rb, \"/\"), \"\") AS ?gb) } } GROUP BY ?ga ?gb ?rel ?conf", ct);
            // da fuori verso l'interno
            var incoming = await RowsAsync(
                $"SELECT ?ga ?rel ?conf (COUNT(?e) AS ?n) WHERE {{ GRAPH {g.DataIri} {{ " +
                "?e a code:Edge ; code:relation ?rel ; code:confidence ?conf0 ; code:path ?sp ; code:target ?t . ?t code:path ?tp . " + EffectiveConfidence +
                $"FILTER({RelationFilter} && STRSTARTS(?tp, {prefix}) && !STRSTARTS(?sp, {prefix})) " +
                $"BIND(STRAFTER(?tp, {prefix}) AS ?ra) {bind}}} }} GROUP BY ?ga ?rel ?conf", ct);

            foreach (var r in outgoing)
            {
                var from = IdOf(Str(r, "ga")); var to = IdOf(Str(r, "gb"));
                if (from == null || to == null || from == to) continue;   // dentro la stessa parte: non è una freccia di questo livello
                AddLink(map, from, to, Str(r, "rel"), Str(r, "conf"), Count(r, "n"));
            }
            foreach (var r in incoming)
            {
                var to = IdOf(Str(r, "ga"));
                if (to == null) continue;
                AddLink(map, "__out", to, Str(r, "rel"), Str(r, "conf"), Count(r, "n"));
            }
            if (map.Links.Any(l => l.From == "__out" || l.To == "__out"))
                map.Groups.Add(new CodeGraphMapGroup { Id = "__out", Kind = "outside", Label = string.Empty, Folder = ParentOf(current) });
            return map;
        }

        private static void AddLink(CodeGraphMap map, string from, string to, string relation, string confidence, int count)
        {
            var link = map.Links.FirstOrDefault(l => l.From == from && l.To == to);
            if (link == null) map.Links.Add(link = new CodeGraphMapLink { From = from, To = to });
            link.Count += count;
            link.ByRelation[relation] = link.ByRelation.GetValueOrDefault(relation) + count;
            link.ByConfidence[confidence] = link.ByConfidence.GetValueOrDefault(confidence) + count;
        }

        private static string FirstSegment(string rel) => rel.Contains('/') ? rel.Substring(0, rel.IndexOf('/')) : string.Empty;

        /// <summary>La cartella comune ai file (percorsi relativi, tutti dentro la stessa prima cartella): «main/java/com/acme».</summary>
        private static string CommonDirectory(List<string> rels)
        {
            var dirs = rels.Select(r => r.Split('/').SkipLast(1).ToArray()).ToList();
            var common = new List<string>();
            for (var i = 0; ; i++)
            {
                if (dirs.Any(d => d.Length <= i)) break;
                var seg = dirs[0][i];
                if (dirs.Any(d => d[i] != seg)) break;
                common.Add(seg);
            }
            return string.Join("/", common);
        }

        private static string ParentOf(string folder) => folder.Contains('/') ? folder.Substring(0, folder.LastIndexOf('/')) : string.Empty;
    }
}
