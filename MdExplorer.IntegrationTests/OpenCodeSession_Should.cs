using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services.AI.OpenCode;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Un turno di MarkAgent con opencode: frammenti mentre la risposta nasce, testo autorevole
    /// a fine turno, attività sui tool, consuntivo, Stop.
    /// <para>Sprint: docs-internal/Sprints/2026-09-13-MarkAgent-Motore-OpenCode.md, fase F4.</para>
    /// </summary>
    [TestClass]
    public class OpenCodeSession_Should
    {
        private string _dir;

        [TestInitialize]
        public void Setup()
        {
            _dir = Path.Combine(Path.GetTempPath(), "mde-opencode", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dir);
            File.WriteAllText(Path.Combine(_dir, "README.md"),
                "# Progetto di prova\n\nLa parola segreta è BAOBAB.\n");
        }

        [TestCleanup]
        public void Cleanup()
        {
            try { if (Directory.Exists(_dir)) Directory.Delete(_dir, true); }
            catch (IOException) { }
        }

        private static void RequireOpenCode()
        {
            if (!OpenCodeProcessLauncher.IsResolvable())
            {
                Assert.Inconclusive(
                    "opencode non è nel PATH di questo processo: la prova col server vero non è stata eseguita.");
            }
        }

        private static OpenCodeServer NewServer() => new(NullLogger<OpenCodeServer>.Instance);

        private OpenCodeSession NewSession(OpenCodeServer server, string model = "opencode/big-pickle")
            => new(NullLogger<OpenCodeSession>.Instance, server, _dir, model);

        [TestMethod]
        public void Split_the_model_into_provider_and_name()
        {
            var (provider, model) = OpenCodeSession.SplitModel("opencode/big-pickle");
            Assert.AreEqual("opencode", provider);
            Assert.AreEqual("big-pickle", model);

            // Il nome del modello può contenere altri slash: si taglia solo al primo.
            var (p2, m2) = OpenCodeSession.SplitModel("anthropic/claude/sonnet-5");
            Assert.AreEqual("anthropic", p2);
            Assert.AreEqual("claude/sonnet-5", m2);
        }

        [TestMethod]
        public void Refuse_a_model_without_a_provider()
        {
            // Niente indovinelli: 'big-pickle' senza provider non diventa 'opencode/big-pickle'
            // per conto suo, perché il provider giusto dipende da come è configurato opencode.
            foreach (var bad in new[] { "big-pickle", "/big-pickle", "opencode/", "" })
            {
                Assert.ThrowsException<ArgumentException>(() => OpenCodeSession.SplitModel(bad), $"'{bad}'");
            }
        }

        [TestMethod]
        public void Refuse_a_folder_that_is_not_there()
        {
            using var server = NewServer();
            var ex = Assert.ThrowsException<ArgumentException>(() => new OpenCodeSession(
                NullLogger<OpenCodeSession>.Instance, server,
                Path.Combine(_dir, "questa-non-esiste"), "opencode/big-pickle"));
            StringAssert.Contains(ex.Message, "non esiste");
        }

        [TestMethod]
        public async Task Answer_streaming_and_tell_what_it_spent()
        {
            RequireOpenCode();
            using var server = NewServer();
            await using var session = NewSession(server);
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(4));

            var streamed = new StringBuilder();
            var kinds = new List<string>();
            await foreach (var chunk in session.PromptAsync("Rispondi con una sola parola: pronto", cts.Token))
            {
                kinds.Add(chunk.Kind);
                if (chunk.Kind == OpenCodeChunk.KindMessage) streamed.Append(chunk.Text);
            }

            Assert.IsTrue(kinds.Contains(OpenCodeChunk.KindMessage),
                "nessun frammento di testo: lo streaming non funziona. Tipi visti: " + string.Join(",", kinds.Distinct()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(session.LastTurnText), "manca il testo autorevole del turno");
            StringAssert.Contains(session.LastTurnText.ToLowerInvariant(), "pronto");

            // I frammenti raccontano lo stesso testo della risposta sincrona: se divergessero,
            // la chat mostrerebbe una cosa e la cronologia ne salverebbe un'altra.
            Assert.AreEqual(session.LastTurnText.Trim(), streamed.ToString().Trim());

            var usage = session.LastTurnUsage;
            Assert.IsNotNull(usage, "manca il consuntivo del turno");
            Assert.AreEqual("opencode", usage.ProviderId);
            Assert.AreEqual("big-pickle", usage.ModelId);
            Assert.IsTrue(usage.OutputTokens > 0, "i token in uscita non possono essere zero per una risposta");
            Assert.IsNotNull(usage.CostUsd, "il costo deve esserci, anche quando è zero");
        }

        /// <summary>
        /// ⚠️ Regressione vera, vista nell'app il 21/09/2026: il ragionamento del modello
        /// finiva <b>dentro</b> la risposta in chat. Nei delta il campo <c>field</c> vale sempre
        /// «text» — è il nome della proprietà della parte, non la sua natura — e a distinguere
        /// risposta e ragionamento è il <b>tipo della parte</b>.
        /// </summary>
        [TestMethod]
        public async Task Keep_the_reasoning_out_of_the_answer()
        {
            RequireOpenCode();
            using var server = NewServer();
            await using var session = NewSession(server);
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(4));

            var answer = new StringBuilder();
            var thinking = new StringBuilder();
            await foreach (var chunk in session.PromptAsync(
                "Pensa bene e poi rispondi con una sola parola: pronto", cts.Token))
            {
                if (chunk.Kind == OpenCodeChunk.KindMessage) answer.Append(chunk.Text);
                else if (chunk.Kind == OpenCodeChunk.KindThinking) thinking.Append(chunk.Text);
            }

            if (thinking.Length == 0)
            {
                Assert.Inconclusive("questo giro il modello non ha ragionato: la prova non dice niente");
            }

            // Il ragionamento va sul suo canale, e NON nella risposta.
            Assert.AreEqual(session.LastTurnText.Trim(), answer.ToString().Trim(),
                "il testo mostrato non coincide con la risposta del server: dentro c'è finito altro");
            Assert.IsFalse(answer.ToString().Contains(thinking.ToString().Substring(0, Math.Min(30, thinking.Length))),
                "il ragionamento è finito dentro la risposta");
        }

        [TestMethod]
        public async Task Keep_the_conversation_between_turns()
        {
            RequireOpenCode();
            using var server = NewServer();
            await using var session = NewSession(server);
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));

            await foreach (var _ in session.PromptAsync("Ricorda questo numero: 4127. Rispondi solo 'ok'.", cts.Token)) { }
            var first = session.SessionId;

            await foreach (var _ in session.PromptAsync("Qual è il numero che ti ho detto? Rispondi col solo numero.", cts.Token)) { }

            Assert.AreEqual(first, session.SessionId, "il secondo turno deve stare nella stessa sessione");
            StringAssert.Contains(session.LastTurnText, "4127",
                "la conversazione non ha memoria: " + session.LastTurnText);
        }

        [TestMethod]
        public async Task Say_which_tools_it_is_using()
        {
            RequireOpenCode();
            using var server = NewServer();
            await using var session = NewSession(server);
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(4));

            var tools = new List<string>();
            await foreach (var chunk in session.PromptAsync(
                "Leggi il file README.md di questa cartella e dimmi la parola segreta.", cts.Token))
            {
                if (chunk.Kind == OpenCodeChunk.KindTool) tools.Add(chunk.Text);
            }

            Assert.IsTrue(tools.Count > 0, "nessuna attività sui tool, ma il file andava letto");
            Assert.IsTrue(tools.Any(t => t.Contains("read", StringComparison.OrdinalIgnoreCase)),
                "atteso il tool 'read'. Visti: " + string.Join(" / ", tools));
            StringAssert.Contains(session.LastTurnText.ToUpperInvariant(), "BAOBAB");
        }

        [TestMethod]
        public async Task Stop_when_the_user_says_stop()
        {
            RequireOpenCode();
            using var server = NewServer();
            await using var session = NewSession(server);
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(4));

            using var stop = new CancellationTokenSource();
            var chunks = 0;
            var cancelled = false;
            try
            {
                await foreach (var chunk in session.PromptAsync(
                    "Scrivi un racconto lunghissimo sulla storia della scrittura, almeno duemila parole.", stop.Token))
                {
                    chunks++;
                    if (chunks == 1) stop.Cancel();   // appena comincia a parlare, Stop
                }
            }
            catch (OperationCanceledException)
            {
                cancelled = true;
            }

            Assert.IsTrue(cancelled, "lo Stop deve interrompere la lettura del turno");
            Assert.IsTrue(chunks >= 1, "qualcosa era già arrivato prima dello Stop");

            // La sessione resta viva: dopo uno Stop si continua a parlare, non si ricomincia.
            await foreach (var _ in session.PromptAsync("Rispondi solo: ok", cts.Token)) { }
            StringAssert.Contains(session.LastTurnText.ToLowerInvariant(), "ok");
        }
    }
}
