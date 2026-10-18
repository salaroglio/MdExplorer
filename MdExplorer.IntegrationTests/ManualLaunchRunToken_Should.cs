using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Features.Agents;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.AgentRun;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Un agente lanciato a mano è un cittadino come gli altri.
    /// <para>
    /// I tool della città — chiamare un collega, chiedere un intervento su un ambito altrui,
    /// leggere la rubrica — si autenticano col <c>RunToken</c>, che il Service passa
    /// nell'ambiente del processo. Il risveglio da messaggio lo coniava; il lancio manuale e
    /// quello schedulato no, perché costruiscono il turno per conto loro invece di passare da
    /// <see cref="LlmAgentWaker"/>. Risultato: un agente lanciato dal pulsante si prendeva
    /// «RunToken assente o non valido» su ogni tentativo di parlare con qualcuno — e nessuno se
    /// ne accorgeva, perché l'agente rispondeva comunque qualcosa.
    /// </para>
    /// </summary>
    [TestClass]
    public class ManualLaunchRunToken_Should
    {
        [TestMethod]
        public async Task Give_the_agent_a_valid_identity_to_talk_to_its_colleagues()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("lancio-manuale");
            var agentFile = WriteAgent(path, "curatore");

            var job = ctx.Factory.Services.GetRequiredService<IAgentRunJobService>();
            var tokens = ctx.Factory.Services.GetRequiredService<IRunTokenStore>();

            await job.RunAsync(new AgentRunRequestModel
            {
                ProjectPath = path,
                AgentFilePath = agentFile,
                PreparedPrompt = "fai qualcosa",
                TriggerSource = "manual",
            });

            var env = ctx.Runner.LastRequest?.Environment;
            Assert.IsNotNull(env, "il turno dev'essere partito");

            var token = env.TryGetValue(LlmAgentWaker.EnvRunToken, out var t) ? t : null;
            Assert.IsFalse(string.IsNullOrWhiteSpace(token),
                "senza RunToken l'agente non può chiamare nessuno");

            // Il token dev'essere quello vero dello store, non una stringa qualsiasi: è ciò che
            // il Service usa per risalire a chi sta parlando.
            Assert.AreEqual(path, ctx.Runner.LastRequest.ProjectPath);
            Assert.AreEqual("curatore", env[LlmAgentWaker.EnvAgentName]);
            Assert.AreEqual(path, env[LlmAgentWaker.EnvProjectPath]);
        }

        [TestMethod]
        public async Task Take_the_identity_back_when_the_run_is_over()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("lancio-revoca");
            var agentFile = WriteAgent(path, "curatore");

            var job = ctx.Factory.Services.GetRequiredService<IAgentRunJobService>();
            var tokens = ctx.Factory.Services.GetRequiredService<IRunTokenStore>();

            await job.RunAsync(new AgentRunRequestModel
            {
                ProjectPath = path,
                AgentFilePath = agentFile,
                PreparedPrompt = "fai qualcosa",
                TriggerSource = "manual",
            });

            var token = ctx.Runner.LastRequest.Environment[LlmAgentWaker.EnvRunToken];

            // Il token vale un run: finito il turno non deve restare spendibile, altrimenti un
            // processo rimasto in giro potrebbe continuare a parlare a nome dell'agente.
            Assert.IsNull(tokens.Validate(token),
                "il token dev'essere revocato a fine run");
        }

        private static string WriteAgent(string projectPath, string name)
        {
            var dir = Path.Combine(projectPath, ".github", "agents");
            Directory.CreateDirectory(dir);
            var file = Path.Combine(dir, name + ".agent.md");
            File.WriteAllText(file, string.Join("\n", new[]
            {
                "---",
                "description: agente di prova",
                "tools: [read, write]",
                "a2a:",
                "  name: " + name,
                "  role: prova",
                "  accepts_messages_from: [\"*\"]",
                "---",
                "",
                "# " + name,
                "",
                "Fai quello che ti si chiede.",
            }));
            return file;
        }
    }
}
