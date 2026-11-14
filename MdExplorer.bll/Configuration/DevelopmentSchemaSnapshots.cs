using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MdExplorer.Features.Configuration
{
    /// <summary>
    /// Le istantanee dello schema di <c>.development.yml</c> che questo MDE porta dentro di sé, una per numero
    /// (<c>Configuration/Schemas/development-schema-N.json</c>, sprint docs-internal/Sprints/2026-11-07-Schema-Development-Yml.md).
    /// Stanno qui e non nel servizio perché le leggono anche i lettori di questa libreria (grafo dei concetti, compatibilità);
    /// l'istantanea del numero di oggi la produce il servizio dal modello C# (<c>DevelopmentSchema.FromModel</c>), e un test
    /// controlla che le due coincidano.
    /// </summary>
    public static class DevelopmentSchemaSnapshots
    {
        /// <summary>
        /// Il numero di schema di questo MDE. Alzalo, e aggiungi l'istantanea, quando cambia il modello del file.
        /// 1 = il file scritto fino al 07/11 (voci vuote omesse); 2 = le stesse voci, tutte presenti nei gruppi scritti (D6, F3); 3 = il gruppo agentCards (R8, 08/11);
        /// 4 = il gruppo seeds, i semi del grafo del codice (F3b, 10/11).
        /// </summary>
        public const int Current = 4;

        /// <summary>Le istantanee incorporate, dalla 1 a <see cref="Current"/>.</summary>
        public static IReadOnlyList<SchemaSnapshot> All => _all.Value;

        private static readonly Lazy<IReadOnlyList<SchemaSnapshot>> _all = new(() => Enumerable.Range(1, Current).Select(Load).ToList());

        public static string ResourceName(int version) => $"MdExplorer.Features.Configuration.Schemas.development-schema-{version}.json";

        private static SchemaSnapshot Load(int version)
        {
            using var stream = typeof(DevelopmentSchemaSnapshots).Assembly.GetManifestResourceStream(ResourceName(version))
                ?? throw new InvalidOperationException(
                    $"Manca l'istantanea dello schema {version} di .development.yml ({ResourceName(version)}): ogni numero fino a {Current} deve averne una. / " +
                    $"The snapshot of .development.yml schema {version} is missing ({ResourceName(version)}): every number up to {Current} needs one.");
            using var reader = new StreamReader(stream);
            return FromJson(reader.ReadToEnd());
        }

        private static readonly JsonSerializerOptions Json = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public static string ToJson(SchemaSnapshot snapshot) => JsonSerializer.Serialize(snapshot, Json).Replace("\r\n", "\n") + "\n";

        public static SchemaSnapshot FromJson(string json) => JsonSerializer.Deserialize<SchemaSnapshot>(json, Json);
    }

    public sealed class SchemaSnapshot
    {
        public int Version { get; set; }

        /// <summary>True solo per lo schema 1: fino al 07/11 MDE non scriveva le voci vuote (D6 vale dallo schema 2).</summary>
        public bool MissingKeysAllowed { get; set; }

        public SchemaNode Root { get; set; }
    }

    public enum SchemaKind { Group, List, Text, Flag, Number }

    public sealed class SchemaNode
    {
        public SchemaKind Kind { get; set; }

        /// <summary>Per un gruppo: le sue voci.</summary>
        public SortedDictionary<string, SchemaNode> Keys { get; set; }

        /// <summary>Per una lista: com'è fatto un elemento.</summary>
        public SchemaNode Item { get; set; }

        /// <summary>Per un gruppo che è elemento di una lista: la voce che lo riconosce.</summary>
        public string ItemKey { get; set; }

        /// <summary>Per un testo a elenco chiuso: i valori ammessi.</summary>
        public List<string> Choices { get; set; }
    }
}
