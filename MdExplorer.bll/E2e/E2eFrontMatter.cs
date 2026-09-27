using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace MdExplorer.Features.E2e
{
    /// <summary>A test file or a folder's settings that cannot be read, with what to do about it.</summary>
    public sealed class E2eFormatException : Exception
    {
        public E2eFormatException(string message) : base(message) { }
    }

    /// <summary>
    /// The execution settings under <c>e2e.run</c>. A null value means "not said here": it is inherited
    /// from the folders above (D22, D23).
    /// </summary>
    public sealed record E2eRunSettings(bool? DedicatedSession, bool? CommitAfterRun, bool? Headless)
    {
        public static readonly E2eRunSettings None = new(null, null, null);

        public bool IsEmpty => DedicatedSession == null && CommitAfterRun == null && Headless == null;
    }

    /// <summary>
    /// Reads and writes the top-level <c>e2e:</c> block of a markdown front matter, leaving every other
    /// line of the file as it is.
    ///
    /// <para>
    /// Only the <c>e2e:</c> block is parsed: a front matter whose other keys YAML cannot read (it
    /// happens in hand-written files) does not stop the tests. The block must be written in block
    /// style (<c>e2e:</c> alone on its line, keys indented below); the flow style
    /// (<c>e2e: { … }</c>) is refused with a message, because it cannot be edited line by line.
    /// </para>
    /// </summary>
    public static class E2eFrontMatter
    {
        public const string DedicatedSessionKey = "dedicatedSession";
        public const string CommitAfterRunKey = "commitAfterRun";
        public const string HeadlessKey = "headless";

        private static readonly Regex BlockStart = new(@"^e2e:\s*(#.*)?$");
        private static readonly Regex FlowStart = new(@"^e2e:\s*[^\s#]");

        /// <summary>The text of the <c>e2e:</c> block (its first line included), or null when there is none.</summary>
        public static string ReadBlock(string markdown)
        {
            var file = Lines.Of(markdown);
            var block = FindBlock(file);
            return block == null ? null : string.Join("\n", file.Items.Skip(block.Value.Start).Take(block.Value.End - block.Value.Start));
        }

        /// <summary>The <c>e2e:</c> block as a YAML mapping, or null when the file has none.</summary>
        public static YamlMappingNode ReadMapping(string markdown, string fileName)
        {
            var text = ReadBlock(markdown);
            if (text == null) return null;

            YamlStream stream;
            try
            {
                stream = new YamlStream();
                stream.Load(new StringReader(text));
            }
            catch (YamlException ex)
            {
                throw new E2eFormatException(
                    $"{fileName}: il blocco 'e2e:' del front matter non è YAML valido (riga {ex.Start.Line}): {ex.InnerException?.Message ?? ex.Message}");
            }

            var root = stream.Documents.FirstOrDefault()?.RootNode as YamlMappingNode;
            var e2e = root?.Children.TryGetValue(new YamlScalarNode("e2e"), out var node) == true ? node : null;
            if (e2e is YamlScalarNode scalar && string.IsNullOrEmpty(scalar.Value)) return new YamlMappingNode();
            if (e2e is not YamlMappingNode mapping)
                throw new E2eFormatException($"{fileName}: 'e2e:' deve contenere delle chiavi (baseUrl, siteMap, credentials, artifacts, run).");
            return mapping;
        }

        /// <summary>The settings under <c>e2e.run</c>; <see cref="E2eRunSettings.None"/> when there are none.</summary>
        public static E2eRunSettings ReadRunSettings(string markdown, string fileName)
        {
            var e2e = ReadMapping(markdown, fileName);
            if (e2e == null || !e2e.Children.TryGetValue(new YamlScalarNode("run"), out var runNode))
                return E2eRunSettings.None;
            if (runNode is YamlScalarNode empty && string.IsNullOrEmpty(empty.Value))
                return E2eRunSettings.None;
            if (runNode is not YamlMappingNode run)
                throw new E2eFormatException($"{fileName}: 'e2e.run' deve contenere delle chiavi ({DedicatedSessionKey}, {CommitAfterRunKey}, {HeadlessKey}).");

            var known = new[] { DedicatedSessionKey, CommitAfterRunKey, HeadlessKey };
            foreach (var key in run.Children.Keys.OfType<YamlScalarNode>().Select(k => k.Value))
            {
                if (!known.Contains(key))
                    throw new E2eFormatException($"{fileName}: chiave sconosciuta 'e2e.run.{key}'. Chiavi ammesse: {string.Join(", ", known)}.");
            }

            return new E2eRunSettings(Bool(run, DedicatedSessionKey, fileName), Bool(run, CommitAfterRunKey, fileName), Bool(run, HeadlessKey, fileName));
        }

        /// <summary>
        /// The markdown with <c>e2e.run</c> replaced by <paramref name="run"/>: a null value removes its key,
        /// an empty <paramref name="run"/> removes <c>run:</c>, and an <c>e2e:</c> left empty is removed.
        /// Nothing else in the file changes. A file without front matter gets one.
        /// </summary>
        public static string WriteRunSettings(string markdown, E2eRunSettings run, string fileName)
        {
            run ??= E2eRunSettings.None;
            var file = Lines.Of(markdown);

            if (file.Open < 0)
            {
                if (run.IsEmpty) return markdown;
                var created = new List<string> { "---", "e2e:" };
                created.AddRange(RunLines(run, "  "));
                created.Add("---");
                file.Insert(0, created);
                return Verified(file.Render(), run, fileName);
            }

            var block = FindBlock(file);
            if (block == null)
            {
                if (run.IsEmpty) return markdown;
                var inserted = new List<string> { "e2e:" };
                inserted.AddRange(RunLines(run, "  "));
                file.Insert(file.Close, inserted);
                return Verified(file.Render(), run, fileName);
            }

            var (start, end) = block.Value;
            var indent = ChildIndent(file.Items, start + 1, end) ?? "  ";
            var runStart = -1;
            for (var i = start + 1; i < end; i++)
            {
                if (Regex.IsMatch(file.Items[i], "^" + indent + @"run:\s*[^\s#]"))
                    throw new E2eFormatException($"{fileName}: scrivi 'e2e.run' in forma a blocchi (una chiave per riga), non 'run: {{ … }}'.");
                if (Regex.IsMatch(file.Items[i], "^" + indent + @"run:\s*(#.*)?$")) { runStart = i; break; }
            }

            var newRun = run.IsEmpty ? new List<string>() : RunLines(run, indent);
            if (runStart >= 0)
            {
                var runEnd = runStart + 1;
                while (runEnd < end && (IsBlank(file.Items[runEnd]) || Indentation(file.Items[runEnd]) > indent.Length)) runEnd++;
                while (runEnd > runStart + 1 && IsBlank(file.Items[runEnd - 1])) runEnd--;
                file.Remove(runStart, runEnd - runStart);
                file.Insert(runStart, newRun);
                end += newRun.Count - (runEnd - runStart);
            }
            else
            {
                file.Insert(end, newRun);
                end += newRun.Count;
            }

            var hasChildren = file.Items.Skip(start + 1).Take(end - start - 1).Any(l => !IsBlank(l) && !l.TrimStart().StartsWith("#", StringComparison.Ordinal));
            if (!hasChildren) file.Remove(start, end - start);

            return Verified(file.Render(), run, fileName);
        }

        /// <summary>
        /// <paramref name="newFile"/> with the <c>e2e:</c> block of <paramref name="oldFile"/> put back at the end
        /// of its front matter. For files MdExplorer regenerates (the folder's <c>.md.directory</c>): the
        /// settings a user wrote there must survive the regeneration.
        /// </summary>
        public static string CarryOverBlock(string oldFile, string newFile)
        {
            string block;
            try
            {
                block = oldFile == null ? null : ReadBlock(oldFile);
            }
            catch (E2eFormatException)
            {
                // A flow-style 'e2e: { … }' cannot be edited, but it must not stop the regeneration of the
                // folder's summary nor vanish: carried over as it is, the tests' checks will report it.
                // Carried over as it is, up to the line that closes its braces (a flow mapping over several lines).
                var lines = oldFile.Replace("\r\n", "\n").Split('\n');
                var first = Array.FindIndex(lines, l => l.StartsWith("e2e:", StringComparison.Ordinal));
                var taken = new List<string>();
                var depth = 0;
                for (var k = first; k >= 0 && k < lines.Length; k++)
                {
                    taken.Add(lines[k]);
                    depth += lines[k].Count(c => c == '{') - lines[k].Count(c => c == '}');
                    if (depth <= 0) break;
                }
                block = first < 0 ? null : string.Join("\n", taken);
            }
            if (block == null) return newFile;

            var target = Lines.Of(newFile);
            if (target.Open < 0 || FindBlock(target) != null) return newFile;
            target.Insert(target.Close, block.Split('\n'));
            return target.Render();
        }

        private static bool? Bool(YamlMappingNode run, string key, string fileName)
        {
            if (!run.Children.TryGetValue(new YamlScalarNode(key), out var node)) return null;
            var value = (node as YamlScalarNode)?.Value;
            if (string.IsNullOrEmpty(value)) return null;
            if (string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(value, "false", StringComparison.OrdinalIgnoreCase)) return false;
            throw new E2eFormatException($"{fileName}: 'e2e.run.{key}' vale '{value}': scrivi true oppure false, o togli la riga per ereditare.");
        }

        private static List<string> RunLines(E2eRunSettings run, string indent)
        {
            var lines = new List<string> { indent + "run:" };
            void Add(string key, bool? value)
            {
                if (value != null) lines.Add(indent + indent + key + ": " + (value.Value ? "true" : "false"));
            }
            Add(DedicatedSessionKey, run.DedicatedSession);
            Add(CommitAfterRunKey, run.CommitAfterRun);
            Add(HeadlessKey, run.Headless);
            return lines;
        }

        /// <summary>Re-reads what was written: a mismatch is a bug here, never something to save.</summary>
        private static string Verified(string written, E2eRunSettings wanted, string fileName)
        {
            var read = ReadRunSettings(written, fileName);
            if (read != (wanted.IsEmpty ? E2eRunSettings.None : wanted))
                throw new InvalidOperationException($"{fileName}: le impostazioni scritte ({read}) non sono quelle chieste ({wanted}).");
            return written;
        }

        private static (int Start, int End)? FindBlock(Lines file)
        {
            if (file.Open < 0) return null;
            for (var i = file.Open + 1; i < file.Close; i++)
            {
                var line = file.Items[i];
                if (FlowStart.IsMatch(line))
                    throw new E2eFormatException("Scrivi 'e2e:' in forma a blocchi (una chiave per riga, rientrate), non 'e2e: { … }'.");
                if (!BlockStart.IsMatch(line)) continue;

                // The block goes on while lines are indented. A comment at column 0 does not end it when
                // indented lines follow (legal YAML): seen cutting 'credentials' off in the review of 27/09/2026.
                var end = i + 1;
                while (end < file.Close)
                {
                    var current = file.Items[end];
                    if (IsBlank(current) || char.IsWhiteSpace(current[0])) { end++; continue; }
                    if (current.StartsWith("#", StringComparison.Ordinal) && IndentedLineFollows(file, end + 1)) { end++; continue; }
                    break;
                }
                while (end > i + 1 && (IsBlank(file.Items[end - 1]) || file.Items[end - 1].StartsWith("#", StringComparison.Ordinal))) end--;
                return (i, end);
            }
            return null;
        }

        private static bool IndentedLineFollows(Lines file, int from)
        {
            for (var k = from; k < file.Close; k++)
            {
                var line = file.Items[k];
                if (IsBlank(line) || line.StartsWith("#", StringComparison.Ordinal)) continue;
                return char.IsWhiteSpace(line[0]);
            }
            return false;
        }

        private static string ChildIndent(IReadOnlyList<string> lines, int from, int to)
        {
            for (var i = from; i < to; i++)
            {
                if (IsBlank(lines[i]) || lines[i].TrimStart().StartsWith("#", StringComparison.Ordinal)) continue;
                return new string(' ', Indentation(lines[i]));
            }
            return null;
        }

        private static int Indentation(string line) => line.Length - line.TrimStart(' ').Length;

        private static bool IsBlank(string line) => line.Trim().Length == 0;

        /// <summary>
        /// A file as lines, with the position of the front matter fences (-1 when there is none). Every line
        /// keeps its own line ending, so the lines this class does not touch come back byte for byte; an
        /// inserted line takes the ending of the line before it.
        /// </summary>
        private sealed class Lines
        {
            private readonly List<string> _ends;

            private Lines(List<string> items, List<string> ends, bool bom)
            {
                Items = items;
                _ends = ends;
                Bom = bom;
            }

            public IReadOnlyList<string> Items { get; }
            public int Open { get; private set; } = -1;
            public int Close { get; private set; } = -1;
            private bool Bom { get; }

            public static Lines Of(string text)
            {
                text ??= string.Empty;
                var bom = text.Length > 0 && text[0] == '\uFEFF';
                if (bom) text = text.Substring(1);

                var items = new List<string>();
                var ends = new List<string>();
                var at = 0;
                foreach (Match m in Regex.Matches(text, "\r\n|\n"))
                {
                    items.Add(text.Substring(at, m.Index - at));
                    ends.Add(m.Value);
                    at = m.Index + m.Length;
                }
                items.Add(text.Substring(at));
                ends.Add(string.Empty);

                var lines = new Lines(items, ends, bom);
                lines.FindFences();
                return lines;
            }

            public void Insert(int index, IEnumerable<string> newLines)
            {
                var list = newLines.ToList();
                var ending = index > 0 ? _ends[index - 1] : _ends.FirstOrDefault(e => e.Length > 0) ?? "\n";
                if (ending.Length == 0) ending = "\n";
                ((List<string>)Items).InsertRange(index, list);
                _ends.InsertRange(index, list.Select(_ => ending));
                FindFences();
            }

            public void Remove(int index, int count)
            {
                ((List<string>)Items).RemoveRange(index, count);
                _ends.RemoveRange(index, count);
                FindFences();
            }

            public string Render()
            {
                var sb = new System.Text.StringBuilder(Bom ? "\uFEFF" : "");
                for (var i = 0; i < Items.Count; i++) sb.Append(Items[i]).Append(_ends[i]);
                return sb.ToString();
            }

            private void FindFences()
            {
                Open = Items.Count > 0 && Items[0].TrimEnd() == "---" ? 0 : -1;
                Close = -1;
                if (Open != 0) return;
                for (var i = 1; i < Items.Count; i++)
                {
                    var t = Items[i].TrimEnd();
                    if (t == "---" || t == "...") { Close = i; break; }
                }
                if (Close < 0) Open = -1;
            }
        }
    }
}
