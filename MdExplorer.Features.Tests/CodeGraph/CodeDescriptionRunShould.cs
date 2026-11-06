using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// Punto 2c del 14/10/2026: la traccia dell'ultima descrizione di una cartella program sta nel grafo di stato, così
    /// un'interruzione (MdExplorer chiuso a metà) si vede anche dopo un riavvio. Fuseki vero (FusekiFixture).
    /// </summary>
    [TestClass]
    public class CodeDescriptionRunShould
    {
        private string _project;
        private CodeGraphStore _store;

        [TestInitialize]
        public void Setup()
        {
            _store = FusekiFixture.NewStore();
            _project = Path.Combine(Path.GetTempPath(), "mde-codegraph-tests", Guid.NewGuid().ToString("N"), "project");
            Directory.CreateDirectory(_project);
        }

        [TestCleanup]
        public void Cleanup()
        {
            try { Directory.Delete(Path.GetDirectoryName(_project), true); } catch { /* best effort */ }
        }

        private static CodeDescriptionRun Run(string folder, string status) => new()
        {
            ProgramFolder = folder, Status = status, Engine = "Claude Code / sonnet", Force = false, StartedAt = "2026-10-14T07:00:00Z",
            FileIndex = 120, FileTotal = 15000, CurrentFile = folder + "/src/Main.java", FilesDescribed = 118,
        };

        [TestMethod]
        public async Task Keep_one_trace_per_program_folder_and_replace_it_on_update()
        {
            await _store.UpsertDescriptionRunAsync(_project, Run("programA", CodeDescriptionRun.Running));
            await _store.UpsertDescriptionRunAsync(_project, Run("@backend", CodeDescriptionRun.Running));
            var updated = Run("programA", CodeDescriptionRun.Failed); updated.FileIndex = 121; updated.Error = "l'AI non ha risposto";
            await _store.UpsertDescriptionRunAsync(_project, updated);

            var runs = await _store.GetDescriptionRunsAsync(_project);
            Assert.AreEqual(2, runs.Count);
            var a = runs.Single(r => r.ProgramFolder == "programA");
            Assert.AreEqual(CodeDescriptionRun.Failed, a.Status);
            Assert.AreEqual(121, a.FileIndex); Assert.AreEqual(15000, a.FileTotal); Assert.AreEqual(118, a.FilesDescribed);
            Assert.AreEqual("programA/src/Main.java", a.CurrentFile);
            Assert.AreEqual("l'AI non ha risposto", a.Error);
            Assert.IsFalse(a.Force);
            Assert.IsNotNull(a.UpdatedAt);
        }

        [TestMethod]
        public async Task Forget_a_trace_on_dismiss()
        {
            await _store.UpsertDescriptionRunAsync(_project, Run("programA", CodeDescriptionRun.Running));
            await _store.RemoveDescriptionRunAsync(_project, "programA");
            Assert.AreEqual(0, (await _store.GetDescriptionRunsAsync(_project)).Count);
        }

        [TestMethod]
        public async Task Survive_the_retirement_of_old_scripts()
        {
            // il ritiro azzera lo stato dello SCANNER, non la traccia delle descrizioni
            await _store.UpsertDescriptionRunAsync(_project, Run("programA", CodeDescriptionRun.Running));
            var old = Path.Combine(_project, ".mde-code", "programA", "x.cs.doc.jsonl");
            Directory.CreateDirectory(Path.GetDirectoryName(old));
            File.WriteAllText(old, "{\"schema\":1,\"kind\":\"doc\",\"source\":\"programA/x.cs\",\"writtenBy\":\"x\",\"writtenAt\":\"2026-10-01T00:00:00Z\"}");
            Assert.AreEqual(1, await _store.RetireOldScriptsAsync(_project));
            Assert.AreEqual(1, (await _store.GetDescriptionRunsAsync(_project)).Count);
        }
    }
}
