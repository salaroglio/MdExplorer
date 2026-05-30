using System;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    /// <summary>
    /// Per-project, per-user Atlassian credentials, stored in the global UserDB.
    /// Only the personal secret lives here (the API token, DPAPI-encrypted, plus
    /// the account email). The shared, non-secret config (base URL, project keys,
    /// planning folder) lives in .development.yml so it travels with the repo and
    /// the token never does.
    /// </summary>
    public class ProjectAtlassianSettings
    {
        public virtual Guid Id { get; set; }
        public virtual Project Project { get; set; }
        public virtual bool Enabled { get; set; } = false;

        /// <summary>Atlassian account email (used as the Basic-auth username).</summary>
        public virtual string Email { get; set; }

        /// <summary>API token, DPAPI-encrypted (CurrentUser scope). Never the plaintext.</summary>
        public virtual string ApiTokenEncrypted { get; set; }

        public virtual DateTime? LastTestedAt { get; set; }
        public virtual bool? LastTestSuccess { get; set; }
    }
}
