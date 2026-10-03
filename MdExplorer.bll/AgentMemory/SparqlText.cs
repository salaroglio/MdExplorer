using System;
using System.Globalization;
using VDS.RDF;
using VDS.RDF.Writing.Formatting;

namespace MdExplorer.Features.AgentMemory
{
    /// <summary>
    /// Serializzazione sicura dei termini SPARQL (Fase 5a). L'escaping delle literal è
    /// delegato a dotNetRDF (<see cref="SparqlFormatter"/>) — decisione di piano: non
    /// reinventarlo. Ogni valore che finisce in una query/update passa da qui; niente
    /// concatenazione diretta di input nel testo SPARQL.
    /// </summary>
    public static class SparqlText
    {
        private static readonly NodeFactory Factory = new NodeFactory(new NodeFactoryOptions());
        private static readonly SparqlFormatter Formatter = new SparqlFormatter();

        /// <summary>Literal stringa quotata ed escapata (es. <c>"riga1\nriga2"</c>).</summary>
        public static string Literal(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            lock (Factory)
                return Formatter.Format(Factory.CreateLiteralNode(value));
        }

        /// <summary>Literal xsd:double invariant-culture (confidence e simili).</summary>
        public static string Double(double value)
            => value.ToString("0.0###", CultureInfo.InvariantCulture);

        /// <summary>Literal xsd:dateTime UTC (provenance temporale).</summary>
        public static string DateTime(DateTime utc)
            => $"\"{utc.ToUniversalTime():yyyy-MM-dd'T'HH:mm:ss.fff'Z'}\"^^<http://www.w3.org/2001/XMLSchema#dateTime>";

        /// <summary>IRI fra angolari, validato assoluto — un URI malformato è un errore, non testo.</summary>
        public static string Iri(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri) || !Uri.IsWellFormedUriString(uri, UriKind.Absolute))
                throw new ArgumentException($"IRI non valido: '{uri}'.", nameof(uri));
            return $"<{uri}>";
        }
    }
}
