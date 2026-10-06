using System;
using System.Collections.Generic;
using System.Linq;
using MdExplorer.Features.Agents.Workflow;
using MdExplorer.Features.Agents.Workflow.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    /// <summary>
    /// Chi risponde di un passo (W20-W22): un agente può essere di un team, e la responsabilità va al pezzo di workflow.
    /// Il team manager assegna i task agli sviluppatori con il pulsante; da lì ognuno risponde del suo pezzo, rifacimenti e
    /// passi successivi dello stesso agente compresi.
    /// </summary>
    [TestClass]
    public class StepOwners_Should
    {
        private const string Team = @"{
  ""mde_workflow"": 2, ""title"": ""Sprint"",
  ""variables"": { ""task"": ""il task"" },
  ""steps"": [
    { ""id"": ""pianifica"", ""agent"": ""team-manager"", ""trigger"": { ""launch"": true }, ""start"": ""manual"" },
    { ""id"": ""sviluppa"", ""agent"": ""sviluppatore"", ""trigger"": { ""reply"": ""assegna"", ""to"": ""pianifica"" }, ""start"": ""ask-owner"",
      ""brief"": ""Sviluppa {task}."" },
    { ""id"": ""collauda"", ""agent"": ""collaudatore"", ""trigger"": { ""after"": [""sviluppa""] }, ""start"": ""auto"", ""brief"": ""Collauda {task}."" },
    { ""id"": ""correggi"", ""agent"": ""sviluppatore"", ""trigger"": { ""after"": [""collauda""] }, ""start"": ""auto"", ""brief"": ""Correggi {task}."" },
    { ""id"": ""rilascia"", ""agent"": ""team-manager"", ""trigger"": { ""after"": [""correggi""] }, ""start"": ""auto"", ""brief"": ""Rilascia {task}."" }
  ]
}";

        private static readonly Dictionary<string, IReadOnlyList<string>> Table = new()
        {
            ["team-manager"] = new[] { "tm@x.it" },
            ["sviluppatore"] = new[] { "mario@x.it", "lucia@x.it" },
            ["collaudatore"] = new[] { "qa@x.it" },
        };

        private static WorkflowDescriptor Wf()
        {
            var r = WorkflowParser.Parse(Team);
            Assert.IsTrue(r.IsValid, string.Join("\n", r.Issues));
            return r.Descriptor;
        }

        private static RoundState Round(params (string Step, string Agent, RoundEvent Event)[] events)
        {
            var round = new RoundState { Header = new RoundHeader { Id = "g", Workflow = "w", StartedAt = DateTime.UtcNow } };
            foreach (var (step, agent, e) in events)
            {
                if (!round.Steps.TryGetValue(step, out var rec)) round.Steps[step] = rec = new StepRecord { Step = step, Agent = agent };
                rec.Events.Add(e);
            }
            return round;
        }

        private static StepOwner Owner(RoundState round, string step)
        {
            var wf = Wf();
            return StepOwners.Of(wf, round, wf.Step(step), a => Table.TryGetValue(a, out var t) ? t : Array.Empty<string>());
        }

        private static readonly (string, string, RoundEvent) Launched = ("pianifica", "team-manager", new RoundEvent { Type = RoundEventType.Started, By = "tm@x.it" });

        private static (string, string, RoundEvent) Pressed(string to) => ("pianifica", "team-manager", new RoundEvent
        {
            Type = RoundEventType.Replied, By = "tm@x.it", Reply = "assegna",
            Values = new() { ["task"] = "login" }, Assign = new() { ["sviluppa"] = to },
        });

        [TestMethod]
        public void Give_the_step_to_whom_the_team_manager_chose_with_the_button()
        {
            var owner = Owner(Round(Launched, Pressed("lucia@x.it")), "sviluppa");
            Assert.AreEqual("lucia@x.it", owner.Email);
        }

        [TestMethod]
        public void Keep_the_same_developer_for_the_next_piece_of_the_same_agent_even_after_another_agent()
        {
            // sviluppa (Lucia) → collauda (QA) → correggi: è il pezzo di Lucia, non di Mario.
            var round = Round(Launched, Pressed("lucia@x.it"),
                ("sviluppa", "sviluppatore", new RoundEvent { Type = RoundEventType.Held, By = "lucia@x.it" }));
            Assert.AreEqual("lucia@x.it", Owner(round, "correggi").Email);
            Assert.AreEqual("qa@x.it", Owner(round, "collauda").Email, "un agente con un solo responsabile va a lui");
            Assert.AreEqual("tm@x.it", Owner(round, "rilascia").Email);
        }

        [TestMethod]
        public void Follow_a_hand_over_to_a_colleague_of_the_team()
        {
            var round = Round(Launched, Pressed("lucia@x.it"),
                ("sviluppa", "sviluppatore", new RoundEvent { Type = RoundEventType.Held, By = "lucia@x.it" }),
                ("sviluppa", "sviluppatore", new RoundEvent { Type = RoundEventType.Assigned, By = "lucia@x.it", Owner = "mario@x.it", Note = "ferie" }));
            Assert.AreEqual("mario@x.it", Owner(round, "sviluppa").Email);
            Assert.AreEqual("mario@x.it", Owner(round, "correggi").Email, "il pezzo successivo segue chi l'ha preso");
        }

        [TestMethod]
        public void Ask_who_when_the_agent_has_a_team_and_nobody_chose()
        {
            var owner = Owner(Round(Launched, ("pianifica", "team-manager", new RoundEvent { Type = RoundEventType.Replied, By = "tm@x.it", Reply = "assegna" })), "sviluppa");
            Assert.IsNull(owner.Email, "non si indovina tra Mario e Lucia");
            Assert.IsTrue(owner.NeedsChoice);
            StringAssert.Contains(owner.Problem, "mario@x.it, lucia@x.it");
        }

        [TestMethod]
        public void Refuse_a_person_who_no_longer_answers_for_the_agent()
        {
            var owner = Owner(Round(Launched, Pressed("paolo@x.it")), "sviluppa");
            Assert.IsNull(owner.Email);
            Assert.IsFalse(owner.NeedsChoice);
            StringAssert.Contains(owner.Problem, "paolo@x.it, che non risponde più dell'agente 'sviluppatore'");
        }

        [TestMethod]
        public void Let_two_pieces_of_the_same_agent_belong_to_two_developers_in_the_same_round()
        {
            // Ogni pezzo ha la sua storia nel registro: il passo dice di chi è, l'agente no.
            var wf = Wf();
            var round = Round(Launched, Pressed("lucia@x.it"),
                ("sviluppa", "sviluppatore", new RoundEvent { Type = RoundEventType.Held, By = "lucia@x.it" }),
                ("correggi", "sviluppatore", new RoundEvent { Type = RoundEventType.Assigned, By = "lucia@x.it", Owner = "mario@x.it" }));
            Assert.AreEqual("lucia@x.it", Owner(round, "sviluppa").Email);
            Assert.AreEqual("mario@x.it", Owner(round, "correggi").Email);

            var lucia = WorkflowScheduler.Decide(wf, round, (WorkflowStep s) => Owner(round, s.Id).Email == "lucia@x.it");
            Assert.IsFalse(lucia.Any(a => a.Step.Id == "correggi"), "correggi non è di Lucia: il suo computer non lo fa");
        }
    }
}
