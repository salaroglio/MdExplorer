using System.IO;
using System.Linq;
using System.Text.Json;
using MdExplorer.Features.Services.AI.CopilotSdk;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    /// <summary>
    /// Which markdown files a Copilot tool call wrote. The argument shapes are the ones measured
    /// with the real CLI, 11/09/2026.
    /// </summary>
    [TestClass]
    public class AgentWrittenMarkdown_Should
    {
        private string _project;

        [TestInitialize]
        public void Setup()
        {
            _project = Path.Combine(Path.GetTempPath(), "mde-written-" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(_project, "docs"));
            File.WriteAllText(Path.Combine(_project, "note.md"), "# n");
            File.WriteAllText(Path.Combine(_project, "docs", "ordini.md"), "# o");
            File.WriteAllText(Path.Combine(_project, "dati.json"), "{}");
        }

        [TestCleanup]
        public void Cleanup() => Directory.Delete(_project, true);

        private string[] Paths(string json)
            => AgentWrittenMarkdown.Paths(JsonDocument.Parse(json).RootElement, _project)
                .Select(p => Path.GetRelativePath(_project, p).Replace('\\', '/')).ToArray();

        private string Json(object o) => JsonSerializer.Serialize(o);

        [TestMethod]
        public void ReadCreateAndEdit()
        {
            CollectionAssert.AreEqual(new[] { "note.md" },
                Paths(Json(new { path = Path.Combine(_project, "note.md"), file_text = "# Note\n```plantuml\n@startuml\n@enduml\n```" })));
            CollectionAssert.AreEqual(new[] { "docs/ordini.md" },
                Paths(Json(new { path = "docs/ordini.md", old_str = "a", new_str = "b" })), "a relative path");
        }

        [TestMethod]
        public void ReadThePathsInsideAPatch()
        {
            var patch = "*** Begin Patch\n*** Add File: note.md\n+# Note\n*** Update File: docs/ordini.md\n@@\n-a\n+b\n*** Update File: dati.json\n*** End Patch";
            CollectionAssert.AreEquivalent(new[] { "note.md", "docs/ordini.md" }, Paths(Json(new { input = patch })));
        }

        [TestMethod]
        public void IgnoreWhatIsNotAMarkdownFileOfTheProject()
        {
            Assert.AreEqual(0, Paths(Json(new { path = Path.Combine(_project, "dati.json") })).Length, "not markdown");
            Assert.AreEqual(0, Paths(Json(new { path = "manca.md" })).Length, "does not exist");
            Assert.AreEqual(0, Paths(Json(new { path = "../fuori.md" })).Length, "outside the project");
            Assert.AreEqual(0, Paths(Json(new { command = "ls -la" })).Length, "a shell command");
            Assert.AreEqual(0, AgentWrittenMarkdown.Paths(null, _project).Count, "no arguments");
        }

        [TestMethod]
        public void NameAFileOnce()
        {
            CollectionAssert.AreEqual(new[] { "note.md" },
                Paths(Json(new { path = "note.md", also = new[] { Path.Combine(_project, "note.md") } })));
        }
    }
}
