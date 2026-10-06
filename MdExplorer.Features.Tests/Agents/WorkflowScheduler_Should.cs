using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MdExplorer.Features.Agents.Workflow;
using MdExplorer.Features.Agents.Workflow.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    /// <summary>
    /// Lo schedulatore distribuito (S2): una funzione pura che, dati workflow, giro e «quali agenti sono miei», dice che
    /// cosa fare. Gli scenari seguono un giro della gara; i «computer» sono chiamate con un <c>isMine</c> diverso: ognuno
    /// decide solo per i propri agenti, leggendo gli stessi fatti.
    /// </summary>
    [TestClass]
    public class WorkflowScheduler_Should
    {
        private const string Gara = @"{
  ""mde_workflow"": 2, ""title"": ""Gara"",
  ""variables"": { ""codice"": ""il bando"" },
  ""steps"": [
    { ""id"": ""ricerca"", ""agent"": ""account-manager"", ""trigger"": { ""launch"": true }, ""start"": ""manual"",
      ""produces"": [""gara/ricerche/ricerca-*.md""] },
    { ""id"": ""tecnica"", ""agent"": ""responsabile-tecnico"", ""trigger"": { ""reply"": ""avvia-giro"", ""to"": ""ricerca"" }, ""start"": ""ask-owner"",
      ""brief"": ""Scrivi la scheda tecnica sul bando {codice}."", ""produces"": [""gara/schede/tecnica.md""] },
    { ""id"": ""contratto"", ""agent"": ""responsabile-legale"", ""trigger"": { ""reply"": ""avvia-giro"", ""to"": ""ricerca"" }, ""start"": ""ask-owner"",
      ""brief"": ""Scrivi la scheda contrattuale sul bando {codice}."", ""produces"": [""gara/schede/contrattuale.md""] },
    { ""id"": ""delivery"", ""agent"": ""responsabile-delivery"", ""trigger"": { ""reply"": ""avvia-giro"", ""to"": ""ricerca"" }, ""start"": ""ask-owner"",
      ""brief"": ""Scrivi la scheda di delivery sul bando {codice}."", ""produces"": [""gara/schede/delivery.md""] },
    { ""id"": ""sintesi"", ""agent"": ""account-manager"", ""trigger"": { ""after"": [""tecnica"", ""contratto"", ""delivery""] }, ""start"": ""auto"",
      ""brief"": ""Scrivi la sintesi sul bando {codice}."", ""produces"": [""gara/schede/sintesi.md""] }
  ],
  ""loops"": [ { ""id"": ""rifacimento"", ""steps"": [""tecnica"", ""contratto"", ""delivery""], ""until"": ""approved"", ""restart"": ""manual"" } ]
}";

        private static readonly DateTime T0 = new(2026, 10, 6, 9, 0, 0, DateTimeKind.Utc);
        private int _tick;

        private static WorkflowDescriptor Parse(string json)
        {
            var r = WorkflowParser.Parse(json);
            Assert.IsTrue(r.IsValid, string.Join("\n", r.Issues));
            return r.Descriptor;
        }

        private static Func<string, bool> Pc(string agent) => a => a == agent;
        private static readonly Func<string, bool> Everyone = _ => true;

        private RoundState NewRound() => new() { Header = new RoundHeader { Id = "2026-10-06-gara-abc123", Workflow = "gara/gara.workflow.json", StartedAt = T0 } };

        private void Add(RoundState round, string step, string agent, string type, int roundNo = 1, int attempt = 1,
            string note = null, string reply = null, Dictionary<string, string> values = null, List<string> files = null)
        {
            if (!round.Steps.TryGetValue(step, out var record))
                round.Steps[step] = record = new StepRecord { Step = step, Agent = agent };
            record.Events.Add(new RoundEvent
            {
                Type = type, At = T0.AddMinutes(++_tick), By = "io@test.local", Round = roundNo, Attempt = attempt,
                Note = note, Reply = reply, Values = values, Files = files,
            });
        }

        /// <summary>La ricerca fatta e approvata, e la persona che preme «Avvia il giro» sul bando NC-1.</summary>
        private RoundState RoundStarted()
        {
            var round = NewRound();
            Add(round, "ricerca", "account-manager", RoundEventType.Started);
            Add(round, "ricerca", "account-manager", RoundEventType.Delivered, files: new() { "gara/ricerche/ricerca-2027-03-08.md" });
            Add(round, "ricerca", "account-manager", RoundEventType.Approved);
            Add(round, "ricerca", "account-manager", RoundEventType.Replied, reply: "avvia-giro", values: new() { ["codice"] = "NC-1" });
            return round;
        }

        private void Approve(RoundState round, string step, string agent, string file, int roundNo = 1, int attempt = 1)
        {
            if (attempt == 1) Add(round, step, agent, RoundEventType.Held, roundNo, attempt);
            Add(round, step, agent, RoundEventType.Started, roundNo, attempt);
            Add(round, step, agent, RoundEventType.Delivered, roundNo, attempt, files: new() { file });
            Add(round, step, agent, RoundEventType.Approved, roundNo, attempt);
        }

        [TestMethod]
        public void Wait_for_the_persons_choice_before_starting_anything()
        {
            var wf = Parse(Gara);
            var round = NewRound();
            Add(round, "ricerca", "account-manager", RoundEventType.Started);
            Add(round, "ricerca", "account-manager", RoundEventType.Delivered, files: new() { "gara/ricerche/r.md" });
            Add(round, "ricerca", "account-manager", RoundEventType.Approved);

            Assert.AreEqual(0, WorkflowScheduler.Decide(wf, round, Everyone).Count, "senza il pulsante non parte niente");
            Assert.AreEqual(StepStatus.Finished, WorkflowScheduler.View(wf, round, wf.Step("ricerca")).Status);
        }

        [TestMethod]
        public void Let_each_computer_decide_only_for_its_own_agents()
        {
            var wf = Parse(Gara);
            var round = RoundStarted();

            var onTechnical = WorkflowScheduler.Decide(wf, round, Pc("responsabile-tecnico"));
            Assert.AreEqual(1, onTechnical.Count);
            Assert.AreEqual(SchedulerActionKind.Hold, onTechnical[0].Kind, "ask-owner: aspetta il responsabile");
            Assert.AreEqual("tecnica", onTechnical[0].Step.Id);
            Assert.AreEqual("Scrivi la scheda tecnica sul bando NC-1.", onTechnical[0].Brief, "il valore del pulsante arriva nel brief");

            Assert.AreEqual(0, WorkflowScheduler.Decide(wf, round, Pc("account-manager")).Count, "il computer dell'account manager non decide per il tecnico");
            Assert.AreEqual(3, WorkflowScheduler.Decide(wf, round, Everyone).Count, "un computer di cui sono tutti: tre «da avviare»");
        }

        [TestMethod]
        public void Repeat_the_same_answer_until_the_action_is_recorded()
        {
            var wf = Parse(Gara);
            var round = RoundStarted();

            var first = WorkflowScheduler.Decide(wf, round, Pc("responsabile-tecnico"));
            var again = WorkflowScheduler.Decide(wf, round, Pc("responsabile-tecnico"));
            Assert.AreEqual(first.Single().ToString(), again.Single().ToString(), "funzione pura: stesso stato, stessa azione");

            Add(round, "tecnica", "responsabile-tecnico", RoundEventType.Held);
            Assert.AreEqual(0, WorkflowScheduler.Decide(wf, round, Pc("responsabile-tecnico")).Count, "registrato «da avviare»: non si ripete");
            Assert.AreEqual(StepStatus.Held, WorkflowScheduler.View(wf, round, wf.Step("tecnica")).Status);
        }

        [TestMethod]
        public void Start_the_summary_only_when_all_three_sheets_are_approved_with_their_files()
        {
            var wf = Parse(Gara);
            var round = RoundStarted();
            Approve(round, "tecnica", "responsabile-tecnico", "gara/schede/tecnica.md");
            Approve(round, "contratto", "responsabile-legale", "gara/schede/contrattuale.md");

            Assert.AreEqual(0, WorkflowScheduler.Decide(wf, round, Pc("account-manager")).Count, "due su tre: la sintesi aspetta");

            Add(round, "delivery", "responsabile-delivery", RoundEventType.Held);
            Add(round, "delivery", "responsabile-delivery", RoundEventType.Started);
            Add(round, "delivery", "responsabile-delivery", RoundEventType.Delivered, files: new() { "gara/schede/delivery.md" });
            Assert.AreEqual(0, WorkflowScheduler.Decide(wf, round, Pc("account-manager")).Count, "consegnata non è approvata");

            Add(round, "delivery", "responsabile-delivery", RoundEventType.Approved);
            var summary = WorkflowScheduler.Decide(wf, round, Pc("account-manager")).Single();
            Assert.AreEqual(SchedulerActionKind.Start, summary.Kind, "auto: parte da sola");
            Assert.AreEqual("Scrivi la sintesi sul bando NC-1.", summary.Brief);
            CollectionAssert.AreEquivalent(new[] { "gara/schede/tecnica.md", "gara/schede/contrattuale.md", "gara/schede/delivery.md" },
                summary.Inputs.ToList(), "riceve i file approvati dei passi da cui dipende");
        }

        [TestMethod]
        public void Leave_a_rejected_sheet_to_the_person_when_the_loop_restarts_manually()
        {
            var wf = Parse(Gara);
            var round = RoundStarted();
            Add(round, "tecnica", "responsabile-tecnico", RoundEventType.Held);
            Add(round, "tecnica", "responsabile-tecnico", RoundEventType.Started);
            Add(round, "tecnica", "responsabile-tecnico", RoundEventType.Delivered, files: new() { "gara/schede/tecnica.md" });
            Add(round, "tecnica", "responsabile-tecnico", RoundEventType.Rejected, note: "manca il RPO");

            Assert.AreEqual(StepStatus.Rejected, WorkflowScheduler.View(wf, round, wf.Step("tecnica")).Status);
            Assert.AreEqual(0, WorkflowScheduler.Decide(wf, round, Pc("responsabile-tecnico")).Count, "restart: manual → aspetta «Fai ripartire»");
        }

        [TestMethod]
        public void Restart_a_rejected_sheet_by_itself_up_to_the_max_when_the_author_says_so()
        {
            var wf = Parse(Gara.Replace(@"""until"": ""approved"", ""restart"": ""manual""", @"""until"": ""approved"", ""restart"": ""auto"", ""max"": 1"));
            var round = RoundStarted();
            Add(round, "tecnica", "responsabile-tecnico", RoundEventType.Held);
            Add(round, "tecnica", "responsabile-tecnico", RoundEventType.Started);
            Add(round, "tecnica", "responsabile-tecnico", RoundEventType.Delivered, files: new() { "gara/schede/tecnica.md" });
            Add(round, "tecnica", "responsabile-tecnico", RoundEventType.Rejected, note: "manca il RPO");

            var rework = WorkflowScheduler.Decide(wf, round, Pc("responsabile-tecnico")).Single();
            Assert.AreEqual(SchedulerActionKind.Start, rework.Kind, "restart: auto → riparte da solo, anche se il passo è ask-owner");
            Assert.AreEqual(2, rework.Attempt);
            Assert.AreEqual("manca il RPO", rework.ReworkNote, "l'agente riceve il motivo");

            Add(round, "tecnica", "responsabile-tecnico", RoundEventType.Started, attempt: 2);
            Add(round, "tecnica", "responsabile-tecnico", RoundEventType.Delivered, attempt: 2, files: new() { "gara/schede/tecnica.md" });
            Add(round, "tecnica", "responsabile-tecnico", RoundEventType.Rejected, attempt: 2, note: "ancora no");

            var stop = WorkflowScheduler.Decide(wf, round, Pc("responsabile-tecnico")).Single();
            Assert.AreEqual(SchedulerActionKind.Blocked, stop.Kind, "max 1: rifatto una volta, al secondo rifiuto si ferma");
            StringAssert.Contains(stop.Reason, "è il massimo del ciclo «rifacimento»");
        }

        [TestMethod]
        public void Run_the_rounds_of_a_for_loop_and_go_on_after_the_last()
        {
            var wf = Parse(@"{ ""mde_workflow"": 2, ""title"": ""Revisione"",
              ""steps"": [
                { ""id"": ""bozza"", ""agent"": ""scrittore"", ""trigger"": { ""launch"": true }, ""start"": ""manual"", ""produces"": [""doc/b.md""] },
                { ""id"": ""revisione"", ""agent"": ""revisore"", ""trigger"": { ""after"": [""bozza""] }, ""start"": ""auto"",
                  ""brief"": ""Rivedi la bozza."", ""produces"": [""doc/r.md""] },
                { ""id"": ""pubblica"", ""agent"": ""scrittore"", ""trigger"": { ""after"": [""revisione""] }, ""start"": ""auto"", ""brief"": ""Pubblica."" } ],
              ""loops"": [ { ""id"": ""giri"", ""steps"": [""revisione""], ""times"": 3 } ] }");
            var round = NewRound();
            Approve(round, "bozza", "scrittore", "doc/b.md");

            Assert.AreEqual(1, WorkflowScheduler.Decide(wf, round, Everyone).Single(a => a.Step.Id == "revisione").Round);
            for (var r = 1; r <= 3; r++)
            {
                Add(round, "revisione", "revisore", RoundEventType.Started, roundNo: r);
                Add(round, "revisione", "revisore", RoundEventType.Delivered, roundNo: r, files: new() { "doc/r.md" });
                Add(round, "revisione", "revisore", RoundEventType.Approved, roundNo: r);
                var next = WorkflowScheduler.Decide(wf, round, Everyone).Single();
                if (r < 3)
                {
                    Assert.AreEqual("revisione", next.Step.Id, $"giro {r} approvato: parte il giro {r + 1}");
                    Assert.AreEqual(r + 1, next.Round);
                }
                else
                {
                    Assert.AreEqual("pubblica", next.Step.Id, "dopo il terzo giro si va avanti");
                    Assert.AreEqual(StepStatus.Finished, WorkflowScheduler.View(wf, round, wf.Step("revisione")).Status);
                }
            }
        }

        [TestMethod]
        public void Start_on_the_first_finished_step_when_waiting_for_any()
        {
            var wf = Parse(Gara.Replace(@"""after"": [""tecnica"", ""contratto"", ""delivery""] }", @"""after"": [""tecnica"", ""contratto"", ""delivery""], ""wait"": ""any"" }"));
            var round = RoundStarted();
            Approve(round, "contratto", "responsabile-legale", "gara/schede/contrattuale.md");

            var summary = WorkflowScheduler.Decide(wf, round, Pc("account-manager")).Single();
            Assert.AreEqual("sintesi", summary.Step.Id);
            CollectionAssert.AreEqual(new[] { "gara/schede/contrattuale.md" }, summary.Inputs.ToList());
        }

        [TestMethod]
        public void Keep_waiting_when_an_owner_declines_and_say_why_a_variable_is_missing()
        {
            var wf = Parse(Gara);
            var round = RoundStarted();
            Approve(round, "tecnica", "responsabile-tecnico", "gara/schede/tecnica.md");
            Approve(round, "delivery", "responsabile-delivery", "gara/schede/delivery.md");
            Add(round, "contratto", "responsabile-legale", RoundEventType.Held);
            Add(round, "contratto", "responsabile-legale", RoundEventType.Declined, note: "lo fa lo studio esterno");

            Assert.AreEqual(StepStatus.Declined, WorkflowScheduler.View(wf, round, wf.Step("contratto")).Status);
            Assert.AreEqual(0, WorkflowScheduler.Decide(wf, round, Everyone).Count, "«tutti»: senza la contrattuale la sintesi non parte");

            var noValue = NewRound();
            Add(noValue, "ricerca", "account-manager", RoundEventType.Replied, reply: "avvia-giro", values: new() { ["altro"] = "x" });
            var blocked = WorkflowScheduler.Decide(wf, noValue, Pc("responsabile-tecnico")).Single();
            Assert.AreEqual(SchedulerActionKind.Blocked, blocked.Kind);
            StringAssert.Contains(blocked.Reason, "manca il valore di {codice}");
        }

        // ---- il registro su disco ----

        [TestMethod]
        public void Write_one_file_per_step_and_read_the_round_back()
        {
            var root = Path.Combine(Path.GetTempPath(), "giri-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                var id = RoundLedger.NewRoundId("gara/gara.workflow.json", T0, new Random(1));
                StringAssert.StartsWith(id, "2026-10-06-gara-");
                RoundLedger.Open(root, new RoundHeader { Id = id, Workflow = "gara/gara.workflow.json", StartedAt = T0, StartedBy = "am@test.local" });
                Assert.ThrowsException<InvalidOperationException>(() => RoundLedger.Open(root, new RoundHeader { Id = id }), "un giro non si riapre");

                RoundLedger.Append(root, id, "ricerca", "account-manager", new RoundEvent { Type = RoundEventType.Started, At = T0 });
                RoundLedger.Append(root, id, "ricerca", "account-manager", new RoundEvent
                {
                    Type = RoundEventType.Replied, At = T0.AddMinutes(1), Reply = "avvia-giro", Values = new() { ["codice"] = "NC-1" },
                });
                RoundLedger.Append(root, id, "tecnica", "responsabile-tecnico", new RoundEvent { Type = RoundEventType.Held, At = T0.AddMinutes(2) });

                var files = Directory.GetFiles(RoundLedger.RoundFolder(root, id)).Select(Path.GetFileName).OrderBy(f => f).ToList();
                CollectionAssert.AreEqual(new[] { "giro.json", "ricerca.json", "tecnica.json" }, files, "un file per passo: un solo scrittore ciascuno");

                var round = RoundLedger.Load(root, id);
                Assert.AreEqual(2, round.Record("ricerca").Events.Count);
                Assert.AreEqual("NC-1", round.Variables["codice"]);
                CollectionAssert.AreEqual(new[] { id }, RoundLedger.RoundIds(root).ToList());

                var tecnica = Path.Combine(RoundLedger.RoundFolder(root, id), "tecnica.json");
                File.WriteAllText(tecnica, File.ReadAllText(tecnica).Replace("\"held\"", "\"in-attesa\""));
                var ex = Assert.ThrowsException<InvalidDataException>(() => RoundLedger.Load(root, id));
                StringAssert.Contains(ex.Message, "'in-attesa' non è un evento");

                File.WriteAllText(tecnica, File.ReadAllText(tecnica).Replace("\"in-attesa\"", "\"held\"").Replace("\"step\"", "\"colore\": \"blu\", \"step\""));
                ex = Assert.ThrowsException<InvalidDataException>(() => RoundLedger.Load(root, id));
                StringAssert.Contains(ex.Message, "tecnica.json");
            }
            finally { Directory.Delete(root, true); }
        }

        [TestMethod]
        public void Refuse_to_write_into_a_round_that_was_never_opened()
        {
            var root = Path.Combine(Path.GetTempPath(), "giri-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                var ex = Assert.ThrowsException<InvalidOperationException>(() =>
                    RoundLedger.Append(root, "2026-10-06-gara-000000", "tecnica", "t", new RoundEvent { Type = RoundEventType.Held, At = T0 }));
                StringAssert.Contains(ex.Message, "va aperto prima");
            }
            finally { Directory.Delete(root, true); }
        }

        [TestMethod]
        public void Coordinate_two_computers_through_their_copies_without_writing_the_same_file()
        {
            // Due cloni del progetto: quello di Anna (account manager) e quello di Marco (tecnico). «git pull» = copiare i
            // file dell'altro; se nessuno scrive un file dell'altro, il pull non ha mai conflitti.
            var wf = Parse(Gara);
            var anna = Path.Combine(Path.GetTempPath(), "anna-" + Guid.NewGuid().ToString("N"));
            var marco = Path.Combine(Path.GetTempPath(), "marco-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(anna); Directory.CreateDirectory(marco);
            var written = new Dictionary<string, string>();   // file → chi l'ha scritto
            void Write(string who, string root, string id, string step, string agent, RoundEvent e)
            {
                RoundLedger.Append(root, id, step, agent, e);
                var key = step + ".json";
                if (written.TryGetValue(key, out var before)) Assert.AreEqual(before, who, $"{key} scritto da due computer");
                written[key] = who;
            }
            void Pull(string from, string to)
            {
                foreach (var f in Directory.GetFiles(Path.Combine(from, RoundLedger.Folder), "*.json", SearchOption.AllDirectories))
                {
                    var target = Path.Combine(to, Path.GetRelativePath(from, f));
                    Directory.CreateDirectory(Path.GetDirectoryName(target));
                    File.Copy(f, target, overwrite: true);
                }
            }
            try
            {
                var id = "2026-10-06-gara-a1b2c3";
                RoundLedger.Open(anna, new RoundHeader { Id = id, Workflow = "gara/gara.workflow.json", StartedAt = T0, StartedBy = "anna@test" });
                Write("anna", anna, id, "ricerca", "account-manager", new RoundEvent { Type = RoundEventType.Started, At = T0 });
                Write("anna", anna, id, "ricerca", "account-manager", new RoundEvent { Type = RoundEventType.Delivered, At = T0.AddMinutes(1), Files = new() { "gara/ricerche/r.md" } });
                Write("anna", anna, id, "ricerca", "account-manager", new RoundEvent { Type = RoundEventType.Approved, At = T0.AddMinutes(2) });
                Write("anna", anna, id, "ricerca", "account-manager", new RoundEvent { Type = RoundEventType.Replied, At = T0.AddMinutes(3), Reply = "avvia-giro", Values = new() { ["codice"] = "NC-1" } });

                Pull(anna, marco);
                var onMarco = WorkflowScheduler.Decide(wf, RoundLedger.Load(marco, id), Pc("responsabile-tecnico")).Single();
                Assert.AreEqual(SchedulerActionKind.Hold, onMarco.Kind);
                Write("marco", marco, id, "tecnica", "responsabile-tecnico", new RoundEvent { Type = RoundEventType.Held, At = T0.AddMinutes(4) });

                Pull(marco, anna);
                var onAnna = RoundLedger.Load(anna, id);
                Assert.AreEqual(StepStatus.Held, WorkflowScheduler.View(wf, onAnna, wf.Step("tecnica")).Status, "Anna vede che la scheda aspetta Marco");
                Assert.AreEqual(0, WorkflowScheduler.Decide(wf, onAnna, Pc("account-manager")).Count, "e non fa niente al posto suo");
                Assert.AreEqual(0, WorkflowScheduler.Decide(wf, RoundLedger.Load(marco, id), Pc("responsabile-tecnico")).Count, "Marco non lo rifà");
            }
            finally { Directory.Delete(anna, true); Directory.Delete(marco, true); }
        }
    }
}
