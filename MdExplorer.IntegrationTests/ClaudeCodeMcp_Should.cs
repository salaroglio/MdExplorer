using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using MdExplorer.Features.Services.AI.ClaudeCode;
using MdExplorer.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// MdExplorer's MCP server for Claude Code: registered for the user when a project targets the
    /// Claude harness, and handed to every MarkAgent Claude Code session. The registration goes
    /// through the real <c>claude</c> CLI, pointed at a temporary <c>CLAUDE_CONFIG_DIR</c> (measured
    /// 13/09/2026: with it set, Claude Code writes <c>$CLAUDE_CONFIG_DIR/.claude.json</c> and nothing
    /// else) — never the <c>~/.claude.json</c> of whoever runs the tests.
    /// <para>Sprint: docs-internal/Sprints/2026-09-13-Harness-Claude-Code.md, phase F2.</para>
    /// </summary>
    [TestClass]
    public class ClaudeCodeMcp_Should
    {
        private string _root;
        private string _configDir;
        private string _previousClaudeConfigDir;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-claude-mcp", Guid.NewGuid().ToString("N"));
            _configDir = Path.Combine(_root, "claude-config");
            Directory.CreateDirectory(_configDir);
            _previousClaudeConfigDir = Environment.GetEnvironmentVariable(ClaudeCodeMcp.ConfigDirVariable);
            Environment.SetEnvironmentVariable(ClaudeCodeMcp.ConfigDirVariable, _configDir);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Environment.SetEnvironmentVariable(ClaudeCodeMcp.ConfigDirVariable, _previousClaudeConfigDir);
            try { if (Directory.Exists(_root)) Directory.Delete(_root, true); }
            catch (IOException) { }
        }

        private static void RequireClaude()
        {
            if (!ClaudeCodeProcessLauncher.IsResolvable())
                Assert.Inconclusive("Claude Code CLI not installed on this machine: the registration cannot be exercised.");
        }

        [TestMethod]
        public void Read_the_user_config_where_claude_code_keeps_it()
        {
            Assert.AreEqual(Path.Combine(_configDir, ".claude.json"), ClaudeCodeMcp.UserConfigFilePath());

            Environment.SetEnvironmentVariable(ClaudeCodeMcp.ConfigDirVariable, null);
            Assert.AreEqual(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".claude.json"),
                ClaudeCodeMcp.UserConfigFilePath());
        }

        [TestMethod]
        public void Write_a_session_config_holding_only_mdexplorer()
        {
            var path = ClaudeCodeMcp.WriteSessionConfig("/opt/mde/MdExplorer.Mcp", _root);

            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            var servers = doc.RootElement.GetProperty("mcpServers");
            Assert.AreEqual(1, servers.EnumerateObject().Count(), "strict config: only MdExplorer's server");
            Assert.AreEqual("/opt/mde/MdExplorer.Mcp", servers.GetProperty("mdexplorer").GetProperty("command").GetString());
            Assert.AreEqual(0, servers.GetProperty("mdexplorer").GetProperty("args").GetArrayLength());
        }

        /// <summary>
        /// I gruppi di funzionalità scelti per il progetto devono arrivare alla sessione: sono la
        /// differenza fra una chat che parte con ~8.000 token di strumenti e una che ne paga 563.
        /// </summary>
        [TestMethod]
        public void Carry_the_chosen_tool_groups_into_the_session_config()
        {
            var path = ClaudeCodeMcp.WriteSessionConfig("/opt/mde/MdExplorer.Mcp", _root, "core,plantuml");

            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            var args = doc.RootElement.GetProperty("mcpServers").GetProperty("mdexplorer").GetProperty("args");
            CollectionAssert.AreEqual(
                new[] { "--groups", "core,plantuml" },
                args.EnumerateArray().Select(a => a.GetString()).ToArray());
        }

        [TestMethod]
        public void Refuse_to_register_without_an_mcp_executable()
        {
            var result = ClaudeCodeMcp.RegisterForUser(null);

            Assert.AreEqual(ClaudeMcpRegistrationOutcome.McpExecutableNotFound, result.Outcome);
            Assert.IsFalse(File.Exists(ClaudeCodeMcp.UserConfigFilePath()), "nothing written when there is nothing to point at");
        }

        [TestMethod]
        public void Register_once_leave_an_identical_entry_alone_and_replace_a_stale_one()
        {
            RequireClaude();
            File.WriteAllText(ClaudeCodeMcp.UserConfigFilePath(),
                "{ \"mcpServers\": { \"altro\": { \"type\": \"stdio\", \"command\": \"/usr/bin/altro\", \"args\": [] } }, \"chiavePropria\": 1 }");

            var first = ClaudeCodeMcp.RegisterForUser("/opt/mde one/MdExplorer.Mcp");
            Assert.AreEqual(ClaudeMcpRegistrationOutcome.Registered, first.Outcome, first.Message);
            Assert.AreEqual("/opt/mde one/MdExplorer.Mcp", ClaudeCodeMcp.RegisteredCommand(), "a path with a space survives the command line");

            var again = ClaudeCodeMcp.RegisterForUser("/opt/mde one/MdExplorer.Mcp");
            Assert.AreEqual(ClaudeMcpRegistrationOutcome.AlreadyRegistered, again.Outcome, again.Message);

            var moved = ClaudeCodeMcp.RegisterForUser("/opt/mde-two/MdExplorer.Mcp");
            Assert.AreEqual(ClaudeMcpRegistrationOutcome.Replaced, moved.Outcome, moved.Message);
            Assert.AreEqual("/opt/mde-two/MdExplorer.Mcp", ClaudeCodeMcp.RegisteredCommand(), "an installation that moved is followed");

            using var doc = JsonDocument.Parse(File.ReadAllText(ClaudeCodeMcp.UserConfigFilePath()));
            Assert.IsTrue(doc.RootElement.GetProperty("mcpServers").TryGetProperty("altro", out _), "the user's own servers are kept");
            Assert.IsTrue(doc.RootElement.TryGetProperty("chiavePropria", out _), "the rest of the file is kept");
        }
    }
}
