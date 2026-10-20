---
title: Preparing the audit
---

# Preparing the audit

## TL;DR

The internal audit (9.2) is the check the system makes on itself before an external body does. In the city it is
prepared by reading the processes, the workflows, the round log and the git history; the report is approved by a person
who does not answer for the processes checked. The nonconformities found become corrective actions (10.2).

- The evidence is already there: approved documents, recorded outcomes, commits. The audit checks that they say the right thing.
- Typical findings are few and recurring: roles without a person, processes without a workflow, approvals too quick.
- A corrective action is closed only when someone has checked that it worked.

## What the standard asks

Internal audits at planned intervals, with defined criteria and scope, done by people who do not check their own work,
with the results retained. The management review (9.3) then uses those results to decide. How often is the group's
choice: write it in the audit plan, for example in `quality/audit/README.md`, one row per audit with date, scope, period
and status («to do», «done»).

## Who prepares the audit

You can do it with the MarkAgent, or write a dedicated agent (skill `mde-agent`) and an audit workflow (skill
`mde-workflow`): the agent prepares the report, whoever has the role of internal auditor approves it, management decides
the actions. An agent that must read the git history needs to be able to run commands: write in its card that it uses
them only to read, so that whoever trusts it knows.

In any case **independence is guaranteed by whoever approves the report**, not by whoever prepares it.

## The evidence to show

| The auditor asks | Where it is |
|---|---|
| who did this activity, with which role | the round's log and `responsibilities.md` at the date of the round |
| the document the decision was made on | the approved commit named in the decision |
| that the procedure followed was the one in force | the version of the workflow saved by the round when it started |
| what happened when something went wrong | the rejections with their reason, and the redone version |
| who changed a rule, and when | the git history of the document or the workflow |

## How to read the history

- **The round log** lives in the git branch `mde/giri`: `git log mde/giri` lists the rounds, `git show mde/giri:<file>`
  reads one.
- **A delivered document**: start from the decision in the log, follow the commit it names to the document.
- **A rule** (a workflow, a procedure): `git log -- <path>` lists every change, with author and date. A change without a
  reason written in its message is already a small finding.

## Typical findings

| Finding | What it means | Clause |
|---|---|---|
| a role of the workflows without a person | nobody answers for that step | 5.3 |
| a process in `quality/processes.md` without workflow or evidence | the process is not under control | 4.4 |
| approvals in a few seconds, always by the same person | the control may be only formal | 8.2.3 |
| a review without the risks section | risks were not considered where the decision was made | 6.1 |
| a workflow changed without a written reason | the change was not controlled | 7.5, 8.5.6 |
| a corrective action closed without a check | nobody knows whether it worked | 10.2 |

## From nonconformity to corrective action

10.2 asks to react, understand the cause, decide an action, carry it out and check its effectiveness. It is worth making
it a workflow:

1. **Record** the nonconformity, with the evidence: an agent prepares, the auditor approves.
2. **Analyse the cause**: why it happened, not only what; whoever answers for the process approves.
3. **Decide the action**: a person's step, with the action and the date by which to check it.
4. **Check effectiveness**: outcomes «Effective» or «Not effective»; in the second case, back to the analysis.

[From process to workflow](03-from-process-to-workflow.md) explains how to build it; the MarkAgent can propose a first
version.

## Before the external audit

- A complete internal audit in the period, with the approved report and the open actions in progress.
- A management review (9.3) that read those results, with its minutes.
- `quality/processes.md` up to date: every process with its owner, its workflow or the reason it has none.

This guide helps you prepare; the judgement on conformity is given by the certification body.

See also: [The clauses that matter](02-the-clauses-that-matter.md), [With the MarkAgent](05-with-the-markagent.md).
