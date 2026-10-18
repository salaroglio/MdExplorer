using MdExplorer.Features.Templates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace MdExplorer.Features.Tests.Templates
{
    [TestClass]
    public class TemplateManifest_Should
    {
        private const string GaraIso = @"mde_template: 1
id: gara-iso
kind: method
title: Gara con ISO 9001
description: Dalla gara alla decisione.
version: 1.2.0
requires:
  mde: 2026.10.17
  templates: []
languages:
  it: { structure: main, example: esempio }
  en: { structure: en }
roles:
  - { role: Account manager, description: decide se partecipare, agents: [gara-iso-account-manager] }
  - { role: Direzione }
workflows:
  - .mde-template/gara-iso/workflows/gara.md
instructions: .mde-template/gara-iso/istruzioni.md
";

        [TestMethod]
        public void Read_the_manifest_of_a_method()
        {
            var m = TemplateManifestReader.Read(GaraIso, "gara-iso", out var problems);
            Assert.AreEqual(0, problems.Count, string.Join("; ", problems));
            Assert.IsTrue(m.IsMethod);
            Assert.AreEqual("1.2.0", m.Version);
            Assert.AreEqual("2026.10.17", m.Requires.Mde);
            Assert.AreEqual("Account manager", m.Roles[0].Role);
            Assert.AreEqual("esempio", m.BranchFor("it", example: true));
            Assert.AreEqual("en", m.BranchFor("en", example: false));
            Assert.AreEqual(".mde-template/gara-iso/mde-template.yml", TemplateManifest.PathOf("gara-iso"));
        }

        [TestMethod]
        public void Refuse_a_language_or_an_example_the_template_does_not_have()
        {
            var m = TemplateManifestReader.Read(GaraIso, "gara-iso", out _);
            StringAssert.Contains(Assert.ThrowsException<InvalidOperationException>(() => m.BranchFor("de", false)).Message, "'de'");
            StringAssert.Contains(Assert.ThrowsException<InvalidOperationException>(() => m.BranchFor("en", true)).Message, "'en'");
        }

        [TestMethod]
        public void Refuse_a_key_it_does_not_know()
        {
            TemplateManifestReader.Read(GaraIso + "colour: blue\n", "gara-iso", out var problems);
            Assert.AreEqual(1, problems.Count);
            StringAssert.Contains(problems[0], "colour");
        }

        [TestMethod]
        public void Say_every_wrong_value()
        {
            var yaml = GaraIso
                .Replace("mde_template: 1", "mde_template: 2")
                .Replace("kind: method", "kind: metodo")
                .Replace("version: 1.2.0", "version: 1.2")
                .Replace("agents: [gara-iso-account-manager]", "agents: [account-manager]")
                .Replace("instructions: .mde-template/gara-iso/istruzioni.md", "instructions: CLAUDE.md");
            TemplateManifestReader.Read(yaml, "gara-iso", out var problems);
            var all = string.Join("\n", problems);
            StringAssert.Contains(all, "mde_template: 2");
            StringAssert.Contains(all, "kind 'metodo'");
            StringAssert.Contains(all, "version '1.2'");
            // Con un kind sconosciuto le regole del metodo non si applicano: l'agente e il percorso non sono contati.
            Assert.AreEqual(3, problems.Count, all);
        }

        [TestMethod]
        public void Keep_a_methods_agents_and_files_inside_its_own_name()
        {
            var yaml = GaraIso
                .Replace("agents: [gara-iso-account-manager]", "agents: [account-manager]")
                .Replace("instructions: .mde-template/gara-iso/istruzioni.md", "instructions: CLAUDE.md");
            TemplateManifestReader.Read(yaml, "gara-iso", out var problems);
            Assert.AreEqual(2, problems.Count, string.Join("\n", problems));
            StringAssert.Contains(problems[0], "'account-manager'");
            StringAssert.Contains(problems[1], "'CLAUDE.md'");
        }

        [TestMethod]
        public void Let_a_project_template_put_its_files_anywhere()
        {
            var yaml = @"mde_template: 1
id: demo
kind: project
title: La demo di MdExplorer
version: 1.0.0
languages:
  it: { structure: main }
  en: { structure: en }
workflows: [citta-degli-agenti/gara/workflow.md]
";
            var m = TemplateManifestReader.Read(yaml, "demo", out var problems);
            Assert.AreEqual(0, problems.Count, string.Join("; ", problems));
            Assert.IsFalse(m.IsMethod);
        }

        [TestMethod]
        public void Read_seeds_that_go_from_the_zone_to_the_groups_files()
        {
            var good = TemplateManifestReader.Read(GaraIso + "seeds:\n  - { from: .mde-template/gara-iso/semi/profilo.md, to: gare/profilo-azienda.md }\n",
                                                   "gara-iso", out var problems);
            Assert.AreEqual(0, problems.Count, string.Join("; ", problems));
            Assert.AreEqual("gare/profilo-azienda.md", good.Seeds.Single().To);

            TemplateManifestReader.Read(GaraIso + "seeds:\n" +
                "  - { from: semi/profilo.md, to: gare/a.md }\n" +
                "  - { from: .mde-template/gara-iso/semi/b.md, to: .mde-template/gara-iso/b.md }\n" +
                "  - { from: .mde-template/gara-iso/semi/c.md, to: .development.yml }\n" +
                "  - { from: .mde-template/gara-iso/semi/d.md, to: ../fuori.md }\n" +
                "  - { from: .mde-template/gara-iso/semi/e.md, to: gare/A.md }\n", "gara-iso", out problems);
            var all = string.Join("\n", problems);
            Assert.AreEqual(5, problems.Count, all);
            StringAssert.Contains(problems[0], "'semi/profilo.md' is outside the method's folder");
            StringAssert.Contains(problems[1], "'.mde-template/gara-iso/b.md' is not a file of the group");
            StringAssert.Contains(problems[2], "'.development.yml' is not a file of the group");
            StringAssert.Contains(problems[3], "'../fuori.md' must be a path from the project root");
            StringAssert.Contains(problems[4], "'gare/A.md' appears twice");
        }

        [TestMethod]
        public void Know_the_features_a_template_needs_and_refuse_the_others()
        {
            var ontology = GaraIso.Replace("  templates: []\n", "  templates: []\n  features: [fuseki]\n");
            var m = TemplateManifestReader.Read(ontology, "gara-iso", out var problems);
            Assert.AreEqual(0, problems.Count, string.Join("; ", problems));
            CollectionAssert.AreEqual(new[] { "fuseki" }, m.Requires.Features);

            TemplateManifestReader.Read(GaraIso.Replace("  templates: []\n", "  templates: []\n  features: [fuseki, neo4j, fuseki]\n"), "gara-iso", out problems);
            Assert.AreEqual(2, problems.Count, string.Join("\n", problems));
            StringAssert.Contains(problems[0], "'neo4j' is not a feature this MdExplorer knows (fuseki, code)");
            StringAssert.Contains(problems[1], "'fuseki' appears twice");
        }

        [TestMethod]
        public void Read_the_code_folders_of_a_template_and_refuse_wrong_ones()
        {
            var m = TemplateManifestReader.Read(GaraIso.Replace("  templates: []\n", "  templates: []\n  features: [code]\n") +
                                                "programs:\n  - { path: codice/assistente, languages: [csharp] }\n", "gara-iso", out var problems);
            Assert.AreEqual(0, problems.Count, string.Join("; ", problems));
            Assert.AreEqual("codice/assistente", m.Programs.Single().Path);

            TemplateManifestReader.Read(GaraIso + "programs:\n" +
                "  - { path: codice, languages: [cobolo] }\n" +
                "  - { path: .mde-template/gara-iso/codice, languages: [csharp] }\n" +
                "  - { path: altro }\n" +
                "  - { path: Codice, languages: [csharp] }\n", "gara-iso", out problems);
            var all = string.Join("\n", problems);
            Assert.AreEqual(4, problems.Count, all);
            StringAssert.Contains(all, "'cobolo' is not a language MdExplorer knows");
            StringAssert.Contains(all, "'.mde-template/gara-iso/codice' must be a folder of the group");
            StringAssert.Contains(all, "'altro' without languages");
            StringAssert.Contains(all, "'Codice' appears twice");
        }

        [TestMethod]
        public void Want_the_id_to_be_the_folder_it_lives_in()
        {
            TemplateManifestReader.Read(GaraIso, "gara", out var problems);
            Assert.AreEqual(1, problems.Count);
            StringAssert.Contains(problems[0], ".mde-template/gara/");
        }

        [TestMethod]
        public void Find_the_files_of_a_method_outside_its_zone()
        {
            var outside = TemplateManifestReader.OutsideZone("gara-iso", new[]
            {
                ".mde-template/gara-iso/mde-template.yml",
                ".claude/skills/gara-iso-riesame/SKILL.md",
                ".claude/skills/riesame/SKILL.md",
                "README.md",
                ".mde-template/ddd/mde-template.yml",
            });
            CollectionAssert.AreEqual(new[] { ".claude/skills/riesame/SKILL.md", "README.md", ".mde-template/ddd/mde-template.yml" }, outside);
        }

        [TestMethod]
        public void Compare_versions_of_MdExplorer()
        {
            Assert.IsTrue(TemplateManifestReader.CompareAppVersions("2026.10.16.8", "2026.10.17") < 0);
            Assert.IsTrue(TemplateManifestReader.CompareAppVersions("2026.10.17", "2026.10.17.0") == 0);
            Assert.IsTrue(TemplateManifestReader.CompareAppVersions("2026.11.1", "2026.10.30") > 0);
        }
    }
}
