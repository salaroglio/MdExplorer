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
        /// Nomi riservati non registrabili (§6): il cittadino speciale <c>user</c>
        /// e il grafo condiviso <c>shared</c>.
        /// </summary>
        public static readonly string[] ReservedNames = { "user", "shared" };

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

            if (wrapper.A2a == null)
            {
                // chiave a2a: presente ma vuota/null → cittadino dichiarato senza contenuto
                return declaresA2a
                    ? Invalid("Blocco 'a2a:' presente ma vuoto: manca 'name'.")
                    : NotACitizen();
            }

            return Validate(wrapper.A2a, wrapper.Tools);
        }

        private static AgentCardParseResult Validate(AgentCardDescriptor card, List<string> tools)
        {
            var error = ValidateAgentName(card.Name);
            if (error != null)
                return Invalid(error);

            card.Name = card.Name.Trim();
            card.Skills ??= new List<AgentCardSkill>();
            card.AcceptsMessagesFrom ??= new List<string>();
            return new AgentCardParseResult
            {
                Card = card,
                Tools = tools,
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
                return $"Nome agente '{name}' riservato: 'user' e 'shared' non sono registrabili.";

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

        private static AgentCardParseResult NotACitizen()
            => new AgentCardParseResult { HasA2aBlock = false, IsValid = true };

        private static AgentCardParseResult Invalid(string reason)
            => new AgentCardParseResult { HasA2aBlock = true, IsValid = false, RegistrationError = reason };

        // Wrapper: cattura la sezione a2a: e il campo tools:, il resto è ignorato.
        private class A2aFrontmatterWrapper
        {
            public AgentCardDescriptor A2a { get; set; }
            public List<string> Tools { get; set; }
        }
    }
}
