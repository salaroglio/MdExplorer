using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Data.Sqlite;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Il report del grafo del codice (F6): <c>.md/codegraph/report.md</c>, rigenerato dopo ogni analisi e dopo ogni
    /// giro di descrizioni. Dice che cosa c'è e che cosa manca: simboli più connessi, file più grossi, simboli senza
    /// descrizione, descrizioni da rivedere, archi ambigui. Scritto con le convenzioni di mde-doc (TL;DR in testa).
    /// </summary>
    public sealed class CodeGraphReportWriter
    {
        private readonly ICodeGraphDatabase _db;
        public CodeGraphReportWriter(ICodeGraphDatabase db) { _db = db; }

        public static string ReportRelativePath => ".md/codegraph/report.md";
        public static string ReportPath(string projectPath) => Path.Combine(projectPath, ".md", "codegraph", "report.md");

        public string Write(string projectPath, IReadOnlyList<string> notes = null)
        {
            using var c = _db.OpenConnection(projectPath);
            var o = CodeGraphQueries.Overview(c);
            var undescribed = Count(c, "SELECT COUNT(*) FROM code_symbol s LEFT JOIN code_symbol_doc d ON d.SymbolId = s.Id WHERE s.Kind NOT IN ('external','module','field') AND d.SymbolId IS NULL");
            var ambiguous = Count(c, "SELECT COUNT(*) FROM code_edge WHERE Confidence = 'ambiguous'");
            var inferred = Count(c, "SELECT COUNT(*) FROM code_edge WHERE Confidence = 'inferred'");
            var stale = StaleDescriptions(c);
            var ambiguousList = Rows(c, "SELECT s.QualifiedName, e.Relation, t.QualifiedName, e.Path, e.Line FROM code_edge e JOIN code_symbol s ON s.Id = e.SourceId JOIN code_symbol t ON t.Id = e.TargetId WHERE e.Confidence = 'ambiguous' ORDER BY e.Path, e.Line LIMIT 30");
            var undescribedList = Rows(c, "SELECT s.Kind, s.QualifiedName, s.Path, s.LineStart FROM code_symbol s LEFT JOIN code_symbol_doc d ON d.SymbolId = s.Id WHERE s.Kind NOT IN ('external','module','field') AND d.SymbolId IS NULL ORDER BY s.Path, s.LineStart LIMIT 40");

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
            sb.AppendLine($"- {o.DescribedSymbols} simboli descritti, {undescribed} ancora senza descrizione, {stale.Count} descrizioni da rivedere (firma cambiata).");
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
            sb.AppendLine("| File | Simboli |");
            sb.AppendLine("|---|---|");
            foreach (var (filePath, n) in o.LargestFiles) sb.AppendLine($"| {filePath} | {n} |");
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

        private static List<string[]> StaleDescriptions(SqliteConnection c)
        {
            var list = new List<string[]>();
            using var cmd = c.CreateCommand();
            cmd.CommandText = $"SELECT {CodeGraphQueries.SymbolColumns} {CodeGraphQueries.SymbolFrom} WHERE d.SymbolId IS NOT NULL";
            var symbols = new List<CodeSymbolView>();
            using (var r = cmd.ExecuteReader()) while (r.Read()) symbols.Add(CodeGraphQueries.ReadSymbol(r));
            foreach (var s in symbols)
            {
                CodeGraphQueries.FillParameters(c, s);
                if (s.DescriptionStale == true) list.Add(new[] { s.QualifiedName, s.Path, s.LineStart?.ToString() });
                if (list.Count >= 40) break;
            }
            return list;
        }

        private static List<string[]> Rows(SqliteConnection c, string sql)
        {
            var list = new List<string[]>();
            using var cmd = c.CreateCommand();
            cmd.CommandText = sql;
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                var row = new string[r.FieldCount];
                for (var i = 0; i < r.FieldCount; i++) row[i] = r.IsDBNull(i) ? "" : r.GetValue(i).ToString();
                list.Add(row);
            }
            return list;
        }

        private static int Count(SqliteConnection c, string sql) { using var cmd = c.CreateCommand(); cmd.CommandText = sql; return Convert.ToInt32(cmd.ExecuteScalar()); }
    }
}
