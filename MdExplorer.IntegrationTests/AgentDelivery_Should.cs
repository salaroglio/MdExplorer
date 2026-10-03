using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.AgentRun;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// La consegna del lavoro di un agente ha tre esiti, e non vanno confusi: niente da consegnare
    /// (l'agente ha solo letto o risposto), consegnato, e <b>non pubblicato</b>.
    /// <para>
    /// Prima il terzo e il primo erano entrambi «nessun risultato»: un agente che non toccava nulla
    /// riempiva origin di rami vuoti e la UI di richieste di revisione senza file, mentre un push
    /// rifiutato (origin di sola lettura, come per chi clona un repository altrui) finiva solo nel
    /// log e il run risultava riuscito. Chi guardava la UI vedeva un agente che non aveva fatto nulla.
    /// </para>
    /// <para>Richiede <c>git</c> nel PATH.</para>
    /// </summary>
    [TestClass]
    public class AgentDelivery_Should
    {
        [TestMethod]
        public async Task Deliver_nothing_when_the_agent_changed_nothing()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (_, path, origin) = SetupGitProject(ctx, "consegna-vuota");
            var m = Manager(ctx);

            var prep = await m.PrepareForRunAsync(path, "alfa", "att1");
            Assert.IsTrue(prep.Success, prep.Error);

            var attempt = await m.TryCommitAndPushBranchAsync(path, "alfa", "lavoro di alfa");

            Assert.IsNotNull(attempt);
            Assert.IsTrue(attempt.NothingToDeliver, "nessun commit rispetto al default: niente da consegnare");
            Assert.IsNull(attempt.Pushed);
            Assert.IsNull(attempt.Error, "non è un errore");
            Assert.AreEqual("", Git(origin, "for-each-ref refs/heads/agent").Out.Trim(),
                "un ramo identico al default non va pubblicato su origin");
            Assert.IsNull(await m.CommitAndPushBranchAsync(path, "alfa", "lavoro di alfa"),
                "il vecchio contratto resta: nessun branch pubblicato");
        }

        [TestMethod]
        public async Task Deliver_the_branch_when_the_agent_wrote_a_file()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (_, path, origin) = SetupGitProject(ctx, "consegna-con-file");
            var m = Manager(ctx);

            var prep = await m.PrepareForRunAsync(path, "alfa", "att1");
            File.WriteAllText(Path.Combine(prep.WorktreePath, "nota.md"), "# nota\n");

            var attempt = await m.TryCommitAndPushBranchAsync(path, "alfa", "lavoro di alfa");

            Assert.IsNotNull(attempt.Pushed, attempt.Error);
            Assert.IsFalse(attempt.NothingToDeliver);
            Assert.IsNull(attempt.Error);
            StringAssert.Contains(Git(origin, "for-each-ref refs/heads/agent").Out, attempt.Pushed.Branch);
        }

        [TestMethod]
        public async Task Say_why_when_the_work_could_not_be_published()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (_, path, origin) = SetupGitProject(ctx, "consegna-rifiutata");
            var m = Manager(ctx);

            var prep = await m.PrepareForRunAsync(path, "alfa", "att1");
            File.WriteAllText(Path.Combine(prep.WorktreePath, "nota.md"), "# nota\n");

            // Origin irraggiungibile: stesso esito pratico di un repository su cui non si può scrivere.
            Directory.Move(origin, origin + ".spostato");

            var attempt = await m.TryCommitAndPushBranchAsync(path, "alfa", "lavoro di alfa");

            Assert.IsNotNull(attempt);
            Assert.IsNull(attempt.Pushed, "non è stato pubblicato");
            Assert.IsFalse(attempt.NothingToDeliver, "un lavoro c'è: non è «niente da consegnare»");
            Assert.IsFalse(string.IsNullOrWhiteSpace(attempt.Error), "l'errore deve dire cosa è successo");
            StringAssert.Contains(attempt.Error, "push");
            Assert.AreEqual(prep.WorktreePath, attempt.WorktreePath, "e dove sta il lavoro");
        }

        [TestMethod]
        public void Tell_the_user_through_the_mailbox_when_the_work_was_not_published()
        {
            var mailbox = new FakeMailbox();
            var reporter = new AgentDeliveryReporter(mailbox, NullLogger<AgentDeliveryReporter>.Instance);

            reporter.ReportNotPublished("/progetto", "alfa",
                new DeliveryAttempt { Error = "il push del ramo 'x' su origin è fallito (permesso negato)", WorktreePath = "/progetto/.worktrees/slot-1" },
                contextId: "ctx-1");

            Assert.AreEqual(1, mailbox.Sent.Count);
            var sent = mailbox.Sent[0];
            Assert.AreEqual("alfa", sent.FromAgent);
            Assert.AreEqual("user", sent.ToAgent, "va all'umano, che è chi deve decidere cosa fare");
            Assert.AreEqual("ctx-1", sent.ContextId, "nella conversazione in cui è nato il lavoro");
            StringAssert.Contains(sent.Body, "permesso negato");
            StringAssert.Contains(sent.Body, "/progetto/.worktrees/slot-1");
        }

        [TestMethod]
        public void Stay_quiet_when_there_is_nothing_to_report()
        {
            var mailbox = new FakeMailbox();
            var reporter = new AgentDeliveryReporter(mailbox, NullLogger<AgentDeliveryReporter>.Instance);

            reporter.ReportNotPublished("/progetto", "alfa", new DeliveryAttempt { NothingToDeliver = true });
            reporter.ReportNotPublished("/progetto", "alfa", null);

            Assert.AreEqual(0, mailbox.Sent.Count, "niente da consegnare non è un errore da riferire");
        }

        // ---- infrastruttura ----

        private sealed class FakeMailbox : IAgentMailbox
        {
            public List<EnqueueRequest> Sent { get; } = new();
            public EnqueueResult Enqueue(EnqueueRequest request)
            {
                Sent.Add(request);
                return new EnqueueResult { Accepted = true };
            }
        }

        private static IAgentWorktreeManager Manager(AgentCityContext ctx)
            => ctx.Factory.Services.GetRequiredService<IAgentWorktreeManager>();

        private static (int Code, string Out) Git(string cwd, string args)
        {
            var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git", Arguments = args, WorkingDirectory = cwd,
                    UseShellExecute = false, RedirectStandardOutput = true,
                    RedirectStandardError = true, CreateNoWindow = true,
                }
            };
            p.StartInfo.EnvironmentVariables["GIT_TERMINAL_PROMPT"] = "0";
            p.Start();
            var o = p.StandardOutput.ReadToEnd();
            p.StandardError.ReadToEnd();
            p.WaitForExit(60000);
            return (p.ExitCode, o);
        }

        private static bool GitAvail() => Git(Path.GetTempPath(), "--version").Code == 0;

        private static (Guid Key, string Path, string Origin) SetupGitProject(AgentCityContext ctx, string name)
        {
            var (key, path) = ctx.SeedProject(name);

            var origin = Path.Combine(ctx.Factory.DataDir, "origins", name + ".git");
            Directory.CreateDirectory(origin);
            Git(origin, "init --bare");
            Git(origin, "symbolic-ref HEAD refs/heads/main");

            Git(path, "init -b main");
            Git(path, "config user.email carlo@test.local");
            Git(path, "config user.name Test");
            Git(path, "config commit.gpgsign false");
            File.WriteAllText(Path.Combine(path, "README.md"), "# doc\n");
            Git(path, "add -A");
            Git(path, "commit -m base");
            Git(path, $"remote add origin \"{origin}\"");
            Git(path, "push -u origin main");

            return (key, path, origin);
        }
    }
}
