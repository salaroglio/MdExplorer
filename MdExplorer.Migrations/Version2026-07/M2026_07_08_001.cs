using System;
using FluentMigrator;

namespace MdExplorer.Migrations.Version202607
{
    /// <summary>
    /// Schedulable *.agent.md agents: per-user schedule definitions, execution log and
    /// launch-dialog prompt drafts. AgentSchedule/AgentExecutionLog are shared with the
    /// satellite scheduler (MdExplorer.Scheduler), which reads/writes them with raw
    /// Microsoft.Data.Sqlite — paths are plain strings, ScheduleId is a plain Guid
    /// column (no FK), so neither writer depends on NHibernate semantics.
    /// </summary>
    [Migration(20260708001, "Create AgentSchedule + AgentExecutionLog + AgentPromptDraft tables for schedulable .agent.md agents")]
    public class M2026_07_08_001 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("AgentSchedule").Exists())
            {
                Create.Table("AgentSchedule")
                    .WithColumn("Id").AsGuid().PrimaryKey()
                    .WithColumn("ProjectPath").AsString(int.MaxValue).NotNullable()
                    .WithColumn("AgentFilePath").AsString(int.MaxValue).NotNullable()
                    .WithColumn("Name").AsString(500).NotNullable()
                    .WithColumn("PreparedPrompt").AsString(int.MaxValue).NotNullable()
                    .WithColumn("TriggerType").AsString(50).NotNullable()
                    .WithColumn("CronExpression").AsString(200).Nullable()
                    .WithColumn("Enabled").AsBoolean().NotNullable().WithDefaultValue(false)
                    .WithColumn("Trusted").AsBoolean().NotNullable().WithDefaultValue(false)
                    .WithColumn("DisabledReason").AsString(int.MaxValue).Nullable()
                    .WithColumn("CreatedAt").AsDateTime().NotNullable()
                    .WithColumn("UpdatedAt").AsDateTime().NotNullable()
                    .WithColumn("LastRunAt").AsDateTime().Nullable()
                    .WithColumn("LastRunStatus").AsString(50).Nullable()
                    .WithColumn("LastRunError").AsString(int.MaxValue).Nullable();

                // The satellite scheduler polls "Enabled=1 AND Trusted=1 AND TriggerType='cron'".
                Create.Index("IX_AgentSchedule_Enabled_TriggerType")
                    .OnTable("AgentSchedule")
                    .OnColumn("Enabled").Ascending()
                    .OnColumn("TriggerType").Ascending();
            }

            if (!Schema.Table("AgentExecutionLog").Exists())
            {
                Create.Table("AgentExecutionLog")
                    .WithColumn("Id").AsGuid().PrimaryKey()
                    .WithColumn("ScheduleId").AsGuid().Nullable()
                    .WithColumn("ProjectPath").AsString(int.MaxValue).NotNullable()
                    .WithColumn("AgentFilePath").AsString(int.MaxValue).NotNullable()
                    .WithColumn("TriggerSource").AsString(50).NotNullable()
                    .WithColumn("ExecutedBy").AsString(50).NotNullable()
                    .WithColumn("StartedAt").AsDateTime().NotNullable()
                    .WithColumn("FinishedAt").AsDateTime().Nullable()
                    .WithColumn("Status").AsString(50).NotNullable()
                    .WithColumn("OutputSummary").AsString(int.MaxValue).Nullable()
                    .WithColumn("Error").AsString(int.MaxValue).Nullable();

                Create.Index("IX_AgentExecutionLog_StartedAt")
                    .OnTable("AgentExecutionLog")
                    .OnColumn("StartedAt").Descending();
            }

            if (!Schema.Table("AgentPromptDraft").Exists())
            {
                Create.Table("AgentPromptDraft")
                    .WithColumn("Id").AsGuid().PrimaryKey()
                    .WithColumn("ProjectPath").AsString(int.MaxValue).NotNullable()
                    .WithColumn("AgentFilePath").AsString(int.MaxValue).NotNullable()
                    .WithColumn("Prompt").AsString(int.MaxValue).NotNullable()
                    .WithColumn("ParameterValuesJson").AsString(int.MaxValue).Nullable()
                    .WithColumn("UpdatedAt").AsDateTime().NotNullable();

                // One draft per (project, agent file).
                Create.Index("UX_AgentPromptDraft_Project_AgentFile")
                    .OnTable("AgentPromptDraft")
                    .OnColumn("ProjectPath").Ascending()
                    .OnColumn("AgentFilePath").Ascending()
                    .WithOptions().Unique();
            }
        }

        public override void Down()
        {
            if (Schema.Table("AgentPromptDraft").Exists())
            {
                Delete.Table("AgentPromptDraft");
            }
            if (Schema.Table("AgentExecutionLog").Exists())
            {
                Delete.Table("AgentExecutionLog");
            }
            if (Schema.Table("AgentSchedule").Exists())
            {
                Delete.Table("AgentSchedule");
            }
        }
    }
}
