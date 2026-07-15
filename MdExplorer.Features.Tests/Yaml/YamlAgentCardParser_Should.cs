using MdExplorer.Features.Yaml;
using MdExplorer.Features.Yaml.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Tests.Yaml
{
    [TestClass]
    public class YamlAgentCardParser_Should
    {
        private IYamlAgentCardParser _parser;

        [TestInitialize]
        public void InitTest()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddMDExplorerCommands();
            _parser = serviceCollection.BuildServiceProvider().GetService<IYamlAgentCardParser>();
        }

        // ---- frontmatter valido ----

        [TestMethod]
        public void Parse_a_valid_a2a_block_into_a_citizen_card()
        {
            var md = @"---
description: Cura il glossario degli stem COBOL del progetto
tools: [read, write, edit, search]
a2a:
  name: stem-curator
  role: Curatore del glossario stem
  skills:
    - id: curate-stems
      description: Aggiorna il glossario quando arrivano nuovi termini
    - id: answer-stem-questions
      description: Risponde a domande sul significato di uno stem
  accepts_messages_from: [cobol-pipeline, user]
  max_hops: 12
mde:
  origin: user
  version: 1
---

# Corpo del documento
";
            var result = _parser.GetDescriptor(md);

            Assert.IsTrue(result.IsValid);
            Assert.IsTrue(result.HasA2aBlock);
            Assert.IsTrue(result.IsCitizen);
            Assert.IsNull(result.RegistrationError);
            Assert.IsNotNull(result.Card);
            Assert.AreEqual("stem-curator", result.Card.Name);
            Assert.AreEqual("Curatore del glossario stem", result.Card.Role);
            Assert.AreEqual(2, result.Card.Skills.Count);
            Assert.AreEqual("curate-stems", result.Card.Skills[0].Id);
            CollectionAssert.AreEqual(new[] { "cobol-pipeline", "user" },
                result.Card.AcceptsMessagesFrom.ToArray());
            Assert.AreEqual(12, result.Card.MaxHops);
        }

        [TestMethod]
        public void Accept_a_minimal_card_with_only_the_name()
        {
            var md = @"---
a2a:
  name: reindexer
---
corpo";
            var result = _parser.GetDescriptor(md);

            Assert.IsTrue(result.IsCitizen);
            Assert.AreEqual("reindexer", result.Card.Name);
            Assert.IsNull(result.Card.MaxHops);
            Assert.AreEqual(0, result.Card.Skills.Count);
            Assert.AreEqual(0, result.Card.AcceptsMessagesFrom.Count);
        }

        // ---- frontmatter assente (retrocompatibilità) ----

        [TestMethod]
        public void Treat_frontmatter_without_a2a_as_a_non_citizen_without_error()
        {
            var md = @"---
description: Un agente vecchio stile, senza blocco a2a
tools: [read]
---
corpo";
            var result = _parser.GetDescriptor(md);

            Assert.IsTrue(result.IsValid, "senza a2a resta lanciabile come oggi");
            Assert.IsFalse(result.HasA2aBlock);
            Assert.IsFalse(result.IsCitizen);
            Assert.IsNull(result.Card);
            Assert.IsNull(result.RegistrationError);
        }

        [TestMethod]
        public void Treat_a_file_without_any_frontmatter_as_a_non_citizen()
        {
            var result = _parser.GetDescriptor("# Solo testo, niente frontmatter");

            Assert.IsTrue(result.IsValid);
            Assert.IsFalse(result.HasA2aBlock);
            Assert.IsFalse(result.IsCitizen);
        }

        // ---- frontmatter malformato (fail-loud) ----

        [TestMethod]
        public void Fail_loud_when_the_a2a_block_is_malformed_yaml()
        {
            // Indentazione incoerente dentro a2a: → YamlDotNet solleva.
            var md = @"---
a2a:
  name: broken
   role: indentazione sbagliata
  skills: [not: a: list
---
corpo";
            var result = _parser.GetDescriptor(md);

            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.HasA2aBlock);
            Assert.IsFalse(result.IsCitizen);
            Assert.IsNotNull(result.RegistrationError);
            StringAssert.Contains(result.RegistrationError, "a2a");
        }

        [TestMethod]
        public void Fail_loud_when_the_a2a_block_is_present_but_empty()
        {
            var md = @"---
description: dichiara a2a ma senza contenuto
a2a:
---
corpo";
            var result = _parser.GetDescriptor(md);

            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.HasA2aBlock);
            Assert.IsNotNull(result.RegistrationError);
        }

        [TestMethod]
        public void Fail_loud_when_the_a2a_block_has_no_name()
        {
            var md = @"---
a2a:
  role: Un ruolo senza nome
---
corpo";
            var result = _parser.GetDescriptor(md);

            Assert.IsFalse(result.IsValid);
            Assert.IsNotNull(result.RegistrationError);
            StringAssert.Contains(result.RegistrationError, "name");
        }

        // ---- nomi riservati user / shared ----

        [DataTestMethod]
        [DataRow("user")]
        [DataRow("shared")]
        [DataRow("USER")]
        [DataRow("Shared")]
        public void Reject_reserved_names(string reserved)
        {
            var md = $@"---
a2a:
  name: {reserved}
  role: Prova nome riservato
---
corpo";
            var result = _parser.GetDescriptor(md);

            Assert.IsFalse(result.IsValid);
            Assert.IsNotNull(result.RegistrationError);
            StringAssert.Contains(result.RegistrationError, "riservat");
        }

        // ---- validazione del nome ----

        [TestMethod]
        public void Reject_a_name_containing_the_federation_at_sign()
        {
            var md = @"---
a2a:
  name: agente@collega.it
---
corpo";
            var result = _parser.GetDescriptor(md);

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains(result.RegistrationError, "@");
        }

        [DataTestMethod]
        [DataRow("StemCurator")]     // maiuscole
        [DataRow("stem_curator")]    // underscore
        [DataRow("stem curator")]    // spazio
        [DataRow("-stem")]           // trattino iniziale
        [DataRow("stem--curator")]   // doppio trattino
        public void Reject_names_that_are_not_kebab_case(string badName)
        {
            var md = $@"---
a2a:
  name: ""{badName}""
---
corpo";
            var result = _parser.GetDescriptor(md);

            Assert.IsFalse(result.IsValid, $"'{badName}' non è kebab-case");
            Assert.IsNotNull(result.RegistrationError);
        }

        // ---- unicità cross-file: duplicati → entrambi esclusi ----

        [TestMethod]
        public void Find_duplicate_names_case_insensitively()
        {
            var names = new[] { "cobol-pipeline", "stem-curator", "Cobol-Pipeline", "reindexer" };

            var duplicates = YamlAgentCardParser.FindDuplicateNames(names);

            Assert.AreEqual(1, duplicates.Count);
            Assert.IsTrue(duplicates.Contains("cobol-pipeline"));
        }

        [TestMethod]
        public void Report_no_duplicates_when_all_names_are_unique()
        {
            var names = new[] { "a", "b", "c" };

            Assert.AreEqual(0, YamlAgentCardParser.FindDuplicateNames(names).Count);
        }

        [TestMethod]
        public void Ignore_null_and_blank_names_when_detecting_duplicates()
        {
            var names = new List<string> { "a", null, "  ", "a" };

            var duplicates = YamlAgentCardParser.FindDuplicateNames(names);

            Assert.AreEqual(1, duplicates.Count);
            Assert.IsTrue(duplicates.Contains("a"));
        }
    }
}
