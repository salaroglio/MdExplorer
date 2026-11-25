using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.ChatHistory;
using MdExplorer.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// «Approva» e «Rifiuta» una alla volta (sprint skill che migliorano, F4, U5): la skill si scrive subito con il blocco
    /// learned:, il commit automatico la riconosce come di MDE, il rifiuto si ricorda, e il prossimo «Impara» sa che cosa è
    /// successo alle proposte di prima.
    /// </summary>
    [TestClass]
    public class SkillLearningDecisions_Should
    {
        private sealed class Env : IDisposable
        {
            public AgentCityContext Ctx;
            public string Project;
            public Guid Thread;
            public void Dispose() => Ctx.Dispose();
        }

        private static Env Setup(string name)
        {
            var ctx = new AgentCityContext();
            var (_, project) = ctx.SeedProject(name);
            File.WriteAllText(Path.Combine(project, ".development.yml"), "folders: []\n");
            HarnessSettings.Write(project, HarnessTarget.Claude);
            var hand = Path.Combine(project, ".claude", "skills", "scritta-a-mano");
            Directory.CreateDirectory(hand);
            File.WriteAllText(Path.Combine(hand, "SKILL.md"), "---\nname: scritta-a-mano\ndescription: Regole del gruppo\n---\n\nUna regola.\n");
            var recorder = ctx.Factory.Services.GetRequiredService<ChatHistoryRecorder>();
            var turn = recorder.BeginTurn("c", project, "claudecode", "sonnet", "default");
            turn.UserPrompt("Fai il rilascio");
            turn.OnClientEvent("ReceiveStreamChunk", new object[] { "fatto", "default" });
            turn.Complete();
            return new Env { Ctx = ctx, Project = project, Thread = turn.ThreadId.Value };
        }

        private static string Skill(string name, string body) => $"---\nname: {name}\ndescription: {name} per prova. Use when: prova.\n---\n\n{body}\n";

        private static async Task<JsonElement> Review(Env env, params (string action, string skill, string body)[] proposals)
        {
            var answer = "Ho imparato.\n<<<mde-learn\n" + JsonSerializer.Serialize(new
            {
                proposals = proposals.Select(p => new { p.action, p.skill, why = "perché sì", evidence = new[] { 1 }, content = Skill(p.skill, p.body) }).ToArray(),
            }) + "\nmde-learn>>>";
            var resp = await env.Ctx.Client.PostAsJsonAsync("/api/skills/learn/review", new { projectPath = env.Project, threadId = env.Thread, from = 1, to = 2, answer });
            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode, await resp.Content.ReadAsStringAsync());
            return await resp.Content.ReadFromJsonAsync<JsonElement>();
        }

        private static Task<HttpResponseMessage> Decide(Env env, string what, string recordId, string content = null)
            => env.Ctx.Client.PostAsJsonAsync($"/api/skills/learn/{what}", new { projectPath = env.Project, recordId, content });

        private static string Record(JsonElement review, int i) => review.GetProperty("proposals")[i].GetProperty("recordId").GetString();

        private static ChatLearning Row(Env env, string id)
        {
            using var scope = env.Ctx.Factory.Services.CreateScope();
            return scope.ServiceProvider.GetRequiredService<IUserSettingsDB>().GetDal<ChatLearning>().GetList().Single(l => l.Id == Guid.Parse(id));
        }

        [TestMethod]
        public async Task Write_an_approved_skill_at_once_and_raise_its_version_on_the_next_correction()
        {
            using var env = Setup("learn-approve");
            var review = await Review(env, ("create", "rilascio", "Sempre ./release.sh --sign"));

            var resp = await Decide(env, "approve", Record(review, 0));
            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode, await resp.Content.ReadAsStringAsync());
            var decision = await resp.Content.ReadFromJsonAsync<JsonElement>();
            Assert.AreEqual("approved", decision.GetProperty("outcome").GetString());
            Assert.AreEqual(1, decision.GetProperty("version").GetInt32());

            var file = Path.Combine(env.Project, ".claude", "skills", "rilascio", "SKILL.md");
            var text = File.ReadAllText(file);
            StringAssert.Contains(text, "name: rilascio\n");
            StringAssert.Contains(text, "learned:\n  version: 1\n");
            StringAssert.Contains(text, "  chat: " + env.Thread.ToString("D"));
            StringAssert.Contains(text, "Sempre ./release.sh --sign");
            Assert.AreEqual(MdeMergeKind.Learned, MdeOwnedFiles.Find(env.Project, file)?.Kind, "il commit automatico la riconosce come di MDE");
            Assert.AreEqual("approved", Row(env, Record(review, 0)).Outcome);

            // Un «Impara» dopo: la stessa skill corretta, con il testo cambiato dall'utente prima di approvarla.
            var again = await Review(env, ("update", "rilascio", "Prima version.txt, poi ./release.sh --sign"));
            var corrected = Skill("rilascio", "Prima version.txt, poi ./release.sh --sign. Mai senza --sign.");
            var second = await (await Decide(env, "approve", Record(again, 0), corrected)).Content.ReadFromJsonAsync<JsonElement>();
            Assert.AreEqual("corrected", second.GetProperty("outcome").GetString());
            Assert.AreEqual(2, second.GetProperty("version").GetInt32());
            var text2 = File.ReadAllText(file);
            StringAssert.Contains(text2, "learned:\n  version: 2\n");
            StringAssert.Contains(text2, "Mai senza --sign.");
            Assert.AreEqual(1, text2.Split("learned:").Length - 1, "un solo blocco learned:");
        }

        [TestMethod]
        public async Task Remember_a_rejection_and_tell_the_next_learn_what_happened_before()
        {
            using var env = Setup("learn-reject");
            var first = await Review(env, ("create", "da-rifiutare", "no"), ("create", "in-sospeso", "forse"));

            Assert.AreEqual(HttpStatusCode.OK, (await Decide(env, "reject", Record(first, 0))).StatusCode);
            Assert.AreEqual("rejected", Row(env, Record(first, 0)).Outcome);
            Assert.IsFalse(Directory.Exists(Path.Combine(env.Project, ".claude", "skills", "da-rifiutare")), "rifiutata: niente file");
            Assert.AreEqual(HttpStatusCode.Conflict, (await Decide(env, "approve", Record(first, 0))).StatusCode, "una decisione si prende una volta");

            var prepared = await (await env.Ctx.Client.PostAsJsonAsync("/api/skills/learn/prepare", new { projectPath = env.Project, threadId = env.Thread }))
                .Content.ReadFromJsonAsync<JsonElement>();
            var prompt = prepared.GetProperty("prompt").GetString();
            StringAssert.Contains(prompt, "- rifiutata: create `da-rifiutare`");
            StringAssert.Contains(prompt, "- in sospeso: create `in-sospeso`");
            StringAssert.Contains(prompt, "forse", "il testo di quella in sospeso, per poterla rivedere");

            // Il nuovo «Impara»: quella in sospeso di prima è stata rivista.
            await Review(env, ("create", "in-sospeso", "sì, ora è chiaro"));
            Assert.AreEqual("superseded", Row(env, Record(first, 1)).Outcome);
        }

        [TestMethod]
        public async Task Refuse_what_cannot_be_written_and_leave_it_pending()
        {
            using var env = Setup("learn-refuse");
            var review = await Review(env, ("create", "nuova", "x"), ("update", "scritta-a-mano", "Una regola.\nE un'altra."));

            // Nel frattempo qualcuno ha creato la skill: «create» non si scrive sopra.
            Directory.CreateDirectory(Path.Combine(env.Project, ".claude", "skills", "nuova"));
            File.WriteAllText(Path.Combine(env.Project, ".claude", "skills", "nuova", "SKILL.md"), Skill("nuova", "del collega"));
            var clash = await Decide(env, "approve", Record(review, 0));
            Assert.AreEqual(HttpStatusCode.Conflict, clash.StatusCode);
            Assert.AreEqual("pending", Row(env, Record(review, 0)).Outcome, "resta in sospeso");
            StringAssert.Contains(File.ReadAllText(Path.Combine(env.Project, ".claude", "skills", "nuova", "SKILL.md")), "del collega");

            // Un testo corretto male non si scrive.
            var bad = await Decide(env, "approve", Record(review, 1), "senza frontmatter");
            Assert.AreEqual(HttpStatusCode.Conflict, bad.StatusCode);

            // La skill scritta a mano si corregge e diventa imparata, versione 1.
            var ok = await (await Decide(env, "approve", Record(review, 1))).Content.ReadFromJsonAsync<JsonElement>();
            Assert.AreEqual(1, ok.GetProperty("version").GetInt32());
            StringAssert.Contains(File.ReadAllText(Path.Combine(env.Project, ".claude", "skills", "scritta-a-mano", "SKILL.md")), "E un'altra.");
        }
    }
}
