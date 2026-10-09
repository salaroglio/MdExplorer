using System;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    public class AvailableModel
    {
        public virtual Guid Id { get; set; }
        public virtual string ModelId { get; set; }
        public virtual string Name { get; set; }
        public virtual string Provider { get; set; }
        /// <summary>Descrizione data dal provider stesso (Claude Code la manda, Copilot no); può essere null.</summary>
        public virtual string Description { get; set; }
        public virtual DateTime DiscoveredAt { get; set; }
    }
}
