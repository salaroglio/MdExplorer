using System;
using System.IO;
using System.Linq;
using System.Threading;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>D43 F3, «not-analyzed»: i file di una cartella program per stato, come li vede MdExplorer sul disco.</summary>
    [TestClass]
    public class CodeQueryNotAnalyzed_Should
    {
        private string _project;

        [TestInitialize]
        public void Setup() { _project = Path.Combine(Path.GetTempPath(), "mde-notanalyzed-tests", Guid.NewGuid().ToString("N")); Directory.CreateDirectory(_project); }

        [TestCleanup]
        public void Cleanup() { try { Directory.Delete(_project, true); } catch { } }

        private void Write(string relative)
        {
            var path = Path.Combine(_project, relative.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, "x");
        }

        [TestMethod]
        public void Sort_the_files_of_a_program_folder_by_what_the_graph_did_with_them()
        {
            Write("program/src/A.java"); Write("program/src/B.java"); Write("program/web/app.js");
            Write("program/conf/beans.xml"); Write("program/conf/app.properties"); Write("program/target/Gen.java");
            Write(".mde-code/program/src/A.java.ast.ttl");   // A è nel grafo, B no
            var scope = new CodeProgramScope
            {
                Folder = "program", Languages = new[] { "java" }, AbsolutePath = Path.Combine(_project, "program"),
                ToLogical = abs => CodeGraphPaths.ToProjectRelative(_project, abs),
            };
            var groups = CodeQueryService.Classify(_project, scope, CodeLanguageCatalog.ForProject(_project), CancellationToken.None);
            string Of(string status, string ext) => string.Join(",", groups.Where(g => g.Status == status && g.Extension == ext).Select(g => g.Count));
            Assert.AreEqual("1", Of("analyzed", ".java"));
            Assert.AreEqual("1", Of("chosen-not-extracted", ".java"), "B: java scelto ma senza script");
            Assert.AreEqual("1", Of("language-not-chosen", ".js"));
            Assert.AreEqual("1", Of("no-parser", ".xml"));
            Assert.AreEqual("1", Of("no-parser", ".properties"));
            Assert.IsFalse(groups.Any(g => g.Examples.Any(e => e.Contains("/target/"))), "le cartelle di build no, come nella scansione");
            CollectionAssert.AreEqual(new[] { "program/conf/beans.xml" }, groups.Single(g => g.Extension == ".xml").Examples);
        }
    }
}

namespace MdExplorer.IntegrationTests
{
    /// <summary>D43: le domande pronte controllano nome e parametri PRIMA di toccare il grafo, e dicono come rimediare.</summary>
    [TestClass]
    public class CodeQueryParameters_Should
    {
        [TestMethod]
        public async System.Threading.Tasks.Task Refuse_unknown_questions_unknown_parameters_and_numbers_out_of_range()
        {
            var service = new CodeQueryService(null, null, null);   // non deve servire: il rifiuto viene prima
            async System.Threading.Tasks.Task<string> Refusal(string name, System.Collections.Generic.Dictionary<string, string> p)
                => (await Assert.ThrowsExceptionAsync<CodeQueryException>(() => service.RunAsync("/x", name, p))).Message;
            StringAssert.Contains(await Refusal("everything", null), "unknown-externals");
            StringAssert.Contains(await Refusal("hotspots", new() { ["depth"] = "2" }), "folder");
            StringAssert.Contains(await Refusal("hotspots", new() { ["limit"] = "9999" }), "200");
            StringAssert.Contains(await Refusal("flow-from", new()), "symbol");
            StringAssert.Contains(await Refusal("program-outline", new()), "folder");
        }
    }
}
