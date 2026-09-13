using System;
using System.IO;
using System.Linq;
using MdExplorer.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// The Claude Code harness: where MdExplorer installs its skills, agents, commands and project
    /// instructions for a project whose agent is Claude Code. The paths are those a functional
    /// probe on Claude Code 2.1.270 showed it reads (13/09/2026).
    /// <para>Sprint: docs-internal/Sprints/2026-09-13-Harness-Claude-Code.md, phase F1.</para>
    /// </summary>
    [TestClass]
    public class ClaudeHarness_Should
    {
        private string _root;
        private string _previousClaudeConfigDir;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-claude-harness", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
            // ConfigTemplates with the Claude harness registers the MCP server through `claude mcp add
            // --scope user`: without this it would write into the ~/.claude.json of whoever runs the tests.
            _previousClaudeConfigDir = Environment.GetEnvironmentVariable(ClaudeCodeMcp.ConfigDirVariable);
            var configDir = Path.Combine(_root, "claude-config");
            Directory.CreateDirectory(configDir);
            Environment.SetEnvironmentVariable(ClaudeCodeMcp.ConfigDirVariable, configDir);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Environment.SetEnvironmentVariable(ClaudeCodeMcp.ConfigDirVariable, _previousClaudeConfigDir);
            try { if (Directory.Exists(_root)) Directory.Delete(_root, true); }
            catch (IOException) { }
        }

        private string[] ProducedFiles(string root)
            => Directory.GetFiles(root, "*", SearchOption.AllDirectories)
                .Select(f => f.Substring(root.Length + 1).Replace(Path.DirectorySeparatorChar, '/'))
                .OrderBy(f => f, StringComparer.Ordinal)
                .ToArray();

        [TestMethod]
        public void Describe_the_paths_claude_code_reads()
        {
            var cl = HarnessLayout.Claude;

            Assert.AreEqual("claude", cl.Id);
            Assert.AreEqual(".claude", cl.RootFolder);
            Assert.AreEqual(".claude/skills/mde-doc/SKILL.md", cl.SkillRelativePath("mde-doc"));
            Assert.AreEqual(".claude/agents/mde-skillcreator.md", cl.AgentRelativePath("mde-skillcreator"));
            Assert.AreEqual(".claude/commands/mde-mark-summarize.md", cl.PromptRelativePath("mde-mark-summarize"));
            Assert.AreEqual("CLAUDE.md", cl.InstructionsFile, "Claude Code ignores AGENTS.md");
            CollectionAssert.Contains(HarnessLayout.All.ToList(), cl);
            Assert.AreSame(cl, HarnessLayout.For(HarnessTarget.Claude));
        }

        [TestMethod]
        public void Parse_claude_as_a_harness_id()
        {
            Assert.IsTrue(HarnessLayout.TryParseId(" Claude ", out var target));
            Assert.AreEqual(HarnessTarget.Claude, target);
            StringAssert.Contains(HarnessLayout.AllowedIds, "claude");
            Assert.AreEqual("claude", HarnessSettings.IdOf(HarnessTarget.Claude));
        }

        [TestMethod]
        public void Install_the_catalogs_where_claude_code_reads_them()
        {
            MdeSkillUpdater.EnsureCatalogsInstalled(_root, HarnessLayout.Claude, fusekiEnabled: false);

            CollectionAssert.AreEqual(
                new[]
                {
                    ".claude/agents/mde-skillcreator.md",
                    ".claude/commands/mde-codegen-graph.md",
                    ".claude/commands/mde-mark-folder-synthesis.md",
                    ".claude/commands/mde-mark-summarize.md",
                    ".claude/skills/mde-doc/SKILL.md",
                    ".claude/skills/mde-features/SKILL.md",
                    ".claude/skills/mde-plantuml/SKILL.md",
                    ".claude/skills/mde-prompt-for-agents/SKILL.md",
                    ".claude/skills/mde-readme/SKILL.md",
                },
                ProducedFiles(_root),
                "agents and commands are flat <name>.md files, skills stay in their folder");
        }

        [TestMethod]
        public void Give_the_agent_the_frontmatter_claude_code_understands()
        {
            MdeSkillUpdater.EnsureCatalogsInstalled(_root, HarnessLayout.Claude, fusekiEnabled: false);

            var agent = File.ReadAllText(HarnessLayout.Claude.AgentFullPath(_root, "mde-skillcreator"));

            StringAssert.StartsWith(agent, "---\nname: mde-skillcreator\n", "Claude Code identifies an agent by its name");
            StringAssert.Contains(agent, "tools: Read, Write, Edit, Grep, Glob");
            Assert.IsFalse(agent.Contains("mode: subagent"), "opencode's keys do not belong here");
            Assert.IsFalse(agent.Contains("tools: [read"), "Copilot's tool names do not belong here");
            Assert.AreEqual("mdexplorer", MdeSkillUpdater.ExtractMdeMarker(agent).Origin,
                "the mde: marker survives: Claude Code ignores the keys it does not know");
        }

        [TestMethod]
        public void Give_the_claude_agent_the_same_body_and_version_as_the_other_layouts()
        {
            var copilotRoot = Path.Combine(_root, "cp");
            var claudeRoot = Path.Combine(_root, "cl");
            Directory.CreateDirectory(copilotRoot);
            Directory.CreateDirectory(claudeRoot);

            MdeSkillUpdater.EnsureCatalogsInstalled(copilotRoot, HarnessLayout.Copilot, fusekiEnabled: false);
            MdeSkillUpdater.EnsureCatalogsInstalled(claudeRoot, HarnessLayout.Claude, fusekiEnabled: false);

            var copilotAgent = File.ReadAllText(HarnessLayout.Copilot.AgentFullPath(copilotRoot, "mde-skillcreator"));
            var claudeAgent = File.ReadAllText(HarnessLayout.Claude.AgentFullPath(claudeRoot, "mde-skillcreator"));

            Assert.AreEqual(Body(copilotAgent), Body(claudeAgent), "the agents must differ ONLY in the frontmatter");
            Assert.AreEqual(
                MdeSkillUpdater.ExtractMdeMarker(copilotAgent).Version,
                MdeSkillUpdater.ExtractMdeMarker(claudeAgent).Version,
                "the version lives in separate frontmatters: they must stay aligned");
        }

        /// <summary>Text after the second '---' line: the body without the frontmatter.</summary>
        private static string Body(string content)
        {
            var first = content.IndexOf("---", StringComparison.Ordinal);
            var second = content.IndexOf("---", first + 3, StringComparison.Ordinal);
            return content.Substring(second + 3);
        }

        [TestMethod]
        public void Create_claude_md_with_the_claude_paths_and_never_overwrite_the_projects_own()
        {
            var fresh = Path.Combine(_root, "fresh");
            var owned = Path.Combine(_root, "owned");
            Directory.CreateDirectory(fresh);
            Directory.CreateDirectory(owned);
            const string mine = "# My own instructions\n";
            File.WriteAllText(Path.Combine(owned, "CLAUDE.md"), mine);

            MdExplorer.Service.ProjectsManager.ConfigTemplates(fresh, null, HarnessTarget.Claude);
            MdExplorer.Service.ProjectsManager.ConfigTemplates(owned, null, HarnessTarget.Claude);

            var created = File.ReadAllText(Path.Combine(fresh, "CLAUDE.md"));
            StringAssert.Contains(created, ".claude/skills/mde-doc/SKILL.md", "the instructions must point at the Claude layout");
            Assert.IsFalse(created.Contains(".opencode") || created.Contains(".github/skills"), "no other layout's paths");
            Assert.IsFalse(File.Exists(Path.Combine(fresh, "AGENTS.md")), "AGENTS.md is opencode's, Claude Code ignores it");
            Assert.AreEqual(HarnessTarget.Claude, HarnessSettings.Read(fresh), "the choice is written in .development.yml");
            Assert.IsTrue(File.Exists(HarnessLayout.Claude.SkillFullPath(fresh, "mde-doc")));

            Assert.AreEqual(mine, File.ReadAllText(Path.Combine(owned, "CLAUDE.md")),
                "a CLAUDE.md the project already has is the team's: never touched");
        }

        [TestMethod]
        public void Not_deduce_the_claude_harness_from_a_claude_folder_on_disk()
        {
            // .claude/ sits in many repositories for reasons of its own (settings.local.json).
            Directory.CreateDirectory(Path.Combine(_root, ".claude"));
            File.WriteAllText(Path.Combine(_root, ".claude", "settings.local.json"), "{}");

            Assert.AreEqual(HarnessTarget.None, HarnessSettings.DetectFromDisk(_root));
        }

        [TestMethod]
        public void Keep_the_harness_folder_out_of_yaml_auto_generation()
        {
            File.WriteAllText(Path.Combine(_root, ".development.yml"), "folders: []\n");

            HarnessSettings.Write(_root, HarnessTarget.Claude);

            var yml = File.ReadAllText(Path.Combine(_root, ".development.yml"));
            StringAssert.Contains(yml, "target: claude");
            StringAssert.Contains(yml, ".claude", "MdExplorer must not write front matter into its own skills");
        }
    }
}
