using System;
using System.Collections.Generic;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// I punti di chiamata con stringhe letterali (D42, 23/10/2026), comuni agli estrattori: quali stringhe valgono e come
    /// si numerano le chiamate della stessa riga. Una stringa vale se ha la forma di un NOME (metodo, template,
    /// <c>alias.metodo</c>, <c>redirect:pagina</c>, un percorso): niente spazi, al più <see cref="MaxValueLength"/>
    /// caratteri. Le frasi sono messaggi di log ed eccezioni: registrarle gonfierebbe il grafo senza dare niente alle regole.
    /// </summary>
    public sealed class CodeCallSites
    {
        public const int MaxValueLength = 200;
        public const int MaxReceiverLength = 200;

        private readonly CodeScript _script;
        private readonly Dictionary<(string, int), int> _ordinals = new();

        public CodeCallSites(CodeScript script) { _script = script; }

        public static bool IsNameLike(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length > MaxValueLength) return false;
            foreach (var ch in value)
                if (char.IsWhiteSpace(ch) || char.IsControl(ch)) return false;
            return true;
        }

        /// <summary>
        /// Registra la chiamata se almeno un argomento è una stringa a forma di nome. <paramref name="arguments"/> sono le
        /// stringhe già senza virgolette per posizione (null = l'argomento non è una stringa letterale). Con
        /// <paramref name="always"/> la registra anche senza stringhe: il JS di un controllo di una pagina (D42 passo 2), dove
        /// conta già sapere QUALE funzione chiama.
        /// </summary>
        public void Add(string sourceId, int line, string callee, string receiver, IReadOnlyList<string> arguments, bool always = false)
        {
            if (sourceId == null || string.IsNullOrEmpty(callee) || arguments == null) return;
            ScriptCallSite site = null;
            ScriptCallSite New()
            {
                _ordinals.TryGetValue((sourceId, line), out var ordinal);
                _ordinals[(sourceId, line)] = ordinal + 1;
                return new ScriptCallSite
                {
                    SourceId = sourceId, Line = line, Ordinal = ordinal, Callee = callee, ArgumentCount = arguments.Count,
                    Receiver = receiver != null && receiver.Length <= MaxReceiverLength ? receiver : null,
                };
            }
            for (var i = 0; i < arguments.Count; i++)
            {
                if (!IsNameLike(arguments[i])) continue;
                site ??= New();
                site.Arguments.Add(new ScriptStringArgument { Position = i, Value = arguments[i] });
            }
            if (site == null && always) site = New();
            if (site != null) _script.CallSites.Add(site);
        }
    }
}
