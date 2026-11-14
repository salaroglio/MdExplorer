using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MdExplorer.Features.Commands;
using MdExplorer.Features.Diagrams;
using MdExplorer.Features.Services.AI.ClaudeCode;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// C4 della valutazione del 07/11, con Claude Code vero: Claude scrive un file con un diagramma rotto, MdExplorer lo
    /// verifica a fine turno, glielo rimanda, e lui lo corregge nello stesso flusso. Il jar è finto (rifiuta la parola
    /// «sbagliato»), così il test non dipende da Java né dal giudizio del modello su cosa sia un errore.
    /// </summary>
    [TestClass]
    public class ClaudeDiagramCorrection_Should
    {
        private string _dir;

        [TestInitialize]
        public void Setup()
        {
            if (!ClaudeCodeProcessLauncher.IsResolvable())
                Assert.Inconclusive("Claude Code CLI not installed on this machine.");
            _dir = Path.Combine(Path.GetTempPath(), "mde-claude-diagrammi", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(_dir, "docs"));
        }

        [TestCleanup]
        public void Cleanup()
        {
            try { if (_dir != null && Directory.Exists(_dir)) Directory.Delete(_dir, true); }
            catch (IOException) { }
        }

        [TestMethod]
        public async Task Hand_the_broken_diagram_back_to_claude_who_fixes_it()
        {
            var checkedSources = new List<string>();
            var verifier = new PlantumlBlockVerifier(source =>
            {
                lock (checkedSources) checkedSources.Add(source);
                return Task.FromResult(source.Contains("sbagliato")
                    ? PlantumlCheckOutcome.FromProcess(200, "ERROR\n3\nSyntax Error? La parola sbagliato non è PlantUML: toglila.")
                    : PlantumlCheckOutcome.FromProcess(0, ""));
            });
            // Come la chat di Mark (ClaudeCodeMcp.ChatOptions): Edit e Write permessi nel progetto, niente MCP.
            var options = new ClaudeCodeSessionOptions { AllowedTools = new[] { "Edit(./**)", "Write(./**)" } };
            await using var session = new ClaudeCodeSession(NullLogger<ClaudeCodeSession>.Instance, _dir, "haiku", options) { DiagramVerifier = verifier };
            await session.StartAsync();

            var tools = new List<string>();
            await foreach (var chunk in session.PromptAsync(
                "Crea il file docs/prova.md con esattamente questo contenuto e poi rispondi solo «fatto»:\n\n" +
                "# Prova\n\n```plantuml\n@startuml\n!theme plain\nsbagliato\nA -> B\n@enduml\n```\n"))
                tools.Add(chunk.Kind + ": " + chunk.Text);

            var log = string.Join("\n", tools);
            var path = Path.Combine(_dir, "docs", "prova.md");
            Assert.IsTrue(File.Exists(path), "Claude non ha scritto il file:\n" + log);
            var file = File.ReadAllText(path);
            Assert.IsTrue(log.Contains("rimandati a Claude") || log.Contains("handed back to Claude"), "MdExplorer non ha rimandato il diagramma:\n" + log);
            Assert.IsFalse(file.Contains("sbagliato"), "Claude non l'ha corretto:\n" + file + "\n\n" + log);
            StringAssert.Contains(file, "A -> B");
            Assert.IsTrue(log.Contains("verificati ✓") || log.Contains("checked ✓"), "dopo la correzione la verifica deve dirlo:\n" + log);
        }
    }
}
