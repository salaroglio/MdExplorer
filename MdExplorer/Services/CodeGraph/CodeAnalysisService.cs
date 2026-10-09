using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.Service.Models;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MdExplorer.Services.CodeGraph
{
    /// <summary>L'esito dell'analisi di una cartella <c>program</c>.</summary>
    public sealed class CodeFolderAnalysisOutcome
    {
        public string Folder { get; set; }
        public string Mode { get; set; }
        public string Reason { get; set; }
        public List<string> Languages { get; set; } = new();
        public int FilesInPerimeter { get; set; }
        public int Candidates { get; set; }
        public int Reparsed { get; set; }
        public int ScriptsRemoved { get; set; }
        /// <summary>File del perimetro di un linguaggio scelto che oggi non ha un estrattore: contati e dichiarati, non saltati.</summary>
        public Dictionary<string, int> FilesWithoutExtractor { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public CodeScriptLoadOutcome Load { get; set; }
        public string Error { get; set; }
    }

    public sealed class CodeFolderLanguages
    {
        public string Folder { get; set; }
        public bool IsProgramFolder { get; set; }
        public List<string> Selected { get; set; } = new();
        public IReadOnlyList<DetectedLanguage> Detected { get; set; } = Array.Empty<DetectedLanguage>();
        public string ScanMode { get; set; }
    }

    public interface ICodeAnalysisService
    {
        /// <summary>I linguaggi rilevati nella cartella (per estensione, dopo gli ignore) e quelli scelti in <c>.development.yml</c>.</summary>
        Task<CodeFolderLanguages> GetLanguagesAsync(string projectPath, string folderRelative, CancellationToken ct = default);
        /// <summary>Salva la scelta in <c>.development.yml</c> (voce della cartella, <c>codeGraph.languages</c>). Lista vuota = toglie la voce.</summary>
        void SaveLanguages(string projectPath, string folderRelative, IReadOnlyList<string> languages);
        /// <summary>Scanner → estrattori → script → caricatore, per una cartella.</summary>
        Task<CodeFolderAnalysisOutcome> AnalyzeFolderAsync(string projectPath, string folderRelative, CancellationToken ct = default);
        /// <summary>Tutte le cartelle <c>program</c> con dei linguaggi scelti.</summary>
        Task<List<CodeFolderAnalysisOutcome>> AnalyzeProjectAsync(string projectPath, CancellationToken ct = default);
    }

    /// <summary>
    /// Il passo «codice → script» della generazione della conoscenza (D9, D14, D16). Per una cartella <c>program</c>:
    /// legge i linguaggi scelti, chiede allo scanner che cosa è cambiato, conferma con l'hash nell'intestazione dello
    /// script esistente, fa emettere solo quei file, toglie gli script dei sorgenti spariti e infine fa caricare gli
    /// script nel DB. I file di un linguaggio scelto che non ha estrattore sono contati e dichiarati nell'esito.
    /// </summary>
    public sealed class CodeAnalysisService : ICodeAnalysisService
    {
        private readonly ICodeChangeScanner _scanner;
        private readonly ICodeExtractorRegistry _extractors;
        private readonly ICodeScriptLoader _loader;
        private readonly CodeScanStateStore _scanState;
        private readonly ILogger<CodeAnalysisService> _logger;
        private readonly SemaphoreSlim _gate = new(1, 1);

        public CodeAnalysisService(ICodeChangeScanner scanner, ICodeExtractorRegistry extractors, ICodeScriptLoader loader,
            CodeScanStateStore scanState, ILogger<CodeAnalysisService> logger)
        {
            _scanner = scanner; _extractors = extractors; _loader = loader; _scanState = scanState; _logger = logger;
        }

        // ------------------------------------------------------------------ .development.yml

        public async Task<CodeFolderLanguages> GetLanguagesAsync(string projectPath, string folderRelative, CancellationToken ct = default)
        {
            folderRelative = NormalizeRelative(folderRelative);
            var (config, entry) = ReadFolderEntry(projectPath, folderRelative);
            var folderAbs = Path.Combine(projectPath, folderRelative.Replace('/', Path.DirectorySeparatorChar));
            if (!Directory.Exists(folderAbs)) throw new DirectoryNotFoundException($"La cartella non esiste: {folderAbs}");

            var scan = await _scanner.ScanAsync(projectPath, folderAbs, previousCommit: null, ct);
            return new CodeFolderLanguages
            {
                Folder = folderRelative,
                IsProgramFolder = entry?.Tags?.Any(t => string.Equals(t, "program", StringComparison.OrdinalIgnoreCase)) == true,
                Selected = entry?.CodeGraph?.Languages?.ToList() ?? new List<string>(),
                Detected = CodeLanguageDetector.Detect(scan.AllFiles.Select(f => f.AbsolutePath), _extractors),
                ScanMode = scan.Mode,
            };
        }

        public void SaveLanguages(string projectPath, string folderRelative, IReadOnlyList<string> languages)
        {
            folderRelative = NormalizeRelative(folderRelative);
            foreach (var l in languages) CodeLanguages.Require(l);   // un id ignoto è un errore, non una riga ignorata

            var path = Path.Combine(projectPath, MdExplorer.Utilities.DevelopmentConfigCleanup.FileName);
            var config = File.Exists(path) ? MdExplorer.Utilities.DevelopmentConfigCleanup.ReadStrict(path) : new DevelopmentConfig();
            var entry = config.Folders.FirstOrDefault(f => string.Equals(NormalizeRelative(f.Path), folderRelative, StringComparison.OrdinalIgnoreCase));
            if (entry == null)
            {
                if (languages.Count == 0) return;
                entry = new DevelopmentFolder { Path = folderRelative, Tags = new List<string> { "program" } };
                config.Folders.Add(entry);
            }
            entry.CodeGraph = languages.Count == 0 ? null : new FolderCodeGraphConfig { Languages = languages.Distinct(StringComparer.OrdinalIgnoreCase).ToList() };

            var serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
            File.WriteAllText(path, serializer.Serialize(config));
            _logger.LogInformation("[CodeGraph] '{Folder}': linguaggi = {Langs}", folderRelative, languages.Count == 0 ? "(nessuno)" : string.Join(", ", languages));
        }

        // ------------------------------------------------------------------ analisi

        public async Task<List<CodeFolderAnalysisOutcome>> AnalyzeProjectAsync(string projectPath, CancellationToken ct = default)
        {
            var outcomes = new List<CodeFolderAnalysisOutcome>();
            var path = Path.Combine(projectPath, MdExplorer.Utilities.DevelopmentConfigCleanup.FileName);
            if (!File.Exists(path)) return outcomes;
            var config = MdExplorer.Utilities.DevelopmentConfigCleanup.ReadStrict(path);
            foreach (var folder in config.Folders.Where(f => f.CodeGraph?.Languages?.Count > 0 &&
                                                              f.Tags?.Any(t => string.Equals(t, "program", StringComparison.OrdinalIgnoreCase)) == true))
            {
                ct.ThrowIfCancellationRequested();
                outcomes.Add(await AnalyzeFolderAsync(projectPath, folder.Path, ct));
            }
            return outcomes;
        }

        public async Task<CodeFolderAnalysisOutcome> AnalyzeFolderAsync(string projectPath, string folderRelative, CancellationToken ct = default)
        {
            folderRelative = NormalizeRelative(folderRelative);
            var outcome = new CodeFolderAnalysisOutcome { Folder = folderRelative };
            await _gate.WaitAsync(ct);
            try
            {
                var (_, entry) = ReadFolderEntry(projectPath, folderRelative);
                var languages = entry?.CodeGraph?.Languages ?? new List<string>();
                outcome.Languages = languages.ToList();
                if (languages.Count == 0)
                {
                    outcome.Error = $"'{folderRelative}': nessun linguaggio scelto in .development.yml (codeGraph.languages). Apri «Ambiente di sviluppo» e scegli il linguaggio.";
                    return outcome;
                }
                var folderAbs = Path.Combine(projectPath, folderRelative.Replace('/', Path.DirectorySeparatorChar));
                if (!Directory.Exists(folderAbs)) { outcome.Error = $"La cartella non esiste: {folderAbs}"; return outcome; }

                // 1) Scanner: che cosa guardare.
                var previous = _scanState.Get(projectPath, folderRelative);
                var scan = await _scanner.ScanAsync(projectPath, folderAbs, previous?.AnalyzedCommit, ct);
                outcome.Mode = scan.Mode; outcome.Reason = scan.Reason;
                var chosen = new HashSet<string>(languages, StringComparer.OrdinalIgnoreCase);
                var inPerimeter = scan.AllFiles.Where(f => chosen.Contains(CodeLanguages.ForFile(f.AbsolutePath)?.Id ?? string.Empty)).ToList();
                outcome.FilesInPerimeter = inPerimeter.Count;
                var candidates = scan.Candidates.Where(f => chosen.Contains(CodeLanguages.ForFile(f.AbsolutePath)?.Id ?? string.Empty)).ToList();
                outcome.Candidates = candidates.Count;

                // 2) Per linguaggio: hash di conferma, estrazione, scrittura.
                foreach (var languageId in languages)
                {
                    ct.ThrowIfCancellationRequested();
                    var lang = CodeLanguages.Require(languageId);
                    var all = inPerimeter.Where(f => CodeLanguages.ForFile(f.AbsolutePath)?.Id == lang.Id).ToList();
                    var extractor = _extractors.Get(lang.Id);
                    if (extractor == null)
                    {
                        if (all.Count > 0) outcome.FilesWithoutExtractor[lang.Id] = all.Count;
                        outcome.Warnings.Add($"{lang.DisplayName}: {all.Count} file, {_extractors.Availability(lang.Id)}");
                        continue;
                    }
                    var toEmit = candidates.Where(f => CodeLanguages.ForFile(f.AbsolutePath)?.Id == lang.Id)
                                           .Where(f => NeedsReparse(projectPath, f, extractor)).ToList();
                    if (toEmit.Count == 0) continue;
                    var extracted = extractor.Extract(new CodeExtractionRequest { ProjectPath = projectPath, AllFiles = all, FilesToEmit = toEmit }, ct);
                    outcome.Warnings.AddRange(extracted.Warnings);
                    foreach (var (file, script) in extracted.Scripts)
                    {
                        CodeScriptWriter.WriteIfChanged(CodeScriptWriter.ScriptPathFor(projectPath, file.ProjectRelativePath, CodeScriptKind.Ast), CodeScriptWriter.Serialize(script));
                        outcome.Reparsed++;
                    }
                }

                // 3) Script AST di sorgenti spariti (o usciti dai linguaggi scelti): via.
                outcome.ScriptsRemoved = RemoveOrphanScripts(projectPath, folderRelative, inPerimeter);

                // 4) Script → DB, e lo stato dello scanner.
                outcome.Load = _loader.LoadProject(projectPath, ct);
                _scanState.Upsert(projectPath, new CodeScanState
                {
                    ProgramFolder = folderRelative, RepositoryRoot = scan.RepositoryRoot, AnalyzedCommit = scan.HeadCommit, Mode = scan.Mode,
                    ScannedAt = DateTime.UtcNow.ToString("o"), Candidates = outcome.Candidates, Reparsed = outcome.Reparsed,
                });
                _logger.LogInformation("[CodeGraph] '{Folder}' ({Mode}, {Reason}): {Files} file, {Cand} candidati, {Rep} riparsati, {Rem} script tolti, caricati {Loaded}",
                    folderRelative, scan.Mode, scan.Reason, outcome.FilesInPerimeter, outcome.Candidates, outcome.Reparsed, outcome.ScriptsRemoved, outcome.Load.Loaded);
                return outcome;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CodeGraph] analisi fallita per '{Folder}'", folderRelative);
                outcome.Error = ex.Message;
                return outcome;
            }
            finally { _gate.Release(); }
        }

        /// <summary>L'hash nell'intestazione dello script esistente conferma o smentisce il candidato (D14).</summary>
        private static bool NeedsReparse(string projectPath, CodeSourceFile file, ICodeExtractor extractor)
        {
            var scriptPath = CodeScriptWriter.ScriptPathFor(projectPath, file.ProjectRelativePath, CodeScriptKind.Ast);
            if (!File.Exists(scriptPath)) return true;
            string firstLine;
            using (var reader = new StreamReader(scriptPath)) firstLine = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(firstLine)) return true;
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(firstLine);
                var root = doc.RootElement;
                var hash = root.TryGetProperty("sourceHash", out var h) ? h.GetString() : null;
                var ext = root.TryGetProperty("extractor", out var e) ? e.GetString() : null;
                var ver = root.TryGetProperty("extractorVersion", out var v) ? v.GetString() : null;
                if (ext != extractor.ExtractorName || ver != extractor.ExtractorVersion) return true;
                return !string.Equals(hash, CodeScriptLoader.Sha256(File.ReadAllText(file.AbsolutePath)), StringComparison.OrdinalIgnoreCase);
            }
            catch (System.Text.Json.JsonException) { return true; }
        }

        private int RemoveOrphanScripts(string projectPath, string folderRelative, List<CodeSourceFile> inPerimeter)
        {
            var scriptsFolder = Path.Combine(CodeGraphPaths.GetScriptsRoot(projectPath), folderRelative.Replace('/', Path.DirectorySeparatorChar));
            if (!Directory.Exists(scriptsFolder)) return 0;
            var alive = new HashSet<string>(inPerimeter.Select(f => f.ProjectRelativePath), StringComparer.Ordinal);
            var removed = 0;
            foreach (var script in Directory.EnumerateFiles(scriptsFolder, "*" + CodeGraphPaths.AstScriptSuffix, SearchOption.AllDirectories))
            {
                var rel = CodeGraphPaths.ToProjectRelative(projectPath, script);
                var source = rel.Substring(CodeGraphPaths.ScriptsFolderName.Length + 1, rel.Length - CodeGraphPaths.ScriptsFolderName.Length - 1 - CodeGraphPaths.AstScriptSuffix.Length);
                if (alive.Contains(source)) continue;
                File.Delete(script);
                removed++;
                _logger.LogInformation("[CodeGraph] script orfano tolto: {Script} (sorgente '{Source}' sparito o fuori dai linguaggi scelti)", rel, source);
            }
            return removed;
        }

        private static (DevelopmentConfig Config, DevelopmentFolder Entry) ReadFolderEntry(string projectPath, string folderRelative)
        {
            var path = Path.Combine(projectPath, MdExplorer.Utilities.DevelopmentConfigCleanup.FileName);
            if (!File.Exists(path)) return (new DevelopmentConfig(), null);
            var config = MdExplorer.Utilities.DevelopmentConfigCleanup.ReadStrict(path);
            var entry = config.Folders.FirstOrDefault(f => string.Equals(NormalizeRelative(f.Path), folderRelative, StringComparison.OrdinalIgnoreCase));
            return (config, entry);
        }

        public static string NormalizeRelative(string path)
            => (path ?? string.Empty).Replace('\\', '/').Trim().TrimStart('/').TrimEnd('/');
    }
}
