using LibGit2Sharp;
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

        public TextDocument(IYamlParser<YamlDocumentDescriptor> yamlParser)
        {
            _yamlParser = yamlParser;
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
            using (var repo = new Repository(projectPath))
            {
                // Object lookup
                var obj = repo.Lookup("sha");
                Configuration config = repo.Config;
                doc.Author = config.Where(_=>_.Key == "user.name").First().Value.ToString();
                doc.Email = config.Where(_ => _.Key == "user.email").First().Value.ToString();
            }
            
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
