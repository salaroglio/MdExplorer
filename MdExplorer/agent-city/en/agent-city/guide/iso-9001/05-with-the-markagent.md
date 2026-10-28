---
title: With the MarkAgent
---

# With the MarkAgent

## TL;DR

The MarkAgent is the assistant you build the quality system with in the city: it describes the processes with you,
proposes agents and workflows, writes them with MdExplorer's skills and checks them. You decide who approves, what stays
written, when a process is ready: an approval is always a person's.

- Ask it one thing at a time: a process, a workflow, an audit.
- The decisions that belong to people it leaves as questions: you answer them.
- Before adding a workflow to the project, have it checked with the tool `check_workflow`.

## What to ask it

Some requests that work, in the order they are usually needed:

- «Create `quality/processes.md` with the columns of this guide, and help me describe the processes we have.»
- «Which processes in `quality/processes.md` do not have an owner, evidence or a workflow yet?»
- «Turn complaints handling into a workflow: propose the steps, who prepares and who approves, the files that stay.»
- «Write the cards of the agents this workflow needs, with the skill `mde-agent`.»
- «Write the workflow with the skill `mde-workflow` and check it with `check_workflow`.»
- «Which ISO 9001 clauses does this workflow cover, and which not?»
- «Prepare the internal audit plan in `quality/audit/README.md` for the next twelve months.»
- «Read the March round log and tell me whether there are suspicious approvals or rejections without a reason.»

## What to write yourself

Some things are not delegated, because they are decisions:

| What | Why it is yours |
|---|---|
| who answers for which role, in `responsibilities.md` | 5.3 asks that responsibilities be assigned by whoever has the authority |
| how many times a rejected step is redone | it is a choice of the group, not of the standard |
| the quality policy and the objectives | they belong to management (5.2, 6.2) |
| whether a clause does not apply, and why | applicability must be justified by whoever knows the company |
| the approval of every delivery | the workflow wants it from a person, from their computer |

## How the MarkAgent works with you

- **It proposes, it does not decide**: when a point is yours, it writes it as an open question.
- **It does not invent the standard**: it describes the clauses in its own words with their numbers; if a point is
  doubtful, it says so and refers to the official text.
- **It asks before changing files**: a workflow in use is changed only by saying so, and the change applies from the
  next round.
- **It uses MdExplorer's tools**: `check_workflow` for workflows, the document search to find what the group has already
  written.

## A path to start

1. Do the «Hello world» round of the [city guide](../README.md), to see a whole round.
2. With the MarkAgent, describe two or three processes in `quality/processes.md`, not all of them.
3. Pick a small one that changes hands often (complaints are fine) and turn it into a workflow.
4. Do a real round, then adjust: the first workflow teaches how to write the next ones.
5. When a few are running, write the audit plan and do the first internal audit.
6. For a complete example, look at the tender in the MdExplorer demo, from the call to the requirements review (8.2.3).

See also: [From process to workflow](03-from-process-to-workflow.md), [Preparing the audit](04-preparing-the-audit.md).
