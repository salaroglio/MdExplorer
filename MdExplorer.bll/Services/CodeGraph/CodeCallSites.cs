using System;
using System.Collections.Generic;
using System.Linq;

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
        /// Da qui si numerano i punti che hanno SOLO argomenti di tipo (D43, 30/10/2026): i numeri dei punti con stringhe
        /// restano quelli di prima, e con loro gli IRI che le regole D42 già conoscono.
        /// </summary>
        public const int TypeOnlyOrdinalBase = 1000;

        private readonly Dictionary<(string, int), int> _typeOnlyOrdinals = new();

        /// <summary>
        /// Registra la chiamata se almeno un argomento è una stringa a forma di nome o, con <paramref name="types"/> (D43),
        /// un tipo (<c>X.class</c>). <paramref name="arguments"/> sono le stringhe già senza virgolette per posizione (null =
        /// l'argomento non è una stringa letterale); <paramref name="types"/> i tipi risolti per posizione (null = non è un
        /// tipo). Con <paramref name="always"/> la registra anche senza: il JS di un controllo di una pagina (D42 passo 2), dove
        /// conta già sapere QUALE funzione chiama. Torna il punto registrato, o null.
        /// </summary>
        public ScriptCallSite Add(string sourceId, int line, string callee, string receiver, IReadOnlyList<string> arguments, bool always = false,
            IReadOnlyList<ScriptTypeArgument> types = null)
        {
            if (sourceId == null || string.IsNullOrEmpty(callee) || arguments == null) return null;
            var hasString = false;
            for (var i = 0; i < arguments.Count; i++) hasString |= IsNameLike(arguments[i]);
            var hasType = types != null && types.Any(t => t != null);
            if (!hasString && !hasType && !always) return null;
            int ordinal;
            if (hasString || always)
            {
                _ordinals.TryGetValue((sourceId, line), out ordinal);
                _ordinals[(sourceId, line)] = ordinal + 1;
            }
            else
            {
                _typeOnlyOrdinals.TryGetValue((sourceId, line), out var n);
                _typeOnlyOrdinals[(sourceId, line)] = n + 1;
                ordinal = TypeOnlyOrdinalBase + n;
            }
            var site = new ScriptCallSite
            {
                SourceId = sourceId, Line = line, Ordinal = ordinal, Callee = callee, ArgumentCount = arguments.Count,
                Receiver = receiver != null && receiver.Length <= MaxReceiverLength ? receiver : null,
            };
            for (var i = 0; i < arguments.Count; i++)
                if (IsNameLike(arguments[i])) site.Arguments.Add(new ScriptStringArgument { Position = i, Value = arguments[i] });
            if (types != null)
                foreach (var t in types.Where(t => t != null)) site.TypeArguments.Add(t);
            _script.CallSites.Add(site);
            return site;
        }
    }
}
