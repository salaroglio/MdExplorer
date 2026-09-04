using System;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    /// <summary>
    /// One row per <c>*.agent.md</c> run — manual, scheduled or hook-triggered.
    /// Written by the full Service (NHibernate) AND by the satellite scheduler
    /// (raw Microsoft.Data.Sqlite): ScheduleId is a plain nullable Guid column,
    /// not an NHibernate reference, so both writers share the same contract.
    /// </summary>
    public class AgentExecutionLog
    {
        public virtual Guid Id { get; set; }

        /// <summary>Null for manual runs launched from the dialog.</summary>
        public virtual Guid? ScheduleId { get; set; }

        public virtual string ProjectPath { get; set; }
        public virtual string AgentFilePath { get; set; }

        /// <summary>"manual" | "cron" | "commit" | "projectOpen".</summary>
        public virtual string TriggerSource { get; set; }

        /// <summary>"service" | "scheduler" — which process executed the run.</summary>
        public virtual string ExecutedBy { get; set; }

        public virtual DateTime StartedAt { get; set; }
        public virtual DateTime? FinishedAt { get; set; }

        /// <summary>"running" | "success" | "error" | "timeout" | "cancelled".</summary>
        public virtual string Status { get; set; }

        /// <summary>Tail of the agent's textual output (capped, for the history UI).</summary>
        public virtual string OutputSummary { get; set; }

        public virtual string Error { get; set; }
    }
}
