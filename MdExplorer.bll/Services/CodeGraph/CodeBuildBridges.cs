using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>Una cartella <c>program</c> (o radice <c>@alias</c>) con la sua cartella su disco.</summary>
    public sealed record CodeProgramLocation(string Key, string FolderAbs);

    /// <summary>«<see cref="From"/> usa <see cref="To"/>» per il linguaggio dato, con la riga di build che lo dice.</summary>
    public sealed record CodeBridge(string From, string To, string Language, string Evidence);

    /// <summary>
    /// I ponti fra programmi letti dai file di build (punto 3b del 14/10/2026): lo sviluppatore li ha già scritti e li
    /// tiene aggiornati, ridichiararli in MdExplorer sarebbe un doppione che invecchia. Deterministico, niente LLM.
    /// Maven (<c>pom.xml</c>: dipendenze per groupId:artifactId), Gradle (<c>settings.gradle</c> + <c>project(':x')</c>
    /// nei <c>build.gradle</c>), .NET (<c>ProjectReference</c> nei <c>.csproj</c>). Mai un ponte per solo nome di classe:
    /// rifarebbe il calderone di prima del 3a. Quello che non si riesce a leggere finisce negli avvisi, non si salta zitto.
    /// </summary>
    public static class CodeBuildBridges
    {
        private static readonly HashSet<string> SkippedDirs = new(StringComparer.OrdinalIgnoreCase)
            { ".git", "node_modules", "target", "build", "bin", "obj", ".gradle", ".idea", ".mde-code", "out" };

        public static List<CodeBridge> Find(IReadOnlyList<CodeProgramLocation> programs, List<string> warnings)
        {
            var bridges = new List<CodeBridge>();
            var usable = programs.Where(p => p.FolderAbs != null && Directory.Exists(p.FolderAbs)).ToList();
            var files = usable.ToDictionary(p => p.Key, p => BuildFiles(p.FolderAbs).ToList());

            Maven(usable, files, bridges, warnings);
            Gradle(usable, files, bridges, warnings);
            DotNet(usable, files, bridges, warnings);

            return bridges.GroupBy(b => (b.From, b.To, b.Language)).Select(g => g.First()).ToList();
        }

        /// <summary>Il programma che contiene il percorso: la cartella più lunga che lo contiene.</summary>
        public static string OwnerOf(IReadOnlyList<CodeProgramLocation> programs, string path)
        {
            var full = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar);
            return programs
                .Where(p => p.FolderAbs != null)
                .Select(p => (p.Key, Dir: Path.GetFullPath(p.FolderAbs).TrimEnd(Path.DirectorySeparatorChar)))
                .Where(p => full.Equals(p.Dir, StringComparison.Ordinal) || full.StartsWith(p.Dir + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                .OrderByDescending(p => p.Dir.Length)
                .Select(p => p.Key)
                .FirstOrDefault();
        }

        private static IEnumerable<string> BuildFiles(string root)
        {
            var stack = new Stack<string>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var dir = stack.Pop();
                IEnumerable<string> entries;
                try { entries = Directory.EnumerateFileSystemEntries(dir); }
                catch (UnauthorizedAccessException) { continue; }
                foreach (var e in entries)
                {
                    var name = Path.GetFileName(e);
                    if (Directory.Exists(e))
                    {
                        if (!SkippedDirs.Contains(name) && !new DirectoryInfo(e).Attributes.HasFlag(FileAttributes.ReparsePoint)) stack.Push(e);
                        continue;
                    }
                    if (name == "pom.xml" || name.StartsWith("settings.gradle", StringComparison.Ordinal) || name.StartsWith("build.gradle", StringComparison.Ordinal)
                        || name.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                        yield return e;
                }
            }
        }

        // ------------------------------------------------------------------ Maven

        private static void Maven(List<CodeProgramLocation> programs, Dictionary<string, List<string>> files, List<CodeBridge> bridges, List<string> warnings)
        {
            var owners = new Dictionary<string, string>(StringComparer.Ordinal);   // groupId:artifactId → programma
            var poms = new List<(string Program, string Path, XElement Root, XNamespace Ns)>();
            foreach (var p in programs)
                foreach (var pom in files[p.Key].Where(f => Path.GetFileName(f) == "pom.xml"))
                {
                    XDocument doc;
                    try { doc = XDocument.Load(pom); }
                    catch (Exception ex) { warnings.Add($"{p.Key}: {pom} non si legge come XML ({ex.Message}): i suoi ponti Maven non ci sono"); continue; }
                    var ns = doc.Root.Name.Namespace;
                    var coords = Coordinates(doc.Root, ns);
                    if (coords == null) { warnings.Add($"{p.Key}: {pom} senza groupId/artifactId"); continue; }
                    if (owners.TryGetValue(coords, out var other) && other != p.Key)
                        warnings.Add($"{coords} è dichiarato sia in {other} sia in {p.Key}: vale il primo");
                    else owners[coords] = p.Key;
                    poms.Add((p.Key, pom, doc.Root, ns));
                }
            foreach (var (program, path, root, ns) in poms)
            {
                // solo <project><dependencies>: non la dependencyManagement (versioni) né i plugin
                foreach (var dep in root.Elements(ns + "dependencies").Elements(ns + "dependency"))
                {
                    var g = dep.Element(ns + "groupId")?.Value?.Trim();
                    var a = dep.Element(ns + "artifactId")?.Value?.Trim();
                    if (g == null || a == null) continue;
                    if (owners.TryGetValue(g + ":" + a, out var to) && to != program)
                        bridges.Add(new CodeBridge(program, to, CodeLanguages.Java, $"{Rel(path)}: dependency {g}:{a}"));
                }
            }
        }

        private static string Coordinates(XElement project, XNamespace ns)
        {
            var a = project.Element(ns + "artifactId")?.Value?.Trim();
            var g = project.Element(ns + "groupId")?.Value?.Trim() ?? project.Element(ns + "parent")?.Element(ns + "groupId")?.Value?.Trim();
            return a == null || g == null ? null : g + ":" + a;
        }

        // ------------------------------------------------------------------ Gradle

        private static readonly Regex IncludeLine = new(@"^\s*include\b(?<args>.*)$", RegexOptions.Multiline);
        private static readonly Regex Quoted = new(@"[""'](?<p>[^""']+)[""']");
        private static readonly Regex ProjectDir = new(@"project\(\s*[""'](?<p>:?[^""']+)[""']\s*\)\s*\.projectDir\s*=\s*(?:new\s+File\(\s*settingsDir\s*,\s*|file\(\s*)[""'](?<d>[^""']+)[""']");
        private static readonly Regex ProjectDep = new(@"project\(\s*(?:path\s*[:=]\s*)?[""'](?<p>:[^""']*)[""']");
        private static readonly Regex TypesafeDep = new(@"\bprojects\.(?<p>[A-Za-z0-9_.]+)");

        private static void Gradle(List<CodeProgramLocation> programs, Dictionary<string, List<string>> files, List<CodeBridge> bridges, List<string> warnings)
        {
            // il settings.gradle sta spesso SOPRA le cartelle program (radice del repository con i moduli come programmi):
            // si cerca anche risalendo, fino alla cartella che ha .git
            var settingsFiles = files.Values.SelectMany(f => f).Where(f => Path.GetFileName(f).StartsWith("settings.gradle", StringComparison.Ordinal))
                .Concat(programs.SelectMany(p => AncestorSettings(p.FolderAbs)))
                .Select(Path.GetFullPath).Distinct().ToList();
            foreach (var settings in settingsFiles)
            {
                var settingsDir = Path.GetDirectoryName(settings);
                var text = File.ReadAllText(settings);
                // ':a:b' → cartella; include senza projectDir = cartelle annidate come il percorso
                var projects = new Dictionary<string, string>(StringComparer.Ordinal) { [":"] = settingsDir };
                foreach (Match m in IncludeLine.Matches(text))
                    foreach (Match q in Quoted.Matches(m.Groups["args"].Value))
                    {
                        var gp = q.Groups["p"].Value.StartsWith(":") ? q.Groups["p"].Value : ":" + q.Groups["p"].Value;
                        projects[gp] = Path.Combine(settingsDir, gp.TrimStart(':').Replace(':', Path.DirectorySeparatorChar));
                    }
                foreach (Match m in ProjectDir.Matches(text))
                {
                    var gp = m.Groups["p"].Value.StartsWith(":") ? m.Groups["p"].Value : ":" + m.Groups["p"].Value;
                    projects[gp] = Path.GetFullPath(Path.Combine(settingsDir, m.Groups["d"].Value));
                }
                var byDir = projects.ToDictionary(kv => Path.GetFullPath(kv.Value).TrimEnd(Path.DirectorySeparatorChar), kv => kv.Key, StringComparer.Ordinal);

                foreach (var build in files.Values.SelectMany(f => f).Where(f => Path.GetFileName(f).StartsWith("build.gradle", StringComparison.Ordinal)).Distinct())
                {
                    var buildDir = Path.GetFullPath(Path.GetDirectoryName(build)).TrimEnd(Path.DirectorySeparatorChar);
                    if (!byDir.ContainsKey(buildDir)) continue;   // non è un progetto di questo settings
                    var from = OwnerOf(programs, buildDir);
                    if (from == null) continue;
                    var body = File.ReadAllText(build);
                    var targets = ProjectDep.Matches(body).Select(m => m.Groups["p"].Value).ToList();
                    foreach (Match t in TypesafeDep.Matches(body))
                    {
                        // accessori type-safe: projects.coreUtils → ':core-utils' (o ':coreUtils'), projects.a.b → ':a:b'
                        var segments = t.Groups["p"].Value.Split('.');
                        var candidates = new[] { ":" + string.Join(":", segments), ":" + string.Join(":", segments.Select(Kebab)) };
                        var hit = candidates.FirstOrDefault(projects.ContainsKey);
                        if (hit != null) targets.Add(hit);
                    }
                    foreach (var gp in targets.Distinct())
                    {
                        if (!projects.TryGetValue(gp, out var dir)) { warnings.Add($"{Rel(build)}: project('{gp}') non è incluso in {Rel(settings)}"); continue; }
                        var to = OwnerOf(programs, dir);
                        if (to != null && to != from) bridges.Add(new CodeBridge(from, to, CodeLanguages.Java, $"{Rel(build)}: project('{gp}')"));
                    }
                }
            }
        }

        private static IEnumerable<string> AncestorSettings(string folder)
        {
            var dir = Directory.GetParent(Path.GetFullPath(folder));
            while (dir != null)
            {
                foreach (var name in new[] { "settings.gradle", "settings.gradle.kts" })
                {
                    var f = Path.Combine(dir.FullName, name);
                    if (File.Exists(f)) yield return f;
                }
                if (Directory.Exists(Path.Combine(dir.FullName, ".git")) || File.Exists(Path.Combine(dir.FullName, ".git"))) yield break;
                dir = dir.Parent;
            }
        }

        private static string Kebab(string camel) => Regex.Replace(camel, "(?<=[a-z0-9])([A-Z])", "-$1").ToLowerInvariant();

        // ------------------------------------------------------------------ .NET

        private static void DotNet(List<CodeProgramLocation> programs, Dictionary<string, List<string>> files, List<CodeBridge> bridges, List<string> warnings)
        {
            foreach (var p in programs)
                foreach (var csproj in files[p.Key].Where(f => f.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)))
                {
                    XDocument doc;
                    try { doc = XDocument.Load(csproj); }
                    catch (Exception ex) { warnings.Add($"{p.Key}: {csproj} non si legge come XML ({ex.Message}): i suoi ponti .NET non ci sono"); continue; }
                    foreach (var reference in doc.Descendants().Where(e => e.Name.LocalName == "ProjectReference"))
                    {
                        var include = reference.Attribute("Include")?.Value;
                        if (string.IsNullOrWhiteSpace(include)) continue;
                        var target = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(csproj), include.Replace('\\', Path.DirectorySeparatorChar)));
                        var to = OwnerOf(programs, target);
                        if (to != null && to != p.Key) bridges.Add(new CodeBridge(p.Key, to, CodeLanguages.CSharp, $"{Rel(csproj)}: ProjectReference {include}"));
                    }
                }
        }

        private static string Rel(string path) => Path.GetFileName(Path.GetDirectoryName(path)) + "/" + Path.GetFileName(path);
    }
}
