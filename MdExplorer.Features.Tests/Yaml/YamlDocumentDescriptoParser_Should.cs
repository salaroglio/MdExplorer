using MdExplorer.Features.Yaml.Interfaces;
using MdExplorer.Features.Yaml.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MdExplorer.Features.Tests.Yaml
{
    [TestClass]
    public class YamlDocumentDescriptoParser_Should
    {
        IServiceCollection serviceCollection;

        [TestInitialize]
        public void InitTest()
        {
            serviceCollection = new ServiceCollection();
            serviceCollection.AddMDExplorerCommands();
        }

        [TestMethod]
        public void ParseYamlFromMarkdown()
        {
            var provider = serviceCollection.BuildServiceProvider();
            var parser = provider.GetService<IYamlParser<MdExplorerDocumentDescriptor>>();
            var text = File.ReadAllText("Yaml/Inputs/ParseYaml.md");
            var documentSettings = parser.GetDescriptor(text);
        }



        [TestMethod]
        public void ParseFromNormalYaml()
        {
            var provider = serviceCollection.BuildServiceProvider();
            var parser = provider.GetService<IYamlParser<MdExplorerDocumentDescriptor>>();

            // read from markdown
            var text = File.ReadAllText("Yaml/Inputs/NormalYaml.md");
            // get document descriptor
            var docDesc = parser.GetDescriptor(text);
            // set new one
            docDesc.WordSection.WriteToc = true;
            docDesc.WordSection.DocumentHeader = WordSection.DocumentHeaderEnum.None;
            var markdown = parser.SetDescriptor(docDesc, text);
        }

        [TestMethod]
        public void DeserializeYaml()
        {
            var provider = serviceCollection.BuildServiceProvider();
            var parser = provider.GetService<IYamlParser<MdExplorerDocumentDescriptor>>();
            var yamlDoc = new MdExplorerDocumentDescriptor
            {
                Author = "Author",
                Date = "",
                Email = "@Carlo",
                Title = "Title",
                DocumentType = MdExplorerDocumentDescriptor.DocumentTypeEnum.Document,
                WordSection = new WordSection
                {
                    WriteToc = false,
                    DocumentHeader = WordSection.DocumentHeaderEnum.None,
                }
                
            };
            var response = parser.SerializeDescriptor(yamlDoc);
            File.WriteAllText("Yaml/Inputs/ParseYaml.md", response);
        }
    }
}
