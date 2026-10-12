namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// L'ontologia <c>mde-code</c> (D19): i nomi che il caricatore scrive in Fuseki e che le query leggono. Il namespace
    /// segue quello della memoria degli agenti (<c>https://mdexplorer.net/ns/agent#</c>). Nelle query si usa il prefisso
    /// <c>code:</c>; qui stanno le costanti con l'IRI intero dove serve fuori dal SPARQL (configurazione di jena-text).
    /// Le proprietà degli archi diretti hanno il nome della relazione (<c>code:calls</c>, <c>code:implements</c>…).
    /// </summary>
    public static class CodeOntology
    {
        public const string Namespace = "https://mdexplorer.net/ns/code#";
        public const string Prefix = "code";

        /// <summary>Nome del dataset Fuseki del grafo del codice (uno per installazione, un grafo nominato per progetto).</summary>
        public const string Dataset = "code";

        /// <summary>Intestazione dei PREFIX per ogni query e update.</summary>
        public const string SparqlPrefixes =
            "PREFIX code: <" + Namespace + ">\n" +
            "PREFIX text: <http://jena.apache.org/text#>\n" +
            "PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>\n";

        // Campi indicizzati da jena-text (entity map in FusekiManagedConfig): IRI interi.
        public const string Name = Namespace + "name";
        public const string QualifiedName = Namespace + "qualifiedName";
        public const string Summary = Namespace + "summary";
        public const string Description = Namespace + "description";

        // Classi
        public const string ClassSymbol = "code:Symbol";
        public const string ClassParameter = "code:Parameter";
        public const string ClassEdge = "code:Edge";
        public const string ClassFile = "code:File";
        public const string ClassDescription = "code:Description";
        public const string ClassParameterDescription = "code:ParameterDescription";
        public const string ClassDescriptionSet = "code:DescriptionSet";
        public const string ClassScript = "code:Script";
        public const string ClassScanState = "code:ScanState";
    }
}
