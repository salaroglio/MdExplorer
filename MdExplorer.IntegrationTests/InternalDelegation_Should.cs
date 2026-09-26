using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Service.Models;
using MdExplorer.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Delega <b>interna</b>: un agente chiede un intervento su un ambito il cui responsabile è
    /// l'umano locale.
    /// <para>
    /// Prima usciva sul relay e tornava indietro alla stessa città (il relay non esclude il
    /// mittente), aprendo un gate umano per farsi autorizzare da sé stessi. Ora resta in
    /// mailbox: <b>niente relay</b>, <b>niente gate</b>, <b>stessa conversazione</b> — così gli
    /// hop continuano ad accumularsi e non si può aggirare l'anti-loop delegando in cerchio.
    /// </para>
    /// </summary>
    [TestClass]
    public class InternalDelegation_Should
    {
        private const string OwnershipDoc = @"---
mde_type: ownership
---
| Ambito | Responsabile | Git Email | Agenti |
|--------|--------------|-----------|--------|
| Wiki | Io | {EMAIL} | curator |
";

        private static void Git(string cwd, string args)
        {
            var p = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "git", Arguments = args, WorkingDirectory = cwd,
                UseShellExecute = false, RedirectStandardOutput = true,
                RedirectStandardError = true, CreateNoWindow = true,
            });
            p.WaitForExit(20000);
        }

        /// <summary>Città attiva + ownership che assegna l'ambito all'email git LOCALE.</summary>
        private static void SeedOwnership(AgentCityContext ctx, string path)
        {
            // L'identità-padrone si legge dalla git email del progetto: senza repo non esiste.
            Git(path, "init -b main");
            Git(path, "config user.email padrone@test.local");
            Git(path, "config user.name Padrone");

            var email = ctx.Factory.Services
                .GetRequiredService<MdExplorer.Services.Federation.IEffectiveOwnerIdentity>()
                .ResolveEmail(path);
            Assert.IsFalse(string.IsNullOrWhiteSpace(email), "serve una git email locale per il test");

            File.WriteAllText(Path.Combine(path, "ownership.md"), OwnershipDoc.Replace("{EMAIL}", email));
            ctx.Factory.Services.GetRequiredService<IProjectMetadataService>()
                .SetAgentCity(path, new AgentCityConfig { Enabled = true, OwnershipDoc = "ownership.md" });
        }

        [TestMethod]
        public async Task Keep_the_work_local_without_touching_the_relay()
        {
            using var ctx = new AgentCityContext();
            var (key, path) = ctx.SeedProject("delega-interna");
            ctx.WriteLlmCitizen(path, "mittente", "Chi delega", new[] { "*" });
            ctx.WriteLlmCitizen(path, "curator", "Chi riceve", new[] { "*" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "mittente");
            ctx.Trust(path, "curator");
            SeedOwnership(ctx, path);

            // Il mittente si sveglia e, DURANTE il turno, chiede un intervento sull'ambito 'Wiki'
            // — che risulta di proprietà dell'umano locale.
            HttpResponseMessage reply = null;
            ctx.Runner.Behavior = async (req, _) =>
            {
                var token = req.Environment[MdExplorer.Features.Agents.LlmAgentWaker.EnvRunToken];
                using var http = ctx.Factory.CreateClient();
                http.DefaultRequestHeaders.Add("X-MDE-Run-Token", token);
                reply = await http.PostAsync("/api/A2A/messages/request-intervention",
                    new StringContent("{\"scope\":\"Wiki\",\"message\":\"pensaci tu\"}",
                        Encoding.UTF8, "application/json"));
                return "delegato";
            };

            var rpc = await GatewayRpc.SendMessage(ctx.Client, key, "mittente", "smista questo");
            Assert.IsFalse(rpc.IsError, $"{rpc.ErrorCode} {rpc.ErrorMessage}");

            // Il destinatario deve ricevere un messaggio: la delega è arrivata via mailbox.
            var msgs = await ctx.WaitForMessages(m => m.Any(x => x.ToAgent == "curator"));

            Assert.IsNotNull(reply, "il turno non ha chiamato request-intervention");
            Assert.AreEqual(System.Net.HttpStatusCode.OK, reply.StatusCode,
                await reply.Content.ReadAsStringAsync());

            var body = await reply.Content.ReadAsStringAsync();
            StringAssert.Contains(body, "\"local\":true", "la delega dev'essere dichiarata locale");

            var delivered = msgs.First(x => x.ToAgent == "curator");
            Assert.AreEqual("mittente", delivered.FromAgent, "il mittente è quello certificato dal token");

            // NIENTE relay: il sender federato non dev'essere stato toccato.
            Assert.IsNull(ctx.Factory.FederationSender.LastPayload,
                "una delega su un ambito proprio non deve uscire sul relay");

            // NIENTE gate: nessuna richiesta federata da autorizzare a sé stessi.
            using var scope = ctx.Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MdExplorer.Abstractions.DB.IUserSettingsDB>();
            db.BeginTransaction();
            var gates = db.GetDal<FederationRequest>().GetList().Count();
            db.Commit();
            Assert.AreEqual(0, gates, "nessun gate umano per una delega verso sé stessi");
        }

        [TestMethod]
        public async Task Record_the_scope_on_the_conversation()
        {
            using var ctx = new AgentCityContext();
            var (key, path) = ctx.SeedProject("delega-ambito");
            ctx.WriteLlmCitizen(path, "mittente", "Chi delega", new[] { "*" });
            ctx.WriteLlmCitizen(path, "curator", "Chi riceve", new[] { "*" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "mittente");
            ctx.Trust(path, "curator");
            SeedOwnership(ctx, path);

            ctx.Runner.Behavior = async (req, _) =>
            {
                var token = req.Environment[MdExplorer.Features.Agents.LlmAgentWaker.EnvRunToken];
                using var http = ctx.Factory.CreateClient();
                http.DefaultRequestHeaders.Add("X-MDE-Run-Token", token);
                await http.PostAsync("/api/A2A/messages/request-intervention",
                    new StringContent("{\"scope\":\"Wiki\",\"message\":\"pensaci tu\"}",
                        Encoding.UTF8, "application/json"));
                return "delegato";
            };

            var rpc = await GatewayRpc.SendMessage(ctx.Client, key, "mittente", "smista questo");
            Assert.IsFalse(rpc.IsError);
            await ctx.WaitForMessages(m => m.Any(x => x.ToAgent == "curator"));

            // L'ambito resta attaccato alla conversazione: senza, la delega sarebbe
            // indistinguibile da un messaggio qualunque e si perderebbe il PERCHÉ del risveglio.
            using var scope = ctx.Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MdExplorer.Abstractions.DB.IUserSettingsDB>();
            db.BeginTransaction();
            var convs = db.GetDal<AgentConversation>().GetList().ToList();
            db.Commit();

            Assert.IsTrue(convs.Any(c => c.Scope == "Wiki"),
                "la conversazione deve portare l'ambito che l'ha generata");
        }
    }
}
