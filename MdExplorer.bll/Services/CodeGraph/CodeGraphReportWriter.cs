using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Il report del grafo del codice (F6, su Fuseki da F7): <c>.md/codegraph/report.md</c>, rigenerato dopo ogni analisi e dopo ogni
    /// giro di descrizioni. Dice che cosa c'è e che cosa manca: simboli più connessi, file più grossi, simboli senza
    /// descrizione, descrizioni da rivedere, archi ambigui. Scritto con le convenzioni di mde-doc (TL;DR in testa).
    /// </summary>
    public sealed class CodeGraphReportWriter
    {
        private readonly ICodeGraphStore _store;
        public CodeGraphReportWriter(ICodeGraphStore store) { _store = store; }

        public static string ReportRelativePath => ".md/codegraph/report.md";
        public static string ReportPath(string projectPath) => Path.Combine(projectPath, ".md", "codegraph", "report.md");

        public async Task<string> WriteAsync(string projectPath, IReadOnlyList<string> notes = null, CancellationToken ct = default)
        {
            var data = await _store.GetReportDataAsync(projectPath, ct);
            var o = data.Overview;
            var undescribed = data.Undescribed;
            var ambiguous = data.Ambiguous;
            var inferred = data.Inferred;
            var stale = data.StaleList;
            var ambiguousList = data.AmbiguousList;
            var undescribedList = data.UndescribedList;

            var sb = new StringBuilder();
            sb.AppendLine("---");
            sb.AppendLine("title: Grafo del codice — report");
            sb.AppendLine("---");
            sb.AppendLine();
            sb.AppendLine("# Grafo del codice — report");
            sb.AppendLine();
            sb.AppendLine("## TL;DR");
            sb.AppendLine($"Il grafo del codice di questo progetto ha {o.Files} file, {o.Symbols} simboli e {o.Edges} archi, generato da MdExplorer il {DateTime.Now:dd/MM/yyyy HH:mm}. Questo report è rigenerato a ogni analisi e a ogni giro di descrizioni: dice che cosa c'è e che cosa manca.");
            sb.AppendLine();
            sb.AppendLine($"- {o.DescribedSymbols} simboli descritti e {o.DescribedFiles} file su {o.Files} con la loro utilità, {undescribed} ancora senza descrizione, {stale.Count} descrizioni da rivedere (firma cambiata).");
            sb.AppendLine($"- {ambiguous} archi ambigui e {inferred} inferiti: le chiamate che l'analisi ha risolto per nome, non con certezza.");
            sb.AppendLine($"- Il simbolo più connesso è {(o.MostConnected.Count > 0 ? "`" + o.MostConnected[0].Symbol.QualifiedName + "`" : "nessuno")}; il file più grosso è {(o.LargestFiles.Count > 0 ? "`" + o.LargestFiles[0].Path + "`" : "nessuno")}.");
            sb.AppendLine();
            sb.AppendLine("## Di che cosa è fatto");
            sb.AppendLine();
            sb.AppendLine("| Linguaggio | Simboli |");
            sb.AppendLine("|---|---|");
            foreach (var kv in o.SymbolsByLanguage.OrderByDescending(k => k.Value)) sb.AppendLine($"| {kv.Key} | {kv.Value} |");
            sb.AppendLine();
            sb.AppendLine("| Relazione | Archi |");
            sb.AppendLine("|---|---|");
            foreach (var kv in o.EdgesByRelation.OrderByDescending(k => k.Value)) sb.AppendLine($"| {kv.Key} | {kv.Value} |");
            sb.AppendLine();
            sb.AppendLine("## I simboli più connessi");
            sb.AppendLine();
            sb.AppendLine("| Simbolo | Tipo | Archi | File |");
            sb.AppendLine("|---|---|---|---|");
            foreach (var (s, deg) in o.MostConnected) sb.AppendLine($"| `{s.QualifiedName}` | {s.Kind} | {deg} | {s.Path}:{s.LineStart} |");
            sb.AppendLine();
            sb.AppendLine("## I file più grossi");
            sb.AppendLine();
            sb.AppendLine("| File | Simboli | A che cosa serve |");
            sb.AppendLine("|---|---|---|");
            foreach (var (filePath, n, summary) in o.LargestFiles) sb.AppendLine($"| {filePath} | {n} | {(summary ?? "—").Replace("|", "\\|").Replace('\n', ' ')} |");
            sb.AppendLine();
            sb.AppendLine("## Senza descrizione");
            sb.AppendLine();
            if (undescribed == 0) sb.AppendLine("Tutti i simboli hanno una descrizione.");
            else
            {
                sb.AppendLine($"{undescribed} simboli. «Descrivi il codice con Mark» nella finestra Ambiente di sviluppo li completa. I primi {undescribedList.Count}:");
                sb.AppendLine();
                foreach (var r in undescribedList) sb.AppendLine($"- {r[0]} `{r[1]}` — {r[2]}:{r[3]}");
            }
            sb.AppendLine();
            sb.AppendLine("## Descrizioni da rivedere");
            sb.AppendLine();
            if (stale.Count == 0) sb.AppendLine("Nessuna: ogni descrizione corrisponde alla firma attuale.");
            else foreach (var r in stale) sb.AppendLine($"- `{r[0]}` — {r[1]}:{r[2]} (la firma è cambiata dopo la descrizione)");
            sb.AppendLine();
            sb.AppendLine("## Archi ambigui");
            sb.AppendLine();
            if (ambiguous == 0) sb.AppendLine("Nessuno.");
            else
            {
                sb.AppendLine($"{ambiguous} archi risolti per nome con più candidati o nessuno (nodo external). I primi {ambiguousList.Count}:");
                sb.AppendLine();
                foreach (var r in ambiguousList) sb.AppendLine($"- `{r[0]}` {r[1]} `{r[2]}` — {r[3]}:{r[4]}");
            }
            if (notes != null && notes.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("## Note dell'ultima analisi");
                sb.AppendLine();
                foreach (var n in notes) sb.AppendLine($"- {n}");
            }

            var path = ReportPath(projectPath);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
            return path;
        }
    }
}
