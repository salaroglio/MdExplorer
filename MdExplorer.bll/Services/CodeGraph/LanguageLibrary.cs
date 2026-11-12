using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>Un pacchetto di linguaggio come lo mostrano la libreria e le impostazioni del progetto.</summary>
    public class LanguageLibraryEntry
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public int? Version { get; set; }
        public List<string> Extensions { get; set; } = new();
        /// <summary><c>database: true</c>: l'SQL del codice C#/VB si ricollega ai suoi simboli.</summary>
        public bool Database { get; set; }
        /// <summary>Un dialetto di pagine (<c>template:</c>), non un linguaggio a righe.</summary>
        public bool Pages { get; set; }
        public List<string> Skills { get; set; } = new();
        public int Findings { get; set; }
        /// <summary>Il pacchetto non si legge: il messaggio, con il file e il motivo.</summary>
        public string Error { get; set; }
    }

    /// <summary>Un pacchetto visto da un progetto: in libreria, attivo nel progetto, o tutti e due; e chi lo usa.</summary>
    public sealed class ProjectLanguageEntry : LanguageLibraryEntry
    {
        public bool InLibrary { get; set; }
        public int? LibraryVersion { get; set; }
        public bool Active { get; set; }
        public int? ProjectVersion { get; set; }
        /// <summary>Le cartelle del progetto che hanno scelto questo linguaggio: finché ce ne sono, non si disattiva.</summary>
        public List<string> UsedBy { get; set; } = new();
        /// <summary>La libreria ha una versione più nuova di quella attiva nel progetto.</summary>
        public bool UpdateAvailable => Active && InLibrary && LibraryVersion > ProjectVersion;
    }

    /// <summary>
    /// La libreria dei linguaggi del grafo (12/11/2026, utente: «i pacchetti dovrebbero essere registrati a livello di
    /// applicazione, e poi deployati dentro i singoli progetti se servono»). Una cartella dell'applicazione, ordinata come
    /// un progetto (<c>&lt;radice&gt;/.mde-languages/&lt;id&gt;/</c>): import, controlli, catalogo ed export sono gli stessi
    /// di un progetto. Attivare un pacchetto in un progetto = copiarlo in <c>.mde-languages/</c> del progetto, dove viaggia
    /// con Git; disattivarlo = toglierlo, solo se nessuna cartella del progetto lo usa.
    /// </summary>
    public static class LanguageLibrary
    {
        public const string FolderName = "language-library";

        public static List<LanguageLibraryEntry> List(string root) => Read(root).Values.OrderBy(e => e.DisplayName ?? e.Id, StringComparer.OrdinalIgnoreCase).ToList();

        /// <summary>Un pacchetto (cartella o .zip) nella libreria. Uno con lo stesso id c'è già: <see cref="LanguagePackageExistsException"/> finché non si conferma.</summary>
        public static LanguagePackageImport.Result Import(string root, string sourcePath, bool replace)
        {
            Directory.CreateDirectory(root);
            return LanguagePackageImport.Import(root, sourcePath, replace);
        }

        /// <summary>Via dalla libreria. I progetti che lo hanno attivo lo tengono: è una loro copia.</summary>
        public static bool Remove(string root, string id)
        {
            var folder = PackageFolder(root, id);
            if (!Directory.Exists(folder)) return false;
            Directory.Delete(folder, recursive: true);
            return true;
        }

        /// <summary>I pacchetti della libreria e quelli attivi nel progetto, insieme; <paramref name="usedBy"/> dà le cartelle che usano un linguaggio.</summary>
        public static List<ProjectLanguageEntry> ForProject(string root, string projectPath, Func<string, IEnumerable<string>> usedBy)
        {
            var library = Read(root);
            var project = Read(projectPath, projectSide: true);
            return library.Keys.Union(project.Keys, StringComparer.Ordinal).Select(id =>
            {
                library.TryGetValue(id, out var lib);
                project.TryGetValue(id, out var prj);
                var shown = prj ?? lib;
                return new ProjectLanguageEntry
                {
                    Id = id, DisplayName = shown.DisplayName, Version = shown.Version, Extensions = shown.Extensions, Database = shown.Database,
                    Pages = shown.Pages, Skills = shown.Skills, Findings = shown.Findings, Error = shown.Error,
                    InLibrary = lib != null, LibraryVersion = lib?.Version, Active = prj != null, ProjectVersion = prj?.Version,
                    UsedBy = (usedBy?.Invoke(id) ?? Enumerable.Empty<string>()).ToList(),
                };
            }).OrderBy(e => e.DisplayName ?? e.Id, StringComparer.OrdinalIgnoreCase).ToList();
        }

        /// <summary>Dalla libreria al progetto (anche per aggiornarlo: <paramref name="replace"/>).</summary>
        public static LanguagePackageImport.Result Activate(string root, string projectPath, string id, bool replace)
        {
            var folder = PackageFolder(root, id);
            if (!Directory.Exists(folder))
                throw new DirectoryNotFoundException(UiText.Say($"la libreria dei linguaggi non ha il pacchetto '{id}'", $"the language library has no package '{id}'"));
            return LanguagePackageImport.Import(projectPath, folder, replace);
        }

        /// <summary>Via dal progetto. Rifiutato finché una cartella lo usa: il grafo di quella cartella resterebbe senza linguaggio.</summary>
        public static void Deactivate(string projectPath, string id, IReadOnlyCollection<string> usedBy)
        {
            if (usedBy != null && usedBy.Count > 0)
                throw new InvalidOperationException(UiText.Say(
                    $"Il linguaggio '{id}' è scelto per {string.Join(", ", usedBy)}: toglilo prima da lì («Ambiente di sviluppo»).",
                    $"The language '{id}' is chosen for {string.Join(", ", usedBy)}: remove it there first («Development environment»)."));
            var folder = PackageFolder(projectPath, id);
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }

        /// <summary>Un pacchetto arrivato con il progetto (da Git, da un collega) entra nella libreria di questo computer.</summary>
        public static LanguagePackageImport.Result Adopt(string root, string projectPath, string id, bool replace)
        {
            var folder = PackageFolder(projectPath, id);
            if (!Directory.Exists(folder))
                throw new DirectoryNotFoundException(UiText.Say($"il progetto non ha il pacchetto '{id}'", $"the project has no package '{id}'"));
            return Import(root, folder, replace);
        }

        private static string PackageFolder(string root, string id)
        {
            if (string.IsNullOrWhiteSpace(id) || id.Contains('/') || id.Contains('\\') || id.StartsWith('.'))
                throw new ArgumentException(UiText.Say($"'{id}' non è un nome di pacchetto", $"'{id}' is not a package name"));
            return Path.Combine(DeclarativeLanguage.FolderOf(root), id);
        }

        /// <summary>I pacchetti di una cartella ordinata come un progetto: quelli che si leggono e quelli rotti, con il motivo.</summary>
        private static Dictionary<string, LanguageLibraryEntry> Read(string root, bool projectSide = false)
        {
            var entries = new Dictionary<string, LanguageLibraryEntry>(StringComparer.Ordinal);
            if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(DeclarativeLanguage.FolderOf(root))) return entries;
            var catalog = CodeLanguageCatalog.ForProject(root);
            foreach (var l in catalog.All.Where(l => l.Package != null))
            {
                var p = l.Package;
                entries[l.Id] = new LanguageLibraryEntry
                {
                    Id = l.Id, DisplayName = p.Language.DisplayName, Version = p.Language.Version, Extensions = p.Language.Extensions.ToList(),
                    Database = p.Language.Database, Pages = p.Language.Template != null, Skills = p.Skills.Select(s => s.Name).ToList(), Findings = p.Findings.Count,
                };
            }
            foreach (var (id, error) in catalog.Errors)
                if (!entries.ContainsKey(id)) entries[id] = new LanguageLibraryEntry { Id = id, DisplayName = id, Error = error.Message };
            return entries;
        }
    }
}
