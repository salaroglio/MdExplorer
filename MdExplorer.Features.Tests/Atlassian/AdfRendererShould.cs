using MdExplorer.Features.Services.Atlassian;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Atlassian
{
    [TestClass]
    public class AdfRendererShould
    {
        [TestMethod]
        public void ReturnEmptyForNullOrWhitespace()
        {
            Assert.AreEqual(string.Empty, AdfRenderer.ToText((string)null));
            Assert.AreEqual(string.Empty, AdfRenderer.ToText("   "));
        }

        [TestMethod]
        public void ReturnRawStringWhenNotJson()
        {
            // A non-ADF plain string is returned verbatim (rendering choice, not a
            // functional fallback — there is no precondition being silently masked).
            Assert.AreEqual("just plain text", AdfRenderer.ToText("just plain text"));
        }

        [TestMethod]
        public void FlattenParagraphText()
        {
            const string adf = @"{
                ""type"":""doc"",""version"":1,
                ""content"":[
                    {""type"":""paragraph"",""content"":[{""type"":""text"",""text"":""Hello world""}]}
                ]
            }";
            Assert.AreEqual("Hello world", AdfRenderer.ToText(adf));
        }

        [TestMethod]
        public void FlattenBulletListToMarkdown()
        {
            const string adf = @"{
                ""type"":""doc"",""version"":1,
                ""content"":[
                    {""type"":""bulletList"",""content"":[
                        {""type"":""listItem"",""content"":[{""type"":""paragraph"",""content"":[{""type"":""text"",""text"":""first""}]}]},
                        {""type"":""listItem"",""content"":[{""type"":""paragraph"",""content"":[{""type"":""text"",""text"":""second""}]}]}
                    ]}
                ]
            }";
            var text = AdfRenderer.ToText(adf);
            StringAssert.Contains(text, "- first");
            StringAssert.Contains(text, "- second");
        }

        [TestMethod]
        public void RenderHeadingWithHashes()
        {
            const string adf = @"{
                ""type"":""doc"",""version"":1,
                ""content"":[
                    {""type"":""heading"",""attrs"":{""level"":2},""content"":[{""type"":""text"",""text"":""Title""}]}
                ]
            }";
            StringAssert.Contains(AdfRenderer.ToText(adf), "## Title");
        }
    }
}
