using System;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    /// <summary>
    /// Pausa temporanea di un agente su QUESTA macchina (§12.5 coda differita, causa "user").
    /// A differenza della manutenzione (WIP condiviso via git in <c>.development.yml</c>), è una
    /// condizione <b>locale</b> del singolo utente/installazione: vive in UserDB, non viaggia col
    /// repo. La <b>presenza di una riga</b> (progetto+agente) significa "in pausa" → le richieste
    /// per quell'agente sono <c>deferred:user</c> (parcheggiate, non fallite) finché la riga esiste.
    /// </summary>
    public class AgentPause
    {
        /// <summary>PK GuidComb — mai pre-assegnare.</summary>
        public virtual Guid Id { get; set; }

        public virtual string ProjectPath { get; set; }

        /// <summary><c>a2a.name</c> dell'agente in pausa.</summary>
        public virtual string AgentName { get; set; }

        /// <summary>Nota opzionale sul perché (mostrata nella UI della coda).</summary>
        public virtual string Reason { get; set; }

        public virtual DateTime CreatedAt { get; set; }
    }
}
