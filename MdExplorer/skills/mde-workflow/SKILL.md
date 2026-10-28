---
name: mde-workflow
description: Plan and change an agents' workflow for MdExplorer's agent city - the `*.workflow.json` that MdExplorer executes as a scheduler (which steps there are, which agent job or which person does each one, what starts it, what it receives, who approves it, which steps repeat), together with the interfaces of the agent cards it connects (inputs, reads, outputs, buttons) and the markdown document that draws it. Use when the user asks to design, describe, review, change or fix how the people and the agents of a project work together, or to add, remove or rewire an input or an output of an agent.
mde:
  origin: mdexplorer
  version: 5
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

# Planning an agents' workflow (`*.workflow.json`, version 3)

The workflow is the **common vocabulary** between you and MdExplorer. **You plan** the work in it; **MdExplorer executes
it** with fixed rules, like a scheduler: when a step is finished it starts the next ones, it writes each agent's
assignment, it asks each person for their decision, it waits where you say to wait, it repeats what you say to repeat.
**Agents do only their own job** — what their card says — and write to the person; they never decide who comes next.

This skill owns **a pair**: the workflow and the **interfaces** of the agent cards it connects. Change them together,
with the procedure below.

| File | What it says | Who reads it |
|---|---|---|
| `.agent.md` → `a2a.skills[]` (the **interface**) | what each job of the agent **receives** (`inputs`), **reads** (`reads`), **delivers** (`outputs`) and which **buttons** it may offer (`replies`) | MdExplorer, the person who trusts the agent |
| `<name>.workflow.json` | **the links**: which job or which person does each step, what starts it, where each input comes from, who approves | MdExplorer, you |
| `workflow.md` (any name) | `mde_type: workflow`, a short text and the diagram MdExplorer draws from the two | people |
| `responsabilita.md` | `mde_type: ownership`: the **roles** (column `Ruolo`), their people and agents | MdExplorer, people |

The rule of the split:

- **A card never names another agent or person.** Its interface says "I receive the tender document", not "the account
  manager sends me the tender". Its body (how to do the job) is the `mde-agent` skill's.
- **The workflow joins the dots.** It says which output goes into which input, which button starts what, who approves.
- **MdExplorer writes the assignment** from both: the job's description, its inputs resolved (texts, and the approved
  files of earlier steps with their commit), what to deliver. The agent receives it as `[ASSIGNMENT]` / `[INCARICO]`.

## What MdExplorer does with it

When the project lists the workflow's document (`agentCity.workflowDocs` in `.development.yml`, the "Workflow
documents" of the project settings):

```yaml
agentCity:
  workflowDocs:
    - gara/workflow.md
    - ddd/workflow.md
```

