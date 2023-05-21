
using LibGit2Sharp;
using MdExplorer.Features.GIT;
using MdExplorer.Features.Yaml.Interfaces;
using MdExplorer.Features.Yaml.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.snippets.text_document
{
    public class TextDocument : ISnippet<DictionarySnippetParam>
    {
       
        private readonly IYamlParser<MdExplorerDocumentDescriptor> _yamlParser;
        private readonly IGitService _gitService;

        public TextDocument(IYamlParser<MdExplorerDocumentDescriptor> yamlParser, 
                            IGitService gitService)
        {
            _yamlParser = yamlParser;
            _gitService = gitService;
        }
        public int Id => 0;

        public string Name => "Text Document";

        public string Group => "Plantuml";

        public string GetSnippet(DictionarySnippetParam parameters)
        {
            var doc = new MdExplorerDocumentDescriptor
            {
                Author = "<not available>",
                Email = "<not available>",
                DocumentType = "Document",
                Date= DateTime.Now.ToString("d", CultureInfo.GetCultureInfo("it-IT")),
            };
            var docType = (string)parameters[ParameterName.DocumentType];
            
            var projectPath = (string) parameters.Where(_ => _.Key == ParameterName.ProjectPath).First().Value;
            doc.Author = _gitService.GetCurrentUser(projectPath);
            doc.Email = _gitService.GetCurrentUserEmail(projectPath);
            doc.DocumentType = docType;
            var title = string.Empty;
            if (docType == "document")
            {
                title = (string)parameters[ParameterName.StringDocumentTitle];
            }
            
            var toReturn = string.Concat( "---\r\n",  _yamlParser.SerializeDescriptor(doc),"---\r\n","# " ,title, "\r\n");
            return toReturn;
        }

        public void SetAssets(string assetsPath)
        {
            throw new NotImplementedException();
        }
    }
}
