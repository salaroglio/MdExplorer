using System;
using System.Threading.Tasks;
using MdExplorer.Features.Services.AI.ClaudeCode;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.ClaudeCode
{
    /// <summary>
    /// La riga di comando di Claude Code non è un dettaglio: ogni flag qui dentro è la
    /// conseguenza di una prova fatta sul campo, e toglierne uno cambia il comportamento in
    /// modi che <b>non</b> danno errore — cambiano solo, in silenzio, quello che arriva
    /// all'utente. Questi test bloccano proprio i silenzi:
    /// <list type="bullet">
    /// <item><description>senza <c>--verbose</c> e <c>--include-partial-messages</c> lo stream
    ///   non porta i delta e la chat sembra ferma finché non finisce il turno;</description></item>
    /// <item><description><c>--tools ""</c> non deve comparire mai: con zero tool il modello
    ///   non rifiuta, <b>si inventa</b> la chiamata e finge l'output;</description></item>
    /// <item><description><c>--mcp-config</c> senza <c>--strict-mcp-config</c> fa ereditare i
    ///   server MCP personali dell'utente.</description></item>
    /// </list>
    /// Sono test puri: costruiscono una stringa, non lanciano nulla e non spendono un token.
    /// </summary>
    [TestClass]
    public class ClaudeCodeArguments_Should
    {
        [TestMethod]
        public void Chiedere_la_sessione_bidirezionale_in_ndjson()
        {
            var args = ClaudeCodeSession.BuildArguments("sonnet", ClaudeCodeSessionOptions.Default);

            StringAssert.Contains(args, "-p");
            StringAssert.Contains(args, "--input-format stream-json");
            StringAssert.Contains(args, "--output-format stream-json");
        }

        [TestMethod]
        public void Chiedere_i_delta_altrimenti_lo_streaming_non_esiste()
        {
            var args = ClaudeCodeSession.BuildArguments("sonnet", ClaudeCodeSessionOptions.Default);

            // Senza --verbose il print mode riduce lo stream; senza --include-partial-messages
            // arrivano solo i messaggi interi a fine blocco.
            StringAssert.Contains(args, "--verbose");
            StringAssert.Contains(args, "--include-partial-messages");
        }

        [TestMethod]
        public void Chiedere_la_modalita_permessi_che_il_cli_onora_davvero()
        {
            var args = ClaudeCodeSession.BuildArguments("sonnet", ClaudeCodeSessionOptions.Default);

            // `dontAsk` è onorata (l'init la riporta). `manual` verrebbe ignorata in silenzio.
            StringAssert.Contains(args, "--permission-mode dontAsk");
            Assert.IsFalse(args.Contains("--permission-mode manual"),
                "manual viene ignorata in silenzio dal CLI: non deve mai essere chiesta");
        }

        [TestMethod]
        public void Passare_il_modello_solo_quando_e_scelto()
        {
            StringAssert.Contains(
                ClaudeCodeSession.BuildArguments("opus", ClaudeCodeSessionOptions.Default),
                "--model opus");

            // "auto" e vuoto significano "decidi tu": passare un --model inventato è peggio
            // che non passarlo.
            Assert.IsFalse(ClaudeCodeSession.BuildArguments("auto", ClaudeCodeSessionOptions.Default).Contains("--model"));
            Assert.IsFalse(ClaudeCodeSession.BuildArguments(null, ClaudeCodeSessionOptions.Default).Contains("--model"));
            Assert.IsFalse(ClaudeCodeSession.BuildArguments("  ", ClaudeCodeSessionOptions.Default).Contains("--model"));
        }

        [TestMethod]
        public void Non_disabilitare_mai_tutti_i_tool_perche_il_modello_allucina()
        {
            foreach (var policy in new[] { ClaudeCodeToolPolicy.Full, ClaudeCodeToolPolicy.NoExecution })
            {
                var args = ClaudeCodeSession.BuildArguments("sonnet",
                    new ClaudeCodeSessionOptions { ToolPolicy = policy });

                Assert.IsFalse(args.Contains("--tools"),
                    $"policy {policy}: con zero tool il modello finge le chiamate invece di rifiutare");
            }
        }

        [TestMethod]
        public void Vietare_lesecuzione_solo_quando_la_policy_lo_chiede()
        {
            var pieno = ClaudeCodeSession.BuildArguments("sonnet",
                new ClaudeCodeSessionOptions { ToolPolicy = ClaudeCodeToolPolicy.Full });
            Assert.IsFalse(pieno.Contains("--disallowedTools"),
                "la policy Full è la parità con Copilot: nessun divieto");

            var senzaEsecuzione = ClaudeCodeSession.BuildArguments("sonnet",
                new ClaudeCodeSessionOptions { ToolPolicy = ClaudeCodeToolPolicy.NoExecution });
            StringAssert.Contains(senzaEsecuzione, "--disallowedTools Bash");
        }

        [TestMethod]
        public void Legare_il_file_mcp_alla_modalita_stretta()
        {
            var args = ClaudeCodeSession.BuildArguments("sonnet",
                new ClaudeCodeSessionOptions { McpConfigPath = "/tmp/mde-mcp.json" });

            StringAssert.Contains(args, "--mcp-config /tmp/mde-mcp.json");
            // Senza questo, ai nostri server si sommano quelli personali dell'utente.
            StringAssert.Contains(args, "--strict-mcp-config");
        }

        [TestMethod]
        public void Non_nominare_mcp_quando_non_ce_nessun_file()
        {
            var args = ClaudeCodeSession.BuildArguments("sonnet", ClaudeCodeSessionOptions.Default);

            Assert.IsFalse(args.Contains("--mcp-config"));
            Assert.IsFalse(args.Contains("--strict-mcp-config"));
        }

        [TestMethod]
        public void Riprendere_la_conversazione_quando_le_viene_dato_un_id()
        {
            var args = ClaudeCodeSession.BuildArguments("sonnet",
                new ClaudeCodeSessionOptions { ResumeSessionId = "576ee202-f406-4ab1-b1c5-41efa2190e1d" });

            StringAssert.Contains(args, "--resume 576ee202-f406-4ab1-b1c5-41efa2190e1d");
        }

        [TestMethod]
        public void Spegnere_memoria_e_personalizzazioni_solo_su_richiesta()
        {
            Assert.IsFalse(
                ClaudeCodeSession.BuildArguments("sonnet", ClaudeCodeSessionOptions.Default).Contains("--bare"),
                "il default legge il CLAUDE.md del progetto: --bare va chiesto");

            StringAssert.Contains(
                ClaudeCodeSession.BuildArguments("sonnet", new ClaudeCodeSessionOptions { Bare = true }),
                "--bare");
        }

        /// <summary>
        /// <c>--bare</c> spegne la memoria automatica ma spegne <b>anche</b> la lettura di
        /// OAuth e portachiavi: con l'accesso via abbonamento il CLI risponde «Not logged in».
        /// Il rifiuto deve arrivare <b>prima</b> di lanciare il processo, altrimenti l'utente
        /// vede un errore di login al posto della causa vera. Il test non lancia nulla:
        /// la guardia scatta prima dello spawn.
        /// </summary>
        [TestMethod]
        public async Task Rifiutare_bare_senza_api_key_prima_di_lanciare_il_processo()
        {
            var precedente = Environment.GetEnvironmentVariable(ClaudeCodeSessionOptions.ApiKeyEnvVariable);
            Environment.SetEnvironmentVariable(ClaudeCodeSessionOptions.ApiKeyEnvVariable, null);
            try
            {
                var session = new ClaudeCodeSession(
                    NullLogger<ClaudeCodeSession>.Instance, null, "haiku",
                    new ClaudeCodeSessionOptions { Bare = true });

                var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                    () => session.StartAsync());

                StringAssert.Contains(ex.Message, ClaudeCodeSessionOptions.ApiKeyEnvVariable);
                StringAssert.Contains(ex.Message, "abbonamento");
            }
            finally
            {
                Environment.SetEnvironmentVariable(ClaudeCodeSessionOptions.ApiKeyEnvVariable, precedente);
            }
        }

        [TestMethod]
        public void Proteggere_con_le_virgolette_i_percorsi_con_spazi()
        {
            var args = ClaudeCodeSession.BuildArguments("sonnet",
                new ClaudeCodeSessionOptions { McpConfigPath = @"C:\Program Files\mde\mcp.json" });

            StringAssert.Contains(args, "\"C:\\Program Files\\mde\\mcp.json\"");
        }
    }
}
