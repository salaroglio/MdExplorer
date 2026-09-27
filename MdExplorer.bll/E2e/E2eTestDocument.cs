using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.RepresentationModel;

namespace MdExplorer.Features.E2e
{
    /// <summary>The kinds of check the mde-e2e skill teaches; anything else is <see cref="Judgment"/>.</summary>
    public enum E2eCheckKind
    {
        TextVisible,
        TextHidden,
        UrlContains,
        TitleIs,
        FieldValue,
        ButtonDisabled,
        ButtonCount,
        LinkCount,
        /// <summary>A check written in another form: the agent judges it, the script only comments it.</summary>
        Judgment,
    }

    public sealed class E2eStep
    {
        public int Number { get; init; }

        /// <summary>The line after the number, ✔ included.</summary>
        public string Text { get; init; }

        /// <summary>Null for an action; the kind of check for a ✔ line.</summary>
        public E2eCheckKind? Check { get; init; }

        /// <summary>The quoted text a check compares with (the expected text, URL part, title or value).</summary>
        public string Expected { get; init; }

        /// <summary>The field or button a check is about, when it names one.</summary>
        public string Target { get; init; }

        public int? Count { get; init; }

        /// <summary>The <c>{{key}}</c> references of the line.</summary>
        public IReadOnlyList<string> CredentialKeys { get; init; }
    }

    public sealed class E2eTest
    {
        public int Number { get; init; }
        public string Title { get; init; }

        /// <summary>1-based line of the <c>## T&lt;n&gt;</c> heading in the file.</summary>
        public int Line { get; init; }

        public IReadOnlyList<E2eStep> Steps { get; init; }

        /// <summary>
        /// <c>sha256:</c> + 16 hex digits of the test's section (heading and lines, trailing blanks and empty
        /// lines ignored): a script whose header carries another fingerprint was written from another
        /// version of the test (D17).
        /// </summary>
        public string Fingerprint { get; init; }
    }

    /// <summary>A <c>*.e2e.md</c> file as the mde-e2e skill describes it.</summary>
    public sealed class E2eTestDocument
    {
        public string BaseUrl { get; init; }
        public string SiteMap { get; init; }
        public string Credentials { get; init; }
        public string Artifacts { get; init; }
        public E2eRunSettings Run { get; init; }
        public IReadOnlyList<E2eTest> Tests { get; init; }
        public bool HasArtifactsSection { get; init; }
        public bool HasResultsSection { get; init; }

        /// <summary>Things that are wrong in the text of the file, each with what to do.</summary>
        public IReadOnlyList<string> Problems { get; init; }

        public IEnumerable<string> CredentialKeys =>
            Tests.SelectMany(t => t.Steps).SelectMany(s => s.CredentialKeys).Distinct(StringComparer.Ordinal);
    }

    /// <summary>
    /// Reads a <c>*.e2e.md</c> file. What makes it unreadable (no <c>e2e:</c> block, YAML that does not
    /// parse) throws <see cref="E2eFormatException"/>; what is only wrong (a repeated test number, steps
    /// out of order, a missing key) goes to <see cref="E2eTestDocument.Problems"/>, so every problem
    /// of a file is reported at once. Lines inside code fences are not read.
    /// </summary>
    public static class E2eTestParser
    {
        private static readonly Regex TestHeading = new(@"^## T(\d+)\s*[—–-]\s*(.+?)\s*$");
        private static readonly Regex OtherHeading = new(@"^#{1,2} ");
        private static readonly Regex StepLine = new(@"^(\d+)\.\s+(.+?)\s*$");
        private static readonly Regex Fence = new(@"^\s*(```|~~~)");
        private static readonly Regex CredentialKey = new(@"\{\{\s*([^}\s]+)\s*\}\}");

        private static readonly (Regex Pattern, E2eCheckKind Kind)[] CheckForms =
        {
            (new Regex(@"^Compare il testo ""(?<expected>.+)""$"), E2eCheckKind.TextVisible),
            (new Regex(@"^Non compare il testo ""(?<expected>.+)""$"), E2eCheckKind.TextHidden),
            (new Regex(@"^L['’]URL contiene ""(?<expected>.+)""$"), E2eCheckKind.UrlContains),
            (new Regex(@"^Il titolo della pagina è ""(?<expected>.+)""$"), E2eCheckKind.TitleIs),
            (new Regex(@"^Il campo ""(?<target>.+?)"" vale ""(?<expected>.*)""$"), E2eCheckKind.FieldValue),
            (new Regex(@"^Il bottone ""(?<target>.+)"" è disabilitato$"), E2eCheckKind.ButtonDisabled),
            (new Regex(@"^Ci sono esattamente (?<count>\d+) bottoni ""(?<target>.+)""$"), E2eCheckKind.ButtonCount),
            (new Regex(@"^Ci sono esattamente (?<count>\d+) link ""(?<target>.+)""$"), E2eCheckKind.LinkCount),
        };

        public static E2eTestDocument Parse(string markdown, string fileName)
        {
            var e2e = E2eFrontMatter.ReadMapping(markdown, fileName)
                ?? throw new E2eFormatException(
                    $"{fileName}: manca il blocco 'e2e:' nel front matter (baseUrl, siteMap, credentials, artifacts). Vedi la skill mde-e2e.");

            var problems = new List<string>();
            string Key(string key)
            {
                var value = e2e.Children.TryGetValue(new YamlScalarNode(key), out var node) ? (node as YamlScalarNode)?.Value : null;
                if (string.IsNullOrWhiteSpace(value)) problems.Add($"{fileName}: manca 'e2e.{key}' nel front matter.");
                return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
            }

            var baseUrl = Key("baseUrl");
            if (baseUrl != null && !(Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)))
                problems.Add($"{fileName}: 'e2e.baseUrl' vale '{baseUrl}': serve un indirizzo completo che inizia con http:// o https://.");

