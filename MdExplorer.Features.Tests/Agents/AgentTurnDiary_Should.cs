using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using MdExplorer.Features.Agents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    /// <summary>
    /// Il diario di un turno: ciò che un agente scrive, pensa e usa mentre lavora, per chi apre il pannello dall'indicatore
    /// «agenti al lavoro»; a turno finito resta su disco.
    /// </summary>
    [TestClass]
    public class AgentTurnDiary_Should
    {
        private string _folder;

        [TestInitialize]
        public void Setup() => _folder = Path.Combine(Path.GetTempPath(), "mde-diario-" + Guid.NewGuid().ToString("N"));

        [TestCleanup]
        public void Cleanup() { try { if (Directory.Exists(_folder)) Directory.Delete(_folder, true); } catch (IOException) { } }

        private static AgentActivity Turn(string agent = "riassunto", string project = "/progetti/acme", DateTime? started = null) => new()
        {
            Id = Guid.NewGuid(), AgentName = agent, ProjectPath = project, Engine = "Claude Code (sonnet)", StartedAtUtc = started ?? DateTime.UtcNow,
        };

        [TestMethod]
        public void Keep_an_answer_that_arrives_in_pieces_as_one_entry_that_grows()
        {
            var diary = new AgentTurnDiary(null); var turn = Turn(); diary.Open(turn);

            diary.Append(turn.Id, AgentTurnEvent.KindThinking, "Devo leggere ");
            diary.Append(turn.Id, AgentTurnEvent.KindThinking, "il file.");
            diary.Append(turn.Id, AgentTurnEvent.KindTool, "Read · docs/analisi.md");
            diary.Append(turn.Id, AgentTurnEvent.KindTool, "↳ ok");
            diary.Append(turn.Id, AgentTurnEvent.KindMessage, "Ho contato ");
            diary.Append(turn.Id, AgentTurnEvent.KindMessage, "3 righe.");
            diary.Append(turn.Id, AgentTurnEvent.KindMessage, "");   // niente testo, niente voce

            var view = diary.Read(turn.Id, 0);
            CollectionAssert.AreEqual(
                new[] { "thinking|Devo leggere il file.", "tool|Read · docs/analisi.md", "tool|↳ ok", "message|Ho contato 3 righe." },
                view.Entries.Select(e => e.Kind + "|" + e.Text).ToArray());
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, view.Entries.Select(e => e.Seq).ToArray());
            Assert.IsNull(view.FinishedAtUtc, "il turno è in corso");
            Assert.AreEqual("riassunto", view.AgentName);
        }

        [TestMethod]
        public void Give_only_what_changed_after_the_version_the_reader_already_has()
        {
            var diary = new AgentTurnDiary(null); var turn = Turn(); diary.Open(turn);
            diary.Append(turn.Id, AgentTurnEvent.KindTool, "Read · a.md");
            diary.Append(turn.Id, AgentTurnEvent.KindMessage, "Prima parte");
            var seen = diary.Read(turn.Id, 0).Version;

            Assert.AreEqual(0, diary.Read(turn.Id, seen).Entries.Count, "niente di nuovo");

            diary.Append(turn.Id, AgentTurnEvent.KindMessage, ", seconda parte");
            var later = diary.Read(turn.Id, seen);
            // La risposta è cresciuta: torna la stessa voce, intera. Lo strumento di prima non torna.
            Assert.AreEqual("message|2|Prima parte, seconda parte", string.Join(";", later.Entries.Select(e => $"{e.Kind}|{e.Seq}|{e.Text}")));
            Assert.IsTrue(later.Version > seen);
        }

        [TestMethod]
        public void Close_with_the_outcome_and_stay_readable_from_disk_in_a_new_process()
        {
            var diary = new AgentTurnDiary(_folder); var turn = Turn(); diary.Open(turn);
            diary.Append(turn.Id, AgentTurnEvent.KindMessage, "Fatto: àèìòù €");
            diary.Close(turn.Id, "Completed", null, "Fatto: àèìòù €");
            diary.Append(turn.Id, AgentTurnEvent.KindMessage, " (dopo la fine non si scrive)");

            // Un altro processo (MDE riavviato): legge dal file.
            var reopened = new AgentTurnDiary(_folder).Read(turn.Id, 0);
            Assert.AreEqual("Completed", reopened.Outcome);
            Assert.IsNotNull(reopened.FinishedAtUtc);
            Assert.AreEqual("Fatto: àèìòù €", reopened.Entries.Single().Text, "la risposta arrivata a pezzi non viene raddoppiata dal testo finale");
            Assert.IsNull(new AgentTurnDiary(_folder).Read(Guid.NewGuid(), 0), "un turno mai esistito");
        }

        [TestMethod]
        public void Put_the_whole_answer_at_the_end_when_the_engine_did_not_send_it_in_pieces()
        {
            var diary = new AgentTurnDiary(null); var turn = Turn(); diary.Open(turn);
            diary.Append(turn.Id, AgentTurnEvent.KindTool, "bash · wc -l nota.txt");

            diary.Close(turn.Id, "Completed", null, "Ho contato 3 righe.");

            Assert.AreEqual("tool|bash · wc -l nota.txt;message|Ho contato 3 righe.",
                string.Join(";", diary.Read(turn.Id, 0).Entries.Select(e => e.Kind + "|" + e.Text)));
        }

        [TestMethod]
        public void Keep_the_reason_of_a_failed_turn()
        {
            var diary = new AgentTurnDiary(_folder); var turn = Turn(); diary.Open(turn);
            diary.Append(turn.Id, AgentTurnEvent.KindError, "Execution failed: Model \"x\" is not available.");
            diary.Close(turn.Id, "ProviderError", "Il modello 'x' non è disponibile.", null);

            var view = new AgentTurnDiary(_folder).Read(turn.Id, 0);
            Assert.AreEqual("ProviderError", view.Outcome);
            Assert.AreEqual("Il modello 'x' non è disponibile.", view.Diagnostic);
            Assert.AreEqual(AgentTurnEvent.KindError, view.Entries.Single().Kind);
        }

        [TestMethod]
        public void List_the_turns_of_a_project_newest_first_running_and_finished_without_their_entries()
        {
            var diary = new AgentTurnDiary(_folder);
            var old = Turn("vecchio", started: DateTime.UtcNow.AddHours(-2)); diary.Open(old); diary.Append(old.Id, AgentTurnEvent.KindMessage, "x"); diary.Close(old.Id, "Completed", null, null);
            var other = Turn("altrove", project: "/progetti/altro"); diary.Open(other); diary.Close(other.Id, "Completed", null, null);
            var running = Turn("al-lavoro", project: "/progetti/ACME/"); diary.Open(running);   // lo stesso progetto, scritto in un altro modo

            var list = new AgentTurnDiary(_folder).Recent("/progetti/acme", 10);
            Assert.AreEqual("vecchio", string.Join(",", list.Select(v => v.AgentName)), "un altro processo vede i finiti, non quelli in corso di questo");

            list = diary.Recent("/progetti/acme", 10);
            Assert.AreEqual("al-lavoro,vecchio", string.Join(",", list.Select(v => v.AgentName)));
            Assert.IsTrue(list.All(v => v.Entries.Count == 0), "solo le testate");
            Assert.IsNull(list[0].FinishedAtUtc); Assert.IsNotNull(list[1].FinishedAtUtc);
            Assert.AreEqual(1, diary.Recent("/progetti/acme", 1).Count);
        }

        [TestMethod]
        public void Cut_a_long_tool_line_and_stop_recording_a_turn_that_writes_too_much_saying_so()
        {
            var diary = new AgentTurnDiary(null); var turn = Turn(); diary.Open(turn);
            diary.Append(turn.Id, AgentTurnEvent.KindTool, "Write · " + new string('k', 5000));
            Assert.AreEqual(AgentTurnDiary.MaxToolChars + 1, diary.Read(turn.Id, 0).Entries[0].Text.Length, "tagliata, con «…» in fondo");

            diary.Append(turn.Id, AgentTurnEvent.KindMessage, new string('m', AgentTurnDiary.MaxChars));
            diary.Append(turn.Id, AgentTurnEvent.KindMessage, "questo non entra");
            diary.Append(turn.Id, AgentTurnEvent.KindError, "un errore entra lo stesso");

            var view = diary.Read(turn.Id, 0);
            Assert.IsTrue(view.Truncated);
            Assert.IsFalse(view.Entries.Any(e => e.Text.Contains("questo non entra")));
            Assert.AreEqual("un errore entra lo stesso", view.Entries.Last().Text);
        }

        [TestMethod]
        public void Tell_who_listens_at_every_change_and_keep_only_the_newest_turns_on_disk()
        {
            var diary = new AgentTurnDiary(_folder); var changes = 0; diary.Changed += (_, _) => changes++;
            var first = Turn("primo", started: DateTime.UtcNow.AddDays(-1)); diary.Open(first); diary.Append(first.Id, AgentTurnEvent.KindMessage, "a"); diary.Close(first.Id, "Completed", null, null);
            Assert.AreEqual(3, changes, "aperto, scritto, chiuso");

            for (var i = 0; i < AgentTurnDiary.MaxKept; i++) { var t = Turn("t" + i); diary.Open(t); diary.Close(t.Id, "Completed", null, null); }

            Assert.IsNull(new AgentTurnDiary(_folder).Read(first.Id, 0), "il più vecchio è uscito");
            Assert.AreEqual(AgentTurnDiary.MaxKept + 1, Directory.GetFiles(_folder, "*.json").Length, "i diari tenuti più l'indice");
        }

        [TestMethod]
        public void Start_from_an_empty_list_when_the_index_on_disk_is_broken()
        {
            Directory.CreateDirectory(_folder); File.WriteAllText(Path.Combine(_folder, "indice.json"), "{rotto");
            var diary = new AgentTurnDiary(_folder); var turn = Turn(); diary.Open(turn);

            diary.Close(turn.Id, "Completed", null, "ok");

            Assert.AreEqual(1, new AgentTurnDiary(_folder).Recent("/progetti/acme", 10).Count, "l'indice è stato riscritto");
        }
    }

    [TestClass]
    public class AgentToolLine_Should
    {
        private static JsonElement Args(string json) => JsonDocument.Parse(json).RootElement.Clone();

        [TestMethod]
        public void Say_the_command_or_the_file_a_tool_works_on()
        {
            Assert.AreEqual("bash · wc -l < nota.txt", AgentToolLine.Describe("bash", Args("{\"command\":\"wc -l < nota.txt\",\"description\":\"Count lines\"}")));
            Assert.AreEqual("Read · docs/analisi.md", AgentToolLine.Describe("Read", Args("{\"file_path\":\"docs/analisi.md\"}")));
            Assert.AreEqual("grep · TODO", AgentToolLine.Describe("grep", Args("{\"pattern\":\"TODO\",\"glob\":\"*.cs\"}")));
            Assert.AreEqual("tool", AgentToolLine.Describe(null, default));
            Assert.AreEqual("status", AgentToolLine.Describe("status", Args("{}")));
        }

        [TestMethod]
        public void Never_carry_the_content_a_tool_writes()
        {
            var content = new string('x', 5000);
            // Scrivere un file: nel diario va QUALE file, non il testo.
            Assert.AreEqual("Write · docs/riassunto.md", AgentToolLine.Describe("Write", Args("{\"file_path\":\"docs/riassunto.md\",\"content\":\"" + content + "\"}")));
            // Uno strumento senza un argomento «su che cosa»: ogni argomento, tagliato.
            var line = AgentToolLine.Describe("mcp_salva", Args("{\"titolo\":\"Nota\",\"testo\":\"" + content + "\",\"n\":3}"));
            Assert.AreEqual("mcp_salva · titolo=Nota, testo=" + new string('x', AgentToolLine.MaxArgumentChars) + "…, n=3", line);
        }

        [TestMethod]
        public void Read_the_name_and_json_form_and_leave_any_other_text_as_it_is()
        {
            Assert.AreEqual("Bash · ls -la", AgentToolLine.FromNameAndJson("Bash {\"command\":\"ls -la\",\"description\":\"List\"}"));
            Assert.AreEqual("↳ ok", AgentToolLine.FromNameAndJson("↳ ok"));
            Assert.AreEqual("glob · pending", AgentToolLine.FromNameAndJson("glob · pending"));
            Assert.AreEqual("Nome {non json", AgentToolLine.FromNameAndJson("Nome {non json"));
            Assert.IsNull(AgentToolLine.FromNameAndJson(null));
        }

        [TestMethod]
        public void Find_what_the_tool_works_on_even_when_its_arguments_arrive_cut()
        {
            // Le righe vere della sessione di Claude Code del 22/11/2026: gli argomenti accorciati a 160 caratteri, «…» in fondo.
            Assert.AreEqual("Write · /progetti/acme/docs/esito.txt",
                AgentToolLine.FromNameAndJson("Write {\"file_path\":\"/progetti/acme/docs/esito.txt\",\"cont…"));
            Assert.AreEqual("Bash · wc -l < /progetti/acme/docs/nota.txt",
                AgentToolLine.FromNameAndJson("Bash {\"command\":\"wc -l < /progetti/acme/docs/nota.txt\",\"…"));
            // Tagliato dentro l'argomento stesso: quello che c'è, con «…».
            Assert.AreEqual("Bash · grep -c \"TODO\" src/un/percorso/molto/lu…",
                AgentToolLine.FromNameAndJson("Bash {\"command\":\"grep -c \\\"TODO\\\" src/un/percorso/molto/lu"));
            // Nessun argomento riconoscibile: la riga resta com'è.
            Assert.AreEqual("mcp_salva {\"titolo\":\"Nota\",\"tes…", AgentToolLine.FromNameAndJson("mcp_salva {\"titolo\":\"Nota\",\"tes…"));
        }
    }
}
