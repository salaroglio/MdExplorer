using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Il TBox di un pacchetto di linguaggio (11/11/2026, utente: «assicurati che i tbox vengano inclusi e gestiti anche negli
    /// zip dei linguaggi aggiuntivi»): che cosa il parser di QUEL linguaggio scrive nel grafo — quali tipi di simbolo, da
    /// quale regola di <c>language.yml</c>, e quali relazioni, verso che cosa. Classi e proprietà sono quelle del grafo
    /// (<see cref="CodeTBox"/>); questo file dice solo la parte che cambia da un linguaggio all'altro.
    /// <para>
    /// È un file DERIVATO: si ricava per intero dalle regole, quindi lo scrive MdExplorer (<see cref="Write"/>) ogni volta che
    /// le regole entrano o escono — «Sì» alla proposta di MarkAgent, import, export — e nessuno lo corregge a mano. Sta nel
    /// pacchetto (<c>tbox.ttl</c>) perché lo zip si spieghi da solo a chi lo riceve.
    /// </para>
    /// </summary>
    public static class LanguageTBox
    {
        public const string FileName = "tbox.ttl";

        public static string Turtle(DeclarativeLanguage lang)
        {
            var sb = new StringBuilder();
            var me = $"<{CodeOntology.Namespace.TrimEnd('#')}/language/{lang.Id}";
            string Node(string local) => me + "#" + local + ">";
            sb.Append("# Written by MdExplorer from language.yml: do not edit, it is rewritten whenever the rules change.\n")
              .Append("@prefix code: <").Append(CodeOntology.Namespace).Append("> .\n@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> .\n\n")
              .Append(me).Append("> a code:LanguageProfile ;\n")
              .Append("  code:language ").Append(Lit(lang.Id)).Append(" ; rdfs:label ").Append(Lit(lang.DisplayName ?? lang.Id)).Append(" ; code:packageVersion ").Append(lang.Version).Append(" ;\n")
              .Append("  code:extensions ").Append(string.Join(", ", lang.Extensions.Select(Lit))).Append(" ; code:ignoreCase ").Append(lang.IgnoreCase ? "true" : "false").Append(" ; code:database ").Append(lang.Database ? "true" : "false").Append(" ;\n")
              .Append("  rdfs:comment \"What the parser of this language package writes in MdExplorer's code graph. The classes of nodes and their properties are those of the code ontology (tool code_ontology); here: which symbol kinds and which relations this language produces, and from which rule of language.yml. A kind or a relation not listed here does not exist for this language.\" .\n\n");

            sb.Append("# ---- symbols\n");
            Kind(sb, Node("kind-module"), "module", "module", "one per source file" + (lang.ModuleName != null ? "; its name is the one the file declares (rule module.name), else the file name" : "; its name is the file name"));
            foreach (var b in lang.Blocks)
                Kind(sb, Node("kind-" + b.Id), b.Kind, "blocks." + b.Id, (b.Nesting != null ? "a block, from its begin line to the line where its nesting closes (open and close words counted outside comments and strings)" : "a block, from its begin line to its end line") + In(b.In) + (lang.Parameters != null ? "; its parameters are code:Parameter nodes" : "")
                    + (b.Completes != null ? $"; it implements the {b.Completes} block with the same name: the name (and every reference to it) belongs to this block, and in the same file the {b.Completes} block is named NAME@{b.Completes}" : ""));
            foreach (var l in lang.Labels)
                Kind(sb, Node("kind-" + l.Id), l.Kind, "labels." + l.Id, "a label inside a block, up to the next label or the end of the block" + In(l.In) + (l.OnlyIfReferenced ? "; a symbol only if something in the same file refers to it" : ""));
            foreach (var d in lang.Declarations.Where(d => d.Emit))
                Kind(sb, Node("kind-" + d.Id), d.Kind, "declarations." + d.Id, "a name declared on one line" + In(d.In));
            if (lang.Fragments != null)
                Kind(sb, Node("kind-fragment"), DeclarativeLanguage.FragmentKind, "fragments", "a piece of a block too long to describe in one go" + In(lang.Fragments.In) + ": the block contains its fragments");
            if (lang.Template != null)
                Kind(sb, Node("kind-control"), "control", "template", "an element of a page with an event or a javascript: link; the page is the module");
            Kind(sb, Node("kind-external"), "external", "references", "a name a reference rule could not find among the analysed files (only where the rule says unresolved: external): no file, no lines");

            sb.Append("\n# ---- relations\n");
            Relation(sb, Node("relation-contains"), "contains", "(structure)", "the module contains its blocks, a block its labels, declarations and fragments", null, null);
            for (var i = 0; i < lang.References.Count; i++)
            {
                var r = lang.References[i];
                var target = r.Resolve switch
                {
                    DeclarativeLanguage.ResolveKind.File => "the module of a file, found by file name among the analysed files",
                    DeclarativeLanguage.ResolveKind.Module => "a module, found by the name it declares",
                    _ => "a symbol of the rules " + string.Join(", ", r.Rules) + ", searched " + r.Scope switch
                    {
                        DeclarativeLanguage.ScopeKind.Block => "in the same block",
                        DeclarativeLanguage.ScopeKind.Module => "in the same file",
                        _ => "in the same file first, then in all the analysed files (then inferred)",
                    },
                };
                if (r.AssignTemplate != null) target += "; the name is in a variable: it is the one assigned to that variable earlier in the block (inferred)";
                Relation(sb, Node("relation-" + i), r.Relation, $"references[{i}]", target, r.ExternalWhenUnresolved ? "an external node with the name as written" : "no edge", r.Confidence);
            }
            foreach (var b in lang.Blocks.Where(b => b.Completes != null))
                Relation(sb, Node("relation-implements-" + b.Id), "implements", $"blocks.{b.Id}.completes",
                    $"the {b.Completes} block with the same name (in the same file, else in the analysed files: inferred), and from each member of this block the member of that block with the same kind and name (with overloads, the same number of parameters; several: ambiguous). A reference that finds both a declaration and its implementation goes to the implementation", null, null);
            if (lang.Fragments != null)
                Relation(sb, Node("relation-jumps"), DeclarativeLanguage.JumpsRelation, "fragments", "another fragment of the same block: a jump written in the code, or the code running on into the next piece (inferred)", null, null);
            if (lang.Template != null)
            {
                Relation(sb, Node("relation-includes"), "includes", "template", "the page a directive includes; code:guard holds the condition in the page", null, null);
                Relation(sb, Node("relation-calls-template"), "calls", "template", "the function a control's event calls", null, null);
            }
            return sb.ToString();
        }

        /// <summary>Scrive <c>tbox.ttl</c> nella cartella del pacchetto dalle sue regole; vero se il file è cambiato.</summary>
        public static bool Write(string packageFolder)
        {
            var lang = DeclarativeLanguage.Load(Path.Combine(packageFolder, LanguagePackage.DefinitionFile));
            var path = Path.Combine(packageFolder, FileName);
            var text = Turtle(lang);
            if (File.Exists(path) && File.ReadAllText(path, Encoding.UTF8) == text) return false;
            File.WriteAllText(path, text, new UTF8Encoding(false));
            return true;
        }

        private static string In(IReadOnlyList<string> rules) => rules == null || rules.Count == 0 ? string.Empty : " (inside: " + string.Join(", ", rules) + ")";

        private static void Kind(StringBuilder sb, string node, string kind, string rule, string meaning)
            => sb.Append(node).Append(" a code:ProducedKind ; code:kind ").Append(Lit(kind)).Append(" ; code:fromRule ").Append(Lit(rule)).Append(" ; rdfs:comment ").Append(Lit(meaning)).Append(" .\n");

        private static void Relation(StringBuilder sb, string node, string relation, string rule, string target, string unresolved, string maxConfidence)
        {
            sb.Append(node).Append(" a code:ProducedRelation ; code:relation ").Append(Lit(relation)).Append(" ; code:fromRule ").Append(Lit(rule)).Append(" ; code:targetIs ").Append(Lit(target));
            if (unresolved != null) sb.Append(" ; code:whenNotFound ").Append(Lit(unresolved));
            if (maxConfidence != null) sb.Append(" ; code:maxConfidence ").Append(Lit(maxConfidence));
            sb.Append(" .\n");
        }

        private static string Lit(string s) => "\"" + (s ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n") + "\"";
    }
}
