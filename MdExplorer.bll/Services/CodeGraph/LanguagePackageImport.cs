using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>Un pacchetto con lo stesso id c'è già nel progetto: si sostituisce solo se chi importa lo conferma.</summary>
    public sealed class LanguagePackageExistsException : Exception
    {
        public string Id { get; }
        public int ExistingVersion { get; }
        public int ImportedVersion { get; }
        public LanguagePackageExistsException(string id, int existing, int imported, string message) : base(message)
        {
            Id = id; ExistingVersion = existing; ImportedVersion = imported;
        }
    }

    /// <summary>
    /// «Importa linguaggio…» (B6d): un pacchetto da una cartella o da uno <c>.zip</c> dentro <c>.mde-languages/&lt;id&gt;/</c> del
    /// progetto. Si legge TUTTO prima di copiare (regole, prompt, skill: <see cref="LanguagePackage.FromFiles"/>), e un
    /// pacchetto rotto non entra; nemmeno uno che riusa un'estensione di un altro linguaggio. Uno con lo stesso id si
    /// sostituisce solo con <c>replace</c> (<see cref="LanguagePackageExistsException"/> dice le due versioni). La copia
    /// passa da una cartella accanto e poi si sposta, così il catalogo non vede mai un pacchetto a metà.
    /// <para>
    /// Dove si sceglie: la cartella del pacchetto (con <c>language.yml</c>) o una che ne contiene uno solo (un repository
    /// con il pacchetto in una sottocartella); lo zip allo stesso modo. Nello zip solo <c>.yml</c> e <c>.md</c>, niente
    /// percorsi che escono dalla cartella, al più <see cref="MaxTotalBytes"/> scompattati.
    /// </para>
    /// </summary>
    public static class LanguagePackageImport
    {
        public const long MaxTotalBytes = 1024 * 1024;
        public const int MaxEntries = 200;

        public sealed class Result
        {
            public string Id { get; init; }
            public string DisplayName { get; init; }
            public int Version { get; init; }
            public string Folder { get; init; }
            public bool Replaced { get; init; }
            public IReadOnlyList<string> Skills { get; init; }
            public bool HasPrompt { get; init; }
        }

        public static Result Import(string projectPath, string sourcePath, bool replace)
        {
            if (string.IsNullOrWhiteSpace(projectPath) || !Directory.Exists(projectPath))
                throw new DirectoryNotFoundException(UiText.Say($"progetto non trovato: {projectPath}", $"project not found: {projectPath}"));
            var (id, display, files) = Read(sourcePath);
            // l'id è il nome della cartella del pacchetto: la si legge come se fosse già al suo posto
            var package = LanguagePackage.FromFiles(id, display, files);

            var root = DeclarativeLanguage.FolderOf(projectPath);
            var target = Path.Combine(root, id);
            var catalog = CodeLanguageCatalog.ForProject(projectPath);
            CheckClashes(package, catalog);
            var existing = catalog.ById(id)?.Package;
            var exists = Directory.Exists(target);
            if (exists && !replace)
            {
                var v = existing?.Language.Version ?? 0;
                throw new LanguagePackageExistsException(id, v, package.Language.Version, UiText.Say(
                    $"il progetto ha già il linguaggio '{id}'{(existing == null ? " (che oggi non si legge)" : $" versione {v}")}; il pacchetto da importare è la versione {package.Language.Version}",
                    $"the project already has the language '{id}'{(existing == null ? " (which cannot be read today)" : $" version {v}")}; the package to import is version {package.Language.Version}"));
            }

            Directory.CreateDirectory(root);
            // dentro .mde-languages (stesso disco: lo spostamento finale è un rename), col punto: il catalogo la salta
            var staging = Path.Combine(root, $".import-{Guid.NewGuid():N}");
            try
            {
                foreach (var (rel, text) in files)
                {
                    var path = Path.Combine(staging, id, rel.Replace('/', Path.DirectorySeparatorChar));
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    File.WriteAllText(path, text, new UTF8Encoding(false));
                }
                if (exists) Directory.Delete(target, recursive: true);
                Directory.Move(Path.Combine(staging, id), target);
            }
            finally
            {
                if (Directory.Exists(staging)) Directory.Delete(staging, recursive: true);
            }
            return new Result
            {
                Id = id, DisplayName = package.Language.DisplayName, Version = package.Language.Version, Folder = target, Replaced = exists,
                Skills = package.Skills.Select(s => s.Name).ToList(), HasPrompt = package.CodeDocPrompt != null,
            };
        }

        /// <summary>Come il catalogo: un id di MdExplorer o un'estensione di un altro linguaggio non entrano.</summary>
        private static void CheckClashes(LanguagePackage package, CodeLanguageCatalog catalog)
        {
            var def = package.Language;
            var path = Path.Combine(package.FolderPath, LanguagePackage.DefinitionFile);
            if (CodeLanguages.ById(def.Id) != null)
                throw new LanguageDefinitionException(path, UiText.Say($"id: '{def.Id}' è già un linguaggio di MdExplorer", $"id: '{def.Id}' is already a MdExplorer language"));
            foreach (var e in def.Extensions)
            {
                var other = catalog.ForFile("x" + e);
                if (other != null && !string.Equals(other.Id, def.Id, StringComparison.OrdinalIgnoreCase))
                    throw new LanguageDefinitionException(path, UiText.Say($"extensions: '{e}' è già di {other.DisplayName}", $"extensions: '{e}' already belongs to {other.DisplayName}"));
            }
        }

        /// <summary>I file del pacchetto (percorso relativo alla sua cartella → testo), il suo id e da dove viene.</summary>
        private static (string Id, string Display, Dictionary<string, string> Files) Read(string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
                throw new ArgumentException(UiText.Say("sourcePath è obbligatorio", "sourcePath is required"));
            if (Directory.Exists(sourcePath)) return ReadFolder(sourcePath);
            if (File.Exists(sourcePath) && sourcePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)) return ReadZip(sourcePath);
            throw new FileNotFoundException(UiText.Say($"{sourcePath}: scegli la cartella di un pacchetto di linguaggio o un file .zip", $"{sourcePath}: choose a language package folder or a .zip file"));
        }

        private static (string, string, Dictionary<string, string>) ReadFolder(string folder)
        {
            var package = File.Exists(Path.Combine(folder, LanguagePackage.DefinitionFile)) ? folder : OnlyChildWithRules(folder);
            var files = new Dictionary<string, string>(StringComparer.Ordinal);
            long total = 0;
            foreach (var file in Directory.EnumerateFiles(package, "*", SearchOption.AllDirectories))
            {
                var rel = Path.GetRelativePath(package, file).Replace('\\', '/');
                if (rel.Split('/').Any(p => p.StartsWith('.'))) continue;   // .git, .gitignore: del repository, non del pacchetto
                total += new FileInfo(file).Length;
                if (total > MaxTotalBytes || files.Count >= MaxEntries) throw TooBig(package);
                files[rel] = File.ReadAllText(file, Encoding.UTF8);
            }
            return (Path.GetFileName(package), package, files);
        }

        private static string OnlyChildWithRules(string folder)
        {
            var candidates = Directory.EnumerateDirectories(folder).Where(d => !Path.GetFileName(d).StartsWith('.') && File.Exists(Path.Combine(d, LanguagePackage.DefinitionFile))).OrderBy(d => d, StringComparer.Ordinal).ToList();
            if (candidates.Count == 1) return candidates[0];
            throw new LanguageDefinitionException(folder, candidates.Count == 0
                ? UiText.Say($"non è un pacchetto di linguaggio: manca {LanguagePackage.DefinitionFile} (qui o in una sottocartella)", $"not a language package: {LanguagePackage.DefinitionFile} is missing (here or in a subfolder)")
                : UiText.Say($"ci sono {candidates.Count} pacchetti ({string.Join(", ", candidates.Select(Path.GetFileName))}): scegli la cartella di uno", $"there are {candidates.Count} packages ({string.Join(", ", candidates.Select(Path.GetFileName))}): choose the folder of one"));
        }

        private static (string, string, Dictionary<string, string>) ReadZip(string zipPath)
        {
            var entries = new Dictionary<string, string>(StringComparer.Ordinal);
            using (var zip = ZipFile.OpenRead(zipPath))
            {
                long total = 0;
                foreach (var entry in zip.Entries)
                {
                    var name = entry.FullName.Replace('\\', '/');
                    if (name.EndsWith('/')) continue;   // una cartella
                    if (name.StartsWith('/') || Regex.IsMatch(name, @"^[A-Za-z]:") || name.Split('/').Any(p => p == ".."))
                        throw new LanguageDefinitionException(zipPath, UiText.Say($"'{name}': un percorso che esce dalla cartella del pacchetto", $"'{name}': a path that leaves the package folder"));
                    if (name.Split('/').Any(p => p.StartsWith('.')) || name.StartsWith("__MACOSX/", StringComparison.Ordinal)) continue;
                    if (!name.EndsWith(".yml", StringComparison.OrdinalIgnoreCase) && !name.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                        throw new LanguageDefinitionException(zipPath, UiText.Say($"'{name}': in un pacchetto di linguaggio ci sono solo file .yml e .md", $"'{name}': a language package holds only .yml and .md files"));
                    total += entry.Length;
                    if (total > MaxTotalBytes || entries.Count >= MaxEntries) throw TooBig(zipPath);
                    using var reader = new StreamReader(entry.Open(), Encoding.UTF8);
                    entries[name] = reader.ReadToEnd();
                }
            }
            // il pacchetto alla radice dello zip, o in una sola cartella
            if (entries.ContainsKey(LanguagePackage.DefinitionFile))
            {
                var id = IdOf(entries[LanguagePackage.DefinitionFile], zipPath);
                return (id, zipPath + "/" + id, entries);
            }
            var tops = entries.Keys.Where(k => k.Contains('/') && k.EndsWith("/" + LanguagePackage.DefinitionFile, StringComparison.Ordinal) && k.Count(c => c == '/') == 1).Select(k => k.Split('/')[0]).OrderBy(t => t, StringComparer.Ordinal).ToList();
            if (tops.Count != 1)
                throw new LanguageDefinitionException(zipPath, tops.Count == 0
                    ? UiText.Say($"non è un pacchetto di linguaggio: manca {LanguagePackage.DefinitionFile} (alla radice o in una cartella)", $"not a language package: {LanguagePackage.DefinitionFile} is missing (at the root or in one folder)")
                    : UiText.Say($"ci sono {tops.Count} pacchetti ({string.Join(", ", tops)}): uno zip ne porta uno", $"there are {tops.Count} packages ({string.Join(", ", tops)}): a zip carries one"));
            var top = tops[0];
            var files = entries.Where(e => e.Key.StartsWith(top + "/", StringComparison.Ordinal)).ToDictionary(e => e.Key[(top.Length + 1)..], e => e.Value, StringComparer.Ordinal);
            var outside = entries.Keys.FirstOrDefault(k => !k.StartsWith(top + "/", StringComparison.Ordinal) && !k.Equals("README.md", StringComparison.Ordinal));
            if (outside != null)
                throw new LanguageDefinitionException(zipPath, UiText.Say($"'{outside}': fuori dalla cartella del pacchetto '{top}/'", $"'{outside}': outside the package folder '{top}/'"));
            return (top, zipPath + "/" + top, files);
        }

        /// <summary>Lo zip col pacchetto alla radice non ha una cartella che gli dia il nome: lo dice <c>id</c> nelle regole.</summary>
        private static string IdOf(string yaml, string zipPath)
        {
            var m = Regex.Match(yaml, @"^id:\s*['""]?(?<id>[a-z][a-z0-9-]*)['""]?\s*$", RegexOptions.Multiline);
            if (!m.Success) throw new LanguageDefinitionException(zipPath + "/" + LanguagePackage.DefinitionFile, UiText.Say("id: obbligatorio", "id: required"));
            return m.Groups["id"].Value;
        }

        private static LanguageDefinitionException TooBig(string where)
            => new(where, UiText.Say($"più di {MaxTotalBytes / 1024} KB o {MaxEntries} file: un pacchetto ha regole e testi, non sorgenti", $"more than {MaxTotalBytes / 1024} KB or {MaxEntries} files: a package holds rules and texts, not sources"));
    }
}
