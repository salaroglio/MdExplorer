using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.AI.ClaudeCode
{
    /// <summary>
    /// Che cosa sa fare il CLI di Claude Code installato, chiesto a lui una volta per eseguibile e per versione del file.
    /// Serve per le opzioni che un CLI vecchio non conosce: in <c>-p</c> un'opzione sconosciuta lo ferma all'avvio
    /// («error: unknown option», uscita 1, verificato il 25/11/2026 su 2.1.331).
    /// <para>⚠️ <c>--thinking-display</c> NON compare in <c>claude --help</c>: è nascosta. Si riconosce dandole un valore
    /// sbagliato insieme a <c>--version</c>: se il CLI la conosce risponde con l'elenco dei valori ammessi; se non la
    /// conosce, con <c>--version</c> stampa solo la versione.</para>
    /// </summary>
    public static class ClaudeCodeCliFeatures
    {
        /// <summary>Il ragionamento riassunto: in <c>-p</c> è l'unico modo di averne il testo (verificato il 25/11/2026, 2.1.331).</summary>
        public const string SummarizedThinking = "summarized";

        private const string ThinkingDisplayOption = "--thinking-display";
        private const string ProbeArguments = ThinkingDisplayOption + " mde-probe --version";
        private const int HelpTimeoutMs = 15_000;

        private static readonly ConcurrentDictionary<string, string> _answerByBinary = new(StringComparer.Ordinal);

        /// <summary>
        /// <see cref="SummarizedThinking"/> se il CLI conosce <c>--thinking-display</c>, altrimenti null. Se l'aiuto non si
        /// legge, null e il motivo nel log: la sessione parte lo stesso, senza il testo del ragionamento.
        /// </summary>
        public static string DefaultThinkingDisplay(ILogger logger)
        {
            var answer = Ask(logger);
            if (answer == null) return null;
            // «option '--thinking-display <display>' argument 'mde-probe' is invalid. Allowed choices are summarized, …»
            if (answer.Contains(ThinkingDisplayOption, StringComparison.Ordinal) && answer.Contains(SummarizedThinking, StringComparison.Ordinal))
                return SummarizedThinking;
            logger?.LogWarning("[ClaudeCodeCliFeatures] il CLI installato non conosce {Option}: il testo del ragionamento non arriverà", ThinkingDisplayOption);
            return null;
        }

        /// <summary>Solo per i test: dimentica le risposte avute.</summary>
        internal static void Forget() => _answerByBinary.Clear();

        private static string Ask(ILogger logger)
        {
            var binary = ClaudeCodeProcessLauncher.ResolvePath();
            if (binary == null) return null;   // l'avvio vero dirà che il CLI manca, con le istruzioni
            string key;
            try { key = binary + "|" + File.GetLastWriteTimeUtc(binary).Ticks; }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) { key = binary; }

            if (_answerByBinary.TryGetValue(key, out var cached)) return cached;
            try
            {
                var psi = ClaudeCodeProcessLauncher.BuildStartInfo(ProbeArguments);
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                using var process = Process.Start(psi) ?? throw new InvalidOperationException("Process.Start ha restituito null");
                var stdout = process.StandardOutput.ReadToEndAsync();
                var stderr = process.StandardError.ReadToEndAsync();
                if (!process.WaitForExit(HelpTimeoutMs))
                {
                    try { process.Kill(entireProcessTree: true); } catch { /* già uscito */ }
                    logger?.LogWarning("[ClaudeCodeCliFeatures] `claude {Args}` non ha risposto in {Ms} ms", ProbeArguments, HelpTimeoutMs);
                    return null;
                }
                var answer = stdout.Result + "\n" + stderr.Result;
                _answerByBinary[key] = answer;
                return answer;
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "[ClaudeCodeCliFeatures] `claude {Args}` non leggibile", ProbeArguments);
                return null;
            }
        }
    }
}
