---
title: From process to workflow
---

# From process to workflow

## TL;DR

A workflow is built starting from the process as it is today, not from an ideal model: describe it in one row of
`quality/processes.md`, split it into steps, decide who prepares and who approves each step, and only at the end write
it, with the MarkAgent. The example that follows is complaints handling.

- First the row of the process, then the steps, then the agents and the workflow.
- An agent prepares, a role approves: every step delivers a file, every decision is an outcome with its buttons.
- Agents with the skill `mde-agent`, workflows with the skill `mde-workflow`, checked with the tool `check_workflow`.

## 1. Describe the process in one row

If `quality/processes.md` does not exist yet, ask the MarkAgent to create it: a table, one row per process.

| Field | What to write | Complaints, for example |
|---|---|---|
| Process, scope and boundaries | why it exists, where it starts and where it ends | from the complaint received to the reply to the customer |
| Input / output | what starts it, what it delivers | a complaint / the reply and, if needed, a corrective action |
| Responsible and evidence | the role that answers for it; what must stay written | Quality manager; the complaint, the analysis, the reply, the decision |
| Workflow | the path of its document, «to design», or «none» with the reason | to design |

If a row cannot be filled in, the problem is not the workflow: the process is not clear yet. Better to stop and talk
about it in the group.

## 2. Split it into steps

A sequence of activities, each with **one** person responsible and **one** result:

1. **Record the complaint**: whoever receives it describes it, with customer, date, product or service.
2. **Analyse**: what happened, which requirement was not met, whether it is an isolated case.
3. **Decide**: is a correction enough, or is a corrective action on the cause needed (10.2)?
4. **Reply to the customer**: the reply, approved by whoever answers for it.
5. **Check effectiveness**, if there was a corrective action: did the problem come back?

A useful rule: if a step has two approvers, it is two steps.

## 3. Decide who prepares and who approves

- **Does an agent prepare it, or does a person do it?** A document to write (the analysis, the draft reply) is prepared
  by an agent; a choice is a person's step, with the outcomes as buttons.
- **Who approves?** Usually whoever answers for the agent; when another signature is needed, the workflow says so with the
  approval of a role.

| Step | Who works | What they deliver | Who approves or decides |
|---|---|---|---|
| Record | customer service agent | `quality/complaints/complaint-*.md` | Customer service |
| Analyse | customer service agent | `quality/complaints/analysis-*.md` | Quality manager |
| Decide | Quality manager | outcome «Correction» or «Corrective action» | it is the decision |
| Reply | customer service agent | `quality/complaints/reply-*.md` | Quality manager |

The roles must exist in `responsibilities.md`, with a person behind them.

## 4. The outputs are files

Every delivery is a file in a folder that exists and has its own `README.md`: not every engine creates folders by
itself. A `*` in the name is fine when each round produces a new file. The files are the evidence: they must say what
was done, not summarise it.

## 5. What happens when something is rejected

A rejected analysis is redone: it is an «until approved» loop. Decide whether it repeats without limit (usually the right
choice) or at most a number of times, and who restarts it. It is a decision for people.

## 6. Write the agents and the workflow

Only now is it written, with the MarkAgent:

1. the cards of the agents needed, with the skill `mde-agent`: each says what it receives, reads and delivers;
2. the workflow, with the skill `mde-workflow`: the document with `mde_type: workflow` and its `.workflow.json`;
3. the tool `check_workflow` must answer that it is valid; every warning must be read;
4. the document is added to the project's workflows (Project settings, «Workflow documents»);
5. whoever answers for the process reads it, with the diagram MdExplorer draws, and approves it;
6. a trial round on a real, small case, and then adjust.

A round keeps the version of the workflow it started with: a change applies from the next round.

## 7. Update the row of the process

In `quality/processes.md`, instead of «to design», write the path of the workflow. That way the list says which
processes are already workflows and which are not: it is the first thing the internal audit will look at.

See also: [With the MarkAgent](05-with-the-markagent.md), [The clauses that matter](02-the-clauses-that-matter.md).
