using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MdExplorer.Features.Configuration;
using MdExplorer.Features.Configuration.Models;
using MdExplorer.Features.Templates;
using MdExplorer.Service.Models;
using YamlDotNet.Serialization.NamingConventions;

namespace MdExplorer.Utilities
{
    /// <summary>
    /// Lo schema di <c>.development.yml</c> come lo descrive il modello C# di oggi (sprint
    /// docs-internal/Sprints/2026-11-07-Schema-Development-Yml.md). Le istantanee incorporate stanno in
    /// <see cref="DevelopmentSchemaSnapshots"/>; un test confronta quella del numero di oggi con <see cref="FromModel"/>, così nessuno
    /// cambia il modello senza alzare il numero.
    /// <para>
    /// I gruppi sono facoltativi, le voci di un gruppo tutte obbligatorie (D6); lo schema 1 è il file scritto fino al 07/11, che
    /// omette le voci vuote, e quindi le ammette mancanti.
    /// </para>
    /// </summary>
    public static class DevelopmentSchema
    {
        /// <summary>Il numero di schema di questo MDE (vedi <see cref="DevelopmentSchemaSnapshots.Current"/>).</summary>
        public const int Current = DevelopmentSchemaSnapshots.Current;

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

        /// <summary>L'istantanea che il modello di oggi produrrebbe con il numero <see cref="Current"/>.</summary>
        public static SchemaSnapshot CurrentFromModel(bool missingKeysAllowed) => new()
        {
            Version = Current,
            MissingKeysAllowed = missingKeysAllowed,
            Root = FromModel(),
        };
    }
}
