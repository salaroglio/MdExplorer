using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;

namespace MdExplorer.Features.Services.CodeGraph
{
    public sealed class CodeSymbolView
    {
        public string Id { get; set; }
        public string Kind { get; set; }
        public string Name { get; set; }
        public string QualifiedName { get; set; }
        public string Path { get; set; }
        public int? LineStart { get; set; }
        public int? LineEnd { get; set; }
        public string Signature { get; set; }
        public string ReturnType { get; set; }
        public string Visibility { get; set; }
        public bool IsStatic { get; set; }
        public bool IsAsync { get; set; }
        public string ParentId { get; set; }
        public string Language { get; set; }
        public string Summary { get; set; }
        public string ReturnsDescription { get; set; }
        public string Remarks { get; set; }
        public string WrittenBy { get; set; }
        /// <summary>true quando la descrizione è stata scritta per una firma diversa da quella attuale.</summary>
        public bool? DescriptionStale { get; set; }
        public List<CodeParameterView> Parameters { get; set; } = new();
    }

    public sealed class CodeParameterView
    {
        public int Position { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Direction { get; set; }
        public string DefaultValue { get; set; }
        public bool IsNullable { get; set; }
        public string Description { get; set; }
    }

    public sealed class CodeEdgeView
    {
        public string Relation { get; set; }
        public string Confidence { get; set; }
        public string Path { get; set; }
        public int? Line { get; set; }
        /// <summary>L'altro capo dell'arco (chi chiama, o chi è chiamato, a seconda della direzione chiesta).</summary>
        public CodeSymbolView Other { get; set; }
        public int Depth { get; set; }
    }

    public sealed class CodeOverview
    {
        public int Files { get; set; }
        public int Symbols { get; set; }
        public int ExternalSymbols { get; set; }
        public int Edges { get; set; }
        public int DescribedSymbols { get; set; }
        public Dictionary<string, int> SymbolsByLanguage { get; set; } = new();
        public Dictionary<string, int> SymbolsByKind { get; set; } = new();
        public Dictionary<string, int> EdgesByRelation { get; set; } = new();
        public List<(CodeSymbolView Symbol, int Degree)> MostConnected { get; set; } = new();
        public List<(string Path, int Symbols)> LargestFiles { get; set; } = new();
    }

    /// <summary>
    /// Le domande di navigazione del grafo (F5, D15): i tool leggono SOLO da qui. Vicini e impatto camminano
    /// <c>code_edge</c> con una CTE ricorsiva a profondità limitata; il cammino più corto è una BFS bidirezionale sugli
    /// archi caricati in memoria per quel progetto (decine di migliaia di righe: millisecondi).
    /// </summary>
    public static class CodeGraphQueries
    {
        public const string SymbolColumns =
            "s.Id, s.Kind, s.Name, s.QualifiedName, s.Path, s.LineStart, s.LineEnd, s.Signature, s.ReturnType, s.Visibility, s.IsStatic, s.IsAsync, s.ParentId, s.Language, " +
            "d.Summary, d.ReturnsDescription, d.Remarks, d.WrittenBy, d.SignatureHash";

        public const string SymbolFrom = "FROM code_symbol s LEFT JOIN code_symbol_doc d ON d.SymbolId = s.Id";

        public static CodeSymbolView ReadSymbol(SqliteDataReader r, int offset = 0)
        {
            string S(int i) => r.IsDBNull(offset + i) ? null : r.GetString(offset + i);
            int? I(int i) => r.IsDBNull(offset + i) ? null : r.GetInt32(offset + i);
            return new CodeSymbolView
            {
                Id = S(0), Kind = S(1), Name = S(2), QualifiedName = S(3), Path = S(4), LineStart = I(5), LineEnd = I(6), Signature = S(7),
                ReturnType = S(8), Visibility = S(9), IsStatic = !r.IsDBNull(offset + 10) && r.GetInt64(offset + 10) != 0,
                IsAsync = !r.IsDBNull(offset + 11) && r.GetInt64(offset + 11) != 0, ParentId = S(12), Language = S(13),
                Summary = S(14), ReturnsDescription = S(15), Remarks = S(16), WrittenBy = S(17),
                DescriptionStale = S(18) == null ? null : null, // si calcola in Describe, servono i parametri
            };
        }

