using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Services.AI.ChatHistory;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.ChatHistory;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Il blocco titolo/dettaglio col servizio vero (sprint 2026-11-24-Chat-Memorizzate, F5): il client non lo vede, la riga
    /// della cronologia prende titolo e dettaglio, il turno dopo li ritrova nelle istruzioni, e quando qualcosa non va lo si dice.
    /// </summary>
    [TestClass]
    public class ChatThreadBlockFlow_Should
    {
        private sealed class CapturingProxy : IClientProxy
        {
            public readonly List<(string Method, object[] Args)> Sent = new();
            public Task SendCoreAsync(string method, object[] args, CancellationToken cancellationToken = default)
            {
                lock (Sent) Sent.Add((method, args));
                return Task.CompletedTask;
            }
            public string Text => string.Concat(Sent.Where(s => s.Method == "ReceiveStreamChunk").Select(s => (string)s.Args[0]));
            public List<string> System => Sent.Where(s => s.Method == "ReceiveMessage").Select(s => (string)s.Args[1]).ToList();
        }

        private static async Task<(CapturingProxy client, ChatTurnRecording turn, string instruction)> Turn(
            ChatHistoryRecorder recorder, string projectPath, string answer)
        {
            var turn = recorder.BeginTurn("conn", projectPath, "claudecode", "sonnet", "default");
            turn.UserPrompt("domanda");
            var instruction = turn.ThreadInstruction();
            var client = new CapturingProxy();
            IClientProxy proxy = new RecordingClientProxy(client, turn, "default");
            // Il motore manda la risposta a pezzi piccoli: il marcatore si spezza.
            for (var i = 0; i < answer.Length; i += 4)
                await proxy.SendAsync("ReceiveStreamChunk", answer.Substring(i, Math.Min(4, answer.Length - i)), "default");
            await proxy.SendAsync("StreamComplete", "default");
            turn.Complete();
            return (client, turn, instruction);
        }

        private static ChatThread Row(AgentCityContext ctx, Guid id)
        {
            using var scope = ctx.Factory.Services.CreateScope();
            return scope.ServiceProvider.GetRequiredService<IUserSettingsDB>().GetDal<ChatThread>().GetList().Single(t => t.Id == id);
        }

        [TestMethod]
        public async Task Hide_the_block_and_keep_title_and_detail_in_the_row()
        {
            using var ctx = new AgentCityContext();
            var (_, projectPath) = ctx.SeedProject("thread-block");
            var recorder = ctx.Factory.Services.GetRequiredService<ChatHistoryRecorder>();
            var files = ctx.Factory.Services.GetRequiredService<IChatFileStore>();

            var (client, turn, instruction) = await Turn(recorder, projectPath,
                "Fatto.\n\n<<<mde-thread\n{\"title\": \"Note di rilascio\", \"detail\": \"- Versione letta\\n- Data aggiunta\"}\nmde-thread>>>");

            StringAssert.Contains(instruction, "<<<mde-thread", "il modello riceve le istruzioni del blocco");
            Assert.AreEqual("Fatto.", client.Text, "il blocco non arriva a video");
            Assert.AreEqual(0, client.System.Count, "tutto a posto: nessun avviso");
            var row = Row(ctx, turn.ThreadId.Value);
            Assert.AreEqual("Note di rilascio", row.Title);
            Assert.AreEqual("- Versione letta\n- Data aggiunta", row.Detail);

            var events = files.Read(row.FilePath).Events;
            Assert.AreEqual("Fatto.", events.Single(e => e.Kind == ChatRecordEvent.KindText).Content, "nella chat il testo come l'ha visto l'utente");
            Assert.AreEqual("thread-block", events.Single(e => e.Kind == ChatRecordEvent.KindMeta).Content, "e il blocco come evento a sé");

            // Il turno dopo: le istruzioni portano titolo e dettaglio attuali; un blocco {} non cambia niente.
            var (_, _, next) = await Turn(recorder, projectPath, "Ancora.\n<<<mde-thread\n{}\nmde-thread>>>");
            StringAssert.Contains(next, "Note di rilascio");
            StringAssert.Contains(next, "- Data aggiunta");
            Assert.AreEqual("Note di rilascio", Row(ctx, turn.ThreadId.Value).Title);
        }

        [TestMethod]
        public async Task Say_when_the_block_is_missing_or_off_the_rule_and_keep_the_previous_detail()
        {
            using var ctx = new AgentCityContext();
            var (_, projectPath) = ctx.SeedProject("thread-block-bad");
            var recorder = ctx.Factory.Services.GetRequiredService<ChatHistoryRecorder>();

            var (_, first, _) = await Turn(recorder, projectPath,
                "Uno.\n<<<mde-thread\n{\"title\": \"Prima\", \"detail\": \"- a\"}\nmde-thread>>>");

            var (missing, _, _) = await Turn(recorder, projectPath, "Due, senza blocco.");
            Assert.AreEqual("Due, senza blocco.", missing.Text);
            Assert.AreEqual(1, missing.System.Count, "si dice che titolo e dettaglio non sono aggiornati");

            var six = string.Join("\\n", Enumerable.Range(1, 6).Select(i => "- p" + i));
            var (tooMany, _, _) = await Turn(recorder, projectPath, "Tre.\n<<<mde-thread\n{\"detail\": \"" + six + "\"}\nmde-thread>>>");
            Assert.AreEqual(1, tooMany.System.Count);
            StringAssert.Contains(tooMany.System[0], "6");

            var row = Row(ctx, first.ThreadId.Value);
            Assert.AreEqual("Prima", row.Title);
            Assert.AreEqual("- a", row.Detail, "il dettaglio fuori regola non sostituisce quello di prima");
        }

        [TestMethod]
        public async Task Leave_side_channels_alone()
        {
            using var ctx = new AgentCityContext();
            var (_, projectPath) = ctx.SeedProject("thread-block-side");
            var recorder = ctx.Factory.Services.GetRequiredService<ChatHistoryRecorder>();

            var turn = recorder.BeginTurn("conn", projectPath, "claudecode", "sonnet", "commit-message");
            var client = new CapturingProxy();
            IClientProxy proxy = new RecordingClientProxy(client, turn, "commit-message");
            await proxy.SendAsync("ReceiveStreamChunk", "feat: x", "commit-message");
            await proxy.SendAsync("StreamComplete", "commit-message");
            turn.Complete();

            Assert.AreEqual("feat: x", client.Text);
            Assert.AreEqual(0, client.System.Count, "a un canale laterale il blocco non si chiede, quindi non manca");
        }
    }
}
