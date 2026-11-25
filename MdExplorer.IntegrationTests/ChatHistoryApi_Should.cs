using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Features.Services.AI.ChatHistory;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.ChatHistory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// La chat che sopravvive a una riconnessione e le API della cronologia (sprint 2026-11-24-Chat-Memorizzate, F4).
    /// </summary>
    [TestClass]
    public class ChatHistoryApi_Should
    {
        private static Guid OneTurn(ChatHistoryRecorder recorder, string connectionId, string projectPath, string prompt, string answer,
            string engineSession = null)
        {
            var turn = recorder.BeginTurn(connectionId, projectPath, "claudecode", "sonnet", "default");
            turn.UserPrompt(prompt);
            turn.OnClientEvent("ReceiveStreamChunk", new object[] { answer, "default" });
            turn.EngineSession(engineSession);
            turn.Complete();
            return turn.ThreadId.Value;
        }

        [TestMethod]
        public void Go_on_in_the_same_chat_after_a_reconnection()
        {
            using var ctx = new AgentCityContext();
            var (_, projectPath) = ctx.SeedProject("chat-resume");
            var recorder = ctx.Factory.Services.GetRequiredService<ChatHistoryRecorder>();
            var files = ctx.Factory.Services.GetRequiredService<IChatFileStore>();

            var first = recorder.BeginTurn("old-conn", projectPath, "claudecode", "sonnet", "default");
            Assert.IsNotNull(first.TakeThreadToAnnounce(), "una chat appena aperta si annuncia al client");
            Assert.IsNull(first.TakeThreadToAnnounce(), "una volta sola");
            first.UserPrompt("uno");
            first.Complete();
            var id = first.ThreadId.Value;
            OneTurn(recorder, "old-conn", projectPath, "due", "risposta due", engineSession: "sess-1");

            // La connessione cade; quella nuova riaggancia la chat.
            recorder.Forget("old-conn");
            var outcome = recorder.Resume("new-conn", id, projectPath);
            Assert.IsTrue(outcome.Resumed, outcome.Reason);

            var third = recorder.BeginTurn("new-conn", projectPath, "claudecode", "sonnet", "default");
            Assert.AreEqual(id, third.ThreadId);
            Assert.IsNull(third.TakeThreadToAnnounce(), "una chat ripresa non si riannuncia");
            third.UserPrompt("tre");
            third.Complete();

            var events = files.Read(files.RelativePathFor(projectPath, id)).Events;
            CollectionAssert.AreEqual(new long[] { 1, 2, 3, 4 }, events.Select(e => e.Seq).ToArray(), "il progressivo continua");
            CollectionAssert.AreEqual(new[] { 1, 2, 2, 3 }, events.Select(e => e.Turn).ToArray(), "e anche il numero di turno");
        }

        [TestMethod]
        public void Refuse_to_resume_a_chat_of_another_project_or_without_its_file()
        {
            using var ctx = new AgentCityContext();
            var (_, projectA) = ctx.SeedProject("chat-a");
            var (_, projectB) = ctx.SeedProject("chat-b");
            var recorder = ctx.Factory.Services.GetRequiredService<ChatHistoryRecorder>();
            var files = ctx.Factory.Services.GetRequiredService<IChatFileStore>();
            var id = OneTurn(recorder, "c1", projectA, "ciao", "ciao");

            var other = recorder.Resume("c2", id, projectB);
            Assert.IsFalse(other.Resumed);
            StringAssert.Contains(other.Reason, projectA);

            files.Delete(files.RelativePathFor(projectA, id));
            var gone = recorder.Resume("c2", id, projectA);
            Assert.IsFalse(gone.Resumed);
            Assert.IsFalse(recorder.Resume("c2", Guid.NewGuid(), projectA).Resumed);
        }

        [TestMethod]
        public async Task List_read_and_delete_the_chats_of_a_project()
        {
            using var ctx = new AgentCityContext();
            var (_, projectPath) = ctx.SeedProject("chat-api");
            var recorder = ctx.Factory.Services.GetRequiredService<ChatHistoryRecorder>();
            var older = OneTurn(recorder, "a", projectPath, "prima chat", "uno", engineSession: "sess-a");
            await Task.Delay(20);
            var newer = OneTurn(recorder, "b", projectPath, "seconda chat", "due");

            var list = await ctx.Client.GetFromJsonAsync<JsonElement>("/api/chats?projectPath=" + Uri.EscapeDataString(projectPath));
            var ids = list.EnumerateArray().Select(c => c.GetProperty("id").GetGuid()).ToArray();
            CollectionAssert.AreEqual(new[] { newer, older }, ids, "le più recenti prima");
            Assert.IsTrue(list.EnumerateArray().All(c => c.GetProperty("fileExists").GetBoolean()));

            var read = await ctx.Client.GetFromJsonAsync<JsonElement>($"/api/chats/{older}/events");
            var kinds = read.GetProperty("events").EnumerateArray().Select(e => e.GetProperty("kind").GetString()).ToArray();
            CollectionAssert.AreEqual(new[] { "user_prompt", "text" }, kinds);
            Assert.AreEqual("prima chat", read.GetProperty("events")[0].GetProperty("content").GetString());

            var deleted = await ctx.Client.DeleteAsync($"/api/chats/{older}");
            Assert.AreEqual(HttpStatusCode.OK, deleted.StatusCode);
            Assert.AreEqual(HttpStatusCode.NotFound, (await ctx.Client.GetAsync($"/api/chats/{older}/events")).StatusCode);
            var after = await ctx.Client.GetFromJsonAsync<JsonElement>("/api/chats?projectPath=" + Uri.EscapeDataString(projectPath));
            Assert.AreEqual(1, after.GetArrayLength());

            Assert.AreEqual(HttpStatusCode.BadRequest, (await ctx.Client.GetAsync("/api/chats")).StatusCode);
        }

        [TestMethod]
        public void Keep_the_engine_session_in_the_row()
        {
            using var ctx = new AgentCityContext();
            var (_, projectPath) = ctx.SeedProject("chat-session");
            var recorder = ctx.Factory.Services.GetRequiredService<ChatHistoryRecorder>();

            var id = OneTurn(recorder, "s", projectPath, "ciao", "ciao", engineSession: "69c827e9-sess");

            using var scope = ctx.Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MdExplorer.Abstractions.DB.IUserSettingsDB>();
            var row = db.GetDal<MdExplorer.Abstractions.Entities.UserDB.ChatThread>().GetList().Single(t => t.Id == id);
            Assert.AreEqual("69c827e9-sess", row.EngineSessionId);
        }
    }
}
