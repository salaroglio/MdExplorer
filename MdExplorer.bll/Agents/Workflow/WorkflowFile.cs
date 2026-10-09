using System;
using System.Collections.Generic;
using System.IO;

namespace MdExplorer.Features.Agents.Workflow
{
    /// <summary>
    /// Un <c>*.workflow.json</c> del progetto, letto e verificato per intero: struttura e progetto insieme. È il punto da
    /// cui passano tutti (l'endpoint di verifica, il diagramma, le regole applicate), così il file si legge in un modo solo.
    /// </summary>
    public static class WorkflowFile
    {
        public const string Extension = ".workflow.json";

        /// <param name="projectRoot">La cartella del progetto.</param>
        /// <param name="path">Il file: dalla radice del progetto, o assoluto ma dentro il progetto.</param>
        /// <param name="agents">Gli agenti del progetto, dal registro.</param>
        public static WorkflowCheckResult Check(string projectRoot, string path, IEnumerable<AgentRegistryEntry> agents)
        {
            var full = Resolve(projectRoot, path);
            var result = WorkflowParser.Parse(File.ReadAllText(full));
            if (result.Descriptor != null)
            {
                var root = Path.GetFullPath(projectRoot);
                result.Issues.AddRange(WorkflowProjectCheck.Check(result.Descriptor, agents,
                    folder => Directory.Exists(Path.Combine(root, folder.Replace('/', Path.DirectorySeparatorChar)))));
            }
            return result;
        }

        /// <summary>Il percorso completo del file, solo se è un workflow dentro il progetto e c'è: altrimenti un errore che dice perché.</summary>
        public static string Resolve(string projectRoot, string path)
        {
            if (string.IsNullOrWhiteSpace(projectRoot) || !Directory.Exists(projectRoot))
                throw new ArgumentException($"La cartella del progetto '{projectRoot}' non esiste.");
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Manca il percorso del workflow.");

            var root = Path.GetFullPath(projectRoot).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var trimmed = path.Trim().Replace('\\', '/').TrimStart('/');
            var full = Path.GetFullPath(Path.IsPathRooted(path.Trim()) ? path.Trim() : Path.Combine(root, trimmed.Replace('/', Path.DirectorySeparatorChar)));
            var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            if (!full.StartsWith(root, comparison))
                throw new ArgumentException($"'{path}' è fuori dal progetto.");
            if (!full.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"'{path}' non è un workflow: il nome finisce con «{Extension}».");
            if (!File.Exists(full))
                throw new FileNotFoundException($"Il workflow '{path}' non esiste nel progetto.", full);
            return full;
        }
    }
}
