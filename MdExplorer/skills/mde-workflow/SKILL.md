---
name: mde-workflow
description: Author or change an agents' workflow for MdExplorer's agent city - the `*.workflow.json` that says how people and their agents pass work to each other (who assigns whom, who starts each agent, who waits for whom, how many times rejected work may be redone) and the markdown document that draws it. Use when the user asks to design, describe, review or fix how the agents of a project work together.
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

# Writing an agents' workflow (`*.workflow.json`)

A workflow is **the rule** for how work changes hands in a project's agent city. MdExplorer reads it and applies it;
a markdown document shows it as a diagram that MdExplorer draws from it. Two files, always together:

| File | What it is | Who reads it |
|---|---|---|
| `<name>.workflow.json` | the rule, in the standard below | MdExplorer, and you |
| `workflow.md` (or any name) | `mde_type: workflow`, a short text and the generated diagram | people |

Keep the split with the agent cards sharp:

- **The card** (`.agent.md`, see the `mde-agent` skill) says **how an agent does its job**: what it reads, what it writes, in
  which format.
- **The workflow** says **how people and their agents pass the work**: who assigns whom, who starts each agent, who waits
  for whom, what happens after an approval or a rejection.

Never put the workflow into a card's body, and never put an agent's instructions into the workflow.

## Ask first (if the user has not said)

1. **The people and their agents.** Who takes part, and which agent works for whom (the ownership document,
   `mde_type: ownership`, already says it if it exists: read it).
2. **What starts the process.** Usually one person launching one agent by hand.
3. **For every hand-over: who starts the next agent.** The person responsible for it, from a launch screen (`ask-owner`),
   or nobody because it starts by itself (`auto`)? When two different people are involved, the answer is almost always
   `ask-owner`: an agent works for its person, and its person decides when it starts.
4. **What each agent produces** (the artifact's path) and **who waits for what** (for example: the summary waits for all
   three sheets).
5. **Rework.** After a person rejects a piece of work, how many times may it be started again?

If you cannot ask, decide, write the decision in the document's text, and tell the user.

## The standard, version 1

```json
{
  "mde_workflow": 1,
  "title": "Tender: from the call to the summary",
  "description": "optional, one sentence",
  "steps": [ ... ],
  "loops": [ ... ]
}
```

Keys are English, values are free. **Every key that is not listed here is an error**: there are no optional extras.

### A step (`steps[]`)

A step is **one turn of work of one agent**: what makes it start, who starts it, what it produces.

| Key | Required | Value |
|---|---|---|
| `id` | yes | kebab-case, unique in the file |
| `agent` | yes | the agent's name (`a2a.name` in its card) |
| `title` | no | the label in the diagram, a few words ("Technical sheet"). Without it, the `id` |
| `trigger` | yes | what makes it start: **exactly one** of the forms below |
| `start` | yes | who starts it: `manual`, `ask-owner`, `auto` |
| `produces` | no | artifact paths from the project's root, `/` as separator; `*` only in the file name |

The forms of `trigger`, and the `start` each one allows:

| Form | Means | `start` |
|---|---|---|
| `{ "launch": true }` | a person launches the agent by hand | `manual` |
| `{ "reply": "<reply id>", "to": "<step id>" }` | a person presses a reply button under the message of that step | `auto` |
| `{ "assignment": "<step id>" }` | the agent of that step sends an `[INCARICO]` | `ask-owner`, `auto` |
| `{ "approval": ["<step id>", …], "wait": "all" \| "any" }` | the artifacts of those steps are approved | `ask-owner`, `auto` |

- `reply`: the button is under the message written by the agent of the `to` step, so **the step's `agent` is that same
  agent**, and the reply id must be **declared** in its card (`a2a.replies`). Pressing the button is already the
  person's choice: `start` is `auto`.
- `approval` with more than one step and `wait: all` is **a wait**: the diagram draws it as a join. `wait` defaults to `all`.

### A loop (`loops[]`)

Version 1 has one kind of loop: **rework after a rejection**, restarted by a person.

```json
{ "id": "rework", "steps": ["technical", "legal"], "on": "rejected", "restart": "manual", "max": 2, "then": "stop" }
```

`on`, `restart` and `then` take exactly those values; `max` is an integer from 1. Steps in a loop should declare
`produces`: what is redone is a rejected artifact.

## Rules that make a workflow right

1. **One step per turn of an agent, not per agent.** An agent that searches, and later (after the person presses a
   button) sends the assignments, has **two** steps: the button does not send anything by itself, the agent's second turn
   does. The same agent can appear in many steps.
2. **Everything starts from a launch.** At least one step has `"launch": true`; every other step must be reachable from
   one, through the triggers.
3. **No cycles between steps** in version 1: steps form a chain that never goes back. Redoing rejected work is a loop,
   declared in `loops`, not a trigger that points backwards.
4. **`ask-owner` between people.** When an agent assigns work to another person's agent, that person starts it.
   Use `auto` only when the same person has already decided (a reply button) or when the user asked for it explicitly.
