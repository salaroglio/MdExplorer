using System;
using System.IO;
using System.Text;
using MdExplorer.Features.Services.CodeGraph;

namespace MdExplorer.Features.Services.Fuseki
{
    /// <summary>
    /// Scrive in <c>FUSEKI_BASE/configuration/code.ttl</c> l'assembler del dataset del grafo del codice:
    /// TDB2 + jena-text (Lucene) con l'entity map sui campi cercabili. Fuseki legge la cartella
    /// <c>configuration/</c> solo all'avvio e l'admin API non accetta assembler (provato il 10/10),
    /// per questo il file va scritto PRIMA di avviare il processo. La prima riga porta la versione:
    /// se cambia l'entity map si alza <see cref="ConfigVersion"/>, il file viene riscritto e l'indice
    /// Lucene va rifatto («Rigenera la conoscenza»).
    /// </summary>
    public static class FusekiManagedConfig
    {
        public const int ConfigVersion = 1;
        public const string CodeDataset = CodeOntology.Dataset;
        private const string VersionMarker = "# mde-config: ";

        public sealed class WriteResult
        {
            public string Path { get; set; }
            public bool Changed { get; set; }
        }

        public static string ConfigurationPath(string dataDirectory)
            => System.IO.Path.Combine(dataDirectory, "configuration", CodeDataset + ".ttl");

        public static WriteResult WriteCodeDataset(string dataDirectory)
        {
            var path = ConfigurationPath(dataDirectory);
            var content = Render(dataDirectory);
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            if (File.Exists(path) && File.ReadAllText(path) == content)
                return new WriteResult { Path = path, Changed = false };
            File.WriteAllText(path, content, new UTF8Encoding(false));
            return new WriteResult { Path = path, Changed = true };
        }

        /// <summary>Versione letta dalla prima riga del file, o <c>null</c> se il file manca o non è nostro.</summary>
        public static int? ReadVersion(string dataDirectory)
        {
            var path = ConfigurationPath(dataDirectory);
            if (!File.Exists(path)) return null;
            using var reader = new StreamReader(path);
            var first = reader.ReadLine();
            if (first == null || !first.StartsWith(VersionMarker, StringComparison.Ordinal)) return null;
            return int.TryParse(first.Substring(VersionMarker.Length).Trim(), out var v) ? v : null;
        }

        public static string Render(string dataDirectory)
        {
            // Percorsi: TDB2 vuole una stringa di percorso; Lucene un IRI file:. Barre in avanti su ogni sistema.
            var tdbLocation = System.IO.Path.Combine(dataDirectory, "databases", CodeDataset).Replace('\\', '/');
            var luceneUri = new Uri(System.IO.Path.Combine(dataDirectory, "lucene", CodeDataset)).AbsoluteUri;

            var sb = new StringBuilder();
            sb.Append(VersionMarker).Append(ConfigVersion).Append('\n');
            sb.Append("# Scritto da MdExplorer: dataset del grafo del codice (D19). Non modificare a mano: viene riscritto.\n");
            sb.Append("@prefix :       <#> .\n");
            sb.Append("@prefix fuseki: <http://jena.apache.org/fuseki#> .\n");
            sb.Append("@prefix rdf:    <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .\n");
            sb.Append("@prefix tdb2:   <http://jena.apache.org/2016/tdb#> .\n");
            sb.Append("@prefix text:   <http://jena.apache.org/text#> .\n");
            sb.Append("@prefix ").Append(CodeOntology.Prefix).Append(":   <").Append(CodeOntology.Namespace).Append("> .\n\n");

            sb.Append(":service rdf:type fuseki:Service ;\n");
            sb.Append("    fuseki:name \"").Append(CodeDataset).Append("\" ;\n");
            sb.Append("    fuseki:endpoint [ fuseki:operation fuseki:query  ; fuseki:name \"query\"  ] ;\n");
            sb.Append("    fuseki:endpoint [ fuseki:operation fuseki:update ; fuseki:name \"update\" ] ;\n");
            sb.Append("    fuseki:endpoint [ fuseki:operation fuseki:gsp-rw ; fuseki:name \"data\"   ] ;\n");
            sb.Append("    fuseki:dataset :textDataset .\n\n");

            sb.Append(":textDataset rdf:type text:TextDataset ;\n");
            sb.Append("    text:dataset :tdb ;\n");
            sb.Append("    text:index :lucene .\n\n");

            sb.Append(":tdb rdf:type tdb2:DatasetTDB2 ;\n");
            sb.Append("    tdb2:location \"").Append(tdbLocation).Append("\" .\n\n");

            sb.Append(":lucene rdf:type text:TextIndexLucene ;\n");
            sb.Append("    text:directory <").Append(luceneUri).Append("> ;\n");
            sb.Append("    text:entityMap :entityMap .\n\n");

            // graphField: le query stanno dentro GRAPH <cartella program>, l'indice deve saperlo.
            sb.Append(":entityMap rdf:type text:EntityMap ;\n");
            sb.Append("    text:defaultField \"name\" ;\n");
            sb.Append("    text:entityField \"uri\" ;\n");
            sb.Append("    text:graphField \"graph\" ;\n");
            sb.Append("    text:map (\n");
            sb.Append("        [ text:field \"name\"             ; text:predicate <").Append(CodeOntology.Name).Append("> ]\n");
            sb.Append("        [ text:field \"qualifiedName\"    ; text:predicate <").Append(CodeOntology.QualifiedName).Append("> ]\n");
            sb.Append("        [ text:field \"summary\"          ; text:predicate <").Append(CodeOntology.Summary).Append("> ]\n");
            sb.Append("        [ text:field \"paramDescription\" ; text:predicate <").Append(CodeOntology.Description).Append("> ]\n");
            sb.Append("    ) .\n");
            return sb.ToString();
        }
    }
}
