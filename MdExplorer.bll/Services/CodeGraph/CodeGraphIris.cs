using System;
using System.IO;
using System.Text;
using MdExplorer.Features.Utilities;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Gli IRI del grafo del codice e la scrittura dei literal SPARQL. Un posto solo, perché un IRI scritto a mano in due
    /// modi diversi è un nodo che non si ritrova più. Tutti sotto <c>urn:mde:…</c>: Jena rifiuta i namespace URN di una
    /// lettera (<c>urn:s:A</c> → «Bad IRI») e l'indice li salta in silenzio (provato il 10/10).
    /// </summary>
    public static class CodeGraphIris
    {
        public const string SymbolPrefix = "urn:mde:sym:";

        /// <summary>L'hash del progetto: lo stesso che dà il nome ai DB per progetto.</summary>
        public static string ProjectHash(string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                throw new InvalidOperationException(UiText.Say("Il grafo del codice richiede un progetto aperto: projectPath è vuoto.", "The code graph needs an open project: projectPath is empty."));
            return Helper.HGetHashString(Path.GetFullPath(projectPath));
        }

        public static string DataGraph(string projectHash) => $"urn:mde:code:{projectHash}:data";
        public static string StateGraph(string projectHash) => $"urn:mde:code:{projectHash}:state";

        public static string Symbol(string id) => SymbolPrefix + id;
        public static string SymbolId(string iri)
            => iri != null && iri.StartsWith(SymbolPrefix, StringComparison.Ordinal) ? iri.Substring(SymbolPrefix.Length) : iri;

        public static string Parameter(string symbolId, int position) => $"urn:mde:sym:{symbolId}:p{position}";
        /// <summary>
        /// Il file sorgente, dal suo percorso LOGICO (<c>program/x.ps1</c>, <c>@backend/src/Foo.cs</c>). Senza l'hash del
        /// progetto (fino al 12/10 c'era): l'hash viene dal percorso assoluto del progetto, diverso su ogni macchina, e
        /// l'IRI finisce negli script Turtle che vanno su Git. Il grafo nominato è già uno per progetto.
        /// </summary>
        public static string File(string logicalPath) => $"urn:mde:file:{Enc(logicalPath)}";
        /// <summary>L'insieme delle descrizioni di un file (intestazione di uno script <c>.doc.ttl</c>).</summary>
        public static string DocSet(string logicalPath) => $"urn:mde:docs:{Enc(logicalPath)}";
        public static string Edge(string sourceId, string relation, string targetId, int? line) => $"urn:mde:edge:{sourceId}:{relation}:{targetId}:{line ?? 0}";
        public static string Doc(string symbolId) => "urn:mde:doc:" + symbolId;
        /// <summary>La scelta dell'LLM su una chiamata ambigua (F12c): una per chiamante, riga e candidata scelta.</summary>
        public static string Resolution(string sourceId, int? line, string targetId) => $"urn:mde:res:{sourceId}:{line ?? 0}:{targetId}";

        /// <summary>Il contrario di <see cref="Edge"/>: (sorgente, relazione, destinazione, riga) dall'IRI di un arco; null se non lo è.</summary>
        public static (string Source, string Relation, string Target, int? Line)? ParseEdge(string iri)
        {
            const string prefix = "urn:mde:edge:";
            if (iri == null || !iri.StartsWith(prefix, StringComparison.Ordinal)) return null;
            var parts = iri.Substring(prefix.Length).Split(':');
            if (parts.Length != 4 || !int.TryParse(parts[3], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var line)) return null;
            return (parts[0], parts[1], parts[2], line == 0 ? null : line);
        }
        public static string FileDoc(string logicalPath) => $"urn:mde:fdoc:{Enc(logicalPath)}";
        public static string ParamDoc(string symbolId, string parameterName) => $"urn:mde:pdoc:{symbolId}:{Enc(parameterName)}";
        public static string Script(string projectHash, string scriptRelativePath) => $"urn:mde:script:{projectHash}:{Enc(scriptRelativePath)}";
        public static string Scan(string projectHash, string programFolder) => $"urn:mde:scan:{projectHash}:{Enc(programFolder)}";
        public static string DescriptionRun(string projectHash, string programFolder) => $"urn:mde:descrun:{projectHash}:{Enc(programFolder)}";

        /// <summary>La proprietà diretta di una relazione: <c>code:calls</c>, <c>code:implements</c>…</summary>
        public static string RelationProperty(string relation) => "code:" + relation;
        public static string RelationIri(string relation) => CodeOntology.Namespace + relation;

        /// <summary>Un segmento di IRI da un testo libero: percent-encoding, così «src/Demo/Foo.cs» resta leggibile e univoco.</summary>
        public static string Enc(string value) => Uri.EscapeDataString(value ?? string.Empty);

        public static string Iri(string iri) => "<" + iri + ">";

        /// <summary>Un literal stringa SPARQL/Turtle con gli escape giusti.</summary>
        public static string Lit(string value)
        {
            var sb = new StringBuilder(value.Length + 2);
            sb.Append('"');
            foreach (var ch in value)
            {
                switch (ch)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '"': sb.Append("\\\""); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default: sb.Append(ch); break;
                }
            }
            sb.Append('"');
            return sb.ToString();
        }

        public static string Int(int value) => value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        public static string Bool(bool value) => value ? "true" : "false";
    }
}
