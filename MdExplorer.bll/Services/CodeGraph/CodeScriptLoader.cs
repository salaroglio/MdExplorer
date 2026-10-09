using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>L'esito del caricamento di uno script.</summary>
    public sealed class CodeScriptResult
    {
        public string ScriptPath { get; set; }
        public string Kind { get; set; }
        public bool Loaded { get; set; }
        public bool Skipped { get; set; }
        public bool Removed { get; set; }
        public int RecordCount { get; set; }
        public string Error { get; set; }
        public bool HasError => Error != null;
    }

    /// <summary>L'esito della «generazione della conoscenza» su un progetto.</summary>
    public sealed class CodeScriptLoadOutcome
    {
        public bool ScriptsFolderMissing { get; set; }
        public List<CodeScriptResult> Results { get; } = new();
        public int Loaded => Results.Count(r => r.Loaded);
        public int Skipped => Results.Count(r => r.Skipped);
        public int Removed => Results.Count(r => r.Removed);
        public int Failed => Results.Count(r => r.HasError);
        public string FirstError => Results.FirstOrDefault(r => r.HasError)?.Error;
    }

    public interface ICodeScriptLoader
    {
        /// <summary>Carica tutti gli script di <c>.mde-code/</c> cambiati dall'ultima volta e toglie quelli spariti.</summary>
        CodeScriptLoadOutcome LoadProject(string projectPath, CancellationToken ct = default);

        /// <summary>Carica (o ricarica, se l'hash è diverso) un singolo script.</summary>
        CodeScriptResult LoadScript(string projectPath, string scriptAbsolutePath);

        /// <summary>Toglie dal DB tutto ciò che uno script aveva portato.</summary>
        CodeScriptResult RemoveScript(string projectPath, string scriptAbsolutePath);
    }

    /// <summary>
    /// Il passo «script → database» della generazione della conoscenza (D9). Gli script in <c>.mde-code/</c> sono la
    /// verità; qui si confronta l'hash di ciascuno con <c>code_script_state</c> e si (ri)carica solo chi è cambiato,
    /// in una transazione per script: o entra tutto o niente.
    /// <list type="bullet">
    /// <item>Script <b>ast</b>: cancella e riscrive file, simboli, parametri e archi di QUEL sorgente; i simboli
    /// <c>external</c> sono condivisi, si inseriscono se mancano e non si cancellano mai da qui.</item>
    /// <item>Script <b>doc</b>: cancella e riscrive le descrizioni portate da QUEL file (per <c>ScriptPath</c>),
    /// agganciate al simbolo per id: ricaricare l'AST non le tocca (D11).</item>
    /// <item>Dopo entrambi si rigenera la riga FTS dei simboli coinvolti (nome + descrizioni).</item>
    /// </list>
    /// Un errore di formato ferma quello script, non gli altri, e finisce nell'esito con file, riga e campo.
    /// </summary>
    public sealed class CodeScriptLoader : ICodeScriptLoader
    {
        private readonly ICodeGraphDatabase _db;
        private readonly ILogger<CodeScriptLoader> _logger;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new(StringComparer.OrdinalIgnoreCase);

        public CodeScriptLoader(ICodeGraphDatabase db, ILogger<CodeScriptLoader> logger)
        {
            _db = db;
            _logger = logger;
        }

        public CodeScriptLoadOutcome LoadProject(string projectPath, CancellationToken ct = default)
        {
            var outcome = new CodeScriptLoadOutcome();
            var root = CodeGraphPaths.GetScriptsRoot(projectPath);
            var gate = GateFor(projectPath);
            gate.Wait(ct);
            try
            {
                using var connection = _db.OpenConnection(projectPath);
                var known = ReadScriptStates(connection);

                var onDisk = Directory.Exists(root)
                    ? Directory.EnumerateFiles(root, "*.jsonl", SearchOption.AllDirectories)
                        .Where(f => CodeGraphPaths.IsAstScript(f) || CodeGraphPaths.IsDocScript(f))
                        .Select(f => Path.GetFullPath(f))
                        .ToList()
                    : new List<string>();
                outcome.ScriptsFolderMissing = !Directory.Exists(root);

                // Prima gli ast (i simboli), poi i doc (che vi si agganciano): così la FTS esce completa.
                foreach (var file in onDisk.Where(CodeGraphPaths.IsAstScript).OrderBy(f => f, StringComparer.Ordinal)
                                           .Concat(onDisk.Where(CodeGraphPaths.IsDocScript).OrderBy(f => f, StringComparer.Ordinal)))
                {
                    ct.ThrowIfCancellationRequested();
                    var rel = CodeGraphPaths.ToProjectRelative(projectPath, file);
                    known.TryGetValue(rel, out var previousHash);
                    outcome.Results.Add(LoadOne(connection, projectPath, file, rel, previousHash));
                }

                // Script spariti dal disco: via anche dal DB.
                var onDiskRel = new HashSet<string>(onDisk.Select(f => CodeGraphPaths.ToProjectRelative(projectPath, f)), StringComparer.Ordinal);
                foreach (var gone in known.Keys.Where(k => !onDiskRel.Contains(k)).ToList())
                {
                    ct.ThrowIfCancellationRequested();
                    outcome.Results.Add(RemoveOne(connection, gone));
                }

                _logger?.LogInformation("[CodeGraph] '{Project}': {Loaded} caricati, {Skipped} invariati, {Removed} tolti, {Failed} falliti",
                    projectPath, outcome.Loaded, outcome.Skipped, outcome.Removed, outcome.Failed);
                return outcome;
            }
            finally { gate.Release(); }
        }

        public CodeScriptResult LoadScript(string projectPath, string scriptAbsolutePath)
        {
            var gate = GateFor(projectPath);
            gate.Wait();
            try
            {
                using var connection = _db.OpenConnection(projectPath);
                var rel = CodeGraphPaths.ToProjectRelative(projectPath, scriptAbsolutePath);
                ReadScriptStates(connection).TryGetValue(rel, out var previousHash);
                return LoadOne(connection, projectPath, Path.GetFullPath(scriptAbsolutePath), rel, previousHash);
            }
            finally { gate.Release(); }
        }

        public CodeScriptResult RemoveScript(string projectPath, string scriptAbsolutePath)
        {
            var gate = GateFor(projectPath);
            gate.Wait();
            try
            {
                using var connection = _db.OpenConnection(projectPath);
                return RemoveOne(connection, CodeGraphPaths.ToProjectRelative(projectPath, scriptAbsolutePath));
            }
            finally { gate.Release(); }
        }

        // ------------------------------------------------------------------ one script

        private CodeScriptResult LoadOne(SqliteConnection connection, string projectPath, string absolutePath, string rel, string previousHash)
        {
            var result = new CodeScriptResult { ScriptPath = rel, Kind = CodeGraphPaths.IsAstScript(rel) ? "ast" : "doc" };
            try
            {
                var content = File.ReadAllText(absolutePath, Encoding.UTF8);
                var hash = Sha256(content);
                if (string.Equals(hash, previousHash, StringComparison.OrdinalIgnoreCase))
                {
                    result.Skipped = true;
                    return result;
                }

                var script = CodeScriptParser.Parse(content, rel);
                var expectedKind = CodeGraphPaths.IsAstScript(rel) ? CodeScriptKind.Ast : CodeScriptKind.Doc;
                if (script.Header.Kind != expectedKind)
                    throw new CodeScriptFormatException(rel, 1, $"kind '{script.Header.Kind.ToString().ToLowerInvariant()}' ma il file si chiama *{(expectedKind == CodeScriptKind.Ast ? CodeGraphPaths.AstScriptSuffix : CodeGraphPaths.DocScriptSuffix)}");

                using var tx = connection.BeginTransaction();
                var touched = new HashSet<string>(StringComparer.Ordinal);
                if (script.Header.Kind == CodeScriptKind.Ast)
                {
                    DeleteAstRows(connection, tx, script.Header.Source, touched);
                    InsertAst(connection, tx, script, rel, touched);
                }
                else
                {
                    DeleteDocRows(connection, tx, rel, touched);
                    InsertDoc(connection, tx, script, rel, touched);
                }
                RefreshFts(connection, tx, touched);
                UpsertScriptState(connection, tx, rel, result.Kind, hash, script.Header.Schema, script.RecordCount);
                tx.Commit();

                result.Loaded = true;
                result.RecordCount = script.RecordCount;
                return result;
            }
            catch (CodeScriptFormatException ex)
            {
                result.Error = ex.Message;
                _logger?.LogWarning("[CodeGraph] script rifiutato: {Why}", ex.Message);
                return result;
            }
            catch (Exception ex)
            {
                result.Error = $"'{rel}': {ex.Message}";
                _logger?.LogError(ex, "[CodeGraph] caricamento fallito per {Script}", rel);
                return result;
            }
        }

        private CodeScriptResult RemoveOne(SqliteConnection connection, string rel)
        {
            var result = new CodeScriptResult { ScriptPath = rel, Kind = CodeGraphPaths.IsAstScript(rel) ? "ast" : "doc" };
            try
            {
                using var tx = connection.BeginTransaction();
                var touched = new HashSet<string>(StringComparer.Ordinal);
                if (CodeGraphPaths.IsAstScript(rel))
                {
                    var source = Scalar<string>(connection, tx, "SELECT Path FROM code_file WHERE ScriptPath = $p", ("$p", rel));
                    if (source != null) DeleteAstRows(connection, tx, source, touched);
                }
                else
                {
                    DeleteDocRows(connection, tx, rel, touched);
                }
                RefreshFts(connection, tx, touched);
                Exec(connection, tx, "DELETE FROM code_script_state WHERE ScriptPath = $p", ("$p", rel));
                tx.Commit();
                result.Removed = true;
                return result;
            }
            catch (Exception ex)
            {
                result.Error = $"'{rel}': {ex.Message}";
                _logger?.LogError(ex, "[CodeGraph] rimozione fallita per {Script}", rel);
                return result;
            }
        }

        // ------------------------------------------------------------------ ast

        private static void DeleteAstRows(SqliteConnection c, SqliteTransaction tx, string source, HashSet<string> touched)
        {
            foreach (var id in Column<string>(c, tx, "SELECT Id FROM code_symbol WHERE Path = $p", ("$p", source))) touched.Add(id);
            // Gli archi del file portano con sé i simboli che toccano: la loro riga FTS va rifatta comunque? No — la FTS
            // non dipende dagli archi. Basta rifare i simboli del file.
            Exec(c, tx, "DELETE FROM code_parameter WHERE SymbolId IN (SELECT Id FROM code_symbol WHERE Path = $p)", ("$p", source));
            Exec(c, tx, "DELETE FROM code_edge WHERE Path = $p", ("$p", source));
            Exec(c, tx, "DELETE FROM code_symbol WHERE Path = $p", ("$p", source));
            Exec(c, tx, "DELETE FROM code_file WHERE Path = $p", ("$p", source));
        }

        private static void InsertAst(SqliteConnection c, SqliteTransaction tx, CodeScript script, string scriptRel, HashSet<string> touched)
        {
            var h = script.Header;
            Exec(c, tx,
                "INSERT INTO code_file (Path, Language, ContentHash, Extractor, ExtractorVersion, ParsedAt, SymbolCount, ScriptPath) " +
                "VALUES ($path, $lang, $hash, $ext, $extv, $at, $n, $script)",
                ("$path", h.Source), ("$lang", h.Language), ("$hash", h.SourceHash), ("$ext", h.Extractor), ("$extv", h.ExtractorVersion),
                ("$at", h.GeneratedAt ?? DateTime.UtcNow.ToString("o")), ("$n", script.Symbols.Count(s => !s.IsExternal)), ("$script", scriptRel));

            using (var cmd = c.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText =
                    "INSERT OR REPLACE INTO code_symbol (Id, Kind, Name, QualifiedName, Path, LineStart, LineEnd, Signature, ReturnType, Visibility, IsStatic, IsAsync, ParentId, Language) " +
                    "VALUES ($id, $kind, $name, $qn, $path, $ls, $le, $sig, $ret, $vis, $st, $as, $parent, $lang)";
                var p = new Dictionary<string, SqliteParameter>();
                foreach (var name in new[] { "$id", "$kind", "$name", "$qn", "$path", "$ls", "$le", "$sig", "$ret", "$vis", "$st", "$as", "$parent", "$lang" })
                    p[name] = cmd.Parameters.Add(name, SqliteType.Text);
                p["$ls"].SqliteType = SqliteType.Integer; p["$le"].SqliteType = SqliteType.Integer;
                p["$st"].SqliteType = SqliteType.Integer; p["$as"].SqliteType = SqliteType.Integer;

                foreach (var s in script.Symbols)
                {
                    if (s.IsExternal)
                    {
                        // Condiviso fra gli script: entra solo se manca, non si tocca se c'è già.
                        Exec(c, tx, "INSERT OR IGNORE INTO code_symbol (Id, Kind, Name, QualifiedName, IsStatic, IsAsync, Language) VALUES ($id, 'external', $name, $qn, 0, 0, $lang)",
                            ("$id", s.Id), ("$name", s.Name), ("$qn", s.QualifiedName), ("$lang", h.Language));
                        touched.Add(s.Id);
                        continue;
                    }
                    p["$id"].Value = s.Id; p["$kind"].Value = s.Kind; p["$name"].Value = s.Name; p["$qn"].Value = s.QualifiedName;
                    p["$path"].Value = h.Source; p["$ls"].Value = (object)s.LineStart ?? DBNull.Value; p["$le"].Value = (object)s.LineEnd ?? DBNull.Value;
                    p["$sig"].Value = (object)s.Signature ?? DBNull.Value; p["$ret"].Value = (object)s.ReturnType ?? DBNull.Value;
                    p["$vis"].Value = (object)s.Visibility ?? DBNull.Value; p["$st"].Value = s.IsStatic ? 1 : 0; p["$as"].Value = s.IsAsync ? 1 : 0;
                    p["$parent"].Value = (object)s.ParentId ?? DBNull.Value; p["$lang"].Value = h.Language;
                    cmd.ExecuteNonQuery();
                    touched.Add(s.Id);
                }
            }

            foreach (var prm in script.Parameters)
            {
                Exec(c, tx,
                    "INSERT OR REPLACE INTO code_parameter (SymbolId, Position, Name, ParameterType, Direction, DefaultValue, IsNullable) " +
                    "VALUES ($sid, $pos, $name, $type, $dir, $def, $null)",
                    ("$sid", prm.SymbolId), ("$pos", prm.Position), ("$name", prm.Name), ("$type", prm.ParameterType),
                    ("$dir", prm.Direction), ("$def", prm.DefaultValue), ("$null", prm.IsNullable ? 1 : 0));
            }

            foreach (var e in script.Edges)
            {
                Exec(c, tx,
                    "INSERT INTO code_edge (SourceId, TargetId, Relation, Confidence, Path, Line) VALUES ($s, $t, $r, $c, $p, $l)",
                    ("$s", e.SourceId), ("$t", e.TargetId), ("$r", e.Relation), ("$c", e.Confidence), ("$p", h.Source), ("$l", e.Line));
            }
        }

        // ------------------------------------------------------------------ doc

        private static void DeleteDocRows(SqliteConnection c, SqliteTransaction tx, string scriptRel, HashSet<string> touched)
        {
            foreach (var id in Column<string>(c, tx, "SELECT SymbolId FROM code_symbol_doc WHERE ScriptPath = $p", ("$p", scriptRel))) touched.Add(id);
            foreach (var id in Column<string>(c, tx, "SELECT SymbolId FROM code_parameter_doc WHERE ScriptPath = $p", ("$p", scriptRel))) touched.Add(id);
            Exec(c, tx, "DELETE FROM code_symbol_doc WHERE ScriptPath = $p", ("$p", scriptRel));
            Exec(c, tx, "DELETE FROM code_parameter_doc WHERE ScriptPath = $p", ("$p", scriptRel));
        }

        private static void InsertDoc(SqliteConnection c, SqliteTransaction tx, CodeScript script, string scriptRel, HashSet<string> touched)
        {
            var h = script.Header;
            foreach (var d in script.SymbolDocs)
            {
                Exec(c, tx,
                    "INSERT OR REPLACE INTO code_symbol_doc (SymbolId, Summary, ReturnsDescription, Remarks, WrittenBy, WrittenAt, SignatureHash, ScriptPath) " +
                    "VALUES ($sid, $sum, $ret, $rem, $by, $at, $sh, $script)",
                    ("$sid", d.SymbolId), ("$sum", d.Summary), ("$ret", d.ReturnsDescription), ("$rem", d.Remarks),
                    ("$by", h.WrittenBy), ("$at", h.WrittenAt), ("$sh", d.SignatureHash), ("$script", scriptRel));
                touched.Add(d.SymbolId);
            }
            foreach (var d in script.ParameterDocs)
            {
                Exec(c, tx,
                    "INSERT OR REPLACE INTO code_parameter_doc (SymbolId, ParameterName, Description, ScriptPath) VALUES ($sid, $name, $desc, $script)",
                    ("$sid", d.SymbolId), ("$name", d.ParameterName), ("$desc", d.Description), ("$script", scriptRel));
                touched.Add(d.SymbolId);
            }
        }

        // ------------------------------------------------------------------ fts

        /// <summary>Rifà la riga FTS di ogni simbolo toccato: nome, nome qualificato, descrizione, descrizioni dei parametri.</summary>
        private static void RefreshFts(SqliteConnection c, SqliteTransaction tx, HashSet<string> symbolIds)
        {
            foreach (var id in symbolIds)
            {
                Exec(c, tx, "DELETE FROM code_symbol_fts WHERE symbol_id = $id", ("$id", id));
                Exec(c, tx,
                    "INSERT INTO code_symbol_fts (name, qualified_name, summary, parameter_descriptions, symbol_id) " +
                    "SELECT s.Name, s.QualifiedName, d.Summary, " +
                    "       (SELECT group_concat(pd.ParameterName || ': ' || pd.Description, ' | ') FROM code_parameter_doc pd WHERE pd.SymbolId = s.Id), " +
                    "       s.Id " +
                    "FROM code_symbol s LEFT JOIN code_symbol_doc d ON d.SymbolId = s.Id WHERE s.Id = $id",
                    ("$id", id));
            }
        }

        // ------------------------------------------------------------------ state

        private static Dictionary<string, string> ReadScriptStates(SqliteConnection c)
        {
            var map = new Dictionary<string, string>(StringComparer.Ordinal);
            using var cmd = c.CreateCommand();
            cmd.CommandText = "SELECT ScriptPath, ContentHash FROM code_script_state";
            using var r = cmd.ExecuteReader();
            while (r.Read()) map[r.GetString(0)] = r.GetString(1);
            return map;
        }

        private static void UpsertScriptState(SqliteConnection c, SqliteTransaction tx, string rel, string kind, string hash, int schema, int records)
            => Exec(c, tx,
                "INSERT OR REPLACE INTO code_script_state (ScriptPath, Kind, ContentHash, SchemaVersion, LoadedAt, RecordCount) VALUES ($p, $k, $h, $s, $at, $n)",
                ("$p", rel), ("$k", kind), ("$h", hash), ("$s", schema), ("$at", DateTime.UtcNow.ToString("o")), ("$n", records));

        // ------------------------------------------------------------------ sql helpers

        private static void Exec(SqliteConnection c, SqliteTransaction tx, string sql, params (string Name, object Value)[] args)
        {
            using var cmd = c.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = sql;
            foreach (var (name, value) in args) cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        private static T Scalar<T>(SqliteConnection c, SqliteTransaction tx, string sql, params (string Name, object Value)[] args) where T : class
        {
            using var cmd = c.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = sql;
            foreach (var (name, value) in args) cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
            var v = cmd.ExecuteScalar();
            return v is DBNull || v == null ? null : (T)v;
        }

        private static List<T> Column<T>(SqliteConnection c, SqliteTransaction tx, string sql, params (string Name, object Value)[] args)
        {
            using var cmd = c.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = sql;
            foreach (var (name, value) in args) cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
            var list = new List<T>();
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add((T)r.GetValue(0));
            return list;
        }

        private SemaphoreSlim GateFor(string projectPath) => _locks.GetOrAdd(Path.GetFullPath(projectPath), _ => new SemaphoreSlim(1, 1));

        public static string Sha256(string content)
        {
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(content))).ToLowerInvariant();
        }
    }
}
