using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.ChatHistory;
using MdExplorer.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// «Impara da questa attività» col servizio vero (sprint skill che migliorano, F3): il prompt con l'attività, le skill
    /// del progetto e l'intervallo; poi la risposta del motore letta, controllata e confrontata con i file.
    /// </summary>
    [TestClass]
    public class SkillLearningService_Should
    {
        private static (AgentCityContext ctx, string project, Guid thread) Setup(string name)
        {
            var ctx = new AgentCityContext();
            var (_, project) = ctx.SeedProject(name);
            File.WriteAllText(Path.Combine(project, ".development.yml"), "folders: []\n");
            HarnessSettings.Write(project, HarnessTarget.Claude);
            var skill = Path.Combine(project, ".claude", "skills", "rilascio");
            Directory.CreateDirectory(skill);
            File.WriteAllText(Path.Combine(skill, "SKILL.md"), "---\nname: rilascio\ndescription: Come si rilascia\n---\n\n1. Lancia release.sh\n");

            var recorder = ctx.Factory.Services.GetRequiredService<ChatHistoryRecorder>();
            var turn = recorder.BeginTurn("c", project, "claudecode", "sonnet", "default");
            turn.UserPrompt("Fai il rilascio");
            turn.OnClientEvent("ReceiveStreamChunk", new object[] { "Lancio relase.sh", "default" });
            turn.OnClientEvent("ReceiveError", new object[] { "command not found: relase.sh", "default" });
            turn.Complete();
            var again = recorder.BeginTurn("c", project, "claudecode", "sonnet", "default");
            again.UserPrompt("No, si chiama release.sh e va lanciato con --sign");
            again.OnClientEvent("ReceiveStreamChunk", new object[] { "Fatto con release.sh --sign", "default" });
            again.Complete();
            return (ctx, project, turn.ThreadId.Value);
        }

        [TestMethod]
        public async Task Prepare_the_prompt_with_the_activity_and_the_project_skills()
        {
            var (ctx, project, thread) = Setup("learn-prepare");
            using var _ = ctx;

            var resp = await ctx.Client.PostAsJsonAsync("/api/skills/learn/prepare", new { projectPath = project, threadId = thread });
            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode, await resp.Content.ReadAsStringAsync());
            var prepared = await resp.Content.ReadFromJsonAsync<JsonElement>();
            var prompt = prepared.GetProperty("prompt").GetString();

            Assert.AreEqual("learn", prepared.GetProperty("channel").GetString());
            Assert.AreEqual(1, prepared.GetProperty("from").GetInt64());
            Assert.AreEqual(5, prepared.GetProperty("to").GetInt64());
            StringAssert.Contains(prompt, "[1] utente: Fai il rilascio");
            StringAssert.Contains(prompt, "errore: command not found: relase.sh");
            StringAssert.Contains(prompt, "[4] utente: No, si chiama release.sh");
            StringAssert.Contains(prompt, "`rilascio` (.claude/skills/rilascio/SKILL.md)");
            StringAssert.Contains(prompt, thread.ToString("D"));
            StringAssert.Contains(prompt, "<<<mde-learn");
            Assert.IsFalse(prompt.Contains("{{"), "nessun segnaposto rimasto");
        }

        [TestMethod]
        public async Task Refuse_with_the_reason_when_the_project_has_no_engine()
        {
            var (ctx, project, thread) = Setup("learn-noengine");
            using var _ = ctx;
            HarnessSettings.Write(project, HarnessTarget.None);

            var resp = await ctx.Client.PostAsJsonAsync("/api/skills/learn/prepare", new { projectPath = project, threadId = thread });

            Assert.AreEqual(HttpStatusCode.Conflict, resp.StatusCode);
            StringAssert.Contains(await resp.Content.ReadAsStringAsync(), "none");
        }

        [TestMethod]
        public async Task Review_the_answer_against_the_files_of_the_project()
        {
            var (ctx, project, _) = Setup("learn-review");
            using var __ = ctx;
            var answer = "Ho imparato il nome giusto dello script.\n<<<mde-learn\n" + JsonSerializer.Serialize(new
            {
                proposals = new object[]
                {
                    new { action = "update", skill = "rilascio", why = "il comando giusto", evidence = new[] { 4 },
                          content = "---\nname: rilascio\ndescription: Come si rilascia\n---\n\n1. Lancia release.sh --sign\n" },
                    new { action = "create", skill = "mde-trucchi", why = "x", evidence = new[] { 2 }, content = "---\nname: mde-trucchi\ndescription: x\n---\n" },
                },
            }) + "\nmde-learn>>>";

            var resp = await ctx.Client.PostAsJsonAsync("/api/skills/learn/review", new { projectPath = project, from = 1, to = 5, answer });
            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode, await resp.Content.ReadAsStringAsync());
            var review = await resp.Content.ReadFromJsonAsync<JsonElement>();

            Assert.AreEqual("Ho imparato il nome giusto dello script.", review.GetProperty("prose").GetString());
            var update = review.GetProperty("proposals")[0];
            Assert.IsTrue(update.GetProperty("isValid").GetBoolean());
            Assert.AreEqual(".claude/skills/rilascio/SKILL.md", update.GetProperty("targetPath").GetString());
            StringAssert.Contains(update.GetProperty("currentContent").GetString(), "1. Lancia release.sh\n");
            var diff = update.GetProperty("diff").EnumerateArray().Select(l => l.GetProperty("kind").GetString() + ":" + l.GetProperty("text").GetString()).ToList();
            CollectionAssert.Contains(diff, "del:1. Lancia release.sh");
            CollectionAssert.Contains(diff, "add:1. Lancia release.sh --sign");

            var refused = review.GetProperty("proposals")[1];
            Assert.IsFalse(refused.GetProperty("isValid").GetBoolean(), "la mde-* resta, con il motivo");
            Assert.AreEqual(JsonValueKind.Null, refused.GetProperty("targetPath").ValueKind);
        }
    }
}
