using System;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    /// <summary>
    /// Redmine per progetto e per utente, nello UserDB (sprint 2026-10-27-Redmine, RM2): solo ciò che è personale, cioè
    /// l'accensione e la chiave API cifrata. Indirizzo, progetti e formato del testo stanno in .development.yml.
    /// </summary>
    public class ProjectRedmineSettings
    {
        public virtual Guid Id { get; set; }
        public virtual Project Project { get; set; }
        public virtual bool Enabled { get; set; } = false;

        /// <summary>La chiave API, cifrata con il protettore della piattaforma (DPAPI su Windows). Mai in chiaro.</summary>
        public virtual string ApiKeyEncrypted { get; set; }

        public virtual DateTime? LastTestedAt { get; set; }
        public virtual bool? LastTestSuccess { get; set; }
    }
}
