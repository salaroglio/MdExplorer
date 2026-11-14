---
title: Writing a workflow
---

# Writing a workflow

## TL;DR

A workflow is work that changes hands, written so that MdExplorer can run it: the steps, who prepares each step (an
agent) and who approves it or decides (a person with a role). First you describe it in words, then you write it with the
MarkAgent, and finally you check it with the `check_workflow` tool.

- Two files: the document for people (`mde_type: workflow`) and the `.workflow.json` that MdExplorer runs.
- Every step has a trigger (a launch, a button, the end of other steps) and a way to start (by hand, by its owner, by itself).
- Outputs are files: a step receives the approved files of the steps before it.

## The two files

| File | For whom | What it says |
|---|---|---|
| `<name>.md` with `mde_type: workflow` | people | what it is for, the drawing of the round, who does what |
| `<name>.workflow.json` | MdExplorer | the steps, the triggers, the inputs, who approves, the rework loops |

The document names its JSON file and draws it with a `plantuml(@workflow, ./<name>.workflow.json)` block. MdExplorer
runs it when the document is in the project's list of workflows (Project settings → «Workflow documents»).

## The steps

- **An agent's step** does one of the jobs its card declares. MdExplorer sends it the assignment: what to do, what it
  receives, what it delivers.
- **A person's step** reaches the mail of whoever has that role: a task, the documents to read and the **outcomes** as
  buttons («We bid», «We do not bid»). It closes by choosing an outcome.

## What starts a step

| Trigger | When | How it starts |
|---|---|---|
| launch | a person launches the agent by hand | by hand |
| a button | someone presses a reply of the agent, or a person's outcome | by its owner, or by itself |
| after other steps | those steps are finished (all of them, or any one) | by its owner, or by itself |

«By its owner» means the step waits in the mail of whoever answers for the agent, as «To start»: it is the right choice
when the agent works for a different person than the one who triggered the step. «By itself» means it starts as soon as
it can.

Every round begins with a launch: every step must be reachable, trigger after trigger, from a step launched by hand.

## Who approves

An agent's delivery is approved by whoever answers for the agent. If someone else must approve it, the step says which
**role** approves: for instance the management approves the contract sheet. Two approvals are two steps in a row.

## Inputs, outputs and variables

- A step's **outputs** are files with a path (`tenders/sheets/technical.md`); a name can have a `*` when the files are
  more than one.
- A step's **inputs** are texts, or the approved files of earlier steps («the output `sheet` of the step `technical`»).
- **Variables** are the values of a round (the tender's code): a button or an outcome carries them, and texts use them in
  braces, like `{code}`.

An output folder must already exist in the project, with its own `README.md`: not every engine creates folders by
itself. `check_workflow` reminds you.

## Redoing a step

Rejected work stops until whoever answers for it presses «Restart». The workflow can say more: which steps are redone
together, whether they restart by hand or by themselves, how many times at most. Without that, you can always restart.

## The agent's card: its interface

The workflow links the steps; the card of every agent says what each of its jobs can do:

| Part of the card | What it says |
|---|---|
| inputs (`inputs`) | what the job receives in the assignment |
| reads (`reads`) | the fixed documents of the project it reads |
| outputs (`outputs`) | the files it delivers |
| replies (`replies`) | the buttons it may offer the person |

A card never names another agent or a person: it says «I receive the tender document», not «my colleague sends me the
tender document». The workflow says where it comes from.

## The order of work

1. **Describe** the work in words: where it begins, where it ends, what it produces.
2. **Split it into steps**, and line up those that depend on each other.
3. For every step decide **who prepares** (which agent) and **who approves or decides** (which role).
4. Decide the **outputs as files**, with their paths.
5. **Write it**: the cards of the new agents, the workflow, the document.
6. **Check it** with the `check_workflow` tool until it says it is valid, and read the warnings.
7. **Add it** to the project's workflows, and run a first trial round.

## Writing it with the MarkAgent

You do not need to write the JSON by hand. Tell the MarkAgent about the work: it uses the skills `mde-workflow` (the
workflow), `mde-agent` (the cards) and `mde-ownership` (the roles), proposes the files and checks them with
`check_workflow` before handing them over. You read, correct and approve.

## A small example

A contract review, in two steps:

| Step | Who | What they deliver | Trigger |
|---|---|---|---|
| Draft the opinion | a «legal assistant» agent | `contracts/opinion.md` | launched by hand by the legal lead |
| Decide | the person with the role «Management» | an outcome: «We sign» or «To be revised» | after the draft, once approved |

The management receives the approved opinion as an input, and its outcome stays in the round log. With «To be revised»
you can add a step that starts from that outcome.

The next chapter applies all this to a quality system:
[Setting up ISO 9001 with workflows](iso-9001/README.md).
