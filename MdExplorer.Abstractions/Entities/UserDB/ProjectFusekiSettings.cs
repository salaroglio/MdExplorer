using System;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    /// <summary>
    /// Configurazione di accesso a un'istanza Apache Jena Fuseki per un singolo
    /// progetto MDE. Specchio di <see cref="ProjectNeo4jSettings"/> ma per il
    /// triplestore RDF.
    /// </summary>
    public class ProjectFusekiSettings
    {
        public virtual Guid Id { get; set; }
        public virtual Project Project { get; set; }
        public virtual bool Enabled { get; set; } = false;

        /// <summary>Base URL del server Fuseki (es. http://localhost:3030).</summary>
        public virtual string Uri { get; set; } = "http://localhost:3030";

        /// <summary>Nome del dataset dedicato a questo progetto sul server Fuseki.
        /// Convenzione: 1 dataset MDE = 1 dataset Fuseki, nome = nome progetto sanitizzato.</summary>
        public virtual string Dataset { get; set; }

        /// <summary>Username Shiro (opzionale; se Fuseki gira senza auth, lascia vuoto).</summary>
        public virtual string Username { get; set; } = "";

        /// <summary>Password Shiro cifrata via DPAPI (opzionale).</summary>
        public virtual string PasswordEncrypted { get; set; }

        public virtual bool SyncOnTocGeneration { get; set; } = true;
        public virtual bool SyncOnKgFileSave { get; set; } = true;
        public virtual DateTime? LastTestedAt { get; set; }
        public virtual bool? LastTestSuccess { get; set; }
    }
}
