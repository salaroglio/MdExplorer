using System;
using System.IO;
using MdExplorer.Features.Configuration;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Utilities
{
    /// <summary>
    /// Deletes from a project's <c>.development.yml</c> the keys MdExplorer no longer has
    /// (<see cref="RetiredConfigKeys"/>), when the project is opened. The file is the team's and is in git:
    /// the change shows as one line removed, to commit as a cleanup, and the service's log says which keys.
    /// </summary>
    public static class DevelopmentConfigCleanup
    {
        public const string FileName = ".development.yml";

        /// <summary>True when the file was rewritten. A file that cannot be read or written is left alone and the reason is logged.</summary>
        public static bool CleanFile(string projectPath, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(projectPath)) return false;
            var path = Path.Combine(projectPath, FileName);
            if (!File.Exists(path)) return false;

            try
            {
                var text = File.ReadAllText(path);
                var cleaned = RetiredConfigKeys.Remove(text, out var removed);
                if (removed.Count == 0) return false;

                File.WriteAllText(path, cleaned);
                MdeRepoWriter.NotifyWritten(path, new MdeWhy("tolte le voci che MDE non usa più: " + string.Join(", ", removed),
                    "removed entries MDE no longer uses: " + string.Join(", ", removed)));
                logger?.LogWarning("[Config] '{File}': tolte o convertite le righe ritirate {Keys}. Il file risulta modificato: serve un commit.",
                    path, string.Join(", ", removed));
                return true;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "[Config] '{File}': pulizia delle righe ritirate non riuscita; il file resta com'è.", path);
                return false;
            }
        }
    }
}
