using MdExplorer.Features.Yaml.Interfaces;
using MdExplorer.Features.Yaml.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Yaml
{
    /// <summary>
    /// The front matter belongs to the user, not to the descriptor: a key MdExplorer does not model
    /// (the slides' <c>reveal:</c> block) must neither hide the keys it does model nor be lost when
    /// the document settings are saved. Today an unknown key made <c>GetDescriptor</c> return null,
    /// and <c>GetDocumentSettings</c> then wrote a second front matter at the top of the file.
    /// </summary>
    [TestClass]
    public class YamlDocumentDescriptorUnknownKeys_Should
    {
        private const string SlideDeck = @"---
title: Revisione trimestrale
document_type: slides
reveal:
  theme: white
  config:
    controls: false
    slideNumber: c/t
    width: 1280
---

# Revisione
";

        private IYamlParser<MdExplorerDocumentDescriptor> _parser;

        [TestInitialize]
        public void InitTest()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddMDExplorerCommands();
            _parser = serviceCollection.BuildServiceProvider().GetService<IYamlParser<MdExplorerDocumentDescriptor>>();
        }

        [TestMethod]
        public void Read_the_known_keys_when_the_front_matter_has_unknown_ones()
        {
            var descriptor = _parser.GetDescriptor(SlideDeck);

            Assert.IsNotNull(descriptor);
            Assert.AreEqual("slides", descriptor.DocumentType);
            Assert.AreEqual("Revisione trimestrale", descriptor.Title);
        }

        [TestMethod]
        public void Keep_the_unknown_keys_as_written_when_the_descriptor_is_saved()
        {
            var descriptor = _parser.GetDescriptor(SlideDeck);
            descriptor.Title = "Revisione di fine anno";

            var saved = _parser.SetDescriptor(descriptor, SlideDeck);

            StringAssert.Contains(saved, "title: Revisione di fine anno");
            StringAssert.Contains(saved, @"reveal:
  theme: white
  config:
    controls: false
    slideNumber: c/t
    width: 1280");
            StringAssert.EndsWith(saved, "---\n\n# Revisione\n");
        }
    }
}
