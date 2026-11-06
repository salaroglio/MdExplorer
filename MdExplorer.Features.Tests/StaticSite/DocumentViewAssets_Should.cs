using MdExplorer.Features.StaticSite;
using MdExplorer.Features.Tests.Slides;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace MdExplorer.Features.Tests.StaticSite
{
    /// <summary>
    /// The document page of the export loads what common.js loads, less what talks to the service. Read on
    /// the real common.js: a script added there is in the export unless it is named in NotInExport.
    /// </summary>
    [TestClass]
    public class DocumentViewAssets_Should
    {
        private static string WebRoot => Path.Combine(SlidePage.RepositoryRoot(), "MdExplorer", "wwwroot");

        private static string CommonJs => File.ReadAllText(Path.Combine(WebRoot, "common.js"));

        [TestMethod]
        public void Read_common_js_in_its_order_each_file_once()
        {
            var files = DocumentViewAssets.ReadCommonJs(CommonJs).ToList();

            Assert.AreEqual("/bootstrap/jquery-3.6.0.js", files[0]);
            Assert.IsTrue(files.IndexOf("/javascripts/jqueryForFirstPage/core/globals.js") < files.IndexOf("/javascripts/jqueryForFirstPage/core/init.js"));
            Assert.AreEqual(1, files.Count(f => f == "/tippy/tippy.js"), "listed twice in common.js, loaded once");
            Assert.IsTrue(files.Contains("/prismjs/prism-tomorrow.min.css"), "a style written with document.write");
        }

        [TestMethod]
        public void Name_only_files_that_common_js_loads_among_those_left_out()
        {
            var files = DocumentViewAssets.ReadCommonJs(CommonJs);

            // A file renamed in common.js would otherwise slip back into the export unnoticed.
            foreach (var file in DocumentViewAssets.NotInExport.Keys)
            {
                Assert.IsTrue(files.Contains(file), $"{file} is not in common.js any more: update NotInExport");
            }
        }

        [TestMethod]
        public void Copy_every_file_the_exported_scripts_name_by_themselves()
        {
            // A script that sets an icon with mdeAsset('…') needs that file in the zip.
            var named = DocumentViewAssets.ForExport(CommonJs)
                .Where(f => f.EndsWith(".js"))
                .SelectMany(f => System.Text.RegularExpressions.Regex.Matches(File.ReadAllText(Path.Combine(WebRoot, f.TrimStart('/'))), @"mdeAsset\(\s*['""]([\w./-]+)['""]\s*\)"))
                .Select(m => m.Groups[1].Value)
                .Distinct()
                .ToList();

            Assert.IsTrue(named.Count > 0, "the scripts name their icons with mdeAsset");
            CollectionAssert.IsSubsetOf(named, DocumentViewAssets.ScriptAssets.ToList());
            foreach (var asset in DocumentViewAssets.ScriptAssets)
            {
                Assert.IsTrue(File.Exists(Path.Combine(WebRoot, asset)), asset);
            }
        }

        [TestMethod]
        public void Load_in_an_export_only_files_that_exist_and_none_that_talk_to_the_service()
        {
            var files = DocumentViewAssets.ForExport(CommonJs);

            foreach (var file in files)
            {
                Assert.IsTrue(File.Exists(Path.Combine(WebRoot, file.TrimStart('/'))), $"{file} is not in wwwroot");
            }
            Assert.IsFalse(files.Any(f => f.Contains("mermaid") || f.Contains("kg-manager") || f.Contains("inline-edit")));
            Assert.IsTrue(files.Contains("/TocBot/tocbot.min.js"));
            Assert.IsTrue(files.Contains("/javascripts/jqueryForFirstPage/core/init.js"));
        }
    }
}
