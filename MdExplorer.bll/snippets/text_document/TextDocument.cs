
using LibGit2Sharp;
using MdExplorer.Features.GIT;
using MdExplorer.Features.Reveal.Interfaces;
using MdExplorer.Features.Reveal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.snippets.text_document
{
    public class TextDocument : ISnippet<DictionarySnippetParam>
    {
       
        private readonly IYamlParser<YamlDocumentDescriptor> _yamlParser;
        private readonly IGitService _gitService;

        public TextDocument(IYamlParser<YamlDocumentDescriptor> yamlParser, 
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
            var doc = new YamlDocumentDescriptor
            {
                Author = "<not available>",
                Email = "<not available>",
                DocumentType = "Document"
            };
            var projectPath = (string) parameters.Where(_ => _.Key == ParameterName.ProjectPath).First().Value;
            doc.Author = _gitService.GetCurrentUser(projectPath);
            doc.Email = _gitService.GetCurrentUserEmail(projectPath);

            var title =(string) parameters[ParameterName.StringDocumentTitle];
            var toReturn = string.Concat( "---\r\n",  _yamlParser.SerializeDescriptor(doc),"---\r\n","# " ,title, "\r\n");
            return toReturn;
        }

        public void SetAssets(string assetsPath)
        {
            throw new NotImplementedException();
        }
    }
}
