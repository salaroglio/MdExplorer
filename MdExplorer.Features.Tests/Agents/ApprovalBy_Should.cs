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
    /// Approvazione per ruolo (workflow v3, F5): chi approva scrive le sue decisioni in un suo file del registro, che alla
    /// lettura entrano negli eventi del passo subito dopo la consegna che giudicano; chi approva è chi ha il ruolo.
    /// </summary>
    [TestClass]
    public class ApprovalBy_Should
    {
        private static readonly DateTime T0 = new(2026, 10, 17, 9, 0, 0, DateTimeKind.Utc);

        [TestMethod]
        public void Read_the_approvers_decisions_right_after_the_delivery_they_judge()
        {
            var root = Path.Combine(Path.GetTempPath(), "giri-" + Guid.NewGuid().ToString("N"));
            try
            {
                const string id = "2026-10-17-gara-abc123";
                RoundLedger.Open(root, new RoundHeader { Id = id, Workflow = "gara.workflow.json", StartedAt = T0 });
                RoundLedger.Append(root, id, "tecnica", "tecnico", new RoundEvent { Type = RoundEventType.Started, At = T0 });
                RoundLedger.Append(root, id, "tecnica", "tecnico", new RoundEvent { Type = RoundEventType.Delivered, At = T0.AddMinutes(10) });
                RoundLedger.Append(root, id, "tecnica", "tecnico", new RoundEvent { Type = RoundEventType.Started, At = T0.AddMinutes(30), Attempt = 2 });
                RoundLedger.Append(root, id, "tecnica", "tecnico", new RoundEvent { Type = RoundEventType.Delivered, At = T0.AddMinutes(40), Attempt = 2 });
                // L'orologio di chi approva è indietro: il rifiuto del primo tentativo «sembra» prima della consegna.
                RoundLedger.AppendDecision(root, id, "tecnica", new RoundEvent { Type = RoundEventType.Rejected, At = T0.AddMinutes(5), By = "dir@x.it", Note = "no" });
                RoundLedger.AppendDecision(root, id, "tecnica", new RoundEvent { Type = RoundEventType.Approved, At = T0.AddMinutes(41), By = "dir@x.it", Attempt = 2 });

                var events = RoundLedger.Load(root, id).EventsOf("tecnica").Select(e => e.Type + e.Attempt).ToList();
                CollectionAssert.AreEqual(new[] { "started1", "delivered1", "rejected1", "started2", "delivered2", "approved2" }, events);

                Assert.ThrowsException<ArgumentException>(() => RoundLedger.AppendDecision(root, id, "tecnica",
                    new RoundEvent { Type = RoundEventType.Started, At = T0 }), "chi approva scrive solo decisioni");
                RoundLedger.AppendDecision(root, id, "tecnica", new RoundEvent { Type = RoundEventType.Approved, At = T0, Attempt = 3 });
                var ex = Assert.ThrowsException<InvalidDataException>(() => RoundLedger.Load(root, id));
                StringAssert.Contains(ex.Message, "which has no delivery", "una decisione senza consegna non si ignora");
            }
            finally { try { Directory.Delete(root, true); } catch (IOException) { } }
        }

        private const string Workflow = @"{
  ""mde_workflow"": 3, ""title"": ""Gara"",
  ""steps"": [
    { ""id"": ""ricerca"", ""agent"": ""capo"", ""skill"": ""cerca"", ""trigger"": { ""launch"": true }, ""start"": ""manual"" },
    { ""id"": ""scheda"", ""agent"": ""tecnico"", ""skill"": ""scheda"", ""trigger"": { ""after"": [""ricerca""] }, ""start"": ""auto"",
      ""approval"": { ""by"": ""Direzione"" } }
  ]
}";

        private static WorkflowDescriptor Wf() => GaraV3.Bound(Workflow, new WorkflowCardSet().Job("capo", "cerca").Job("tecnico", "scheda", "schede/tecnica.md"));

        private static RoundState Round(params (string Step, RoundEvent Event)[] events)
        {
            var round = new RoundState { Header = new RoundHeader { Id = "g", Workflow = "w", StartedAt = T0 } };
            foreach (var (step, e) in events)
            {
                if (!round.Steps.TryGetValue(step, out var rec)) round.Steps[step] = rec = new StepRecord { Step = step };
                rec.Events.Add(e);
            }
            return round;
        }

        private static StepOwner Approver(RoundState round, params string[] direzione)
            => StepOwners.Approver(Wf(), round, Wf().Step("scheda"), a => a == "tecnico" ? new[] { "marco@x.it" } : new[] { "anna@x.it" },
                role => role == "Direzione" ? direzione : Array.Empty<string>());

        [TestMethod]
        public void Be_who_has_the_role_and_in_a_team_who_already_acted_in_the_round()
        {
            var started = Round(("ricerca", new RoundEvent { Type = RoundEventType.Done, At = T0, By = "anna@x.it" }),
                                ("scheda", new RoundEvent { Type = RoundEventType.Delivered, At = T0, By = "marco@x.it" }));
            Assert.AreEqual("dir@x.it", Approver(started, "dir@x.it").Email, "una persona sola nel ruolo");
            Assert.AreEqual("anna@x.it", Approver(started, "dir@x.it", "anna@x.it").Email, "team: chi ha lanciato il giro ha agito prima");
            Assert.AreEqual("marco@x.it", Approver(started, "anna@x.it", "marco@x.it").Email, "chi risponde dell'agente, se ha il ruolo");

            var nobody = Approver(started, "dir@x.it", "lia@x.it");
            Assert.IsNull(nobody.Email, "nessuno del team ha agito: non si indovina");
            StringAssert.Contains(nobody.Problem, "the role «Direzione» that approves it belongs to more than one person");
            Assert.IsNotNull(Approver(started).Problem, "nessuno ha il ruolo: si dice");
        }
    }
}
