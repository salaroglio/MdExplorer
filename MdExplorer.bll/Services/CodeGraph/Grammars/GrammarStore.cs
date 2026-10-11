using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace MdExplorer.Features.Services.CodeGraph.Grammars
{
    /// <summary>Che cosa si sa di una grammatica: come si chiama il file, quale simbolo esporta, come si chiama l'add-on.</summary>
    public sealed class GrammarInfo
    {
        public string Id { get; init; }
        /// <summary>Il linguaggio del catalogo che la usa.</summary>
        public string LanguageId { get; init; }
        public string DisplayName { get; init; }
        /// <summary>Peso indicativo per la UI, in KB (quello del NuGet TreeSitter.DotNet 1.3.0).</summary>
        public int ApproxSizeKb { get; init; }
        public string Symbol => "tree_sitter_" + Id.Replace('-', '_');
        /// <summary>Il nome del file nativo nel NuGet e nello zip: <c>.dll</c> su Windows, <c>.dylib</c> su macOS, <c>.so</c> altrove.</summary>
        public string FileName(string rid) =>
            rid.StartsWith("win", StringComparison.OrdinalIgnoreCase) ? $"tree-sitter-{Id}.dll"
            : rid.StartsWith("osx", StringComparison.OrdinalIgnoreCase) ? $"libtree-sitter-{Id}.dylib"
            : $"libtree-sitter-{Id}.so";
        /// <summary>L'id dell'add-on su mdexplorer.net: <c>grammar-&lt;id&gt;-&lt;rid&gt;</c>.</summary>
        public string AddonId(string rid) => $"grammar-{Id}-{rid}";
    }

    /// <summary>Lo stato di una grammatica sul disco.</summary>
    public sealed class InstalledGrammar
    {
        public string Id { get; set; }
        public string Version { get; set; }
        public string Checksum { get; set; }
        public string InstalledAt { get; set; }
        public string Path { get; set; }
    }

    /// <summary>
    /// Il catalogo delle grammatiche tree-sitter (D8). Essere qui = si può scaricare; essere installata lo dice
    /// <see cref="GrammarStore"/>. Il core (<c>libtree-sitter</c>) non è una grammatica: viaggia col pacchetto.
    /// </summary>
    public static class GrammarCatalog
    {
        public static readonly IReadOnlyList<GrammarInfo> All = new[]
        {
            new GrammarInfo { Id = "typescript", LanguageId = CodeLanguages.TypeScript, DisplayName = "TypeScript", ApproxSizeKb = 1412 },
            new GrammarInfo { Id = "javascript", LanguageId = CodeLanguages.JavaScript, DisplayName = "JavaScript", ApproxSizeKb = 432 },
            new GrammarInfo { Id = "python", LanguageId = CodeLanguages.Python, DisplayName = "Python", ApproxSizeKb = 480 },
            new GrammarInfo { Id = "java", LanguageId = CodeLanguages.Java, DisplayName = "Java", ApproxSizeKb = 428 },
            new GrammarInfo { Id = "go", LanguageId = CodeLanguages.Go, DisplayName = "Go", ApproxSizeKb = 236 },
            new GrammarInfo { Id = "rust", LanguageId = CodeLanguages.Rust, DisplayName = "Rust", ApproxSizeKb = 1120 },
        };

        public static GrammarInfo ById(string id) => All.FirstOrDefault(g => string.Equals(g.Id, id, StringComparison.OrdinalIgnoreCase));
        public static GrammarInfo ForLanguage(string languageId) => All.FirstOrDefault(g => string.Equals(g.LanguageId, languageId, StringComparison.OrdinalIgnoreCase));

        public static GrammarInfo Require(string id)
            => ById(id) ?? throw new InvalidOperationException($"Grammatica '{id}' sconosciuta. Ammesse: {string.Join(", ", All.Select(g => g.Id))}");

        /// <summary><c>linux-x64</c>, <c>win-x64</c>, <c>osx-arm64</c>…: la cartella e il nome dell'add-on dipendono da qui.</summary>
        public static string CurrentRid()
        {
            var arch = RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return $"win-{arch}";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return $"linux-{arch}";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return $"osx-{arch}";
            throw new PlatformNotSupportedException("Piattaforma non supportata per le grammatiche tree-sitter.");
        }
    }

    /// <summary>
    /// Dove stanno le grammatiche scaricate: <c>&lt;appData&gt;/grammars/&lt;rid&gt;/</c>, un file per grammatica più un
    /// <c>&lt;id&gt;.json</c> con versione, checksum e data. Da qui si carica per percorso assoluto.
    /// </summary>
    public sealed class GrammarStore
    {
        private readonly string _root;
        public GrammarStore(string appDataPath)
        {
            if (string.IsNullOrWhiteSpace(appDataPath)) throw new ArgumentException("appDataPath is required", nameof(appDataPath));
            _root = Path.Combine(appDataPath, "grammars");
        }

        public string Rid { get; } = GrammarCatalog.CurrentRid();
        public string Folder => Path.Combine(_root, Rid);
        public string PathOf(GrammarInfo g) => Path.Combine(Folder, g.FileName(Rid));
        private string MetaPathOf(GrammarInfo g) => Path.Combine(Folder, g.Id + ".json");

        public bool IsInstalled(GrammarInfo g) => File.Exists(PathOf(g));

        public InstalledGrammar Installed(GrammarInfo g)
        {
            if (!IsInstalled(g)) return null;
            var meta = MetaPathOf(g);
            if (File.Exists(meta))
            {
                try { var m = JsonSerializer.Deserialize<InstalledGrammar>(File.ReadAllText(meta)); if (m != null) { m.Path = PathOf(g); return m; } }
                catch (JsonException) { /* metadati rotti: la grammatica c'è lo stesso */ }
            }
            return new InstalledGrammar { Id = g.Id, Path = PathOf(g) };
        }

        public IReadOnlyList<InstalledGrammar> InstalledAll() => GrammarCatalog.All.Select(Installed).Where(i => i != null).ToList();

        /// <summary>Mette il file al suo posto (sostituendo l'eventuale precedente) e scrive i metadati.</summary>
        public InstalledGrammar Install(GrammarInfo g, string downloadedFile, string version, string checksum)
        {
            Directory.CreateDirectory(Folder);
            var target = PathOf(g);
            File.Copy(downloadedFile, target, overwrite: true);
            var meta = new InstalledGrammar { Id = g.Id, Version = version, Checksum = checksum, InstalledAt = DateTime.UtcNow.ToString("o"), Path = target };
            File.WriteAllText(MetaPathOf(g), JsonSerializer.Serialize(new { meta.Id, meta.Version, meta.Checksum, meta.InstalledAt }));
            return meta;
        }

        public void Remove(GrammarInfo g)
        {
            if (File.Exists(PathOf(g))) File.Delete(PathOf(g));
            if (File.Exists(MetaPathOf(g))) File.Delete(MetaPathOf(g));
        }
    }
}
