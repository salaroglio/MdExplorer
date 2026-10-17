using System;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    /// <summary>
    /// Accesso al <b>relay</b> della federazione (namespace <c>/mdfed</c>) per un singolo
    /// progetto. Specchio di <see cref="ProjectFusekiSettings"/> ma per il canale federato.
    /// <para>
    /// Perché per-progetto e perché in UserDB: l'indirizzo del relay può cambiare da progetto a
    /// progetto (un team può ospitarsi il proprio), mentre la <b>chiave</b> è una credenziale
    /// locale della macchina e NON deve finire in git — a differenza del room secret, che vive
    /// in <c>.development.yml</c> proprio per essere condiviso col team.
    /// </para>
    /// </summary>
    public class ProjectRelaySettings
    {
        public virtual Guid Id { get; set; }

        public virtual Project Project { get; set; }

        /// <summary>
        /// URL del relay per questo progetto (es. <c>wss://errantia.net/mdchat</c>). Vuoto/null
        /// ⇒ si ricade su <c>agentCity.relayUrl</c> del <c>.development.yml</c> e infine sul
        /// default globale.
        /// </summary>
        public virtual string RelayUrl { get; set; }

        /// <summary>
        /// API key del relay, cifrata (DPAPI/<c>IPasswordProtector</c>). Null ⇒ si ricade sulla
        /// chiave globale di <c>MdChat:ApiKey</c>, se configurata e non placeholder.
        /// </summary>
        public virtual string ApiKeyEncrypted { get; set; }

        public virtual DateTime? LastTestedAt { get; set; }

        public virtual bool? LastTestSuccess { get; set; }
    }
}
