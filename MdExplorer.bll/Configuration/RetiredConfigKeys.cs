using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Configuration
{
    /// <summary>
    /// The keys of <c>.development.yml</c> that MdExplorer wrote once and no longer has: they are <b>deleted
    /// from the file</b>, by name, not ignored. Reading stays strict — a key that is neither known nor listed
    /// here is an error — so a retired key, a typo and a key of a newer version are never confused.
    /// <para>
    /// To retire a key: remove it from the model and add it here, with the date and what replaced it. A key
    /// can leave this list only when no project file can still hold it.
    /// </para>
    /// </summary>
    public static class RetiredConfigKeys
    {
        /// <summary>Section (top-level key) and key, in the file's camelCase.</summary>
        public static readonly IReadOnlyList<(string Section, string Key)> All = new[]
        {
            // 2026-08-02 retired, 2026-10-05 removed: a document delivered by an agent is proposed and a
            // person decides (the merge request); nothing merged by itself any more, and nothing read the flag.
            ("agentCity", "autoMergeAgentDeliverables"),
        };

        /// <summary>
        /// The keys that became a list: the value is <b>kept</b>, moved to the new key as its only item. Same rules as
        /// a retired key (a scalar on one line, directly under its section); the file is rewritten once, when the
        /// project is opened, and the change is a commit like any cleanup.
        /// </summary>
        public static readonly IReadOnlyList<(string Section, string Key, string ListKey)> BecameList = new[]
        {
            // 2026-10-18 (PW): a project has several workflows; agentCity.workflowDoc → agentCity.workflowDocs.
            ("agentCity", "workflowDoc", "workflowDocs"),
        };

        private static readonly Regex TopLevel = new(@"^(?<key>[A-Za-z_][\w-]*)\s*:", RegexOptions.Compiled);
        private static readonly Regex Child = new(@"^(?<indent>[ \t]+)(?<key>[A-Za-z_][\w-]*)\s*:(?<value>.*)$", RegexOptions.Compiled);

        /// <summary>
        /// The text without the retired keys, and which ones were there (<c>section.key</c>). A retired key is
        /// a scalar on one line, directly under its section; a section left with no keys goes with it. The
        /// rest of the file — order, comments, line endings — is not touched.
        /// </summary>
        public static string Remove(string yaml, out IReadOnlyList<string> removed)
        {
            var found = new List<string>();
            removed = found;
            if (string.IsNullOrEmpty(yaml)) return yaml;

            var newline = yaml.Contains("\r\n") ? "\r\n" : "\n";
            var lines = yaml.Replace("\r\n", "\n").Split('\n').ToList();

            string section = null;
            string sectionIndent = null;   // the indentation of the section's direct children
            for (var i = 0; i < lines.Count; i++)
            {
                var top = TopLevel.Match(lines[i]);
                if (top.Success) { section = top.Groups["key"].Value; sectionIndent = null; continue; }

                var child = Child.Match(lines[i]);
                if (!child.Success || section == null) continue;
                sectionIndent ??= child.Groups["indent"].Value;
                if (child.Groups["indent"].Value != sectionIndent) continue;   // deeper: not a direct child

                var key = child.Groups["key"].Value;
                var listed = BecameList.FirstOrDefault(r => r.Section == section && r.Key == key);
                if (listed.ListKey != null)
                {
                    var indent = child.Groups["indent"].Value;
                    if (SectionHas(lines, i, indent, listed.ListKey))
                        throw new InvalidOperationException(
                            $"'{section}' ha sia '{key}' sia '{listed.ListKey}': '{key}' è diventato l'elenco '{listed.ListKey}'. Sposta il valore nell'elenco e togli la riga '{key}'. " +
                            $"/ '{section}' has both '{key}' and '{listed.ListKey}': '{key}' became the list '{listed.ListKey}'. Move the value into the list and remove the '{key}' line.");
                    var value = child.Groups["value"].Value.Trim();
                    found.Add(section + "." + key + " → " + listed.ListKey);
                    if (value.Length == 0 || value == "null" || value == "~" || value == "''" || value == "\"\"")
                    {
                        lines.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        lines[i] = indent + listed.ListKey + ":";
                        lines.Insert(i + 1, indent + "  - " + value);
                        i++;
                    }
                    continue;
                }
                if (!All.Any(r => r.Section == section && r.Key == key)) continue;

                found.Add(section + "." + key);
                lines.RemoveAt(i);
                i--;
            }
            if (found.Count == 0) return yaml;

            // A section that only held retired keys is an empty heading: it goes too.
            foreach (var name in found.Where(f => !f.Contains(" → ")).Select(f => f.Split('.')[0]).Distinct().ToList())
            {
                var at = lines.FindIndex(l => { var m = TopLevel.Match(l); return m.Success && m.Groups["key"].Value == name && l.Substring(m.Length).Trim().Length == 0; });
                if (at < 0) continue;
                var next = at + 1;
                while (next < lines.Count && lines[next].Trim().Length == 0) next++;
                var hasChildren = next < lines.Count && (lines[next].StartsWith(" ") || lines[next].StartsWith("\t") || lines[next].TrimStart().StartsWith("-"));
                if (!hasChildren) lines.RemoveAt(at);
            }

            return string.Join(newline, lines);
        }

        /// <summary>The section that holds line <paramref name="at"/> already has <paramref name="key"/> at the same indentation.</summary>
        private static bool SectionHas(List<string> lines, int at, string indent, string key)
        {
            bool Is(int i) { var m = Child.Match(lines[i]); return m.Success && m.Groups["indent"].Value == indent && m.Groups["key"].Value == key; }
            for (var i = at - 1; i >= 0 && !TopLevel.IsMatch(lines[i]); i--) if (Is(i)) return true;
            for (var i = at + 1; i < lines.Count && !TopLevel.IsMatch(lines[i]); i++) if (Is(i)) return true;
            return false;
        }
    }
}
