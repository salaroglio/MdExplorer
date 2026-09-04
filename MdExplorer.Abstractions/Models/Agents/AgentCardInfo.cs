using System.Collections.Generic;

namespace MdExplorer.Abstractions.Models.Agents
{
    /// <summary>
    /// Agent Card di un cittadino della "città degli agenti" (§6 del design doc
    /// Agent-Harness-A2A), nella forma neutra rispetto alla sorgente.
    /// <para>
    /// È il gemello, lato <c>MdExplorer.Abstractions</c>, del blocco <c>a2a:</c>
    /// che per gli agenti LLM vive nel frontmatter (<c>AgentCardDescriptor</c> in
    /// <c>MdExplorer.bll</c>). Gli agenti algoritmici la producono in codice via
    /// <see cref="Services.IAlgorithmicAgent.GetCard"/>. Il registry (Fase 1, step 4)
    /// unifica le due sorgenti in un unico catalogo.
    /// </para>
    /// Abstractions non può dipendere da bll, perciò i due tipi restano distinti:
    /// espongono gli stessi campi ma appartengono a livelli diversi.
    /// </summary>
    public class AgentCardInfo
    {
        /// <summary>Identità stabile, kebab-case, unica nel progetto.</summary>
        public string Name { get; set; }

        /// <summary>Ruolo umano-leggibile del cittadino.</summary>
        public string Role { get; set; }

        /// <summary>Le "skills" della Agent Card.</summary>
        public IList<AgentCardSkillInfo> Skills { get; set; } = new List<AgentCardSkillInfo>();
    }

    /// <summary>Una skill dichiarata nella Agent Card di un agente algoritmico.</summary>
    public class AgentCardSkillInfo
    {
        public string Id { get; set; }
        public string Description { get; set; }
    }
}
