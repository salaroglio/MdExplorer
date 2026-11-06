using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static MdExplorer.IntegrationTests.ProjectTemplateCreator_Should;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// La vetrina di «Crea nuovo progetto» (docs-internal/Sprints/2026-10-19-Vetrina-Dei-Template.md): un template letto dal
    /// suo repository prima di crearci un progetto, con le sue slide nella lingua scelta, le inglesi di riserva o la slide di
    /// circostanza. <para>Richiede <c>git</c> nel PATH.</para>
    /// </summary>
    [TestClass]
    public class TemplateShowcase_Should
    {
        [TestInitialize]
        public void ResetCwd() => Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        private static string Manifest(string slides = null, string languages = "  it: { structure: main }\n") => $@"mde_template: 1
id: ddd
kind: method
title: Sviluppo con DDD
description: Il DDD strategico.
version: 1.0.0
languages:
{languages}{(slides == null ? "" : "slides:\n" + slides)}";

        private const string BothSlides = "  it: .mde-template/ddd/presentazione.md\n  en: .mde-template/ddd/presentation.md\n";

        private static Dictionary<string, string> Method(string slides = BothSlides) => new()
        {
            [".mde-template/ddd/mde-template.yml"] = Manifest(slides),
            [".mde-template/ddd/presentazione.md"] = "---\ndocument_type: slides\n---\n# Il dominio prima del codice\n\n![schema](img/schema.svg)\n",
            [".mde-template/ddd/presentation.md"] = "---\ndocument_type: slides\n---\n# The domain before the code\n",
            [".mde-template/ddd/img/schema.svg"] = "<svg xmlns=\"http://www.w3.org/2000/svg\"/>",
            ["fuori.md"] = "fuori dalla zona: non si copia\n",
        };

        private static async Task<(HttpStatusCode Status, JsonElement Body)> Describe(AgentCityContext ctx, string url, string language)
        {
            var resp = await ctx.Client.GetAsync($"/api/templateshowcase/describe?url={Uri.EscapeDataString(url)}&language={language}");
            return (resp.StatusCode, JsonDocument.Parse(await resp.Content.ReadAsStringAsync()).RootElement);
        }

        [TestMethod]
        public async Task Show_the_slides_of_the_chosen_language_with_their_images()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var url = Template(ctx, "ddd", new() { ["main"] = Method() });

            var (status, entry) = await Describe(ctx, url, "it");

            Assert.AreEqual(HttpStatusCode.OK, status, entry.ToString());
            Assert.AreEqual("ddd", entry.GetProperty("id").GetString());
            Assert.IsTrue(entry.GetProperty("speaks").GetBoolean());
            Assert.AreEqual("it", entry.GetProperty("slidesLanguage").GetString());
            Assert.IsFalse(entry.GetProperty("theory").GetBoolean(), "il manifesto non ha theory");
            var page = entry.GetProperty("page").GetString();
            StringAssert.EndsWith(page, "/.mde-template/ddd/presentazione.md");

            var html = await ctx.Client.GetStringAsync(page);
            StringAssert.Contains(html, "Il dominio prima del codice");
            StringAssert.Contains(html, "src=\"img/schema.svg\"", "l'immagine resta relativa: si risolve contro la pagina");
            var image = await ctx.Client.GetAsync(page[..page.LastIndexOf('/')] + "/img/schema.svg");
            Assert.AreEqual(HttpStatusCode.OK, image.StatusCode);
            Assert.AreEqual("image/svg+xml", image.Content.Headers.ContentType?.MediaType);

            Assert.AreEqual(HttpStatusCode.NotFound, (await ctx.Client.GetAsync(page[..page.IndexOf("/.mde-template", StringComparison.Ordinal)] + "/fuori.md")).StatusCode,
                "fuori da .mde-template/ non si serve niente");
        }

        [TestMethod]
        public async Task Show_the_english_slides_for_a_language_the_method_does_not_speak()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var url = Template(ctx, "ddd", new() { ["main"] = Method() });

            var (_, entry) = await Describe(ctx, url, "en");

            Assert.IsFalse(entry.GetProperty("speaks").GetBoolean(), "il metodo è solo italiano: non si spunta");
            Assert.AreEqual("en", entry.GetProperty("slidesLanguage").GetString());
            CollectionAssert.AreEqual(new[] { "it" }, JsonSerializer.Deserialize<string[]>(entry.GetProperty("languages").GetRawText()));
            StringAssert.Contains(await ctx.Client.GetStringAsync(entry.GetProperty("page").GetString()), "The domain before the code");
        }

        [TestMethod]
        public async Task Show_a_stand_in_slide_for_a_template_without_slides()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var url = Template(ctx, "ddd", new() { ["main"] = Method(slides: null) });

            var (status, entry) = await Describe(ctx, url, "en");

            Assert.AreEqual(HttpStatusCode.OK, status, entry.ToString());
            Assert.AreEqual(JsonValueKind.Null, entry.GetProperty("slidesLanguage").ValueKind);
            var html = await ctx.Client.GetStringAsync(entry.GetProperty("page").GetString());
            StringAssert.Contains(html, "Sviluppo con DDD", "il titolo del manifesto: il template non è nell'elenco");
            StringAssert.Contains(html, "Only in: Italian");
            StringAssert.Contains(html, "This template has no presentation yet.");
        }

        [TestMethod]
        public async Task Read_the_branch_of_the_chosen_language()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            const string languages = "  it: { structure: main }\n  en: { structure: en }\n";
            var english = Method("  en: .mde-template/ddd/presentation.md\n");
            english[".mde-template/ddd/mde-template.yml"] = Manifest("  en: .mde-template/ddd/presentation.md\n", languages).Replace("version: 1.0.0", "version: 1.2.0");
            var italian = Method();
            italian[".mde-template/ddd/mde-template.yml"] = Manifest(BothSlides, languages);
            var url = Template(ctx, "ddd", new() { ["main"] = italian, ["en"] = english });

            var (_, entry) = await Describe(ctx, url, "en");

            Assert.IsTrue(entry.GetProperty("speaks").GetBoolean());
            Assert.AreEqual("1.2.0", entry.GetProperty("version").GetString(), "il manifesto del ramo inglese");

            var (_, italianEntry) = await Describe(ctx, url, "it");
            Assert.AreEqual("1.0.0", italianEntry.GetProperty("version").GetString());
            Assert.AreEqual(HttpStatusCode.OK, (await ctx.Client.GetAsync(entry.GetProperty("page").GetString())).StatusCode,
                "la copia del ramo inglese resta: la pulizia toglie solo i commit vecchi dello stesso ramo");
        }

        [TestMethod]
        public async Task Read_a_new_commit_instead_of_the_cached_one()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var url = Template(ctx, "ddd", new() { ["main"] = Method() });
            var (_, first) = await Describe(ctx, url, "it");
            StringAssert.Contains(await ctx.Client.GetStringAsync(first.GetProperty("page").GetString()), "Il dominio prima del codice");

            var work = Path.Combine(Path.GetDirectoryName(url), "lavoro");
            File.WriteAllText(Path.Combine(work, ".mde-template", "ddd", "presentazione.md"), "---\ndocument_type: slides\n---\n# Versione nuova\n");
            Git(work, "commit -q -am nuova"); Git(work, $"push -q \"{url}\" main");

            var (_, second) = await Describe(ctx, url, "it");

            Assert.AreNotEqual(first.GetProperty("page").GetString(), second.GetProperty("page").GetString());
            StringAssert.Contains(await ctx.Client.GetStringAsync(second.GetProperty("page").GetString()), "Versione nuova");
            Assert.AreEqual(HttpStatusCode.NotFound, (await ctx.Client.GetAsync(first.GetProperty("page").GetString())).StatusCode,
                "la copia del commit vecchio non serve più");
        }

        [TestMethod]
        public async Task Say_what_cannot_be_reached_or_read()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();

            var (status, body) = await Describe(ctx, Path.Combine(ctx.Factory.DataDir, "non-ce"), "it");
            Assert.AreEqual(HttpStatusCode.BadGateway, status);
            StringAssert.Contains(body.GetProperty("error").GetString(), "I cannot reach the template");

            var missing = Method();
            missing.Remove(".mde-template/ddd/presentazione.md");
            var url = Template(ctx, "ddd", new() { ["main"] = missing });
            (status, body) = await Describe(ctx, url, "it");
            Assert.AreEqual(HttpStatusCode.BadGateway, status);
            StringAssert.Contains(body.GetProperty("error").GetString(), "names the slides .mde-template/ddd/presentazione.md, which are not in the branch 'main'");
        }

        [TestMethod]
        public async Task Show_the_empty_project_in_its_language_or_in_english()
        {
            using var ctx = new AgentCityContext();
            StringAssert.Contains(await ctx.Client.GetStringAsync("/api/templateshowcase/empty?language=it"), "Progetto vuoto");
            StringAssert.Contains(await ctx.Client.GetStringAsync("/api/templateshowcase/empty?language=de"), "Empty project");
        }

        [TestMethod]
        public async Task Say_whether_the_template_has_its_theory_in_the_chosen_language()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var files = Method();
            files[".mde-template/ddd/mde-template.yml"] = Manifest(BothSlides) + "theory:\n  it: { from: .mde-template/ddd/teoria, to: teoria/ddd }\n";
            files[".mde-template/ddd/teoria/perché.md"] = "# Perché\n";
            var url = Template(ctx, "ddd", new() { ["main"] = files });

            Assert.IsTrue((await Describe(ctx, url, "it")).Body.GetProperty("theory").GetBoolean());
            Assert.IsFalse((await Describe(ctx, url, "en")).Body.GetProperty("theory").GetBoolean(), "in inglese il metodo non c'è: niente da offrire");
        }
    }
}
