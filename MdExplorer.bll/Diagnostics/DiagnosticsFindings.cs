using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;

namespace MdExplorer.Features.Diagnostics
{
    /// <summary>Un file di rilevazione da allegare: il nome dentro lo zip e dove sta sul disco.</summary>
    public sealed record DiagnosticsFindingFile(string Name, string Path);

    /// <summary>
    /// Le rilevazioni di MarkAgent che «Invia diagnostica» può allegare ai log (11/11/2026, utente: «mi piace che sia
    /// "invia diagnostica" a mandarti quello che serve»). Chi mantiene MdExplorer non vede il codice del cliente: sul posto
    /// MarkAgent scrive che cosa ha trovato — forme e numeri, mai codice — e quei testi tornano con la diagnostica.
    /// <list type="bullet">
    /// <item><c>&lt;progetto&gt;/.mde-code/_findings/*.md</c>: le rilevazioni sul grafo del codice (linguaggi di MdExplorer).</item>
    /// <item><c>&lt;progetto&gt;/.mde-languages/&lt;id&gt;/</c>, per i pacchetti che hanno <c>findings/*.md</c>: il pacchetto
    /// intero, perché le regole corrette sul posto tornino insieme a ciò che le spiega.</item>
    /// </list>
    /// Nello zip i progetti hanno un numero, non il nome: il nome di un progetto è spesso quello del cliente.
    /// </summary>
    public static class DiagnosticsFindings
    {
        public const string ZipFolder = "rilevazioni";
        /// <summary>Come un file di pacchetto: sono testi, non sorgenti. Uno più grande non parte.</summary>
        public const long MaxFileBytes = LanguagePackage.MaxFileBytes;

        public static string CodeFindingsFolder(string projectPath)
            => System.IO.Path.Combine(CodeGraphPaths.GetScriptsRoot(projectPath), CodeGraphPaths.FindingsFolderName);

        public static IReadOnlyList<DiagnosticsFindingFile> Collect(IEnumerable<string> projectPaths)
        {
            var result = new List<DiagnosticsFindingFile>();
            var number = 0;
            foreach (var project in (projectPaths ?? Enumerable.Empty<string>()).Where(p => !string.IsNullOrWhiteSpace(p)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var mine = new List<(string Rel, string Path)>();
                try
                {
                    if (!Directory.Exists(project)) continue;
                    var code = CodeFindingsFolder(project);
                    if (Directory.Exists(code))
                        foreach (var f in Directory.EnumerateFiles(code, "*.md").OrderBy(f => f, StringComparer.Ordinal))
                            mine.Add(("codice/" + System.IO.Path.GetFileName(f), f));
                    foreach (var package in LanguagePackage.FoldersOf(project))
                    {
                        var findings = System.IO.Path.Combine(package, LanguagePackage.FindingsFolder);
                        if (!Directory.Exists(findings) || !Directory.EnumerateFiles(findings, "*.md").Any()) continue;
                        var id = System.IO.Path.GetFileName(package);
                        foreach (var f in Directory.EnumerateFiles(package, "*", SearchOption.AllDirectories).OrderBy(f => f, StringComparer.Ordinal))
                        {
                            var rel = System.IO.Path.GetRelativePath(package, f).Replace('\\', '/');
                            if (rel.Split('/').Any(p => p.StartsWith('.'))) continue;
                            if (!rel.EndsWith(".md", StringComparison.OrdinalIgnoreCase) && !rel.EndsWith(".yml", StringComparison.OrdinalIgnoreCase) && rel != LanguageTBox.FileName) continue;
                            mine.Add(($"linguaggi/{id}/{rel}", f));
                        }
                    }
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) { continue; }   // un progetto su un disco staccato non ferma la diagnostica
                mine = mine.Where(f => new FileInfo(f.Path).Length <= MaxFileBytes).ToList();
                if (mine.Count == 0) continue;
                number++;
                result.AddRange(mine.Select(f => new DiagnosticsFindingFile($"{ZipFolder}/progetto-{number}/{f.Rel}", f.Path)));
            }
            return result;
        }
    }
}
