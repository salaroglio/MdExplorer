using System;
using System.Collections.Generic;
using System.IO;
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
    /// La chat registrata mentre succede, col servizio vero (sprint 2026-11-24-Chat-Memorizzate, F2): la riga della
    /// cronologia nel DB utente e il contenuto nel file, attraverso il client del turno che inoltra e registra.
    /// </summary>
    [TestClass]
    public class ChatHistoryRecorder_Should
    {
        private sealed class CapturingProxy : IClientProxy
        {
            public readonly List<(string Method, object[] Args)> Sent = new();
            public Task SendCoreAsync(string method, object[] args, CancellationToken cancellationToken = default)
            {
                lock (Sent) Sent.Add((method, args));
                return Task.CompletedTask;
            }
        }

        private static ChatThread Row(AgentCityContext ctx, Guid id)
        {
            using var scope = ctx.Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            return db.GetDal<ChatThread>().GetList().Single(t => t.Id == id);
        }

        [TestMethod]
        public async Task Write_the_row_and_the_file_while_the_client_sees_the_same_events()
        {
            using var ctx = new AgentCityContext();
            var (_, projectPath) = ctx.SeedProject("chat-history");
            var recorder = ctx.Factory.Services.GetRequiredService<ChatHistoryRecorder>();
            var files = ctx.Factory.Services.GetRequiredService<IChatFileStore>();

            // Primo turno, dal tab.
            var turn1 = recorder.BeginTurn("conn-1", projectPath, "claudecode", "sonnet", "default");
            turn1.UserPrompt("Leggi note.md");
            var client = new CapturingProxy();
            IClientProxy proxy = new RecordingClientProxy(client, turn1, "default");
            await proxy.SendAsync("ReceiveThinking", "Devo leggere.", "default");
            await proxy.SendAsync("ReceiveStreamChunk", "La versione ", "default");
            await proxy.SendAsync("ReceiveStreamChunk", "è 3.15.228.", "default");
            await proxy.SendAsync("StreamComplete", "default");
            turn1.Complete();

            // Secondo turno, da un canale laterale della stessa connessione: stessa chat (D2).
            var turn2 = recorder.BeginTurn("conn-1", projectPath, "claudecode", "sonnet", "commit-message");
            turn2.UserPrompt("Scrivi il messaggio di commit");
            IClientProxy proxy2 = new RecordingClientProxy(client, turn2, "commit-message");
            await proxy2.SendAsync("ReceiveStreamChunk", "feat: x", "commit-message");
            await proxy2.SendAsync("StreamComplete", "commit-message");
            turn2.Complete();

            Assert.AreEqual(turn1.ThreadId, turn2.ThreadId);
            CollectionAssert.AreEqual(
                new[] { "ReceiveThinking", "ReceiveStreamChunk", "ReceiveStreamChunk", "StreamComplete", "ReceiveStreamChunk", "StreamComplete" },
                client.Sent.Select(s => s.Method).ToArray(), "il client vede esattamente quello che vedeva prima");

            var row = Row(ctx, turn1.ThreadId.Value);
            Assert.AreEqual(projectPath, row.ProjectPath);
            Assert.AreEqual("claudecode", row.Engine);
            Assert.AreEqual("sonnet", row.ModelId);
            Assert.AreEqual(files.RelativePathFor(projectPath, row.Id), row.FilePath);

            var read = files.Read(row.FilePath);
            Assert.AreEqual(0, read.Problems.Count, string.Join("; ", read.Problems));
            CollectionAssert.AreEqual(new[] { "user_prompt", "thinking", "text", "user_prompt", "text" }, read.Events.Select(e => e.Kind).ToArray());
            CollectionAssert.AreEqual(new long[] { 1, 2, 3, 4, 5 }, read.Events.Select(e => e.Seq).ToArray());
            CollectionAssert.AreEqual(new[] { 1, 1, 1, 2, 2 }, read.Events.Select(e => e.Turn).ToArray());
            Assert.AreEqual("La versione è 3.15.228.", read.Events[2].Content);
            Assert.AreEqual("commit-message", read.Events[4].Channel);

            files.Delete(row.FilePath);
        }

        [TestMethod]
        public void Open_a_new_chat_after_new_chat_or_disconnection()
        {
            using var ctx = new AgentCityContext();
            var (_, projectPath) = ctx.SeedProject("chat-history-new");
            var recorder = ctx.Factory.Services.GetRequiredService<ChatHistoryRecorder>();

            var first = recorder.BeginTurn("conn-2", projectPath, "opencode", null, "default");
            first.Complete();
            recorder.Forget("conn-2");
            var second = recorder.BeginTurn("conn-2", projectPath, "opencode", null, "default");
            second.Complete();

            Assert.AreNotEqual(first.ThreadId, second.ThreadId);
            Assert.IsNotNull(Row(ctx, first.ThreadId.Value), "la chat di prima resta nella cronologia");
        }

        [TestMethod]
        public async Task Say_once_per_connection_that_a_chat_without_project_is_not_saved()
        {
            using var ctx = new AgentCityContext();
            var recorder = ctx.Factory.Services.GetRequiredService<ChatHistoryRecorder>();
            var client = new CapturingProxy();

            foreach (var _ in Enumerable.Range(0, 2))
            {
                var turn = recorder.BeginTurn("conn-3", null, "claudecode", null, "default");
                Assert.IsFalse(turn.IsRecording);
                IClientProxy proxy = new RecordingClientProxy(client, turn, "default");
                await proxy.SendAsync("ReceiveStreamChunk", "ciao", "default");
                turn.Complete();
            }

            var errors = client.Sent.Where(s => s.Method == "ReceiveError").ToList();
            Assert.AreEqual(1, errors.Count, "una volta sola, non a ogni turno");
            var said = (string)errors[0].Args[0];
            Assert.IsTrue(said.Contains("progetto") || said.Contains("project"), said);
            Assert.AreEqual(2, client.Sent.Count(s => s.Method == "ReceiveStreamChunk"), "la chat va avanti lo stesso");
        }
    }
}
