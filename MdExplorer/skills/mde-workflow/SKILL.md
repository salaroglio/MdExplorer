---
name: mde-workflow
description: Plan an agents' workflow for MdExplorer's agent city - the `*.workflow.json` that MdExplorer executes as a scheduler (which steps there are, what starts each one, who starts it, what the agent receives, which steps wait for which, which steps repeat) and the markdown document that draws it. Use when the user asks to design, describe, review or fix how the people and the agents of a project work together.
mde:
  origin: mdexplorer
  version: 3
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

# Planning an agents' workflow (`*.workflow.json`)

The workflow is the **common vocabulary** between you and MdExplorer. **You plan** the work in it; **MdExplorer executes
it** with fixed rules, like a scheduler: when a step is finished it starts the next ones, it waits where you say to wait,
it repeats what you say to repeat. **Agents do only their own step** — what their card says — and write to the person;
they never decide who comes next.

Two files, always together:

| File | What it is | Who reads it |
|---|---|---|
| `<name>.workflow.json` | the plan, in the vocabulary below | MdExplorer, and you |
| `workflow.md` (or any name) | `mde_type: workflow`, a short text and the diagram MdExplorer draws from the JSON | people |

The split with the agent cards (`.agent.md`, see the `mde-agent` skill):

- **The card** says **how an agent does its step**: what it reads, what it writes, in which format, what it tells the person.
- **The workflow** says **how the work moves**: which steps, in which order, who starts each one, what each agent receives.

Never put the workflow into a card's body ("then send an assignment to X", "when the three sheets are approved…"), and
never put an agent's working instructions into the workflow.

## What MdExplorer does with it

When the project points at the workflow's document (`agentCity.workflowDoc` in `.development.yml`, the "Workflow
document" field of the project settings):

- A step starts when what it waits for has happened; MdExplorer sends its agent the step's **`brief`**, with the round's
  variables filled in and the paths of the artifacts of the steps it waits for.
- **`ask-owner`**: the step waits in its owner's mail as "To start"; the owner starts it from the launch screen (adding
  instructions, choosing engine and model) or declines it with a reason.
- A message between agents that the workflow does not describe is **refused**: hand-overs are the workflow's, not the
  agents'.
- **Loops are yours.** MdExplorer imposes no limit: with no loop, "Restart" after a rejection is always possible; with a
  loop, it does exactly what the loop says.
- A workflow with errors stops the hand-overs and says so: without the plan MdExplorer cannot know who starts what.

## Ask first (if the user has not said)

1. **The people and their agents.** Who takes part, and which agent works for whom (the ownership document,
   `mde_type: ownership`, says it if it exists: read it).
2. **What starts the process.** Usually one person launching one agent by hand.
3. **The steps and their order.** What each agent produces, and which steps can run side by side.
4. **Who starts each step.** Its owner from a launch screen (`ask-owner`), or nobody because it starts by itself (`auto`)?
   When the step's agent works for a different person than the one who triggered it, the answer is almost always
   `ask-owner`: an agent works for its person, and its person decides when it starts.
5. **The choices the person makes on the way** (a reply button, for example "start the round on tender X") and the values
   they carry: they become the round's variables.
6. **Repetitions.** Does some work get redone after a rejection, by whom, and how many times at most? Is some work done a
   fixed number of rounds on purpose?

If you cannot ask, decide, write the decision in the document's text, and tell the user.

## The vocabulary, version 2

```json
{
  "mde_workflow": 2,
  "title": "Tender: from the call to the summary",
  "description": "optional, one sentence",
  "variables": { "code": "the code of the tender chosen with «Start the round»" },
  "steps": [ ... ],
  "loops": [ ... ]
}
```

Keys are English, values are free. **Every key that is not listed here is an error**: there are no optional extras.

### A step (`steps[]`)

A step is **one turn of work of one agent**.

