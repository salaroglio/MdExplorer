using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>Nella cartella scelta c'è già lo zip con lo stesso nome, ma con un contenuto diverso.</summary>
    public sealed class LanguagePackageExportExistsException : Exception
    {
        public string ZipPath { get; }
        public LanguagePackageExportExistsException(string zipPath, string message) : base(message) { ZipPath = zipPath; }
    }

    /// <summary>
    /// «Esporta linguaggio…» (10/11/2026), lo specchio di <see cref="LanguagePackageImport"/>: il pacchetto del progetto
    /// (<c>.mde-languages/&lt;id&gt;/</c>) in uno zip <c>mde-language-&lt;id&gt;-v&lt;versione&gt;.zip</c> che l'import accetta così
    /// com'è. Serve a riportare fuori un pacchetto cambiato nel progetto (da MarkAgent o a mano), per esempio dal cliente.
    /// <para>
    /// Il pacchetto si rilegge con lo stesso lettore rigido dell'import (<see cref="LanguagePackage.Load"/>): uno rotto non si
    /// esporta. Lo zip è sempre uguale per lo stesso contenuto (ordine fisso, data fissa): se nella cartella c'è già lo zip con
    /// quel nome e lo stesso contenuto non si riscrive; con un contenuto diverso non si sovrascrive mai, perché due pacchetti
    /// diversi con la stessa versione si confonderebbero (<see cref="LanguagePackageExportExistsException"/>): si alza
    /// <c>version</c> o si sceglie un'altra cartella.
    /// </para>
    /// </summary>
    public static class LanguagePackageExport
    {
        public sealed class Result
        {
            public string Id { get; init; }
            public string DisplayName { get; init; }
            public int Version { get; init; }
            public string ZipPath { get; init; }
            public int Files { get; init; }
            /// <summary>C'era già, identico: non si è riscritto niente.</summary>
            public bool Unchanged { get; init; }
        }

        private static readonly DateTimeOffset FixedTime = new(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);

        public static string ZipName(string id, int version) => $"mde-language-{id}-v{version}.zip";

        public static Result Export(string projectPath, string id, string destinationFolder)
        {
            if (string.IsNullOrWhiteSpace(projectPath) || !Directory.Exists(projectPath))
                throw new DirectoryNotFoundException(UiText.Say($"progetto non trovato: {projectPath}", $"project not found: {projectPath}"));
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException(UiText.Say("id è obbligatorio: il linguaggio da esportare", "id is required: the language to export"));
            if (string.IsNullOrWhiteSpace(destinationFolder) || !Directory.Exists(destinationFolder))
                throw new DirectoryNotFoundException(UiText.Say($"cartella di destinazione non trovata: {destinationFolder}", $"destination folder not found: {destinationFolder}"));
            var folder = Path.Combine(DeclarativeLanguage.FolderOf(projectPath), id);
            if (!Directory.Exists(folder))
                throw new DirectoryNotFoundException(UiText.Say(
                    $"il progetto non ha il pacchetto di linguaggio '{id}' ({DeclarativeLanguage.FolderName}/{id}/)",
                    $"the project has no language package '{id}' ({DeclarativeLanguage.FolderName}/{id}/)"));

            var package = LanguagePackage.Load(folder);
            var files = Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories)
                .Select(f => (Abs: f, Rel: Path.GetRelativePath(folder, f).Replace('\\', '/')))
                .Where(f => !f.Rel.Split('/').Any(p => p.StartsWith('.')))
                .OrderBy(f => f.Rel, StringComparer.Ordinal)
                .ToList();

            var bytes = Build(id, files.Select(f => (f.Rel, File.ReadAllBytes(f.Abs))));
            var zipPath = Path.Combine(destinationFolder, ZipName(id, package.Language.Version));
            var result = new Result
            {
                Id = id, DisplayName = package.Language.DisplayName, Version = package.Language.Version, ZipPath = zipPath, Files = files.Count,
            };
            if (File.Exists(zipPath))
            {
                if (File.ReadAllBytes(zipPath).AsSpan().SequenceEqual(bytes)) return new Result
                {
                    Id = result.Id, DisplayName = result.DisplayName, Version = result.Version, ZipPath = zipPath, Files = result.Files, Unchanged = true,
                };
                throw new LanguagePackageExportExistsException(zipPath, UiText.Say(
                    $"{zipPath} c'è già, con un contenuto diverso: è un'altra versione {package.Language.Version} del pacchetto. Alza «version» in {LanguagePackage.DefinitionFile} se il pacchetto è cambiato, oppure scegli un'altra cartella",
                    $"{zipPath} already exists with different content: it is another version {package.Language.Version} of the package. Raise «version» in {LanguagePackage.DefinitionFile} if the package changed, or choose another folder"));
            }
            // accanto e poi al suo posto: nella cartella non resta mai uno zip a metà
            var temp = zipPath + $".{Guid.NewGuid():N}.tmp";
            try
            {
                File.WriteAllBytes(temp, bytes);
                File.Move(temp, zipPath);
            }
            finally
            {
                if (File.Exists(temp)) File.Delete(temp);
            }
            return result;
        }

        /// <summary>Lo zip come lo legge l'import: il pacchetto in una sola cartella <c>&lt;id&gt;/</c>; stesso contenuto, stessi byte.</summary>
        private static byte[] Build(string id, IEnumerable<(string Rel, byte[] Content)> files)
        {
            using var ms = new MemoryStream();
            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true, entryNameEncoding: Encoding.UTF8))
                foreach (var (rel, content) in files)
                {
                    var entry = zip.CreateEntry(id + "/" + rel, CompressionLevel.Optimal);
                    entry.LastWriteTime = FixedTime;
                    using var s = entry.Open();
                    s.Write(content, 0, content.Length);
                }
            return ms.ToArray();
        }
    }
}
