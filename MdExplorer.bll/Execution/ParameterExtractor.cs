using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Execution
{
    public class ExecutionParameter
    {
        public string Name { get; set; }
        public string DefaultValue { get; set; }
        public bool IsSecret { get; set; }
        public string Description { get; set; }
        public string Kind { get; set; }
        /// <summary>
        /// When set, renders the parameter as a path-picker button instead of a text input.
        /// Allowed values: <c>"file"</c> (single file), <c>"dir"</c> (folder). Null = plain text input.
        /// </summary>
        public string Picker { get; set; }
        /// <summary>
        /// Closed set of admissible values, declared in the <c>@param</c> description as an
        /// alternation (<c>cobol | pli</c>). When present the UI renders a dropdown instead of a
        /// free-text input. Null = free text.
        /// </summary>
        public List<string> Options { get; set; }
    }

    /// <summary>
    /// Detects runtime parameters declared in a fenced shell script.
    ///
    /// Supported patterns:
    ///   1. <c># @param NAME — description [default: value] [secret]</c>
    ///      The canonical convention taught by the readme-with-runnable-examples
    ///      Copilot skill. Works in any shell — use <c>#</c> for bash/sh/pwsh,
    ///      <c>REM</c> or <c>::</c> for cmd/batch.
    ///   2. <c>export VAR="default"</c> (Runme convention) — only for bash/sh/shell.
    ///   3. <c>param([type]$Name = "default")</c> — PowerShell function header.
    ///   4. <c>&lt;placeholder&gt;</c> inline tokens — letters/digits/dash/underscore.
    ///      Single-word placeholders are accepted only when an @param declares them;
    ///      multi-segment names (with at least one dash or underscore) are always
    ///      accepted, to keep backward compatibility with the original convention.
    ///
    /// A param is flagged as secret when the @param line carries the <c>secret</c>
    /// keyword OR when its name matches KEY/TOKEN/SECRET/PASSWORD/PWD.
    /// </summary>
    public static class ParameterExtractor
    {
        // # @param NAME — optional description...
        // REM @param NAME ...
        // :: @param NAME ...
        private static readonly Regex ParamDocRegex = new(
            @"^[\t ]*(?:#|REM\b|::)[\t ]*@param[\t ]+([A-Za-z][A-Za-z0-9_-]*)[\t ]*(?:[—:\-][\t ]*(.*))?$",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);

        // export VAR="..." | 'value' | bareword
        private static readonly Regex ExportRegex = new(
            @"^[\t ]*export[\t ]+([A-Za-z_][A-Za-z0-9_]*)[\t ]*=[\t ]*(?:""([^""]*)""|'([^']*)'|(\S+))[\t ]*$",
            RegexOptions.Compiled | RegexOptions.Multiline);

        // PowerShell: param([type]$Name = "default") — captured one entry at a time
        private static readonly Regex PwshParamRegex = new(
            @"\[[A-Za-z][A-Za-z0-9\.]*\][\t ]*\$([A-Za-z_][A-Za-z0-9_]*)[\t ]*(?:=[\t ]*(?:""([^""]*)""|'([^']*)'|([^,\)\r\n]+)))?",
            RegexOptions.Compiled);

        // Permissive placeholder — must start with a letter, ASCII only
        private static readonly Regex PlaceholderRegex = new(
            @"<([A-Za-z][A-Za-z0-9_-]*)>",
            RegexOptions.Compiled);

        private static readonly Regex SecretNameRegex = new(
            @"(KEY|TOKEN|SECRET|PASSWORD|PASSWD|PWD)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Pulls `default: value` (or `default = value`) out of a @param description.
        private static readonly Regex DefaultInDescriptionRegex = new(
            @"\bdefault[\t ]*[:=][\t ]*(?:""([^""]*)""|'([^']*)'|([^,;\)\]\r\n]+?))(?=[\t ]*(?:[,;\)\]]|$))",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // The `secret` keyword inside a @param description.
        private static readonly Regex SecretKeywordRegex = new(
            @"(?<![A-Za-z])secret(?![A-Za-z])",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // The `type: file|dir|folder|out-file|...|text` token inside a @param description.
        private static readonly Regex TypeKeywordRegex = new(
            @"\btype[\t ]*[:=][\t ]*(out-file|output-file|save-file|savefile|file|dir|folder|directory|text|string)\b",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // An alternation of admissible values — `cobol | pli`, `dev|staging|prod`.
        // A value is a single word: spaces would make the list indistinguishable from prose
        // that merely happens to contain a pipe.
        private const string OptionListPattern =
            @"[A-Za-z0-9][A-Za-z0-9._+/-]*(?:[\t ]*\|[\t ]*[A-Za-z0-9][A-Za-z0-9._+/-]*)+";

        // Explicit form: `options: a|b|c` (also values/choices/one of). Stripped out of the
        // description, whatever else surrounds it.
        private static readonly Regex OptionsLabeledRegex = new(
            @"\b(?:options|values|choices|one[\t ]+of)[\t ]*[:=][\t ]*(?<list>" + OptionListPattern + @")",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Bare form: the description IS the list, optionally bracketed — `— cobol | pli`.
        // Anchored on the whole (already cleaned) description so a pipe inside prose is ignored.
        private static readonly Regex OptionsBareRegex = new(
            @"^[\[\(]?[\t ]*(?<list>" + OptionListPattern + @")[\t ]*[\]\)]?$",
            RegexOptions.Compiled);

        public static List<ExecutionParameter> Extract(string code, string language)
        {
            var parameters = new List<ExecutionParameter>();
            var byNormalizedName = new Dictionary<string, ExecutionParameter>();
            if (string.IsNullOrWhiteSpace(code)) return parameters;

            // Pass 1 — @param documentation lines (canonical convention).
            foreach (Match match in ParamDocRegex.Matches(code))
            {
                var name = match.Groups[1].Value;
                var description = match.Groups[2].Success ? match.Groups[2].Value.Trim() : null;

                string defaultValue = null;
                string picker = null;
                List<string> options = null;
                var hasSecretKeyword = false;
                if (!string.IsNullOrEmpty(description))
                {
                    var defaultMatch = DefaultInDescriptionRegex.Match(description);
                    if (defaultMatch.Success)
                    {
                        defaultValue = defaultMatch.Groups[1].Success ? defaultMatch.Groups[1].Value
                            : defaultMatch.Groups[2].Success ? defaultMatch.Groups[2].Value
                            : defaultMatch.Groups[3].Value.Trim();
                        description = DefaultInDescriptionRegex.Replace(description, string.Empty);
                    }
                    var typeMatch = TypeKeywordRegex.Match(description);
                    if (typeMatch.Success)
                    {
                        picker = NormalizePicker(typeMatch.Groups[1].Value);
                        description = TypeKeywordRegex.Replace(description, string.Empty);
                    }
                    if (SecretKeywordRegex.IsMatch(description))
                    {
                        hasSecretKeyword = true;
                        description = SecretKeywordRegex.Replace(description, string.Empty);
                    }
                    var labeledOptions = OptionsLabeledRegex.Match(description);
                    if (labeledOptions.Success)
                    {
                        options = SplitOptions(labeledOptions.Groups["list"].Value);
                        description = OptionsLabeledRegex.Replace(description, string.Empty);
                    }
                    // Clean up artefacts left by stripping `default: x` / `secret` / `type: x` out
                    // of structures like "target env (default: staging)" → "target env ()".
                    description = Regex.Replace(description, @"\(\s*\)|\[\s*\]", string.Empty);
                    description = Regex.Replace(description, @"\s+", " ").Trim(' ', '\t', ',', ';');

                    // Bare form — checked last, on the cleaned description, because the whole of
                    // it must be the list: `# @param DIALECT — cobol | pli (default: pli)`.
                    if (options == null)
                    {
                        var bareOptions = OptionsBareRegex.Match(description);
                        if (bareOptions.Success)
                        {
                            options = SplitOptions(bareOptions.Groups["list"].Value);
                            description = string.Empty; // the dropdown already shows the values
                        }
                    }
                }

                // Align the declared default with the option it names, so a casing slip
                // (`default: PLI` against `cobol | pli`) still preselects the dropdown entry.
                if (options != null && !string.IsNullOrEmpty(defaultValue))
                {
                    var declaredOption = options.Find(o => string.Equals(o, defaultValue, System.StringComparison.OrdinalIgnoreCase));
                    if (declaredOption != null) defaultValue = declaredOption;
                }

                AddOrEnrich(parameters, byNormalizedName, new ExecutionParameter
                {
                    Name = name,
                    DefaultValue = defaultValue ?? string.Empty,
                    IsSecret = hasSecretKeyword || SecretNameRegex.IsMatch(name),
                    Description = string.IsNullOrWhiteSpace(description) ? null : description,
                    Kind = "doc",
                    Picker = picker,
                    Options = options,
                });
            }

            var isPosix = language == "bash" || language == "sh" || language == "shell";
            if (isPosix)
            {
                // Pass 2 — bash export convention.
                foreach (Match match in ExportRegex.Matches(code))
                {
                    var name = match.Groups[1].Value;
                    var quotedDouble = match.Groups[2].Success ? match.Groups[2].Value : null;
                    var quotedSingle = match.Groups[3].Success ? match.Groups[3].Value : null;
                    var unquoted = match.Groups[4].Success ? match.Groups[4].Value : null;
                    var defaultValue = quotedDouble ?? quotedSingle ?? unquoted ?? string.Empty;

                    AddOrEnrich(parameters, byNormalizedName, new ExecutionParameter
                    {
                        Name = name,
                        DefaultValue = defaultValue,
                        IsSecret = SecretNameRegex.IsMatch(name),
                        Description = null,
                        Kind = "export",
                    });
                }
            }

            var isPwsh = language == "powershell" || language == "pwsh" || language == "ps1";
            if (isPwsh)
            {
                // Pass 3 — PowerShell param() block.
                foreach (Match match in PwshParamRegex.Matches(code))
                {
                    var name = match.Groups[1].Value;
                    var quotedDouble = match.Groups[2].Success ? match.Groups[2].Value : null;
                    var quotedSingle = match.Groups[3].Success ? match.Groups[3].Value : null;
                    var bare = match.Groups[4].Success ? match.Groups[4].Value.Trim() : null;
                    var defaultValue = quotedDouble ?? quotedSingle ?? bare ?? string.Empty;

                    AddOrEnrich(parameters, byNormalizedName, new ExecutionParameter
                    {
                        Name = name,
                        DefaultValue = defaultValue,
                        IsSecret = SecretNameRegex.IsMatch(name),
                        Description = null,
                        Kind = "pwsh-param",
                    });
                }
            }

            // Pass 4 — inline <placeholder> tokens.
            foreach (Match match in PlaceholderRegex.Matches(code))
            {
                var raw = match.Groups[1].Value;
                var normalized = NormalizeKey(raw);

                if (byNormalizedName.TryGetValue(normalized, out var existing))
                {
                    // Already declared via @param/export/pwsh-param — just remember it's also used as a placeholder.
                    if (existing.Kind == "doc")
                    {
                        existing.Kind = "placeholder";
                    }
                    continue;
                }

                // Backward-compatible heuristic: a bare `<word>` is too easy to mis-detect
                // (think HTML tags). Accept it only if it carries a separator.
                var hasSeparator = raw.Contains('-') || raw.Contains('_');
                if (!hasSeparator) continue;

                var param = new ExecutionParameter
                {
                    Name = raw.Replace('-', '_').ToUpperInvariant(),
                    DefaultValue = string.Empty,
                    IsSecret = SecretNameRegex.IsMatch(raw),
                    Description = $"<{raw}>",
                    Kind = "placeholder",
                };
                parameters.Add(param);
                byNormalizedName[normalized] = param;
            }

            return parameters;
        }

        private static void AddOrEnrich(
            List<ExecutionParameter> bucket,
            Dictionary<string, ExecutionParameter> index,
            ExecutionParameter incoming)
        {
            var key = NormalizeKey(incoming.Name);
            if (index.TryGetValue(key, out var existing))
            {
                if (string.IsNullOrEmpty(existing.Description) && !string.IsNullOrEmpty(incoming.Description))
                    existing.Description = incoming.Description;
                if (string.IsNullOrEmpty(existing.DefaultValue) && !string.IsNullOrEmpty(incoming.DefaultValue))
                    existing.DefaultValue = incoming.DefaultValue;
                if (string.IsNullOrEmpty(existing.Picker) && !string.IsNullOrEmpty(incoming.Picker))
                    existing.Picker = incoming.Picker;
                if ((existing.Options == null || existing.Options.Count == 0) && incoming.Options != null)
                    existing.Options = incoming.Options;
                existing.IsSecret = existing.IsSecret || incoming.IsSecret;
                if (existing.Kind == "doc" && incoming.Kind != "doc")
                    existing.Kind = incoming.Kind;
                return;
            }
            bucket.Add(incoming);
            index[key] = incoming;
        }

        /// <summary>
        /// Turns <c>"cobol | pli"</c> into <c>["cobol", "pli"]</c>, dropping duplicates
        /// (case-insensitive) while keeping the order the author wrote.
        /// </summary>
        private static List<string> SplitOptions(string list)
        {
            var seen = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            var values = new List<string>();
            foreach (var raw in list.Split('|'))
            {
                var value = raw.Trim();
                if (value.Length == 0) continue;
                if (seen.Add(value)) values.Add(value);
            }
            return values.Count > 1 ? values : null;
        }

        private static string NormalizePicker(string raw)
        {
            var lower = raw.ToLowerInvariant();
            return lower switch
            {
                "file" => "file",
                "dir" => "dir",
                "folder" => "dir",
                "directory" => "dir",
                "out-file" => "out-file",
                "output-file" => "out-file",
                "save-file" => "out-file",
                "savefile" => "out-file",
                _ => null, // text/string/anything-else falls back to plain input
            };
        }

        internal static string NormalizeKey(string raw)
        {
            var buffer = new System.Text.StringBuilder(raw.Length);
            foreach (var ch in raw)
            {
                buffer.Append(ch == '-' ? '_' : ch);
            }
            return buffer.ToString().ToUpperInvariant();
        }
    }
}
