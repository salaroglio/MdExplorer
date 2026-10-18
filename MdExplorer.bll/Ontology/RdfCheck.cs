using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using ShapesGraph = VDS.RDF.Shacl.ShapesGraph;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace MdExplorer.Features.Ontology
{
    /// <summary>
    /// La verifica di un'ontologia sui file del progetto (P6 dei template di progetto): i <c>.ttl</c> si leggono, i dati si
    /// validano con le regole SHACL, le query <c>.rq</c> (le domande di competenza) si eseguono sui dati. Tutto in memoria con
    /// dotNetRDF: niente Fuseki, stesso risultato su ogni computer. Restituisce i fatti; giudicare se una risposta è quella
    /// attesa resta a chi ha scritto la domanda.
    /// </summary>
    public static class RdfCheck
    {
        /// <summary>Le righe di una SELECT riportate al massimo: il conteggio vero c'è sempre.</summary>
        public const int MaxRows = 50;

        /// <param name="projectRoot">La cartella del progetto.</param>
        /// <param name="data">File <c>.ttl</c> o cartelle (tutti i <c>.ttl</c> dentro): lo schema e le istanze, uniti in un grafo.</param>
        /// <param name="shapes">File <c>.ttl</c> o cartelle con le regole SHACL; tolti dai dati se compaiono anche lì.</param>
        /// <param name="queries">File <c>.rq</c> o cartelle (tutti i <c>.rq</c> dentro): le domande di competenza.</param>
        /// <exception cref="ArgumentException">Un percorso fuori dal progetto, che non c'è, o nessun dato.</exception>
        public static RdfCheckReport Check(string projectRoot, IEnumerable<string> data, IEnumerable<string> shapes = null, IEnumerable<string> queries = null)
        {
            if (string.IsNullOrWhiteSpace(projectRoot) || !Directory.Exists(projectRoot))
                throw new ArgumentException(UiText.Say($"La cartella del progetto '{projectRoot}' non esiste.", $"The project folder '{projectRoot}' does not exist."));
            var root = Path.GetFullPath(projectRoot);

            var shapeFiles = Expand(root, shapes, "*.ttl", "shapes");
            var dataFiles = Expand(root, data, "*.ttl", "data").Where(f => !shapeFiles.Contains(f)).ToList();
            var queryFiles = Expand(root, queries, "*.rq", "queries");
            if (dataFiles.Count == 0)
                throw new ArgumentException(UiText.Say("Nessun file .ttl da verificare in 'data'.", "No .ttl file to check in 'data'."));

            var report = new RdfCheckReport();
            var dataGraph = new Graph();
            foreach (var file in dataFiles) report.Files.Add(Load(root, file, dataGraph, "data"));

            Graph shapesGraph = null;
            if (shapeFiles.Count > 0)
            {
                shapesGraph = new Graph();
                foreach (var file in shapeFiles) report.Files.Add(Load(root, file, shapesGraph, "shapes"));
            }

            // Con un file che non si legge i dati sono incompleti: SHACL e query direbbero cose false.
            if (report.Files.Any(f => f.Error != null))
            {
                report.Note = UiText.Say("Un file non si legge: SHACL e query non sono stati eseguiti.", "A file cannot be read: SHACL and the queries were not run.");
                return report;
            }

            var formatter = new TurtleFormatter(dataGraph.NamespaceMap);
            if (shapesGraph != null)
            {
                var validation = new ShapesGraph(shapesGraph).Validate(dataGraph);
                report.Shacl = new RdfShaclResult
                {
                    Conforms = validation.Conforms,
                    Violations = validation.Results.Select(r => new RdfShaclViolation
                    {
                        Severity = Short(r.Severity, formatter),
                        FocusNode = Short(r.FocusNode, formatter),
                        Path = r.ResultPath?.ToString(),
                        Value = Short(r.ResultValue, formatter),
                        Message = (r.Message as ILiteralNode)?.Value ?? Short(r.Message, formatter),
                        Shape = Short(r.SourceShape, formatter),
                    }).ToList(),
                };
            }

            var processor = new LeviathanQueryProcessor(new InMemoryDataset(dataGraph));
            foreach (var file in queryFiles)
                report.Queries.Add(Run(root, file, processor, formatter));

            return report;
        }

        private static RdfFileResult Load(string root, string file, Graph target, string role)
        {
            var result = new RdfFileResult { Path = Relative(root, file), Role = role };
            try
            {
                var g = new Graph();
                new TurtleParser().Load(g, file);
                result.Triples = g.Triples.Count;
                target.Merge(g);
                foreach (var prefix in g.NamespaceMap.Prefixes)
                    if (!target.NamespaceMap.HasNamespace(prefix)) target.NamespaceMap.AddNamespace(prefix, g.NamespaceMap.GetNamespaceUri(prefix));
            }
            catch (RdfParseException ex)
            {
                result.Error = ex.HasPositionInformation
                    ? UiText.Say($"riga {ex.StartLine}, colonna {ex.StartPosition}: {ex.Message}", $"line {ex.StartLine}, column {ex.StartPosition}: {ex.Message}")
                    : ex.Message;
            }
            catch (RdfException ex) { result.Error = ex.Message; }
            return result;
        }

        private static RdfQueryResult Run(string root, string file, LeviathanQueryProcessor processor, TurtleFormatter formatter)
        {
            var result = new RdfQueryResult { Path = Relative(root, file) };
            try
            {
                var query = new SparqlQueryParser().ParseFromFile(file);
                switch (processor.ProcessQuery(query))
                {
                    case SparqlResultSet set when set.ResultsType == SparqlResultsType.Boolean:
                        result.Kind = "ask";
                        result.Answer = set.Result;
                        break;
                    case SparqlResultSet set:
                        result.Kind = "select";
                        result.Variables = set.Variables.ToList();
                        result.RowCount = set.Count;
                        result.Rows = set.Results.Take(MaxRows)
                            .Select(r => result.Variables.ToDictionary(v => v, v => r.HasValue(v) ? Short(r[v], formatter) : null))
                            .ToList();
                        break;
                    case IGraph g:
                        result.Kind = "graph";
                        result.RowCount = g.Triples.Count;
                        break;
                    default:
                        result.Error = UiText.Say("la query non ha dato un risultato che si sappia leggere", "the query gave no result that can be read");
                        break;
                }
            }
            catch (RdfException ex) { result.Error = ex.Message; }
            return result;
        }

        /// <summary>I file indicati, o quelli con l'estensione giusta dentro le cartelle indicate; sempre dentro il progetto.</summary>
        private static HashSet<string> Expand(string root, IEnumerable<string> paths, string pattern, string what)
        {
            var found = new HashSet<string>(StringComparer.Ordinal);
            foreach (var p in paths ?? Enumerable.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(p)) continue;
                var full = Path.GetFullPath(Path.Combine(root, p.Trim().Replace('\\', '/')));
                var inside = full == root || full.StartsWith(root + Path.DirectorySeparatorChar, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
                if (!inside)
                    throw new ArgumentException(UiText.Say($"{what}: '{p}' è fuori dal progetto.", $"{what}: '{p}' is outside the project."));
                if (File.Exists(full)) found.Add(full);
                else if (Directory.Exists(full))
                    foreach (var f in Directory.EnumerateFiles(full, pattern, SearchOption.AllDirectories).OrderBy(f => f, StringComparer.Ordinal)) found.Add(f);
                else
                    throw new ArgumentException(UiText.Say($"{what}: '{p}' non c'è nel progetto.", $"{what}: '{p}' is not in the project."));
            }
            return found;
        }

        private static string Relative(string root, string file) => Path.GetRelativePath(root, file).Replace('\\', '/');

        private static string Short(INode node, TurtleFormatter formatter)
        {
            if (node == null) return null;
            // Il formattatore Turtle scrive i numeri con uno spazio in coda («1 »), per non confonderli con il punto finale.
            try { return formatter.Format(node).Trim(); }
            catch (RdfOutputException) { return node.ToString(); }
        }
    }

    public sealed class RdfCheckReport
    {
        /// <summary>Tutti i file si leggono, i dati rispettano le regole (se ci sono) e tutte le query sono andate.</summary>
        public bool Ok => Files.All(f => f.Error == null) && (Shacl == null || Shacl.Conforms) && Queries.All(q => q.Error == null);
        public List<RdfFileResult> Files { get; } = new();
        public RdfShaclResult Shacl { get; set; }
        public List<RdfQueryResult> Queries { get; } = new();
        public string Note { get; set; }
    }

    public sealed class RdfFileResult
    {
        public string Path { get; set; }
        /// <summary><c>data</c> o <c>shapes</c>.</summary>
        public string Role { get; set; }
        public int Triples { get; set; }
        public string Error { get; set; }
    }

    public sealed class RdfShaclResult
    {
        public bool Conforms { get; set; }
        public List<RdfShaclViolation> Violations { get; set; } = new();
    }

    public sealed class RdfShaclViolation
    {
        public string Severity { get; set; }
        public string FocusNode { get; set; }
        public string Path { get; set; }
        public string Value { get; set; }
        public string Message { get; set; }
        public string Shape { get; set; }
    }

    public sealed class RdfQueryResult
    {
        public string Path { get; set; }
        /// <summary><c>ask</c>, <c>select</c> o <c>graph</c> (CONSTRUCT/DESCRIBE).</summary>
        public string Kind { get; set; }
        public bool? Answer { get; set; }
        public List<string> Variables { get; set; }
        public int RowCount { get; set; }
        public List<Dictionary<string, string>> Rows { get; set; }
        public string Error { get; set; }
    }
}
