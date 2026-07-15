using System;
using System.Collections.Generic;

namespace MdExplorer.Features.Agents
{
    /// <summary>Skill di una Agent Card, forma neutra usata nel catalogo del registry.</summary>
    public class AgentRegistrySkill
    {
        public string Id { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// Una card scoperta da una delle due sorgenti (file <c>.agent.md</c> o
    /// <c>IAlgorithmicAgent</c>) PRIMA della riconciliazione. Input del
    /// <see cref="AgentRegistryReconciler"/>.
    /// <para>
    /// <see cref="ParseError"/> non-null = questa scoperta è già invalida a monte
    /// (blocco a2a: malformato, nome non valido, ...): entrerà nel catalogo come
    /// esclusa, con quel motivo — salvo che la regola dei duplicati non lo sovrascriva.
    /// </para>
    /// </summary>
    public class DiscoveredAgentCard
    {
        /// <summary>Nome dichiarato (può essere null se il parsing è fallito prima del nome).</summary>
        public string Name { get; set; }

        /// <summary>"llm" | "algorithmic" (<c>AgentIdentity.KindEnum</c>).</summary>
        public string Kind { get; set; }

        /// <summary>Path del <c>.agent.md</c>; null per gli agenti algoritmici.</summary>
        public string AgentFilePath { get; set; }

        public string Role { get; set; }
        public IList<AgentRegistrySkill> Skills { get; set; } = new List<AgentRegistrySkill>();

        /// <summary>Motivo di invalidità già noto a monte; null se la scoperta è pulita.</summary>
        public string ParseError { get; set; }

        /// <summary>
        /// Hash CORRENTE del blocco <c>a2a:</c> + <c>tools:</c> (R3). Confrontato con
        /// quello memorizzato al trust: se differisce, il trust decade.
        /// </summary>
        public string CurrentA2ABlockHash { get; set; }
    }

    /// <summary>
    /// Identità già persistita in <c>AgentIdentity</c>, in forma neutra: il reconciler
    /// non tocca NHibernate, riceve solo i dati che gli servono per il merge del trust.
    /// </summary>
    public class ExistingIdentity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool Trusted { get; set; }
        public bool Enabled { get; set; }
        public string A2ABlockHash { get; set; }
    }

    /// <summary>
    /// Una voce del catalogo del registry (le "Pagine Gialle" del progetto): la card
    /// scoperta più lo stato di trust persistito. Se <see cref="RegistrationError"/> è
    /// non-null la voce è ESCLUSA dalla cittadinanza attiva (ma resta visibile in UI,
    /// fail-loud).
    /// </summary>
    public class AgentRegistryEntry
    {
        public string Name { get; set; }
        public string Kind { get; set; }
        public string AgentFilePath { get; set; }
        public string Role { get; set; }
        public IList<AgentRegistrySkill> Skills { get; set; } = new List<AgentRegistrySkill>();

        /// <summary>Trust confermato dall'umano (dal record <c>AgentIdentity</c>).</summary>
        public bool Trusted { get; set; }

        /// <summary>Abilitato (vincolo Enabled ⇒ Trusted).</summary>
        public bool Enabled { get; set; }

        /// <summary>Hash del blocco a2a:+tools: MEMORIZZATO al momento del trust (R3).</summary>
        public string A2ABlockHash { get; set; }

        /// <summary>Hash CORRENTE del blocco a2a:+tools: (dalla sorgente, ora).</summary>
        public string CurrentA2ABlockHash { get; set; }

        /// <summary>
        /// True se il trust è DECADUTO in questa riconciliazione perché il blocco
        /// <c>a2a:</c>/<c>tools:</c> è cambiato dall'ultima conferma (R3): l'umano deve
        /// riconfermare. <see cref="Trusted"/> è già stato riportato a false.
        /// </summary>
        public bool TrustDecayed { get; set; }

        /// <summary>Motivo fail-loud dell'esclusione; null = cittadino valido.</summary>
        public string RegistrationError { get; set; }

        /// <summary>Id stabile dell'identità persistita; null finché non c'è una riga.</summary>
        public Guid? IdentityId { get; set; }

        public bool IsCitizen => string.IsNullOrEmpty(RegistrationError);
        public bool IsExcluded => !IsCitizen;
    }
}
