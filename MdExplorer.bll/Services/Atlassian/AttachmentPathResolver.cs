using System;
using System.IO;

namespace MdExplorer.Features.Services.Atlassian
{
    /// <summary>
    /// Individua il file da allegare a una issue Jira.
    ///
    /// Nessun vincolo sulla posizione: qualunque percorso leggibile dalla macchina è
    /// ammesso (scelta esplicita del prodotto — allegare log di sistema o export
    /// generati fuori dal progetto è un caso d'uso legittimo). Restano i soli controlli
    /// di correttezza: percorso valido, file esistente. Un percorso relativo si legge
    /// rispetto alla root del progetto, che è la forma comoda per l'uso quotidiano.
    /// </summary>
    public static class AttachmentPathResolver
    {
        /// <summary>
        /// Risolve <paramref name="filePath"/> — assoluto, oppure relativo alla root di
        /// <paramref name="projectPath"/> — in un percorso completo esistente.
        /// Restituisce false con un messaggio azionabile quando il percorso è malformato
        /// o il file non c'è: nessun ripiego silenzioso.
        /// </summary>
        public static bool TryResolve(
            string projectPath, string filePath, out string resolved, out string error)
        {
            resolved = null;
            error = null;

            if (string.IsNullOrWhiteSpace(filePath))
            {
                error = "filePath is required.";
                return false;
            }

            var isAbsolute = Path.IsPathRooted(filePath);
            if (!isAbsolute && (string.IsNullOrWhiteSpace(projectPath) || !Directory.Exists(projectPath)))
            {
                error = "A relative filePath needs the project folder, which is not available " +
                        "on this machine. Give an absolute path instead.";
                return false;
            }

            string full;
            try
            {
                full = Path.GetFullPath(isAbsolute
                    ? filePath
                    : Path.Combine(Path.GetFullPath(projectPath), filePath));
            }
            catch (Exception ex)
            {
                error = $"Invalid filePath '{filePath}': {ex.Message}";
                return false;
            }

            if (Directory.Exists(full))
            {
                error = $"'{filePath}' is a folder, not a file. Attach a single file.";
                return false;
            }
            if (!File.Exists(full))
            {
                error = $"File not found: '{filePath}'.";
                return false;
            }

            resolved = full;
            return true;
        }
    }
}
