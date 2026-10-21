using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using MdExplorer.RemoteProtocol;
using MdExplorer.RemoteAgent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.RemoteAgent
{
    /// <summary>Sprint Cartella-Remota-Via-SSH, F1: il ciclo JSON su stdin/stdout del serverino.</summary>
    [TestClass]
    public class AgentLoopShould
    {
        private string _root;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-loop-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
            File.WriteAllText(Path.Combine(_root, "a.txt"), "uno");
        }

        [TestCleanup]
        public void Cleanup() { if (Directory.Exists(_root)) Directory.Delete(_root, true); }

        private RemoteResponse[] Run(params string[] lines)
        {
            var output = new StringWriter();
            AgentLoop.Run(new RemoteWorkspace(_root), "1.2.3", new StringReader(string.Join("\n", lines) + "\n"), output);
            return output.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(l => JsonSerializer.Deserialize(l, RemoteJson.Default.RemoteResponse)).ToArray();
        }

        [TestMethod]
        public void Say_hello_first_then_answer_in_order()
        {
            var responses = Run(
                """{"id":1,"op":"read","path":"a.txt"}""",
                "",
                """{"id":2,"op":"list","path":""}""");
            Assert.AreEqual(3, responses.Length, "saluto + due risposte; le righe vuote si saltano");
            Assert.AreEqual(0, responses[0].Id);
            Assert.AreEqual(AgentProtocol.Version, responses[0].Hello.Protocol);
            Assert.AreEqual("1.2.3", responses[0].Hello.Version);
            Assert.AreEqual(Path.TrimEndingDirectorySeparator(Path.GetFullPath(_root)), responses[0].Hello.Root);
            Assert.AreEqual("uno", responses[1].Text);
            Assert.AreEqual("a.txt", responses[2].Entries.Single().Name);
        }

        [TestMethod]
        public void Answer_errors_with_a_code_and_keep_going()
        {
            var responses = Run(
                "non è json",
                """{"id":5,"op":"rm","path":"a.txt"}""",
                """{"id":6,"op":"read","path":"../x"}""",
                """{"id":7,"op":"read","path":"manca.txt"}""",
                """{"id":8,"op":"stat","path":"a.txt"}""");
            Assert.AreEqual(RemoteErrorCodes.BadRequest, responses[1].Error.Code);
            Assert.AreEqual(-1, responses[1].Id, "senza richiesta leggibile non c'è un id");
            Assert.AreEqual(RemoteErrorCodes.Unsupported, responses[2].Error.Code);
            Assert.AreEqual(5, responses[2].Id);
            Assert.AreEqual(RemoteErrorCodes.OutsideRoot, responses[3].Error.Code);
            Assert.AreEqual(RemoteErrorCodes.NotFound, responses[4].Error.Code);
            Assert.IsTrue(responses[5].Ok, "dopo gli errori il ciclo continua");
            Assert.IsNotNull(responses[5].Entry.Stamp);
        }

        [TestMethod]
        public void Write_the_json_in_camel_case_without_null_fields()
        {
            var output = new StringWriter();
            AgentLoop.Run(new RemoteWorkspace(_root), "v", new StringReader("""{"id":1,"op":"mkdir","path":"d"}""" + "\n"), output);
            var lines = output.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual("""{"id":1,"ok":true}""", lines[1]);
        }
    }
}
