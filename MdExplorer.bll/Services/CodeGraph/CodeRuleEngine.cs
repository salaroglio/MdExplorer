using System;
using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>Una classe del grafo con uno dei suoi metodi (o senza, se non ne ha): quello che una regola cerca.</summary>
    public sealed class CodeOwnerMember
    {
        public string OwnerId { get; set; }
        public string OwnerQualifiedName { get; set; }
        public string OwnerName { get; set; }
        public string MemberId { get; set; }
        public string MemberName { get; set; }
        public string MemberQualifiedName { get; set; }
    }

    /// <summary>Perché un punto di chiamata non ha trovato il metodo: il motivo che Mark legge per correggere la regola.</summary>
    public enum CodeRuleMiss { NoSeparator, NoPairedCall, AliasNotInMap, ClassNotFound, TooManyClasses, MethodNotFound, TooManyTargets }

    public sealed class CodeRuleSiteResult
    {
        public CodeCallSiteView Site { get; set; }
        public string Value { get; set; }
        public string OwnerKey { get; set; }
        public string MemberName { get; set; }
        public CodeRuleMiss? Miss { get; set; }
        /// <summary>I metodi trovati (più di uno = overload della stessa classe, o la stessa classe in più programmi).</summary>
        public List<CodeOwnerMember> Targets { get; } = new();
    }

    /// <summary>La prova a secco di una regola: i numeri, gli esempi, gli archi che produrrebbe e il verdetto di MdExplorer.</summary>
    public sealed class CodeRulePreview
    {
        public string RuleId { get; set; }
        /// <summary>Tutti i punti di chiamata del nome chiamato.</summary>
        public int CallSites { get; set; }
        /// <summary>Quelli in cui l'argomento della regola non è una stringa (una variabile, una costante): la regola non li vede.</summary>
        public int NotLiteral { get; set; }
        /// <summary>Quelli scartati dai prefissi esclusi.</summary>
        public int Excluded { get; set; }
        public int Resolved { get; set; }
        public int Unresolved { get; set; }
        public Dictionary<string, int> MissCounts { get; } = new(StringComparer.Ordinal);
        /// <summary>La quota collegata fra i punti che la regola deve risolvere (né non-stringa né esclusi).</summary>
        public double ResolvedShare => Resolved + Unresolved == 0 ? 0 : (double)Resolved / (Resolved + Unresolved);
        public bool Accepted { get; set; }
        /// <summary>Perché MdExplorer la respinge (vuoto se accettata).</summary>
        public List<string> RejectReasons { get; } = new();
        public List<ScriptEdge> Edges { get; } = new();
        /// <summary>Fino a <see cref="CodeRuleEngine.SampleSize"/> collegamenti presi lungo tutto l'elenco: Mark li verifica nel sorgente.</summary>
        public List<CodeRuleSiteResult> Sample { get; } = new();
        /// <summary>Fino a <see cref="CodeRuleEngine.MaxMissExamples"/> punti non collegati, con il motivo.</summary>
        public List<CodeRuleSiteResult> MissExamples { get; } = new();
    }

    /// <summary>
    /// Il motore delle regole di arricchimento (D42, 23/10/2026), puro: dai punti di chiamata e dalle classi del grafo agli
    /// archi <c>dispatches</c>, con la prova a secco e i controlli che respingono da soli una regola cattiva (punto 2 della
    /// robustezza: l'utente non deve vederla). Due giri: <see cref="OwnerKeys"/> dice quali classi cercare nel grafo,
    /// <see cref="Evaluate"/> collega.
    /// </summary>
    public static class CodeRuleEngine
    {
        public const string Relation = "dispatches";
        /// <summary>Sotto questa quota di punti collegati la regola non descrive davvero la convenzione: respinta.</summary>
        public const double MinResolvedShare = 0.6;
        /// <summary>Un nome che porta a più classi di così è troppo generico per decidere.</summary>
        public const int MaxOwnerClasses = 3;
        /// <summary>Più metodi di così per un punto (overload, copie in più programmi) e il collegamento non dice niente.</summary>
        public const int MaxTargetsPerSite = 5;
        public const int SampleSize = 5;
        public const int MaxMissExamples = 20;

        /// <summary>Le chiavi da cercare fra le classi del grafo (nome semplice o qualificato), dopo la mappa degli alias.</summary>
        public static HashSet<string> OwnerKeys(CodeEnrichmentRule rule, IEnumerable<CodeCallSiteView> sites, IEnumerable<CodeCallSiteView> paired, IReadOnlyDictionary<string, string> aliasMap)
        {
            var keys = new HashSet<string>(StringComparer.Ordinal);
            var pairedByCaller = PairedByCaller(paired);
            foreach (var site in sites)
            {
                var r = Prepare(rule, site, pairedByCaller, aliasMap, out _);
                if (r?.Miss == null && r?.OwnerKey != null) keys.Add(r.OwnerKey);
            }
            return keys;
        }

        public static CodeRulePreview Evaluate(CodeEnrichmentRule rule, IReadOnlyList<CodeCallSiteView> sites, IEnumerable<CodeCallSiteView> paired,
            IReadOnlyDictionary<string, string> aliasMap, IEnumerable<CodeOwnerMember> owners)
        {
            var preview = new CodeRulePreview { RuleId = rule.Id, CallSites = sites.Count };
            var pairedByCaller = PairedByCaller(paired);
            var rows = owners.ToList();
            var byQualified = rows.GroupBy(o => o.OwnerQualifiedName, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);
            var byName = rows.GroupBy(o => o.OwnerName, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);
            var resolved = new List<CodeRuleSiteResult>();
            var edgeKeys = new HashSet<string>(StringComparer.Ordinal);

            foreach (var site in sites)
            {
                var r = Prepare(rule, site, pairedByCaller, aliasMap, out var skip);
                if (skip == Skip.NotLiteral) { preview.NotLiteral++; continue; }
                if (skip == Skip.Excluded) { preview.Excluded++; continue; }
                if (r.Miss == null)
                {
                    // il nome qualificato vince sul nome semplice: «com.x.Foo» non deve prendere anche un altro Foo
                    var ownerRows = byQualified.TryGetValue(r.OwnerKey, out var q) ? q : byName.TryGetValue(r.OwnerKey, out var n) ? n : null;
                    var classes = ownerRows?.Select(o => o.OwnerId).Distinct(StringComparer.Ordinal).Count() ?? 0;
                    if (classes == 0) r.Miss = CodeRuleMiss.ClassNotFound;
                    else if (classes > MaxOwnerClasses) r.Miss = CodeRuleMiss.TooManyClasses;
                    else
                    {
                        r.Targets.AddRange(ownerRows.Where(o => o.MemberId != null && o.MemberName == r.MemberName)
                                                    .GroupBy(o => o.MemberId, StringComparer.Ordinal).Select(g => g.First()));
                        if (r.Targets.Count == 0) r.Miss = CodeRuleMiss.MethodNotFound;
                        else if (r.Targets.Count > MaxTargetsPerSite) r.Miss = CodeRuleMiss.TooManyTargets;
                    }
                }
                if (r.Miss != null)
                {
                    preview.Unresolved++;
                    var key = r.Miss.Value.ToString();
                    preview.MissCounts[key] = preview.MissCounts.TryGetValue(key, out var c) ? c + 1 : 1;
                    if (preview.MissExamples.Count < MaxMissExamples) preview.MissExamples.Add(r);
                    continue;
                }
                preview.Resolved++;
                resolved.Add(r);
                foreach (var t in r.Targets)
                {
                    if (t.MemberId == site.CallerId || !edgeKeys.Add($"{site.CallerId}|{t.MemberId}|{site.Line}")) continue;
                    preview.Edges.Add(new ScriptEdge
                    {
                        SourceId = site.CallerId, TargetId = t.MemberId, Relation = Relation, Confidence = CodeOntology.RuleConfidence,
                        Line = site.Line, Path = site.Path,
                    });
                }
            }

            // il campione lungo tutto l'elenco, non i primi cinque dello stesso file
            for (var i = 0; i < Math.Min(SampleSize, resolved.Count); i++)
                preview.Sample.Add(resolved[(int)((long)i * resolved.Count / Math.Min(SampleSize, resolved.Count))]);

            if (preview.Resolved + preview.Unresolved == 0)
                preview.RejectReasons.Add(UiText.Say($"Nessuna chiamata a «{rule.Call.Callee}» con una stringa in posizione {rule.Call.Argument}: la regola non collega niente.",
                    $"No call to «{rule.Call.Callee}» with a string at position {rule.Call.Argument}: the rule links nothing."));
            else if (preview.ResolvedShare < MinResolvedShare)
                preview.RejectReasons.Add(UiText.Say($"Collega solo {preview.Resolved} chiamate su {preview.Resolved + preview.Unresolved} ({preview.ResolvedShare:P0}, servono almeno {MinResolvedShare:P0}): la convenzione è un'altra.",
                    $"It links only {preview.Resolved} calls out of {preview.Resolved + preview.Unresolved} ({preview.ResolvedShare:P0}, at least {MinResolvedShare:P0} needed): the convention is a different one."));
            if (preview.MissCounts.TryGetValue(nameof(CodeRuleMiss.TooManyClasses), out var many) && many * 10 > preview.Resolved + preview.Unresolved)
                preview.RejectReasons.Add(UiText.Say($"In {many} chiamate il nome porta a più di {MaxOwnerClasses} classi: la regola è troppo generica.",
                    $"In {many} calls the name leads to more than {MaxOwnerClasses} classes: the rule is too generic."));
            preview.Accepted = preview.RejectReasons.Count == 0;
            return preview;
        }

        private enum Skip { None, NotLiteral, Excluded }

        private static Dictionary<string, List<CodeCallSiteView>> PairedByCaller(IEnumerable<CodeCallSiteView> paired)
            => (paired ?? Enumerable.Empty<CodeCallSiteView>()).GroupBy(p => p.CallerId, StringComparer.Ordinal)
               .ToDictionary(g => g.Key, g => g.OrderBy(p => p.Line).ThenBy(p => p.Ordinal).ToList(), StringComparer.Ordinal);

        /// <summary>La stringa, il proprietario (dopo la mappa) e il metodo di un punto; null con <paramref name="skip"/> se il punto non conta.</summary>
        private static CodeRuleSiteResult Prepare(CodeEnrichmentRule rule, CodeCallSiteView site, Dictionary<string, List<CodeCallSiteView>> pairedByCaller,
            IReadOnlyDictionary<string, string> aliasMap, out Skip skip)
        {
            skip = Skip.None;
            if (!site.Arguments.TryGetValue(rule.Call.Argument, out var value)) { skip = Skip.NotLiteral; return null; }
            if (rule.ExcludePrefixes?.Any(p => !string.IsNullOrEmpty(p) && value.StartsWith(p, StringComparison.Ordinal)) == true) { skip = Skip.Excluded; return null; }
            var r = new CodeRuleSiteResult { Site = site, Value = value, MemberName = value };
            string ownerPart = null;
            if (rule.Split != null)
            {
                var cut = rule.Split.At == CodeRuleSplitAt.First ? value.IndexOf(rule.Split.Separator, StringComparison.Ordinal) : value.LastIndexOf(rule.Split.Separator, StringComparison.Ordinal);
                if (cut <= 0 || cut + rule.Split.Separator.Length >= value.Length) { r.Miss = CodeRuleMiss.NoSeparator; return r; }
                ownerPart = value.Substring(0, cut);
                r.MemberName = value.Substring(cut + rule.Split.Separator.Length);
            }
            switch (rule.Owner.From)
            {
                case CodeRuleOwnerSource.String:
                    r.OwnerKey = ownerPart;
                    break;
                case CodeRuleOwnerSource.Class:
                    r.OwnerKey = rule.Owner.Class;
                    break;
                case CodeRuleOwnerSource.PairedCall:
                    r.OwnerKey = PairedValue(rule.Owner.PairedCall, site, pairedByCaller);
                    if (r.OwnerKey == null) { r.Miss = CodeRuleMiss.NoPairedCall; return r; }
                    break;
            }
            if (rule.Owner.AliasMap != null)
            {
                if (aliasMap == null || !aliasMap.TryGetValue(r.OwnerKey, out var mapped) || string.IsNullOrWhiteSpace(mapped)) { r.Miss = CodeRuleMiss.AliasNotInMap; return r; }
                r.OwnerKey = mapped;
            }
            return r;
        }

        /// <summary>
        /// La chiamata in coppia: nello stesso metodo, quella scritta nel ricevente (<c>crea("a").imposta("m")</c>) o, se
        /// no, l'ultima prima di questa. Deterministico; la prova a secco e il campione dicono se la convenzione regge.
        /// </summary>
        private static string PairedValue(CodeRuleCall pair, CodeCallSiteView site, Dictionary<string, List<CodeCallSiteView>> pairedByCaller)
        {
            if (!pairedByCaller.TryGetValue(site.CallerId, out var candidates)) return null;
            CodeCallSiteView chosen = null;
            if (site.Receiver != null && site.Receiver.Contains(pair.Callee + "(", StringComparison.Ordinal))
                chosen = candidates.FirstOrDefault(c => c.Line == site.Line && c.Arguments.ContainsKey(pair.Argument));
            chosen ??= candidates.LastOrDefault(c => (c.Line < site.Line || c.Line == site.Line && c.Ordinal < site.Ordinal) && c.Arguments.ContainsKey(pair.Argument));
            return chosen != null && chosen.Arguments.TryGetValue(pair.Argument, out var v) ? v : null;
        }
    }
}
