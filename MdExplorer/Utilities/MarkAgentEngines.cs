using System;

namespace MdExplorer.Utilities
{
    /// <summary>
    /// Il CLI agentico con cui MarkAgent parla in un progetto. Uno solo: i motori sono esclusivi,
    /// come l'harness.
    /// </summary>
    public enum MarkAgentEngine
    {
        /// <summary>Nessun motore: MarkAgent non si collega a nessun CLI in questo progetto.</summary>
        None = 0,

        /// <summary>GitHub Copilot CLI.</summary>
        Copilot = 1,

        /// <summary>opencode.</summary>
        OpenCode = 2,

        /// <summary>Claude Code CLI.</summary>
        Claude = 3,
    }

    /// <summary>
    /// Il motore di MarkAgent e l'harness del repository sono la stessa decisione vista da due lati:
    /// l'harness dice <b>dove</b> stanno skill, agenti e prompt (<c>.github/</c>, <c>.claude/</c>,
    /// <c>.opencode/</c>), il motore dice <b>chi</b> li legge. Tenerli separati ha prodotto progetti
    /// con le cartelle di un CLI e la chat su un altro, che quelle cartelle non guarda nemmeno.
    /// <para>
    /// Qui sta la regola, in un posto solo: <b>il motore segue l'harness</b>, a meno che questa
    /// macchina non abbia detto il contrario. Perché i due dati non possono vivere insieme —
    /// l'harness è nel <c>.development.yml</c> committato (vale per il team), il motore è in UserDB
    /// (vale per la macchina, dove è installato un CLI e non un altro).
    /// </para>
    /// <para>Sprint: docs-internal/Sprints/2026-09-13-MarkAgent-Motore-OpenCode.md, fase F1.</para>
    /// </summary>
    public static class MarkAgentEngines
    {
        public const string CopilotId = "copilot";
        public const string OpenCodeId = "opencode";
        public const string ClaudeId = "claude";
        public const string NoneId = "none";

        /// <summary>Valori ammessi per <c>Project.MarkAgentEngine</c>, da mostrare in un errore.</summary>
        public static string AllowedIds => $"{CopilotId}, {OpenCodeId}, {ClaudeId}, {NoneId}";

        public static string IdOf(MarkAgentEngine engine) => engine switch
        {
            MarkAgentEngine.Copilot => CopilotId,
            MarkAgentEngine.OpenCode => OpenCodeId,
            MarkAgentEngine.Claude => ClaudeId,
            MarkAgentEngine.None => NoneId,
            _ => throw new ArgumentOutOfRangeException(nameof(engine), engine, "Motore di MarkAgent sconosciuto."),
        };

        public static bool TryParseId(string id, out MarkAgentEngine engine)
        {
            switch ((id ?? string.Empty).Trim().ToLowerInvariant())
            {
                case CopilotId: engine = MarkAgentEngine.Copilot; return true;
                case OpenCodeId: engine = MarkAgentEngine.OpenCode; return true;
                case ClaudeId: engine = MarkAgentEngine.Claude; return true;
                case NoneId: engine = MarkAgentEngine.None; return true;
                default: engine = MarkAgentEngine.None; return false;
            }
        }

        /// <summary>
        /// Il motore che legge davvero le cartelle di un harness. Uno a uno, e non è un caso: è la
        /// ragione per cui questa coppia esiste.
        /// </summary>
        public static MarkAgentEngine FromHarness(HarnessTarget harness) => harness switch
        {
            HarnessTarget.Copilot => MarkAgentEngine.Copilot,
            HarnessTarget.OpenCode => MarkAgentEngine.OpenCode,
            HarnessTarget.Claude => MarkAgentEngine.Claude,
            HarnessTarget.None => MarkAgentEngine.None,
            _ => throw new ArgumentOutOfRangeException(nameof(harness), harness, "Harness sconosciuto."),
        };

        /// <summary>
        /// Il motore di un progetto: <paramref name="stored"/> è <c>Project.MarkAgentEngine</c>.
        /// <list type="bullet">
        /// <item><description><c>null</c>/vuoto = <b>collegato</b>: vale il motore dell'harness dichiarato.</description></item>
        /// <item><description>un valore = <b>scollegato a mano</b> su questa macchina, e vince lui.</description></item>
        /// </list>
        /// </summary>
        /// <param name="linked">
        /// true quando il motore arriva dall'harness. Serve alla UI per dire «segue l'ambiente» invece
        /// di far credere che sia una scelta separata.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Il valore salvato non è fra quelli ammessi. Non si indovina e non si torna a un default: un
        /// progetto con un motore illeggibile lo dice, con il valore trovato e quelli ammessi.
        /// </exception>
        public static MarkAgentEngine Resolve(string stored, string projectPath, out bool linked)
        {
            if (!string.IsNullOrWhiteSpace(stored))
            {
                if (!TryParseId(stored, out var chosen))
                {
                    throw new InvalidOperationException(
                        $"Motore di MarkAgent sconosciuto ('{stored}') per il progetto '{projectPath}'. " +
                        $"Valori ammessi: {AllowedIds}.");
                }
                linked = false;
                return chosen;
            }

            linked = true;
            return FromHarness(HarnessOf(projectPath));
        }

        /// <summary>
        /// Il comando da digitare in un terminale per parlare con questo motore: è anche il nome
        /// dell'eseguibile nel PATH. Serve a chi apre il CLI dell'ambiente dalla toolbar.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <see cref="MarkAgentEngine.None"/> non ha un comando: non c'è niente da aprire, e
        /// restituire una stringa vuota farebbe lanciare un processo senza nome.
        /// </exception>
        public static string CommandOf(MarkAgentEngine engine) => engine switch
        {
            MarkAgentEngine.Copilot => "copilot",
            MarkAgentEngine.OpenCode => "opencode",
            MarkAgentEngine.Claude => "claude",
            MarkAgentEngine.None => throw new InvalidOperationException(
                "Questo progetto non ha un ambiente agentico: non c'è nessun CLI da aprire."),
            _ => throw new ArgumentOutOfRangeException(nameof(engine), engine, "Motore di MarkAgent sconosciuto."),
        };

        /// <summary>
        /// L'harness del progetto: quello dichiarato nel <c>.development.yml</c> o, per un progetto
        /// nato prima che l'impostazione esistesse, quello che si vede sul disco. Stessa coppia di
        /// letture di <c>GetHarness</c>, perché due posti non devono rispondere diversamente.
        /// </summary>
        public static HarnessTarget HarnessOf(string projectPath)
            => HarnessSettings.Read(projectPath) ?? HarnessSettings.DetectFromDisk(projectPath);
    }
}
