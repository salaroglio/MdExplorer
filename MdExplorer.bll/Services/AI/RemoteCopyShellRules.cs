using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace MdExplorer.Features.Services.AI
{
    /// <summary>
    /// I comandi di shell che raggiungono un server da soli: ssh, scp, sftp, rsync. In MDE i file delle radici remote passano
    /// SOLO da MDE Remote (remote_read, remote_edit, remote_write), che tiene la codifica dell'estensione (CP-850…): con la
    /// shell un modello li riscrive in UTF-8 senza che nessuno se ne accorga (26/11/2026, con Copilot). Lo stesso divieto, nella
    /// forma di ogni motore, tutte provate dal vivo anche su «echo ciao &amp;&amp; ssh -V»:
    /// <list type="bullet">
    /// <item>Copilot CLI 1.0.114: <c>--deny-tool=shell(ssh)</c>; sul trasporto SDK la politica dei permessi;</item>
    /// <item>Claude Code 2.1.332: <c>--disallowedTools "Bash(ssh:*)"</c>;</item>
    /// <item>opencode 1.18.30: <c>permission.bash</c> <c>{"ssh": "deny", "ssh *": "deny"}</c>, solo i divieti (vince l'ultima
    /// regola che combacia: il resto della configurazione dell'utente resta com'è).</item>
    /// </list>
    /// </summary>
    public static class RemoteCopyShellRules
    {
        public static readonly IReadOnlyList<string> Commands = new[] { "ssh", "scp", "sftp", "rsync" };

        /// <summary>Per Copilot CLI (<c>--deny-tool</c>).</summary>
        public static IEnumerable<string> Copilot => Commands.Select(c => $"shell({c})");

        /// <summary>Per Claude Code (<c>--disallowedTools</c>).</summary>
        public static IEnumerable<string> Claude => Commands.Select(c => $"Bash({c}:*)");

        /// <summary>
        /// Per Codex (28/11/2026): Codex chiede il permesso per ogni comando con la riga intera (<c>/bin/bash -lc "scp a b"</c>),
        /// e MdExplorer risponde. Vero se uno dei comandi compare come comando, non dentro una parola o un percorso.
        /// </summary>
        public static bool IsRemoteCopy(string commandLine)
            => !string.IsNullOrWhiteSpace(commandLine) && CodexCommand.IsMatch(commandLine);

        private static readonly System.Text.RegularExpressions.Regex CodexCommand = new(
            @"(?:^|[\s;&|(""'`])(?:\S*/)?(?:" + string.Join("|", Commands) + @")(?=$|[\s;&|)""'`])",
            System.Text.RegularExpressions.RegexOptions.Compiled);

        /// <summary>Per opencode: le regole di <c>permission.bash</c>, il comando da solo e con argomenti.</summary>
        public static IEnumerable<KeyValuePair<string, string>> OpenCode
            => Commands.SelectMany(c => new[] { new KeyValuePair<string, string>(c, "deny"), new KeyValuePair<string, string>(c + " *", "deny") });

        /// <summary>
        /// <c>OPENCODE_CONFIG_CONTENT</c> del server della chat: i divieti, che si sommano alla configurazione dell'utente, e il
        /// web su «ask». Il server è uno per tutti i progetti, mentre l'accesso al web si sceglie per progetto: così ogni domanda
        /// arriva alla sessione, che risponde con la scelta del suo progetto (<c>OpenCodeSession.ChatReply</c>). Senza, il
        /// default di opencode («allow») apriva il web a ogni chat.
        /// </summary>
        public static string OpenCodeChatConfig()
        {
            var bash = new JsonObject();
            foreach (var rule in OpenCode) bash[rule.Key] = rule.Value;
            var permission = new JsonObject { ["bash"] = bash };
            foreach (var web in MdExplorer.Features.Services.AI.OpenCode.OpenCodeSession.WebPermissions) permission[web] = "ask";
            // Le scritture su «ask»: la sessione rifiuta quelle sulle radici di codice (si cambiano con remote_edit/remote_write,
            // che ne tengono l'originale per la revisione) e approva le altre — sprint 2026-11-27-Modifiche-Da-Approvare, D4.
            permission["edit"] = "ask";
            return new JsonObject { ["permission"] = permission }.ToJsonString();
        }
    }
}
