using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MdExplorer.Features.Federation;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Service.Models;
using MdExplorer.Services;
using MdExplorer.Services.Federation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Impersonazione utente (test della città): il seam identità risolve la git email reale di
    /// default e l'utente impersonato SOLO in modalità test; gli endpoint elencano i padroni
    /// dell'ownership doc e commutano l'identità. <para>Richiede <c>git</c> nel PATH.</para>
    /// </summary>
    [TestClass]
    public class UserImpersonation_Should
    {
        [TestInitialize]
        public void ResetCwd() => Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        private static void Git(string cwd, string args)
        {
            var p = new Process { StartInfo = new ProcessStartInfo { FileName = "git", Arguments = args, WorkingDirectory = cwd, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true } };
            p.Start(); p.StandardOutput.ReadToEnd(); p.StandardError.ReadToEnd(); p.WaitForExit(60000);
        }

        private static bool GitAvail()
        {
            try { var p = new Process { StartInfo = new ProcessStartInfo { FileName = "git", Arguments = "--version", UseShellExecute = false, RedirectStandardOutput = true, CreateNoWindow = true } }; p.Start(); p.WaitForExit(10000); return p.ExitCode == 0; }
            catch { return false; }
        }

        private static string SetupProject(AgentCityContext ctx, string name)
        {
            var (_, path) = ctx.SeedProject(name);
            Git(path, "init -b main");
            Git(path, "config user.email me@real.local");
            Git(path, "config user.name Me");
            ctx.Factory.Services.GetRequiredService<IProjectMetadataService>()
                .SetAgentCity(path, new AgentCityConfig { Enabled = true, OwnershipDoc = "ownership.md" });
            File.WriteAllText(Path.Combine(path, "ownership.md"), @"---
mde_type: ownership
---
| Ambito | Git Email | Agenti |
|--------|-----------|--------|
| WSAA-TOT | marco@acme.it | javadev |
");
            return path;
        }

        [TestMethod]
        public void Resolve_real_email_by_default_and_impersonated_only_in_test_mode()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var path = SetupProject(ctx, "imp-seam");
            var identity = ctx.Factory.Services.GetRequiredService<IEffectiveOwnerIdentity>();

            var d0 = identity.Resolve(path);
            Assert.AreEqual("me@real.local", d0.Email);
            Assert.IsFalse(d0.Impersonated);
            Assert.AreEqual(FederationRoom.ComputeUserId("me@real.local"), d0.OwnerId);

            // Override senza modalità test → IGNORATO (fail-safe).
            identity.SetImpersonation(path, "marco@acme.it");
            Assert.AreEqual("me@real.local", identity.Resolve(path).Email, "override ignorato con test-mode OFF");

            // Modalità test ON → l'override ha effetto.
            identity.SetTestMode(true);
            var d1 = identity.Resolve(path);
            Assert.AreEqual("marco@acme.it", d1.Email);
            Assert.IsTrue(d1.Impersonated);
            Assert.AreEqual(FederationRoom.ComputeUserId("marco@acme.it"), d1.OwnerId);

            // Torna reale.
            identity.ClearImpersonation(path);
            Assert.AreEqual("me@real.local", identity.Resolve(path).Email);
        }

        [TestMethod]
        public async Task List_users_and_switch_identity_via_endpoints()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var path = SetupProject(ctx, "imp-api");
            var enc = Uri.EscapeDataString(path);

            // GET /users → elenca i padroni del doc, con isMe corretto (io = git email reale).
            var (s, json) = await ctx.GetJson($"/api/A2A/federation/users?projectPath={enc}");
            Assert.AreEqual(HttpStatusCode.OK, s);
            var users = json.RootElement.GetProperty("users").EnumerateArray().ToList();
            Assert.IsTrue(users.Any(u => u.GetProperty("email").GetString() == "marco@acme.it"), "marco è un padrone del doc");
            // Badge presenza: ogni utente porta il flag 'online'; senza relay attivo (test) è false.
            Assert.IsTrue(users.All(u => u.TryGetProperty("online", out var o) && o.ValueKind == System.Text.Json.JsonValueKind.False),
                "senza connessione federata attiva nessun padrone risulta online");

            // POST /impersonate con test-mode OFF → 409 azionabile.
            var (c1, _) = await ctx.PostJson("/api/A2A/federation/impersonate",
                System.Text.Json.JsonSerializer.Serialize(new { projectPath = path, email = "marco@acme.it" }));
            Assert.AreEqual(HttpStatusCode.Conflict, c1);

            // Abilita test-mode.
            await ctx.PostJson("/api/A2A/federation/impersonate/test-mode",
                System.Text.Json.JsonSerializer.Serialize(new { enabled = true }));

            // Email sconosciuta → 422.
            var (c2, _) = await ctx.PostJson("/api/A2A/federation/impersonate",
                System.Text.Json.JsonSerializer.Serialize(new { projectPath = path, email = "ghost@nope.it" }));
            Assert.AreEqual(HttpStatusCode.UnprocessableEntity, c2);

            // Impersona marco → ok.
            var (c3, b3) = await ctx.PostJson("/api/A2A/federation/impersonate",
                System.Text.Json.JsonSerializer.Serialize(new { projectPath = path, email = "marco@acme.it" }));
            Assert.AreEqual(HttpStatusCode.OK, c3, b3);
            StringAssert.Contains(b3, "\"impersonated\":true");
            Assert.AreEqual(path, ctx.FederationSender.LastReconnectedProject, "la stanza è stata riconnessa col nuovo ownerId");

            // Stato: impersonato.
            var (_, statusJson) = await ctx.GetJson($"/api/A2A/federation/impersonate?projectPath={enc}");
            Assert.IsTrue(statusJson.RootElement.GetProperty("impersonated").GetBoolean());

            // DELETE → torna reale.
            var del = await ctx.Client.DeleteAsync($"/api/A2A/federation/impersonate?projectPath={enc}");
            Assert.AreEqual(HttpStatusCode.OK, del.StatusCode);
            var (_, statusJson2) = await ctx.GetJson($"/api/A2A/federation/impersonate?projectPath={enc}");
            Assert.IsFalse(statusJson2.RootElement.GetProperty("impersonated").GetBoolean());
        }
    }
}
