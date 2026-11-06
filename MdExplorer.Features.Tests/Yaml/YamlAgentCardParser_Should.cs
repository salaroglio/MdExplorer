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

        // ---- nomi riservati user / shared / external ----

        [DataTestMethod]
        [DataRow("user")]
        [DataRow("shared")]
        [DataRow("USER")]
        [DataRow("Shared")]
        [DataRow("external")]
        [DataRow("External")]
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

        // ---- interfacce dei lavori (workflow v3, D8) ----

        private const string Interfaces = @"---
a2a:
  name: account-manager
  skills:
    - id: cerca-bandi
      description: Cerca i bandi
      reads:
        - { path: gara/bandi.md, note: i bandi del committente }
        - { path: gara/profilo.md, section: Criteri di valutazione }
      outputs:
        - { id: ricerca, path: gara/ricerche/ricerca-*.md, note: con una domanda per ciascuno }
        - { id: capitolato, path: gara/documenti/*-capitolato.md, action: downloads }
      replies: [avvia-giro]
    - id: sintesi-gara
      inputs:
        - { id: codice, description: il codice del bando }
        - { id: schede, description: le schede approvate }
      outputs:
        - { id: registro, path: gara/registro.md, action: updates }
  replies:
    - id: avvia-giro
      label: Avvia il giro su {codice}
      values: [codice, capitolato, ricerca]
---
corpo";

        [TestMethod]
        public void Read_the_interface_of_each_skill()
        {
            var result = _parser.GetDescriptor(Interfaces);

            Assert.IsTrue(result.IsValid, result.RegistrationError);
            var search = result.Card.Skills[0];
            Assert.AreEqual("Criteri di valutazione", search.Reads[1].Section);
            Assert.AreEqual("downloads", search.Outputs[1].Action);
            Assert.IsNull(search.Outputs[0].Action, "senza action vale writes");
            CollectionAssert.AreEqual(new[] { "avvia-giro" }, search.Replies.ToArray());
            var synthesis = result.Card.Skills[1];
            CollectionAssert.AreEqual(new[] { "codice", "schede" }, synthesis.Inputs.Select(i => i.Id).ToArray());
            CollectionAssert.AreEqual(new[] { "codice", "capitolato", "ricerca" }, result.Card.Replies[0].Values.ToArray());
        }

        [DataTestMethod]
        [DataRow("    - id: cerca-bandi\n", "    - id: cerca-bandi\n      ouputs: []\n", "a2a.skills[0].ouputs")]
        [DataRow("{ id: registro, path: gara/registro.md, action: updates }", "{ id: registro, path: gara/registro.md, verb: updates }", "a2a.skills[1].outputs[0].verb")]
        [DataRow("  name: account-manager\n", "  name: account-manager\n  owner: anna\n", "a2a.owner")]
        public void Refuse_an_unknown_key_in_the_a2a_block_with_its_path(string from, string to, string path)
        {
            var result = _parser.GetDescriptor(Interfaces.Replace("\r\n", "\n").Replace(from, to));

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains(result.RegistrationError, path);
        }

        [DataTestMethod]
        [DataRow("{ id: registro, path: gara/registro.md, action: updates }", "{ id: registro, action: updates }", "registro")]
        [DataRow("action: downloads", "action: deletes", "deletes")]
        [DataRow("replies: [avvia-giro]", "replies: [avvia-altro]", "avvia-altro")]
        [DataRow("{ id: schede, description: le schede approvate }", "{ id: codice, description: di nuovo }", "codice")]
        [DataRow("values: [codice, capitolato, ricerca]", "values: [codice, codice]", "codice")]
        public void Refuse_an_inconsistent_interface(string from, string to, string named)
        {
            var result = _parser.GetDescriptor(Interfaces.Replace("\r\n", "\n").Replace(from, to));

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains(result.RegistrationError, named);
        }

        [TestMethod]
        public void Keep_the_trust_fingerprint_of_a_card_without_interfaces()
        {
            // Una scheda di prima non dichiara interfacce: la sua impronta non deve cambiare, o tutte le schede già fidate
            // chiederebbero di nuovo la fiducia senza che nessuno le abbia toccate.
            var card = _parser.GetDescriptor(@"---
a2a:
  name: worker
  skills:
    - id: lavora
      description: Lavora
  replies:
    - { id: ok, label: Va bene, description: chiude, message: ok }
---").Card;
            var yaml = new YamlDotNet.Serialization.SerializerBuilder()
                .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.UnderscoredNamingConvention.Instance).Build().Serialize(card);

            foreach (var key in new[] { "inputs", "reads", "outputs", "values" })
                Assert.IsFalse(yaml.Contains(key + ":"), $"'{key}' non deve comparire nell'impronta di una scheda che non lo dichiara:\n{yaml}");
        }
    }
}
