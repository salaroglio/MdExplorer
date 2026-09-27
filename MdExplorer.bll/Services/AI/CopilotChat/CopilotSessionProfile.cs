using System;
using System.Collections.Generic;

namespace MdExplorer.Features.Services.AI.CopilotChat
{
    /// <summary>An MCP server started by the Copilot session itself (stdio).</summary>
    public sealed record CopilotMcpServer(string Command, IReadOnlyList<string> Args, IReadOnlyDictionary<string, string> Env);

    /// <summary>
    /// The configuration of a Copilot SDK session that runs e2e tests (sprint
    /// docs-internal/Sprints/2026-09-26-Test-E2E-Da-Markdown.md, F4c): its own MCP servers — MdExplorer's
    /// and Playwright, declared explicitly so the session does not depend on the user's global
    /// <c>~/.copilot/mcp-config.json</c> — no shell (D29: a <c>cat</c> would read the credentials past any
    /// file rule), and files it must not read.
    /// </summary>
    public sealed class CopilotSessionProfile
    {
        /// <summary>Two sessions with the same key have the same configuration.</summary>
        public string Key { get; init; }

        public IReadOnlyDictionary<string, CopilotMcpServer> McpServers { get; init; } = new Dictionary<string, CopilotMcpServer>();

        /// <summary>Absolute paths the agent must not read.</summary>
        public IReadOnlyList<string> DeniedReadPaths { get; init; } = Array.Empty<string>();

        /// <summary>File-name patterns (<c>*</c> wildcard) the agent must not read anywhere: every credentials file of the project.</summary>
        public IReadOnlyList<string> DeniedReadNames { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Paths relative to the project the agent must not write: folders (ending with <c>/</c>) and files. The
        /// agents' own configuration (hooks, settings, instructions) would run or be read with the shell in the
        /// next session.
        /// </summary>
        public IReadOnlyList<string> DeniedWritePaths { get; init; } = Array.Empty<string>();

        public bool DenyShell { get; init; } = true;
    }
}
