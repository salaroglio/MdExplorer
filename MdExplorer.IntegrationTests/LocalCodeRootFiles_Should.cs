using System;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.RemoteProtocol;
using MdExplorer.Services.RemoteFolders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// FL (sprint 2026-10-21-Cartella-Remota-Via-SSH, D8): una radice esterna LOCALE si modifica dall'albero con le stesse
    /// regole di MDE Remote (dentro la radice, impronta per scrivere, mai sopra un file esistente).
    /// </summary>
    [TestClass]
    public class LocalCodeRootFiles_Should
    {
        static LocalCodeRootFiles_Should() => System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        private string _root;
        private CodeRootState _state;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-localroot", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(_root, "src"));
            File.WriteAllText(Path.Combine(_root, "src", "a.cs"), "class A {}\n");
            _state = new CodeRootState { Alias = "codice", Status = CodeRootStatus.Linked, RootPath = _root };
        }

        [TestCleanup]
        public void Cleanup() { try { Directory.Delete(_root, true); } catch { } }

        private RemoteResponse Op(string op, string path, string text = null, string stamp = null, string to = null, bool recursive = false)
            => RemoteCodeRootFiles.OperateLocal(_state, new RemoteRequest { Op = op, Path = path, Text = text, Stamp = stamp, To = to, Recursive = recursive });

        [TestMethod]
        public void Create_edit_rename_and_delete_on_the_disk()
        {
            var created = Op(RemoteOps.Create, "docs/nota.md", "# Nota\n");
            Assert.AreEqual("# Nota\n", File.ReadAllText(Path.Combine(_root, "docs", "nota.md")));
            var read = Op(RemoteOps.Read, "docs/nota.md");
            Assert.AreEqual(created.Stamp, read.Stamp);
            Op(RemoteOps.Write, "docs/nota.md", "# Nota cambiata\n", read.Stamp);
            Assert.AreEqual("# Nota cambiata\n", File.ReadAllText(Path.Combine(_root, "docs", "nota.md")));
            Op(RemoteOps.Move, "docs/nota.md", to: "docs/rinominata.md");
            Assert.IsTrue(File.Exists(Path.Combine(_root, "docs", "rinominata.md")));
            Op(RemoteOps.Delete, "docs", recursive: true);
            Assert.IsFalse(Directory.Exists(Path.Combine(_root, "docs")));
        }

        [TestMethod]
        public void Refuse_with_the_code_like_the_agent()
        {
            var stale = Op(RemoteOps.Read, "src/a.cs").Stamp;
            File.AppendAllText(Path.Combine(_root, "src", "a.cs"), "// cambiato da un altro\n");
            Assert.AreEqual(RemoteErrorCodes.Changed, Assert.ThrowsException<RemoteOperationException>(() => Op(RemoteOps.Write, "src/a.cs", "x", stale)).Code);
            Assert.AreEqual(RemoteErrorCodes.Exists, Assert.ThrowsException<RemoteOperationException>(() => Op(RemoteOps.Create, "src/a.cs", "x")).Code);
            Assert.AreEqual(RemoteErrorCodes.OutsideRoot, Assert.ThrowsException<RemoteOperationException>(() => Op(RemoteOps.Read, "../fuori.txt")).Code);
            Assert.AreEqual(RemoteErrorCodes.NotEmpty, Assert.ThrowsException<RemoteOperationException>(() => Op(RemoteOps.Delete, "src")).Code);
        }

        [TestMethod]
        public void Validate_and_save_the_encoding_rules_of_a_root_in_the_development_yml()
        {
            var rules = MdExplorer.Services.CodeGraph.CodeRootService.NormalizeEncodings(new[]
            {
                new MdExplorer.Service.Models.CodeRootEncodingRule { Extensions = new System.Collections.Generic.List<string> { "CBL, .cpy", "cob" }, Encoding = TextDecoding.Ibm850 },
                new MdExplorer.Service.Models.CodeRootEncodingRule { Extensions = new System.Collections.Generic.List<string>(), Encoding = TextDecoding.Utf8 },
            });
            Assert.AreEqual(1, rules.Count, "le regole vuote si tolgono");
            CollectionAssert.AreEqual(new[] { ".cbl", ".cpy", ".cob" }, rules[0].Extensions, "punto e minuscole");

            Assert.ThrowsException<InvalidOperationException>(() => MdExplorer.Services.CodeGraph.CodeRootService.NormalizeEncodings(new[]
            {
                new MdExplorer.Service.Models.CodeRootEncodingRule { Extensions = new System.Collections.Generic.List<string> { ".cbl" }, Encoding = TextDecoding.Ibm850 },
                new MdExplorer.Service.Models.CodeRootEncodingRule { Extensions = new System.Collections.Generic.List<string> { ".CBL" }, Encoding = TextDecoding.Windows1252 },
            }), "un'estensione in due regole");
            Assert.ThrowsException<InvalidOperationException>(() => MdExplorer.Services.CodeGraph.CodeRootService.NormalizeEncodings(new[]
            {
                new MdExplorer.Service.Models.CodeRootEncodingRule { Extensions = new System.Collections.Generic.List<string> { ".cbl" }, Encoding = "ebcdic" },
            }), "una codifica che MDE non sa usare");
        }

        [TestMethod]
        public void Read_a_local_root_file_with_the_rule_of_its_extension()
        {
            File.WriteAllBytes(Path.Combine(_root, "src", "p.cbl"), System.Text.Encoding.GetEncoding(1252).GetBytes("perché\n"));
            var withRules = new CodeRootState { Alias = "codice", Status = CodeRootStatus.Linked, RootPath = _root,
                ExtensionEncodings = new System.Collections.Generic.Dictionary<string, string> { [".cbl"] = TextDecoding.Ibm850 } };
            var read = RemoteCodeRootFiles.OperateLocal(withRules, new RemoteRequest { Op = RemoteOps.Read, Path = "src/p.cbl" });
            Assert.AreEqual(TextDecoding.Ibm850, read.Encoding, "la regola della radice arriva fino alla lettura");
            Assert.AreEqual(TextDecoding.Windows1252, RemoteCodeRootFiles.OperateLocal(_state, new RemoteRequest { Op = RemoteOps.Read, Path = "src/p.cbl" }).Encoding, "senza regola: la stima");
        }
    }
}
