using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Service.Models;
using MdExplorer.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Un agente lavora <b>solo sul computer di chi risponde del suo output</b>.
    /// <para>
    /// Prima un messaggio per nome svegliava il destinatario sul computer di chi lo mandava, con il suo
    /// abbonamento, di chiunque fosse l'agente. Ora, nella città: il mio parte qui; quello di un altro
    /// aspetta (lavora da lui); quello di nessuno non parte finché non viene assegnato. In tutti e due i
    /// casi la persona lo viene a sapere, una volta.
    /// </para>
    /// <para>Richiede <c>git</c> nel PATH: chi sei, per il progetto, è la sua email git.</para>
    /// </summary>
    [TestClass]
    public class AgentWakeRule_Should
    {
        private const string Me = "anna@pentagroup.test";

        [TestInitialize]
        public void ResetCwd() => Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        private static int Git(string cwd, string args)
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
            p.Start();
            p.StandardOutput.ReadToEnd();
            p.StandardError.ReadToEnd();
            p.WaitForExit(20000);
            return p.ExitCode;
        }

        private static bool GitAvail() => Git(Path.GetTempPath(), "--version") == 0;

        private static void WriteOwnership(string path, params (string Email, string Agent)[] rows)
            => File.WriteAllText(Path.Combine(path, "responsabilita.md"),
                "---\nmde_type: ownership\n---\n| Ambito | Git Email | Agenti |\n|--------|-----------|--------|\n" +
                string.Concat(rows.Select((r, i) => $"| Ambito{i} | {r.Email} | {r.Agent} |\n")));

        /// <summary>Una città con tre agenti: uno mio, uno di Marco, uno di nessuno. Tutti e tre abilitati qui.</summary>
        private static (Guid Key, string Path) City(AgentCityContext ctx, string name)
        {
            var (key, path) = ctx.SeedProject(name);
            Git(path, "init -b main");
            Git(path, $"config user.email {Me}");
            foreach (var agent in new[] { "mio", "suo", "orfano" })
                ctx.WriteLlmCitizen(path, agent, "Ruolo " + agent, new[] { "*" });
            ctx.IndexAgentFiles(path);
            foreach (var agent in new[] { "mio", "suo", "orfano" })
                ctx.Trust(path, agent);
            WriteOwnership(path, (Me, "mio"), ("marco@pentagroup.test", "suo"));
            ctx.Factory.Services.GetRequiredService<IProjectMetadataService>()
                .SetAgentCity(path, new AgentCityConfig { Enabled = true, OwnershipDoc = "responsabilita.md" });
            return (key, path);
        }

        private static int NoticesFrom(AgentCityContext ctx, string agent)
            => ctx.Messages().Count(m => m.FromAgent == agent && m.ToAgent == ConversationHopGuard.UserRecipient);

        [TestMethod]
        public async Task Wake_my_agent_here()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var (key, _) = City(ctx, "sveglia-mio");

            var rpc = await GatewayRpc.SendMessage(ctx.Client, key, "mio", "al lavoro");
            Assert.IsFalse(rpc.IsError, $"{rpc.ErrorCode} {rpc.ErrorMessage}");

            var msgs = await ctx.WaitForMessages(m => m.Any(x => x.ToAgent == "mio" && x.State == AgentMessage.StateEnum.Processed));
            Assert.AreEqual(AgentMessage.StateEnum.Processed, msgs.First(x => x.ToAgent == "mio").State);
            Assert.AreEqual(1, ctx.Runner.Calls);
        }

        [TestMethod]
        public async Task Not_wake_here_the_agent_of_someone_else_and_tell_the_person_once()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var (key, _) = City(ctx, "sveglia-suo");

            await GatewayRpc.SendMessage(ctx.Client, key, "suo", "al lavoro");

            var msgs = await ctx.WaitForMessages(m => m.Any(x => x.ToAgent == "suo"
                && x.DeferredReason == AgentMessage.DeferredReasonEnum.OwnerElsewhere));
            var parked = msgs.First(x => x.ToAgent == "suo");
            Assert.AreEqual(AgentMessage.DeferredReasonEnum.OwnerElsewhere, parked.DeferredReason);
            Assert.AreEqual(AgentMessage.StateEnum.Pending, parked.State, "aspetta, non fallisce");
            Assert.AreEqual(0, parked.Attempts, "aspettare non consuma tentativi");

            // Il messaggio viene riguardato ogni pochi secondi: l'avviso alla persona resta uno.
            await Task.Delay(12000);
            Assert.AreEqual(0, ctx.Runner.Calls, "l'agente di Marco non gira sul computer di Anna");
            Assert.AreEqual(1, NoticesFrom(ctx, "suo"));
            var notice = ctx.Messages().First(m => m.FromAgent == "suo" && m.ToAgent == ConversationHopGuard.UserRecipient);
            StringAssert.Contains(notice.Body, "marco@pentagroup.test");
            StringAssert.Contains(notice.Body, "resta in attesa");
        }

        [TestMethod]
        public async Task Not_wake_an_agent_of_nobody_until_it_is_assigned()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var (key, path) = City(ctx, "sveglia-orfano");

            await GatewayRpc.SendMessage(ctx.Client, key, "orfano", "al lavoro");

            var msgs = await ctx.WaitForMessages(m => m.Any(x => x.ToAgent == "orfano"
                && x.DeferredReason == AgentMessage.DeferredReasonEnum.Unassigned));
            Assert.AreEqual(AgentMessage.DeferredReasonEnum.Unassigned, msgs.First(x => x.ToAgent == "orfano").DeferredReason);
            Assert.AreEqual(0, ctx.Runner.Calls);
            StringAssert.Contains(
                ctx.Messages().First(m => m.FromAgent == "orfano" && m.ToAgent == ConversationHopGuard.UserRecipient).Body,
                "non ha un responsabile");

            // Assegnato a me, il messaggio che aspettava parte da solo: non va rimandato.
            WriteOwnership(path, (Me, "mio"), ("marco@pentagroup.test", "suo"), (Me, "orfano"));

            msgs = await ctx.WaitForMessages(m => m.Any(x => x.ToAgent == "orfano" && x.State == AgentMessage.StateEnum.Processed));
            Assert.AreEqual(AgentMessage.StateEnum.Processed, msgs.First(x => x.ToAgent == "orfano").State);
            Assert.AreEqual(1, ctx.Runner.Calls);
        }

        [TestMethod]
        public async Task Refuse_to_launch_by_hand_an_agent_that_is_not_mine()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var (_, path) = City(ctx, "lancio-a-mano");

            foreach (var (agent, expected) in new[] { ("suo", "marco@pentagroup.test"), ("orfano", "non ha un responsabile") })
            {
                var (status, body) = await ctx.PostJson("/api/AgentPrompts/launch", JsonSerializer.Serialize(new
                {
                    projectPath = path,
                    agentFilePath = Path.Combine(path, agent + ".agent.md"),
                    prompt = "al lavoro",
                }));

                Assert.AreEqual(System.Net.HttpStatusCode.Conflict, status, body);
                StringAssert.Contains(body, expected);
            }
            Assert.AreEqual(0, ctx.Runner.Calls);
        }

        [TestMethod]
        public async Task Give_an_agent_of_nobody_to_me_when_I_say_it_is_mine()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var (_, path) = City(ctx, "e-mio");
            Git(path, "config user.name \"Anna Rossi\"");

            var (status, body) = await ctx.PostJson("/api/A2A/owners/assign-to-me",
                JsonSerializer.Serialize(new { projectPath = path, agentNames = new[] { "orfano", "mio" } }));

            Assert.AreEqual(System.Net.HttpStatusCode.OK, status, body);
            StringAssert.Contains(body, "\"kind\":\"mine\"");
            Assert.AreEqual(1, File.ReadAllText(Path.Combine(path, "responsabilita.md")).Split("| mio |").Length - 1, "quello già mio non prende una seconda riga");
            var doc = File.ReadAllText(Path.Combine(path, "responsabilita.md"));
            StringAssert.Contains(doc, $"| orfano | {Me} | orfano |");
            StringAssert.Contains(doc, "marco@pentagroup.test", "le righe degli altri restano");

            var (_, view) = await ctx.GetJson("/api/A2A/owners?projectPath=" + Uri.EscapeDataString(path));
            var kinds = view.RootElement.GetProperty("agents").EnumerateArray()
                .ToDictionary(a => a.GetProperty("agentName").GetString(), a => a.GetProperty("kind").GetString());
            Assert.AreEqual("mine", kinds["mio"]);
            Assert.AreEqual("mine", kinds["orfano"]);
            Assert.AreEqual("someoneElse", kinds["suo"]);
            Assert.AreEqual(Me, view.RootElement.GetProperty("me").GetString());
        }

        [TestMethod]
        public async Task Not_take_an_agent_from_the_person_who_answers_for_it()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var (_, path) = City(ctx, "non-mio");
            var before = File.ReadAllText(Path.Combine(path, "responsabilita.md"));

            var (status, body) = await ctx.PostJson("/api/A2A/owners/assign-to-me",
                JsonSerializer.Serialize(new { projectPath = path, agentNames = new[] { "orfano", "suo" } }));

            Assert.AreEqual(System.Net.HttpStatusCode.Conflict, status, body);
            StringAssert.Contains(body, "marco@pentagroup.test");
            Assert.AreEqual(before, File.ReadAllText(Path.Combine(path, "responsabilita.md")), "il documento non cambia: nemmeno per quello che si poteva prendere");
        }

        [TestMethod]
        public async Task Start_the_document_in_a_city_that_has_none()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var (key, path) = ctx.SeedProject("senza-documento");
            ctx.SetGitEmail(path, Me);
            Git(path, "config user.name \"Anna Rossi\"");
            ctx.WriteLlmCitizen(path, "solo", "Ruolo", new[] { "*" });
            ctx.IndexAgentFiles(path);
            var meta = ctx.Factory.Services.GetRequiredService<IProjectMetadataService>();
            meta.SetAgentCity(path, new AgentCityConfig { Enabled = true });

            var (status, body) = await ctx.PostJson("/api/A2A/owners/assign-to-me",
                JsonSerializer.Serialize(new { projectPath = path, agentNames = new[] { "solo" } }));

            Assert.AreEqual(System.Net.HttpStatusCode.OK, status, body);
            Assert.AreEqual("responsabilita-agenti.md", meta.GetAgentCity(path).OwnershipDoc, "il documento è collegato alla città");
            Assert.IsTrue(meta.GetAgentCity(path).Enabled, "la città resta accesa");
            StringAssert.Contains(File.ReadAllText(Path.Combine(path, "responsabilita-agenti.md")), $"| solo | Anna Rossi | {Me} | solo |");

            // Ora che è mio lo posso abilitare, e parte.
            var (trust, _) = await ctx.PostJson("/api/A2A/agents/trust", JsonSerializer.Serialize(new { projectPath = path, agentName = "solo" }));
            Assert.AreEqual(System.Net.HttpStatusCode.OK, trust);
            await GatewayRpc.SendMessage(ctx.Client, key, "solo", "al lavoro");
            await ctx.WaitForMessages(m => m.Any(x => x.ToAgent == "solo" && x.State == AgentMessage.StateEnum.Processed));
            Assert.AreEqual(1, ctx.Runner.Calls);
        }

        [TestMethod]
        public async Task Not_let_me_enable_an_agent_that_is_not_mine()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("abilita-non-mio");
            ctx.SetGitEmail(path, Me);
            ctx.WriteLlmCitizen(path, "suo", "Ruolo", new[] { "*" });
            ctx.WriteLlmCitizen(path, "orfano", "Ruolo", new[] { "*" });
            ctx.IndexAgentFiles(path);
            WriteOwnership(path, ("marco@pentagroup.test", "suo"));
            ctx.Factory.Services.GetRequiredService<IProjectMetadataService>()
                .SetAgentCity(path, new AgentCityConfig { Enabled = true, OwnershipDoc = "responsabilita.md" });

            foreach (var (agent, expected) in new[] { ("suo", "marco@pentagroup.test"), ("orfano", "non ha un responsabile") })
            {
                var (status, body) = await ctx.PostJson("/api/A2A/agents/trust", JsonSerializer.Serialize(new { projectPath = path, agentName = agent }));
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, status, body);
                StringAssert.Contains(body, expected);
            }
        }
    }
}
