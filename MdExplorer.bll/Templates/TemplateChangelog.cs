using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MdExplorer.Features.Services;

namespace MdExplorer.Features.Templates
{
    /// <summary>Una voce del CHANGELOG di un template: una versione, la sua data, le sue righe.</summary>
    public sealed class TemplateChangelogEntry
    {
        public string Version { get; init; }
        public string Date { get; init; }
        /// <summary>Le righe della voce, senza il titolo e senza le righe vuote.</summary>
        public IReadOnlyList<string> Lines { get; init; } = Array.Empty<string>();
        /// <summary>Le righe <c>⚠️ incompatibile: … Cosa fare: …</c>: da leggere prima di unire.</summary>
        public IReadOnlyList<string> Incompatible { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Il CHANGELOG di un template (P0, «Il CHANGELOG»), letto con tolleranza zero: voci <c>## x.y.z — aaaa-mm-gg</c> dalla
    /// più recente, la prima uguale alla versione del manifesto; una versione maggiore ha almeno una riga
    /// <c>⚠️ incompatibile: … Cosa fare: …</c> e solo una maggiore ce l'ha. Ogni scostamento è un problema detto.
    /// </summary>
    public static class TemplateChangelog
    {
        public const string FileName = "CHANGELOG.md";

        private static readonly Regex Heading = new(@"^##\s+(?<v>\S+)\s+—\s+(?<d>\S+)\s*$", RegexOptions.Compiled);
        private static readonly Regex AnyHeading = new(@"^##\s", RegexOptions.Compiled);
        private static readonly Regex SemVer = new(@"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)$", RegexOptions.Compiled);
        private static readonly Regex Day = new(@"^\d{4}-\d{2}-\d{2}$", RegexOptions.Compiled);
        private static readonly Regex Warning = new(@"^[-*]?\s*⚠️\s*(incompatibile|incompatible)\s*:", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex WhatToDo = new(@"(Cosa fare|What to do)\s*:", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>Le voci, dalla più recente; <paramref name="problems"/> vuoto se il CHANGELOG è in regola.</summary>
        public static IReadOnlyList<TemplateChangelogEntry> Read(string markdown, string manifestVersion, out List<string> problems)
        {
            problems = new List<string>();
            var entries = new List<TemplateChangelogEntry>();
            if (string.IsNullOrWhiteSpace(markdown))
            {
                problems.Add(UiText.Say($"Il {FileName} è vuoto: serve almeno la voce della versione {manifestVersion}.", $"The {FileName} is empty: it needs at least the entry of version {manifestVersion}."));
                return entries;
            }

            string version = null, date = null;
            var lines = new List<string>();
            void Close()
            {
                if (version == null) return;
                entries.Add(new TemplateChangelogEntry
                {
                    Version = version, Date = date, Lines = lines.ToList(),
                    Incompatible = lines.Where(l => Warning.IsMatch(l)).ToList(),
                });
                lines.Clear();
            }

            foreach (var raw in markdown.Replace("\r\n", "\n").Split('\n'))
            {
                var line = raw.TrimEnd();
                if (AnyHeading.IsMatch(line))
                {
                    Close();
                    var m = Heading.Match(line);
                    if (!m.Success || !SemVer.IsMatch(m.Groups["v"].Value) || !Day.IsMatch(m.Groups["d"].Value))
                    {
                        problems.Add(UiText.Say($"Titolo di voce non valido: «{line}». Serve «## x.y.z — aaaa-mm-gg».", $"Invalid entry title: «{line}». It must be «## x.y.z — yyyy-mm-dd»."));
                        version = null;
                        continue;
                    }
                    version = m.Groups["v"].Value;
                    date = m.Groups["d"].Value;
                    continue;
                }
                if (version != null && line.Trim().Length > 0) lines.Add(line.Trim());
            }
            Close();

            if (entries.Count == 0)
            {
                problems.Add(UiText.Say($"Il {FileName} non ha voci «## x.y.z — aaaa-mm-gg».", $"The {FileName} has no «## x.y.z — yyyy-mm-dd» entries."));
                return entries;
            }
            if (entries[0].Version != manifestVersion)
                problems.Add(UiText.Say($"La prima voce del {FileName} è {entries[0].Version}, ma il manifesto dice {manifestVersion}: devono essere uguali.",
                                        $"The first entry of the {FileName} is {entries[0].Version}, but the manifest says {manifestVersion}: they must be equal."));
            for (var i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                var older = i + 1 < entries.Count ? entries[i + 1] : null;
                if (older != null && Compare(e.Version, older.Version) <= 0)
                    problems.Add(UiText.Say($"Le voci del {FileName} vanno dalla più recente: {e.Version} viene prima di {older.Version}.",
                                            $"The {FileName} entries go from the most recent: {e.Version} comes before {older.Version}."));
                var major = older != null && Major(e.Version) > Major(older.Version);
                if (major && e.Incompatible.Count == 0)
                    problems.Add(UiText.Say($"La {e.Version} è una versione maggiore: la sua voce deve dire che cosa rompe, con una riga «⚠️ incompatibile: … Cosa fare: …».",
                                            $"{e.Version} is a major version: its entry must say what it breaks, with a line «⚠️ incompatible: … What to do: …»."));
                if (!major && e.Incompatible.Count > 0 && older != null)
                    problems.Add(UiText.Say($"La {e.Version} ha una riga «⚠️ incompatibile» ma non è una versione maggiore: chi rompe qualcosa alza il primo numero.",
                                            $"{e.Version} has an «⚠️ incompatible» line but is not a major version: whoever breaks something raises the first number."));
                foreach (var w in e.Incompatible.Where(w => !WhatToDo.IsMatch(w)))
                    problems.Add(UiText.Say($"La riga «{w}» della {e.Version} non dice che cosa fare: serve «Cosa fare: …».",
                                            $"The line «{w}» of {e.Version} does not say what to do: it needs «What to do: …»."));
            }
            return entries;
        }

        /// <summary>Le voci dopo <paramref name="from"/> fino a <paramref name="to"/> compresa, dalla più recente.</summary>
        public static IReadOnlyList<TemplateChangelogEntry> Between(IEnumerable<TemplateChangelogEntry> entries, string from, string to)
            => entries.Where(e => Compare(e.Version, from) > 0 && Compare(e.Version, to) <= 0).ToList();

        /// <summary>Confronta due versioni x.y.z; una versione che non si legge solleva.</summary>
        public static int Compare(string a, string b)
        {
            int[] Parts(string v)
            {
                if (v == null || !SemVer.IsMatch(v)) throw new InvalidOperationException(UiText.Say($"Versione non valida: '{v}' (serve x.y.z).", $"Invalid version: '{v}' (x.y.z needed)."));
                return v.Split('.').Select(int.Parse).ToArray();
            }
            var (x, y) = (Parts(a), Parts(b));
            for (var i = 0; i < 3; i++) if (x[i] != y[i]) return x[i].CompareTo(y[i]);
            return 0;
        }

        private static int Major(string v) => int.Parse(v.Split('.')[0]);
    }
}
