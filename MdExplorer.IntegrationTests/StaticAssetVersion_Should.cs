using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Script e stili della pagina del documento si chiedono con la loro versione e restano nel browser (23/11/2026):
    /// prima ogni documento aperto chiedeva conferma al servizio per un centinaio di file.
    /// </summary>
    [TestClass]
    public class StaticAssetVersion_Should
    {
        [TestMethod]
        public void Change_when_a_script_or_a_style_changes_and_only_then()
        {
            var root = Path.Combine(Path.GetTempPath(), "mde-versione-" + Guid.NewGuid().ToString("N"));
            try
            {
                Directory.CreateDirectory(Path.Combine(root, "javascripts", "tabelle"));
                Directory.CreateDirectory(Path.Combine(root, "client2"));
                File.WriteAllText(Path.Combine(root, "common.js"), "uno");
                File.WriteAllText(Path.Combine(root, "common.css"), "stile");
                File.WriteAllText(Path.Combine(root, "javascripts", "tabelle", "a.js"), "a");
                var first = StaticAssetVersion.Compute(root);
                Assert.AreEqual(16, first.Length);
                Assert.AreEqual(first, StaticAssetVersion.Compute(root), "stessi file, stessa versione");

                File.WriteAllText(Path.Combine(root, "client2", "main.js"), "angular");
                File.WriteAllText(Path.Combine(root, "immagine.png"), "x");
                Assert.AreEqual(first, StaticAssetVersion.Compute(root), "l'applicazione Angular e le immagini non contano");

                File.WriteAllText(Path.Combine(root, "javascripts", "tabelle", "a.js"), "a cambiato");
                var second = StaticAssetVersion.Compute(root);
                Assert.AreNotEqual(first, second, "uno script cambiato");

                File.WriteAllText(Path.Combine(root, "javascripts", "nuovo.js"), "n");
                Assert.AreNotEqual(second, StaticAssetVersion.Compute(root), "uno script in più");
                Assert.AreEqual("0", StaticAssetVersion.Compute(Path.Combine(root, "non-esiste")));
            }
            finally { Directory.Delete(root, true); }
        }

        [TestMethod]
        public async Task Let_the_browser_keep_a_file_asked_with_its_version_and_check_the_others()
        {
            using var ctx = new AgentCityContext();
            var versioned = await ctx.Client.GetAsync("/common.js?v=" + StaticAssetVersion.Current);
            Assert.AreEqual(HttpStatusCode.OK, versioned.StatusCode);
            Assert.AreEqual(StaticAssetVersion.ImmutableCacheControl, string.Join(", ", versioned.Headers.GetValues("Cache-Control")));

            var plain = await ctx.Client.GetAsync("/common.js");
            Assert.AreEqual("no-cache", string.Join(", ", plain.Headers.GetValues("Cache-Control")), "senza versione si chiede conferma, come prima");
        }

        [TestMethod]
        public void Keep_in_common_js_the_loading_lines_the_static_export_reads()
        {
            string commonJs = null;
            for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null && commonJs == null; dir = dir.Parent)
            {
                var candidate = Path.Combine(dir.FullName, "MdExplorer", "wwwroot", "common.js");
                if (File.Exists(candidate)) commonJs = File.ReadAllText(candidate);
            }
            Assert.IsNotNull(commonJs, "wwwroot/common.js non trovato");
            // La versione si aggiunge mentre le righe vengono scritte nella pagina: le righe restano com'erano.
            Assert.IsTrue(Regex.Matches(commonJs, @"loadScriptOnce\('/[^'?]+'\)").Count > 50, "gli script si caricano ancora con il loro percorso nudo");
            Assert.IsFalse(Regex.IsMatch(commonJs, @"loadScriptOnce\('[^']*\?v="), "nessuna versione scritta a mano");
            StringAssert.Contains(commonJs, "document.write = plainDocumentWrite;");
        }
    }
}
