using System;
using System.IO;
using System.Linq;

namespace MdExplorer.Features.Utilities
{
    /// <summary>
    /// Un nome libero come lo sceglie un file manager: il nome com'è, e solo se è preso <c>nome-2</c>, <c>nome-3</c>…
    /// Sostituisce la data attaccata al nome (<c>nome20261126143000.docx</c>), illeggibile, usata per non sovrascrivere
    /// (26/11/2026).
    /// </summary>
    public static class FreeFileName
    {
        /// <summary>Il percorso completo di un file libero in <paramref name="folder"/>: <c>nome.ext</c> o <c>nome-N.ext</c>.</summary>
        public static string For(string folder, string name, string extension)
            => Path.Combine(folder, First(name, n => !Taken(Path.Combine(folder, n + extension))) + extension);

        /// <summary>
        /// Il nome di base libero per un documento convertito in markdown e quello che si porta dietro: <c>nome.md</c>, i
        /// sotto-documenti degli allegati <c>nome.attachN….md</c>, le immagini in <c>assets/nome_img</c> e
        /// <c>assets/nome.attachN…_img</c>. Libero vuol dire che nessuno di questi c'è già, così il gruppo resta insieme.
        /// </summary>
        public static string ForConvertedDocument(string documentFolder, string baseName)
        {
            var assets = Path.Combine(documentFolder, "assets");
            return First(baseName, n =>
                !Taken(Path.Combine(documentFolder, n + ".md"))
                && !Entries(documentFolder).Any(e => e.StartsWith(n + ".attach", StringComparison.OrdinalIgnoreCase)
                                                     && e.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                && !Taken(Path.Combine(assets, n + "_img"))
                && !Entries(assets).Any(e => e.StartsWith(n + ".attach", StringComparison.OrdinalIgnoreCase)
                                            && e.EndsWith("_img", StringComparison.OrdinalIgnoreCase)));
        }

        private static string First(string name, Func<string, bool> free)
        {
            if (free(name)) return name;
            for (var i = 2; ; i++)
                if (free(name + "-" + i)) return name + "-" + i;
        }

        private static bool Taken(string path) => File.Exists(path) || Directory.Exists(path);

        private static string[] Entries(string folder)
            => Directory.Exists(folder) ? Directory.EnumerateFileSystemEntries(folder).Select(Path.GetFileName).ToArray() : Array.Empty<string>();
    }
}
