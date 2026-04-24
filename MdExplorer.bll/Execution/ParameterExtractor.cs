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
    }

    /// <summary>
    /// Detects runtime parameters declared in a fenced shell script.
    /// Two patterns supported in MVP:
    ///   1. bash convention: <c>export VAR="default"</c> (same as Runme) — only for bash/sh/shell
    ///   2. generic placeholder: <c>&lt;some-placeholder&gt;</c> — any language, must contain '-' or '_' to avoid false positives
    /// A param is flagged as secret when its name matches KEY/TOKEN/SECRET/PASSWORD/PWD.
    /// </summary>
    public static class ParameterExtractor
    {
        private static readonly Regex ExportRegex = new(
            @"^[\t ]*export[\t ]+([A-Za-z_][A-Za-z0-9_]*)[\t ]*=[\t ]*(?:""([^""]*)""|'([^']*)'|(\S+))[\t ]*$",
            RegexOptions.Compiled | RegexOptions.Multiline);

        private static readonly Regex PlaceholderRegex = new(
            @"<([A-Za-z][A-Za-z0-9]*(?:[-_][A-Za-z0-9]+)+)>",
            RegexOptions.Compiled);

        private static readonly Regex SecretNameRegex = new(
            @"(KEY|TOKEN|SECRET|PASSWORD|PASSWD|PWD)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static List<ExecutionParameter> Extract(string code, string language)
        {
            var parameters = new List<ExecutionParameter>();
            var seen = new HashSet<string>();
            if (string.IsNullOrWhiteSpace(code)) return parameters;

            var isPosix = language == "bash" || language == "sh" || language == "shell";
            if (isPosix)
            {
                foreach (Match match in ExportRegex.Matches(code))
                {
                    var name = match.Groups[1].Value;
                    var quotedDouble = match.Groups[2].Success ? match.Groups[2].Value : null;
                    var quotedSingle = match.Groups[3].Success ? match.Groups[3].Value : null;
                    var unquoted = match.Groups[4].Success ? match.Groups[4].Value : null;
                    var defaultValue = quotedDouble ?? quotedSingle ?? unquoted ?? string.Empty;

                    if (seen.Add(name))
                    {
                        parameters.Add(new ExecutionParameter
                        {
                            Name = name,
                            DefaultValue = defaultValue,
                            IsSecret = SecretNameRegex.IsMatch(name),
                            Description = null,
                            Kind = "export",
                        });
                    }
                }
            }

            foreach (Match match in PlaceholderRegex.Matches(code))
            {
                var raw = match.Groups[1].Value;
                var name = NormalizeName(raw);
                if (seen.Add(name))
                {
                    parameters.Add(new ExecutionParameter
                    {
                        Name = name,
                        DefaultValue = string.Empty,
                        IsSecret = SecretNameRegex.IsMatch(raw),
                        Description = $"<{raw}>",
                        Kind = "placeholder",
                    });
                }
            }

            return parameters;
        }

        private static string NormalizeName(string raw)
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
