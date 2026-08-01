using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Features.Agents;
using MdExplorer.Services.AgentRun;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Fase B.2 — il ponte verso il <b>nostro stesso</b> server MCP. Fa partire il server vero
    /// via stdio, ne legge i tool e li filtra col catalogo.
    /// <para>
    /// Non è un test di cablaggio: se il server non parte, o se i nomi dei tool cambiano, o se
    /// il filtro lascia passare qualcosa che non dovrebbe, qui si vede. È la controparte
    /// eseguibile dell'affermazione "una sola definizione dei tool, quella del server MCP".
    /// </para>
    /// </summary>
    [TestClass]
    public class AgentMcpTools_Should
    {
        private static AgentMcpToolProvider Provider()
            => new AgentMcpToolProvider(NullLogger<AgentMcpToolProvider>.Instance);

        /// <summary>Ambiente minimo di un turno: senza RunToken i tool della città rifiutano, ma
        /// esistono e si elencano — che è ciò che questo test verifica.</summary>
        private static Dictionary<string, string> Env() => new()
        {
            ["MDE_AGENT_NAME"] = "test-agent",
            ["MDE_PROJECT_PATH"] = "/tmp/nonesiste",
        };

        [TestMethod]
        public async Task Read_the_tools_from_the_real_mcp_server()
        {
            AgentToolSet set;
            try
            {
                set = await Provider().OpenAsync(Env(), new[] { "read", "write" }, trusted: true);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Server MCP"))
            {
                Assert.Inconclusive("Eseguibile MCP non presente in questo ambiente di build.");
                return;
            }

            await using (set)
            {
                Assert.IsTrue(set.Decisions.Count > 0, "il server deve offrire dei tool");

                // I cinque della città vengono dal server, non da una nostra seconda definizione.
                foreach (var cityTool in new[] { "list_agents", "send_agent_message", "request_intervention", "assert_learned_fact", "query_agent_memory" })
                    Assert.IsTrue(set.Decisions.Any(d => d.Tool == cityTool),
                        $"'{cityTool}' deve arrivare dal server MCP");
            }
        }

        [TestMethod]
        public async Task Withhold_writing_and_outbound_from_an_untrusted_agent()
        {
            AgentToolSet set;
            try
            {
                set = await Provider().OpenAsync(Env(), new[] { "read", "write" }, trusted: false);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Server MCP"))
            {
                Assert.Inconclusive("Eseguibile MCP non presente in questo ambiente di build.");
                return;
            }

            await using (set)
            {
                var exposed = set.Functions.Select(f => f.Name).ToList();

                // La cittadinanza è read-only: leggere sì...
                CollectionAssert.Contains(exposed, "list_agents");
                CollectionAssert.Contains(exposed, "query_agent_memory");

                // ...agire no, finché l'umano non si fida.
                CollectionAssert.DoesNotContain(exposed, "send_agent_message");
                CollectionAssert.DoesNotContain(exposed, "request_intervention");
                CollectionAssert.DoesNotContain(exposed, "assert_learned_fact");

                Assert.IsTrue(set.Decisions.Any(d =>
                        d.Tool == "send_agent_message" && d.Access == ToolAccess.ApprovalRequired),
                    "il rifiuto è motivato, non silenzioso");
            }
        }

        [TestMethod]
        public async Task Withhold_memory_writing_when_the_manifest_does_not_declare_it()
        {
            AgentToolSet set;
            try
            {
                // Fidato, ma dichiara solo lettura: la fiducia autorizza il dichiarato, non tutto.
                set = await Provider().OpenAsync(Env(), new[] { "read" }, trusted: true);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Server MCP"))
            {
                Assert.Inconclusive("Eseguibile MCP non presente in questo ambiente di build.");
                return;
            }

            await using (set)
            {
                var exposed = set.Functions.Select(f => f.Name).ToList();
                CollectionAssert.DoesNotContain(exposed, "assert_learned_fact");
                CollectionAssert.Contains(exposed, "query_agent_memory");

                // Fidato: le azioni verso l'esterno restano concesse (non dipendono dal manifesto).
                CollectionAssert.Contains(exposed, "send_agent_message");
            }
        }
    }
}
