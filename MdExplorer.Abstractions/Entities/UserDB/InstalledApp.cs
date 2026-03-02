using System;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    public class InstalledApp
    {
        public virtual Guid Id { get; set; }
        public virtual string AppId { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual string Version { get; set; }
        public virtual string LocalPath { get; set; }
        public virtual string ExecutableName { get; set; }
        public virtual string DefaultArgsJson { get; set; }
        public virtual string Icon { get; set; }
        public virtual DateTime InstalledAt { get; set; }
        public virtual DateTime? UpdatedAt { get; set; }
        public virtual string Platform { get; set; }
    }
}
