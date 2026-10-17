using MdExplorer.Features.Yaml.Interfaces;
using MdExplorer.Features.Yaml.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MdExplorer.Features.Yaml
{
    /// <summary>
    /// Estrae la Agent Card A2A dal blocco <c>a2a:</c> del frontmatter di un
    /// <c>.agent.md</c> (§5 del design doc Agent-Harness-A2A). Fotocopia del
    /// pattern di <see cref="YamlDocumentDescriptorParser"/> (stessa regex, stessa
    /// naming convention) ma con esito fail-loud:
    /// <list type="bullet">
    /// <item><c>a2a:</c> assente → file retrocompatibile, non cittadino (nessun errore).</item>
    /// <item><c>a2a:</c> presente ma malformato/invalido → esclusione con
    /// <see cref="AgentCardParseResult.RegistrationError"/> visibile in UI.</item>
    /// </list>
    /// </summary>
    public class YamlAgentCardParser : IYamlAgentCardParser
    {
        // Stesso pattern di YamlDocumentDescriptorParser: primo blocco frontmatter.
        private static readonly Regex FrontmatterRx = new Regex(@"^-{3}(.*?)-{3}",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        // Rileva la PRESENZA della chiave a2a: per distinguere "assente" da "malformato".
        private static readonly Regex A2aKeyRx = new Regex(@"(?m)^\s*a2a\s*:",
            RegexOptions.Compiled);

        // kebab-case: minuscole, cifre, trattini singoli interni. Esclude '@' e maiuscole.
        private static readonly Regex KebabCaseRx = new Regex(@"^[a-z0-9]+(-[a-z0-9]+)*$",
            RegexOptions.Compiled);

        /// <summary>
        /// Nomi riservati non registrabili (§6): il cittadino speciale <c>user</c>, il grafo
        /// condiviso <c>shared</c> e <c>external</c> — identità di sistema assegnata dal gateway
        /// ai mittenti anonimi (vedi <c>MessageAuthorization.ExternalSender</c>). Riservarlo
        /// impedisce che un cittadino omonimo faccia collidere e rifiutare il traffico anonimo.
        /// </summary>
        public static readonly string[] ReservedNames = { "user", "shared", "external" };

        public AgentCardParseResult GetDescriptor(string markdown)
        {
            var matches = FrontmatterRx.Matches(markdown ?? string.Empty);
            if (matches.Count == 0)
                return NotACitizen(); // niente frontmatter: retrocompatibile

            var yaml = matches[0].Groups[1].Value;
            var declaresA2a = A2aKeyRx.IsMatch(yaml);

            A2aFrontmatterWrapper wrapper;
            try
            {
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance)
                    .IgnoreUnmatchedProperties() // description/tools/mde ignorati
                    .Build();
                wrapper = deserializer.Deserialize<A2aFrontmatterWrapper>(yaml)
                          ?? new A2aFrontmatterWrapper();
            }
            catch (Exception ex)
            {
                // YAML malformato: è un errore fail-loud SOLO se il file rivendica cittadinanza.
                return declaresA2a
                    ? Invalid($"Blocco 'a2a:' malformato: {ex.Message}")
                    : NotACitizen();
            }

            if (declaresA2a)
            {
                var unknown = UnknownA2aKey(yaml);
                if (unknown != null) return Invalid(unknown);
            }

            if (wrapper.A2a == null)
            {
                // chiave a2a: presente ma vuota/null → cittadino dichiarato senza contenuto
                // runtime: vale anche per un agente che non è un cittadino: lanciato a mano o da una
                // pianificazione, gira sul motore che dichiara (sprint 2026-09-29-Motore-LLM-Unico, F5).
                return declaresA2a
                    ? Invalid("Blocco 'a2a:' presente ma vuoto: manca 'name'.")
                    : NotACitizen(wrapper.Runtime);
            }

            return Validate(wrapper.A2a, wrapper.Tools, wrapper.Runtime);
        }

        private static AgentCardParseResult Validate(AgentCardDescriptor card, List<string> tools, AgentRuntimeDescriptor runtime)
        {
            var error = ValidateAgentName(card.Name);
            if (error != null)
                return Invalid(error);

            card.Name = card.Name.Trim();
            if (card.Summary != null)
            {
                card.Summary = card.Summary.Trim();
                if (card.Summary.Length == 0) card.Summary = null;
                else if (card.Summary.Length > AgentCardDescriptor.SummaryMaxLength)
                    return Invalid($"'summary' è lungo {card.Summary.Length} caratteri: il massimo è {AgentCardDescriptor.SummaryMaxLength}. " +
                                   "Chi autorizza l'agente lo legge per intero, deve restare breve.");
            }
            card.Skills ??= new List<AgentCardSkill>();
            card.AcceptsMessagesFrom ??= new List<string>();
            error = ValidateInterfaces(card);
            if (error != null)
                return Invalid(error);
            return new AgentCardParseResult
            {
                Card = card,
                Tools = tools,
                Runtime = runtime,
                HasA2aBlock = true,
                IsValid = true,
            };
        }

        /// <summary>
        /// Regole di identità del nome agente (§5, §6, §12.6), unica fonte di verità
        /// riusata anche per gli agenti algoritmici (<c>IAlgorithmicAgent.GetCard</c>).
        /// Restituisce null se il nome è valido, altrimenti il motivo fail-loud.
        /// </summary>
        public static string ValidateAgentName(string rawName)
        {
            var name = rawName?.Trim();
            if (string.IsNullOrEmpty(name))
                return "Manca il nome dell'agente ('name'): identità obbligatoria.";

            // '@' è riservato ai nomi federati (agente@gitEmail, §12.6): vietato esplicitamente.
            if (name.Contains('@'))
                return $"Nome agente '{name}' non valido: il carattere '@' è riservato ai nomi federati.";

            if (ReservedNames.Contains(name, StringComparer.OrdinalIgnoreCase))
                return $"Nome agente '{name}' riservato: 'user', 'shared' ed 'external' non sono registrabili.";

            if (!KebabCaseRx.IsMatch(name))
                return $"Nome agente '{name}' non valido: usare kebab-case (minuscole, cifre e trattini).";

            return null;
        }

        /// <summary>
        /// Regola di unicità cross-file (§6): due card con lo stesso <c>a2a.name</c>
        /// vanno <b>entrambe</b> escluse dal registry, in modo deterministico
        /// (mai "vince il primo"). Restituisce i nomi duplicati (case-insensitive);
        /// il registry (Fase 1, step 4) userà l'esito per assegnare il
        /// <see cref="AgentCardParseResult.RegistrationError"/> a tutte le card coinvolte.
        /// </summary>
        public static ISet<string> FindDuplicateNames(IEnumerable<string> names)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var duplicates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var raw in names ?? Enumerable.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(raw))
                    continue;
                var name = raw.Trim();
                if (!seen.Add(name))
                    duplicates.Add(name);
            }
            return duplicates;
        }

        /// <summary>
        /// Tolleranza zero sul blocco <c>a2a:</c>: una chiave che MDE non conosce è un errore con il suo percorso, mai un
        /// silenzio (un refuso in <c>outputs</c> farebbe sparire dove l'agente scrive). Il blocco si rilegge come dati grezzi
        /// e si confronta, livello per livello, con le proprietà del modello. Il resto del frontmatter (description, tools,
        /// mde…) non è affare della scheda e resta libero.
        /// </summary>
        private static string UnknownA2aKey(string yaml)
        {
            Dictionary<object, object> root;
            try { root = new DeserializerBuilder().Build().Deserialize<Dictionary<object, object>>(yaml); }
            catch (Exception) { return null; }   // il frontmatter malformato lo segnala già la lettura principale
            if (root == null || !root.TryGetValue("a2a", out var a2a) || a2a == null) return null;
            return UnknownKey(a2a, typeof(AgentCardDescriptor), "a2a");
        }

        private static string UnknownKey(object node, Type model, string path)
        {
            if (node is not Dictionary<object, object> map) return null;   // i tipi li controlla la lettura principale
            var props = model.GetProperties().ToDictionary(p => UnderscoredNamingConvention.Instance.Apply(p.Name), p => p, StringComparer.Ordinal);
            foreach (var kv in map)
            {
                var key = kv.Key?.ToString();
                if (key == null || !props.TryGetValue(key, out var prop))
                    return MdExplorer.Features.Services.UiText.Say(
                        $"Chiave sconosciuta '{path}.{key}': le chiavi ammesse sono {string.Join(", ", props.Keys)}.",
                        $"Unknown key '{path}.{key}': the allowed keys are {string.Join(", ", props.Keys)}.");
                var type = prop.PropertyType;
                var item = type.IsGenericType && type.GetGenericArguments().Length == 1 ? type.GetGenericArguments()[0] : null;
                if (item != null && item != typeof(string) && kv.Value is List<object> list)
                {
                    for (var i = 0; i < list.Count; i++)
                    {
                        var inner = UnknownKey(list[i], item, $"{path}.{key}[{i}]");
                        if (inner != null) return inner;
                    }
                }
                else if (type.IsClass && type != typeof(string) && item == null)
                {
                    var inner = UnknownKey(kv.Value, type, $"{path}.{key}");
                    if (inner != null) return inner;
                }
            }
            return null;
        }

        /// <summary>
        /// Le interfacce dei lavori: ogni nome una volta sola, i percorsi delle uscite e dei documenti letti presenti, il
        /// verbo di un'uscita tra quelli ammessi, i pulsanti di un lavoro dichiarati nella scheda.
        /// </summary>
        private static string ValidateInterfaces(AgentCardDescriptor card)
        {
            static string Say(string it, string en) => MdExplorer.Features.Services.UiText.Say(it, en);
            var replies = new HashSet<string>((card.Replies ?? new List<AgentCardReply>()).Select(r => r?.Id).Where(id => id != null), StringComparer.Ordinal);

            var skillIds = new HashSet<string>(StringComparer.Ordinal);
            for (var s = 0; s < card.Skills.Count; s++)
            {
                var skill = card.Skills[s];
                var at = $"a2a.skills[{s}]";
                if (string.IsNullOrWhiteSpace(skill?.Id))
                    return Say($"'{at}' non ha 'id'.", $"'{at}' has no 'id'.");
                if (!skillIds.Add(skill.Id))
                    return Say($"La skill '{skill.Id}' è dichiarata due volte.", $"The skill '{skill.Id}' is declared twice.");

                var inputIds = new HashSet<string>(StringComparer.Ordinal);
                for (var i = 0; i < (skill.Inputs?.Count ?? 0); i++)
                {
                    var id = skill.Inputs[i]?.Id;
                    if (string.IsNullOrWhiteSpace(id))
                        return Say($"'{at}.inputs[{i}]' non ha 'id'.", $"'{at}.inputs[{i}]' has no 'id'.");
                    if (!inputIds.Add(id))
                        return Say($"L'ingresso '{id}' di '{skill.Id}' è dichiarato due volte.", $"The input '{id}' of '{skill.Id}' is declared twice.");
                }

                for (var i = 0; i < (skill.Reads?.Count ?? 0); i++)
                    if (string.IsNullOrWhiteSpace(skill.Reads[i]?.Path))
                        return Say($"'{at}.reads[{i}]' non ha 'path'.", $"'{at}.reads[{i}]' has no 'path'.");

                var outputIds = new HashSet<string>(StringComparer.Ordinal);
                for (var i = 0; i < (skill.Outputs?.Count ?? 0); i++)
                {
                    var o = skill.Outputs[i];
                    if (string.IsNullOrWhiteSpace(o?.Id))
                        return Say($"'{at}.outputs[{i}]' non ha 'id'.", $"'{at}.outputs[{i}]' has no 'id'.");
                    if (!outputIds.Add(o.Id))
                        return Say($"L'uscita '{o.Id}' di '{skill.Id}' è dichiarata due volte.", $"The output '{o.Id}' of '{skill.Id}' is declared twice.");
                    if (string.IsNullOrWhiteSpace(o.Path))
                        return Say($"L'uscita '{o.Id}' di '{skill.Id}' non ha 'path': dove scrive l'agente va detto.",
                                   $"The output '{o.Id}' of '{skill.Id}' has no 'path': where the agent writes must be stated.");
                    if (o.Action != null && !AgentCardOutput.Actions.Contains(o.Action))
                        return Say($"L'uscita '{o.Id}' di '{skill.Id}' ha 'action: {o.Action}': i valori ammessi sono {string.Join(", ", AgentCardOutput.Actions)}.",
                                   $"The output '{o.Id}' of '{skill.Id}' has 'action: {o.Action}': the allowed values are {string.Join(", ", AgentCardOutput.Actions)}.");
                }

                foreach (var reply in skill.Replies ?? new List<string>())
                    if (!replies.Contains(reply))
                        return Say($"La skill '{skill.Id}' propone il pulsante '{reply}', che non è tra le 'replies' della scheda.",
                                   $"The skill '{skill.Id}' proposes the button '{reply}', which is not among the card's 'replies'.");
            }

            foreach (var reply in card.Replies ?? new List<AgentCardReply>())
            {
                var values = reply?.Values ?? new List<string>();
                if (values.Any(string.IsNullOrWhiteSpace))
                    return Say($"Il pulsante '{reply.Id}' ha un valore senza nome.", $"The button '{reply.Id}' has a value with no name.");
                var twice = values.GroupBy(v => v, StringComparer.Ordinal).FirstOrDefault(g => g.Count() > 1);
                if (twice != null)
                    return Say($"Il pulsante '{reply.Id}' porta due volte il valore '{twice.Key}'.", $"The button '{reply.Id}' carries the value '{twice.Key}' twice.");
            }
            return null;
        }

        private static AgentCardParseResult NotACitizen(AgentRuntimeDescriptor runtime = null)
            => new AgentCardParseResult { HasA2aBlock = false, IsValid = true, Runtime = runtime };

        private static AgentCardParseResult Invalid(string reason)
            => new AgentCardParseResult { HasA2aBlock = true, IsValid = false, RegistrationError = reason };

        // Wrapper: cattura la sezione a2a:, il campo tools: e il blocco runtime:, il resto è ignorato.
        private class A2aFrontmatterWrapper
        {
            public AgentCardDescriptor A2a { get; set; }
            public List<string> Tools { get; set; }
            public AgentRuntimeDescriptor Runtime { get; set; }
        }
    }
}