        public static CodeSymbolView GetSymbol(SqliteConnection c, string id)
        {
            using var cmd = c.CreateCommand();
            cmd.CommandText = $"SELECT {SymbolColumns} {SymbolFrom} WHERE s.Id = $id";
            cmd.Parameters.AddWithValue("$id", id);
            using var r = cmd.ExecuteReader();
            return r.Read() ? ReadSymbol(r) : null;
        }

        /// <summary>Un simbolo per id oppure per nome qualificato esatto, oppure per nome semplice se unico.</summary>
        public static List<CodeSymbolView> ResolveSymbol(SqliteConnection c, string idOrName, int limit = 10)
        {
            var list = new List<CodeSymbolView>();
            foreach (var sql in new[]
            {
                $"SELECT {SymbolColumns} {SymbolFrom} WHERE s.Id = $q",
                $"SELECT {SymbolColumns} {SymbolFrom} WHERE s.QualifiedName = $q",
                $"SELECT {SymbolColumns} {SymbolFrom} WHERE s.Name = $q AND s.Kind <> 'external' ORDER BY s.QualifiedName LIMIT $n",
                $"SELECT {SymbolColumns} {SymbolFrom} WHERE s.QualifiedName LIKE $like AND s.Kind <> 'external' ORDER BY length(s.QualifiedName) LIMIT $n",
            })
            {
                using var cmd = c.CreateCommand();
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("$q", idOrName);
                cmd.Parameters.AddWithValue("$like", "%" + idOrName + "%");
                cmd.Parameters.AddWithValue("$n", limit);
                using var r = cmd.ExecuteReader();
                while (r.Read()) list.Add(ReadSymbol(r));
                if (list.Count > 0) return list;
            }
            return list;
        }

