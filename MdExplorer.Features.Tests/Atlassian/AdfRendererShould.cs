using System.Collections.Generic;
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
    
        // Prima l'immagine incorporata spariva senza traccia: l'LLM non sapeva che ci fosse.
        [TestMethod]
        public void LeaveAPlaceholderForAnEmbeddedImage()
        {
            const string adf = @"{
                ""type"":""doc"",""version"":1,
                ""content"":[
                    {""type"":""paragraph"",""content"":[{""type"":""text"",""text"":""Vedi screenshot:""}]},
                    {""type"":""mediaSingle"",""content"":[
                        {""type"":""media"",""attrs"":{""id"":""abc-123"",""type"":""file"",""collection"":""jira-1"",""alt"":""errore.png""}}
                    ]},
                    {""type"":""paragraph"",""content"":[{""type"":""text"",""text"":""fine""}]}
                ]
            }";
            Assert.AreEqual("Vedi screenshot:\n\n[embedded attachment: errore.png]\n\nfine", AdfRenderer.ToText(adf));
        }

        [TestMethod]
        public void NameTheImageFromTheAttachmentsWhenGiven()
        {
            const string adf = @"{
                ""type"":""doc"",""version"":1,
                ""content"":[
                    {""type"":""mediaGroup"",""content"":[
                        {""type"":""media"",""attrs"":{""id"":""f-1"",""type"":""file""}},
                        {""type"":""media"",""attrs"":{""id"":""f-2"",""type"":""file""}}
                    ]}
                ]
            }";
            var names = new Dictionary<string, string> { ["f-1"] = "schema.png" };
            var text = AdfRenderer.ToText(adf, names);
            StringAssert.Contains(text, "[embedded attachment: schema.png]");
            StringAssert.Contains(text, "[embedded attachment (name unknown, see the attachments list)]");
        }

        [TestMethod]
        public void RenderAnExternalImageAsAMarkdownImage()
        {
            const string adf = @"{
                ""type"":""doc"",""version"":1,
                ""content"":[
                    {""type"":""mediaSingle"",""content"":[
                        {""type"":""media"",""attrs"":{""type"":""external"",""url"":""https://example.com/a.png"",""alt"":""logo""}}
                    ]}
                ]
            }";
            Assert.AreEqual("![logo](https://example.com/a.png)", AdfRenderer.ToText(adf));
        }
}
}
