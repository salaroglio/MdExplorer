using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MdExplorer.Features.Agents
{
    /// <summary>
    /// Calcola l'<c>A2ABlockHash</c> (R3, §10): l'impronta del blocco <c>a2a:</c>
    /// <b>+ campo <c>tools:</c></b> fissata al momento del trust. Se cambia, il trust
    /// decade e va riconfermato — così un agente non può allargarsi la whitelist dei
    /// mittenti o i permessi editando la propria dichiarazione dopo l'approvazione.
    /// <para>
    /// Deterministico: stesso contenuto → stesso hash. L'hash si basa sulla
    /// serializzazione canonica dell'oggetto card (formattazione irrilevante, conta il
    /// contenuto) più i tool. Conservativo: qualunque cambiamento di contenuto (nomi
    /// mittenti, skill, max_hops, tools) produce un hash diverso e fa decadere il trust.
    /// </para>
    /// </summary>
    public static class AgentTrustHasher
    {
        private static readonly ISerializer Serializer = new SerializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();

        /// <summary>
        /// Hash del contenuto della card (blocco <c>a2a:</c> serializzato) più i
        /// <paramref name="tools"/> dichiarati. <paramref name="card"/> è l'oggetto
        /// della card (LLM: <c>AgentCardDescriptor</c>; algoritmico: <c>AgentCardInfo</c>).
        /// </summary>
        public static string ComputeHash(object card, IEnumerable<string> tools)
        {
            var cardYaml = card == null ? string.Empty : Serializer.Serialize(card);
            var toolsPart = string.Join(",", (tools ?? Enumerable.Empty<string>())
                .Select(t => (t ?? string.Empty).Trim()));

            var payload = cardYaml + "\n--tools--\n" + toolsPart;

            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}
