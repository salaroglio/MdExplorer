using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using LibGit2Sharp;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.ChatHistory;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// I punti del riassunto delle chat con il ramo e lo SHA di HEAD (27/11/2026), col servizio vero: il registratore li
    /// scrive a fine turno, la cronologia li dà con ora e ramo, «Genera descrizione» del commit prende solo quelli del ramo
    /// dopo l'ultimo commit, e l'eliminazione della chat li porta via.
    /// </summary>
    [TestClass]
    public class ChatThreadPoints_Should
    {
        private sealed class NullProxy : IClientProxy
        {
            public Task SendCoreAsync(string method, object[] args, CancellationToken cancellationToken = default) => Task.CompletedTask;
        }

        private static async Task<Guid> Turn(ChatHistoryRecorder recorder, string projectPath, string point)
        {
            var turn = recorder.BeginTurn("conn", projectPath, "claudecode", "sonnet", "default");
            turn.UserPrompt("domanda");
            turn.ThreadInstruction();
            IClientProxy proxy = new RecordingClientProxy(new NullProxy(), turn, "default");
            await proxy.SendAsync("ReceiveStreamChunk", "Fatto.\n<<<mde-thread\n" + JsonSerializer.Serialize(new { title = "Lavoro", point }) + "\nmde-thread>>>", "default");
            await proxy.SendAsync("StreamComplete", "default");
            turn.Complete();
            return turn.ThreadId.Value;
        }

        private static List<ChatThreadPoint> Points(AgentCityContext ctx, Guid threadId)
        {
            using var scope = ctx.Factory.Services.CreateScope();
            return scope.ServiceProvider.GetRequiredService<IUserSettingsDB>().GetDal<ChatThreadPoint>().GetList()
                .Where(p => p.ThreadId == threadId).ToList().OrderBy(p => p.At).ToList();
        }

        private static Signature Me => new("Mario Rossi", "mario.rossi@example.com", DateTimeOffset.Now);

        private static string InitRepository(string path)
        {
            Repository.Init(path);
            using var repo = new Repository(path);
            File.WriteAllText(Path.Combine(path, "a.md"), "# A\n");
            Commands.Stage(repo, "a.md");
            return repo.Commit("primo", Me, Me).Sha;
        }

        private static async Task<(int ChatPoints, string Prompt)> CommitPrompt(AgentCityContext ctx, string path)
        {
            var (status, body) = await ctx.PostJson("/api/GitAi/commit-prompt", JsonSerializer.Serialize(new { projectPath = path, language = "it" }));
            Assert.AreEqual(HttpStatusCode.OK, status, body);
            using var json = JsonDocument.Parse(body);
            return (json.RootElement.GetProperty("chatPoints").GetInt32(), json.RootElement.GetProperty("prompt").GetString());
        }

        [TestMethod]
        public async Task Keep_branch_and_head_of_each_point_and_use_only_the_ones_after_the_last_commit()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("chat-points");
            var firstSha = InitRepository(path);
            string branch;
            using (var repo = new Repository(path)) branch = repo.Head.FriendlyName;
            var recorder = ctx.Factory.Services.GetRequiredService<ChatHistoryRecorder>();

            var thread = await Turn(recorder, path, "Aggiunta la sezione A");
            await Turn(recorder, path, "Aggiunta la sezione A");   // lo stesso punto non si ripete
            await Turn(recorder, path, "Corretto il titolo");
            var points = Points(ctx, thread);
            CollectionAssert.AreEqual(new[] { "Aggiunta la sezione A", "Corretto il titolo" }, points.Select(p => p.Point).ToList());
            Assert.IsTrue(points.All(p => p.Branch == branch && p.HeadSha == firstSha), "ramo e HEAD di quando il punto è stato scritto");

            // Le modifiche non registrate: il prompt del commit porta i due punti.
            File.AppendAllText(Path.Combine(path, "a.md"), "testo\n");
            var (count, prompt) = await CommitPrompt(ctx, path);
            Assert.AreEqual(2, count);
            StringAssert.Contains(prompt, "  - Aggiunta la sezione A\n  - Corretto il titolo");

            // Dopo il commit quei punti sono registrati: una modifica nuova senza chat ha 0 punti.
            using (var repo = new Repository(path))
            {
                Commands.Stage(repo, "a.md");
                repo.Commit("secondo", Me, Me);
            }
            File.AppendAllText(Path.Combine(path, "a.md"), "ancora\n");
            (count, prompt) = await CommitPrompt(ctx, path);
            Assert.AreEqual(0, count, "i punti di prima sono già nel commit precedente");
            Assert.IsFalse(prompt.Contains("Corretto il titolo"));

            // Un altro ramo sullo stesso HEAD: i suoi punti non sono di questo ramo.
            using (var repo = new Repository(path))
                Commands.Checkout(repo, repo.CreateBranch("feat/2026-11-27-altro"));
            await Turn(recorder, path, "Lavoro sull'altro ramo");
            using (var repo = new Repository(path)) Commands.Checkout(repo, repo.Branches[branch]);
            (count, _) = await CommitPrompt(ctx, path);
            Assert.AreEqual(0, count, "stesso SHA ma altro ramo: non conta");
        }

        [TestMethod]
        public async Task List_points_with_time_and_branch_and_delete_them_with_the_chat()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("chat-points-list");
            var recorder = ctx.Factory.Services.GetRequiredService<ChatHistoryRecorder>();
            var thread = await Turn(recorder, path, "Fuori da git");
            Assert.IsNull(Points(ctx, thread).Single().Branch, "fuori da git il ramo non c'è");

            var (status, json) = await ctx.GetJson("/api/chats?projectPath=" + WebUtility.UrlEncode(path));
            Assert.AreEqual(HttpStatusCode.OK, status);
            var listed = json.RootElement.EnumerateArray().Single().GetProperty("points").EnumerateArray().Single();
            Assert.AreEqual("Fuori da git", listed.GetProperty("point").GetString());
            Assert.IsTrue(listed.TryGetProperty("at", out _));
            Assert.IsFalse(listed.TryGetProperty("headSha", out _), "lo SHA resta sul server");

            var response = await ctx.Client.DeleteAsync("/api/chats/" + thread);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(0, Points(ctx, thread).Count, "eliminata la chat, i suoi punti non restano");
        }
    }
}