        public static void FillParameters(SqliteConnection c, CodeSymbolView s)
        {
            using var cmd = c.CreateCommand();
            cmd.CommandText =
                "SELECT p.Position, p.Name, p.ParameterType, p.Direction, p.DefaultValue, p.IsNullable, pd.Description " +
                "FROM code_parameter p LEFT JOIN code_parameter_doc pd ON pd.SymbolId = p.SymbolId AND pd.ParameterName = p.Name " +
                "WHERE p.SymbolId = $id ORDER BY p.Position";
            cmd.Parameters.AddWithValue("$id", s.Id);
            using var r = cmd.ExecuteReader();
            s.Parameters.Clear();
            while (r.Read())
            {
                s.Parameters.Add(new CodeParameterView
                {
                    Position = r.GetInt32(0), Name = r.GetString(1), Type = r.IsDBNull(2) ? null : r.GetString(2), Direction = r.GetString(3),
                    DefaultValue = r.IsDBNull(4) ? null : r.GetString(4), IsNullable = r.GetInt64(5) != 0, Description = r.IsDBNull(6) ? null : r.GetString(6),
                });
            }
            // «da rivedere»: la descrizione fu scritta per un'altra firma
            using var cmd2 = c.CreateCommand();
            cmd2.CommandText = "SELECT SignatureHash FROM code_symbol_doc WHERE SymbolId = $id";
            cmd2.Parameters.AddWithValue("$id", s.Id);
            var stored = cmd2.ExecuteScalar() as string;
            if (stored != null)
            {
                var current = CodeDocPlanner.SignatureHash(
                    new ScriptSymbol { Kind = s.Kind, QualifiedName = s.QualifiedName, Signature = s.Signature, ReturnType = s.ReturnType },
                    s.Parameters.Select(p => new ScriptParameter { Position = p.Position, Name = p.Name, ParameterType = p.Type, Direction = p.Direction, DefaultValue = p.DefaultValue }));
                s.DescriptionStale = !string.Equals(stored, current, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>I simboli di un file, in ordine di riga: l'albero lo ricostruisce chi legge, da ParentId.</summary>
        public static List<CodeSymbolView> Outline(SqliteConnection c, string path)
        {
            using var cmd = c.CreateCommand();
            cmd.CommandText = $"SELECT {SymbolColumns} {SymbolFrom} WHERE s.Path = $p ORDER BY s.LineStart, s.Kind";
            cmd.Parameters.AddWithValue("$p", path);
            var list = new List<CodeSymbolView>();
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(ReadSymbol(r));
            return list;
        }

        /// <summary>
        /// I vicini di un simbolo fino a <paramref name="depth"/> salti: <c>out</c> = archi che partono da lui (chi usa,
        /// chi chiama, che cosa istanzia), <c>in</c> = archi che arrivano (chi lo chiama, chi lo implementa).
        /// </summary>
        public static List<CodeEdgeView> Neighbors(SqliteConnection c, string symbolId, string relation, string direction, int depth, int limit)
        {
            depth = Math.Clamp(depth, 1, 5); limit = Math.Clamp(limit, 1, 500);
            var outward = !string.Equals(direction, "in", StringComparison.OrdinalIgnoreCase);
            var fromCol = outward ? "SourceId" : "TargetId";
            var toCol = outward ? "TargetId" : "SourceId";
            using var cmd = c.CreateCommand();
            cmd.CommandText =
                $"WITH RECURSIVE walk(id, depth) AS (" +
                $"  SELECT $start, 0 " +
                $"  UNION " +
                $"  SELECT e.{toCol}, w.depth + 1 FROM code_edge e JOIN walk w ON e.{fromCol} = w.id WHERE w.depth < $depth" +
                (relation != null ? " AND e.Relation = $rel" : string.Empty) +
                $") " +
                $"SELECT DISTINCT e.Relation, e.Confidence, e.Path, e.Line, w.depth + 1 AS d, {SymbolColumns} " +
                $"FROM walk w JOIN code_edge e ON e.{fromCol} = w.id " +
                (relation != null ? "AND e.Relation = $rel " : string.Empty) +
                $"JOIN code_symbol s ON s.Id = e.{toCol} LEFT JOIN code_symbol_doc d ON d.SymbolId = s.Id " +
                $"WHERE w.depth < $depth ORDER BY d, e.Relation, s.QualifiedName LIMIT $limit";
            cmd.Parameters.AddWithValue("$start", symbolId);
            cmd.Parameters.AddWithValue("$depth", depth);
            cmd.Parameters.AddWithValue("$limit", limit);
            if (relation != null) cmd.Parameters.AddWithValue("$rel", relation);
            var list = new List<CodeEdgeView>();
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new CodeEdgeView
                {
                    Relation = r.GetString(0), Confidence = r.GetString(1), Path = r.GetString(2), Line = r.IsDBNull(3) ? null : r.GetInt32(3),
                    Depth = r.GetInt32(4), Other = ReadSymbol(r, 5),
                });
            }
            return list;
        }

        /// <summary>Che cosa dipende da un simbolo, anche indirettamente: gli archi entranti seguiti a ritroso (calls, references, inherits, implements, instantiates, imports).</summary>
        public static List<CodeEdgeView> Impact(SqliteConnection c, string symbolId, int depth, int limit)
            => Neighbors(c, symbolId, null, "in", depth, limit).Where(e => e.Relation != "contains").ToList();

        /// <summary>Il cammino più corto fra due simboli seguendo gli archi in entrambi i versi (BFS in memoria).</summary>
        public static List<CodeEdgeView> ShortestPath(SqliteConnection c, string fromId, string toId, int maxDepth = 8)
        {
            var adj = new Dictionary<string, List<(string To, long EdgeRow)>>(StringComparer.Ordinal);
            var edges = new Dictionary<long, (string Src, string Tgt, string Rel, string Conf, string Path, int? Line)>();
            using (var cmd = c.CreateCommand())
            {
                cmd.CommandText = "SELECT Id, SourceId, TargetId, Relation, Confidence, Path, Line FROM code_edge";
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    var id = r.GetInt64(0); var s = r.GetString(1); var t = r.GetString(2);
                    edges[id] = (s, t, r.GetString(3), r.GetString(4), r.GetString(5), r.IsDBNull(6) ? null : r.GetInt32(6));
                    if (!adj.TryGetValue(s, out var ls)) adj[s] = ls = new(); ls.Add((t, id));
                    if (!adj.TryGetValue(t, out var lt)) adj[t] = lt = new(); lt.Add((s, id));
                }
            }
            var prev = new Dictionary<string, (string From, long Edge)>(StringComparer.Ordinal) { [fromId] = (null, -1) };
            var queue = new Queue<(string Id, int D)>(); queue.Enqueue((fromId, 0));
            while (queue.Count > 0)
            {
                var (cur, d) = queue.Dequeue();
                if (cur == toId) break;
                if (d >= maxDepth || !adj.TryGetValue(cur, out var next)) continue;
                foreach (var (to, edge) in next)
                {
                    if (prev.ContainsKey(to)) continue;
                    prev[to] = (cur, edge);
                    queue.Enqueue((to, d + 1));
                }
            }
            if (!prev.ContainsKey(toId)) return new List<CodeEdgeView>();
            var path = new List<CodeEdgeView>();
            for (var cur = toId; prev[cur].From != null; cur = prev[cur].From)
            {
                var e = edges[prev[cur].Edge];
                var forward = e.Src == prev[cur].From;
                path.Add(new CodeEdgeView
                {
                    Relation = (forward ? string.Empty : "← ") + e.Rel, Confidence = e.Conf, Path = e.Path, Line = e.Line,
                    Other = GetSymbol(c, cur), Depth = path.Count + 1,
                });
            }
            path.Reverse();
            for (var i = 0; i < path.Count; i++) path[i].Depth = i + 1;
            return path;
        }

