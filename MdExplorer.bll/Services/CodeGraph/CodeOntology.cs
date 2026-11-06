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
        /// <summary>A che cosa serve un file (12/10/2026): <c>code:describes</c> punta al <c>code:File</c>.</summary>
        public const string ClassFileDescription = "code:FileDescription";
        public const string ClassScript = "code:Script";
        /// <summary>La scelta dell'LLM su una chiamata ambigua (F12c): <c>code:chooses</c> l'arco giusto, <c>code:rejects</c> gli altri.</summary>
        public const string ClassCallResolution = "code:CallResolution";
        /// <summary>La certezza di un arco ambiguo scelto dall'LLM: mai confusa con quelle del parser.</summary>
        public const string LlmResolved = "llm-resolved";
        /// <summary>
        /// Un punto di chiamata con argomenti stringa letterali (D42, 23/10/2026): il fatto che leggono le regole di
        /// arricchimento (<c>setX("nomeMetodo")</c>). Le stringhe stanno nei nodi <c>code:StringArgument</c>.
        /// </summary>
        public const string ClassCallSite = "code:CallSite";
        public const string ClassStringArgument = "code:StringArgument";
        /// <summary>Un argomento <c>X.class</c> di un punto di chiamata, col tipo risolto in <c>code:typeName</c> (D43, 30/10/2026).</summary>
        public const string ClassTypeArgument = "code:TypeArgument";
        /// <summary>Un'annotazione su un simbolo (<c>@Provides</c>, <c>@ImplementedBy(Y.class)</c>), per le regole di registrazione (D43).</summary>
        public const string ClassAnnotation = "code:Annotation";
        public const string ClassAnnotationArgument = "code:AnnotationArgument";
        /// <summary>La descrizione di un collegamento fatto da una regola, per punto (file:riga), dalla domanda <c>describe</c> (D43).</summary>
        public const string ClassLinkDescription = "code:LinkDescription";
        /// <summary>L'insieme degli archi di una regola di arricchimento (D42): id, descrizione, chi l'ha scritta.</summary>
        public const string ClassRuleSet = "code:RuleSet";
        /// <summary>La certezza degli archi delle regole: dedotti da una convenzione dichiarata, non letti dal parser.</summary>
        public const string RuleConfidence = "rule";
        public const string ClassScanState = "code:ScanState";
        /// <summary>La traccia dell'ultima descrizione di una cartella program (punto 2c del 14/10): da qui si sa se si è interrotta.</summary>
        public const string ClassDescriptionRun = "code:DescriptionRun";
    }
}
