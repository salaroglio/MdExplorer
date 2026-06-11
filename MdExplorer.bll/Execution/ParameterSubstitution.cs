using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Execution
{
    /// <summary>
    /// Substitutes user-provided parameter values into the script body just before execution.
    /// <para>
    /// Two substitution channels are supported, matching the two detection paths in
    /// <see cref="ParameterExtractor"/>:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <c>&lt;placeholder&gt;</c> tokens are replaced by the user value, quoted safely for the
    /// target shell (single-quote wrapping for POSIX/PowerShell, double-quote for cmd).
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <c>export VAR="default"</c> (POSIX) and <c>$Var = "default"</c> assignments inside a
    /// PowerShell <c>param()</c> block have their literal default rewritten to the user value
    /// so the rest of the script keeps reading <c>$VAR</c> with the expected semantics.
    /// </description>
    /// </item>
    /// </list>
    /// </summary>
    public static class ParameterSubstitution
    {
        public static string Apply(
            string code,
            string language,
            IReadOnlyList<ExecutionParameter> parameters,
            IReadOnlyDictionary<string, string> values)
        {
            if (string.IsNullOrEmpty(code) || parameters == null || parameters.Count == 0 || values == null || values.Count == 0)
                return code;

            var result = code;
            foreach (var param in parameters)
            {
                if (!TryGetValueCaseInsensitive(values, param.Name, out var userValue) || userValue == null)
                    continue;

                switch (param.Kind)
                {
                    case "placeholder":
                    case "doc": // @param declared but never inlined — still allow placeholder substitution if present
                        result = ReplacePlaceholder(result, param.Name, QuoteForShell(userValue, language));
                        break;
                    case "export":
                        result = RewriteExportDefault(result, param.Name, userValue);
                        // Some scripts also reference the same name as a placeholder — handle both.
                        result = ReplacePlaceholder(result, param.Name, QuoteForShell(userValue, language));
                        break;
                    case "pwsh-param":
                        result = RewritePwshParamDefault(result, param.Name, userValue);
                        result = ReplacePlaceholder(result, param.Name, QuoteForShell(userValue, language));
                        break;
                }
            }
            return result;
        }

        private static bool TryGetValueCaseInsensitive(IReadOnlyDictionary<string, string> values, string name, out string value)
        {
            if (values.TryGetValue(name, out value)) return true;
            foreach (var kv in values)
            {
                if (string.Equals(kv.Key, name, System.StringComparison.OrdinalIgnoreCase))
                {
                    value = kv.Value;
                    return true;
                }
            }
            value = null;
            return false;
        }

        // Integer / decimal / boolean literals — safe to inject bare (no shell metachars).
        // Quoting them breaks downstream parsing: e.g. `--port '3030'` is rejected by Java as
        // a bad port number, and `"https://api/'3030'"` yields a malformed URL.
        private static readonly Regex BareSafeLiteralRegex = new(
            @"^(?:-?\d+(?:\.\d+)?|true|false)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static string QuoteForShell(string value, string language)
        {
            value ??= string.Empty;

            // Numeric / boolean values don't need shell-quoting and quoting them actually breaks
            // common cases (process args, URL fragments, casts to [int]). Stringy values still
            // get the protective quoting below.
            if (BareSafeLiteralRegex.IsMatch(value))
                return value;

            switch (language)
            {
                case "bash":
                case "sh":
                case "shell":
                    // POSIX-safe single-quoting: 'foo'\''bar' for embedded single quotes.
                    return "'" + value.Replace("'", "'\\''") + "'";
                case "powershell":
                case "pwsh":
                case "ps1":
                    // PowerShell single-quoted: doubled '' escapes a single quote.
                    return "'" + value.Replace("'", "''") + "'";
                case "cmd":
                case "bat":
                case "batch":
                    // cmd is fragile: escape % and ", wrap in double quotes.
                    return "\"" + value.Replace("%", "%%").Replace("\"", "\"\"") + "\"";
                default:
                    return "'" + value.Replace("'", "'\\''") + "'";
            }
        }

        private static string ReplacePlaceholder(string code, string paramName, string quotedValue)
        {
            // Match any <name> whose normalized form equals the parameter's normalized name.
            var targetKey = paramName.Replace('-', '_').ToUpperInvariant();
            return Regex.Replace(code, @"<([A-Za-z][A-Za-z0-9_-]*)>", m =>
            {
                var raw = m.Groups[1].Value;
                var normalized = raw.Replace('-', '_').ToUpperInvariant();
                return normalized == targetKey ? quotedValue : m.Value;
            });
        }

        private static string RewriteExportDefault(string code, string paramName, string userValue)
        {
            // Replace the value portion of `export NAME=<anything-on-one-line>`.
            var pattern = $@"^([\t ]*export[\t ]+{Regex.Escape(paramName)}[\t ]*=[\t ]*)(?:""[^""\r\n]*""|'[^'\r\n]*'|[^\s\r\n]+)([\t ]*(?:\r?\n|$))";
            var replacement = "$1" + ToBashDoubleQuoted(userValue) + "$2";
            return Regex.Replace(code, pattern, replacement, RegexOptions.Multiline);
        }

        private static string RewritePwshParamDefault(string code, string paramName, string userValue)
        {
            // Replace the default in `[type]$Name = <value>` inside a PowerShell param() block.
            // Matches up to a comma/closing paren so multi-param signatures stay intact.
            var pattern = $@"(\[[A-Za-z][A-Za-z0-9\.]*\][\t ]*\${Regex.Escape(paramName)}[\t ]*=[\t ]*)(?:""[^""\r\n]*""|'[^'\r\n]*'|[^,\)\r\n]+)";
            var replacement = "$1" + ToPwshSingleQuoted(userValue);
            return Regex.Replace(code, pattern, replacement);
        }

        private static string ToBashDoubleQuoted(string value)
        {
            value ??= string.Empty;
            // Inside double quotes bash still interprets $, `, \, " — escape those.
            var escaped = value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("$", "\\$")
                .Replace("`", "\\`");
            return "\"" + escaped + "\"";
        }

        private static string ToPwshSingleQuoted(string value)
        {
            value ??= string.Empty;
            return "'" + value.Replace("'", "''") + "'";
        }
    }
}
