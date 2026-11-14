using System.IO;
using System.Text.RegularExpressions;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Commands.html;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Commands
{
    /// <summary>
    /// A PlantUML diagram stayed on the left while a pasted image is centered: the SVG goes in a
    /// full-width .defaultImg, where margin:auto centers nothing. The container of a diagram gets
    /// mdePlantuml (text-align:center in MdCustomCSS.css); the other images with curly brackets don't.
    /// </summary>
    [TestClass]
    public class PlantumlIsCenteredShould
    {
        private static RequestInfo Request() => new RequestInfo
        {
            AbsolutePathFile = Path.Combine(Path.GetTempPath(), "mde-plantuml-centered", "doc.md"),
            CurrentQueryRequest = "doc.md",
            CurrentRoot = Path.Combine(Path.GetTempPath(), "mde-plantuml-centered"),
            ConnectionId = "c1",
        };

        private static int Count(string text, string what) => Regex.Matches(text, Regex.Escape(what)).Count;

        [TestMethod]
        public void OnlyTheDiagramContainerIsMarked()
        {
            var command = new ToolbarImagesHtml(NullLogger<ToolbarImagesHtml>.Instance, new Helper(NullLogger<Helper>.Instance));
            var diagram = "![](/.md/abc.svg){ md-plantuml=\"dynamic:false;copy:true;linkHasClass:false;linkClassHasCSS:false;\"}";
            var styled = "![b](/y.png){.classe}";

            var result = command.TransformInNewMDFromMD(diagram + "\n\nTesto.\n\n" + styled + "\n", Request());

            Assert.AreEqual(2, Count(result, "class=\"defaultImg "), "both images with curly brackets get the container");
            Assert.AreEqual(1, Count(result, "class=\"defaultImg mdePlantuml\""), "only the diagram is centered");
        }
    }
}
