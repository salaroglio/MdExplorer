using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Services.AI.CopilotSdk
{
    /// <summary>
    /// Which markdown files of the project a Copilot tool call has just written, read from its
    /// arguments.
    ///
    /// <para>
    /// The tools that write change name and shape with the model and the CLI version — measured
    /// 11/09/2026: <c>create</c> <c>{path, file_text}</c>, <c>edit</c> <c>{path, old_str, new_str}</c>,
    /// <c>apply_patch</c> with the paths inside the patch text (<c>*** Add File: …</c>,
    /// <c>*** Update File: …</c>). So the arguments are not read by tool: every string is looked
    /// at, a single-line one ending in <c>.md</c> is a path, a multi-line one is searched for patch
    /// headers. Only existing <c>.md</c> files inside the project count.
    /// </para>
    ///
    /// <para>
    /// A file written by a shell command (<c>echo … &gt; x.md</c>) does not show in the arguments
    /// and is not seen: Copilot writes documents with its file tools.
    /// </para>
    /// </summary>
    internal static class AgentWrittenMarkdown
    {
        private static readonly Regex PatchHeader = new Regex(
            @"^\*\*\*\s+(?:(?:Add|Update)\s+File|Move\s+to):\s*(?<path>.+?)\s*$",
            RegexOptions.Multiline | RegexOptions.Compiled);

        public static IReadOnlyList<string> Paths(JsonElement? arguments, string workingDirectory)
        {
            var found = new List<string>();
            if (arguments == null || string.IsNullOrWhiteSpace(workingDirectory)) return found;

            var root = Path.GetFullPath(workingDirectory);
            if (!root.EndsWith(Path.DirectorySeparatorChar)) root += Path.DirectorySeparatorChar;

            var candidates = new List<string>();
            Collect(arguments.Value, candidates);

            var seen = new HashSet<string>(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
            foreach (var candidate in candidates)
            {
                string full;
                try
                {
                    full = Path.GetFullPath(Path.IsPathRooted(candidate) ? candidate : Path.Combine(root, candidate));
                }
                catch (Exception)
                {
                    continue; // not a path after all
                }
                var inside = full.StartsWith(root, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
                if (inside && full.EndsWith(".md", StringComparison.OrdinalIgnoreCase) && File.Exists(full) && seen.Add(full))
                {
                    found.Add(full);
                }
            }
            return found;
        }

        private static void Collect(JsonElement element, List<string> candidates)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var property in element.EnumerateObject()) Collect(property.Value, candidates);
                    break;
                case JsonValueKind.Array:
                    foreach (var item in element.EnumerateArray()) Collect(item, candidates);
                    break;
                case JsonValueKind.String:
                    var text = element.GetString();
                    if (string.IsNullOrWhiteSpace(text)) break;
                    if (text.IndexOf('\n') >= 0)
                    {
                        foreach (Match m in PatchHeader.Matches(text)) candidates.Add(m.Groups["path"].Value);
                    }
                    else if (text.Trim().EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                    {
                        candidates.Add(text.Trim());
                    }
                    break;
            }
        }
    }
}
