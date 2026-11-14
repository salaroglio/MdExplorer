using System;
using System.IO;

namespace MdExplorer.Features.Utilities
{
    /// <summary>
    /// «Questo percorso sta dentro quella radice?» in un posto solo. Il confronto per prefisso SENZA separatore finale
    /// faceva passare <c>/doc/proj2</c> per dentro <c>/doc/proj</c> (trovato il 12/10/2026 in CodeGraphController,
    /// MdExplorerController.TextFileToShow e PathValidator, sprint Radici-Di-Codice-Esterne F0). Serve anche alle radici
    /// di codice esterne: lo stesso controllo su più radici.
    /// </summary>
    public static class PathContainment
    {
        /// <summary>Maiuscole indifferenti dove lo è il file system di solito (Windows, macOS), esatte su Linux.</summary>
        public static StringComparison Comparison =>
            OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        /// <summary>
        /// Vero se <paramref name="candidate"/> (assoluto, o relativo a <paramref name="root"/>) è la radice stessa o sta
        /// sotto di essa, dopo aver risolto <c>..</c>. Una radice vuota non contiene niente.
        /// </summary>
        public static bool IsInside(string root, string candidate)
        {
            if (string.IsNullOrWhiteSpace(root) || string.IsNullOrWhiteSpace(candidate)) return false;
            var rootFull = Normalize(root);
            var full = Normalize(Path.IsPathRooted(candidate) ? candidate : Path.Combine(rootFull, candidate));
            return string.Equals(full, rootFull, Comparison)
                || full.StartsWith(rootFull + Path.DirectorySeparatorChar, Comparison);
        }

        /// <summary>Il percorso relativo a <paramref name="root"/> con <c>/</c>, o null se <paramref name="candidate"/> sta fuori.</summary>
        public static string RelativeOrNull(string root, string candidate)
        {
            if (!IsInside(root, candidate)) return null;
            var rootFull = Normalize(root);
            var full = Normalize(Path.IsPathRooted(candidate) ? candidate : Path.Combine(rootFull, candidate));
            return Path.GetRelativePath(rootFull, full).Replace('\\', '/');
        }

        private static string Normalize(string path)
        {
            var full = Path.GetFullPath(path);
            var trimmed = full.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            // la radice del disco («/» o «C:\») resta com'è: togliere il separatore la cambierebbe di significato
            return trimmed.Length == 0 || (trimmed.Length == 2 && trimmed[1] == ':') ? full : trimmed;
        }
    }
}
