using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>Una query di MarkAgent rifiutata prima di arrivare a Fuseki: il messaggio dice perché e come rimediare.</summary>
    public sealed class CodeSparqlException : Exception
    {
        public CodeSparqlException(string message) : base(message) { }
    }

    /// <summary>L'esito di <see cref="CodeSparql"/>: le colonne e le righe (al massimo il limite chiesto), o il sì/no di un ASK.</summary>
    public sealed class CodeSparqlResult
    {
        public List<string> Columns { get; set; } = new();
        public List<Dictionary<string, string>> Rows { get; set; } = new();
        /// <summary>Le righe che Fuseki ha dato; più di <see cref="Rows"/> quando <see cref="Truncated"/>.</summary>
        public int RowCount { get; set; }
        public bool Truncated { get; set; }
        /// <summary>La risposta di un ASK; null per una SELECT.</summary>
        public bool? Boolean { get; set; }
        /// <summary>La query come è arrivata a Fuseki, con i prefissi e il GRAPH.</summary>
        public string Query { get; set; }
    }

    /// <summary>
    /// SPARQL scritto da MarkAgent sul grafo del codice (12/11/2026, utente: «dovrebbe usare sparql solo quando i tool non hanno
    /// direttamente la soluzione alla domanda»). Lo schema lo legge con <c>code_ontology</c>. Qui si controlla che la query
    /// sia di sola lettura e che legga solo il grafo dati del progetto:
    /// <list type="bullet">
    /// <item>solo <c>SELECT</c> e <c>ASK</c> (e comunque va all'endpoint <c>/query</c> di Fuseki, che non esegue update);</item>
    /// <item>niente <c>GRAPH</c>, <c>FROM</c>, <c>SERVICE</c>: il grafo lo sceglie MdExplorer, racchiudendo il blocco del <c>WHERE</c> in
    /// <c>GRAPH &lt;grafo dati&gt; { … }</c> come fanno le altre query (fuori da GRAPH jena-text vede solo il grafo di default);</item>
    /// <item>i prefissi (<see cref="Prefixes"/>) ci sono già.</item>
    /// </list>
    /// Il tempo e le righe hanno un tetto (<see cref="Timeout"/>, <see cref="MaxRows"/>).
    /// </summary>
    public static class CodeSparql
    {
        public const int DefaultRows = 100;
        public const int MaxRows = 1000;
        public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(60);

        /// <summary>I prefissi già dichiarati: quelli dell'ontologia più rdf, rdfs, owl.</summary>
        public const string Prefixes = CodeOntology.SparqlPrefixes +
            "PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>\n" +
            "PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>\n" +
            "PREFIX owl: <http://www.w3.org/2002/07/owl#>\n";

        private static readonly TimeSpan RegexLimit = TimeSpan.FromSeconds(1);
        private static readonly Regex Form = new(@"(?<![?$:\w])(SELECT|ASK|CONSTRUCT|DESCRIBE)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, RegexLimit);
        private static readonly Regex Forbidden = new(@"(?<![?$:\w-])(GRAPH|FROM|NAMED|SERVICE|INSERT|DELETE|LOAD|CLEAR|DROP|CREATE|COPY|MOVE|ADD)\b(?![:\w-])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, RegexLimit);

        /// <summary>La query pronta per Fuseki: prefissi dell'ontologia, poi la query con <c>FROM &lt;grafo&gt;</c>. Torna anche se è un <c>ASK</c>.</summary>
        public static (string Query, bool Ask) Prepare(string query, string dataGraph)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new CodeSparqlException(UiText.Say("la query è vuota", "the query is empty"));
            var masked = Mask(query);
            var form = Form.Match(masked);
            if (!form.Success)
                throw new CodeSparqlException(UiText.Say("serve una query SELECT o ASK", "a SELECT or ASK query is needed"));
            var kind = form.Value.ToUpperInvariant();
            if (kind is "CONSTRUCT" or "DESCRIBE")
                throw new CodeSparqlException(UiText.Say($"{kind} non è ammesso: scrivi una SELECT con le colonne che ti servono", $"{kind} is not allowed: write a SELECT with the columns you need"));
            var bad = Forbidden.Match(masked);
            if (bad.Success)
                throw new CodeSparqlException(UiText.Say(
                    $"'{bad.Value.ToUpperInvariant()}' non è ammesso: la query è di sola lettura e legge già il grafo del progetto, scrivi i pattern senza GRAPH né FROM",
                    $"'{bad.Value.ToUpperInvariant()}' is not allowed: the query is read-only and already reads the project's graph, write the patterns without GRAPH or FROM"));
            var open = masked.IndexOf('{', form.Index);
            if (open < 0)
                throw new CodeSparqlException(UiText.Say("manca il blocco { … } dei pattern", "the { … } block of patterns is missing"));
            var close = Matching(masked, open);
            if (close < 0)
                throw new CodeSparqlException(UiText.Say("le parentesi graffe non si chiudono", "the curly braces are not balanced"));
            // il blocco del WHERE dentro GRAPH <grafo dati>: le sotto-query e gli OPTIONAL leggono lo stesso grafo
            var sb = new StringBuilder(Prefixes);
            sb.Append(query, 0, open + 1).Append(" GRAPH <").Append(dataGraph).Append("> { ")
              .Append(query, open + 1, close - open - 1).Append(" } ").Append(query, close, query.Length - close);
            return (sb.ToString(), kind == "ASK");
        }

        private static int Matching(string masked, int open)
        {
            var depth = 0;
            for (var i = open; i < masked.Length; i++)
            {
                if (masked[i] == '{') depth++;
                else if (masked[i] == '}' && --depth == 0) return i;
            }
            return -1;
        }

        /// <summary>
        /// La query con stringhe, IRI e commenti coperti da spazi, alla stessa lunghezza: le parole chiave si cercano solo nel
        /// codice, e le posizioni valgono anche per la query vera.
        /// </summary>
        internal static string Mask(string q)
        {
            var c = q.ToCharArray();
            for (var i = 0; i < c.Length; i++)
            {
                var ch = c[i];
                if (ch == '#') { while (i < c.Length && c[i] != '\n') c[i++] = ' '; continue; }
                if (ch == '"' || ch == '\'')
                {
                    var triple = i + 2 < c.Length && c[i + 1] == ch && c[i + 2] == ch;
                    var len = triple ? 3 : 1;
                    var j = i + len;
                    while (j < c.Length)
                    {
                        if (c[j] == '\\') { j += 2; continue; }
                        if (c[j] == ch && (!triple || (j + 2 < c.Length && c[j + 1] == ch && c[j + 2] == ch))) break;
                        if (!triple && c[j] == '\n') break;
                        j++;
                    }
                    var end = Math.Min(c.Length, j + len);
                    for (var k = i; k < end; k++) if (c[k] != '\n') c[k] = ' ';
                    i = end - 1;
                    continue;
                }
                if (ch == '<')
                {
                    // un IRI è <…> senza spazi; «?a < ?b» resta un confronto
                    var j = i + 1;
                    while (j < c.Length && c[j] != '>' && !char.IsWhiteSpace(c[j]) && c[j] != '<') j++;
                    if (j < c.Length && c[j] == '>') { for (var k = i; k <= j; k++) c[k] = ' '; i = j; }
                }
            }
            return new string(c);
        }
    }
}