            var document = new DocumentBuilder(fileName, problems);
            document.Read(markdown);

            return new E2eTestDocument
            {
                BaseUrl = baseUrl,
                SiteMap = Key("siteMap"),
                Credentials = Key("credentials"),
                Artifacts = Key("artifacts"),
                Run = E2eFrontMatter.ReadRunSettings(markdown, fileName),
                Tests = document.Tests,
                HasArtifactsSection = document.HasArtifactsSection,
                HasResultsSection = document.HasResultsSection,
                Problems = problems,
            };
        }

        public static E2eStep ReadStep(int number, string text)
        {
            var keys = CredentialKey.Matches(text).Select(m => m.Groups[1].Value).Distinct(StringComparer.Ordinal).ToList();
            if (!text.StartsWith("✔"))
                return new E2eStep { Number = number, Text = text, CredentialKeys = keys };

            var check = text.Substring(1).Trim();
            foreach (var (pattern, kind) in CheckForms)
            {
                var m = pattern.Match(check);
                if (!m.Success) continue;
                return new E2eStep
                {
                    Number = number,
                    Text = text,
                    Check = kind,
                    Expected = m.Groups["expected"].Success ? m.Groups["expected"].Value : null,
                    Target = m.Groups["target"].Success ? m.Groups["target"].Value : null,
                    Count = m.Groups["count"].Success ? int.Parse(m.Groups["count"].Value) : null,
                    CredentialKeys = keys,
                };
            }
            return new E2eStep { Number = number, Text = text, Check = E2eCheckKind.Judgment, CredentialKeys = keys };
        }

        public static string Fingerprint(IEnumerable<string> sectionLines)
        {
            var normalized = string.Join("\n", sectionLines.Select(l => l.TrimEnd()).Where(l => l.Length > 0));
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
            return "sha256:" + Convert.ToHexString(hash).Substring(0, 16).ToLowerInvariant();
        }

        private sealed class DocumentBuilder
        {
            private readonly string _fileName;
            private readonly List<string> _problems;
            private readonly List<E2eTest> _tests = new();

            public DocumentBuilder(string fileName, List<string> problems)
            {
                _fileName = fileName;
                _problems = problems;
            }

            public IReadOnlyList<E2eTest> Tests => _tests;
            public bool HasArtifactsSection { get; private set; }
            public bool HasResultsSection { get; private set; }

            public void Read(string markdown)
            {
                var lines = (markdown ?? string.Empty).Replace("\r\n", "\n").Split('\n');
                var first = BodyStart(lines);
                var inFence = false;

                int? number = null;
                string title = null;
                var headingLine = 0;
                var section = new List<string>();
                var steps = new List<E2eStep>();

                void Close()
                {
                    if (number == null) return;
                    if (steps.Count == 0)
                        _problems.Add($"{_fileName}: il test T{number} (riga {headingLine}) non ha passi numerati.");
                    for (var i = 0; i < steps.Count; i++)
                    {
                        if (steps[i].Number != i + 1)
                        {
                            _problems.Add($"{_fileName}: nel test T{number} il passo {i + 1} è numerato {steps[i].Number}: numera i passi 1, 2, 3, … senza salti.");
                            break;
                        }
                    }
                    _tests.Add(new E2eTest
                    {
                        Number = number.Value,
                        Title = title,
                        Line = headingLine,
                        Steps = steps.ToList(),
                        Fingerprint = Fingerprint(section),
                    });
                    number = null;
                    steps.Clear();
                    section.Clear();
                }

                for (var i = first; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (Fence.IsMatch(line)) inFence = !inFence;
                    if (inFence) { if (number != null) section.Add(line); continue; }

                    var heading = TestHeading.Match(line);
                    if (heading.Success)
                    {
                        Close();
                        number = int.Parse(heading.Groups[1].Value);
                        title = heading.Groups[2].Value;
                        headingLine = i + 1;
                        section.Add(line);
                        continue;
                    }
                    if (OtherHeading.IsMatch(line))
                    {
                        Close();
                        var name = line.TrimStart('#').Trim();
                        if (line.StartsWith("## ") && name == "Artefatti") HasArtifactsSection = true;
                        if (line.StartsWith("## ") && name == "Esiti") HasResultsSection = true;
                        continue;
                    }
                    if (number == null) continue;

                    section.Add(line);
                    var step = StepLine.Match(line);
                    if (step.Success) steps.Add(ReadStep(int.Parse(step.Groups[1].Value), step.Groups[2].Value));
                }
                Close();

                if (_tests.Count == 0)
                    _problems.Add($"{_fileName}: nessun test. Un test comincia con una riga '## T1 — <titolo>' seguita da passi numerati.");
                foreach (var repeated in _tests.GroupBy(t => t.Number).Where(g => g.Count() > 1))
                    _problems.Add($"{_fileName}: il numero T{repeated.Key} è usato da più test (righe {string.Join(", ", repeated.Select(t => t.Line))}): i numeri dei test non si riusano.");
                if (!HasArtifactsSection)
                    _problems.Add($"{_fileName}: manca la sezione '## Artefatti' (può essere vuota).");
                if (!HasResultsSection)
                    _problems.Add($"{_fileName}: manca la sezione '## Esiti' (può essere vuota).");
            }

            private static int BodyStart(string[] lines)
            {
                if (lines.Length == 0 || lines[0].TrimStart('﻿').TrimEnd() != "---") return 0;
                for (var i = 1; i < lines.Length; i++)
                {
                    var t = lines[i].TrimEnd();
                    if (t == "---" || t == "...") return i + 1;
                }
                return 0;
            }
        }
    }
}
