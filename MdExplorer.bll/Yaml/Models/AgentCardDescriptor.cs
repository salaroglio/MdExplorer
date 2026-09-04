using System.Collections.Generic;

namespace MdExplorer.Features.Yaml.Models
{
    /// <summary>
    /// Fonte di verità della Agent Card A2A: mappa il blocco <c>a2a:</c> del
    /// frontmatter di un <c>.agent.md</c> (§5 del design doc Agent-Harness-A2A).
    /// Chiavi in snake_case, coerenti con la <c>UnderscoredNamingConvention</c>
    /// già usata da <see cref="YamlDocumentDescriptorParser"/>.
    /// </summary>
    public class AgentCardDescriptor
    {
        /// <summary>Identità stabile, kebab-case, unica nel progetto (<c>a2a.name</c>).</summary>
        public string Name { get; set; }

        /// <summary>Ruolo umano-leggibile del cittadino (<c>a2a.role</c>).</summary>
        public string Role { get; set; }

        /// <summary>Le "skills" della Agent Card A2A.</summary>
        public IList<AgentCardSkill> Skills { get; set; } = new List<AgentCardSkill>();

        /// <summary>
        /// Whitelist dei mittenti ammessi (<c>a2a.accepts_messages_from</c>).
        /// <c>["*"]</c> = chiunque nel progetto.
        /// </summary>
        public IList<string> AcceptsMessagesFrom { get; set; } = new List<string>();

        /// <summary>
        /// Override del limite conversazione (<c>a2a.max_hops</c>).
        /// Null = default harness. Il cap (16) è applicato a valle, dal registry.
        /// </summary>
        public int? MaxHops { get; set; }
    }

    /// <summary>Una skill dichiarata nella Agent Card (<c>a2a.skills[]</c>).</summary>
    public class AgentCardSkill
    {
        public string Id { get; set; }
        public string Description { get; set; }
    }
}
