using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>Che cosa un parser ha scritto nel grafo di un progetto: il linguaggio, l'estrattore, e quanti nodi e archi di ogni tipo.</summary>
    public sealed class CodeParserProfile
    {
        public string Language { get; set; }
        public string Extractor { get; set; }
        public string ExtractorVersion { get; set; }
        public int Files { get; set; }
        public Dictionary<string, int> SymbolKinds { get; } = new(StringComparer.Ordinal);
        /// <summary>Gli archi che partono da un simbolo di questo linguaggio, per relazione.</summary>
        public Dictionary<string, int> Relations { get; } = new(StringComparer.Ordinal);
    }

    /// <summary>
    /// Il TBox del grafo del codice (11/11/2026, utente: «voglio che MarkAgent si informi prima dello schema e sappia
    /// esattamente come sono organizzate le strutture dati»): classi, proprietà con dominio e codominio, e i valori ammessi
    /// di tipi, relazioni e certezze, in Turtle (OWL). È lo stesso per tutti i parser — ogni estrattore scrive questi nodi —
    /// e nasce dalle tabelle con cui MdExplorer legge gli script (<see cref="CodeScriptTurtle.Allowed"/>): una classe o un
    /// predicato nuovo senza la sua riga qui è un test rosso, non un TBox che invecchia.
    /// </summary>
    public static class CodeTBox
    {
        public static readonly IReadOnlyDictionary<string, string> Classes = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["File"] = "A source file read by a parser. One per file; its symbols point to it with code:inFile.",
            ["Symbol"] = "A named thing of the code (class, method, field, page control…) or, with kind \"external\", something used but declared outside the analysed code (a library type, a database table, a procedure): only its name is known, no file, no lines.",
            ["Parameter"] = "A parameter of a method, constructor or function, in order.",
            ["Edge"] = "A relation between two symbols, as a node of its own: where it is written (path, line), how sure it is (confidence), and which rule made it. The same relation is also a direct triple <source> code:<relation> <target>.",
            ["CallSite"] = "A FACT for enrichment rules, not a relation: a point of the code where a name is called (or a property assigned, a constructor invoked) with string or type arguments.",
            ["StringArgument"] = "A string argument of a CallSite, shaped like a name (no spaces).",
            ["TypeArgument"] = "A type argument of a CallSite: X.class, typeof(X), GetType(X) at its position, or a generic argument (position 1000, 1001…).",
            ["Annotation"] = "A FACT for registration rules: an annotation or attribute written on a symbol.",
            ["AnnotationArgument"] = "An argument of an Annotation: a string or a type.",
            ["RuleSet"] = "An enrichment rule applied to the graph: its edges point to it with code:rule.",
            ["LinkDescription"] = "The answer to a rule's «describe» question for one linked point (path, line).",
            ["DescriptionSet"] = "The descriptions written for one source file (by the LLM or by hand).",
            ["Description"] = "What a symbol is for, in words: summary, what it returns, remarks.",
            ["ParameterDescription"] = "What one parameter of a symbol is for.",
            ["FileDescription"] = "What a file is for.",
            ["CallResolution"] = "The LLM's choice among the candidates of an ambiguous call: the edge chosen and those rejected.",
        };

        /// <summary>Il codominio (un tipo XSD o una classe) e il senso di ogni predicato.</summary>
        private static readonly Dictionary<string, (string Range, string Meaning)> Properties = new(StringComparer.Ordinal)
        {
            ["path"] = ("string", "logical path of the source file (program/…, @alias/…), the same on every machine"),
            ["language"] = ("string", "language id (csharp, visualbasic, java, typescript, or a package of .mde-languages/); on an external symbol, the language of a script that names it"),
            ["contentHash"] = ("string", "hash of the source file when it was read"),
            ["extractor"] = ("string", "the parser that read the file"),
            ["extractorVersion"] = ("string", "version of that parser: it rises when the parser records more or differently"),
            ["parsedAt"] = ("dateTime", "when the file was read"),
            ["schema"] = ("integer", "version of the script format"),
            ["symbolCount"] = ("integer", "symbols declared in the file, externals excluded"),
            ["kind"] = ("string", "what the symbol is: one of code:SymbolKind"),
            ["name"] = ("string", "simple name"),
            ["qualifiedName"] = ("string", "full name: namespace and type for C#/VB/Java (with parameter types for methods), file:name for declarative languages; for an external, the name as written"),
            ["inFile"] = ("File", "the file that declares the symbol (not on externals)"),
            ["isStatic"] = ("boolean", "static member; in VB also a member of a Module"),
            ["isAsync"] = ("boolean", "async method"),
            ["lineStart"] = ("integer", "first line of the declaration"),
            ["lineEnd"] = ("integer", "last line of the declaration"),
            ["signature"] = ("string", "the declaration as a reader would write it"),
            ["returnType"] = ("string", "return type (or type of a field/property) as displayed"),
            ["returnTypeName"] = ("string", "return type as a qualified name, when it resolves: for registration rules"),
            ["visibility"] = ("string", "public, private, protected, internal…"),
            ["parent"] = ("Symbol", "the symbol that contains this one (the type of a member, the module of a function)"),
            ["signatureHash"] = ("string", "hash of the signature: a description written for another signature is stale"),
            ["bodyHash"] = ("string", "hash of the body: tells whether the code changed since it was described"),
            ["trivial"] = ("string", "the body is a trivial shape (getter, setter, delegation): not worth describing"),
            ["trivialOf"] = ("string", "what the trivial body returns or delegates to"),
            ["guard"] = ("string", "the condition under which the control or the edge exists (a page directive)"),
            ["parameterOf"] = ("Symbol", "the symbol the parameter belongs to"),
            ["position"] = ("integer", "position from 0 (a generic type argument: 1000 + its position)"),
            ["direction"] = ("string", "in, out, ref"),
            ["isNullable"] = ("boolean", "the parameter accepts null"),
            ["type"] = ("string", "the parameter's type as displayed"),
            ["defaultValue"] = ("string", "default value, when optional"),
            ["source"] = ("Symbol", "where the relation starts"),
            ["target"] = ("Symbol", "where the relation ends"),
            ["relation"] = ("string", "the relation's name: one of code:Relation"),
            ["relationIri"] = ("Property", "the same relation as the IRI of its property (code:calls…)"),
            ["confidence"] = ("string", "how sure: one of code:Confidence"),
            ["line"] = ("integer", "the line where it is written"),
            ["rule"] = ("RuleSet", "the enrichment rule that made it"),
            ["written"] = ("string", "the reference as written in the source (kind:name), kept to link it again when the target is analysed later"),
            ["ruleId"] = ("string", "id of the rule, as in code_rules"),
            ["description"] = ("string", "what the rule does, or what a parameter is for"),
            ["writtenBy"] = ("string", "who wrote it (an LLM engine, a person, MarkAgent)"),
            ["writtenAt"] = ("dateTime", "when"),
            ["caller"] = ("Symbol", "the symbol that contains the point"),
            ["callee"] = ("string", "the name called, as written; the property name for an assignment, the type name for a constructor"),
            ["ordinal"] = ("integer", "order among the points (or annotations) of the same symbol and line"),
            ["argumentCount"] = ("integer", "how many arguments the call has"),
            ["receiver"] = ("string", "what is written before the dot"),
            ["receiverSite"] = ("CallSite", "the recorded call further down the receiver chain (bind(X).to(Y): to → bind)"),
            ["argumentOf"] = ("CallSite", "the point (or annotation) the argument belongs to"),
            ["value"] = ("string", "the string"),
            ["typeName"] = ("string", "the type as a qualified name, resolved through the imports"),
            ["annotationOf"] = ("Symbol", "the annotated symbol"),
            ["element"] = ("string", "name of the annotation's element (value when positional)"),
            ["summary"] = ("string", "one or two sentences: what it is for"),
            ["factHash"] = ("string", "hash of the fact described: a changed fact makes the description stale"),
            ["promptVersion"] = ("string", "version of the prompt that produced the description"),
            ["describes"] = ("Symbol", "what is described"),
            ["returnsDescription"] = ("string", "what the method returns"),
            ["remarks"] = ("string", "what a reader must know beyond the summary"),
            ["contextHash"] = ("string", "hash of the callees' descriptions used as context"),
            ["parameterName"] = ("string", "name of the described parameter"),
            ["chooses"] = ("Edge", "the candidate edge the LLM chose"),
            ["rejects"] = ("Edge", "a candidate edge the LLM rejected"),
        };

        /// <summary>Dove il codominio di un predicato dipende dalla classe che lo porta.</summary>
        private static readonly Dictionary<string, string> RangeOn = new(StringComparer.Ordinal)
        {
            ["AnnotationArgument.argumentOf"] = "Annotation",
            ["FileDescription.describes"] = "File",
        };

        /// <summary>Tutte le coppie classe.predicato del TBox: il test le confronta con ciò che gli script ammettono.</summary>
        internal static IEnumerable<(string Class, string Property, string Range)> Rows()
            => from c in CodeScriptTurtle.Allowed
               from p in c.Value
               where !CodeScriptParser.Relations.Contains(p)
               select (c.Key, p, RangeOn.TryGetValue(c.Key + "." + p, out var r) ? r : Properties.TryGetValue(p, out var d) ? d.Range : null);

        public static string Turtle()
        {
            var sb = new StringBuilder();
            sb.Append("@prefix code: <").Append(CodeOntology.Namespace).Append("> .\n")
              .Append("@prefix owl: <http://www.w3.org/2002/07/owl#> .\n@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> .\n@prefix xsd: <http://www.w3.org/2001/XMLSchema#> .\n\n")
              .Append("<").Append(CodeOntology.Namespace.TrimEnd('#')).Append("> a owl:Ontology ; owl:versionInfo \"").Append(CodeScriptParser.CurrentSchema).Append("\" ;\n")
              .Append("  rdfs:comment \"The code graph of MdExplorer. Every parser (Roslyn for C# and Visual Basic, tree-sitter, the declarative language packages) writes these same nodes; what differs per language is which kinds and relations it produces.\" .\n\n");

            sb.Append("# ---- classes\n");
            foreach (var c in Classes)
                sb.Append("code:").Append(c.Key).Append(" a owl:Class ; rdfs:comment ").Append(Lit(c.Value)).Append(" .\n");

            sb.Append("\n# ---- properties (domain = the classes that carry it)\n");
            foreach (var g in Rows().GroupBy(r => r.Property, StringComparer.Ordinal).OrderBy(g => g.Key, StringComparer.Ordinal))
            {
                var ranges = g.Select(r => r.Range).Distinct(StringComparer.Ordinal).ToList();
                var isObject = ranges.All(r => r != null && (Classes.ContainsKey(r) || r == "Property"));
                sb.Append("code:").Append(g.Key).Append(" a ").Append(isObject ? "owl:ObjectProperty" : "owl:DatatypeProperty")
                  .Append(" ; rdfs:domain ").Append(Union(g.Select(r => "code:" + r.Class)))
                  .Append(" ; rdfs:range ").Append(Union(ranges.Select(RangeIri)))
                  .Append(" ; rdfs:comment ").Append(Lit(Properties[g.Key].Meaning)).Append(" .\n");
            }

            sb.Append("\n# ---- relations between symbols: <source> code:<relation> <target>, and a code:Edge with the details\n");
            foreach (var r in CodeScriptParser.Relations)
                sb.Append("code:").Append(r).Append(" a owl:ObjectProperty ; rdfs:subPropertyOf code:Relation ; rdfs:domain code:Symbol ; rdfs:range code:Symbol ; rdfs:comment ")
                  .Append(Lit(CodeGraphSchema.Relations[r])).Append(" .\n");

            sb.Append("\n# ---- allowed values\n");
            Values(sb, "SymbolKind", "kind", CodeScriptParser.SymbolKinds.Select(k => (k, CodeGraphSchema.SymbolKinds[k])));
            Values(sb, "Confidence", "confidence", CodeGraphSchema.Confidences.Select(c => (c.Key, c.Value)));
            return sb.ToString();
        }

        private static void Values(StringBuilder sb, string name, string property, IEnumerable<(string Value, string Meaning)> values)
        {
            sb.Append("code:").Append(name).Append(" a rdfs:Datatype ; rdfs:comment \"values of code:").Append(property).Append("\" ; owl:oneOf ( ")
              .Append(string.Join(" ", values.Select(v => Lit(v.Value)))).Append(" ) .\n");
            foreach (var (value, meaning) in values) sb.Append("#   ").Append(value).Append(" = ").Append(meaning).Append('\n');
        }

        private static string RangeIri(string range) => range switch
        {
            "string" or "integer" or "boolean" or "dateTime" => "xsd:" + range,
            "Property" => "owl:ObjectProperty",
            _ => "code:" + range,
        };

        private static string Union(IEnumerable<string> items)
        {
            var list = items.Distinct(StringComparer.Ordinal).ToList();
            return list.Count == 1 ? list[0] : "[ owl:unionOf ( " + string.Join(" ", list) + " ) ]";
        }

        private static string Lit(string s) => "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
    }
}
