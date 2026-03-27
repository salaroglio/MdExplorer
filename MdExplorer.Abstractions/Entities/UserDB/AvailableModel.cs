using System;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    public class AvailableModel
    {
        public virtual Guid Id { get; set; }
        public virtual string ModelId { get; set; }
        public virtual string Name { get; set; }
        public virtual string Provider { get; set; }
        public virtual DateTime DiscoveredAt { get; set; }
    }
}
