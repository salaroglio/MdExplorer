using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MdExplorer.Features.Yaml.Models;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Exports
{
    public class WordTemplateService : IWordTemplateService
    {
        private readonly ILogger<WordTemplateService> _logger;
        private readonly Regex _tagRegex = new Regex(@"\{\{([^}]+)\}\}", RegexOptions.Compiled);

        public WordTemplateService(ILogger<WordTemplateService> logger)
        {
            _logger = logger;
        }

        public async Task<string> InsertPredefinedPagesAsync(
            string markdownContent, 
            MdExplorerDocumentDescriptor descriptor,
            string projectPath)
        {
            if (descriptor?.WordSection?.PredefinedPages == null)
            {
                return markdownContent;
            }

            var result = new StringBuilder();
            var predefinedPages = descriptor.WordSection.PredefinedPages;

            // 1. Inserisci Cover Page
            if (predefinedPages.Cover?.Enabled == true)
            {
                var coverContent = await ProcessPageAsync(predefinedPages.Cover, descriptor, projectPath);
                if (!string.IsNullOrEmpty(coverContent))
                {
                    result.AppendLine(coverContent);
                }
            }

            // 2. Contenuto principale
            result.AppendLine(markdownContent);

            // 3. Inserisci Disclaimer
            if (predefinedPages.Disclaimer?.Enabled == true)
            {
                var disclaimerContent = await ProcessPageAsync(predefinedPages.Disclaimer, descriptor, projectPath);
                if (!string.IsNullOrEmpty(disclaimerContent))
                {
                    result.AppendLine(disclaimerContent);
                }
            }

            // 4. Inserisci Appendix
            if (predefinedPages.Appendix?.Enabled == true)
            {
                var appendixContent = await ProcessPageAsync(predefinedPages.Appendix, descriptor, projectPath);
                if (!string.IsNullOrEmpty(appendixContent))
                {
                    result.AppendLine(appendixContent);
                }
            }

            return result.ToString();
        }

        private async Task<string> ProcessPageAsync(
            PageDescriptor page, 
            MdExplorerDocumentDescriptor descriptor,
            string projectPath)
        {
            try
            {
                // Usa la struttura esistente .md/templates/word/pages/
                var templatePath = Path.Combine(projectPath, ".md", "templates", "word", "pages", 
                                               page.Type, $"{page.Template}.md");
                
                if (!File.Exists(templatePath))
                {
                    _logger.LogWarning($"Template non trovato: {templatePath}");
                    return string.Empty;
                }

                var templateContent = await File.ReadAllTextAsync(templatePath);
                var allTags = BuildTagsDictionary(page, descriptor, projectPath);
                
                return ReplaceTags(templateContent, allTags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nel processare la pagina {page.Template}");
                return string.Empty;
            }
        }

        private Dictionary<string, string> BuildTagsDictionary(
            PageDescriptor page,
            MdExplorerDocumentDescriptor descriptor,
            string projectPath)
        {
            var tags = new Dictionary<string, string>();

            // Tag globali dal documento
            if (!string.IsNullOrEmpty(descriptor.Title))
                tags["title"] = descriptor.Title;
            if (!string.IsNullOrEmpty(descriptor.Author))
                tags["author"] = descriptor.Author;
            if (!string.IsNullOrEmpty(descriptor.Email))
                tags["email"] = descriptor.Email;
            if (!string.IsNullOrEmpty(descriptor.Date))
                tags["date"] = descriptor.Date;

            // Tag di sistema
            tags["current_date"] = DateTime.Now.ToString("dd/MM/yyyy");
            tags["current_time"] = DateTime.Now.ToString("HH:mm:ss");
            tags["project_path"] = projectPath;

            // Tag specifici della pagina (sovrascrivono quelli globali)
            if (page.Tags != null)
            {
                foreach (var tag in page.Tags)
                {
                    tags[tag.Key] = tag.Value;
                }
            }

            return tags;
        }

        public string ReplaceTags(string template, Dictionary<string, string> tags)
        {
            if (string.IsNullOrEmpty(template))
                return template;

            return _tagRegex.Replace(template, match =>
            {
                var tagName = match.Groups[1].Value.Trim();

                // Supporta tag con valore di default: {{tag|default}}
                var parts = tagName.Split('|');
                var actualTagName = parts[0].Trim();
                var defaultValue = parts.Length > 1 ? parts[1].Trim() : string.Empty;

                if (tags.ContainsKey(actualTagName))
                {
                    return tags[actualTagName];
                }
                else if (!string.IsNullOrEmpty(defaultValue))
                {
                    return defaultValue;
                }
                else
                {
                    return match.Value; // Mantieni il tag originale se non trovato
                }
            });
        }

        /// <summary>
        /// Prepara il markdown per Pandoc, risolvendo problemi di compatibilità.
        /// In particolare, sostituisce i separatori orizzontali "---" con "***"
        /// per evitare che Pandoc li interpreti come inizio di un blocco YAML.
        /// </summary>
        public string PrepareMarkdownForPandoc(string markdownContent)
        {
            if (string.IsNullOrEmpty(markdownContent))
                return markdownContent;

            // Regex per trovare "---" su una linea da sola (separatore orizzontale)
            // che NON sia il front matter YAML (all'inizio del documento)
            // Il front matter è: ---\n...contenuto...\n---
            // I separatori successivi sono solo "---" su una linea

            var lines = markdownContent.Split('\n');
            var result = new StringBuilder();
            var inYamlFrontMatter = false;
            var frontMatterClosed = false;

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var trimmedLine = line.TrimEnd('\r');

                // Gestione YAML front matter
                if (i == 0 && trimmedLine == "---")
                {
                    // Inizio del front matter
                    inYamlFrontMatter = true;
                    result.AppendLine(line.TrimEnd('\r'));
                    continue;
                }

                if (inYamlFrontMatter && !frontMatterClosed && trimmedLine == "---")
                {
                    // Fine del front matter
                    frontMatterClosed = true;
                    inYamlFrontMatter = false;
                    result.AppendLine(line.TrimEnd('\r'));
                    continue;
                }

                // Se siamo fuori dal front matter e troviamo "---" da solo, sostituiscilo
                if (frontMatterClosed && trimmedLine == "---")
                {
                    result.AppendLine("***");
                    _logger.LogDebug("[WordTemplateService] Replaced horizontal rule '---' with '***' at line {0}", i + 1);
                }
                else
                {
                    result.AppendLine(line.TrimEnd('\r'));
                }
            }

            // Rimuovi l'ultimo newline extra aggiunto da AppendLine
            var resultStr = result.ToString();
            if (resultStr.EndsWith("\n"))
                resultStr = resultStr.Substring(0, resultStr.Length - 1);

            return resultStr;
        }
    }
}