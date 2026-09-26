using System;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    /// <summary>
    /// Per-user schedule of a <c>*.agent.md</c> agent run. Lives in the global UserDB
    /// (never in the documentation project). Paths are stored as plain strings — no FK
    /// to Project — because the satellite scheduler (<c>MdExplorer.Scheduler</c>) reads
    /// this table with raw Microsoft.Data.Sqlite and must not need joins.
    /// </summary>
    public class AgentSchedule
    {
        public virtual Guid Id { get; set; }

        public virtual string ProjectPath { get; set; }
        public virtual string AgentFilePath { get; set; }

        /// <summary>Display name of the schedule.</summary>
        public virtual string Name { get; set; }

        /// <summary>Normalized prompt with parameter values already substituted.</summary>
        public virtual string PreparedPrompt { get; set; }

        /// <summary>"cron" | "commit" | "projectOpen".</summary>
        public virtual string TriggerType { get; set; }

        /// <summary>Standard 5-field cron expression; null unless TriggerType == "cron".</summary>
        public virtual string CronExpression { get; set; }

        public virtual bool Enabled { get; set; }

        /// <summary>
        /// User confirmed the agent may run unattended with full tool access.
        /// Server-side rule: a schedule can be Enabled only if Trusted.
        /// </summary>
        public virtual bool Trusted { get; set; }

        /// <summary>Why the schedule was auto-disabled (orphan paths, invalid cron, ...).</summary>
        public virtual string DisabledReason { get; set; }

        public virtual DateTime CreatedAt { get; set; }
        public virtual DateTime UpdatedAt { get; set; }

        public virtual DateTime? LastRunAt { get; set; }
        /// <summary>"success" | "error" | "timeout".</summary>
        public virtual string LastRunStatus { get; set; }
        public virtual string LastRunError { get; set; }
    }
}
