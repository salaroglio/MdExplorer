using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace MdExplorer.Features.E2e
{
    /// <summary>
    /// The log of a replay (P3 of sprint 2026-09-28-Script-E2E-Affidabili): the base class of the scripts
    /// (<c>E2eTest</c> in <c>E2eSupport.cs</c>) records, per test, the network calls in a HAR file and the console
    /// in a TSV file; MdExplorer reduces them to <c>registro.T&lt;n&gt;.md</c> — time, method, address, status,
    /// duration, size, kind — and deletes them. A HAR holds headers, cookies and form bodies, credentials included
    /// (the login POST): the raw files never stay in the run folder, which may end up in git (D26).
    /// </summary>
    public static class E2eReplayLog
    {
        private const string HarSuffix = ".har";
        private const string ConsoleSuffix = ".console.tsv";
        private const int MaxText = 300;
        private const int MaxMessage = 4000;

        /// <summary>Resources a page loads to show itself, not data it asks for: counted, not listed.</summary>
        private static readonly HashSet<string> StaticKinds = new(StringComparer.OrdinalIgnoreCase)
            { "stylesheet", "image", "font", "script", "media", "manifest", "texttrack", "imageset" };

        /// <summary>
        /// Writes a <c>registro.T&lt;n&gt;.md</c> for every test class of <paramref name="testNumbers"/> (full class
        /// name → test number) that left raw files in <paramref name="runFolder"/>, then deletes every raw file,
        /// also those of unknown classes. Returns the names of the logs written.
        /// </summary>
        public static IReadOnlyList<string> Process(string runFolder, IReadOnlyDictionary<string, int> testNumbers,
            IReadOnlyDictionary<string, string> secrets, string baseUrl, IReadOnlyList<E2eReplayOutcome> outcomes = null)
        {
            var written = new List<string>();
            if (!Directory.Exists(runFolder)) return written;
            try
            {
                foreach (var (cls, number) in testNumbers.OrderBy(t => t.Value))
                {
                    var har = Path.Combine(runFolder, cls + HarSuffix);
                    var console = Path.Combine(runFolder, cls + ConsoleSuffix);
                    if (!File.Exists(har) && !File.Exists(console)) continue;
                    var name = "registro.T" + number + ".md";
                    var text = Build(number, File.Exists(har) ? File.ReadAllText(har) : null,
                        File.Exists(console) ? File.ReadAllLines(console) : Array.Empty<string>(), baseUrl,
                        outcomes?.FirstOrDefault(o => o.Test == number));
                    File.WriteAllText(Path.Combine(runFolder, name), E2ePostRun.Redact(text, secrets));
                    written.Add(name);
                }
            }
            finally
            {
                DeleteRaw(runFolder);
            }
            return written;
        }

        /// <summary>Deletes the raw recordings of a run folder: also when the replay was interrupted.</summary>
        public static void DeleteRaw(string runFolder)
        {
            if (!Directory.Exists(runFolder)) return;
            foreach (var file in Directory.EnumerateFiles(runFolder)
                         .Where(f => f.EndsWith(HarSuffix, StringComparison.OrdinalIgnoreCase) || f.EndsWith(ConsoleSuffix, StringComparison.OrdinalIgnoreCase))
                         .ToList())
                File.Delete(file);
        }

        public static string Build(int testNumber, string harJson, IReadOnlyList<string> consoleLines, string baseUrl,
            E2eReplayOutcome outcome = null)
        {
            var calls = ReadCalls(harJson);
            var console = consoleLines.Select(ReadConsole).Where(c => c != null).ToList();
            var origin = calls.Select(c => c.Start).Concat(console.Select(c => c!.Value.Time)).DefaultIfEmpty(DateTimeOffset.MinValue).Min();

            var md = new StringBuilder();
            md.Append("# Registro del rigioco — T").Append(testNumber).Append("\n\n");
            md.Append("Chiamate di rete e console del test durante il rigioco dello script; i tempi sono in secondi dall'inizio.\n");
            md.Append("Scritto da MdExplorer, senza intestazioni, cookie e corpi delle chiamate.\n\n");

            if (outcome != null)
            {
                md.Append("## Esito dello script\n\n");
                if (outcome.Passed) md.Append("✅ superato\n\n");
                else
                {
                    var message = (outcome.Message ?? "").Trim();
                    if (message.Length > MaxMessage) message = message.Substring(0, MaxMessage) + "\n…";
                    md.Append("❌ fallito:\n\n```text\n").Append(message.Replace("```", "'''")).Append("\n```\n\n");
                }
            }

            md.Append("## Chiamate\n\n");
            var listed = calls.Where(c => !StaticKinds.Contains(c.Kind)).ToList();
            if (listed.Count == 0) md.Append("Nessuna chiamata oltre alle risorse statiche.\n");
            else
            {
                md.Append("| + s | metodo | indirizzo | stato | durata | byte | tipo |\n|---|---|---|---|---|---|---|\n");
                foreach (var c in listed)
                    md.Append("| ").Append(Seconds(c.Start - origin)).Append(" | ").Append(c.Method).Append(" | ").Append(Cell(Address(c.Url, baseUrl)))
                      .Append(" | ").Append(c.Status > 0 ? c.Status.ToString(CultureInfo.InvariantCulture) : "fallita")
                      .Append(" | ").Append(c.Duration >= 0 ? Math.Round(c.Duration).ToString(CultureInfo.InvariantCulture) + " ms" : "?")
                      .Append(" | ").Append(c.Size >= 0 ? c.Size.ToString(CultureInfo.InvariantCulture) : "?")
                      .Append(" | ").Append(c.Kind).Append(" |\n");
            }
            var statics = calls.Count - listed.Count;
            if (statics > 0) md.Append("\nRisorse statiche non elencate: ").Append(statics).Append(" (script, fogli di stile, immagini, font).\n");

            md.Append("\n## Console\n\n");
            if (console.Count == 0) md.Append("Nessun messaggio.\n");
            else
            {
                md.Append("| + s | tipo | messaggio |\n|---|---|---|\n");
                foreach (var c in console)
                    md.Append("| ").Append(Seconds(c!.Value.Time - origin)).Append(" | ").Append(c.Value.Kind).Append(" | ").Append(Cell(c.Value.Text)).Append(" |\n");
            }
            return md.ToString();
        }

        private sealed record Call(DateTimeOffset Start, string Method, string Url, int Status, double Duration, long Size, string Kind);

        private static List<Call> ReadCalls(string harJson)
        {
            var calls = new List<Call>();
            if (string.IsNullOrWhiteSpace(harJson)) return calls;
            using var har = JsonDocument.Parse(harJson);
            if (!har.RootElement.TryGetProperty("log", out var log) || !log.TryGetProperty("entries", out var entries)) return calls;
            foreach (var e in entries.EnumerateArray())
            {
                var request = e.GetProperty("request");
                var response = e.TryGetProperty("response", out var r) ? r : default;
                long size = -1;
                if (response.ValueKind == JsonValueKind.Object && response.TryGetProperty("content", out var content)
                    && content.TryGetProperty("size", out var s) && s.ValueKind == JsonValueKind.Number) size = s.GetInt64();
                calls.Add(new Call(
                    DateTimeOffset.Parse(e.GetProperty("startedDateTime").GetString()!, CultureInfo.InvariantCulture),
                    request.GetProperty("method").GetString()!,
                    request.GetProperty("url").GetString()!,
                    response.ValueKind == JsonValueKind.Object && response.TryGetProperty("status", out var st) ? st.GetInt32() : 0,
                    e.TryGetProperty("time", out var t) && t.ValueKind == JsonValueKind.Number ? t.GetDouble() : -1,
                    size,
                    e.TryGetProperty("_resourceType", out var k) ? k.GetString() ?? "" : ""));
            }
            return calls.OrderBy(c => c.Start).ToList();
        }

        private static (DateTimeOffset Time, string Kind, string Text)? ReadConsole(string line)
        {
            var parts = line.Split('\t', 3);
            if (parts.Length < 3 || !DateTimeOffset.TryParse(parts[0], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var time)) return null;
            return (time, parts[1], parts[2]);
        }

        /// <summary>The path of an address of the tested site, the whole address of any other.</summary>
        private static string Address(string url, string baseUrl)
        {
            if (!string.IsNullOrEmpty(baseUrl) && Uri.TryCreate(baseUrl, UriKind.Absolute, out var site)
                && Uri.TryCreate(url, UriKind.Absolute, out var call)
                && string.Equals(site.GetLeftPart(UriPartial.Authority), call.GetLeftPart(UriPartial.Authority), StringComparison.OrdinalIgnoreCase))
                return call.PathAndQuery;
            return url;
        }

        private static string Seconds(TimeSpan span) => span.TotalSeconds.ToString("0.00", CultureInfo.InvariantCulture);

        private static string Cell(string text)
        {
            var clean = text.Replace("\r", " ").Replace("\n", " ").Replace("|", "\\|");
            return clean.Length > MaxText ? clean.Substring(0, MaxText) + "…" : clean;
        }
    }
}
