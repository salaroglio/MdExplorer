using System.IO;
using System.Reflection;

namespace MdExplorer.Features.AgentMemory
{
    /// <summary>
    /// L'ontologia <c>mde-agent:</c> v0 (§11.2), embedded come risorsa Turtle e caricata nel
    /// dataset al primo uso. Prefissi e IRI qui sono l'<b>unico punto di verità</b> per i
    /// builder SPARQL: cambiarli qui li cambia ovunque.
    /// </summary>
    public static class AgentMemoryOntology
    {
        public const string Prefix = "https://mdexplorer.net/ns/agent#";
        public const string ProvPrefix = "http://www.w3.org/ns/prov#";

        /// <summary>Grafo dove vive la TBox (separato dai grafi di memoria).</summary>
        public const string OntologyGraph = "urn:mde:mem:ontology";

        private static string _cached;

        /// <summary>Il Turtle dell'ontologia (embedded resource <c>mde-agent.ttl</c>).</summary>
        public static string Turtle()
        {
            if (_cached != null) return _cached;
            var asm = Assembly.GetExecutingAssembly();
            // Il nome della risorsa termina con "mde-agent.ttl" a prescindere dal default namespace.
            var name = null as string;
            foreach (var r in asm.GetManifestResourceNames())
                if (r.EndsWith("mde-agent.ttl", System.StringComparison.OrdinalIgnoreCase)) { name = r; break; }
            if (name == null)
                throw new FileNotFoundException("Risorsa embedded 'mde-agent.ttl' non trovata nell'assembly.");
            using var stream = asm.GetManifestResourceStream(name);
            using var reader = new StreamReader(stream);
            _cached = reader.ReadToEnd();
            return _cached;
        }
    }
}
