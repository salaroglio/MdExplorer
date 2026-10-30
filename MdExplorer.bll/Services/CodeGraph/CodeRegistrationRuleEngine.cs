using System;
using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>Un punto da cui la regola di registrazione legge una coppia astrazione → implementazione (o perché non ci riesce).</summary>
    public sealed class CodeRegistrationCandidate
    {
        /// <summary>Il punto, per la prova a secco e il campione: per un'annotazione il simbolo annotato, con <c>@nome</c> come chiamato.</summary>
        public CodeCallSiteView Site { get; init; }
        public string Abstraction { get; set; }
        public string Implementation { get; set; }
        /// <summary>L'implementazione è un membro (il metodo annotato): l'arco va a lui.</summary>
        public string ImplementationSymbolId { get; set; }
        public CodeRuleMiss? Miss { get; set; }
    }

    /// <summary>
    /// Il motore delle regole di registrazione (D43, 30/10/2026), puro come <see cref="CodeRuleEngine"/>: dai fatti
    /// dell'estrattore (argomenti X.class, catene, annotazioni, tipo restituito) alle coppie astrazione → implementazione,
    /// poi agli archi <c>resolvesTo</c> con <see cref="CodeRegistrations.Edges"/>. Stessi controlli delle regole D42: quota
    /// collegata, campione lungo tutto l'elenco, portata.
    /// </summary>
    public static class CodeRegistrationRuleEngine
    {
        /// <summary>Le coppie, una per punto (o più, se un'annotazione elenca più tipi); quello che manca col motivo.</summary>
        public static List<CodeRegistrationCandidate> Prepare(CodeEnrichmentRule rule, IReadOnlyList<CodeCallSiteView> sites,
            IReadOnlyDictionary<string, List<CodeCallSiteView>> chains, IReadOnlyList<CodeAnnotatedView> annotated)
        {
            var list = new List<CodeRegistrationCandidate>();
            if (rule.Anchor.Call != null)
                foreach (var site in sites)
                    Pair(list, site, ReadCall(rule.Abstraction, site, chains), ReadCall(rule.Implementation, site, chains), null);
            else
                foreach (var a in annotated)
                {
                    var site = new CodeCallSiteView { CallerId = a.SymbolId, Callee = "@" + a.Name, Path = a.Path, Line = a.Line, Iri = a.AnnotationIri };
                    var member = a.Kind is "method" or "function" or "constructor" or "property" or "field";
                    var abs = ReadAnnotation(rule.Abstraction, a);
                    var impl = ReadAnnotation(rule.Implementation, a);
                    // il metodo annotato che FORNISCE (@Provides): l'implementazione è lui, non una classe
                    Pair(list, site, abs, impl, rule.Implementation.From == CodeRuleEndSource.Annotated && member ? a.SymbolId : null);
                }
            return list;
        }

        private static void Pair(List<CodeRegistrationCandidate> list, CodeCallSiteView site, (List<string> Names, CodeRuleMiss? Miss) abs,
            (List<string> Names, CodeRuleMiss? Miss) impl, string implementationSymbolId)
        {
            var miss = abs.Miss ?? impl.Miss;
            if (miss != null) { list.Add(new CodeRegistrationCandidate { Site = site, Miss = miss }); return; }
            foreach (var a in abs.Names)
                foreach (var i in impl.Names)
                    list.Add(new CodeRegistrationCandidate { Site = site, Abstraction = a, Implementation = i, ImplementationSymbolId = implementationSymbolId });
        }

        private static (List<string>, CodeRuleMiss?) ReadCall(CodeRuleEnd end, CodeCallSiteView site, IReadOnlyDictionary<string, List<CodeCallSiteView>> chains)
        {
            switch (end.From)
            {
                case CodeRuleEndSource.Argument:
                    return site.TypeArguments.TryGetValue(end.Argument!.Value, out var t) ? (new List<string> { t.TypeName }, null) : (null, CodeRuleMiss.NoTypeArgument);
                case CodeRuleEndSource.Chain:
                    if (site.Iri == null || !chains.TryGetValue(site.Iri, out var down) || down.Count == 0) return (null, CodeRuleMiss.NoChain);
                    if (down.Count > 1) return (null, CodeRuleMiss.ChainAmbiguous);
                    return down[0].TypeArguments.TryGetValue(end.Argument!.Value, out var d) ? (new List<string> { d.TypeName }, null) : (null, CodeRuleMiss.NoTypeArgument);
                default:
                    throw new InvalidOperationException($"{end.From} su una chiamata: la regola andava respinta da Validate");
            }
        }

        private static (List<string>, CodeRuleMiss?) ReadAnnotation(CodeRuleEnd end, CodeAnnotatedView a)
        {
            switch (end.From)
            {
                case CodeRuleEndSource.Annotated:
                    return (new List<string> { a.QualifiedName }, null);
                case CodeRuleEndSource.ReturnType:
                    return a.ReturnTypeName != null ? (new List<string> { a.ReturnTypeName }, null) : (null, CodeRuleMiss.NoReturnType);
                case CodeRuleEndSource.AnnotationArgument:
                    var element = end.Element ?? "value";
                    var types = a.Arguments.Where(x => x.Element == element && x.TypeName != null).Select(x => x.TypeName).Distinct(StringComparer.Ordinal).ToList();
                    return types.Count > 0 ? (types, null) : (null, CodeRuleMiss.NoTypeArgument);
                default:
                    throw new InvalidOperationException($"{end.From} su un'annotazione: la regola andava respinta da Validate");
            }
        }

        /// <summary>Le chiavi da cercare fra le classi del grafo: tutti i nomi qualificati delle coppie.</summary>
        public static HashSet<string> OwnerKeys(IEnumerable<CodeRegistrationCandidate> candidates)
            => candidates.Where(c => c.Miss == null).SelectMany(c => c.ImplementationSymbolId != null ? new[] { c.Abstraction } : new[] { c.Abstraction, c.Implementation })
                .ToHashSet(StringComparer.Ordinal);

        public static CodeRulePreview Evaluate(CodeEnrichmentRule rule, IReadOnlyList<CodeRegistrationCandidate> candidates, IEnumerable<CodeOwnerMember> owners)
        {
            var preview = new CodeRulePreview { RuleId = rule.Id, CallSites = candidates.Select(c => c.Site.Iri ?? $"{c.Site.Path}:{c.Site.Line}").Distinct().Count() };
            var rows = owners.ToList();
            var known = rows.Select(o => o.OwnerQualifiedName).ToHashSet(StringComparer.Ordinal);
            var scope = (rule.Scope ?? new List<string>()).Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Replace('\\', '/').TrimEnd('/') + "/").ToList();
            var resolved = new List<CodeRegistrationCandidate>();
            var contexts = new Dictionary<string, CodeRuleContext>(StringComparer.Ordinal);
            foreach (var c in candidates)
            {
                if (c.Miss == null && !known.Contains(c.Abstraction)) c.Miss = CodeRuleMiss.AbstractionNotInGraph;
                if (c.Miss == null && c.ImplementationSymbolId == null && !known.Contains(c.Implementation)) c.Miss = CodeRuleMiss.ImplementationNotInGraph;
                foreach (var x in CodeRuleEngine.ContextsOf(c.Site.Path, contexts)) { x.CallSites++; if (c.Miss == null) x.Resolved++; }
                if (scope.Count > 0 && !scope.Any(s => (c.Site.Path ?? string.Empty).StartsWith(s, StringComparison.Ordinal))) { preview.OutOfScope++; continue; }
                if (c.Miss != null)
                {
                    if (c.Miss is CodeRuleMiss.AbstractionNotInGraph or CodeRuleMiss.ImplementationNotInGraph) preview.OutsideGraph++;
                    else preview.Unresolved++;
                    var key = System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(c.Miss.Value.ToString());
                    preview.MissCounts[key] = preview.MissCounts.TryGetValue(key, out var n) ? n + 1 : 1;
                    if (preview.MissExamples.Count < CodeRuleEngine.MaxMissExamples) preview.MissExamples.Add(Result(c));
                    continue;
                }
                preview.Resolved++;
                resolved.Add(c);
            }
            var registrations = resolved.Select(c => new CodeRegistration
            {
                Mechanism = rule.Id, AbstractionQualifiedName = c.Abstraction, ImplementationQualifiedName = c.Implementation,
                ImplementationSymbolId = c.ImplementationSymbolId, Path = c.Site.Path, Line = c.Site.Line,
            });
            preview.Edges.AddRange(CodeRegistrations.Edges(registrations, rows, out _, CodeOntology.RuleConfidence));
            preview.Contexts.AddRange(contexts.Values.OrderByDescending(c => c.CallSites).ThenBy(c => c.Prefix, StringComparer.Ordinal).Take(CodeRuleEngine.MaxContexts));

            for (var i = 0; i < Math.Min(CodeRuleEngine.SampleSize, resolved.Count); i++)
                preview.Sample.Add(Result(resolved[(int)((long)i * resolved.Count / Math.Min(CodeRuleEngine.SampleSize, resolved.Count))]));

            var what = rule.Anchor.Call != null ? UiText.Say($"chiamate a «{rule.Anchor.Call}» con un argomento X.class", $"calls to «{rule.Anchor.Call}» with an X.class argument")
                : UiText.Say($"simboli con l'annotazione «{rule.Anchor.Annotation}»", $"symbols with the annotation «{rule.Anchor.Annotation}»");
            if (preview.Resolved == 0 && preview.Unresolved == 0 && preview.OutsideGraph > 0)
                preview.RejectReasons.Add(UiText.Say($"Tutti i {preview.OutsideGraph} {what} portano a classi fuori dal grafo: la regola non collega niente qui.",
                    $"All {preview.OutsideGraph} {what} lead to classes outside the graph: the rule links nothing here."));
            else if (preview.Resolved + preview.Unresolved == 0)
                preview.RejectReasons.Add(UiText.Say($"Nessun punto: nel grafo non ci sono {what}. Se il fatto non è registrato dall'estrattore, è un'analisi ulteriore.",
                    $"No point: the graph has no {what}. If the extractor does not record the fact, it is a further analysis."));
            else if (preview.ResolvedShare < CodeRuleEngine.MinResolvedShare)
                preview.RejectReasons.Add(UiText.Say($"Collega solo {preview.Resolved} {what} su {preview.Resolved + preview.Unresolved} ({preview.ResolvedShare:P0}, servono almeno {CodeRuleEngine.MinResolvedShare:P0}): la convenzione è un'altra.",
                    $"It links only {preview.Resolved} {what} out of {preview.Resolved + preview.Unresolved} ({preview.ResolvedShare:P0}, at least {CodeRuleEngine.MinResolvedShare:P0} needed): the convention is a different one."));
            preview.Accepted = preview.RejectReasons.Count == 0;
            return preview;
        }

        private static CodeRuleSiteResult Result(CodeRegistrationCandidate c)
            => new() { Site = c.Site, Value = c.Miss == null ? $"{c.Abstraction} → {c.Implementation}" : null, OwnerKey = c.Abstraction, MemberName = c.Implementation, Miss = c.Miss };
    }
}
