namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// L'ontologia <c>mde-code</c> (D19): i nomi che il caricatore scrive in Fuseki e che le query leggono.
    /// Il namespace segue quello della memoria degli agenti (<c>https://mdexplorer.net/ns/agent#</c>).
    /// In F7a servono solo i campi dell'indice a testo pieno; F7b aggiunge il resto.
    /// </summary>
    public static class CodeOntology
    {
        public const string Namespace = "https://mdexplorer.net/ns/code#";
        public const string Prefix = "code";

        // Campi indicizzati da jena-text (entity map in FusekiManagedConfig).
        public const string Name = Namespace + "name";
        public const string QualifiedName = Namespace + "qualifiedName";
        public const string Summary = Namespace + "summary";
        public const string Description = Namespace + "description";

        /// <summary>Nome del dataset Fuseki del grafo del codice (uno per installazione, un grafo per cartella).</summary>
        public const string Dataset = "code";
    }
}