| Key | Required | Value |
|---|---|---|
| `id` | yes | kebab-case, unique in the file |
| `agent` | yes | the agent's name (`a2a.name` in its card) |
| `title` | no | the label in the diagram, a few words ("Technical sheet"). Without it, the `id` |
| `trigger` | yes | what makes it start: **exactly one** of the forms below |
| `start` | yes | who starts it: `manual`, `ask-owner`, `auto` |
| `brief` | yes, except for a launch | what MdExplorer sends the agent when the step starts, with `{variables}` |
| `produces` | no | artifact paths from the project's root, `/` as separator; `*` only in the file name |

The forms of `trigger`, and the `start` each one allows:

| Form | Means | `start` |
|---|---|---|
| `{ "launch": true }` | a person launches the agent by hand | `manual` |
| `{ "reply": "<reply id>", "to": "<step id>" }` | a person presses a reply button under the message of that step | `ask-owner`, `auto` |
| `{ "after": ["<step id>", …], "wait": "all" \| "any" }` | those steps are finished | `ask-owner`, `auto` |

- **Finished** means: its artifact is approved, for a step that `produces`; its agent has finished, for one that does not.
- `after` with more than one step is **a wait**: `all` (default) starts at the last one, `any` at the first.
- `reply`: the button is declared in the card of the agent of the `to` step (`a2a.replies`). The step it starts can be of
  **any** agent: pressing the button is the person's choice, MdExplorer does the rest. The values the button carries
  (`{code}` in the reply's texts) become the round's variables: **declare each one in `variables`**.

### The brief

The brief is the assignment: what the agent must do in this step, in a sentence or two, with the round's variables.

- Write **what to produce and on what**, not how ("Write the technical sheet on tender {code}."): the how is in the card.
- Every `{name}` must be declared in `variables`.
- Do not list the input files: MdExplorer adds the paths of the artifacts of the steps this one waits for.

### A loop (`loops[]`)

Two kinds, both the author's choice. A step is in at most **one loop of each kind**.

**"Until"** — after a rejection the step is redone, until it is approved:

```json
{ "id": "rework", "steps": ["technical", "legal"], "until": "approved", "restart": "manual", "max": 3 }
```

- `restart`: `manual` (the person presses "Restart", the usual choice) or `auto` (it restarts by itself, with the reason
  of the rejection).
- `max`: how many times at most; **leave it out for no limit**.

**"For"** — the step is done a fixed number of rounds on purpose, each one approved; the next steps start after the last:

```json
{ "id": "review-rounds", "steps": ["draft"], "times": 3 }
```

**Both on the same step**: each round of the "for" is redone after a rejection as the "until" says. "Three review rounds,
each approved; a rejected round is redone at most twice" is a `times: 3` loop and an `until` loop with `max: 2` on the
same step.

Without a loop a rejected step can still be restarted, as many times as the person wants: write a loop only when the
repetition is part of the plan.

## Rules that make a plan right

1. **One step per turn of an agent, not per agent.** The same agent can appear in many steps (it searches, and later it
   writes the summary).
2. **Everything starts from a launch.** Every step must be reachable from a `"launch": true` through the triggers.
3. **No step points backwards.** Steps form a chain; repetitions are loops, not triggers that go back.
4. **`ask-owner` between people.** Use `auto` only when the same person already decided, or when the user asked for it.
5. **Artifact folders exist.** Every folder in `produces` must already be in the project (with a `README.md`).
6. **Cards do not route.** If a card tells its agent to write to a colleague, that message will be refused: remove it from
   the card, and make it a step.

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

- **Never draw the workflow by hand**: the block `plantuml(@workflow, …)` draws it from the JSON every time. A workflow
  with errors is not drawn: the errors are shown instead.
- In the diagram: blue = a person launches it, amber = its owner starts it, grey = it starts by itself; the hexagon is a
  wait; the red dashed arrow is "until approved"; the thick blue arrow is "for N rounds"; a dashed file is an artifact not
  written yet.
- Put the document next to the ownership document, and link the two.

## Check before handing it over

Call **`CheckWorkflow`** (MdExplorer's MCP server, group `agents`) with the project's folder and the file's path, every
time you write or change a workflow.

- `valid: false`: there are `error` issues. Each has the JSON `path` (`steps[2].trigger`), the reason and often the
  `fix`. Fix **all** of them and check again. Do not hand over a workflow with errors.
- `warning` issues: the plan is valid but something looks wrong (for example a loop "until approved" on a step that
  produces nothing). Tell the user.
- An HTTP or connection error means the file was **not** checked: do not change it because of that; say so.

Without the MCP tool, the same check is `GET /api/A2A/workflow/check?projectPath=<folder>&path=<file>` on the running
MdExplorer.

## A complete example

A tender round: the account manager's agent searches; the person presses "Start the round on {code}" under the search;
three colleagues' agents write their sheets, each started by its owner; the summary waits for all three approvals. A
rejected sheet is redone when the person says so, with no limit.

```json
{
  "mde_workflow": 2,
  "title": "Tender: from the call to the summary",
  "variables": { "code": "the code of the tender chosen with «Start the round»" },
  "steps": [
    { "id": "search", "agent": "account-manager", "title": "Search the calls",
      "trigger": { "launch": true }, "start": "manual",
      "produces": ["tender/searches/search-*.md"] },
    { "id": "technical", "agent": "technical-lead", "title": "Technical sheet",
      "trigger": { "reply": "start-round", "to": "search" }, "start": "ask-owner",
      "brief": "Write the technical feasibility sheet on tender {code}.",
      "produces": ["tender/sheets/technical.md"] },
    { "id": "legal", "agent": "legal-lead", "title": "Contract sheet",
      "trigger": { "reply": "start-round", "to": "search" }, "start": "ask-owner",
      "brief": "Write the contract sheet on tender {code}.",
      "produces": ["tender/sheets/contract.md"] },
    { "id": "delivery", "agent": "delivery-lead", "title": "Delivery sheet",
      "trigger": { "reply": "start-round", "to": "search" }, "start": "ask-owner",
      "brief": "Write the delivery sheet (team and timing) on tender {code}.",
      "produces": ["tender/sheets/delivery.md"] },
    { "id": "summary", "agent": "account-manager", "title": "Summary to decide",
      "trigger": { "after": ["technical", "legal", "delivery"], "wait": "all" }, "start": "auto",
      "brief": "The three sheets on tender {code} are approved: write the summary to decide whether to bid.",
      "produces": ["tender/sheets/summary.md"] }
  ],
  "loops": [
    { "id": "rework", "steps": ["technical", "legal", "delivery"], "until": "approved", "restart": "manual" }
  ]
}
```

The project's demo has the round in Italian: `citta-degli-agenti/gara/gara.workflow.json` and `workflow.md`.

## Checklist before handing it over

- [ ] `mde_workflow: 2`, a `title`, at least one step with `"launch": true` and `"start": "manual"`.
- [ ] One step per turn of an agent; ids kebab-case and unique; every `agent` exists in the project.
- [ ] Every trigger has exactly one form, and `start` fits it (table above).
- [ ] Every step that is not a launch has a `brief`; every `{variable}` is declared in `variables`.
- [ ] Every value a reply button carries is declared in `variables`.
- [ ] No step points backwards; repetitions are loops, with the limits the user wants (or none).
- [ ] `produces` paths start from the project's root, and their folders exist.
- [ ] The cards do not route (no "send an assignment to…", no counting of colleagues' sheets).
- [ ] `CheckWorkflow` answers `valid: true`; you told the user about every warning.
- [ ] The document has `mde_type: workflow`, the TL;DR, and the `plantuml(@workflow, …)` block; it links the ownership document.

## What not to do

- Do not draw the workflow in PlantUML by hand, and do not describe it only in prose: the JSON is the plan.
- Do not add keys that are not in the vocabulary ("notes", "color", "owner"): they are errors, not comments. Explanations
  go in the document.
- Do not add a limit nobody asked for: a loop's `max` is the user's decision.
- Do not make a step `auto` to "save a click" when another person is responsible for its agent: that person decides.
- Do not hide a repetition in the triggers (A after B, B after A): it is refused; write a loop.
