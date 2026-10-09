using System;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    /// <summary>
    /// Override dell'identità-padrone per un progetto (test della città degli agenti): questa
    /// istanza <b>agisce come</b> l'utente <see cref="Email"/> invece che con la git email reale.
    /// Ha effetto SOLO se la modalità test identità è abilitata (setting globale). Una riga per
    /// progetto = "agisci come Email"; nessuna riga = identità reale. Loopback + R12.
    /// </summary>
    public class ImpersonatedOwner
    {
        /// <summary>PK GuidComb — mai pre-assegnare.</summary>
        public virtual Guid Id { get; set; }

        /// <summary>Progetto per cui vale l'impersonazione.</summary>
        public virtual string ProjectPath { get; set; }

        /// <summary>Git email dell'utente-padrone impersonato (dev'essere un padrone dell'ownership doc).</summary>
        public virtual string Email { get; set; }

        public virtual DateTime CreatedAt { get; set; }
    }
}
