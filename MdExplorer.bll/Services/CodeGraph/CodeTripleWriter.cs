using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static MdExplorer.Features.Services.CodeGraph.CodeGraphIris;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Da uno script (il record del parser) alle triple: costruisce le richieste SPARQL Update che il caricatore manda a
    /// Fuseki, UNA richiesta per script, quindi UNA transazione: o entra tutto o niente. Dentro, nell'ordine: via gli archi
    /// diretti dei nodi arco di quello script, via ogni nodo che porta <c>code:fromScript</c> uguale allo script, poi
    /// <c>INSERT DATA</c> di tutto il nuovo. I simboli <c>external</c> non hanno <c>fromScript</c>: condivisi, non si
    /// cancellano mai (e in RDF inserirli due volte non fa due nodi). Funzioni pure: si testano senza Fuseki.
    /// </summary>
    public static class CodeTripleWriter
    {
        /// <summary>Lo script AST «rel» (sorgente <c>script.Header.Source</c>) entra nel grafo <paramref name="dataGraph"/>.</summary>
        public static string LoadAstUpdate(CodeScript script, string scriptRel, string dataGraph)
        {
            var g = Iri(dataGraph);
            var sb = new StringBuilder();
            sb.Append(CodeOntology.SparqlPrefixes);
            AppendRemove(sb, scriptRel, g);
            sb.Append("INSERT DATA { GRAPH ").Append(g).Append(" {\n");
            AppendAstTriples(sb, script, Lit(scriptRel));
            sb.Append("} }\n");
            return sb.ToString();
        }

        /// <summary>
        /// Le triple di uno script AST, una riga per nodo: sono il corpo dell'<c>INSERT DATA</c> E il contenuto del file
        /// <c>.ast.ttl</c> (radici esterne, R4: una fonte sola, Git e Fuseki non possono divergere). Con
        /// <paramref name="fromScriptLit"/> null (il file) niente <c>code:fromScript</c>: è una faccenda del caricatore.
        /// </summary>
        public static void AppendAstTriples(StringBuilder sb, CodeScript script, string fromScriptLit)
        {
            var h = script.Header;
            var fileIri = Iri(File(h.Source));
            sb.Append(fileIri).Append(" a ").Append(CodeOntology.ClassFile)
              .Append(" ; code:path ").Append(Lit(h.Source))
              .Append(" ; code:language ").Append(Lit(h.Language ?? string.Empty))
              .Append(" ; code:contentHash ").Append(Lit(h.SourceHash ?? string.Empty))
              .Append(" ; code:extractor ").Append(Lit(h.Extractor ?? string.Empty))
              .Append(" ; code:extractorVersion ").Append(Lit(h.ExtractorVersion ?? string.Empty))
              .Append(" ; code:parsedAt ").Append(Lit(h.GeneratedAt ?? DateTime.UtcNow.ToString("o")))
              .Append(" ; code:schema ").Append(Int(CodeScriptParser.CurrentSchema))
              .Append(" ; code:symbolCount ").Append(Int(script.Symbols.Count(s => !s.IsExternal)));
            FromScript(sb, fromScriptLit).Append(" .\n");

            var parametersBySymbol = script.Parameters.GroupBy(p => p.SymbolId, StringComparer.Ordinal)
                .ToDictionary(gp => gp.Key, gp => gp.OrderBy(p => p.Position).ToList(), StringComparer.Ordinal);

            foreach (var s in script.Symbols)
            {
                var iri = Iri(Symbol(s.Id));
                sb.Append(iri).Append(" a ").Append(CodeOntology.ClassSymbol)
                  .Append(" ; code:kind ").Append(Lit(s.Kind))
                  .Append(" ; code:name ").Append(Lit(s.Name ?? string.Empty))
                  .Append(" ; code:qualifiedName ").Append(Lit(s.QualifiedName ?? string.Empty))
                  .Append(" ; code:language ").Append(Lit(h.Language ?? string.Empty));
                if (s.IsExternal)
                {
                    // Condiviso fra gli script: niente file, niente fromScript, niente righe.
                    sb.Append(" .\n");
                    continue;
                }
                sb.Append(" ; code:inFile ").Append(fileIri)
                  .Append(" ; code:path ").Append(Lit(h.Source))
                  .Append(" ; code:isStatic ").Append(Bool(s.IsStatic))
                  .Append(" ; code:isAsync ").Append(Bool(s.IsAsync));
                if (s.LineStart != null) sb.Append(" ; code:lineStart ").Append(Int(s.LineStart.Value));
                if (s.LineEnd != null) sb.Append(" ; code:lineEnd ").Append(Int(s.LineEnd.Value));
                if (s.Signature != null) sb.Append(" ; code:signature ").Append(Lit(s.Signature));
                if (s.ReturnType != null) sb.Append(" ; code:returnType ").Append(Lit(s.ReturnType));
                if (s.Visibility != null) sb.Append(" ; code:visibility ").Append(Lit(s.Visibility));
                if (s.ParentId != null) sb.Append(" ; code:parent ").Append(Iri(Symbol(s.ParentId)));
                if (s.Trivial != null) sb.Append(" ; code:trivial ").Append(Lit(s.Trivial));
                if (s.TrivialOf != null) sb.Append(" ; code:trivialOf ").Append(Lit(s.TrivialOf));
                if (s.BodyHash != null) sb.Append(" ; code:bodyHash ").Append(Lit(s.BodyHash));
                parametersBySymbol.TryGetValue(s.Id, out var prms);
                sb.Append(" ; code:signatureHash ").Append(Lit(CodeDocPlanner.SignatureHash(s, prms ?? new List<ScriptParameter>())));
                FromScript(sb, fromScriptLit).Append(" .\n");
            }

            foreach (var p in script.Parameters)
            {
                sb.Append(Iri(Parameter(p.SymbolId, p.Position))).Append(" a ").Append(CodeOntology.ClassParameter)
                  .Append(" ; code:parameterOf ").Append(Iri(Symbol(p.SymbolId)))
                  .Append(" ; code:position ").Append(Int(p.Position))
                  .Append(" ; code:name ").Append(Lit(p.Name ?? string.Empty))
                  .Append(" ; code:direction ").Append(Lit(p.Direction ?? "in"))
                  .Append(" ; code:isNullable ").Append(Bool(p.IsNullable));
                if (p.ParameterType != null) sb.Append(" ; code:type ").Append(Lit(p.ParameterType));
                if (p.DefaultValue != null) sb.Append(" ; code:defaultValue ").Append(Lit(p.DefaultValue));
                FromScript(sb, fromScriptLit).Append(" .\n");
            }

            foreach (var e in script.Edges)
            {
                var src = Iri(Symbol(e.SourceId));
                var tgt = Iri(Symbol(e.TargetId));
                // La proprietà diretta serve ai property path e a chi legge il grafo in SPARQL; il nodo arco porta
                // confidenza, riga e lo script che l'ha scritto.
                sb.Append(src).Append(' ').Append(RelationProperty(e.Relation)).Append(' ').Append(tgt).Append(" .\n");
                sb.Append(Iri(Edge(e.SourceId, e.Relation, e.TargetId, e.Line))).Append(" a ").Append(CodeOntology.ClassEdge)
                  .Append(" ; code:source ").Append(src)
                  .Append(" ; code:target ").Append(tgt)
                  .Append(" ; code:relation ").Append(Lit(e.Relation))
                  .Append(" ; code:relationIri ").Append(Iri(RelationIri(e.Relation)))
                  .Append(" ; code:confidence ").Append(Lit(e.Confidence))
                  .Append(" ; code:path ").Append(Lit(h.Source));
                if (e.Line != null) sb.Append(" ; code:line ").Append(Int(e.Line.Value));
                FromScript(sb, fromScriptLit).Append(" .\n");
            }

            foreach (var c in script.CallSites)
            {
                var cs = CallSite(c.SourceId, c.Line, c.Ordinal);
                sb.Append(Iri(cs)).Append(" a ").Append(CodeOntology.ClassCallSite)
                  .Append(" ; code:caller ").Append(Iri(Symbol(c.SourceId)))
                  .Append(" ; code:callee ").Append(Lit(c.Callee))
                  .Append(" ; code:line ").Append(Int(c.Line))
                  .Append(" ; code:ordinal ").Append(Int(c.Ordinal))
                  .Append(" ; code:argumentCount ").Append(Int(c.ArgumentCount))
                  .Append(" ; code:path ").Append(Lit(h.Source));
                if (c.Receiver != null) sb.Append(" ; code:receiver ").Append(Lit(c.Receiver));
                FromScript(sb, fromScriptLit).Append(" .\n");
                foreach (var a in c.Arguments)
                {
                    sb.Append(Iri(StringArgument(cs, a.Position))).Append(" a ").Append(CodeOntology.ClassStringArgument)
                      .Append(" ; code:argumentOf ").Append(Iri(cs))
                      .Append(" ; code:position ").Append(Int(a.Position))
                      .Append(" ; code:value ").Append(Lit(a.Value));
                    FromScript(sb, fromScriptLit).Append(" .\n");
                }
            }
        }

        /// <summary>Lo script di una regola «rel» entra nel grafo: l'insieme e i suoi archi (D42).</summary>
        public static string LoadRuleUpdate(CodeScript script, string scriptRel, string dataGraph)
        {
            var g = Iri(dataGraph);
            var sb = new StringBuilder();
            sb.Append(CodeOntology.SparqlPrefixes);
            AppendRemove(sb, scriptRel, g);
            sb.Append("INSERT DATA { GRAPH ").Append(g).Append(" {\n");
            AppendRuleTriples(sb, script, Lit(scriptRel));
            sb.Append("} }\n");
            return sb.ToString();
        }

        /// <summary>
        /// Le triple di uno script di regola: il <c>code:RuleSet</c> e un nodo arco per ogni arco, con la certezza
        /// <c>rule</c>, la regola che l'ha prodotto e il file del chiamante. Corpo dell'<c>INSERT DATA</c> e del <c>.rules.ttl</c>.
        /// </summary>
        public static void AppendRuleTriples(StringBuilder sb, CodeScript script, string fromScriptLit)
        {
            var h = script.Header;
            var rule = Iri(RuleSet(h.Source));
            sb.Append(rule).Append(" a ").Append(CodeOntology.ClassRuleSet)
              .Append(" ; code:ruleId ").Append(Lit(h.Source))
              .Append(" ; code:description ").Append(Lit(script.RuleDescription ?? string.Empty))
              .Append(" ; code:writtenBy ").Append(Lit(h.WrittenBy ?? string.Empty))
              .Append(" ; code:writtenAt ").Append(Lit(h.WrittenAt ?? string.Empty))
              .Append(" ; code:schema ").Append(Int(CodeScriptParser.CurrentSchema));
            FromScript(sb, fromScriptLit).Append(" .\n");
            foreach (var e in script.Edges)
            {
                var src = Iri(Symbol(e.SourceId));
                var tgt = Iri(Symbol(e.TargetId));
                sb.Append(src).Append(' ').Append(RelationProperty(e.Relation)).Append(' ').Append(tgt).Append(" .\n");
                sb.Append(Iri(Edge(e.SourceId, e.Relation, e.TargetId, e.Line))).Append(" a ").Append(CodeOntology.ClassEdge)
                  .Append(" ; code:source ").Append(src)
                  .Append(" ; code:target ").Append(tgt)
                  .Append(" ; code:relation ").Append(Lit(e.Relation))
                  .Append(" ; code:relationIri ").Append(Iri(RelationIri(e.Relation)))
                  .Append(" ; code:confidence ").Append(Lit(e.Confidence))
                  .Append(" ; code:rule ").Append(rule)
                  .Append(" ; code:path ").Append(Lit(e.Path ?? string.Empty));
                if (e.Line != null) sb.Append(" ; code:line ").Append(Int(e.Line.Value));
                FromScript(sb, fromScriptLit).Append(" .\n");
            }
        }

        private static StringBuilder FromScript(StringBuilder sb, string fromScriptLit)
            => fromScriptLit == null ? sb : sb.Append(" ; code:fromScript ").Append(fromScriptLit);

        /// <summary>Lo script di descrizioni «rel» entra nel grafo: un nodo descrizione per simbolo, uno per parametro descritto.</summary>
        public static string LoadDocUpdate(CodeScript script, string scriptRel, string dataGraph)
        {
            var g = Iri(dataGraph);
            var sb = new StringBuilder();
            sb.Append(CodeOntology.SparqlPrefixes);
            AppendRemove(sb, scriptRel, g);
            sb.Append("INSERT DATA { GRAPH ").Append(g).Append(" {\n");
            AppendDocTriples(sb, script, Lit(scriptRel));
            sb.Append("} }\n");
            return sb.ToString();
        }

        /// <summary>Le triple di uno script di descrizioni: corpo dell'<c>INSERT DATA</c> e contenuto del file <c>.doc.ttl</c>.</summary>
        public static void AppendDocTriples(StringBuilder sb, CodeScript script, string fromScriptLit)
        {
            var h = script.Header;
            sb.Append(Iri(DocSet(h.Source))).Append(" a ").Append(CodeOntology.ClassDescriptionSet)
              .Append(" ; code:path ").Append(Lit(h.Source))
              .Append(" ; code:writtenBy ").Append(Lit(h.WrittenBy ?? string.Empty))
              .Append(" ; code:writtenAt ").Append(Lit(h.WrittenAt ?? string.Empty))
              .Append(" ; code:schema ").Append(Int(CodeScriptParser.CurrentSchema));
            FromScript(sb, fromScriptLit).Append(" .\n");
            if (script.FileDoc != null)
            {
                sb.Append(Iri(FileDoc(h.Source))).Append(" a ").Append(CodeOntology.ClassFileDescription)
                  .Append(" ; code:describes ").Append(Iri(File(h.Source)))
                  .Append(" ; code:summary ").Append(Lit(script.FileDoc.Summary ?? string.Empty))
                  .Append(" ; code:writtenBy ").Append(Lit(h.WrittenBy ?? string.Empty))
                  .Append(" ; code:writtenAt ").Append(Lit(h.WrittenAt ?? string.Empty));
                FromScript(sb, fromScriptLit).Append(" .\n");
            }
            foreach (var d in script.SymbolDocs)
            {
                sb.Append(Iri(Doc(d.SymbolId))).Append(" a ").Append(CodeOntology.ClassDescription)
                  .Append(" ; code:describes ").Append(Iri(Symbol(d.SymbolId)))
                  .Append(" ; code:summary ").Append(Lit(d.Summary ?? string.Empty))
                  .Append(" ; code:writtenBy ").Append(Lit(d.WrittenBy ?? h.WrittenBy ?? string.Empty))
                  .Append(" ; code:writtenAt ").Append(Lit(h.WrittenAt ?? string.Empty));
                if (d.ReturnsDescription != null) sb.Append(" ; code:returnsDescription ").Append(Lit(d.ReturnsDescription));
                if (d.Remarks != null) sb.Append(" ; code:remarks ").Append(Lit(d.Remarks));
                if (d.SignatureHash != null) sb.Append(" ; code:signatureHash ").Append(Lit(d.SignatureHash));
                if (d.BodyHash != null) sb.Append(" ; code:bodyHash ").Append(Lit(d.BodyHash));
                if (d.ContextHash != null) sb.Append(" ; code:contextHash ").Append(Lit(d.ContextHash));
                if (d.PromptVersion != null) sb.Append(" ; code:promptVersion ").Append(Lit(d.PromptVersion));
                FromScript(sb, fromScriptLit).Append(" .\n");
            }
            foreach (var r in script.CallResolutions)
            {
                sb.Append(Iri(Resolution(r.SourceId, r.Line, r.TargetId))).Append(" a ").Append(CodeOntology.ClassCallResolution)
                  .Append(" ; code:source ").Append(Iri(Symbol(r.SourceId)))
                  .Append(" ; code:target ").Append(Iri(Symbol(r.TargetId)))
                  .Append(" ; code:chooses ").Append(Iri(Edge(r.SourceId, "calls", r.TargetId, r.Line)));
                if (r.Line != null) sb.Append(" ; code:line ").Append(Int(r.Line.Value));
                foreach (var x in r.Rejected) sb.Append(" ; code:rejects ").Append(Iri(Edge(r.SourceId, "calls", x, r.Line)));
                FromScript(sb, fromScriptLit).Append(" .\n");
            }
            foreach (var d in script.ParameterDocs)
            {
                sb.Append(Iri(ParamDoc(d.SymbolId, d.ParameterName))).Append(" a ").Append(CodeOntology.ClassParameterDescription)
                  .Append(" ; code:describes ").Append(Iri(Symbol(d.SymbolId)))
                  .Append(" ; code:parameterName ").Append(Lit(d.ParameterName ?? string.Empty))
                  .Append(" ; code:description ").Append(Lit(d.Description ?? string.Empty));
                FromScript(sb, fromScriptLit).Append(" .\n");
            }
        }

        /// <summary>Via tutto ciò che lo script «rel» aveva portato (archi diretti compresi).</summary>
        public static string RemoveUpdate(string scriptRel, string dataGraph)
        {
            var sb = new StringBuilder();
            sb.Append(CodeOntology.SparqlPrefixes);
            AppendRemove(sb, scriptRel, Iri(dataGraph));
            return sb.ToString();
        }

        /// <summary>
        /// Via in UNA transazione tutto ciò che gli script sotto <paramref name="scriptsPrefix"/> (<c>.mde-code/&lt;cartella&gt;/</c>)
        /// avevano portato, archi diretti e stato dei caricamenti compresi: la ricarica da capo di una cartella (22/10/2026).
        /// Le altre cartelle e gli altri progetti non si toccano.
        /// </summary>
        public static string RemoveFolderUpdate(string scriptsPrefix, string dataGraph, string stateGraph)
        {
            var g = Iri(dataGraph);
            var p = Lit(scriptsPrefix);
            var sb = new StringBuilder();
            sb.Append(CodeOntology.SparqlPrefixes);
            sb.Append("DELETE { GRAPH ").Append(g).Append(" { ?a ?p ?b } } WHERE { GRAPH ").Append(g)
              .Append(" { ?e a code:Edge ; code:fromScript ?sc ; code:source ?a ; code:target ?b ; code:relationIri ?p . FILTER(STRSTARTS(?sc, ").Append(p).Append(")) } } ;\n");
            sb.Append("DELETE { GRAPH ").Append(g).Append(" { ?s ?p ?o } } WHERE { GRAPH ").Append(g)
              .Append(" { ?s code:fromScript ?sc ; ?p ?o . FILTER(STRSTARTS(?sc, ").Append(p).Append(")) } } ;\n");
            sb.Append("DELETE { GRAPH ").Append(Iri(stateGraph)).Append(" { ?s ?p ?o } } WHERE { GRAPH ").Append(Iri(stateGraph))
              .Append(" { ?s a ").Append(CodeOntology.ClassScript).Append(" ; code:scriptPath ?sc ; ?p ?o . FILTER(STRSTARTS(?sc, ").Append(p).Append(")) } }\n");
            return sb.ToString();
        }

        private static void AppendRemove(StringBuilder sb, string scriptRel, string g)
        {
            var rel = Lit(scriptRel);
            // 1) gli archi diretti: li conosce solo il nodo arco, quindi prima di cancellare i nodi
            sb.Append("DELETE { GRAPH ").Append(g).Append(" { ?a ?p ?b } } WHERE { GRAPH ").Append(g)
              .Append(" { ?e a code:Edge ; code:fromScript ").Append(rel).Append(" ; code:source ?a ; code:target ?b ; code:relationIri ?p } } ;\n");
            // 2) ogni nodo dello script, con tutte le sue triple
            sb.Append("DELETE { GRAPH ").Append(g).Append(" { ?s ?p ?o } } WHERE { GRAPH ").Append(g)
              .Append(" { ?s code:fromScript ").Append(rel).Append(" ; ?p ?o } } ;\n");
        }

        // ------------------------------------------------------------------ stato (grafo state)

        public static string ScriptStateUpsert(string stateGraph, string scriptIri, string scriptRel, string kind, string contentHash, int schema, int records, string loadedAt)
        {
            var g = Iri(stateGraph);
            var s = Iri(scriptIri);
            return "DELETE WHERE { GRAPH " + g + " { " + s + " ?p ?o } } ;\n" +
                   "INSERT DATA { GRAPH " + g + " { " + s + " a " + CodeOntology.ClassScript +
                   " ; code:scriptPath " + Lit(scriptRel) + " ; code:kind " + Lit(kind) + " ; code:contentHash " + Lit(contentHash) +
                   " ; code:schemaVersion " + Int(schema) + " ; code:recordCount " + Int(records) + " ; code:loadedAt " + Lit(loadedAt) + " . } }\n";
        }

        public static string ScriptStateRemove(string stateGraph, string scriptIri)
            => "DELETE WHERE { GRAPH " + Iri(stateGraph) + " { " + Iri(scriptIri) + " ?p ?o } }\n";

        public static string ScanStateUpsert(string stateGraph, string scanIri, CodeScanState state)
        {
            var g = Iri(stateGraph);
            var s = Iri(scanIri);
            var sb = new StringBuilder();
            sb.Append("DELETE WHERE { GRAPH ").Append(g).Append(" { ").Append(s).Append(" ?p ?o } } ;\n");
            sb.Append("INSERT DATA { GRAPH ").Append(g).Append(" { ").Append(s).Append(" a ").Append(CodeOntology.ClassScanState)
              .Append(" ; code:programFolder ").Append(Lit(state.ProgramFolder))
              .Append(" ; code:mode ").Append(Lit(state.Mode ?? string.Empty))
              .Append(" ; code:scannedAt ").Append(Lit(state.ScannedAt ?? DateTime.UtcNow.ToString("o")))
              .Append(" ; code:candidates ").Append(Int(state.Candidates))
              .Append(" ; code:reparsed ").Append(Int(state.Reparsed));
            if (state.RepositoryRoot != null) sb.Append(" ; code:repositoryRoot ").Append(Lit(state.RepositoryRoot));
            if (state.AnalyzedCommit != null) sb.Append(" ; code:analyzedCommit ").Append(Lit(state.AnalyzedCommit));
            if (!string.IsNullOrEmpty(state.Bridges)) sb.Append(" ; code:bridges ").Append(Lit(state.Bridges));
            sb.Append(" . } }\n");
            return sb.ToString();
        }

        public static string ScanStateRemove(string stateGraph, string scanIri)
            => "DELETE WHERE { GRAPH " + Iri(stateGraph) + " { " + Iri(scanIri) + " ?p ?o } }\n";

        public static string DescriptionRunUpsert(string stateGraph, string runIri, CodeDescriptionRun run)
        {
            var g = Iri(stateGraph);
            var s = Iri(runIri);
            var sb = new StringBuilder();
            sb.Append("DELETE WHERE { GRAPH ").Append(g).Append(" { ").Append(s).Append(" ?p ?o } } ;\n");
            sb.Append("INSERT DATA { GRAPH ").Append(g).Append(" { ").Append(s).Append(" a ").Append(CodeOntology.ClassDescriptionRun)
              .Append(" ; code:programFolder ").Append(Lit(run.ProgramFolder))
              .Append(" ; code:status ").Append(Lit(run.Status))
              .Append(" ; code:engine ").Append(Lit(run.Engine ?? string.Empty))
              .Append(" ; code:force ").Append(Bool(run.Force))
              .Append(" ; code:review ").Append(Bool(run.Review))
              .Append(" ; code:startedAt ").Append(Lit(run.StartedAt ?? string.Empty))
              .Append(" ; code:updatedAt ").Append(Lit(run.UpdatedAt ?? DateTime.UtcNow.ToString("o")))
              .Append(" ; code:fileIndex ").Append(Int(run.FileIndex))
              .Append(" ; code:fileTotal ").Append(Int(run.FileTotal))
              .Append(" ; code:filesDescribed ").Append(Int(run.FilesDescribed))
              .Append(" ; code:questionsDone ").Append(Int(run.QuestionsDone));
            if (run.CurrentFile != null) sb.Append(" ; code:currentFile ").Append(Lit(run.CurrentFile));
            if (run.Error != null) sb.Append(" ; code:error ").Append(Lit(run.Error));
            sb.Append(" . } }\n");
            return sb.ToString();
        }

        public static string DescriptionRunRemove(string stateGraph, string runIri)
            => "DELETE WHERE { GRAPH " + Iri(stateGraph) + " { " + Iri(runIri) + " ?p ?o } }\n";

        public static string ScanStatesRemoveAll(string stateGraph)
            => "DELETE WHERE { GRAPH " + Iri(stateGraph) + " { ?s a " + CodeOntology.ClassScanState + " ; ?p ?o } }\n";
    }
}
