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
    /// <c>&lt;placeholder&gt;</c> tokens are replaced by the user value <b>verbatim</b> — no shell
    /// quoting is added. The script author owns the delimiters: whatever surrounds the token in
    /// the markdown (<c>"&lt;NAME&gt;"</c>, <c>'&lt;NAME&gt;'</c>, or nothing at all) is exactly
    /// what the shell sees. See the <c>mde-readme</c> skill for the authoring convention.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <c>export VAR="default"</c> (POSIX) and <c>$Var = "default"</c> assignments inside a
    /// PowerShell <c>param()</c> block have their literal default rewritten to the user value
    /// so the rest of the script keeps reading <c>$VAR</c> with the expected semantics. These
    /// two <b>do</b> quote, because the rewrite replaces the whole right-hand side of an
    /// assignment — including the author's own quotes — and an unquoted multi-word value would
    /// produce syntactically broken script.
    /// </description>
    /// </item>
    /// </list>
    /// </summary>
    public static class ParameterSubstitution
    {
        public static string Apply(
            string code,
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
                        result = ReplacePlaceholder(result, param.Name, userValue);
                        break;
                    case "export":
                        result = RewriteExportDefault(result, param.Name, userValue);
                        // Some scripts also reference the same name as a placeholder — handle both.
                        result = ReplacePlaceholder(result, param.Name, userValue);
                        break;
                    case "pwsh-param":
                        result = RewritePwshParamDefault(result, param.Name, userValue);
                        result = ReplacePlaceholder(result, param.Name, userValue);
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

        /// <summary>
        /// Replaces every <c>&lt;name&gt;</c> token matching <paramref name="paramName"/> with the
        /// user value <b>as typed</b>.
        /// <para>
        /// Deliberately no shell-quoting: the runner used to wrap the value in quotes, which
        /// double-quoted every author who wrote <c>"&lt;NAME&gt;"</c> (the value ended up holding
        /// literal quote characters) and forced awkward templates on anyone who needed the raw
        /// text spliced into a larger literal. Delimiters are now the author's job — the token
        /// stands for the text, not for a shell word.
        /// </para>
        /// </summary>
        private static string ReplacePlaceholder(string code, string paramName, string rawValue)
        {
            rawValue ??= string.Empty;
            // Match any <name> whose normalized form equals the parameter's normalized name.
            var targetKey = paramName.Replace('-', '_').ToUpperInvariant();
            return Regex.Replace(code, @"<([A-Za-z][A-Za-z0-9_-]*)>", m =>
            {
                var raw = m.Groups[1].Value;
                var normalized = raw.Replace('-', '_').ToUpperInvariant();
                return normalized == targetKey ? rawValue : m.Value;
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
