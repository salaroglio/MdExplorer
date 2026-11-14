using System;
using System.IO;
using System.Linq;

namespace MdExplorer.Features.Services.GitIgnore
{
    /// <summary>
    /// Le skill, gli agenti e i prompt che MDE installa per i motori AI (<c>.github/**/mde-*</c>, <c>.claude/**/mde-*</c>,
    /// <c>.opencode/**/mde-*</c>). MDE li esclude da git — nel <c>.gitignore</c> per il motore del progetto, in
    /// <c>.git/info/exclude</c> per quello personale — ma restano nell'albero (26/10/2026): sono ciò che l'AI usa, e chi non
    /// li vede non capisce che cosa sta succedendo. È l'eccezione alla regola «ciò che git ignora non si mostra» (W2).
    /// </summary>
    public static class MdeHarnessAssets
    {
        /// <summary>Le cartelle radice dei motori: le stesse di <c>HarnessLayout.All</c> nel servizio (un test le confronta).</summary>
        public static readonly string[] HarnessRoots = { ".github", ".claude", ".opencode" };

        private const string AssetPrefix = "mde-";

        /// <summary>Vero se il file o la cartella è un asset <c>mde-*</c> di un motore, o sta dentro uno di questi.</summary>
        public static bool IsInside(string fullPath, string projectPath)
        {
            if (string.IsNullOrEmpty(fullPath) || string.IsNullOrEmpty(projectPath)) return false;
            var relative = Path.GetRelativePath(projectPath, fullPath);
            if (relative.StartsWith("..", StringComparison.Ordinal) || Path.IsPathRooted(relative)) return false;

            var parts = relative.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 2
                && HarnessRoots.Contains(parts[0], StringComparer.OrdinalIgnoreCase)
                && parts.Skip(1).Any(p => p.StartsWith(AssetPrefix, StringComparison.OrdinalIgnoreCase));
        }
    }
}