        public static CodeOverview Overview(SqliteConnection c)
        {
            var o = new CodeOverview();
            int Count(string sql) { using var cmd = c.CreateCommand(); cmd.CommandText = sql; return Convert.ToInt32(cmd.ExecuteScalar()); }
            o.Files = Count("SELECT COUNT(*) FROM code_file");
            o.Symbols = Count("SELECT COUNT(*) FROM code_symbol WHERE Kind <> 'external'");
            o.ExternalSymbols = Count("SELECT COUNT(*) FROM code_symbol WHERE Kind = 'external'");
            o.Edges = Count("SELECT COUNT(*) FROM code_edge");
            o.DescribedSymbols = Count("SELECT COUNT(*) FROM code_symbol_doc");
            void Group(string sql, Dictionary<string, int> into)
            {
                using var cmd = c.CreateCommand(); cmd.CommandText = sql; using var r = cmd.ExecuteReader();
                while (r.Read()) into[r.IsDBNull(0) ? "?" : r.GetString(0)] = r.GetInt32(1);
            }
            Group("SELECT Language, COUNT(*) FROM code_symbol WHERE Kind <> 'external' GROUP BY Language", o.SymbolsByLanguage);
            Group("SELECT Kind, COUNT(*) FROM code_symbol GROUP BY Kind", o.SymbolsByKind);
            Group("SELECT Relation, COUNT(*) FROM code_edge GROUP BY Relation", o.EdgesByRelation);
            using (var cmd = c.CreateCommand())
            {
                cmd.CommandText =
                    $"SELECT deg.n, {SymbolColumns} FROM (SELECT SourceId AS id, COUNT(*) AS n FROM code_edge WHERE Relation <> 'contains' GROUP BY SourceId " +
                    "UNION ALL SELECT TargetId, COUNT(*) FROM code_edge WHERE Relation <> 'contains' GROUP BY TargetId) deg " +
                    $"JOIN code_symbol s ON s.Id = deg.id LEFT JOIN code_symbol_doc d ON d.SymbolId = s.Id WHERE s.Kind <> 'external' " +
                    "GROUP BY s.Id ORDER BY SUM(deg.n) DESC LIMIT 15";
                using var r = cmd.ExecuteReader();
                while (r.Read()) o.MostConnected.Add((ReadSymbol(r, 1), r.GetInt32(0)));
            }
            using (var cmd = c.CreateCommand())
            {
                cmd.CommandText = "SELECT Path, SymbolCount FROM code_file ORDER BY SymbolCount DESC LIMIT 10";
                using var r = cmd.ExecuteReader();
                while (r.Read()) o.LargestFiles.Add((r.GetString(0), r.GetInt32(1)));
            }
            return o;
        }
    }
}
