using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MdExplorer.Features.Utilities;

namespace MdExplorer.Features.Services.RootChanges
{
    /// <summary>
    /// Un file di una radice di codice che MarkAgent ha cambiato e che l'utente non ha ancora rivisto (sprint
    /// 2026-11-27-Modifiche-Da-Approvare-Sulle-Radici, modello «arretra» come VS Code): il file è già cambiato, qui c'è com'era
    /// prima che MarkAgent lo toccasse. La revisione confronta l'originale col file di adesso: «Tieni» sposta l'originale,
    /// «Annulla» riscrive nel file il pezzo originale.
    /// </summary>
    public sealed class RootChange
    {
        public string Alias { get; set; }
        /// <summary>Il percorso dentro la radice, con «/».</summary>
        public string Relative { get; set; }
        /// <summary>Il file c'era prima di MarkAgent: falso per un file che ha creato lui.</summary>
        public bool OriginalExists { get; set; }
        /// <summary>Il testo prima della prima scrittura di MarkAgent; null se il file non c'era.</summary>
        public string OriginalText { get; set; }
        /// <summary>La codifica del file (CP-850…): l'annullamento riscrive in quella.</summary>
        public string Encoding { get; set; }
        public bool Bom { get; set; }
        /// <summary>
        /// L'impronta del file dopo l'ultima scrittura di MarkAgent: se all'apertura della revisione è diversa, qualcuno
        /// (il collega in Eclipse) l'ha cambiato dopo, e la revisione lo dice.
        /// </summary>
        public string LastStamp { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// I file da rivedere, nella cartella dati di MDE: mai nella radice né nel repository del progetto. Un JSON per file in
    /// <c>&lt;hash del progetto&gt;/&lt;hash di alias+percorso&gt;.json</c>: niente nomi di file da tradurre e l'elenco del
    /// progetto è la cartella. Sopravvivono alla chiusura di MDE.
    /// </summary>
    public sealed class RootChangeStore
    {
        private static readonly JsonSerializerOptions Json = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
        };

        private readonly string _root;
        private readonly object _gate = new();

        public RootChangeStore(string root)
        {
            if (string.IsNullOrWhiteSpace(root)) throw new ArgumentException("La cartella delle modifiche da rivedere è obbligatoria.", nameof(root));
            _root = Path.GetFullPath(root);
        }

        private string ProjectFolder(string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath)) throw new ArgumentException("Il progetto è obbligatorio.", nameof(projectPath));
            // Come le chat (ChatFileStore): UTF-8, così due progetti con lettere accentate diverse non si scambiano.
            return Path.Combine(_root, Helper.HGetHashString(Path.GetFullPath(projectPath), System.Text.Encoding.UTF8));
        }

        private string FileOf(string projectPath, string alias, string relative)
        {
            var key = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(alias + "\n" + Normalize(relative)))).ToLowerInvariant();
            return Path.Combine(ProjectFolder(projectPath), key + ".json");
        }

        /// <summary>Il percorso dentro la radice con «/», senza «/» in testa né in coda.</summary>
        public static string Normalize(string relative) => (relative ?? string.Empty).Replace('\\', '/').Trim('/');

        public RootChange Get(string projectPath, string alias, string relative)
        {
            var file = FileOf(projectPath, alias, relative);
            lock (_gate)
                return File.Exists(file) ? JsonSerializer.Deserialize<RootChange>(File.ReadAllText(file), Json) : null;
        }

        public void Save(string projectPath, RootChange change)
        {
            change.Relative = Normalize(change.Relative);
            var file = FileOf(projectPath, change.Alias, change.Relative);
            lock (_gate)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(file));
                // Prima in un file a parte, poi al suo posto: l'originale non resta mai scritto a metà.
                var temp = file + ".tmp";
                File.WriteAllText(temp, JsonSerializer.Serialize(change, Json), new UTF8Encoding(false));
                File.Move(temp, file, overwrite: true);
            }
        }

        public void Remove(string projectPath, string alias, string relative)
        {
            var file = FileOf(projectPath, alias, relative);
            lock (_gate)
                if (File.Exists(file)) File.Delete(file);
        }

        /// <summary>I file da rivedere del progetto (di una radice sola, con <paramref name="alias"/>), dal più vecchio.</summary>
        public IReadOnlyList<RootChange> List(string projectPath, string alias = null)
        {
            var folder = ProjectFolder(projectPath);
            lock (_gate)
            {
                if (!Directory.Exists(folder)) return Array.Empty<RootChange>();
                return Directory.GetFiles(folder, "*.json")
                    .Select(f => JsonSerializer.Deserialize<RootChange>(File.ReadAllText(f), Json))
                    .Where(c => c != null && (alias == null || string.Equals(c.Alias, alias, StringComparison.Ordinal)))
                    .OrderBy(c => c.CreatedAt).ThenBy(c => c.Relative, StringComparer.Ordinal)
                    .ToList();
            }
        }
    }
}
