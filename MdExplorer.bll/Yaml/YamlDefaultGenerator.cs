using MdExplorer.Features.GIT;
using MdExplorer.Features.Yaml.Interfaces;
using MdExplorer.Features.Yaml.Models;
using System;
using System.Globalization;

namespace MdExplorer.Features.Yaml
{
    public class YamlDefaultGenerator : IYamlDefaultGenerator
    {
        private readonly IYamlParser<MdExplorerDocumentDescriptor> _yamlParser;
        private readonly IGitService _gitService;

        public YamlDefaultGenerator(IYamlParser<MdExplorerDocumentDescriptor> yamlParser, 
                                   IGitService gitService)
        {
            _yamlParser = yamlParser;
            _gitService = gitService;
        }

        public string GenerateDefaultYaml(string projectPath = null)
        {
            var descriptor = GenerateDefaultDescriptor(projectPath);
            var serializedYaml = _yamlParser.SerializeDescriptor(descriptor);
            return $"---\r\n{serializedYaml}---\r\n";
        }

        public MdExplorerDocumentDescriptor GenerateDefaultDescriptor(string projectPath = null)
        {
            var doc = new MdExplorerDocumentDescriptor
            {
                Author = "<not available>",
                Email = "<not available>",
                DocumentType = "Document",
                Date = DateTime.Now.ToString("d", CultureInfo.GetCultureInfo("it-IT")),
                WordSection = new WordSection
                {
                    WriteToc = false,
                    TemplateSection = new TemplateSection
                    {
                        TemplateType = "default",
                        InheritFromTemplate = string.Empty,
                        CustomTemplate = string.Empty
                    },
                    DocumentHeader = string.Empty
                }
            };

            // Tenta di recuperare le informazioni Git se il path del progetto è disponibile
            if (!string.IsNullOrEmpty(projectPath))
            {
                try
                {
                    var author = _gitService.GetCurrentUser(projectPath);
                    var email = _gitService.GetCurrentUserEmail(projectPath);
                    
                    if (!string.IsNullOrEmpty(author) && author != "<not available>")
                        doc.Author = author;
                    
                    if (!string.IsNullOrEmpty(email) && email != "<not available>")
                        doc.Email = email;
                }
                catch
                {
                    // Se c'è un errore nel recupero delle info Git, mantieni i valori di default
                }
            }

            return doc;
        }
    }
}