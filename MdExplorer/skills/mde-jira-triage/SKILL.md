---
name: mde-jira-triage
description: Triage the user's Jira issues and draft an implementation plan as markdown. Use when the user asks to find their most urgent / next Jira issue, or to plan/prepare work for a Jira issue. Read-only on Jira; writes only a local markdown plan into the project's planning folder.
mde:
  origin: mdexplorer
  version: 1
  updatePolicy: replace
---

<!--
MdExplorer-managed skill.
The `mde:` block above marks this file as distributed by MdExplorer. When you
open a project, MdExplorer compares the embedded version with what is on disk
and will overwrite this file to keep it in sync with the current MdExplorer
features. To customize the skill while keeping your edits, remove the `mde:`
block (or change `origin` to something else) — MdExplorer will then leave the
file alone.
-->

# Jira triage → markdown plan

Workflow for an AI agent (Copilot CLI / chat) to turn the user's most urgent Jira
issue into an implementation plan committed as a markdown file, using the
MdExplorer MCP tools. **Read-only on Jira. The only thing written is a local
`.md` file.**

## Tools used (MdExplorer MCP server)

- `GetProjects` — discover the current project name (needed by the tools below).
- `JiraFindMyIssues(project, maxResults?)` — open issues assigned to the user,
  most urgent first. The response also carries `planningFolder` (where to write).
- `JiraGetIssue(project, issueKey)` — full issue detail (description, acceptance
  criteria, comments, links), rich text already flattened to markdown.

## Steps

1. **Resolve the project.** Call `GetProjects`; use the project the user is
   working in. Pass its `name` to the Jira tools.
2. **Find the work.** Call `JiraFindMyIssues`. Take the **first** issue (the list
   is already ordered by priority desc, due date asc). If the user named a
   specific issue, use that key instead. If the list is empty, tell the user
   there are no open issues assigned to them and **stop** — do not write a file.
3. **Read the issue.** Call `JiraGetIssue` for the chosen key. Base the plan
   ONLY on what it returns plus the project's own docs — do not invent
   requirements.
4. **Write the plan.** Create one markdown file at:
   `<planningFolder>/<ISSUE-KEY>-piano.md`
   where `<planningFolder>` is the value returned by the tools (project-relative).
   If `planningFolder` is empty, ask the user where to put the plan before
   writing — do not guess a location.
5. **Confirm.** Report the file path you wrote and a one-line summary. MdExplorer
   indexes the new file automatically; it will appear in the tree.

## Plan document shape

Start with the mandatory TL;DR header (see the `mde-doc` skill), then:

```
## TL;DR
<=3 lines, then exactly 3 bullets>

# <ISSUE-KEY> — <issue summary>

> Jira: <issue url> · Priority: <priority> · Due: <duedate> · Status: <status>

## Context
<what the issue asks for, in your words, from the description + acceptance criteria>

## Plan
1. <step>
2. <step>
...

## Open questions
- <anything ambiguous in the issue that needs the user / reporter to clarify>
```

## Hard rules

- **Never write to Jira** (no comments, no transitions). This MVP is read-only.
- **Never fabricate** issue content; if a field is missing, say so in "Open questions".
- If a tool returns an error about a missing/expired token or disabled
  integration, relay it verbatim and point the user to **Project Settings →
  Atlassian**. Do not retry blindly.
- One plan file per issue; reuse the `<ISSUE-KEY>-piano.md` name so re-running
  updates the same file instead of creating duplicates.
