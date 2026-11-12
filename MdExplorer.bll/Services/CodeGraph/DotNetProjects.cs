using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Quello che un file di progetto .NET (<c>.csproj</c>, <c>.vbproj</c>) dice e che cambia come si leggono i sorgenti
    /// (11/11/2026): lo spazio dei nomi di radice e gli <c>Imports</c> di progetto del VB, il framework a cui punta, le
    /// librerie riferite con un percorso e i progetti riferiti. Si legge l'XML e basta: niente MSBuild, niente SDK.
    /// </summary>
    public sealed class DotNetProject
    {
        /// <summary>Il percorso assoluto del file di progetto.</summary>
        public string File { get; init; }
        public string Directory => Path.GetDirectoryName(File);
        /// <summary><c>RootNamespace</c>: in VB sta davanti a ogni tipo del progetto, anche senza <c>Namespace</c> nei file.</summary>
        public string RootNamespace { get; init; }
        /// <summary>Gli <c>&lt;Import Include="…"/&gt;</c> del VB: valgono per tutti i file del progetto.</summary>
        public IReadOnlyList<string> Imports { get; init; } = Array.Empty<string>();
        /// <summary><c>MyType</c> del VB (<c>WindowsForms</c>, <c>Web</c>, <c>Console</c>…): accende lo spazio dei nomi <c>My</c>.</summary>
        public string MyType { get; init; }
        public bool? OptionStrict { get; init; }
        public bool? OptionInfer { get; init; }
        public bool? OptionExplicit { get; init; }
        /// <summary>La versione di .NET Framework (<c>4.8</c>, <c>3.5</c>); null se il progetto non è .NET Framework.</summary>
        public string FrameworkVersion { get; init; }
        /// <summary>Le librerie riferite con un percorso (<c>HintPath</c>) che su questa macchina esistono.</summary>
        public IReadOnlyList<string> Libraries { get; init; } = Array.Empty<string>();
        /// <summary>Quelle con un percorso che qui non c'è: si dicono, perché ciò che usano resta sconosciuto.</summary>
        public IReadOnlyList<string> MissingLibraries { get; init; } = Array.Empty<string>();
        /// <summary>I file dei progetti riferiti (<c>ProjectReference</c>), assoluti.</summary>
        public IReadOnlyList<string> ProjectReferences { get; init; } = Array.Empty<string>();
        /// <summary>
        /// Visual Studio 2002/2003: l'identità del progetto (<c>ProjectGuid</c>) e i progetti riferiti per identità
        /// (<c>&lt;Reference Name="…" Project="{GUID}"/&gt;</c>), col nome per dirlo quando quel progetto non è fra i sorgenti.
        /// </summary>
        public string ProjectGuid { get; init; }
        public IReadOnlyList<(string Guid, string Name)> ProjectReferenceGuids { get; init; } = Array.Empty<(string, string)>();

        public bool IsNetFramework => FrameworkVersion != null;

        /// <summary>Un file di progetto letto; un XML che non si legge è un errore col file, mai un progetto vuoto.</summary>
        public static DotNetProject Load(string path)
        {
            XDocument doc;
            try { doc = XDocument.Load(path); }
            catch (Exception ex) when (ex is System.Xml.XmlException || ex is IOException || ex is UnauthorizedAccessException)
            {
                throw new InvalidDataException(UiText.Say($"{path}: il file di progetto non si legge ({ex.Message})", $"{path}: the project file cannot be read ({ex.Message})"), ex);
            }
            var dir = Path.GetDirectoryName(path);
            string Full(string relative) => Path.GetFullPath(Path.Combine(dir, relative.Replace('\\', Path.DirectorySeparatorChar)));
            if (doc.Root?.Name.LocalName == "VisualStudioProject") return LoadVisualStudio7(path, doc, Full);

            // le proprietà ripetute (una per configurazione) valgono la prima non vuota: quella senza condizione sta in testa
            string Property(string name) => doc.Descendants().Where(e => e.Name.LocalName == name && e.Parent?.Name.LocalName == "PropertyGroup")
                .Select(e => e.Value?.Trim()).FirstOrDefault(v => !string.IsNullOrEmpty(v));
            bool? Switch(string name) => Property(name) is { } v ? On(v) : null;

            var hints = doc.Descendants().Where(e => e.Name.LocalName == "HintPath").Select(e => e.Value?.Trim()).Where(v => !string.IsNullOrEmpty(v) && !v.Contains("$("))
                .Select(Full).Where(h => !IsFrameworkLibrary(h)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            return new DotNetProject
            {
                File = path,
                RootNamespace = Property("RootNamespace"),
                Imports = doc.Descendants().Where(e => e.Name.LocalName == "Import" && e.Parent?.Name.LocalName == "ItemGroup")
                    .Select(e => e.Attribute("Include")?.Value?.Trim()).Where(v => !string.IsNullOrEmpty(v)).Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
                MyType = Property("MyType"),
                OptionStrict = Switch("OptionStrict"), OptionInfer = Switch("OptionInfer"), OptionExplicit = Switch("OptionExplicit"),
                FrameworkVersion = NetFrameworkVersion(Property("TargetFrameworkVersion"), Property("TargetFramework") ?? Property("TargetFrameworks")),
                Libraries = hints.Where(System.IO.File.Exists).ToList(),
                MissingLibraries = hints.Where(h => !System.IO.File.Exists(h)).ToList(),
                ProjectReferences = doc.Descendants().Where(e => e.Name.LocalName == "ProjectReference").Select(e => e.Attribute("Include")?.Value?.Trim())
                    .Where(v => !string.IsNullOrEmpty(v) && !v.Contains("$(")).Select(Full).Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
            };
        }

        private static bool On(string v) => v.Equals("On", StringComparison.OrdinalIgnoreCase) || v.Equals("true", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Una libreria del framework stesso (<c>…\Microsoft.NET\Framework\v1.1.4322\System.dll</c>): non è una libreria del
        /// progetto, la dà la cartella del framework (<see cref="NetFrameworkLibraries"/>). Così un percorso di una volta che su
        /// questa macchina non c'è non si dice mancante.
        /// </summary>
        internal static bool IsFrameworkLibrary(string fullPath)
        {
            var parts = fullPath.Split('\\', '/');
            for (var i = 0; i + 1 < parts.Length; i++)
                if (parts[i].Equals("Microsoft.NET", StringComparison.OrdinalIgnoreCase)
                    && (parts[i + 1].Equals("Framework", StringComparison.OrdinalIgnoreCase) || parts[i + 1].Equals("Framework64", StringComparison.OrdinalIgnoreCase)))
                    return true;
            return false;
        }

        /// <summary>
        /// Il formato di Visual Studio .NET 2002/2003 (12/11/2026, rilevazione dal cliente: 20 riferimenti fra progetti in
        /// questa forma, che prima non si leggevano): <c>&lt;VisualStudioProject&gt;&lt;VisualBasic ProjectGuid="…"&gt;</c> (o
        /// <c>CSHARP</c>), le impostazioni come attributi di <c>Build/Settings</c>, i riferimenti in <c>Build/References</c>
        /// (<c>HintPath</c> per una libreria, <c>Project</c> per un altro progetto), gli <c>Imports</c> in
        /// <c>Build/Imports/Import Namespace</c>. <c>ProductVersion</c> 7.0 è .NET Framework 1.0, 7.10 è 1.1. Il VB di allora
        /// non ha <c>Option Infer</c>: spenta.
        /// </summary>
        private static DotNetProject LoadVisualStudio7(string path, XDocument doc, Func<string, string> full)
        {
            static bool Is(XElement e, string name) => e.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase);
            static string Attr(XElement e, string name)
                => e?.Attributes().FirstOrDefault(a => a.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value?.Trim() is { Length: > 0 } v ? v : null;
            var language = doc.Root.Elements().FirstOrDefault()
                ?? throw new InvalidDataException(UiText.Say($"{path}: VisualStudioProject senza l'elemento del linguaggio (VisualBasic, CSHARP)", $"{path}: VisualStudioProject without the language element (VisualBasic, CSHARP)"));
            var build = language.Elements().FirstOrDefault(e => Is(e, "Build"));
            var settings = build?.Elements().FirstOrDefault(e => Is(e, "Settings"));
            var references = build?.Elements().Where(e => Is(e, "References")).SelectMany(e => e.Elements().Where(r => Is(r, "Reference"))).ToList() ?? new List<XElement>();
            bool? Switch(string name) => Attr(settings, name) is { } v ? On(v) : null;

            var hints = references.Where(r => Attr(r, "Project") == null).Select(r => Attr(r, "HintPath")).Where(h => h != null)
                .Select(full).Where(h => !IsFrameworkLibrary(h)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var product = Attr(language, "ProductVersion");
            var framework = product == null ? null : product.StartsWith("7.0.", StringComparison.Ordinal) || product == "7.0" ? "1.0" : product.StartsWith("7.1", StringComparison.Ordinal) ? "1.1" : null;
            if (framework == null)
                throw new InvalidDataException(UiText.Say(
                    $"{path}: VisualStudioProject con ProductVersion '{product}': si leggono Visual Studio .NET 2002 (7.0) e 2003 (7.10)",
                    $"{path}: VisualStudioProject with ProductVersion '{product}': Visual Studio .NET 2002 (7.0) and 2003 (7.10) are read"));
            return new DotNetProject
            {
                File = path,
                RootNamespace = Attr(settings, "RootNamespace"),
                Imports = build?.Elements().Where(e => Is(e, "Imports")).SelectMany(e => e.Elements().Where(i => Is(i, "Import")))
                    .Select(i => Attr(i, "Namespace")).Where(v => v != null).Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? new List<string>(),
                OptionStrict = Switch("OptionStrict"), OptionExplicit = Switch("OptionExplicit"), OptionInfer = false,
                FrameworkVersion = framework,
                Libraries = hints.Where(System.IO.File.Exists).ToList(),
                MissingLibraries = hints.Where(h => !System.IO.File.Exists(h)).ToList(),
                ProjectGuid = NormalGuid(Attr(language, "ProjectGuid")),
                ProjectReferenceGuids = references.Where(r => Attr(r, "Project") != null)
                    .Select(r => (NormalGuid(Attr(r, "Project")), Attr(r, "Name") ?? Attr(r, "Project"))).Distinct().ToList(),
            };
        }

        /// <summary><c>{abc-…}</c> → <c>ABC-…</c>: lo stesso progetto scritto con o senza graffe, in maiuscolo o minuscolo.</summary>
        internal static string NormalGuid(string guid) => guid?.Trim().Trim('{', '}').ToUpperInvariant();

        /// <summary><c>v4.8</c> (progetti di una volta) o <c>net48</c>/<c>net472</c> (progetti SDK) → <c>4.8</c>, <c>4.7.2</c>; null per .NET (Core).</summary>
        internal static string NetFrameworkVersion(string targetFrameworkVersion, string targetFramework)
        {
            if (!string.IsNullOrWhiteSpace(targetFrameworkVersion))
            {
                var v = targetFrameworkVersion.Trim().TrimStart('v', 'V');
                return Version.TryParse(v, out _) ? v : null;
            }
            foreach (var tfm in (targetFramework ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                // net48, net472, net35: solo cifre dopo «net», senza punto (net8.0 è .NET, non .NET Framework)
                if (!tfm.StartsWith("net", StringComparison.OrdinalIgnoreCase)) continue;
                var digits = tfm.Substring(3);
                if (digits.Length is < 2 or > 3 || !digits.All(char.IsDigit)) continue;
                return string.Join(".", digits.Select(c => c.ToString()));
            }
            return null;
        }
    }

    /// <summary>I progetti a cui appartengono i sorgenti: per ogni file, il file di progetto più vicino risalendo le cartelle.</summary>
    public sealed class DotNetProjectMap
    {
        private readonly string _extension;
        private readonly string _stopAt;
        private readonly Dictionary<string, DotNetProject> _byDirectory = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DotNetProject> _byFile = new(StringComparer.OrdinalIgnoreCase);
        /// <summary>I file di progetto che non si sono letti, col motivo: chi estrae li dice.</summary>
        public List<string> Problems { get; } = new();

        /// <param name="extension"><c>.csproj</c> o <c>.vbproj</c>.</param>
        /// <param name="stopAt">Oltre questa cartella non si risale (la radice del progetto di documentazione o della radice di codice).</param>
        public DotNetProjectMap(string extension, string stopAt)
        {
            _extension = extension;
            _stopAt = string.IsNullOrEmpty(stopAt) ? null : Path.GetFullPath(stopAt).TrimEnd(Path.DirectorySeparatorChar);
        }

        /// <summary>Il progetto del sorgente, o null se sopra di lui non c'è nessun file di progetto.</summary>
        public DotNetProject Of(string sourceAbsolutePath)
        {
            var dir = Path.GetDirectoryName(Path.GetFullPath(sourceAbsolutePath));
            var visited = new List<string>();
            DotNetProject found = null;
            for (var depth = 0; dir != null && depth < 12; depth++)
            {
                if (_byDirectory.TryGetValue(dir, out found)) break;
                visited.Add(dir);
                string[] candidates;
                try { candidates = System.IO.Directory.Exists(dir) ? System.IO.Directory.GetFiles(dir, "*" + _extension) : Array.Empty<string>(); }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) { candidates = Array.Empty<string>(); }
                // GetFiles("*.vbproj") su Windows prende anche .vbproj.user: solo l'estensione esatta
                candidates = candidates.Where(c => c.EndsWith(_extension, StringComparison.OrdinalIgnoreCase)).OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToArray();
                if (candidates.Length > 0) { found = Load(candidates[0]); break; }
                if (_stopAt != null && string.Equals(dir.TrimEnd(Path.DirectorySeparatorChar), _stopAt, StringComparison.OrdinalIgnoreCase)) break;
                dir = Path.GetDirectoryName(dir);
            }
            foreach (var v in visited) _byDirectory[v] = found;
            return found;
        }

        /// <summary>Un progetto riferito (<c>ProjectReference</c>) già incontrato fra quelli dei sorgenti; null se è fuori dal perimetro.</summary>
        public DotNetProject Known(string projectFile) => _byFile.TryGetValue(projectFile, out var p) ? p : null;

        private DotNetProject Load(string file)
        {
            if (_byFile.TryGetValue(file, out var known)) return known;
            DotNetProject project = null;
            try { project = DotNetProject.Load(file); }
            catch (InvalidDataException ex) { Problems.Add(ex.Message); }
            _byFile[file] = project;
            return project;
        }
    }

    /// <summary>
    /// Le librerie di .NET Framework presenti su questa macchina, per risolvere <c>System.Web</c>, Windows Forms,
    /// <c>System.Data.OracleClient</c>… nei progetti che puntano lì. MdExplorer gira su .NET e si porta dietro solo le sue:
    /// senza queste, ciò che un progetto .NET Framework eredita o chiama dal framework resta un nome scritto.
    /// </summary>
    public static class NetFrameworkLibraries
    {
        private static readonly Dictionary<string, IReadOnlyList<string>> Cache = new(StringComparer.OrdinalIgnoreCase);
        private static readonly object Gate = new();

        /// <summary>Per le prove e per chi le ha altrove: una cartella con le librerie di riferimento, al posto di quelle cercate.</summary>
        public const string OverrideVariable = "MDE_NETFX_REFERENCES";

        /// <summary>La cartella da usare per quella versione, o null se su questa macchina non ce n'è nessuna.</summary>
        public static string FindDirectory(string version)
        {
            var forced = Environment.GetEnvironmentVariable(OverrideVariable);
            if (!string.IsNullOrWhiteSpace(forced)) return Directory.Exists(forced) ? forced : null;
            var wanted = Version.TryParse(version, out var w) ? w : new Version(4, 8);
            var candidates = new List<(Version Version, string Path)>();
            void AddVersioned(string root, string prefix, string suffix)
            {
                if (string.IsNullOrEmpty(root) || !Directory.Exists(root)) return;
                foreach (var d in Directory.GetDirectories(root, prefix + "*"))
                {
                    var name = Path.GetFileName(d).Substring(prefix.Length);
                    if (suffix.Length > 0) { if (!name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)) continue; name = name[..^suffix.Length]; }
                    if (Version.TryParse(name, out var v) && File.Exists(Path.Combine(d, "mscorlib.dll"))) candidates.Add((v, d));
                }
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Visual Studio / Build Tools / Developer Pack
                AddVersioned(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Reference Assemblies", "Microsoft", "Framework", ".NETFramework"), "v", "");
            }
            else
            {
                AddVersioned("/usr/lib/mono", "", "-api");
            }
            // il pacchetto NuGet delle librerie di riferimento, se qualcuno l'ha già scaricato
            var nuget = Environment.GetEnvironmentVariable("NUGET_PACKAGES") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
            if (Directory.Exists(nuget))
                foreach (var package in Directory.GetDirectories(nuget, "microsoft.netframework.referenceassemblies.net*"))
                    foreach (var versionDir in Directory.GetDirectories(package))
                        AddVersioned(Path.Combine(versionDir, "build", ".NETFramework"), "v", "");

            // la stessa serie (2.x-3.x oppure 4.x): la versione chiesta, o la più alta sotto mano
            var sameLine = candidates.Where(c => (c.Version.Major >= 4) == (wanted.Major >= 4)).ToList();
            var pool = sameLine.Count > 0 ? sameLine : candidates;
            var best = pool.OrderByDescending(c => c.Version == wanted).ThenByDescending(c => c.Version).Select(c => c.Path).FirstOrDefault();
            if (best != null) return best;

            // su Windows il framework stesso c'è sempre: le sue librerie valgono come riferimento
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                foreach (var bits in new[] { "Framework64", "Framework" })
                {
                    var runtime = Path.Combine(windows, "Microsoft.NET", bits, "v4.0.30319");
                    if (File.Exists(Path.Combine(runtime, "mscorlib.dll"))) return runtime;
                }
            }
            return null;
        }

        /// <summary>Le librerie gestite della cartella (e di <c>Facades</c>): si guardano una volta sola.</summary>
        public static IReadOnlyList<string> Assemblies(string directory)
        {
            lock (Gate)
            {
                if (Cache.TryGetValue(directory, out var known)) return known;
                var list = new List<string>();
                foreach (var dir in new[] { directory, Path.Combine(directory, "Facades") }.Where(Directory.Exists))
                    foreach (var dll in Directory.GetFiles(dir, "*.dll").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
                    {
                        // nella cartella del framework ci sono anche DLL native: non sono librerie da riferire
                        try { AssemblyName.GetAssemblyName(dll); list.Add(dll); }
                        catch (Exception ex) when (ex is BadImageFormatException || ex is IOException || ex is UnauthorizedAccessException) { }
                    }
                Cache[directory] = list;
                return list;
            }
        }
    }
}
