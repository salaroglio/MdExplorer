using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Commands;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Commands
{
    /// <summary>
    /// A link written as "../assets/pagina.html" reached the page as "/api/mdexplorer../assets/…"
    /// (no slash: 404, 30/09/2026). It is relative to the folder of the document: it stays relative,
    /// the browser resolves it against the document's own address.
    /// </summary>
    [TestClass]
    public class ManageLinkAbsolutePath_Should
    {
        private static string Rewrite(string markdown)
            => new ManageLinkAbsolutePath(NullLogger<ManageLinkAbsolutePath>.Instance)
                .TransformInNewMDFromMD(markdown, new RequestInfo { ConnectionId = "c1" });

        [TestMethod]
        public void KeepAParentRelativeLinkRelative()
        {
            Assert.AreEqual("[p](../assets/pagina.html?connectionId=c1)", Rewrite("[p](../assets/pagina.html)"));
        }

        [TestMethod]
        public void KeepTheAnchorOfAParentRelativeLink()
        {
            Assert.AreEqual("[p](../altro.md?connectionId=c1#/3)", Rewrite("[p](../altro.md#/3)"));
        }

        [TestMethod]
        public void PrefixAProjectAbsoluteLink()
        {
            Assert.AreEqual("[p](/api/mdexplorer/assets/pagina.html?connectionId=c1)", Rewrite("[p](/assets/pagina.html)"));
        }

        [TestMethod]
        public void AddConnectionIdAfterThePagesOfALinkToADeck()
        {
            Assert.AreEqual("[c](vendite.md?pages=2,6-9&connectionId=c1)", Rewrite("[c](vendite.md?pages=2,6-9)"));
            Assert.AreEqual("[c](../vendite.md?pages=2-&connectionId=c1#/1)", Rewrite("[c](../vendite.md?pages=2-#/1)"));
        }

        [TestMethod]
        public void LeaveAPlainFileLinkAlone()
        {
            Assert.AreEqual("[c](dati.json)", Rewrite("[c](dati.json)"));
        }
    }
}
