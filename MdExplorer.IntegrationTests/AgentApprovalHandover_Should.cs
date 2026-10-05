using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Controllers.A2A;
using MdExplorer.Features.Agents;
using MdExplorer.Services.AgentRun;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// «Autorizza» passa il lavoro al collega: a nome della persona, solo verso i destinatari che la scheda
    /// dell'agente dichiara, e con la persona che sceglie quando sono più d'uno. Senza scelta non si fonde niente.
    /// <para>Un agente che non dichiara destinatari si comporta come sempre.</para>
    /// </summary>
    [TestClass]
    public class AgentApprovalHandover_Should
    {
        private static ApprovalRecipient R(string name, bool available = true, string reason = null)
            => new ApprovalRecipient { Name = name, Role = "ruolo " + name, Available = available, Reason = reason };

        private sealed class FakeRequests : IAgentMergeRequestService
        {
            public AgentMergeRequest Request = new AgentMergeRequest
            {
                Id = Guid.NewGuid(), ProjectPath = "/p", AgentName = "tecnico",
                Status = AgentMergeRequest.StatusEnum.Pending, ChangedFiles = "added\tgara/tecnico.md",
            };
            public string MergeOutcome = AgentMergeRequest.StatusEnum.Merged;
            public int Approved;

            public AgentMergeRequest Open(string a, string b, string c, string d, string e, IEnumerable<ChangedFile> f, string g = null, string h = null) => throw new NotSupportedException();
            public IReadOnlyList<AgentMergeRequest> Pending(string projectPath) => new[] { Request };
            public string PublishedBranchOf(string localBranch) => null;
            public bool SomeoneIsWaitingFor(AgentMergeRequest request) => false;
            public IReadOnlyList<AgentMergeRequest> Stopped(string projectPath) => Array.Empty<AgentMergeRequest>();
            public AgentMergeRequest Rework(Guid id) => throw new NotSupportedException();
            public Task<string> PublishCopyAsync(string a, string b, string c, string d, CancellationToken ct = default) => throw new NotSupportedException();
            public AgentMergeRequest Get(Guid id) => id == Request.Id ? Request : null;
            public IReadOnlyList<ChangedFile> FilesOf(AgentMergeRequest r) => new[] { new ChangedFile { Change = "added", Path = "gara/tecnico.md" } };
            public Task<AgentMergeRequest> ApproveAsync(Guid id, CancellationToken ct = default)
            {
                Approved++;
                Request.Status = MergeOutcome;
                return Task.FromResult(Request);
            }
            public AgentMergeRequest Reject(Guid id, string note) => throw new NotSupportedException();
        }

        private sealed class FakeNotifier : IAgentApprovalNotifier
        {
            public List<ApprovalRecipient> Candidates = new();
            public ApprovalNotice Result;
            public readonly List<string> Notified = new();

            public string Summary;
            public string SummaryOf(string projectPath, string agentName) => Summary;
            public IReadOnlyList<ApprovalRecipient> CandidatesFor(string projectPath, string producerAgent) => Candidates;
            public ApprovalNotice Notify(string projectPath, string producerAgent, string recipient, IEnumerable<string> files)
            {
                Notified.Add(recipient);
                return Result ?? new ApprovalNotice { Notified = true, Recipient = recipient };
            }
        }

        private sealed class FakeHold : IAgentWorktreeHoldService
        {
            public bool IsHeld(string projectPath, string agentName) => false;
            public string ReasonFor(string projectPath, string agentName) => null;
            public void Open(string projectPath, string agentName, string reason) { }
            public InterventionCloseResult Close(string projectPath, string agentName, bool discardWork) => throw new NotSupportedException();
        }

        private static (AgentReviewController c, FakeRequests req, FakeNotifier n) Build(params ApprovalRecipient[] candidates)
        {
            var req = new FakeRequests();
            var n = new FakeNotifier { Candidates = candidates.ToList() };
            var c = new AgentReviewController(req, null, new FakeHold(), n, NullLogger<AgentReviewController>.Instance)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
            };
            return (c, req, n);
        }

        private static (int status, JsonElement body) Read(IActionResult result)
        {
            var obj = (ObjectResult)result;
            var json = JsonSerializer.SerializeToElement(obj.Value);
            return (obj.StatusCode ?? 200, json);
        }

        [TestMethod]
        public async Task Approve_as_always_when_the_card_declares_no_recipients()
        {
            var (c, req, n) = Build();
            var (status, body) = Read(await c.Approve(req.Request.Id));
            Assert.AreEqual(200, status);
            Assert.AreEqual(1, req.Approved);
            Assert.AreEqual(0, n.Notified.Count);
            Assert.AreEqual(JsonValueKind.Null, body.GetProperty("notice").ValueKind);
        }

        [TestMethod]
        public async Task Notify_the_only_recipient_without_asking()
        {
            var (c, req, n) = Build(R("account-manager"));
            var (status, body) = Read(await c.Approve(req.Request.Id));
            Assert.AreEqual(200, status);
            CollectionAssert.AreEqual(new[] { "account-manager" }, n.Notified);
            Assert.IsTrue(body.GetProperty("notice").GetProperty("notified").GetBoolean());
        }

        [TestMethod]
        public async Task Refuse_to_approve_when_there_are_several_recipients_and_no_choice()
        {
            var (c, req, n) = Build(R("legale"), R("account-manager"));
            var (status, body) = Read(await c.Approve(req.Request.Id));
            Assert.AreEqual(409, status);
            Assert.IsTrue(body.GetProperty("needsChoice").GetBoolean());
            Assert.AreEqual("choose-recipient", body.GetProperty("code").GetString());
            Assert.AreEqual(2, body.GetProperty("candidates").GetArrayLength());
            Assert.AreEqual(0, req.Approved, "senza scelta non si fonde niente");
            Assert.AreEqual(0, n.Notified.Count);
        }

        [TestMethod]
        public async Task Notify_the_recipient_the_person_chose()
        {
            var (c, req, n) = Build(R("legale"), R("account-manager"));
            var (status, _) = Read(await c.Approve(req.Request.Id, new AgentReviewController.ApproveRequest { Notify = "ACCOUNT-manager" }));
            Assert.AreEqual(200, status);
            CollectionAssert.AreEqual(new[] { "account-manager" }, n.Notified);
            Assert.AreEqual(1, req.Approved);
        }

        [TestMethod]
        public async Task Approve_without_notifying_anyone_when_the_person_says_nobody()
        {
            var (c, req, n) = Build(R("legale"), R("account-manager"));
            var (status, _) = Read(await c.Approve(req.Request.Id, new AgentReviewController.ApproveRequest { Nobody = true }));
            Assert.AreEqual(200, status);
            Assert.AreEqual(1, req.Approved);
            Assert.AreEqual(0, n.Notified.Count);
        }

        [TestMethod]
        public async Task Refuse_a_recipient_the_card_does_not_declare()
        {
            var (c, req, n) = Build(R("legale"), R("account-manager"));
            var (status, body) = Read(await c.Approve(req.Request.Id, new AgentReviewController.ApproveRequest { Notify = "qualcun-altro" }));
            Assert.AreEqual(422, status);
            Assert.AreEqual("recipient-unknown", body.GetProperty("code").GetString());
            Assert.AreEqual(0, req.Approved);
        }

        [TestMethod]
        public async Task Refuse_a_recipient_that_cannot_be_reached_before_merging()
        {
            var (c, req, n) = Build(R("legale", available: false, reason: "'legale' non è fidato."), R("account-manager"));
            var (status, body) = Read(await c.Approve(req.Request.Id, new AgentReviewController.ApproveRequest { Notify = "legale" }));
            Assert.AreEqual(422, status);
            Assert.AreEqual("recipient-unavailable", body.GetProperty("code").GetString());
            StringAssert.Contains(body.GetProperty("error").GetString(), "non è fidato");
            Assert.AreEqual(0, req.Approved, "un destinatario irraggiungibile non lascia un lavoro fuso a metà");
        }

        [TestMethod]
        public async Task Ask_the_person_when_the_only_recipient_is_unreachable()
        {
            var (c, req, n) = Build(R("legale", available: false, reason: "'legale' non è fidato."));
            var (status, body) = Read(await c.Approve(req.Request.Id));
            Assert.AreEqual(409, status);
            Assert.IsTrue(body.GetProperty("needsChoice").GetBoolean());
            Assert.AreEqual(0, req.Approved);

            // Resta la via d'uscita: approvare senza avvisare.
            var (status2, _) = Read(await c.Approve(req.Request.Id, new AgentReviewController.ApproveRequest { Nobody = true }));
            Assert.AreEqual(200, status2);
        }

        [TestMethod]
        public async Task Not_notify_anyone_when_the_merge_did_not_complete()
        {
            var (c, req, n) = Build(R("account-manager"));
            req.MergeOutcome = AgentMergeRequest.StatusEnum.Failed;
            var (status, _) = Read(await c.Approve(req.Request.Id));
            Assert.AreEqual(409, status);
            Assert.AreEqual(0, n.Notified.Count, "se il lavoro non è nel ramo principale non si passa a nessuno");
        }

        [TestMethod]
        public async Task Say_so_when_the_notice_could_not_be_sent_after_the_merge()
        {
            var (c, req, n) = Build(R("account-manager"));
            n.Result = new ApprovalNotice { Notified = false, Recipient = "account-manager", Error = "mailbox piena" };
            var (status, body) = Read(await c.Approve(req.Request.Id));
            Assert.AreEqual(200, status, "il lavoro è comunque fuso");
            var notice = body.GetProperty("notice");
            Assert.IsFalse(notice.GetProperty("notified").GetBoolean());
            Assert.AreEqual("mailbox piena", notice.GetProperty("error").GetString());
        }

        [TestMethod]
        public void Show_what_the_agent_does_next_to_the_approval()
        {
            var (c, req, n) = Build();
            n.Summary = "Controlla le fatture contro il contratto.";
            var item = Read(c.Pending("/p")).body.GetProperty("requests")[0];
            Assert.AreEqual("Controlla le fatture contro il contratto.", item.GetProperty("agentSummary").GetString());

            n.Summary = null;
            item = Read(c.Pending("/p")).body.GetProperty("requests")[0];
            Assert.AreEqual(JsonValueKind.Null, item.GetProperty("agentSummary").ValueKind, "una scheda senza riassunto non ne inventa uno");
        }

        [TestMethod]
        public void Show_the_recipients_before_the_person_decides()
        {
            var (c, req, _) = Build(R("legale"), R("account-manager", available: false, reason: "non esiste"));
            var (status, body) = Read(c.Pending("/p"));
            var item = body.GetProperty("requests")[0];
            var cands = item.GetProperty("notifyCandidates");
            Assert.AreEqual(2, cands.GetArrayLength());
            Assert.AreEqual("legale", cands[0].GetProperty("name").GetString());
            Assert.IsFalse(cands[1].GetProperty("available").GetBoolean());
            Assert.AreEqual("non esiste", cands[1].GetProperty("reason").GetString());
        }
    }
}
