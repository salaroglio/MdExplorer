using System;
using System.Text.RegularExpressions;
using MdExplorer.Features.Services;

namespace MdExplorer.Features.Templates
{
    /// <summary>
    /// Le istruzioni di un metodo nei file d'istruzioni del progetto (<c>CLAUDE.md</c>, <c>AGENTS.md</c>,
    /// <c>.github/copilot-instructions.md</c>): un blocco per metodo fra <c>&lt;!-- mde-template:&lt;id&gt; --&gt;</c> e
    /// <c>&lt;!-- /mde-template:&lt;id&gt; --&gt;</c>. Il file è del progetto: MDE tocca solo il blocco del metodo.
    /// </summary>
    public static class TemplateInstructions
    {
        public static string Start(string id) => $"<!-- mde-template:{id} -->";
        public static string End(string id) => $"<!-- /mde-template:{id} -->";

        /// <summary>
        /// Il testo con il blocco del metodo: sostituito se c'è, in fondo se non c'è. <paramref name="text"/> null = il file non
        /// c'è ancora. Un blocco aperto e non chiuso (o chiuso due volte) è un errore che dice dove: non si indovina dove finisce.
        /// </summary>
        public static string Apply(string text, string id, string instructions)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id is required", nameof(id));
            text ??= string.Empty;
            var newline = text.Contains("\r\n") ? "\r\n" : "\n";
            var body = (instructions ?? string.Empty).Replace("\r\n", "\n").Trim('\n').Replace("\n", newline);
            var block = Start(id) + newline + body + newline + End(id);

            var starts = Regex.Matches(text, Regex.Escape(Start(id))).Count;
            var ends = Regex.Matches(text, Regex.Escape(End(id))).Count;
            if (starts == 0 && ends == 0)
            {
                if (text.Length == 0) return block + newline;
                var separator = text.EndsWith(newline + newline) ? "" : text.EndsWith(newline) ? newline : newline + newline;
                return text + separator + block + newline;
            }
            var from = text.IndexOf(Start(id), StringComparison.Ordinal);
            var to = text.IndexOf(End(id), StringComparison.Ordinal);
            if (starts != 1 || ends != 1 || to < from)
                throw new InvalidOperationException(UiText.Say(
                    $"Il blocco del metodo '{id}' non è intero: serve una riga {Start(id)} e, dopo, una riga {End(id)}. Correggilo a mano e riprova.",
                    $"The block of method '{id}' is not whole: it needs a line {Start(id)} and, after it, a line {End(id)}. Fix it by hand and try again."));
            return text.Substring(0, from) + block + text.Substring(to + End(id).Length);
        }
    }
}
