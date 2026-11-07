using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using MdExplorer.Features.Configuration.Models;
using MdExplorer.Features.Templates;
using MdExplorer.Service.Models;
using YamlDotNet.Serialization.NamingConventions;

namespace MdExplorer.Utilities
{
    /// <summary>
    /// Lo schema di <c>.development.yml</c> che questo MDE porta dentro di sé (sprint
    /// docs-internal/Sprints/2026-11-07-Schema-Development-Yml.md). Ogni numero ha la sua istantanea incorporata
    /// (<c>Schemas/development-schema-N.json</c>): il controllo riconosce anche i gruppi scritti con uno schema precedente, e un
    /// test confronta l'istantanea del numero di oggi con il modello C#, così nessuno cambia il modello senza alzare il numero.
    /// <para>
    /// I gruppi sono facoltativi, le voci di un gruppo tutte obbligatorie (D6); lo schema 1 è il file scritto fino al 07/11, che
    /// omette le voci vuote, e quindi le ammette mancanti.
    /// </para>
    /// </summary>
    public static class DevelopmentSchema
    {
        /// <summary>Il numero di schema di questo MDE. Alzalo, e aggiungi l'istantanea, quando cambia il modello del file.</summary>
        public const int Current = 1;

        /// <summary>Le istantanee incorporate, dalla 1 a <see cref="Current"/>.</summary>
        public static IReadOnlyList<SchemaSnapshot> Snapshots => _snapshots.Value;

        private static readonly Lazy<IReadOnlyList<SchemaSnapshot>> _snapshots = new(() =>
            Enumerable.Range(1, Current).Select(LoadSnapshot).ToList());

        public static string ResourceName(int version) => $"MdExplorer.Service.Schemas.development-schema-{version}.json";

        private static SchemaSnapshot LoadSnapshot(int version)
        {
            using var stream = typeof(DevelopmentSchema).Assembly.GetManifestResourceStream(ResourceName(version))
                ?? throw new InvalidOperationException(
                    $"Manca l'istantanea dello schema {version} di .development.yml ({ResourceName(version)}): ogni numero fino a {Current} deve averne una. / " +
                    $"The snapshot of .development.yml schema {version} is missing ({ResourceName(version)}): every number up to {Current} needs one.");
            using var reader = new StreamReader(stream);
            return FromJson(reader.ReadToEnd());
        }

        // ---- dal modello C# ----

        /// <summary>
        /// I valori ammessi delle voci a elenco chiuso, presi dal codice e mai scritti a mano (F0: un elenco scritto a mano aveva
        /// dimenticato <c>harness.target: none</c>). Per tipo e proprietà, così valgono ovunque il tipo compaia.
        /// </summary>
        private static readonly Dictionary<(Type, string), Func<IEnumerable<string>>> Choices = new()
        {
            [(typeof(HarnessConfig), nameof(HarnessConfig.Target))] = () => HarnessLayout.All.Select(l => l.Id).Append(HarnessSettings.IdOf(HarnessTarget.None)),
            [(typeof(FolderCodeGraphConfig), nameof(FolderCodeGraphConfig.Describe))] = () => new[] { CodeDescriptionModes.All, CodeDescriptionModes.OnDemand },
            [(typeof(RedmineConfig), nameof(RedmineConfig.TextFormatting))] = () => RedmineConfig.TextFormats,
            [(typeof(ProjectTemplateConfig), nameof(ProjectTemplateConfig.Kind))] = () => new[] { TemplateManifest.Method, TemplateManifest.Project, TemplateManifest.Foundation },
            [(typeof(CompatibilityConfig), nameof(CompatibilityConfig.Mode))] = () => CompatibilityConfig.Modes,
        };

        /// <summary>La voce che riconosce un elemento di lista (per l'avviso e, poi, per la scrittura per gruppo).</summary>
        private static readonly Dictionary<Type, string> ItemKeys = new()
        {
            [typeof(DevelopmentFolder)] = nameof(DevelopmentFolder.Path),
            [typeof(CodeRootConfig)] = nameof(CodeRootConfig.Alias),
            [typeof(CodeRootProgramConfig)] = nameof(CodeRootProgramConfig.Path),
            [typeof(ProjectParticipant)] = nameof(ProjectParticipant.GitEmail),
            [typeof(ProjectTemplateConfig)] = nameof(ProjectTemplateConfig.Id),
        };

        private static string Name(string property) => CamelCaseNamingConvention.Instance.Apply(property);

        /// <summary>Lo schema che il modello C# di oggi descrive.</summary>
        public static SchemaNode FromModel() => Describe(typeof(DevelopmentConfig), "");

        private static SchemaNode Describe(Type type, string where)
        {
            var nullable = Nullable.GetUnderlyingType(type);
            if (nullable != null) type = nullable;
            if (type == typeof(string)) return new SchemaNode { Kind = SchemaKind.Text };
            if (type == typeof(bool)) return new SchemaNode { Kind = SchemaKind.Flag };
            if (type == typeof(int) || type == typeof(long)) return new SchemaNode { Kind = SchemaKind.Number };
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return new SchemaNode { Kind = SchemaKind.List, Item = Describe(type.GetGenericArguments()[0], where + "[]") };
            if (type.IsClass && !typeof(IEnumerable).IsAssignableFrom(type))
            {
                var keys = new SortedDictionary<string, SchemaNode>(StringComparer.Ordinal);
                foreach (var p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead && p.CanWrite))
                {
                    var node = Describe(p.PropertyType, where + "." + Name(p.Name));
                    if (Choices.TryGetValue((type, p.Name), out var values))
                        node.Choices = values().Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(v => v, StringComparer.Ordinal).ToList();
                    keys[Name(p.Name)] = node;
                }
                return new SchemaNode
                {
                    Kind = SchemaKind.Group,
                    Keys = keys,
                    ItemKey = ItemKeys.TryGetValue(type, out var k) ? Name(k) : null,
                };
            }
            // Niente ripieghi: un tipo nuovo nel modello deve dire come si controlla.
            throw new NotSupportedException($"DevelopmentSchema: il tipo {type.Name} di «{where.TrimStart('.')}» non ha una regola di controllo: aggiungila in Describe.");
        }

        // ---- istantanee ----

        private static readonly JsonSerializerOptions Json = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public static string ToJson(SchemaSnapshot snapshot) => JsonSerializer.Serialize(snapshot, Json).Replace("\r\n", "\n") + "\n";

        public static SchemaSnapshot FromJson(string json) => JsonSerializer.Deserialize<SchemaSnapshot>(json, Json);

        /// <summary>L'istantanea che il modello di oggi produrrebbe con il numero <see cref="Current"/>.</summary>
        public static SchemaSnapshot CurrentFromModel(bool missingKeysAllowed) => new()
        {
            Version = Current,
            MissingKeysAllowed = missingKeysAllowed,
            Root = FromModel(),
        };
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
