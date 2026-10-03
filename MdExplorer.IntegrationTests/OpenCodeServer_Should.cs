using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services.AI.OpenCode;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Il server di opencode gestito da MdExplorer: acceso alla prima richiesta, protetto da una
    /// password casuale, spento con l'applicazione.
    /// <para>Sprint: docs-internal/Sprints/2026-09-13-MarkAgent-Motore-OpenCode.md, fase F3.</para>
    /// </summary>
    [TestClass]
    public class OpenCodeServer_Should
    {
        private static OpenCodeServer New() => new(NullLogger<OpenCodeServer>.Instance);

        /// <summary>
        /// Le prove che accendono il server hanno bisogno di opencode installato. Se non c'è, il
        /// test non è "passato": è <b>inconcludente</b>, e lo dice.
        /// </summary>
        private static void RequireOpenCode()
        {
            if (!OpenCodeProcessLauncher.IsResolvable())
            {
                Assert.Inconclusive(
                    "opencode non è nel PATH di questo processo: la prova col server vero non è stata eseguita. " +
                    "`npm i -g opencode-ai` per eseguirla.");
            }
        }

        [TestMethod]
        public void Authenticate_the_way_the_server_wants()
        {
            var auth = OpenCodeServer.BuildAuthentication("segreto");

            Assert.AreEqual("Basic", auth.Scheme);
            var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(auth.Parameter));
            // ⚠️ Misurato il 21/09/2026: l'utente DEVE essere 'opencode'. Con 'x', 'admin' o
            // l'utente vuoto la stessa password prende 401, e un Bearer pure.
            Assert.AreEqual("opencode:segreto", decoded);
        }

        [TestMethod]
        public void Say_where_to_find_opencode_when_it_is_missing()
        {
            // Non si accende niente: si controlla solo che il messaggio nomini il problema.
            if (OpenCodeProcessLauncher.IsResolvable())
            {
                var path = OpenCodeProcessLauncher.ResolvePath();
                StringAssert.Contains(path, OpenCodeProcessLauncher.ExecutableName);
                return;
            }

            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => OpenCodeProcessLauncher.BuildServeStartInfo("x"));
            StringAssert.Contains(ex.Message, "opencode");
            StringAssert.Contains(ex.Message, "PATH");
        }

        [TestMethod]
        public void Refuse_to_start_without_a_password()
        {
            Assert.ThrowsException<ArgumentException>(
                () => OpenCodeProcessLauncher.BuildServeStartInfo("   "));
        }

        [TestMethod]
        public void Not_be_running_before_anyone_asks()
        {
            using var server = New();
            Assert.IsFalse(server.IsRunning, "il server nasce alla prima richiesta, non prima");
            Assert.IsNull(server.BaseAddress);
        }

        [TestMethod]
        public async Task Start_on_demand_and_answer_only_with_the_password()
        {
            RequireOpenCode();
            using var server = New();
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));

            var address = await server.EnsureRunningAsync(cts.Token);

            Assert.IsTrue(server.IsRunning);
            Assert.AreEqual("127.0.0.1", address.Host, "il server non deve affacciarsi fuori dalla macchina");
            Assert.IsTrue(address.Port > 0);

            Assert.IsTrue(await server.IsHealthyAsync(cts.Token), "il server acceso deve rispondere davvero");

            // Senza credenziali: 401. La password non è un ornamento.
            using var naked = new HttpClient { BaseAddress = address, Timeout = TimeSpan.FromSeconds(20) };
            using var res = await naked.GetAsync("/config/providers", cts.Token);
            Assert.AreEqual(HttpStatusCode.Unauthorized, res.StatusCode);

            // Una seconda richiesta non accende un secondo server.
            var again = await server.EnsureRunningAsync(cts.Token);
            Assert.AreEqual(address, again);

            await server.StopAsync(CancellationToken.None);
            Assert.IsFalse(server.IsRunning, "StopAsync spegne il server");

            using var afterStop = new HttpClient { BaseAddress = address, Timeout = TimeSpan.FromSeconds(5) };
            await Assert.ThrowsExceptionAsync<HttpRequestException>(
                () => afterStop.GetAsync("/config/providers"),
                "dopo lo spegnimento la porta non deve rispondere piu'");
        }
    }
}
