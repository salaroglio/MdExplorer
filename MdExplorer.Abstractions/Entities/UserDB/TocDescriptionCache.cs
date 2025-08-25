using System;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    public class TocDescriptionCache
    {
        public virtual int Id { get; set; }
        public virtual string FilePath { get; set; }
        public virtual string FileHash { get; set; }
        public virtual string Description { get; set; }
        public virtual DateTime GeneratedAt { get; set; }
        public virtual string ModelUsed { get; set; }
        public virtual int? ProjectId { get; set; }
    }
}