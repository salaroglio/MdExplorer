using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.CodeGraph
{
    /// <summary>Un passo della scoperta delle dipendenze, per la finestra «Analizza questo programma».</summary>
    public sealed class SeedProgress
    {
        /// <summary><c>files</c> (elenco della radice), <c>index</c> (le dichiarazioni), <c>read</c> (un file letto), <c>extract</c> (un file analizzato).</summary>
        public string Step { get; init; }
        public bool Done { get; init; }
        public int? Count { get; init; }
        public double? Seconds { get; init; }
        public string File { get; init; }
        public int? Lines { get; init; }
    }

    /// <summary>
    /// Lo spazio di lavoro della scoperta delle dipendenze, misurato (20/11/2026): ogni operazione — l'elenco della radice,
    /// la ricerca delle dichiarazioni, la lettura di un file — va nel log con il suo tempo e alla finestra come passo. Su una
    /// radice SSH la finestra restava su «Cerco le dipendenze…» senza dire dove fosse né quanto mancasse.
    /// </summary>
    public sealed class ObservedCodeWorkspace : ICodeWorkspace
    {
        private readonly ICodeWorkspace _inner;
        private readonly ILogger _logger;
        private readonly Action<SeedProgress> _progress;
        private readonly string _label;

        public TimeSpan FilesTime { get; private set; }
        public TimeSpan IndexTime { get; private set; }
        public TimeSpan ReadTime { get; private set; }
        public int ReadCount { get; private set; }

        public ObservedCodeWorkspace(ICodeWorkspace inner, ILogger logger, Action<SeedProgress> progress, string label)
        {
            _inner = inner; _logger = logger; _progress = progress ?? (_ => { }); _label = label;
        }

        public IReadOnlyList<string> Files()
        {
            _progress(new SeedProgress { Step = "files" });
            var clock = Stopwatch.StartNew();
            var files = _inner.Files();
            FilesTime += clock.Elapsed;
            _logger.LogInformation("[Seed] {Label}: elenco della radice, {Count} file in {Seconds:0.0} s", _label, files.Count, clock.Elapsed.TotalSeconds);
            _progress(new SeedProgress { Step = "files", Done = true, Count = files.Count, Seconds = clock.Elapsed.TotalSeconds });
            return files;
        }

        public string Read(string logicalPath)
        {
            _progress(new SeedProgress { Step = "read", File = logicalPath, Count = ReadCount });
            var clock = Stopwatch.StartNew();
            var text = _inner.Read(logicalPath);
            ReadTime += clock.Elapsed;
            ReadCount++;
            _logger.LogInformation("[Seed] {Label}: letto {File} ({Chars} caratteri) in {Seconds:0.00} s", _label, logicalPath, text.Length, clock.Elapsed.TotalSeconds);
            return text;
        }

        public CodeSourceFile Source(string logicalPath) => _inner.Source(logicalPath);

        public IEnumerable<(string Path, int Line, string Text)> Grep(Regex pattern, IReadOnlyCollection<string> extensions)
        {
            var clock = Stopwatch.StartNew();
            var hits = _inner.Grep(pattern, extensions).ToList();
            IndexTime += clock.Elapsed;
            _logger.LogInformation("[Seed] {Label}: ricerca «{Pattern}», {Count} righe in {Seconds:0.0} s", _label, pattern, hits.Count, clock.Elapsed.TotalSeconds);
            return hits;
        }

        public IEnumerable<(int Pattern, string Path, int Line, string Text)> GrepMany(IReadOnlyList<Regex> patterns, IReadOnlyCollection<string> extensions)
        {
            _progress(new SeedProgress { Step = "index", Count = patterns.Count });
            var clock = Stopwatch.StartNew();
            var hits = _inner.GrepMany(patterns, extensions).ToList();
            IndexTime += clock.Elapsed;
            _logger.LogInformation("[Seed] {Label}: dichiarazioni ({Patterns} espressioni, file {Extensions}), {Count} righe in {Seconds:0.0} s",
                _label, patterns.Count, string.Join(" ", extensions), hits.Count, clock.Elapsed.TotalSeconds);
            _progress(new SeedProgress { Step = "index", Done = true, Count = hits.Count, Seconds = clock.Elapsed.TotalSeconds });
            return hits;
        }
    }
}