- **An agent's step** starts when its trigger happens. MdExplorer sends the agent the assignment: `[ASSIGNMENT] Job «x» of
  your card: <description>.`, then **Inputs** (each one with its text, or the files with «output X of «step», approved in
  commit …»), **You deliver** (each output with its verb: you write / you update / you download), **Workflow notes** (the
  `brief`, if any), round, step and attempt. On an Italian interface the same text is in Italian and starts with
  `[INCARICO]`.
- An input that has no file yet (the source step is not approved, or a `*` output was not delivered in this round) **holds
  the step** with the reason. With `"wait": "any"` a source that did not finish is skipped: that is wanted.
- **`ask-owner`**: the step waits in its owner's mail as "To start"; the owner starts it from the launch screen or declines
  it with a reason.
- **A person's step** arrives in the mail of whoever has its role as "Your turn" (from `workflow`): the task, what they
  receive, the documents to write, and the **outcomes as buttons**. There is no free text: the step closes with an outcome.
  An outcome's values are typed in before pressing. If the step `produces` documents, MdExplorer checks them before
  recording the outcome: each one changed after the step was asked, nothing uncommitted, the commit pushed.
- **Approval**: an agent's delivery is approved by the agent's owner, or — with `"approval": { "by": "<role>" }` — by whoever
  has that role, **from their own computer**. A rejection is restarted by the agent's owner, because the agent works on
  their computer.
- **A team** (the same role on several rows of `responsabilita.md`): who presses the button that starts the step chooses
  the person; otherwise the first of the team who already acted on the steps it comes from; otherwise MdExplorer asks. It
  never picks at random.
- A message between agents that the workflow does not describe is **refused**: hand-overs are the workflow's.
- **Loops are yours.** With no loop, "Restart" after a rejection is always possible; with a loop, MdExplorer does exactly
  what it says.
- **A round keeps what it was born with**: it saves the workflow and the cards' interfaces when it starts. A change made
  now applies to the **next** round.
- A workflow with errors stops the hand-overs and says so.
- **Several workflows** in one project (the tender and another process) work side by side: each round knows its
  workflow and goes on with it. They must go together: the same document or the same `*.workflow.json` listed twice is an
  error, and **an agent opens the rounds of one workflow only** — two workflows with a `launch` for the same agent are
  refused, because launching it would not say which round to open. Hand-overs between agents follow all the listed
  workflows: a message is allowed if one of them describes it.

## Ask first (if the user has not said)

1. **The people and their roles.** Read `responsabilita.md`: who has which role, with which agents. A person's step and an
   approval name a **role**, never a person.
2. **What starts the process.** Usually one person launching one agent by hand.
3. **The steps and their order.** For each agent step: which **job** of its card. For each step: what it needs (inputs) and
   from where; which steps can run side by side.
4. **Who starts each step.** Its owner from a launch screen (`ask-owner`), or nobody because it starts by itself (`auto`)?
   When the step's agent works for a different person than the one who triggered it, the answer is almost always
   `ask-owner`.
5. **The decisions of the people.** Buttons under an agent's message (declared in its card) and **person steps** with
   outcomes ("Bid" / "Do not bid"). Which values they carry: they become the round's variables.
6. **Who approves.** The agent's owner (default) or another role (`approval.by`)? Two approvals = two steps in a row: the
   second is a person step with the document as input and outcomes "Approve" / "Reject".
7. **Repetitions.** Is some work redone after a rejection, by whom, at most how many times? Is some work done a fixed number
   of rounds on purpose?

If you cannot ask, decide, write the decision in the document's text, and tell the user.

## The interface in the card (`a2a.skills[]`)

```yaml
a2a:
  name: technical-lead
  skills:
    - id: technical-sheet
      description: Writes the technical feasibility sheet of a tender
      inputs:
        - { id: code, description: the tender's code }
        - { id: tender, description: the tender document }
        - { id: criterion, description: the evaluation criterion that is yours }
      reads:
        - { path: tender/company-profile.md, section: Technical, note: our solutions }
      outputs:
        - { id: sheet, path: tender/sheets/technical.md, note: technical feasibility }
      replies: []
  replies: []
```

| Key | Value |
|---|---|
| `inputs[]` | `{ id, description }`: what the job receives in the assignment. The workflow gives each one a value. |
| `reads[]` | `{ path, section?, note? }`: fixed documents of the project the job reads; `section` = the title of the one chapter it reads. Checked and drawn, not written in the assignment. |
| `outputs[]` | `{ id, path, note?, action? }`; `action` = `writes` (default), `updates`, `downloads`; `*` allowed in the file name (`search-*.md`). |
| `replies` (in the skill) | the ids of the buttons this job may offer (declared in `a2a.replies`). |
| `a2a.replies[].values` | the values a button carries: they become the round's variables. |

- Paths from the project's root, `/` as separator.
- **Every key not listed is an error** (with its path: `a2a.skills[1].outputs[0].colour`).
- **Quote any value with a comma or a colon**: `note: "the five criteria, with their source"`. Inside `{ … }` a comma ends
  the value, and the card is refused.
- Everything is inside `a2a:`: changing an interface **resets the trust** of the agent. That is wanted: the person sees
  what changed before the agent works again.

## The workflow, version 3

```json
{
  "mde_workflow": 3,
  "title": "Tender: from the call to the decision",
  "description": "optional, one sentence",
  "variables": { "code": "the code of the tender chosen with «Start the round»" },
  "steps": [ ... ],
  "loops": [ ... ]
}
```

Keys are English, values are free. **Every key that is not listed here is an error.**

### An agent's step

| Key | Required | Value |
|---|---|---|
| `id` | yes | kebab-case, unique in the file |
| `agent` | yes | the agent's name (`a2a.name`) |
| `skill` | yes | the job of its card that this step does (`a2a.skills[].id`) |
| `title` | no | the label in the diagram ("Technical sheet"); without it, the `id` |
| `trigger` | yes | what starts it: **exactly one** of the forms below |
| `start` | yes | who starts it: `manual`, `ask-owner`, `auto` |
| `inputs` | when the job has inputs | **one value for each input of the job**, no more, no less (below) |
| `brief` | no | only what the job's description and inputs do not already say, with `{variables}` |
| `approval` | no | `{ "by": "<role>" }`: that role approves instead of the agent's owner. The job must have outputs. |

### A person's step

| Key | Required | Value |
|---|---|---|
| `id`, `title`, `trigger` | as above | a person's step is never a launch: it starts from a button, an outcome or after other steps |
| `person` | yes | a **role** of `responsabilita.md` |
| `task` | yes | what is asked of the person, with `{variables}` |
| `outcomes` | yes | `[ { "id", "label", "values"? } ]`: the buttons that close the step; a later step can start from one |
| `inputs` | no | what the person receives: texts or outputs of earlier steps, like an agent's inputs |
| `produces` | no | `[ { "id", "path", "note"? } ]`: documents the person writes; checked before the outcome is recorded |

A person's step has no `agent`, `skill`, `start`, `brief` or `approval`.

### Inputs

```json
"inputs": {
  "code": "{code}",
  "criterion": "2 — competence",
  "sheets": { "from": ["technical.sheet", "legal.sheet"] },
  "summary": { "from": "summary.summary" }
}
```

- **A string** is a text, with `{variables}`. A fixed text can contain a dot, so it is never read as a reference.
- **An object `{ "from": … }`** is the approved output(s) of earlier steps, as `"<step>.<output>"`. The step must depend on
  them (through its trigger, directly or further up). For an output with `*`, the input gets the file **delivered in this
  round**, not the pattern.

### Triggers

| Form | Means | `start` |
|---|---|---|
| `{ "launch": true }` | a person launches the agent by hand | `manual` |
| `{ "reply": "<id>", "to": "<step id>" }` | a person presses a button under that step: a reply of the agent's card, or an **outcome** of a person's step | `ask-owner`, `auto` |
| `{ "after": ["<step id>", …], "wait": "all" \| "any" }` | those steps are finished | `ask-owner`, `auto` |

- **Finished** means: approved, for an agent's step with outputs; done, for an agent without outputs; an outcome pressed,
  for a person's step.
- `after` with more than one step is **a wait**: `all` (default) starts at the last one, `any` at the first.
- `reply` to an agent: the button is declared in that agent's card and proposed by the job of the `to` step
  (`skills[].replies`). Every value it carries (its `values`, and `{name}` in its texts) is declared in `variables`.

### Loops (`loops[]`)

**"Until"** — after a rejection the step is redone, until it is approved:

```json
{ "id": "rework", "steps": ["technical", "legal"], "until": "approved", "restart": "manual", "max": 3 }
```

`restart`: `manual` (the person presses "Restart", the usual choice) or `auto`. `max`: leave it out for no limit.

**"For"** — the step is done a fixed number of rounds on purpose, each one approved: `{ "id": "rounds", "steps": ["draft"],
"times": 3 }`. Both kinds on one step: each round of the "for" is redone after a rejection as the "until" says. A loop
repeats **agents' steps with outputs** (otherwise there is nothing to approve); never a person's step.

## Changing the pair — the procedure

Every change follows these steps, in this order. Do not skip the check.

1. **Say where the change belongs.** What a job *receives, reads or delivers* → the card's interface. *Where a value comes
   from, what starts a step, who approves, who decides* → the workflow. A card never names a colleague; a workflow never
   says how to do a job.
2. **Change the interface first**, if it changes: add, rename or remove the `inputs` / `reads` / `outputs` / `replies` of
   the job. Keep the card's body (sections 2, 3 and 4 of `mde-agent`) consistent with it.
3. **Then every step that uses that job**: each input of the job gets exactly one value in `inputs`; an output that was
   renamed or removed is renamed or removed in every `from` that names it; a removed button is removed from every
   `trigger.reply`.
4. **Variables**: every `{name}` used, and every value carried by a button or an outcome, is in `variables`; a variable no
   one uses any more is removed.
5. **Check** with `CheckWorkflow` (below) until `valid: true`.
6. **Tell the user**: which agents **must be trusted again** (their `a2a:` changed); that **open rounds keep the old
   version** and the change applies to the next round; which folders you created.

Typical changes:

| The user wants | You change |
|---|---|
| "the legal lead must also read the research" | card: add an input `research`; workflow: `"research": { "from": "search.research" }` in the `legal` step |
| "the summary must also update the register" | card: add an output `{ id: register, path: …, action: updates }` to the job |
| "Management approves the contract sheet" | workflow: `"approval": { "by": "Management" }` on the step; `responsabilita.md` has the role |
| "the account manager decides after the summary" | workflow: a person's step `after: ["summary"]`, input `{ "from": "summary.summary" }`, outcomes |
| "two people approve, one after the other" | workflow: the agent's step (first approver) + a person's step (second) with outcomes "Approve" / "Reject" |
| "start the offer only if we bid" | workflow: the offer step with `"trigger": { "reply": "bid", "to": "decision" }` |

## From version 2

A `mde_workflow: 2` file is an error that explains the conversion:

| v2 | v3 |
|---|---|
| `produces` of an agent's step | `outputs` of the job, in the card |
| `brief` with `{variables}` | `inputs` (one per input of the job); `brief` only for what is left |
| a step without `skill` | `skill`: the id of the job in the agent's card |
| — | `person` / `task` / `outcomes` for people's steps; `approval.by` |
| column `Ambito` of `responsabilita.md` | column `Ruolo` (the old name is an error that says to rename it) |

## Rules that make a plan right

1. **One step per turn of work, not per agent.** The same agent can appear in many steps, with different jobs.
2. **Everything starts from a launch.** Every step must be reachable from a `"launch": true` through the triggers.
3. **No step points backwards.** Repetitions are loops, not triggers that go back.
4. **`ask-owner` between people.** Use `auto` only when the same person already decided, or when the user asked for it.
5. **Folders and documents exist.** Every folder of an output exists (with a `README.md`); every `reads` exists, with its
   chapter; every role exists in `responsabilita.md`.
6. **Cards do not route.** A card that tells its agent to write to a colleague will see that message refused: make it a
   step.
7. **One step = one agent, one file, one approver.** More approvals are more steps.

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

One or two sentences on how to read it.
````

- **Never draw the workflow by hand.** The block `plantuml(@workflow, …)` draws it from the workflow, the cards and
  `responsabilita.md` every time. A workflow with errors is not drawn: the errors are shown instead.
- What the diagram shows: the **roles** as people, with what they do on each step («launches», «starts», «approves»); each
  step with its agent (blue = launched by a person, amber = started by its owner, grey = starts by itself; white with a
  person icon = a person's step); in **green** the documents a job reads, in **lilac** the ones a step delivers, with
  their note and verb; **hexagons** for buttons, outcomes and waits; an arrow from a file when a step receives that output;
  the red dashed arrow is "until approved", the thick blue one "for N rounds"; a dashed file is not written yet.
- Put the document next to `responsabilita.md`, and link the two.

## Check before handing it over

Call **`CheckWorkflow`** (MdExplorer's MCP server, group `agents`) with the project's folder and the file's path, every
time you change the workflow **or a card it uses**.

- `valid: false`: there are `error` issues, each with its `path` (`steps[2].inputs.tender`), the reason and often the `fix`.
  A problem in a card names the step that uses it (`steps[0].agent`, `steps[0].skill`). Fix **all** of them and check
  again.
- `warning` issues: tell the user.
- An HTTP or connection error means the file was **not** checked: do not change it because of that; say so.

Without the MCP tool: `GET /api/A2A/workflow/check?projectPath=<folder>&path=<file>` on the running MdExplorer.

## A complete example

A tender round. The account manager's agent searches and offers a button per compatible tender; the technical and legal
leads' agents write their sheets, each started by its owner, with the tender and the research; Management approves the
contract sheet; the summary waits for both; the account manager decides.

```json
{
  "mde_workflow": 3,
  "title": "Tender: from the call to the decision",
  "variables": {
    "code": "the tender's code, chosen with «Start the round»",
    "tender": "the tender document downloaded for that code",
    "research": "the account manager's research"
  },
  "steps": [
    { "id": "search", "agent": "account-manager", "skill": "search-calls", "title": "Search the calls",
      "trigger": { "launch": true }, "start": "manual" },
    { "id": "technical", "agent": "technical-lead", "skill": "technical-sheet", "title": "Technical sheet",
      "trigger": { "reply": "start-round", "to": "search" }, "start": "ask-owner",
      "inputs": { "code": "{code}", "tender": "{tender}", "criterion": "2 — competence" } },
    { "id": "legal", "agent": "legal-lead", "skill": "contract-sheet", "title": "Contract sheet",
      "trigger": { "reply": "start-round", "to": "search" }, "start": "ask-owner",
      "approval": { "by": "Management" },
      "inputs": { "code": "{code}", "tender": "{tender}", "criterion": "4 — contract risk" } },
    { "id": "summary", "agent": "account-manager", "skill": "tender-summary", "title": "Summary to decide",
      "trigger": { "after": ["technical", "legal"], "wait": "all" }, "start": "auto",
      "inputs": { "code": "{code}", "research": "{research}",
                  "sheets": { "from": ["technical.sheet", "legal.sheet"] } } },
    { "id": "decision", "person": "Account manager", "title": "Decide whether to bid",
      "trigger": { "after": ["summary"] },
      "task": "Read the summary on tender {code} and decide whether we bid.",
      "inputs": { "summary": { "from": "summary.summary" } },
      "outcomes": [ { "id": "bid", "label": "We bid" }, { "id": "no-bid", "label": "We do not bid" } ] }
  ],
  "loops": [ { "id": "rework", "steps": ["technical", "legal"], "until": "approved", "restart": "manual" } ]
}
```

The `start-round` button is declared in the account manager's card, proposed by its `search-calls` job, with
`values: [code, tender, research]`. The project's demo has the round in Italian: `citta-degli-agenti/gara/`.

## Checklist before handing it over

- [ ] `mde_workflow: 3`, a `title`, at least one step with `"launch": true` and `"start": "manual"`.
- [ ] Every agent step has `agent` + `skill`, and the job exists in the card; every person step has `person` (a role),
      `task`, `outcomes`.
- [ ] Every input of every job gets exactly one value; every `from` names an output of a step this one depends on.
- [ ] Every trigger has exactly one form, and `start` fits it.
- [ ] Every `{variable}`, and every value of a button or an outcome, is declared in `variables`.
- [ ] Every role (`person`, `approval.by`) is in `responsabilita.md`; every `reads` exists with its chapter; every output
      folder exists.
- [ ] Card values with a comma or a colon are quoted.
- [ ] No step points backwards; repetitions are loops, with the limits the user wants (or none).
- [ ] `CheckWorkflow` answers `valid: true`; you told the user about every warning, every agent to trust again, and that
      open rounds keep the old version.
- [ ] The document has `mde_type: workflow`, the TL;DR, and the `plantuml(@workflow, …)` block; it links `responsabilita.md`.

## What not to do

- Do not draw the workflow by hand, and do not describe it only in prose.
- Do not name a colleague in a card's interface or body: the workflow does the routing.
- Do not put in `brief` what an input already carries; do not list input files in it.
- Do not add keys that are not in the vocabulary ("notes", "color", "owner"): they are errors. Explanations go in the
  document.
- Do not name a person in `person` or `approval.by`: name the role.
- Do not add a limit nobody asked for: a loop's `max` is the user's decision.
- Do not make a step `auto` to "save a click" when another person is responsible for its agent.
- Do not hide a repetition in the triggers (A after B, B after A): it is refused; write a loop.
