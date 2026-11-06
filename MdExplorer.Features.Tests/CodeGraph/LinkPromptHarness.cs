using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// Prova a mano con un LLM vero (D43): <c>MDE_LINK_PROMPT_OUT</c> scrive la domanda sui collegamenti di un modulo Guice;
    /// la si manda al motore (<c>claude -p</c>…) e <c>MDE_LINK_ANSWER</c> rilegge la risposta con il lettore vero. Senza
    /// variabili: Inconclusive.
    /// </summary>
    [TestClass]
    public class LinkPromptHarness
    {
        private static readonly string[] Module =
        {
            "package com.acme;", "", "import com.google.inject.AbstractModule;", "import com.google.inject.Singleton;", "import com.google.inject.name.Names;", "",
            "public class AppModule extends AbstractModule {", "    protected void configure() {",
            "        bind(Exporter.class).annotatedWith(Names.named(\"main\")).to(PdfExporter.class).in(Singleton.class);",
            "        bind(Store.class).to(FileStore.class);", "    }", "}",
        };

        private static List<CodeLinkPoint> Points() => new()
        {
            new() { RuleId = "guice-bind", Path = "program/AppModule.java", Line = 9, Excerpt = CodeLinkDocs.Excerpt(Module, 9),
                Links = new[] { "com.acme.Exporter («Esporta un documento.») → com.acme.PdfExporter («Esporta in PDF.»)" } },
            new() { RuleId = "guice-bind", Path = "program/AppModule.java", Line = 10, Excerpt = CodeLinkDocs.Excerpt(Module, 10),
                Links = new[] { "com.acme.Store → com.acme.FileStore" } },
        };

        private static readonly CodeEnrichmentRule Rule = new()
        {
            Id = "guice-bind", Kind = CodeRuleKind.Registration, Description = "bind(X).to(Y): Guice usa Y dove serve X.",
            Describe = "Con quale qualificatore (@Named) e con quale durata (scope) viene fornita l'implementazione?",
        };

        [TestMethod]
        public void Write_a_link_prompt()
        {
            var output = Environment.GetEnvironmentVariable("MDE_LINK_PROMPT_OUT");
            if (string.IsNullOrEmpty(output)) Assert.Inconclusive("MDE_LINK_PROMPT_OUT non impostata");
            File.WriteAllText(output, CodeLinkDocs.BuildPrompt(Rule, Points(), "it"));
        }

        [TestMethod]
        public void Read_a_link_answer()
        {
            var input = Environment.GetEnvironmentVariable("MDE_LINK_ANSWER");
            if (string.IsNullOrEmpty(input)) Assert.Inconclusive("MDE_LINK_ANSWER non impostata");
            var read = CodeLinkDocs.ParseAnswer(File.ReadAllText(input), Points(), out var rejected);
            foreach (var (key, summary) in read) Console.WriteLine($"{key}: {summary}");
            foreach (var r in rejected) Console.WriteLine("SCARTATA: " + r);
            Assert.AreEqual(2, read.Count, "un punto per punto chiesto");
            Assert.AreEqual(0, rejected.Count);
        }
    }
}
