using System;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    public class KgIngestState
    {
        public virtual Guid Id { get; set; }
        public virtual Project Project { get; set; }
        public virtual string KgFilePath { get; set; }
        public virtual string ContentHash { get; set; }
        public virtual string GraphNamespace { get; set; }
        public virtual DateTime LastIngestedAt { get; set; }
        public virtual int NodeCount { get; set; }
        public virtual int EdgeCount { get; set; }
    }
}
