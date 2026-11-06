using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Una regola di arricchimento del grafo del codice (D42, 23/10/2026): una CONVENZIONE del programma che il parser non
    /// vede, scritta come dati. Gira dopo la scansione, legge i punti di chiamata con stringhe (<see cref="ScriptCallSite"/>)
    /// e aggiunge archi <c>dispatches</c> dal chiamante al metodo che la stringa nomina. Sta nel progetto, in
    /// <c>.mde-code/_rules/&lt;id&gt;.rule.json</c>, condivisa con Git: in MdExplorer c'è solo il meccanismo.
    /// <para>Esempio: <c>inviaPagina('aliasController.metodo')</c> → divisa all'ultimo punto, l'alias diventa la classe con
    /// la mappa di un file <c>.properties</c>, il metodo si cerca in quella classe.</para>
    /// </summary>
    public sealed class CodeEnrichmentRule
    {
        public string Id { get; set; }
        /// <summary>
        /// <c>dispatch</c> (predefinito, D42): una stringa nomina il metodo eseguito → archi <c>dispatches</c>.
        /// <c>registration</c> (D43, 30/10/2026): un'astrazione si risolve in un'implementazione dichiarata altrove (Guice
        /// <c>bind(X.class).to(Y.class)</c>, <c>@ImplementedBy</c>, <c>@Provides</c>…) → archi <c>resolvesTo</c>.
        /// </summary>
        public CodeRuleKind Kind { get; set; } = CodeRuleKind.Dispatch;
        /// <summary>Che cosa fa la regola, in parole semplici: la legge l'utente prima di dire sì.</summary>
        public string Description { get; set; }
        /// <summary>
        /// La domanda all'LLM per descrivere OGNI arco che la regola produce (D43, utente 30/10/2026: «una domanda specifica
        /// per creare una descrizione associata ai nuovi grafi»), facoltativa. Si fa dentro «Descrivi con Mark», che la conta
        /// nell'anteprima; la risposta è la descrizione dell'arco, che gli strumenti <c>code_*</c> restituiscono.
        /// </summary>
        public string Describe { get; set; }
        /// <summary>Solo <c>dispatch</c>: la chiamata che porta la stringa.</summary>
        public CodeRuleCall Call { get; set; }
        /// <summary>Solo <c>registration</c>: il fatto da cui nasce ogni registrazione, una chiamata o un'annotazione.</summary>
        public CodeRuleAnchor Anchor { get; set; }
        /// <summary>Solo <c>registration</c>: dove si legge l'astrazione (l'interfaccia, il tipo richiesto).</summary>
        public CodeRuleEnd Abstraction { get; set; }
        /// <summary>Solo <c>registration</c>: dove si legge l'implementazione (la classe, o il metodo che la fornisce).</summary>
        public CodeRuleEnd Implementation { get; set; }
        /// <summary>Dove si divide la stringa in proprietario + metodo (<c>alias.metodo</c>); null = la stringa è il metodo.</summary>
        public CodeRuleSplit Split { get; set; }
        /// <summary>Come si trova la classe che ha il metodo.</summary>
        public CodeRuleOwner Owner { get; set; }
        /// <summary>Le stringhe con questi prefissi non nominano un metodo (<c>redirect:</c>…): si contano a parte.</summary>
        public List<string> ExcludePrefixes { get; set; } = new();
        /// <summary>
        /// La portata: solo le chiamate nei file sotto questi percorsi logici (<c>program/web/</c>…). Vuota = tutto il
        /// grafo. La sceglie l'utente nel wizard, dai contesti della prova a secco.
        /// </summary>
        public List<string> Scope { get; set; } = new();
        public string WrittenBy { get; set; }
        public string WrittenAt { get; set; }

        public static readonly Regex IdShape = new("^[a-z0-9][a-z0-9-]{0,62}$", RegexOptions.CultureInvariant);

        private static readonly JsonSerializerOptions Json = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false) },
        };

        public static CodeEnrichmentRule Parse(string json)
        {
            CodeEnrichmentRule rule;
            try { rule = JsonSerializer.Deserialize<CodeEnrichmentRule>(json, Json); }
            catch (JsonException ex) { throw new CodeRuleException(UiText.Say("La regola non è un JSON valido: ", "The rule is not valid JSON: ") + ex.Message); }
            rule?.Validate();
            return rule ?? throw new CodeRuleException(UiText.Say("La regola è vuota.", "The rule is empty."));
        }

        public string ToJson() => JsonSerializer.Serialize(this, Json);

        /// <summary>Una regola incompleta è un errore che dice che cosa manca: mai una regola applicata a metà.</summary>
        public void Validate()
        {
            static CodeRuleException Bad(string it, string en) => new(UiText.Say(it, en));
            if (string.IsNullOrWhiteSpace(Id) || !IdShape.IsMatch(Id))
                throw Bad($"id '{Id}' non valido: minuscole, cifre e trattini, fino a 63 caratteri.", $"invalid id '{Id}': lowercase letters, digits and dashes, up to 63 characters.");
            if (string.IsNullOrWhiteSpace(Description))
                throw Bad("manca description: che cosa fa la regola, in parole semplici.", "description missing: what the rule does, in plain words.");
            if (Describe != null && string.IsNullOrWhiteSpace(Describe))
                throw Bad("describe vuota: togli il campo o scrivi la domanda.", "describe is empty: remove the field or write the question.");
            if (Kind == CodeRuleKind.Registration) { ValidateRegistration(); return; }
            if (Anchor != null || Abstraction != null || Implementation != null)
                throw Bad("anchor, abstraction e implementation valgono solo con kind = registration.", "anchor, abstraction and implementation are only for kind = registration.");
            if (Call == null || string.IsNullOrWhiteSpace(Call.Callee) || Call.Argument < 0)
                throw Bad("call vuole callee (il nome chiamato) e argument (la posizione della stringa, da 0).", "call needs callee (the called name) and argument (the string's position, from 0).");
            if (Split != null && string.IsNullOrEmpty(Split.Separator))
                throw Bad("split vuole separator.", "split needs separator.");
            if (Owner == null) throw Bad("manca owner: come si trova la classe del metodo.", "owner missing: how the method's class is found.");
            switch (Owner.From)
            {
                case CodeRuleOwnerSource.String:
                    if (Split == null) throw Bad("owner.from = string vuole split: il proprietario è la parte prima del separatore.", "owner.from = string needs split: the owner is the part before the separator.");
                    break;
                case CodeRuleOwnerSource.PairedCall:
                    if (Owner.PairedCall == null || string.IsNullOrWhiteSpace(Owner.PairedCall.Callee) || Owner.PairedCall.Argument < 0)
                        throw Bad("owner.from = pairedCall vuole pairedCall con callee e argument.", "owner.from = pairedCall needs pairedCall with callee and argument.");
                    break;
                case CodeRuleOwnerSource.Class:
                    if (string.IsNullOrWhiteSpace(Owner.Class)) throw Bad("owner.from = class vuole class.", "owner.from = class needs class.");
                    break;
            }
            if (Owner.AliasMap != null && (Path.IsPathRooted(Owner.AliasMap) || Owner.AliasMap.Contains("..")))
                throw Bad($"aliasMap '{Owner.AliasMap}' deve essere un percorso relativo alla radice del progetto.", $"aliasMap '{Owner.AliasMap}' must be a path relative to the project root.");
        }

        /// <summary>Le combinazioni che hanno senso: ogni capo si legge da un fatto che l'àncora ha davvero.</summary>
        private void ValidateRegistration()
        {
            static CodeRuleException Bad(string it, string en) => new(UiText.Say(it, en));
            if (Call != null || Owner != null || Split != null || ExcludePrefixes?.Count > 0)
                throw Bad("kind = registration non usa call, owner, split, excludePrefixes: usa anchor, abstraction, implementation.", "kind = registration does not use call, owner, split, excludePrefixes: use anchor, abstraction, implementation.");
            if (Anchor == null || (Anchor.Call == null) == (Anchor.Annotation == null))
                throw Bad("anchor vuole call (il nome chiamato) oppure annotation (il nome dell'annotazione), uno solo.", "anchor needs call (the called name) or annotation (the annotation name), exactly one.");
            if (Anchor.Call != null && string.IsNullOrWhiteSpace(Anchor.Call)) throw Bad("anchor.call vuoto.", "anchor.call is empty.");
            if (Anchor.Annotation != null && string.IsNullOrWhiteSpace(Anchor.Annotation)) throw Bad("anchor.annotation vuoto.", "anchor.annotation is empty.");
            foreach (var (end, name) in new[] { (Abstraction, "abstraction"), (Implementation, "implementation") })
            {
                if (end == null) throw Bad($"manca {name}.", $"{name} missing.");
                var onCall = Anchor.Call != null;
                switch (end.From)
                {
                    case CodeRuleEndSource.Argument when !onCall:
                    case CodeRuleEndSource.Chain when !onCall:
                        throw Bad($"{name}.from = {Name(end.From)} vale solo con anchor.call.", $"{name}.from = {Name(end.From)} is only for anchor.call.");
                    case CodeRuleEndSource.Annotated when onCall:
                    case CodeRuleEndSource.ReturnType when onCall:
                    case CodeRuleEndSource.AnnotationArgument when onCall:
                        throw Bad($"{name}.from = {Name(end.From)} vale solo con anchor.annotation.", $"{name}.from = {Name(end.From)} is only for anchor.annotation.");
                    case CodeRuleEndSource.Argument when end.Argument is null or < 0:
                        throw Bad($"{name}.from = argument vuole argument (la posizione di X.class, da 0).", $"{name}.from = argument needs argument (the position of X.class, from 0).");
                    case CodeRuleEndSource.Chain when string.IsNullOrWhiteSpace(end.Callee) || end.Argument is null or < 0:
                        throw Bad($"{name}.from = chain vuole callee (la chiamata più giù nella catena) e argument.", $"{name}.from = chain needs callee (the call further down the chain) and argument.");
                }
            }
            if (Abstraction.From == CodeRuleEndSource.Chain && Implementation.From == CodeRuleEndSource.Chain && Abstraction.Callee != Implementation.Callee)
                throw Bad("abstraction e implementation dalla catena: la stessa chiamata (callee), con argomenti diversi.", "abstraction and implementation from the chain: the same call (callee), with different arguments.");
            if (Abstraction.From == Implementation.From && Abstraction.From is CodeRuleEndSource.Annotated or CodeRuleEndSource.ReturnType)
                throw Bad("abstraction e implementation non possono venire dallo stesso posto.", "abstraction and implementation cannot come from the same place.");
            static string Name(CodeRuleEndSource s) => JsonNamingPolicy.CamelCase.ConvertName(s.ToString());
        }
    }

    [JsonConverter(typeof(CamelCaseEnumConverter))]
    public enum CodeRuleKind { Dispatch, Registration }

    /// <summary>
    /// Il fatto da cui nasce una registrazione (D43): una chiamata (<c>to</c>, ogni suo punto con un argomento X.class) o
    /// un'annotazione (<c>ImplementedBy</c>, <c>Provides</c>: il nome com'è scritto, o qualificato).
    /// </summary>
    public sealed class CodeRuleAnchor
    {
        public string Call { get; set; }
        public string Annotation { get; set; }
    }

    [JsonConverter(typeof(CamelCaseEnumConverter))]
    public enum CodeRuleEndSource
    {
        /// <summary>L'argomento X.class dell'àncora in posizione <c>argument</c>.</summary>
        Argument,
        /// <summary>L'argomento X.class (posizione <c>argument</c>) della chiamata <c>callee</c> più giù nella catena dell'àncora: <c>bind(X.class)…to(Y.class)</c>.</summary>
        Chain,
        /// <summary>Il simbolo che porta l'annotazione (la classe di <c>@ImplementedBy</c>, il metodo di <c>@Provides</c>).</summary>
        Annotated,
        /// <summary>Il tipo restituito dal metodo annotato (<c>@Provides Exporter make()</c> → Exporter).</summary>
        ReturnType,
        /// <summary>Un argomento X.class dell'annotazione (<c>element</c>, predefinito <c>value</c>).</summary>
        AnnotationArgument,
    }

    public sealed class CodeRuleEnd
    {
        public CodeRuleEndSource From { get; set; }
        public int? Argument { get; set; }
        public string Callee { get; set; }
        public string Element { get; set; }
    }

    public sealed class CodeRuleCall
    {
        public string Callee { get; set; }
        /// <summary>La posizione della stringa fra gli argomenti, da 0.</summary>
        public int Argument { get; set; }
    }

    public sealed class CodeRuleSplit
    {
        public string Separator { get; set; }
        /// <summary><c>last</c> (predefinito) o <c>first</c> occorrenza del separatore.</summary>
        public CodeRuleSplitAt At { get; set; } = CodeRuleSplitAt.Last;
    }

    [JsonConverter(typeof(CamelCaseEnumConverter))]
    public enum CodeRuleSplitAt { Last, First }

    [JsonConverter(typeof(CamelCaseEnumConverter))]
    public enum CodeRuleOwnerSource
    {
        /// <summary>La parte della stringa prima del separatore.</summary>
        String,
        /// <summary>La stringa di un'altra chiamata dello stesso metodo, prima di questa (<c>creaServizio("alias")</c> + <c>impostaDestinazione("m")</c>).</summary>
        PairedCall,
        /// <summary>Una classe fissa.</summary>
        Class,
        /// <summary>
        /// La stringa nomina una PAGINA, non un metodo (D42 tipo 3, 24/10/2026): <c>impostaDestinazione("ordini")</c> mostra
        /// <c>ordini.template</c>. L'arco va alla pagina di un dialetto di pagine del progetto, cercata per nome (con o
        /// senza estensione) o per percorso che finisce così (<c>ordini/elenco</c>).
        /// </summary>
        Page,
    }

    public sealed class CodeRuleOwner
    {
        public CodeRuleOwnerSource From { get; set; }
        public CodeRuleCall PairedCall { get; set; }
        public string Class { get; set; }
        /// <summary>Un file <c>.properties</c> (relativo al progetto) che traduce l'alias in classe: <c>alias=com.x.Classe</c>.</summary>
        public string AliasMap { get; set; }
    }

    /// <summary>Gli enum delle regole in JSON come nel file: <c>pairedCall</c>, non 1 né <c>PairedCall</c> (anche nelle risposte delle API).</summary>
    public sealed class CamelCaseEnumConverter : JsonStringEnumConverter
    {
        public CamelCaseEnumConverter() : base(JsonNamingPolicy.CamelCase, allowIntegerValues: false) { }
    }

    public sealed class CodeRuleException : Exception
    {
        public CodeRuleException(string message) : base(message) { }
    }

    /// <summary>Un file <c>.properties</c> come mappa: <c>chiave=valore</c> o <c>chiave: valore</c>, commenti con # e !.</summary>
    public static class PropertiesFile
    {
        public static Dictionary<string, string> Parse(string content)
        {
            var map = new Dictionary<string, string>(StringComparer.Ordinal);
            string pending = null;
            foreach (var raw in content.Replace("\r\n", "\n").Split('\n'))
            {
                var line = pending != null ? pending + raw.TrimStart() : raw.Trim();
                pending = null;
                if (line.EndsWith("\\", StringComparison.Ordinal)) { pending = line.Substring(0, line.Length - 1); continue; }
                if (line.Length == 0 || line[0] is '#' or '!') continue;
                var cut = line.IndexOfAny(new[] { '=', ':' });
                if (cut <= 0) continue;
                map[line.Substring(0, cut).Trim()] = line.Substring(cut + 1).Trim();
            }
            return map;
        }
    }
}
