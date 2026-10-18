using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Scheduler;

public record ScheduleRow(
    Guid Id,
    string ProjectPath,
    string AgentFilePath,
    string Name,
    string PreparedPrompt,
    string CronExpression);

/// <summary>
/// Raw Microsoft.Data.Sqlite access to the per-user MdExplorer.db, shared with the
/// full Service (NHibernate). Interop contract, verified empirically (2026-07-08):
/// <list type="bullet">
/// <item>Guid columns are BLOBs in .NET <see cref="Guid.ToByteArray"/> order
///   (mixed-endian) — never write Guids as TEXT.</item>
/// <item>DateTime columns are TEXT "yyyy-MM-dd HH:mm:ss.fffffffZ" (UTC).</item>
/// <item>The DB runs WAL + busy_timeout 5000 on the Service side; we mirror the
///   pragmas, open a connection per operation, and keep write transactions
///   (BEGIN IMMEDIATE) down to 1-2 rows so the two writers never collide for long.</item>
/// </list>
/// </summary>
public class SchedulerDb
{
    private readonly ILogger<SchedulerDb> _logger;
    private readonly string _connectionString;

    public SchedulerDb(ILogger<SchedulerDb> logger)
    {
        _logger = logger;
        var dbPath = Path.Combine(GetDbDirectory(), "MdExplorer.db");
        if (!File.Exists(dbPath))
        {
            // Fail-loud: without the user DB there is nothing to schedule. The worker
            // logs and retries — the DB appears after the first MdExplorer start.
            _logger.LogWarning("[SchedulerDb] User DB not found at {Path} — will keep polling", dbPath);
        }
        _connectionString = $"Data Source={dbPath}";
    }

    /// <summary>Per-platform MdExplorer data dir (port of Electron project-discovery getDbPath).</summary>
    public static string GetDbDirectory()
    {
        if (OperatingSystem.IsWindows())
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MdExplorer");
        if (OperatingSystem.IsMacOS())
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Application Support", "MdExplorer");
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share", "MdExplorer");
    }

    private SqliteConnection Open()
    {
        var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var pragma = conn.CreateCommand();
        pragma.CommandText = "PRAGMA busy_timeout=5000; PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL;";
        pragma.ExecuteNonQuery();
        return conn;
    }

    /// <summary>
    /// Sanity check on the shared-table contract: if Guids are not stored the way we
    /// expect, refuse to execute rather than corrupt the table (fail-loud, no guessing).
    /// Returns true when the contract holds or the table is still empty.
    /// </summary>
    public bool VerifyGuidContract()
    {
        try
        {
            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT typeof(Id) FROM AgentSchedule LIMIT 1";
            var result = cmd.ExecuteScalar() as string;
            if (result == null) return true; // empty table — nothing to verify yet
            if (result == "blob") return true;
            _logger.LogError(
                "[SchedulerDb] AgentSchedule.Id is stored as '{Type}', expected 'blob' " +
                "(Guid.ToByteArray order). Refusing to execute schedules — the Service " +
                "and this satellite would disagree on row identity. Check the NHibernate " +
                "driver configuration.", result);
            return false;
        }
        catch (SqliteException ex)
        {
            _logger.LogWarning(ex, "[SchedulerDb] Contract check failed (DB missing or locked) — will retry");
            return false;
        }
    }

    public List<ScheduleRow> LoadCronSchedules()
    {
        var rows = new List<ScheduleRow>();
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT Id, ProjectPath, AgentFilePath, Name, PreparedPrompt, CronExpression
            FROM AgentSchedule
            WHERE Enabled = 1 AND Trusted = 1 AND TriggerType = 'cron'";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            rows.Add(new ScheduleRow(
                new Guid((byte[])reader[0]),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.IsDBNull(5) ? string.Empty : reader.GetString(5)));
        }
        return rows;
    }

    public Guid InsertRunningLog(ScheduleRow schedule)
    {
        var id = Guid.NewGuid();
        using var conn = Open();
        using var tx = conn.BeginTransaction();
        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            INSERT INTO AgentExecutionLog
                (Id, ScheduleId, ProjectPath, AgentFilePath, TriggerSource, ExecutedBy, StartedAt, Status)
            VALUES (@id, @scheduleId, @projectPath, @agentFilePath, 'cron', 'scheduler', @startedAt, 'running')";
        cmd.Parameters.AddWithValue("@id", id.ToByteArray());
        cmd.Parameters.AddWithValue("@scheduleId", schedule.Id.ToByteArray());
        cmd.Parameters.AddWithValue("@projectPath", schedule.ProjectPath);
        cmd.Parameters.AddWithValue("@agentFilePath", schedule.AgentFilePath);
        cmd.Parameters.AddWithValue("@startedAt", UtcNowText());
        cmd.ExecuteNonQuery();
        tx.Commit();
        return id;
    }

    public void CompleteLog(Guid logId, string status, string? outputSummary, string? error)
    {
        using var conn = Open();
        using var tx = conn.BeginTransaction();
        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            UPDATE AgentExecutionLog
            SET FinishedAt = @finishedAt, Status = @status, OutputSummary = @output, Error = @error
            WHERE Id = @id";
        cmd.Parameters.AddWithValue("@finishedAt", UtcNowText());
        cmd.Parameters.AddWithValue("@status", status);
        cmd.Parameters.AddWithValue("@output", (object?)outputSummary ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@error", (object?)error ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@id", logId.ToByteArray());
        cmd.ExecuteNonQuery();
        tx.Commit();
    }

    public void UpdateScheduleLastRun(Guid scheduleId, string status, string? error)
    {
        using var conn = Open();
        using var tx = conn.BeginTransaction();
        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            UPDATE AgentSchedule
            SET LastRunAt = @at, LastRunStatus = @status, LastRunError = @error, UpdatedAt = @at
            WHERE Id = @id";
        cmd.Parameters.AddWithValue("@at", UtcNowText());
        cmd.Parameters.AddWithValue("@status", status);
        cmd.Parameters.AddWithValue("@error", (object?)error ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@id", scheduleId.ToByteArray());
        cmd.ExecuteNonQuery();
        tx.Commit();
    }

    /// <summary>Fail-loud auto-disable (orphan paths, invalid cron): the schedule stops
    /// firing and the reason is visible in the UI.</summary>
    public void DisableSchedule(Guid scheduleId, string reason)
    {
        using var conn = Open();
        using var tx = conn.BeginTransaction();
        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            UPDATE AgentSchedule
            SET Enabled = 0, DisabledReason = @reason, UpdatedAt = @at
            WHERE Id = @id";
        cmd.Parameters.AddWithValue("@reason", reason);
        cmd.Parameters.AddWithValue("@at", UtcNowText());
        cmd.Parameters.AddWithValue("@id", scheduleId.ToByteArray());
        cmd.ExecuteNonQuery();
        tx.Commit();
    }

    /// <summary>Same TEXT format NHibernate/System.Data.SQLite writes: "yyyy-MM-dd HH:mm:ss.fffffffZ".</summary>
    private static string UtcNowText()
    {
        return DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffffff") + "Z";
    }
}
