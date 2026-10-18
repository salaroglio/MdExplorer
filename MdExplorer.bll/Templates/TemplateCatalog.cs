using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using MdExplorer.Features.Services;

namespace MdExplorer.Features.Templates
{
    /// <summary>
    /// L'elenco dei template che MdExplorer propone in «Crea nuovo progetto» (TD4: un file distribuito con MDE,
    /// <c>project-templates.json</c>, aggiornato a ogni rilascio). È solo una vetrina: ciò che fa fede è il manifesto del
    /// repository, letto alla creazione.
    /// </summary>
    public sealed class TemplateCatalogEntry
    {
        [JsonPropertyName("id")] public string Id { get; set; }
        /// <summary><c>project</c> (una base) o <c>method</c> (un metodo).</summary>
        [JsonPropertyName("kind")] public string Kind { get; set; }
        [JsonPropertyName("url")] public string Url { get; set; }
        [JsonPropertyName("languages")] public List<string> Languages { get; set; }
        /// <summary>Lingua → testo.</summary>
        [JsonPropertyName("title")] public Dictionary<string, string> Title { get; set; }
        [JsonPropertyName("description")] public Dictionary<string, string> Description { get; set; }
    }

    public sealed class TemplateCatalogFile
    {
        [JsonPropertyName("mde_templates")] public int Format { get; set; }
        [JsonPropertyName("templates")] public List<TemplateCatalogEntry> Templates { get; set; }
    }

    /// <summary>Legge l'elenco con tolleranza zero: una chiave sconosciuta o un valore sbagliato sono un errore che dice dove.</summary>
    public static class TemplateCatalog
    {
        public const string ResourceName = "MdExplorer.Service.project-templates.json";

        private static readonly Regex Kebab = new(@"^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.Compiled);
        private static readonly Regex LanguageCode = new(@"^[a-z]{2}$", RegexOptions.Compiled);

        public static IReadOnlyList<TemplateCatalogEntry> Read(string json)
        {
            TemplateCatalogFile file;
            try
            {
                file = JsonSerializer.Deserialize<TemplateCatalogFile>(json ?? "", new JsonSerializerOptions
                {
                    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
                    ReadCommentHandling = JsonCommentHandling.Disallow,
                });
            }
            catch (JsonException ex)
            {
                throw Problem($"l'elenco dei template non si legge: {ex.Message}", $"the template list cannot be read: {ex.Message}");
            }
            if (file?.Format != 1)
                throw Problem($"l'elenco dei template ha il formato {file?.Format}: questo MdExplorer conosce solo 1 (mde_templates: 1).",
                              $"the template list has format {file?.Format}: this MdExplorer knows only 1 (mde_templates: 1).");

            var problems = new List<string>();
            var entries = file.Templates ?? new List<TemplateCatalogEntry>();
            for (var i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                var at = $"templates[{i}]";
                if (e.Id == null || !Kebab.IsMatch(e.Id)) problems.Add($"{at}.id '{e.Id}': kebab-case");
                if (e.Kind != TemplateManifest.Method && e.Kind != TemplateManifest.Project) problems.Add($"{at}.kind '{e.Kind}': method | project");
                if (string.IsNullOrWhiteSpace(e.Url)) problems.Add($"{at}.url");
                if (e.Languages == null || e.Languages.Count == 0 || e.Languages.Any(l => l == null || !LanguageCode.IsMatch(l)))
                    problems.Add($"{at}.languages");
                foreach (var (name, texts) in new[] { ("title", e.Title), ("description", e.Description) })
                    if (texts == null || e.Languages == null || e.Languages.Any(l => string.IsNullOrWhiteSpace(texts.GetValueOrDefault(l ?? ""))))
                        problems.Add($"{at}.{name}: " + UiText.Say("un testo per ogni lingua", "one text per language"));
            }
            foreach (var twin in entries.GroupBy(e => e.Id).Where(g => g.Key != null && g.Count() > 1))
                problems.Add(UiText.Say($"l'id '{twin.Key}' compare più volte", $"the id '{twin.Key}' appears more than once"));
            if (problems.Count > 0)
                throw Problem($"l'elenco dei template non è in regola: {string.Join("; ", problems)}.", $"the template list is not right: {string.Join("; ", problems)}.");
            return entries;
        }

        private static InvalidOperationException Problem(string it, string en) => new(UiText.Say(it, en));
    }
}
