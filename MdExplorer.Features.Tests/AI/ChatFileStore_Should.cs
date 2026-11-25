using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MdExplorer.Features.Services.AI.ChatHistory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.AI
{
    /// <summary>
    /// Il file di una chat di MarkAgent (sprint 2026-11-24-Chat-Memorizzate, F1): tutto quello che torna dal motore, intero,
    /// una riga per evento; e quando qualcosa non si legge lo dice, non lo salta.
    /// </summary>
    [TestClass]
    public class ChatFileStore_Should
    {
        private string _root;
        private ChatFileStore _store;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "chatstore_" + Guid.NewGuid().ToString("N"));
            _store = new ChatFileStore(_root);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
        }

        private static ChatRecordEvent Event(long seq, string kind, string content, string callId = null) => new()
        {
            Seq = seq, Turn = 1, At = new DateTime(2026, 11, 25, 8, 0, 0, DateTimeKind.Utc), Kind = kind,
            Engine = "claude", Content = content, CallId = callId,
        };

        [TestMethod]
        public void Keep_every_event_whole_and_in_order()
        {
            var path = _store.RelativePathFor("/home/acme/progetto", Guid.NewGuid());
            // Un risultato di strumento grosso e con a capo, virgolette e lettere accentate: deve tornare identico.
            var big = string.Join("\n", Enumerable.Range(0, 20_000).Select(i => $"riga {i} \"citata\" — è così"));
            using var args = JsonDocument.Parse("{\"file_path\":\"note.md\"}");

            _store.Append(path, new[] { Event(1, ChatRecordEvent.KindUserPrompt, "Leggi note.md") });
            _store.Append(path, new[]
            {
                new ChatRecordEvent { Seq = 2, Turn = 1, Kind = ChatRecordEvent.KindToolCall, Tool = "Read", CallId = "toolu_1", Json = args.RootElement.Clone() },
                Event(3, ChatRecordEvent.KindToolResult, big, "toolu_1"),
                Event(4, ChatRecordEvent.KindText, "Fatto."),
            });

            var read = _store.Read(path);

            Assert.IsTrue(read.Exists);
            Assert.AreEqual(0, read.Problems.Count, string.Join("; ", read.Problems));
            CollectionAssert.AreEqual(new long[] { 1, 2, 3, 4 }, read.Events.Select(e => e.Seq).ToArray());
            Assert.AreEqual(big, read.Events[2].Content, "il contenuto non si taglia né si altera");
            Assert.AreEqual("toolu_1", read.Events[2].CallId);
            Assert.AreEqual("note.md", read.Events[1].Json.Value.GetProperty("file_path").GetString());
            Assert.AreEqual(4, File.ReadAllText(Path.Combine(_root, path)).Count(c => c == '\n'), "una riga per evento");
        }

        [TestMethod]
        public void Say_when_the_file_is_missing()
        {
            var read = _store.Read(_store.RelativePathFor("/home/acme/progetto", Guid.NewGuid()));

            Assert.IsFalse(read.Exists);
            Assert.AreEqual(0, read.Events.Count);
        }

        [TestMethod]
        public void Report_an_interrupted_last_line_instead_of_skipping_it()
        {
            var path = _store.RelativePathFor("/home/acme/progetto", Guid.NewGuid());
            _store.Append(path, new[] { Event(1, ChatRecordEvent.KindUserPrompt, "ciao") });
            // Un crash a metà scrittura: l'ultima riga resta senza chiusura.
            File.AppendAllText(Path.Combine(_root, path), "{\"seq\":2,\"kind\":\"text\",\"content\":\"a me", new UTF8Encoding(false));

            var read = _store.Read(path);

            Assert.AreEqual(1, read.Events.Count, "la parte sana si legge");
            Assert.AreEqual(1, read.Problems.Count);
            StringAssert.Contains(read.Problems[0], "riga 2");
            StringAssert.Contains(read.Problems[0], "incompleta");
        }

        [TestMethod]
        public void Report_a_broken_line_in_the_middle()
        {
            var path = _store.RelativePathFor("/home/acme/progetto", Guid.NewGuid());
            _store.Append(path, new[] { Event(1, ChatRecordEvent.KindUserPrompt, "uno") });
            File.AppendAllText(Path.Combine(_root, path), "non json\n", new UTF8Encoding(false));
            _store.Append(path, new[] { Event(3, ChatRecordEvent.KindText, "tre") });

            var read = _store.Read(path);

            CollectionAssert.AreEqual(new long[] { 1, 3 }, read.Events.Select(e => e.Seq).ToArray());
            Assert.AreEqual(1, read.Problems.Count);
            StringAssert.Contains(read.Problems[0], "riga 2");
        }

        [TestMethod]
        public void Not_mix_lines_written_at_the_same_time()
        {
            var path = _store.RelativePathFor("/home/acme/progetto", Guid.NewGuid());
            var text = new string('x', 50_000);

            Parallel.For(1, 101, i => _store.Append(path, new[] { Event(i, ChatRecordEvent.KindText, text) }));

            var read = _store.Read(path);
            Assert.AreEqual(0, read.Problems.Count, string.Join("; ", read.Problems));
            Assert.AreEqual(100, read.Events.Count);
            CollectionAssert.AreEquivalent(Enumerable.Range(1, 100).Select(i => (long)i).ToArray(), read.Events.Select(e => e.Seq).ToArray());
        }

        [TestMethod]
        public void Delete_the_file_and_tell_if_it_was_there()
        {
            var path = _store.RelativePathFor("/home/acme/progetto", Guid.NewGuid());
            _store.Append(path, new[] { Event(1, ChatRecordEvent.KindUserPrompt, "ciao") });

            Assert.IsTrue(_store.Delete(path));
            Assert.IsFalse(_store.Read(path).Exists);
            Assert.IsFalse(_store.Delete(path));
        }

        [TestMethod]
        public void Keep_projects_apart_even_when_they_differ_only_in_accented_letters()
        {
            var id = Guid.NewGuid();
            var a = _store.RelativePathFor("/home/acme/Università", id);
            var b = _store.RelativePathFor("/home/acme/Universitè", id);

            Assert.AreNotEqual(a.Split('/')[0], b.Split('/')[0]);
            StringAssert.EndsWith(a, "/" + id.ToString("D") + ".jsonl");
        }

        [TestMethod]
        public void Refuse_a_path_that_leaves_the_chats_folder()
        {
            Assert.ThrowsExactly<ArgumentException>(() => _store.Read("../fuori.jsonl"));
            Assert.ThrowsExactly<ArgumentException>(() => _store.Read(Path.Combine(Path.GetTempPath(), "x.jsonl")));
            Assert.ThrowsExactly<ArgumentException>(() => _store.Append("a/../../fuori.jsonl", new[] { Event(1, ChatRecordEvent.KindText, "x") }));
        }
    }
}
