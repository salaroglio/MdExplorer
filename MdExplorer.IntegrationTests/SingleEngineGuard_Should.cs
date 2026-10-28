using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MdExplorer.Abstractions.Services;
using MdExplorer.Features.Services.AI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Guard of sprint 2026-09-29-Motore-LLM-Unico (F6): every access to the LLM goes through the engine of the project
    /// (the MarkAgent tab's session, a dedicated session of that engine, or an agent's engine). A server class that gets
    /// hold of an <see cref="IAiProvider"/> by itself picks an engine on its own — it is how «spiega il diagramma»
    /// started Copilot while the tab used Claude. The allowed ones are listed with the reason; a new one makes this
    /// test fail, and the answer is to use the project's engine, not to extend the list without thinking.
    /// </summary>
    [TestClass]
    public class SingleEngineGuard_Should
    {
        private static readonly Dictionary<string, string> Allowed = new()
        {
            ["MdExplorer.Hubs.AiChatHub"] = "the MarkAgent tab: the engine chosen there, saved in the project",
            ["MdExplorer.Service.Controllers.MdProjects.MdProjectsController"] = "availability probe of the project's engine when a project opens",
            ["MdExplorer.Services.AgentRun.CopilotTurnRunner"] = "an agent's turn on Copilot, chosen by EngineTurnRunner",
            ["MdExplorer.Controllers.AI.AiProvidersController"] = "settings: which providers exist and their models (no chat)",
            ["MdExplorer.Controllers.AI.CopilotCliController"] = "settings of Copilot CLI: version, models, system prompt (no chat)",
            ["MdExplorer.Controllers.AI.ClaudeCodeController"] = "settings of Claude Code: version, models (no chat)",
            ["MdExplorer.Controllers.AI.OpenCodeController"] = "settings of opencode: models (no chat)",
            ["MdExplorer.Controllers.AI.OpenAiController"] = "OpenAI, outside the rule for now (D4)",
            ["MdExplorer.Controllers.AI.AiModelsController"] = "local model management: download, load, prompts (no chat; D4)",
        };

        private static bool IsProviderType(Type t)
        {
            if (t == typeof(IAiProvider) || typeof(IAiProvider).IsAssignableFrom(t) && !t.IsInterface) return true;
            if (t.IsGenericType && t.GetGenericArguments().Any(IsProviderType)) return true;
            return false;
        }

        [TestMethod]
        public void Let_only_the_known_classes_take_an_AI_provider()
        {
            var assemblies = new[] { typeof(MdExplorer.Startup).Assembly, typeof(CopilotCliProvider).Assembly };
            var takers = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !typeof(IAiProvider).IsAssignableFrom(t))
                .Where(t => t.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Any(c => c.GetParameters().Any(p => IsProviderType(p.ParameterType))))
                .Select(t => t.FullName)
                .ToList();

            var unexpected = takers.Where(t => !Allowed.ContainsKey(t)).ToList();
            Assert.AreEqual(0, unexpected.Count,
                "These classes take an AI provider and so choose an engine by themselves: " + string.Join(", ", unexpected) +
                ". Use the project's engine (ProjectsManager.ProjectEngine, OneShotReadOnlySession, the tab's channel).");
            Assert.IsTrue(takers.Count > 0, "the scan found nothing: the guard would pass without looking");
        }

        [TestMethod]
        public void Not_resolve_AI_providers_from_the_container_by_hand()
        {
            // The constructor check cannot see a GetService call: the service locator is how AiToolsTestController
            // reached the providers (removed in F6).
            var root = FindRepositoryRoot();
            var offenders = Directory.EnumerateFiles(Path.Combine(root, "MdExplorer"), "*.cs", SearchOption.AllDirectories)
                .Concat(Directory.EnumerateFiles(Path.Combine(root, "MdExplorer.bll"), "*.cs", SearchOption.AllDirectories))
                .Where(f => !f.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar)
                            && !f.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar))
                .Where(f =>
                {
                    var text = File.ReadAllText(f);
                    return text.Contains("GetServices<IAiProvider>") || text.Contains("GetService<IEnumerable<IAiProvider>>")
                        || text.Contains("typeof(System.Collections.Generic.IEnumerable<IAiProvider>)")
                        || text.Contains("typeof(IEnumerable<IAiProvider>)") || text.Contains("typeof(IAiProvider)");
                })
                .Select(f => Path.GetRelativePath(root, f))
                .ToList();

            Assert.AreEqual(0, offenders.Count, "AI providers resolved by hand in: " + string.Join(", ", offenders));
        }

        private static string FindRepositoryRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "MdExplorer.sln"))) dir = dir.Parent;
            return dir?.FullName ?? throw new InvalidOperationException("MdExplorer.sln not found above " + AppContext.BaseDirectory);
        }
    }
}
