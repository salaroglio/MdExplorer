using System;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>Gli avvisi dell'ultima estrazione per cartella (12/11/2026): MarkAgent li legge da code_inventory, il report li elenca tutti.</summary>
    [TestClass]
    public class CodeAnalysisWarningsShould
    {
        [TestMethod]
        public void Keep_the_warnings_of_each_folder_and_tell_none_recorded_from_no_warnings()
        {
            var project = Path.Combine(Path.GetTempPath(), "mde-warnings-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(project);
            try
            {
                Assert.IsNull(CodeAnalysisWarnings.Read(project, "app"), "nessuna estrazione registrata: null, non «nessun avviso»");
                CodeAnalysisWarnings.Write(project, "app", 3, 10, new[] { "Dl.vbproj: manca X", "Dl.vbproj: manca X", " " });
                CodeAnalysisWarnings.Write(project, "db", 5, 5, Array.Empty<string>());
                var app = CodeAnalysisWarnings.Read(project, "app");
                CollectionAssert.AreEqual(new[] { "Dl.vbproj: manca X" }, app.Warnings, "doppioni e righe vuote via");
                Assert.AreEqual((3, 10), (app.Reparsed, app.Files));
                Assert.AreEqual(0, CodeAnalysisWarnings.Read(project, "db").Warnings.Count);
                CollectionAssert.AreEqual(new[] { "app", "db" }, CodeAnalysisWarnings.ReadAll(project).Select(e => e.Folder).ToArray(), "una cartella non cancella l'altra");
            }
            finally { try { Directory.Delete(project, true); } catch (IOException) { } }
        }

        [TestMethod]
        public void Keep_the_kind_of_each_warning_and_summarize_by_kind_with_one_example()
        {
            // 14/11/2026: code_inventory dà la sintesi per tipo, e l'elenco intero di un tipo solo a chi lo chiede
            var project = Path.Combine(Path.GetTempPath(), "mde-warnings-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(project);
            try
            {
                var long_ = "Web.vbproj: 47 file non compilati: " + string.Join(", ", Enumerable.Range(1, 47).Select(i => $"cartella/sotto/File{i}.vb"));
                var w = new CodeWarnings();
                w.Add(CodeWarnings.ProjectMissing, "Bl.vbproj: riferisce Dl, che non c'è");
                w.Add(CodeWarnings.FilesNotCompiled, long_);
                w.Add(CodeWarnings.ProjectMissing, "Web.vbproj: riferisce Bl, che non c'è");
                w.Add(CodeWarnings.ProjectMissing, "Bl.vbproj: riferisce Dl, che non c'è");   // doppione
                w.Add("senza tipo");
                CodeAnalysisWarnings.Write(project, "app", 3, 10, w);

                var app = CodeAnalysisWarnings.Read(project, "app");
                Assert.AreEqual(4, app.Warnings.Count);
                CollectionAssert.AreEqual(new[] { CodeWarnings.ProjectMissing, CodeWarnings.FilesNotCompiled, CodeWarnings.ProjectMissing, CodeWarnings.Other }, app.Kinds);

                var summary = CodeAnalysisWarnings.Summary(app);
                Assert.AreEqual("referenced-project-missing:2 | files-not-compiled:1 | other:1", string.Join(" | ", summary.Select(k => $"{k.Kind}:{k.Count}")), "prima i più numerosi");
                Assert.AreEqual("Bl.vbproj: riferisce Dl, che non c'è", summary[0].Example);
                var cut = summary.Single(k => k.Kind == CodeWarnings.FilesNotCompiled).Example;
                Assert.IsTrue(cut.StartsWith(long_[..CodeAnalysisWarnings.ExampleChars]) && cut.Length < long_.Length, "l'esempio lungo è accorciato");
                StringAssert.Contains(cut, (long_.Length - CodeAnalysisWarnings.ExampleChars).ToString(), "e dice di quanto");

                CollectionAssert.AreEqual(new[] { long_ }, CodeAnalysisWarnings.OfKind(app, CodeWarnings.FilesNotCompiled), "l'elenco di un tipo è intero");
                Assert.AreEqual(4, CodeAnalysisWarnings.OfKind(app, "all").Count);
                Assert.AreEqual(0, CodeAnalysisWarnings.OfKind(app, "non-esiste").Count);

                // un file scritto prima dei tipi: il tipo non si sa, e si dice
                CodeAnalysisWarnings.Write(project, "vecchio", 1, 1, new[] { "un avviso" });
                var old = CodeAnalysisWarnings.Read(project, "vecchio");
                old.Kinds = null;
                Assert.AreEqual(CodeAnalysisWarnings.KindNotRecorded, CodeAnalysisWarnings.Summary(old).Single().Kind);
            }
            finally { try { Directory.Delete(project, true); } catch (IOException) { } }
        }

        [TestMethod]
        public void Carry_the_kinds_when_lists_are_joined()
        {
            var a = new CodeWarnings();
            a.Add(CodeWarnings.LibraryMissing, "manca log4net");
            var b = new CodeWarnings { "senza tipo" };
            b.AddRange(a);                                         // un'altra lista: col suo tipo
            b.AddRange(a, t => "ponti: " + t);                     // col testo cambiato, il tipo resta
            b.AddRange(CodeWarnings.Bridges, new[] { "x", "y" });  // testi tutti di un tipo
            Assert.AreEqual("other:senza tipo | library-missing:manca log4net | library-missing:ponti: manca log4net | bridges:x | bridges:y",
                string.Join(" | ", b.Typed.Select(w => $"{w.Kind}:{w.Text}")));
            b.TruncateTo(2);
            Assert.AreEqual(2, b.Count);
            Assert.AreEqual(CodeWarnings.LibraryMissing, b.KindAt(1));
            // verso l'interfaccia (l'esito dell'analisi in JSON) resta un elenco di testi, come prima
            Assert.AreEqual("[\"senza tipo\",\"manca log4net\"]", System.Text.Json.JsonSerializer.Serialize(b));
            Assert.AreEqual("{\"W\":[\"senza tipo\",\"manca log4net\"]}", System.Text.Json.JsonSerializer.Serialize(new { W = b }));
        }
    }
}
