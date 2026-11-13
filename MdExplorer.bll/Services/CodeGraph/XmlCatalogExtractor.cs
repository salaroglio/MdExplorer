using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using MdExplorer.Features.Services;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// I cataloghi di SQL in XML (13/11/2026, rilevazione dal cliente: il livello dati legge le frasi da file XML per gruppo e
    /// nome, ci mette i valori con <c>AppendFormat</c> e le esegue; il grafo si fermava al lettore). Un pacchetto di linguaggio
    /// con la sezione <c>xml</c> (<see cref="DeclarativeLanguage.XmlRule"/>) dice con XPath quali elementi sono query, il loro
    /// nome, il gruppo e i frammenti. Ogni query è un simbolo <c>query</c> (<c>gruppo.nome</c>, figlio del suo gruppo), ogni
    /// frammento un <c>query</c> figlio della query; il testo di ognuno si legge con <see cref="EmbeddedSql"/>, lo stesso
    /// lettore dell'SQL nelle stringhe C# e VB, e dà gli stessi nodi esterni (che il PL/SQL analizzato poi sostituisce). Un
    /// segnaposto (<c>{0}</c>) è un pezzo non scritto: un nome di tabella che cade lì è il nodo <c>?</c>.
    /// Il collegamento chiamata → query lo fa <see cref="XmlCatalogLinker"/>, a grafo caricato.
    /// </summary>
    public sealed class XmlCatalogExtractor : ICodeExtractor
    {
        /// <summary>1 = query e frammenti con XPath, SQL letto con EmbeddedSql (13/11/2026).</summary>
        public const string EngineVersion = "1";

        private readonly DeclarativeLanguage _lang;

        public XmlCatalogExtractor(DeclarativeLanguage language)
        {
            _lang = language ?? throw new ArgumentNullException(nameof(language));
            if (language.Xml == null) throw new ArgumentException(UiText.Say($"{language.Id}: il pacchetto non ha la sezione xml", $"{language.Id}: the package has no xml section"), nameof(language));
        }

        public string LanguageId => _lang.Id;
        public string ExtractorName => "xml-catalog";
        public string ExtractorVersion => $"{EngineVersion}-{_lang.Version}.{_lang.DefinitionHash}";
        public string Availability => "ready";

        public CodeExtractionResult Extract(CodeExtractionRequest request, CancellationToken ct = default)
        {
            var result = new CodeExtractionResult();
            foreach (var file in request.FilesToEmit)
            {
                ct.ThrowIfCancellationRequested();
                string text;
                try { text = request.ReadText(file); }
                catch (Exception ex) { result.Warnings.Add(UiText.Say($"{file.LogicalPath}: non leggibile ({ex.Message})", $"{file.LogicalPath}: cannot be read ({ex.Message})")); continue; }
                var script = new FileReader(this, file, text, result.Warnings).Read();
                result.Scripts.Add((file, script));
            }
            return result;
        }

        private string StableId(string kind, string qualifiedName) => RoslynCSharpExtractor.StableId(_lang.Id, kind, qualifiedName);

        private sealed class FileReader
        {
            private readonly XmlCatalogExtractor _owner;
            private readonly CodeSourceFile _file;
            private readonly string _text;
            private readonly List<string> _warnings;
            private readonly CodeScript _script = new();
            private readonly HashSet<string> _qualified = new(StringComparer.Ordinal);
            private readonly HashSet<string> _externals = new(StringComparer.Ordinal);
            private readonly HashSet<string> _edges = new(StringComparer.Ordinal);
            private readonly Dictionary<string, string> _groups = new(StringComparer.Ordinal);
            private string _moduleId;
            private Regex _hole;

            public FileReader(XmlCatalogExtractor owner, CodeSourceFile file, string text, List<string> warnings)
            {
                _owner = owner; _file = file; _text = text; _warnings = warnings;
                _hole = new Regex(@"[\w$#]*(?:" + owner._lang.Xml.Placeholder + @")[\w$#]*", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
            }

            private DeclarativeLanguage.XmlRule Rule => _owner._lang.Xml;

            public CodeScript Read()
            {
                var lines = _text.Count(c => c == '\n') + 1;
                _script.Header = new CodeScriptHeader
                {
                    Schema = CodeScriptParser.CurrentSchema, Kind = CodeScriptKind.Ast, Source = _file.LogicalPath, Language = _owner.LanguageId,
                    SourceHash = CodeSourceText.Hash(_text), Extractor = _owner.ExtractorName, ExtractorVersion = _owner.ExtractorVersion,
                    GeneratedAt = DateTime.UtcNow.ToString("o"),
                };
                _moduleId = _owner.StableId("module", _file.LogicalPath);
                _script.Symbols.Add(new ScriptSymbol
                {
                    Id = _moduleId, Kind = "module", Name = System.IO.Path.GetFileName(_file.LogicalPath), QualifiedName = _file.LogicalPath,
                    LineStart = 1, LineEnd = lines, Signature = _file.LogicalPath, Visibility = "public",
                });

                XDocument doc;
                try { doc = XDocument.Parse(_text, LoadOptions.SetLineInfo); }
                catch (XmlException ex)
                {
                    // un XML che non si legge non è un catalogo: il file c'è, senza query, e lo si dice
                    _warnings.Add(UiText.Say($"{_file.LogicalPath}: XML non valido alla riga {ex.LineNumber} ({ex.Message}): nessuna query letta",
                        $"{_file.LogicalPath}: invalid XML at line {ex.LineNumber} ({ex.Message}): no query read"));
                    return _script;
                }
                foreach (var rule in Rule.Queries)
                    foreach (var element in Select(doc, rule.Match).OfType<XElement>())
                        Query(rule, element);
                return _script;
            }

            private static IEnumerable<object> Select(XNode node, XPathExpression expression)
                => node.CreateNavigator().Select(expression).Cast<XPathNavigator>().Select(n => n.UnderlyingObject);

            private static string Text(XElement element, XPathExpression expression)
            {
                if (expression == null) return null;
                var value = element.CreateNavigator().Evaluate(expression);
                var text = value switch
                {
                    XPathNodeIterator it => it.MoveNext() ? it.Current.Value : null,
                    null => null,
                    _ => Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture),
                };
                return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
            }

            private static int LineOf(XObject o) => o is IXmlLineInfo info && info.HasLineInfo() ? info.LineNumber : 1;

            /// <summary>L'ultima riga dell'elemento: quella del suo ultimo discendente, o della sua chiusura se è dopo.</summary>
            private static int EndOf(XElement element)
            {
                var last = element.DescendantNodesAndSelf().Select(LineOf).DefaultIfEmpty(LineOf(element)).Max();
                return last;
            }

            private void Query(DeclarativeLanguage.XmlQueryRule rule, XElement element)
            {
                var line = LineOf(element);
                var name = Text(element, rule.Name);
                if (name == null)
                {
                    _warnings.Add(UiText.Say($"{_file.LogicalPath}:{line}: elemento <{element.Name.LocalName}> preso come query, ma senza nome (xml.queries name): saltato",
                        $"{_file.LogicalPath}:{line}: element <{element.Name.LocalName}> taken as a query, but without a name (xml.queries name): skipped"));
                    return;
                }
                var group = Text(element, rule.Group);
                var parentId = group == null ? _moduleId : Group(group, element);
                var local = group == null ? name : group + "." + name;
                var queryId = Declare("query", name, local, parentId, line, EndOf(element), local, out var declared);
                if (rule.Fragments == null)
                {
                    Sql(queryId, element.Value, line);
                    return;
                }
                foreach (var fragment in Select(element, rule.Fragments).OfType<XElement>())
                {
                    var fline = LineOf(fragment);
                    var fname = fragment.Name.LocalName;
                    var fid = Declare("query", fname, declared + "." + fname, queryId, fline, EndOf(fragment), local + "." + fname, out _);
                    Sql(fid, fragment.Value, fline);
                }
            }

            private string Group(string group, XElement query)
            {
                if (_groups.TryGetValue(group, out var id)) return id;
                var holder = query.Ancestors().FirstOrDefault() ?? query;
                id = Declare("namespace", group, group, _moduleId, LineOf(holder), EndOf(holder), group, out _);
                _groups[group] = id;
                return id;
            }

            /// <param name="declared">Il percorso dentro il file com'è stato dichiarato (con <c>@riga</c> se era un doppione): i figli lo prolungano.</param>
            private string Declare(string kind, string name, string local, string parentId, int line, int end, string signature, out string declared)
            {
                var qualified = _file.LogicalPath + ":" + local;
                declared = local;
                if (!_qualified.Add(qualified))
                {
                    // lo stesso nome due volte nello stesso file: si tengono entrambi, distinti dalla riga, e lo si dice
                    _warnings.Add(UiText.Say($"{_file.LogicalPath}:{line}: '{local}' è già dichiarato in questo file", $"{_file.LogicalPath}:{line}: '{local}' is already declared in this file"));
                    qualified += "@" + line;
                    declared = local + "@" + line;
                    _qualified.Add(qualified);
                }
                var id = _owner.StableId(kind, qualified);
                _script.Symbols.Add(new ScriptSymbol
                {
                    Id = id, Kind = kind, Name = name, QualifiedName = qualified, LineStart = line, LineEnd = Math.Max(line, end),
                    Signature = signature, Visibility = "public", ParentId = parentId,
                });
                Edge(parentId, id, "contains", "extracted", line);
                return id;
            }

            /// <summary>Le tabelle e le procedure del testo; i segnaposto sono pezzi non scritti.</summary>
            private void Sql(string ownerId, string text, int line)
            {
                if (string.IsNullOrWhiteSpace(text)) return;
                // il segnaposto e le lettere attaccate (TAB_{0}): un nome che lo contiene non si sa tutto
                var sql = _hole.Replace(text, EmbeddedSql.Hole.ToString());
                foreach (var o in EmbeddedSql.Analyze(sql))
                {
                    var name = o.Dynamic ? "?" : o.Name;
                    var key = o.Dynamic ? "dynamic:?" : (o.Access == SqlAccess.Call ? "symbol:" : "table:") + name.ToUpperInvariant();
                    var id = RoslynCSharpExtractor.StableId(RoslynExtractorBase.SqlLanguage, "external", key);
                    if (_externals.Add(id))
                        _script.Symbols.Add(new ScriptSymbol { Id = id, Kind = "external", Name = name.Split('.').Last(), QualifiedName = name, Language = RoslynExtractorBase.SqlLanguage });
                    Edge(ownerId, id, o.Access switch { SqlAccess.Write => "writes", SqlAccess.Call => "calls", _ => "reads" }, o.Dynamic ? "ambiguous" : "inferred", line,
                        o.Dynamic ? UiText.Say("il nome è un segnaposto riempito dal codice", "the name is a placeholder filled by the code") : null);
                }
            }

            private void Edge(string source, string target, string relation, string confidence, int line, string guard = null)
            {
                if (!_edges.Add($"{source}|{target}|{relation}")) return;
                _script.Edges.Add(new ScriptEdge { SourceId = source, TargetId = target, Relation = relation, Confidence = confidence, Line = line, Guard = guard });
            }
        }
    }
}
