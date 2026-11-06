using System;
using System.Collections.Generic;
using System.Linq;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Agents.Workflow;
using MdExplorer.Features.Agents.Workflow.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    /// <summary>
    /// Il passo di una persona (workflow v3, F4): quando le tocca, il suo computer le mette in posta «tocca a te» con il
    /// compito, che cosa riceve e i pulsanti degli esiti; l'esito chiude il passo e fa da bivio per quelli dopo. Il passo è di
    /// chi ha il ruolo; con un team lo sceglie chi lo fa partire, o lo eredita chi ha già agito prima.
    /// </summary>
    [TestClass]
    public class PersonStep_Should
    {
        private static readonly DateTime T0 = new(2026, 10, 17, 9, 0, 0, DateTimeKind.Utc);
        private int _tick;

        /// <summary>La gara con la decisione della persona, e dopo la decisione un passo che parte solo da «Partecipiamo».</summary>
        private const string WithBranch = @"{
  ""mde_workflow"": 3, ""title"": ""Gara"",
  ""variables"": { ""codice"": ""il bando"", ""motivo"": ""perché si partecipa"" },
  ""steps"": [
    { ""id"": ""sintesi"", ""agent"": ""account-manager"", ""skill"": ""sintesi-gara"", ""trigger"": { ""launch"": true }, ""start"": ""manual"",
      ""inputs"": { ""codice"": ""NC-1"", ""ricerca"": ""la ricerca"", ""schede"": ""le schede"" } },
    { ""id"": ""decisione"", ""person"": ""Account manager"", ""title"": ""Decidi se partecipare"", ""trigger"": { ""after"": [""sintesi""] },
      ""task"": ""Leggi la sintesi e decidi."",
      ""inputs"": { ""sintesi"": { ""from"": ""sintesi.sintesi"" } },
      ""outcomes"": [ { ""id"": ""partecipa"", ""label"": ""Partecipiamo"", ""values"": [""motivo""] }, { ""id"": ""rinuncia"", ""label"": ""Non partecipiamo"" } ] },
    { ""id"": ""offerta"", ""person"": ""Responsabile tecnico"", ""title"": ""Prepara l'offerta"", ""trigger"": { ""reply"": ""partecipa"", ""to"": ""decisione"" },
      ""task"": ""Prepara l'offerta: {motivo}."",
      ""produces"": [ { ""id"": ""offerta"", ""path"": ""gara/offerte/offerta-*.md"" } ],
      ""outcomes"": [ { ""id"": ""fatto"", ""label"": ""Fatto"" } ] }
  ]
}";

        private static WorkflowDescriptor Wf() => GaraV3.Bound(WithBranch);

        private RoundState NewRound() => new() { Header = new RoundHeader { Id = "g1", Workflow = "gara.workflow.json", StartedAt = T0, StartedBy = "anna@x.it" } };

        private void Add(RoundState round, string step, string type, string by = "anna@x.it", string reply = null,
            Dictionary<string, string> values = null, Dictionary<string, string> assign = null, List<string> files = null, string commit = null)
        {
            if (!round.Steps.TryGetValue(step, out var record)) round.Steps[step] = record = new StepRecord { Step = step };
            record.Events.Add(new RoundEvent { Type = type, At = T0.AddMinutes(++_tick), By = by, Reply = reply, Values = values, Assign = assign, Files = files, Commit = commit });
        }

        private RoundState SummaryApproved()
        {
            var round = NewRound();
            Add(round, "sintesi", RoundEventType.Started);
            Add(round, "sintesi", RoundEventType.Delivered, files: new() { "gara/schede/sintesi.md" }, commit: "c-sintesi");
            Add(round, "sintesi", RoundEventType.Approved, commit: "c-sintesi");
            return round;
        }

        private static readonly Func<WorkflowStep, bool> Everyone = _ => true;

        [TestMethod]
        public void Ask_the_person_with_her_task_what_she_receives_and_her_outcomes()
        {
            var wf = Wf();
            var round = SummaryApproved();

            var ask = WorkflowScheduler.Decide(wf, round, Everyone).Single();
            Assert.AreEqual(SchedulerActionKind.Ask, ask.Kind, "il passo di una persona non si avvia: le tocca");
            Assert.AreEqual("decisione", ask.Step.Id);
            var file = ask.Inputs.Single().Files.Single();
            Assert.AreEqual("gara/schede/sintesi.md", file.Path);
            Assert.AreEqual("c-sintesi", file.Commit, "la versione approvata");

            Assert.AreEqual(@"Tocca a te: «Decidi se partecipare» (ruolo Account manager).

Leggi la sintesi e decidi.

Che cosa ricevi:
- sintesi:
  - gara/schede/sintesi.md — uscita «sintesi» di «sintesi», approvata nel commit c-sintesi

Chiudi il passo con uno dei pulsanti qui sotto.

Giro g1, passo «Decidi se partecipare».".Replace("\r\n", "\n"), AgentTexts.PersonTurn("it", ask, "g1"));

            Add(round, "decisione", RoundEventType.Asked, commit: "head");
            Assert.AreEqual(StepStatus.Asked, WorkflowScheduler.View(wf, round, wf.Step("decisione")).Status);
            Assert.AreEqual(0, WorkflowScheduler.Decide(wf, round, Everyone).Count, "già in posta: non si chiede due volte");
            Assert.IsFalse(WorkflowScheduler.IsStopped(wf, round, WorkflowScheduler.Views(wf, round)), "aspetta la persona: non è fermo");
        }

        [TestMethod]
        public void Close_the_step_with_the_outcome_and_start_only_the_branch_it_chose()
        {
            var wf = Wf();
            var round = SummaryApproved();
            Add(round, "decisione", RoundEventType.Asked, commit: "head");
            Add(round, "decisione", RoundEventType.Replied, reply: "partecipa", values: new() { ["motivo"] = "è il nostro mestiere" });
            Add(round, "decisione", RoundEventType.Done);

            Assert.AreEqual(StepStatus.Finished, WorkflowScheduler.View(wf, round, wf.Step("decisione")).Status);
            var next = WorkflowScheduler.Decide(wf, round, Everyone).Single();
            Assert.AreEqual("offerta", next.Step.Id, "«Partecipiamo» fa partire l'offerta");
            Assert.AreEqual("Prepara l'offerta: è il nostro mestiere.", next.Task, "il valore dell'esito diventa una variabile del giro");
            Assert.AreEqual("offerta", next.Outputs.Single().Id);

            var no = SummaryApproved();
            Add(no, "decisione", RoundEventType.Asked, commit: "head");
            Add(no, "decisione", RoundEventType.Replied, reply: "rinuncia");
            Add(no, "decisione", RoundEventType.Done);
            Assert.AreEqual(0, WorkflowScheduler.Decide(wf, no, Everyone).Count, "«Non partecipiamo»: l'altra strada non parte");
        }

        [TestMethod]
        public void Give_the_documents_a_person_wrote_to_the_steps_after_her()
        {
            var wf = GaraV3.Bound(WithBranch.Replace(@"""outcomes"": [ { ""id"": ""fatto"", ""label"": ""Fatto"" } ] }",
                @"""outcomes"": [ { ""id"": ""fatto"", ""label"": ""Fatto"" } ] },
    { ""id"": ""rilegge"", ""person"": ""Account manager"", ""trigger"": { ""after"": [""offerta""] }, ""task"": ""Rileggi."",
      ""inputs"": { ""offerta"": { ""from"": ""offerta.offerta"" } }, ""outcomes"": [ { ""id"": ""ok"", ""label"": ""Va bene"" } ] }"));
            var round = SummaryApproved();
            Add(round, "decisione", RoundEventType.Asked, commit: "h1");
            Add(round, "decisione", RoundEventType.Replied, reply: "partecipa", values: new() { ["motivo"] = "sì" });
            Add(round, "decisione", RoundEventType.Done);
            Add(round, "offerta", RoundEventType.Asked, by: "marco@x.it", commit: "h2");
            Add(round, "offerta", RoundEventType.Replied, by: "marco@x.it", reply: "fatto");
            Add(round, "offerta", RoundEventType.Done, by: "marco@x.it", files: new() { "gara/offerte/offerta-nc1.md" }, commit: "c-offerta");

            var file = WorkflowScheduler.Decide(wf, round, Everyone).Single().Inputs.Single().Files.Single();
            Assert.AreEqual("gara/offerte/offerta-nc1.md", file.Path, "il file che la persona ha scritto, non il modello con «*»");
            Assert.AreEqual("c-offerta", file.Commit);
        }

        private static readonly Dictionary<string, IReadOnlyList<string>> Agents = new()
        {
            ["account-manager"] = new[] { "anna@x.it" },
        };

        private static StepOwner Owner(WorkflowDescriptor wf, RoundState round, string step, params string[] members)
            => StepOwners.Of(wf, round, wf.Step(step), a => Agents.TryGetValue(a, out var t) ? t : Array.Empty<string>(),
                role => role == "Account manager" ? members : new[] { "marco@x.it" });

        [TestMethod]
        public void Belong_to_who_has_the_role_and_in_a_team_to_who_already_acted_before()
        {
            var wf = Wf();
            var round = SummaryApproved();
            Assert.AreEqual("anna@x.it", Owner(wf, round, "decisione", "anna@x.it").Email, "una persona sola: è sua");
            Assert.AreEqual("anna@x.it", Owner(wf, round, "decisione", "lia@x.it", "anna@x.it").Email,
                "team: chi ha fatto la sintesi la legge anche");

            var nobody = SummaryApproved();
            foreach (var e in nobody.Steps["sintesi"].Events) e.By = "esterno@x.it";
            var open = Owner(wf, nobody, "decisione", "lia@x.it", "anna@x.it");
            Assert.IsNull(open.Email, "nessuno del team ha agito prima: non si indovina");
            Assert.IsTrue(open.NeedsChoice);
            StringAssert.Contains(open.Problem, "the role «Account manager» belongs to more than one person");

            Assert.IsNotNull(Owner(wf, round, "decisione").Problem, "nessuno ha il ruolo: si dice");
        }

        [TestMethod]
        public void Go_to_the_person_chosen_by_who_pressed_the_outcome()
        {
            var wf = Wf();
            var round = SummaryApproved();
            Add(round, "decisione", RoundEventType.Asked, commit: "head");
            Add(round, "decisione", RoundEventType.Replied, reply: "partecipa", values: new() { ["motivo"] = "sì" },
                assign: new() { ["offerta"] = "piero@x.it" });

            var owner = StepOwners.Of(wf, round, wf.Step("offerta"), _ => Array.Empty<string>(), _ => new[] { "marco@x.it", "piero@x.it" });
            Assert.AreEqual("piero@x.it", owner.Email, "chi ha premuto «Partecipiamo» ha scelto");
        }

        [TestMethod]
        public void Stop_and_say_why_when_what_the_person_needs_is_missing()
        {
            var wf = Wf();
            var round = SummaryApproved();
            Add(round, "decisione", RoundEventType.Asked, commit: "head");
            Add(round, "decisione", RoundEventType.Replied, reply: "partecipa");   // senza il motivo
            Add(round, "decisione", RoundEventType.Done);

            var blocked = WorkflowScheduler.Decide(wf, round, Everyone).Single();
            Assert.AreEqual(SchedulerActionKind.Blocked, blocked.Kind);
            StringAssert.Contains(blocked.Reason, "{motivo}");
        }
    }
}