5. **Artifact folders exist.** Every folder in `produces` must already be in the project (with a `README.md`): not every
   engine creates folders.
6. **The cards must agree.** For an `assignment`, the receiving agent's card accepts messages from the sender
   (`accepts_messages_from`); for an `approval`, each producer's card notifies the waiting agent (`on_approval_notify`).
   When they do not, the check gives a warning: tell the user which card to change. Changing `a2a:` in a card asks the
   person to trust the agent again.

## The document

````markdown
---
mde_type: workflow
title: How the work changes hands
workflow: sales.workflow.json
---

# How the work changes hands

## TL;DR
Three lines and three points (see the `mde-doc` skill).

## The round

```plantuml(@workflow, ./sales.workflow.json)
```

One or two sentences on how to read it: each box is a turn of an agent; a click on a box opens the agent's card, a click
on a file opens the artifact.
````

- **Never draw the workflow by hand** and never paste a PlantUML copy of it: the block `plantuml(@workflow, …)` draws it
  from the JSON every time, so it is never out of date. A workflow with errors is not drawn: the errors are shown instead.
- In the diagram: blue = a person launches it, amber = its responsible person starts it, grey = it starts by itself;
  the hexagon is a wait; the red dashed arrow is rework; a dashed file is an artifact not written yet.
- Put the document next to the ownership document, and link the two.

## Check before handing it over

Call **`CheckWorkflow`** (MdExplorer's MCP server, group `agents`) with the project's folder and the file's path, every
time you write or change a workflow.

- `valid: false`: there are `error` issues. Each has the JSON `path` (`steps[2].trigger`), the reason and often the
  `fix`. Fix **all** of them and check again. Do not hand over a workflow with errors.
- `warning` issues: the workflow is valid, but a card routes differently. Tell the user and let them decide.
- An HTTP or connection error means the file was **not** checked: do not change it because of that; say so.

Without the MCP tool, the same check is `GET /api/A2A/workflow/check?projectPath=<folder>&path=<file>` on the running
MdExplorer.

## A complete example

A tender round: the account manager's agent searches, the person presses "Start the round", the agent assigns three
sheets to three colleagues' agents, each colleague starts their own agent, and the summary waits for all three approvals.

```json
{
  "mde_workflow": 1,
  "title": "Tender: from the call to the summary",
  "steps": [
    { "id": "search", "agent": "account-manager", "title": "Search the calls",
      "trigger": { "launch": true }, "start": "manual",
      "produces": ["tender/searches/search-*.md"] },
    { "id": "kick-off", "agent": "account-manager", "title": "Start the round",
      "trigger": { "reply": "start-round", "to": "search" }, "start": "auto" },
    { "id": "technical", "agent": "technical-lead", "title": "Technical sheet",
      "trigger": { "assignment": "kick-off" }, "start": "ask-owner",
      "produces": ["tender/sheets/technical.md"] },
    { "id": "legal", "agent": "legal-lead", "title": "Contract sheet",
      "trigger": { "assignment": "kick-off" }, "start": "ask-owner",
      "produces": ["tender/sheets/contract.md"] },
    { "id": "delivery", "agent": "delivery-lead", "title": "Delivery sheet",
      "trigger": { "assignment": "kick-off" }, "start": "ask-owner",
      "produces": ["tender/sheets/delivery.md"] },
    { "id": "summary", "agent": "account-manager", "title": "Summary to decide",
      "trigger": { "approval": ["technical", "legal", "delivery"], "wait": "all" }, "start": "auto",
      "produces": ["tender/sheets/summary.md"] }
  ],
  "loops": [
    { "id": "rework", "steps": ["technical", "legal", "delivery"],
      "on": "rejected", "restart": "manual", "max": 2, "then": "stop" }
  ]
}
```

The project's demo has the same round in Italian: `citta-degli-agenti/gara/gara.workflow.json` and `workflow.md`.

## Checklist before handing it over

- [ ] `mde_workflow: 1`, a `title`, at least one step with `"launch": true` and `"start": "manual"`.
- [ ] One step per turn of an agent; ids kebab-case and unique; every `agent` exists in the project.
- [ ] Every trigger has exactly one form, and `start` fits it (table above).
- [ ] Hand-overs between different people's agents use `ask-owner`, unless the user asked otherwise.
- [ ] No step points backwards; rework is in `loops`, with a `max`.
- [ ] `produces` paths start from the project's root, and their folders exist.
- [ ] `CheckWorkflow` answers `valid: true`; you told the user about every warning.
- [ ] The document has `mde_type: workflow`, the TL;DR, and the `plantuml(@workflow, …)` block; it links the ownership document.

## What not to do

- Do not draw the workflow in PlantUML by hand, and do not describe it only in prose: the JSON is the rule.
- Do not add keys that are not in the standard ("notes", "color", "owner"): they are errors, not comments. Explanations
  go in the document.
- Do not make a step `auto` to "save a click" when another person is responsible for the agent: that person decides.
- Do not hide a loop in the triggers (A assigns B, B assigns A): version 1 refuses it.
- Do not change an agent card's `a2a:` to make a warning go away without telling the user.
