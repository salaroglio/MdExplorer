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
            StringAssert.Contains(await Refusal("unknown-externals", new() { ["members"] = "5000" }), "2000");
        }

        /// <summary>Il grafo finto: solo gli usi degli esterni, il resto non deve servire.</summary>
        public class ExternalsOnlyStore : System.Reflection.DispatchProxy
        {
            public System.Collections.Generic.List<MdExplorer.Features.Services.CodeGraph.CodeExternalUse> Uses { get; set; }
            protected override object Invoke(System.Reflection.MethodInfo targetMethod, object[] args)
                => targetMethod.Name == nameof(MdExplorer.Features.Services.CodeGraph.ICodeGraphStore.ExternalUsesAsync)
                    ? System.Threading.Tasks.Task.FromResult(Uses)
                    : throw new System.NotSupportedException(targetMethod.Name);
        }

        /// <summary>
        /// 10/11/2026: un linguaggio senza tipi (un linguaggio da pacchetto) mette tutte le chiamate esterne nel gruppo `?`; prima se ne
        /// vedevano 10 fisse, senza dirlo. Ora 10 per default con l'avviso e il totale, tutte con «members».
        /// </summary>
        [TestMethod]
        public async System.Threading.Tasks.Task Give_every_external_call_of_a_language_without_types_when_asked()
        {
            var store = System.Reflection.DispatchProxy.Create<MdExplorer.Features.Services.CodeGraph.ICodeGraphStore, ExternalsOnlyStore>();
            ((ExternalsOnlyStore)(object)store).Uses = System.Linq.Enumerable.Range(1, 25)
                .Select(i => new MdExplorer.Features.Services.CodeGraph.CodeExternalUse { QualifiedName = $"PRIM{i:00}", Relation = "calls", Uses = 100 - i, Callers = 1, Example = $"@legacy/a.toy:{i}" })
                .ToList();
            var service = new CodeQueryService(store, null, null);
            System.Text.Json.JsonElement Run(System.Collections.Generic.Dictionary<string, string> p)
                => System.Text.Json.JsonSerializer.SerializeToElement(service.RunAsync("/x", "unknown-externals", p).GetAwaiter().GetResult());

            var cut = Run(new()).GetProperty("types")[0];
            Assert.AreEqual("?", cut.GetProperty("Type").GetString());
            Assert.AreEqual(25, cut.GetProperty("membersTotal").GetInt32());
            Assert.AreEqual(10, cut.GetProperty("members").GetArrayLength(), "per default come prima");
            StringAssert.Contains(Run(new()).GetProperty("membersCut").GetString(), "members", "e lo dice");

            var all = Run(new() { ["members"] = "2000" });
            Assert.AreEqual(25, all.GetProperty("types")[0].GetProperty("members").GetArrayLength());
            Assert.AreEqual(System.Text.Json.JsonValueKind.Null, all.GetProperty("membersCut").ValueKind, "niente tagliato, niente avviso");
            Assert.AreEqual("PRIM01", all.GetProperty("types")[0].GetProperty("members")[0].GetProperty("Name").GetString(), "dal più usato");
        }
    }
}
