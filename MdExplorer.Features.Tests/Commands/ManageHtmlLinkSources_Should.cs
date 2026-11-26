using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Commands;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Commands
{
    /// <summary>
    /// Un'immagine scritta in HTML (<c>&lt;img src="assets/x.png"&gt;</c>) nella pagina di MDE non si vedeva mai: l'indirizzo
    /// arrivava al servizio senza <c>connectionId</c> (26/11/2026). Ora l'HTML riceve la stessa regola dei link markdown.
    /// </summary>
    [TestClass]
    public class ManageHtmlLinkSources_Should
    {
        private static string Rewrite(string markdown, RequestInfo info = null)
            => new ManageHtmlLinkSources(NullLogger<ManageHtmlLinkSources>.Instance)
                .TransformInNewMDFromMD(markdown, info ?? new RequestInfo { ConnectionId = "c1" });

        [TestMethod]
        public void Add_the_connection_to_a_relative_image_and_keep_it_relative()
        {
            Assert.AreEqual("<img src=\"assets/logo.png?connectionId=c1\" width=\"40\">\n",
                Rewrite("<img src=\"assets/logo.png\" width=\"40\">\n"));
        }

        [TestMethod]
        public void Prefix_a_project_root_image_and_keep_single_quotes()
        {
            Assert.AreEqual("Logo <img src='/api/mdexplorer/docs/assets/logo.png?connectionId=c1'> qui\n",
                Rewrite("Logo <img src='/docs/assets/logo.png'> qui\n"));
        }

        [TestMethod]
        public void Put_the_connection_before_the_anchor_of_a_link_to_a_document()
        {
            Assert.AreEqual("<a href=\"../altro.md?connectionId=c1#sezione\">Altro</a>\n",
                Rewrite("<a href=\"../altro.md#sezione\">Altro</a>\n"));
        }

        [TestMethod]
        public void Leave_web_addresses_anchors_and_code_alone()
        {
            const string md = "<a href=\"https://example.com/a.png\">w</a> <a href=\"#su\">su</a>\n\n```\n<img src=\"assets/x.png\">\n```\n\n`<img src=\"y.png\">`\n";
            Assert.AreEqual(md, Rewrite(md));
        }

        [TestMethod]
        public void Do_nothing_in_a_static_export()
        {
            const string md = "<img src=\"assets/logo.png\">\n";
            Assert.AreEqual(md, Rewrite(md, new RequestInfo { ConnectionId = "c1", StaticExport = true }));
        }
    }
}
