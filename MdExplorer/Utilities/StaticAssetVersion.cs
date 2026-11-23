using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MdExplorer.Utilities
{
    /// <summary>
    /// La versione degli script e dei fogli di stile della pagina del documento (tutti i <c>.js</c> e <c>.css</c> sotto
    /// <c>wwwroot</c>, librerie comprese, tranne <c>client2</c>): un'impronta di nomi, date e dimensioni dei file. La pagina li chiede con
    /// <c>?v=&lt;versione&gt;</c> e il servizio risponde «tienili per sempre»: finché i file non cambiano il browser non
    /// li richiede più. Prima ogni documento aperto chiedeva conferma per un centinaio di file (misurato il 23/11/2026:
    /// 109 richieste a documento), e dove ogni richiesta è lenta l'apertura durava secondi.
    /// <para>Un aggiornamento cambia i file, quindi l'impronta, quindi gli indirizzi: non si possono mescolare versioni.
    /// In una build di sviluppo l'impronta si ricalcola (al più ogni due secondi), così uno script appena modificato
    /// arriva subito; in una build di rilascio si calcola una volta.</para>
    /// </summary>
    public static class StaticAssetVersion
    {
        public const string QueryName = "v";
        public const string ImmutableCacheControl = "public, max-age=31536000, immutable";

        private static readonly object Gate = new object();
        private static string _value;
        private static string _root;
        private static DateTime _computedAt;

        /// <summary>Dove stanno i file: lo dice l'avvio del servizio.</summary>
        public static void Use(string webRootPath)
        {
            lock (Gate) { _root = webRootPath; _value = null; }
        }

        public static string Current
        {
            get
            {
                lock (Gate)
                {
#if DEBUG
                    var stale = _value == null || DateTime.UtcNow - _computedAt > TimeSpan.FromSeconds(2);
#else
                    var stale = _value == null;
#endif
                    if (stale) { _value = Compute(_root); _computedAt = DateTime.UtcNow; }
                    return _value;
                }
            }
        }

        /// <summary>«/common.js» → «/common.js?v=…».</summary>
        public static string Url(string path) => path + (path.Contains('?') ? "&" : "?") + QueryName + "=" + Current;

        /// <summary>L'impronta dei file della pagina sotto <paramref name="webRootPath"/>; «0» se la cartella non c'è.</summary>
        public static string Compute(string webRootPath)
        {
            if (string.IsNullOrEmpty(webRootPath) || !Directory.Exists(webRootPath)) return "0";
            // Tutti gli script e gli stili sotto wwwroot (anche le librerie: bootstrap, mermaid…), tranne l'applicazione
            // Angular in client2, che ha già i nomi con l'impronta.
            var skip = Path.Combine(webRootPath, "client2") + Path.DirectorySeparatorChar;
            var files = Directory.EnumerateFiles(webRootPath, "*", SearchOption.AllDirectories)
                .Where(f => (f.EndsWith(".js", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
                            && !f.StartsWith(skip, StringComparison.OrdinalIgnoreCase));

            var text = new StringBuilder();
            foreach (var file in files.OrderBy(f => f, StringComparer.Ordinal))
            {
                var info = new FileInfo(file);
                text.Append(Path.GetRelativePath(webRootPath, file).Replace('\\', '/')).Append('|')
                    .Append(info.LastWriteTimeUtc.Ticks).Append('|').Append(info.Length).Append('\n');
            }
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text.ToString())), 0, 8).ToLowerInvariant();
        }
    }
}
