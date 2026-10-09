using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite;

namespace MdExplorer.Features.Services.CodeGraph
{
    public sealed class CodeGraphStatus
    {
        public bool ScriptsFolderExists { get; set; }
        public int ScriptsOnDisk { get; set; }
        public int ScriptsLoaded { get; set; }
        /// <summary>Script su disco con hash diverso da quello caricato, o mai caricati: «conoscenza da generare».</summary>
        public int ScriptsPending { get; set; }
        public int Files { get; set; }
        public int Symbols { get; set; }
        public int ExternalSymbols { get; set; }
        public int Parameters { get; set; }
        public int Edges { get; set; }
        public int DescribedSymbols { get; set; }
        public string DatabasePath { get; set; }
    }

    public sealed class CodeSymbolHit
    {
        public string Id { get; set; }
        public string Kind { get; set; }
        public string Name { get; set; }
        public string QualifiedName { get; set; }
        public string Path { get; set; }
        public int? LineStart { get; set; }
        public string Signature { get; set; }
        public string Summary { get; set; }
    }

    /// <summary>
    /// Il modello di lettura del grafo del codice (D15): i tool leggono SOLO da qui, mai dagli script. In F1 porta lo
    /// stato e la ricerca FTS; le domande di navigazione (vicini, cammini, impatto) arrivano in F5.
    /// </summary>
    public sealed class CodeGraphRepository
    {
        private readonly ICodeGraphDatabase _db;

        public CodeGraphRepository(ICodeGraphDatabase db) { _db = db; }

        public CodeGraphStatus GetStatus(string projectPath)
        {
            var status = new CodeGraphStatus { DatabasePath = _db.GetDatabasePath(projectPath) };
            using var c = _db.OpenConnection(projectPath);

            status.Files = Count(c, "SELECT COUNT(*) FROM code_file");
            status.Symbols = Count(c, "SELECT COUNT(*) FROM code_symbol WHERE Kind <> 'external'");
            status.ExternalSymbols = Count(c, "SELECT COUNT(*) FROM code_symbol WHERE Kind = 'external'");
            status.Parameters = Count(c, "SELECT COUNT(*) FROM code_parameter");
            status.Edges = Count(c, "SELECT COUNT(*) FROM code_edge");
            status.DescribedSymbols = Count(c, "SELECT COUNT(*) FROM code_symbol_doc");
            status.ScriptsLoaded = Count(c, "SELECT COUNT(*) FROM code_script_state");

            var root = CodeGraphPaths.GetScriptsRoot(projectPath);
            status.ScriptsFolderExists = Directory.Exists(root);
            if (!status.ScriptsFolderExists) return status;

            var loaded = new Dictionary<string, string>(StringComparer.Ordinal);
            using (var cmd = c.CreateCommand())
            {
                cmd.CommandText = "SELECT ScriptPath, ContentHash FROM code_script_state";
                using var r = cmd.ExecuteReader();
                while (r.Read()) loaded[r.GetString(0)] = r.GetString(1);
            }
            foreach (var f in Directory.EnumerateFiles(root, "*.jsonl", SearchOption.AllDirectories)
                                       .Where(f => CodeGraphPaths.IsAstScript(f) || CodeGraphPaths.IsDocScript(f)))
            {
                status.ScriptsOnDisk++;
                var rel = CodeGraphPaths.ToProjectRelative(projectPath, f);
                if (!loaded.TryGetValue(rel, out var hash) ||
                    !string.Equals(hash, CodeScriptLoader.Sha256(File.ReadAllText(f)), StringComparison.OrdinalIgnoreCase))
                {
                    status.ScriptsPending++;
                }
            }
            return status;
        }

        /// <summary>Ricerca a testo libero (FTS5 trigram) su nomi e descrizioni. Il testo deve avere almeno 3 caratteri.</summary>
        public IReadOnlyList<CodeSymbolHit> SearchSymbols(string projectPath, string query, int limit = 20)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 3)
                throw new ArgumentException("La ricerca FTS5 trigram richiede almeno 3 caratteri.", nameof(query));
            using var c = _db.OpenConnection(projectPath);
            using var cmd = c.CreateCommand();
            cmd.CommandText =
                "SELECT s.Id, s.Kind, s.Name, s.QualifiedName, s.Path, s.LineStart, s.Signature, d.Summary " +
                "FROM code_symbol_fts f JOIN code_symbol s ON s.Id = f.symbol_id LEFT JOIN code_symbol_doc d ON d.SymbolId = s.Id " +
                "WHERE code_symbol_fts MATCH $q ORDER BY bm25(code_symbol_fts) LIMIT $n";
            // Il testo va fra virgolette: così FTS5 lo prende alla lettera (niente operatori AND/OR/NOT interpretati).
            cmd.Parameters.AddWithValue("$q", "\"" + query.Trim().Replace("\"", "\"\"") + "\"");
            cmd.Parameters.AddWithValue("$n", Math.Clamp(limit, 1, 200));
            var hits = new List<CodeSymbolHit>();
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                hits.Add(new CodeSymbolHit
                {
                    Id = r.GetString(0), Kind = r.GetString(1), Name = r.GetString(2), QualifiedName = r.GetString(3),
                    Path = r.IsDBNull(4) ? null : r.GetString(4), LineStart = r.IsDBNull(5) ? null : r.GetInt32(5),
                    Signature = r.IsDBNull(6) ? null : r.GetString(6), Summary = r.IsDBNull(7) ? null : r.GetString(7),
                });
            }
            return hits;
        }

        // ------------------------------------------------------------------ navigazione (F5)

        /// <summary>Il simbolo per id, nome qualificato o nome; con parametri e descrizioni. Più candidati = ambiguità da sciogliere.</summary>
        public List<CodeSymbolView> Resolve(string projectPath, string idOrName, int limit = 10)
        {
            using var c = _db.OpenConnection(projectPath);
            var list = CodeGraphQueries.ResolveSymbol(c, idOrName, limit);
            foreach (var s in list) CodeGraphQueries.FillParameters(c, s);
            return list;
        }

        public CodeSymbolView Describe(string projectPath, string symbolId)
        {
            using var c = _db.OpenConnection(projectPath);
            var s = CodeGraphQueries.GetSymbol(c, symbolId);
            if (s != null) CodeGraphQueries.FillParameters(c, s);
            return s;
        }

        public List<CodeSymbolView> Outline(string projectPath, string path)
        {
            using var c = _db.OpenConnection(projectPath);
            return CodeGraphQueries.Outline(c, path);
        }

        public List<CodeEdgeView> Neighbors(string projectPath, string symbolId, string relation, string direction, int depth, int limit)
        {
            using var c = _db.OpenConnection(projectPath);
            return CodeGraphQueries.Neighbors(c, symbolId, string.IsNullOrWhiteSpace(relation) ? null : relation, direction, depth, limit);
        }

        public List<CodeEdgeView> Impact(string projectPath, string symbolId, int depth, int limit)
        {
            using var c = _db.OpenConnection(projectPath);
            return CodeGraphQueries.Impact(c, symbolId, depth, limit);
        }

        public List<CodeEdgeView> ShortestPath(string projectPath, string fromId, string toId)
        {
            using var c = _db.OpenConnection(projectPath);
            return CodeGraphQueries.ShortestPath(c, fromId, toId);
        }

        public CodeOverview Overview(string projectPath)
        {
            using var c = _db.OpenConnection(projectPath);
            return CodeGraphQueries.Overview(c);
        }

        private static int Count(SqliteConnection c, string sql)
        {
            using var cmd = c.CreateCommand();
            cmd.CommandText = sql;
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }
}
