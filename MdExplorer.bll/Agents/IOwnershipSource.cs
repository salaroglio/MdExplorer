using System.Collections.Generic;

namespace MdExplorer.Features.Agents
{
    /// <summary>
    /// Il documento delle responsabilità del progetto, letto da chi sa dove sta (<c>agentCity.ownershipDoc</c> in
    /// <c>.development.yml</c>): serve anche a ciò che vive nella libreria, come il diagramma del workflow che disegna i ruoli.
    /// </summary>
    public interface IOwnershipSource
    {
        /// <summary>Le righe di <c>responsabilita.md</c>, o null se il progetto non ne ha (città spenta, documento assente o con errori).</summary>
        IReadOnlyList<OwnershipEntry> GetActiveOwnership(string projectPath);

        /// <summary>
        /// Tutte le righe dei ruoli, anche quelle che nessuno ha ancora preso (senza responsabile né email): ciò che serve
        /// per sapere quali ruoli esistono e di quali agenti sono, come nel diagramma del workflow. Null come sopra.
        /// </summary>
        IReadOnlyList<OwnershipEntry> GetRoleRows(string projectPath);
    }
}
