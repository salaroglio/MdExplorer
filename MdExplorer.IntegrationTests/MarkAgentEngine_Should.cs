using System;
using System.IO;
using MdExplorer.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Il motore di MarkAgent: una scelta sola, che di norma segue l'ambiente agentico del
    /// repository e si scollega solo se questa macchina lo dice.
    /// <para>Sprint: docs-internal/Sprints/2026-09-13-MarkAgent-Motore-OpenCode.md, fase F1.</para>
    /// </summary>
    [TestClass]
    public class MarkAgentEngine_Should
    {
        private string _root;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-markagent-engine", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
        }

        [TestCleanup]
        public void Cleanup()
        {
            try { if (Directory.Exists(_root)) Directory.Delete(_root, true); }
            catch (IOException) { }
        }

        /// <summary>Un <c>.development.yml</c> che dichiara solo l'harness: quanto basta a queste prove.</summary>
        private void DeclareHarness(string id)
            => File.WriteAllText(Path.Combine(_root, ".development.yml"), $"harness:\n  target: {id}\n");

        [TestMethod]
        public void Map_every_harness_to_the_engine_that_reads_it()
        {
            Assert.AreEqual(MarkAgentEngine.Copilot, MarkAgentEngines.FromHarness(HarnessTarget.Copilot));
            Assert.AreEqual(MarkAgentEngine.OpenCode, MarkAgentEngines.FromHarness(HarnessTarget.OpenCode));
            Assert.AreEqual(MarkAgentEngine.Claude, MarkAgentEngines.FromHarness(HarnessTarget.Claude));
            Assert.AreEqual(MarkAgentEngine.None, MarkAgentEngines.FromHarness(HarnessTarget.None));
        }

        [TestMethod]
        public void Round_trip_every_id()
        {
            foreach (var engine in new[] { MarkAgentEngine.Copilot, MarkAgentEngine.OpenCode, MarkAgentEngine.Claude, MarkAgentEngine.None })
            {
                var id = MarkAgentEngines.IdOf(engine);
                Assert.IsTrue(MarkAgentEngines.TryParseId(id, out var back), $"'{id}' non si rilegge");
                Assert.AreEqual(engine, back);
            }

            // Le maiuscole e gli spazi di un file scritto a mano non sono un errore.
            Assert.IsTrue(MarkAgentEngines.TryParseId("  Claude ", out var parsed));
            Assert.AreEqual(MarkAgentEngine.Claude, parsed);

            Assert.IsFalse(MarkAgentEngines.TryParseId("gemini", out _));
            Assert.IsFalse(MarkAgentEngines.TryParseId(null, out _));
        }

        [TestMethod]
        public void Follow_the_declared_harness_when_nothing_is_stored()
        {
            foreach (var (harnessId, expected) in new[]
            {
                ("copilot", MarkAgentEngine.Copilot),
                ("opencode", MarkAgentEngine.OpenCode),
                ("claude", MarkAgentEngine.Claude),
                ("none", MarkAgentEngine.None),
            })
            {
                DeclareHarness(harnessId);

                Assert.AreEqual(expected, MarkAgentEngines.Resolve(null, _root, out var linked),
                    $"harness {harnessId}");
                Assert.IsTrue(linked, "senza un valore salvato il motore SEGUE l'ambiente");

                // Vuoto e spazi valgono come "mai scelto": un campo svuotato a mano non deve
                // diventare un motore inesistente.
                Assert.AreEqual(expected, MarkAgentEngines.Resolve("   ", _root, out _));
            }
        }

        [TestMethod]
        public void Let_this_machine_override_the_harness()
        {
            DeclareHarness("copilot");

            var engine = MarkAgentEngines.Resolve("claude", _root, out var linked);

            Assert.AreEqual(MarkAgentEngine.Claude, engine, "il valore salvato vince sull'harness");
            Assert.IsFalse(linked, "un motore scelto a mano NON segue piu' l'ambiente");
        }

        [TestMethod]
        public void Refuse_an_engine_it_does_not_know()
        {
            DeclareHarness("claude");

            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => MarkAgentEngines.Resolve("gpt-5", _root, out _));

            // Nessun ripiego su un motore a caso: si dice che cosa c'e' scritto e che cosa si accetta.
            StringAssert.Contains(ex.Message, "gpt-5");
            StringAssert.Contains(ex.Message, MarkAgentEngines.AllowedIds);
        }

        [TestMethod]
        public void Fall_back_to_what_the_disk_shows_when_the_repository_never_declared_one()
        {
            // Progetto nato prima che l'impostazione esistesse: nessun .development.yml, ma la
            // cartella .github c'e'. Stessa coppia di letture dell'endpoint GetHarness.
            Directory.CreateDirectory(Path.Combine(_root, ".github"));

            Assert.AreEqual(HarnessTarget.Copilot, MarkAgentEngines.HarnessOf(_root));
            Assert.AreEqual(MarkAgentEngine.Copilot, MarkAgentEngines.Resolve(null, _root, out var linked));
            Assert.IsTrue(linked);
        }

        [TestMethod]
        public void Say_none_for_a_folder_with_no_harness_at_all()
        {
            Assert.AreEqual(MarkAgentEngine.None, MarkAgentEngines.Resolve(null, _root, out var linked));
            Assert.IsTrue(linked);
        }
    }
}
