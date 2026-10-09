using System;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>Lo stato dello scanner per una cartella <c>program</c> (D14): che commit ha analizzato e in che modo.</summary>
    public sealed class CodeScanState
    {
        public string ProgramFolder { get; set; }
        public string RepositoryRoot { get; set; }
        public string AnalyzedCommit { get; set; }
        /// <summary><c>git</c> oppure <c>hash</c> (cartella fuori da un repository: scansione completa, dichiarata).</summary>
        public string Mode { get; set; }
        public string ScannedAt { get; set; }
        public int Candidates { get; set; }
        public int Reparsed { get; set; }
    }

    public sealed class CodeScanStateStore
    {
        private readonly ICodeGraphDatabase _db;
        public CodeScanStateStore(ICodeGraphDatabase db) { _db = db; }

        public CodeScanState Get(string projectPath, string programFolder)
        {
            using var c = _db.OpenConnection(projectPath);
            using var cmd = c.CreateCommand();
            cmd.CommandText = "SELECT ProgramFolder, RepositoryRoot, AnalyzedCommit, Mode, ScannedAt, Candidates, Reparsed FROM code_scan_state WHERE ProgramFolder = $f";
            cmd.Parameters.AddWithValue("$f", programFolder);
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;
            return new CodeScanState
            {
                ProgramFolder = r.GetString(0), RepositoryRoot = r.IsDBNull(1) ? null : r.GetString(1),
                AnalyzedCommit = r.IsDBNull(2) ? null : r.GetString(2), Mode = r.GetString(3), ScannedAt = r.GetString(4),
                Candidates = r.GetInt32(5), Reparsed = r.GetInt32(6),
            };
        }

        public void Upsert(string projectPath, CodeScanState state)
        {
            using var c = _db.OpenConnection(projectPath);
            using var cmd = c.CreateCommand();
            cmd.CommandText =
                "INSERT OR REPLACE INTO code_scan_state (ProgramFolder, RepositoryRoot, AnalyzedCommit, Mode, ScannedAt, Candidates, Reparsed) " +
                "VALUES ($f, $root, $commit, $mode, $at, $cand, $rep)";
            cmd.Parameters.AddWithValue("$f", state.ProgramFolder);
            cmd.Parameters.AddWithValue("$root", (object)state.RepositoryRoot ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$commit", (object)state.AnalyzedCommit ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$mode", state.Mode);
            cmd.Parameters.AddWithValue("$at", state.ScannedAt ?? DateTime.UtcNow.ToString("o"));
            cmd.Parameters.AddWithValue("$cand", state.Candidates);
            cmd.Parameters.AddWithValue("$rep", state.Reparsed);
            cmd.ExecuteNonQuery();
        }

        public void Remove(string projectPath, string programFolder)
        {
            using var c = _db.OpenConnection(projectPath);
            using var cmd = c.CreateCommand();
            cmd.CommandText = "DELETE FROM code_scan_state WHERE ProgramFolder = $f";
            cmd.Parameters.AddWithValue("$f", programFolder);
            cmd.ExecuteNonQuery();
        }
    }
}
