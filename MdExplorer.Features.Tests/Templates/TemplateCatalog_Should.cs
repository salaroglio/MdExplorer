using System;
using System.IO;
using System.Linq;
using MdExplorer.Features.Templates;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Templates
{
    /// <summary>L'elenco dei template che viaggia con MdExplorer (P4, TD4), letto con tolleranza zero.</summary>
    [TestClass]
    public class TemplateCatalog_Should
    {
        private const string One = @"{ ""mde_templates"": 1, ""templates"": [ { ""id"": ""demo"", ""kind"": ""project"", ""url"": ""https://x/demo.git"",
            ""languages"": [""it"", ""en""], ""title"": { ""it"": ""La demo"", ""en"": ""The demo"" }, ""description"": { ""it"": ""d"", ""en"": ""d"" } } ] }";

        [TestMethod]
        public void Read_the_list_that_ships_with_MdExplorer()
        {
            // Il file vero, quello che la build mette nel servizio.
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "MdExplorer", "project-templates.json"))) dir = dir.Parent;
            Assert.IsNotNull(dir, "project-templates.json non trovato risalendo dalla cartella dei test");
            var entries = TemplateCatalog.Read(File.ReadAllText(Path.Combine(dir.FullName, "MdExplorer", "project-templates.json")));
            Assert.IsTrue(entries.Any(e => e.Id == "demo" && e.Kind == "project"), "la demo è un template dell'elenco (TD1)");
            // I metodi componibili (M5-M6, 20/10): lo strato comune e due metodi, in italiano e in inglese; il DDD non c'è più.
            foreach (var (id, kind) in new[] { ("documenti", "foundation"), ("ontologie", "method"), ("iso9001", "method") })
            {
                var m = entries.Single(e => e.Id == id);
                Assert.AreEqual(kind, m.Kind, id);
                Assert.AreEqual($"https://github.com/salaroglio/mdexplorer-template-{id}.git", m.Url);
                CollectionAssert.AreEqual(new[] { "it", "en" }, m.Languages, id);
            }
            Assert.IsFalse(entries.Any(e => e.Id is "ddd" or "gara-iso"), "il DDD è tolto; gara-iso è diventato iso9001");
        }

        [TestMethod]
        public void Refuse_a_list_that_is_not_right()
        {
            Assert.AreEqual("The demo", TemplateCatalog.Read(One).Single().Title["en"]);

            string Problem(string json) => Assert.ThrowsException<InvalidOperationException>(() => TemplateCatalog.Read(json)).Message;
            StringAssert.Contains(Problem(One.Replace("\"url\"", "\"uri\"")), "cannot be read");
            StringAssert.Contains(Problem(One.Replace("\"mde_templates\": 1", "\"mde_templates\": 2")), "format 2");
            StringAssert.Contains(Problem(One.Replace("\"kind\": \"project\"", "\"kind\": \"base\"")), "templates[0].kind 'base'");
            StringAssert.Contains(Problem(One.Replace("\"en\": \"The demo\"", "\"en\": \"\"")), "templates[0].title: one text per language");
            StringAssert.Contains(Problem(One.Replace("]}", "").Replace("} ] }", "}, " + One.Substring(One.IndexOf("{ \"id\"")).Replace(" ] }", " ] }"))), "appears more than once");
        }

        [TestMethod]
        public void Accept_one_common_layer_only()
        {
            var entry = One.Substring(One.IndexOf("{ \"id\""), One.LastIndexOf(" ] }") - One.IndexOf("{ \"id\""));
            string Foundation(string id) => entry.Replace("\"id\": \"demo\"", $"\"id\": \"{id}\"").Replace("\"kind\": \"project\"", "\"kind\": \"foundation\"");
            string List(params string[] items) => "{ \"mde_templates\": 1, \"templates\": [ " + string.Join(", ", items) + " ] }";

            Assert.AreEqual("foundation", TemplateCatalog.Read(List(Foundation("documenti"), entry)).First().Kind);
            StringAssert.Contains(Assert.ThrowsException<InvalidOperationException>(() => TemplateCatalog.Read(List(Foundation("a"), Foundation("b")))).Message,
                "more than one foundation template");
        }
    }
}
