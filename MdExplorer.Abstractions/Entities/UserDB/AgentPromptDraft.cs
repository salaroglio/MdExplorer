using System;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    /// <summary>
    /// Last working prompt of the Agent Launch dialog for one agent file — per user,
    /// in the global UserDB (never in the documentation project). One row per
    /// (ProjectPath, AgentFilePath); reopening the dialog restores prompt and
    /// parameter values.
    /// </summary>
    public class AgentPromptDraft
    {
        public virtual Guid Id { get; set; }

        public virtual string ProjectPath { get; set; }
        public virtual string AgentFilePath { get; set; }

        /// <summary>Normalized prompt as last seen in the dialog (placeholders intact).</summary>
        public virtual string Prompt { get; set; }

        /// <summary>JSON object: parameter name → chosen value.</summary>
        public virtual string ParameterValuesJson { get; set; }

        public virtual DateTime UpdatedAt { get; set; }
    }
}
