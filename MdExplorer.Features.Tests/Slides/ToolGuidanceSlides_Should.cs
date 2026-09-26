using MdExplorer.Abstractions.Models.AI;
using MdExplorer.bll.Services.AI;
using MdExplorer.Features.Services.SourceMapping;
using MdExplorer.Features.Slides;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace MdExplorer.Features.Tests.Slides
{
    /// <summary>
    /// The chat providers with MdExplorer's own tools (Gemini, OpenAI, local) do not read skills:
    /// their slide rule is in ToolGuidanceBuilder, and its example must come out of the engine as
    /// the rule says.
    /// </summary>
    [TestClass]
    public class ToolGuidanceSlides_Should
    {
        private static string Example(string guidance)
        {
            const string start = "SLIDE DECK EXAMPLE:";
            const string end = "END OF EXAMPLE";
            var from = guidance.IndexOf(start, StringComparison.Ordinal) + start.Length;
            var to = guidance.IndexOf(end, from, StringComparison.Ordinal);
            return guidance.Substring(from, to - from).Replace("\r\n", "\n").TrimStart('\n');
        }

        [TestMethod]
        public void Teach_an_example_the_engine_renders_as_the_rule_says()
        {
            var page = SlideDeckRenderer.Render(
                Example(ToolGuidanceBuilder.BuildForProvider(ProviderType.Gemini)),
                new SlideDeckRenderOptions { Pipeline = DocumentViewPipeline.Build(null) });

            var sections = SlidePage.Sections(page);
            Assert.AreEqual(3, sections.Length);
            Assert.AreEqual(2, sections[1].SelectNodes(".//li[@class='fragment']").Count);
            StringAssert.Contains(sections[1].SelectSingleNode("aside[@class='notes']").InnerText, "outage");
            Assert.AreEqual("#1b2a3a", sections[2].GetAttributeValue("data-background-color", null));
        }

        [TestMethod]
        public void Not_teach_the_old_html_format_any_more()
        {
            foreach (var provider in new[] { ProviderType.Gemini, ProviderType.OpenAI, ProviderType.Local })
            {
                var guidance = ToolGuidanceBuilder.BuildForProvider(provider);
                Assert.IsFalse(guidance.Contains("create_slide_presentation"), provider.ToString());
                Assert.IsFalse(guidance.Contains("data-template"), provider.ToString());
            }
        }
    }
}
