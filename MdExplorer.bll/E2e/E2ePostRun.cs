using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.E2e
{
    /// <summary>A script written for one test of a <c>.e2e.md</c>, as its header describes it.</summary>
    public sealed record E2eScript(int TestNumber, string Path, string State, string Fingerprint, string Generator, string CurrentFingerprint,
        string CurrentGenerator = null)
    {
        /// <summary>
        /// Written from another version of the test (another fingerprint, D17), or by another version of the
        /// skill (the source hash is not freshness: the emitter's version counts too). A script whose
        /// fingerprint is still to be computed is stale as well: it was never checked.
        /// </summary>
        public bool Stale => !string.Equals(Fingerprint, CurrentFingerprint, StringComparison.Ordinal)
            || (CurrentGenerator != null && !string.Equals(Generator, CurrentGenerator, StringComparison.Ordinal));
    }

    /// <param name="Replaced">True when the value was replaced by the key; false when it is only reported.</param>
    public sealed record E2eLeak(string File, string Key, bool Replaced);

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
        private static readonly Regex ScriptName = new(@"\.T([0-9]{1,6})\.spec\.cs$", RegexOptions.IgnoreCase);
        private static readonly Regex FingerprintLine = new(@"^// impronta-sorgente:[^\r\n]*", RegexOptions.Multiline);
        private static readonly string[] TextExtensions = { ".md", ".cs", ".txt", ".json", ".yml", ".yaml", ".csproj", ".runsettings", ".xml" };

        /// <summary>Values shorter than this are not searched: "1" or "si" would be found everywhere.</summary>
        public const int MinimumSecretLength = 4;

        /// <summary>The placeholder the skill has the agent write: MdExplorer fills it in.</summary>
        public const string PendingFingerprint = "da calcolare";

        public static IReadOnlyList<E2eScript> Scripts(E2eRunItem item, string currentGenerator = null)
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
                    tests.TryGetValue(number, out var current) ? current : null, currentGenerator));
            }
            return scripts;
        }

        /// <param name="runStartUtc">Files not written since then are not looked at: they are not this run's.</param>
        public static E2ePostRunResult Process(E2eRunItem item, IReadOnlyDictionary<string, string> secrets, DateTime runStartUtc)
        {
            var problems = new List<string>();
            var fingerprinted = new List<string>();

            // Only the scripts the agent has just written carry the placeholder. A script left from an older
            // version of the test keeps its old fingerprint, and so stays stale (D17): stamping every script
            // would make an outdated one look current (review of 27/09/2026).
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
                if (!string.Equals(script.Fingerprint, PendingFingerprint, StringComparison.Ordinal)) continue;
                File.WriteAllText(script.Path, FingerprintLine.Replace(text, "// impronta-sorgente: " + script.CurrentFingerprint, 1));
                fingerprinted.Add(script.Path);
            }

            var leaks = new List<E2eLeak>();
            var searched = secrets.Where(s => s.Value != null && s.Value.Length >= MinimumSecretLength)
                                  .OrderByDescending(s => s.Value.Length).ToList();
            if (searched.Count > 0)
            {
                var runFolder = item.RunFolder == null ? null : Path.GetFullPath(item.RunFolder) + Path.DirectorySeparatorChar;
                foreach (var file in ProducedTextFiles(item).Where(f => File.GetLastWriteTimeUtc(f) >= runStartUtc))
                {
                    // In the run's own folder (report, notes) the key replaces the value. Elsewhere — the test,
                    // the site map, the scripts, the support files — a value can be an ordinary word ("admin" in
                    // "/admin", "true" in the runsettings): it is reported, not rewritten.
                    var rewrite = runFolder != null && file.StartsWith(runFolder, StringComparison.Ordinal);
                    var text = File.ReadAllText(file);
                    var cleaned = text;
                    foreach (var (key, value) in searched)
                    {
                        if (!cleaned.Contains(value, StringComparison.Ordinal)) continue;
                        if (rewrite) cleaned = cleaned.Replace(value, "{{" + key + "}}", StringComparison.Ordinal);
                        leaks.Add(new E2eLeak(file, key, rewrite));
                    }
                    if (cleaned != text) File.WriteAllText(file, cleaned);
                }
            }

            return new E2ePostRunResult(fingerprinted, leaks, problems);
        }

        /// <summary>The credential values in <paramref name="text"/> replaced by their keys (replay messages).</summary>
        public static string Redact(string text, IReadOnlyDictionary<string, string> secrets)
        {
            if (string.IsNullOrEmpty(text)) return text;
            foreach (var (key, value) in secrets.Where(s => s.Value != null && s.Value.Length >= MinimumSecretLength).OrderByDescending(s => s.Value.Length))
                text = text.Replace(value, "{{" + key + "}}", StringComparison.Ordinal);
            return text;
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
