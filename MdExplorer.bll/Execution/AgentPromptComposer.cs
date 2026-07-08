using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Execution
{
    /// <summary>
    /// Composes the final prompt sent to the headless Copilot CLI when an agent
    /// (<c>*.agent.md</c>) is launched — manually from the Agent Launch dialog or
    /// automatically by a schedule/hook.
    ///
    /// Works on prompts normalized by the <c>mde-prompt-for-agents</c> skill:
    /// a <c>```params</c> fenced block declaring <c># @param NAME — ..., type: file</c>
    /// lines, and <c>&lt;placeholder&gt;</c> tokens in the task body. Placeholder names
    /// match parameter names case-insensitively with <c>-</c>/<c>_</c> equivalence
    /// (same rules as <see cref="ParameterExtractor"/>).
    ///
    /// NOTE: the satellite scheduler (<c>MdExplorer.Scheduler</c>) keeps a small
    /// duplicated copy of this logic (it must not reference MdExplorer.bll).
    /// If you change the composition rules here, update the satellite's
    /// <c>PromptComposer.cs</c> too.
    /// </summary>
    public static class AgentPromptComposer
    {
        // Same token shape ParameterExtractor's placeholder pass accepts.
        private static readonly Regex PlaceholderRegex = new(
            @"<([A-Za-z][A-Za-z0-9_-]*)>",
            RegexOptions.Compiled);

        // The ```params fenced block (any fence length >= 3, optional trailing spaces).
        private static readonly Regex ParamsFenceRegex = new(
            @"^[\t ]*`{3,}params[\t ]*\r?\n.*?\r?\n[\t ]*`{3,}[\t ]*(\r?\n|$)",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.Singleline);

        // A "## Parameters" heading left empty once the fence above has been stripped.
        private static readonly Regex EmptyParametersHeadingRegex = new(
            @"^[\t ]*##[\t ]*Parameters[\t ]*\r?\n(?:[\t ]*\r?\n)*(?=^[\t ]*#|\z)",
            RegexOptions.Compiled | RegexOptions.Multiline);

        /// <summary>
        /// Replaces every <c>&lt;placeholder&gt;</c> whose normalized name has an entry in
        /// <paramref name="values"/>, then strips the <c>```params</c> declaration block
        /// (it has served its purpose; leaving it would only confuse the executing agent).
        /// Placeholders without a value are left untouched — the caller decides whether
        /// that is an error.
        /// </summary>
        public static string Substitute(string normalizedPrompt, IDictionary<string, string> values)
        {
            if (string.IsNullOrEmpty(normalizedPrompt)) return normalizedPrompt;

            var byKey = new Dictionary<string, string>();
            if (values != null)
            {
                foreach (var pair in values)
                {
                    if (string.IsNullOrEmpty(pair.Key)) continue;
                    byKey[ParameterExtractor.NormalizeKey(pair.Key)] = pair.Value ?? string.Empty;
                }
            }

            var substituted = PlaceholderRegex.Replace(normalizedPrompt, match =>
            {
                var key = ParameterExtractor.NormalizeKey(match.Groups[1].Value);
                return byKey.TryGetValue(key, out var value) ? value : match.Value;
            });

            var withoutFence = ParamsFenceRegex.Replace(substituted, string.Empty);
            return EmptyParametersHeadingRegex.Replace(withoutFence, string.Empty).Trim() + "\n";
        }

        /// <summary>
        /// Final prompt for the run: the full <c>.agent.md</c> content (the agent's
        /// standing instructions) followed by the prepared task prompt.
        /// </summary>
        public static string ComposeRunPrompt(string agentFileContent, string preparedPrompt)
        {
            if (string.IsNullOrWhiteSpace(agentFileContent))
                throw new ArgumentException("Agent file content is empty — refusing to run a bodyless agent.", nameof(agentFileContent));
            if (string.IsNullOrWhiteSpace(preparedPrompt))
                throw new ArgumentException("Prepared prompt is empty — nothing to ask the agent.", nameof(preparedPrompt));

            return agentFileContent.TrimEnd() + "\n\n---\n\n# Task\n\n" + preparedPrompt.Trim() + "\n";
        }

        /// <summary>
        /// Names of placeholders that remain unsubstituted in a prompt (normalized keys).
        /// Lets callers fail loudly before spawning the agent.
        /// </summary>
        public static List<string> FindUnresolvedPlaceholders(string prompt)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(prompt)) return result;
            foreach (Match match in PlaceholderRegex.Matches(prompt))
            {
                var key = ParameterExtractor.NormalizeKey(match.Groups[1].Value);
                if (!result.Contains(key)) result.Add(key);
            }
            return result;
        }
    }
}
