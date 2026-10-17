using System;
using System.Collections.Generic;
using System.Linq;
using MdExplorer.Features.Agents.Workflow;
using MdExplorer.Features.Agents.Workflow.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    /// <summary>
    /// L'avviso di lavoro non processato per i giri (17/10/2026): chi ha passato un passo a un altro computer lo sa se da
    /// 24 ore non succede niente. Anna lancia la gara e fa contratto e delivery; la scheda tecnica è di Marco.
    /// </summary>
    [TestClass]
    public class StalledSteps_Should
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

        private const string Anna = "anna@test.local";
        private const string Marco = "marco@test.local";
        private static readonly DateTime T0 = new(2026, 10, 6, 9, 0, 0, DateTimeKind.Utc);
        private int _tick;

        private static WorkflowDescriptor Wf()
        {
            var r = WorkflowParser.Parse(Gara);
            Assert.IsTrue(r.IsValid, string.Join("\n", r.Issues));
            return r.Descriptor;
        }

        private static string OwnerOf(WorkflowStep s) => s.Agent == "responsabile-tecnico" ? Marco : Anna;

        private void Add(RoundState round, string step, string agent, string type)
        {
            if (!round.Steps.TryGetValue(step, out var record))
                round.Steps[step] = record = new StepRecord { Step = step, Agent = agent };
            record.Events.Add(new RoundEvent { Type = type, At = T0.AddMinutes(++_tick), By = "x", Round = 1, Attempt = 1 });
        }

        /// <summary>Anna ha fatto la ricerca e premuto «Avvia il giro»: le tre schede possono partire.</summary>
        private RoundState Started()
        {
            var round = new RoundState { Header = new RoundHeader { Id = "2026-10-06-gara-abc123", Workflow = "gara/gara.workflow.json", StartedAt = T0, StartedBy = Anna } };
            Add(round, "ricerca", "account-manager", RoundEventType.Started);
            Add(round, "ricerca", "account-manager", RoundEventType.Delivered);
            Add(round, "ricerca", "account-manager", RoundEventType.Approved);
            round.Steps["ricerca"].Events.Add(new RoundEvent
            {
                Type = RoundEventType.Replied, At = T0.AddMinutes(++_tick), By = Anna, Reply = "avvia-giro",
                Values = new Dictionary<string, string> { ["codice"] = "NC-1" },
            });
            return round;
        }

        [TestMethod]
        public void Tell_who_passed_the_step_when_the_other_computer_never_took_it()
        {
            var stalled = StalledSteps.Find(Wf(), Started(), OwnerOf, Anna, T0.AddHours(25)).Single();
            Assert.AreEqual("tecnica", stalled.Step.Id);
            Assert.AreEqual(Marco, stalled.OwnerEmail);
            Assert.AreEqual(StepStatus.NotStarted, stalled.Status);
            Assert.AreEqual(T0.AddMinutes(4), stalled.Since, "da quando poteva partire: l'ultima notizia della ricerca");
        }

        [TestMethod]
        public void Wait_24_hours_before_telling()
        {
            Assert.AreEqual(0, StalledSteps.Find(Wf(), Started(), OwnerOf, Anna, T0.AddHours(23)).Count);
        }

        [TestMethod]
        public void Tell_when_the_step_waits_to_be_started_or_is_being_worked_for_too_long()
        {
            var held = Started();
            Add(held, "tecnica", "responsabile-tecnico", RoundEventType.Held);
            var h = StalledSteps.Find(Wf(), held, OwnerOf, Anna, T0.AddHours(25)).Single();
            Assert.AreEqual(StepStatus.Held, h.Status);
            Assert.AreEqual(T0.AddMinutes(5), h.Since);

            var working = Started();
            Add(working, "tecnica", "responsabile-tecnico", RoundEventType.Held);
            Add(working, "tecnica", "responsabile-tecnico", RoundEventType.Started);
            Assert.AreEqual(StepStatus.Working, StalledSteps.Find(Wf(), working, OwnerOf, Anna, T0.AddHours(25)).Single().Status);
            Assert.AreEqual(0, StalledSteps.Find(Wf(), working, OwnerOf, Anna, T0.AddMinutes(6).AddHours(23)).Count,
                "il conto riparte dall'ultima notizia");
        }

        [TestMethod]
        public void Stay_quiet_once_the_step_has_given_its_news()
        {
            var delivered = Started();
            Add(delivered, "tecnica", "responsabile-tecnico", RoundEventType.Held);
            Add(delivered, "tecnica", "responsabile-tecnico", RoundEventType.Started);
            Add(delivered, "tecnica", "responsabile-tecnico", RoundEventType.Delivered);
            Assert.AreEqual(0, StalledSteps.Find(Wf(), delivered, OwnerOf, Anna, T0.AddDays(3)).Count);

            var declined = Started();
            Add(declined, "tecnica", "responsabile-tecnico", RoundEventType.Held);
            Add(declined, "tecnica", "responsabile-tecnico", RoundEventType.Declined);
            Assert.AreEqual(0, StalledSteps.Find(Wf(), declined, OwnerOf, Anna, T0.AddDays(3)).Count);
        }

        [TestMethod]
        public void Not_tell_the_owner_of_the_step_nor_who_did_not_pass_it()
        {
            Assert.AreEqual(0, StalledSteps.Find(Wf(), Started(), OwnerOf, Marco, T0.AddHours(25)).Count,
                "Marco non avvisa sé stesso, e le schede di Anna non gliele ha passate lui");
            Assert.AreEqual(0, StalledSteps.Find(Wf(), Started(), OwnerOf, "luca@test.local", T0.AddHours(25)).Count,
                "un altro computer del team non ha passato niente");
        }

        [TestMethod]
        public void Not_call_stopped_a_round_that_waits_for_a_computer_that_is_off()
        {
            // Il pulsante è stato premuto e nessuna scheda è ancora partita: aspettano i computer dei responsabili.
            // Fino al 17/10/2026 il giro risultava fermo, e si poteva archiviare.
            var round = Started();
            Assert.IsFalse(WorkflowScheduler.IsStopped(Wf(), round, WorkflowScheduler.Views(Wf(), round)));
        }

        [TestMethod]
        public void Ignore_a_step_whose_owner_is_unknown()
        {
            Assert.AreEqual(0, StalledSteps.Find(Wf(), Started(), s => s.Agent == "responsabile-tecnico" ? null : Anna, Anna, T0.AddHours(25)).Count,
                "senza responsabile lo dice già un altro avviso");
        }
    }
}
