# MdExplorer

> **The natural-language IDE.** An open source desktop application where people and AI agents work on the same documents: markdown files, kept in git, that have value as data and as algorithms.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Website](https://img.shields.io/badge/site-mdexplorer.net-1a73e8)](https://www.mdexplorer.net)
[![Download](https://img.shields.io/badge/download-Windows%20%7C%20Linux-e8710a)](https://www.mdexplorer.net/download/)

**Site and documentation: [www.mdexplorer.net](https://www.mdexplorer.net)** · [Download](https://www.mdexplorer.net/download/) · [Demo project](https://github.com/salaroglio/mdexplorer-demo) · [Italiano](https://www.mdexplorer.net/it/)

## What it is

A development environment has an editor, a compiler, a runtime, version control and tests. MdExplorer has the same parts, and the language they work on is the one you already write: plain text, in markdown.

- **Documents are data.** A document includes the real files of the project, draws them as diagrams and stays aligned by itself. Nothing is copied, so nothing goes stale.
- **Documents are algorithms.** A skill, a house rule, a workflow, a test written in plain words: text files that tell an AI agent how to work, reviewed and approved like any other document.
- **The AI is the runtime.** GitHub Copilot, Claude Code or opencode: one engine, chosen once per project. There is no built-in model.
- **Git is the control.** Everything an agent writes is a change you read before accepting it. Every change has an author and a date.

It runs on Windows and Linux. Your documents stay on your computer and in your git.

## What you do with it

| | |
|---|---|
| [Living documents](https://www.mdexplorer.net/features/living-documents/) | Include real files, run a command from the page, correct a sentence with a right click, paste a screenshot where it belongs. |
| [Diagrams that answer](https://www.mdexplorer.net/features/diagrams/) | PlantUML written as text. A click lights up the relations; a right click asks the agent to explain. |
| [MarkAgent](https://www.mdexplorer.net/features/markagent/) | The AI agent that reads and writes the documents of the project, with the engine and the rules the project carries with it. |
| [Presentations](https://www.mdexplorer.net/features/presentations/) | Slides are a markdown file with one more line in the header. |
| [Word, PDF and site](https://www.mdexplorer.net/features/export/) | Markdown stays the only source; the delivery formats are regenerated when needed. |
| [Git without the terminal](https://www.mdexplorer.net/features/git/) | What changed, who changed it, how to save it, for people who never opened a terminal. |
| [Rules, skills and MCP](https://www.mdexplorer.net/features/rules-skills-mcp/) | Skills in the repository and an MCP server that gives the agent the project's knowledge. |
| [Tests in plain language](https://www.mdexplorer.net/features/e2e-tests/) | The agent checks a web site on a real browser and leaves screenshots, a report and a script that replays without AI. |
| [Integrations](https://www.mdexplorer.net/features/integrations/) | Redmine, Jira and Confluence read and updated by the agent with your personal key; notices on a private Telegram channel. |

### The agent city

AI agents can live in a project, each with a name, a role and a person who answers for it. The agent prepares, the person checks and approves; who works after whom is written in the project's workflow, and MdExplorer runs it. No agent passes work to another by itself, and nothing enters the project without an approval. → [The agent city](https://www.mdexplorer.net/city-of-agents/)

### The code graph

MdExplorer also reads source code. Mark a folder as a development environment and the program becomes a graph of classes, methods and relations, each with its line and with how sure it is. A deterministic parser builds the map (Roslyn for C# and Visual Basic, tree-sitter for TypeScript, JavaScript, Python and Java, declarative language packages for the rest); the AI only reasons over it and answers with file and line.

The graph is stored in git as Turtle (RDF), one file per source file, in `.mde-code/`. Its vocabulary is open: [`https://mdexplorer.net/ns/code#`](https://www.mdexplorer.net/code-graph/ontology/). → [The code graph](https://www.mdexplorer.net/code-graph/)

## Install

Download the installer for Windows or the AppImage for Linux from **[mdexplorer.net/download](https://www.mdexplorer.net/download/)**. Installed copies update themselves.

The fastest way to see everything is the demo project: after installing, open Mark's guide (the question mark) and choose «Create demo project». The same project is public at [salaroglio/mdexplorer-demo](https://github.com/salaroglio/mdexplorer-demo) (branch `main` in Italian, branch `en` in English).

To use the AI functions, install and sign in to one of the command-line tools `copilot`, `claude` or `opencode`. → [Getting started](https://www.mdexplorer.net/docs/)

## For AI agents

Every page of the site is also published as plain markdown: add `index.md` to the address of any page. Start from [`llms.txt`](https://www.mdexplorer.net/llms.txt) or from [the page written for agents](https://www.mdexplorer.net/for-agents/). This repository has its own [`llms.txt`](llms.txt).

## Build from source

```bash
git clone --recursive https://github.com/salaroglio/MdExplorer.git
cd MdExplorer

dotnet build                                   # backend, .NET 8

cd MdExplorer/client2 && npm install && npm run build && cd ../..   # Angular frontend

dotnet run --project MdExplorer/MdExplorer.Service.csproj

cd ElectronMdExplorer && npm install && npm start                   # desktop application
```

`--recursive` brings in the submodules (the Electron application, the website, the demo project). On Linux, `build-linux.sh` builds everything. See [CONTRIBUTING.md](CONTRIBUTING.md) for the development setup.

### Technology

- **Backend**: ASP.NET Core 8, NHibernate, FluentMigrator, SignalR, SQLite
- **Frontend**: Angular 15 with Material Design; editor in React 19 with Milkdown
- **Desktop**: Electron, with automatic updates
- **Git**: LibGit2Sharp
- **Code graph**: Roslyn, tree-sitter, RDF/Turtle, SPARQL
- **Agents**: an MCP server (`MdExplorer.Mcp`) and the command-line tools of GitHub Copilot, Claude Code and opencode

### Repository

```
MdExplorer/                  ASP.NET Core service
  client2/                   Angular frontend
MdExplorer.bll/              features and business logic (code graph included)
MdExplorer.Abstractions/     domain models and interfaces
MdExplorer.Mcp/              MCP server for the agents
MdEditor.React/              Milkdown editor
ElectronMdExplorer/          desktop application (submodule)
mdexplorer-demo/             demo project (submodule)
mdExplorerWebSite/           www.mdexplorer.net (submodule)
sprints/                     sprint plans, in markdown
```

## Built with itself

MdExplorer is developed with the method it proposes: a sprint plan in markdown, an agent that carries out a phase, a trial in the real application, a commit, and the plan updated. The [`sprints/`](sprints/) folder and [CLAUDE.md](CLAUDE.md) are the working documents.

## Contributing, security, support

- Contributions are welcome: [CONTRIBUTING.md](CONTRIBUTING.md), [Code of Conduct](CODE_OF_CONDUCT.md)
- Security: report vulnerabilities to security@mdexplorer.net, see [SECURITY.md](SECURITY.md)
- [Issues](https://github.com/salaroglio/MdExplorer/issues) · [Discussions](https://github.com/salaroglio/MdExplorer/discussions) · developer@mdexplorer.net

## Author and citation

**MdExplorer is created and maintained by [Carlo Salaroglio](https://github.com/salaroglio)**, a software engineer based in Italy. The project began in March 2021 and has been open source under the MIT License since October 2025.

If you reference MdExplorer in articles, posts or other publications, please credit Carlo Salaroglio and link to this repository. For academic citation use [CITATION.cff](CITATION.cff) («Cite this repository» in the GitHub sidebar).

## License

MIT License, see [LICENSE](LICENSE). Third-party libraries and their licences: [librerie-licenze.md](librerie-licenze.md).
