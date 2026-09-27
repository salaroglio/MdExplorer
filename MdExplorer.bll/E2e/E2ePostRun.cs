using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.E2e
{
    /// <summary>A script written for one test of a <c>.e2e.md</c>, as its header describes it.</summary>
    public sealed record E2eScript(int TestNumber, string Path, string State, string Fingerprint, string Generator, string CurrentFingerprint)
    {
        /// <summary>
        /// Written from another version of the test: its header carries another fingerprint (D17). A script
        /// whose fingerprint is still to be computed is stale too, it was never checked.
        /// </summary>
        public bool Stale => !string.Equals(Fingerprint, CurrentFingerprint, StringComparison.Ordinal);
    }

    public sealed record E2eLeak(string File, string Key);

    public sealed record E2ePostRunResult(IReadOnlyList<string> Fingerprinted, IReadOnlyList<E2eLeak> Leaks, IReadOnlyList<string> Problems);

    /// <summary>
    /// What MdExplorer does after the agent has run a test file (F5), deterministically:
    /// <list type="bullet">
    /// <item>writes in every script the fingerprint of the test it was written from (the skill has the agent
    /// write <c>da calcolare</c>): from now on a script whose test changes is known to be stale;</item>
    /// <item>looks for credential values in the text files the run produced or touched and, if it finds one,
    /// puts the key back in its place (<c>{{key}}</c>) and says so: the value must never stay on disk
    /// outside the credentials file.</item>
    /// </list>
    /// </summary>
    public static class E2ePostRun
    {
        private static readonly Regex ScriptName = new(@"\.T(\d+)\.spec\.cs$", RegexOptions.IgnoreCase);
        private static readonly Regex FingerprintLine = new(@"^// impronta-sorgente:.*$", RegexOptions.Multiline);
        private static readonly string[] TextExtensions = { ".md", ".cs", ".txt", ".json", ".yml", ".yaml", ".csproj", ".runsettings", ".xml" };

        /// <summary>Values shorter than this are not searched: "1" or "si" would be found everywhere.</summary>
        public const int MinimumSecretLength = 4;

        public static IReadOnlyList<E2eScript> Scripts(E2eRunItem item)
        {
            var document = item.Preflight.Document;
            if (document?.Artifacts == null) return Array.Empty<E2eScript>();
            var folder = Path.Combine(Path.GetDirectoryName(item.TestFile)!, document.Artifacts, "scripts");
            if (!Directory.Exists(folder)) return Array.Empty<E2eScript>();

            var tests = document.Tests.GroupBy(t => t.Number).ToDictionary(g => g.Key, g => g.First().Fingerprint);
            var scripts = new List<E2eScript>();
            foreach (var path in Directory.GetFiles(folder, "*.spec.cs").OrderBy(p => p, StringComparer.Ordinal))
            {
                var m = ScriptName.Match(path);
                if (!m.Success) continue;
                var number = int.Parse(m.Groups[1].Value);
                var text = File.ReadAllText(path);
                scripts.Add(new E2eScript(number, path,
                    Header(text, "stato"), Header(text, "impronta-sorgente"), Header(text, "generatore"),
                    tests.TryGetValue(number, out var current) ? current : null));
            }
            return scripts;
        }

        public static E2ePostRunResult Process(E2eRunItem item, IReadOnlyDictionary<string, string> secrets)
        {
            var problems = new List<string>();
            var fingerprinted = new List<string>();

            foreach (var script in Scripts(item))
            {
                if (script.CurrentFingerprint == null)
                {
                    problems.Add($"{Path.GetFileName(script.Path)}: nel test non c'è un T{script.TestNumber}.");
                    continue;
                }
                var text = File.ReadAllText(script.Path);
                if (!FingerprintLine.IsMatch(text))
                {
                    problems.Add($"{Path.GetFileName(script.Path)}: manca la riga '// impronta-sorgente:' nell'intestazione (vedi la skill mde-e2e).");
                    continue;
                }
                var updated = FingerprintLine.Replace(text, "// impronta-sorgente: " + script.CurrentFingerprint, 1);
                if (updated != text)
                {
                    File.WriteAllText(script.Path, updated);
                    fingerprinted.Add(script.Path);
                }
            }

            var leaks = new List<E2eLeak>();
            var searched = secrets.Where(s => s.Value != null && s.Value.Length >= MinimumSecretLength)
                                  .OrderByDescending(s => s.Value.Length).ToList();
            if (searched.Count > 0)
            {
                foreach (var file in ProducedTextFiles(item))
                {
                    var text = File.ReadAllText(file);
                    var cleaned = text;
                    foreach (var (key, value) in searched)
                    {
                        if (!cleaned.Contains(value, StringComparison.Ordinal)) continue;
                        cleaned = cleaned.Replace(value, "{{" + key + "}}", StringComparison.Ordinal);
                        leaks.Add(new E2eLeak(file, key));
                    }
                    if (cleaned != text) File.WriteAllText(file, cleaned);
                }
            }

            return new E2ePostRunResult(fingerprinted, leaks, problems);
        }

        /// <summary>The test file, the site map, the artifacts and the support files: never the credentials file.</summary>
        private static IEnumerable<string> ProducedTextFiles(E2eRunItem item)
        {
            var folder = Path.GetDirectoryName(item.TestFile)!;
            var document = item.Preflight.Document;
            var files = new List<string> { item.TestFile };
            if (document?.SiteMap != null) files.Add(Path.Combine(folder, document.SiteMap));
            foreach (var support in new[] { "E2eTests.csproj", "E2eSupport.cs", "e2e.runsettings" })
                files.Add(Path.Combine(folder, support));
            if (document?.Artifacts != null)
            {
                var artifacts = Path.Combine(folder, document.Artifacts);
                if (Directory.Exists(artifacts))
                    files.AddRange(Directory.GetFiles(artifacts, "*", SearchOption.AllDirectories));
            }
            var credentials = item.CredentialsFile == null ? null : Path.GetFullPath(item.CredentialsFile);
            return files.Select(Path.GetFullPath).Distinct()
                .Where(f => File.Exists(f) && f != credentials && TextExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()));
        }

        private static string Header(string script, string field)
        {
            var m = Regex.Match(script, @"^// " + Regex.Escape(field) + @":\s*(.*?)\s*$", RegexOptions.Multiline);
            return m.Success ? m.Groups[1].Value : null;
        }
    }
}
