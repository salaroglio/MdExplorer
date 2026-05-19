using System;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    public class ProjectNeo4jSettings
    {
        public virtual Guid Id { get; set; }
        public virtual Project Project { get; set; }
        public virtual bool Enabled { get; set; } = false;
        public virtual string Uri { get; set; } = "bolt://localhost:7687";
        public virtual string Database { get; set; } = "neo4j";
        public virtual string Username { get; set; } = "neo4j";
        public virtual string PasswordEncrypted { get; set; }
        public virtual bool SyncOnTocGeneration { get; set; } = true;
        public virtual bool SyncOnKgFileSave { get; set; } = true;
        public virtual DateTime? LastTestedAt { get; set; }
        public virtual bool? LastTestSuccess { get; set; }
    }
}
