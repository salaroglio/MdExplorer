---
title: The clauses that matter
---

# The clauses that matter

## TL;DR

The map between the clauses of ISO 9001:2015 that touch everyday work and the things of the city that cover them:
roles, documents, workflows, git history. The clauses are described in our own words with their numbers: for the exact
text, read the standard.

- Roles and responsibilities (5.3) live in `responsibilities.md`; evidence (7.5) in documents, git and the round log.
- Requirements (8.2), changes (8.5.6), nonconformities (8.7, 10.2) and audit (9.2) become workflows with outcomes.
- Context, leadership and management review remain people's work, but they too leave documents in the project.

## How to read the tables

For each clause: what it asks, briefly, and where it lives in a project with the city. A clause not listed is no less
important: it is just less tied to the way the group passes work along.

## Organisation and responsibilities

| Clause | What it asks | Where it lives |
|---|---|---|
| 4.4 processes of the system | know the processes, their inputs and outputs, who answers for them, how they are controlled | `quality/processes.md`, one row per process; a workflow for each process that changes hands |
| 5.3 roles and responsibilities | assign and communicate who has which responsibility | `responsibilities.md`: role, person, email, agents |
| 7.2 competence | competent people for the role, with evidence | `responsibilities.md` says who; the evidence of competence is kept separately |

The **role** is the name the workflows use; the person is who answers for it today. When someone changes job, one row
changes, and the git history says since when.

## Risks and documented information

| Clause | What it asks | Where it lives |
|---|---|---|
| 6.1 risks and opportunities | consider the risks that can prevent the results, and act | a mandatory «Risks» section in the documents decisions are made on |
| 7.5 documented information | keep written what is needed, identified, up to date, protected | the project's documents, the git history, the round log in the branch `mde/giri` |

6.1 does not ask for a risk register separate from everything else: it asks that risks be considered where decisions are
made. A mandatory section of a document that a role approves is the simplest way to show it.

7.5 distinguishes what is **maintained** (the rules: procedures, workflows) from what is **retained** (the records:
reviews done, outcomes, minutes). In the city the former are documents that change with approved versions; the latter
are the documents delivered in the rounds, which are not rewritten.

## Operation

| Clause | What it asks | Where it lives |
|---|---|---|
| 8.2.2 and 8.2.3 requirements and their review | understand what the customer asks and review it before committing, recording the outcome | a workflow with sheets, review and a decision with an outcome, like the demo's tender |
| 8.5.6 control of changes | review and control changes, recording who authorised them | a «change» workflow: request, impact analysis, approval by a role |
| 8.7 nonconforming outputs | identify what is wrong and decide what to do with it | a person's step with outcomes: correct, redo, accept under concession, discard |

## Evaluation and improvement

| Clause | What it asks | Where it lives |
|---|---|---|
| 9.1 monitoring and measurement | decide what to measure and analyse it | the round log: how many, how long, how many rejections |
| 9.2 internal audit | check at planned intervals that the system works | an audit workflow, with the plan and the reports in `quality/audit/` |
| 9.3 management review | management reviews the system and decides | a person's step of the role Management, with the minutes as the document produced |
| 10.2 nonconformity and corrective action | react, find the cause, correct, check that it worked | a workflow with outcomes: cause, action, check of effectiveness |

## What stays outside the workflows

- **Clause 4** (context and interested parties) and **5.2** (quality policy): documents that management writes and
  approves. In the project they live in `quality/`, like any other document, with their history.
- **7.1** (resources, infrastructure, measuring instruments): it depends on the company's work.
- **Applicability**: if a clause does not apply, it must be said and justified. It is a decision for people.

See also: [ISO 9001 as workflows](01-iso-9001-as-workflows.md),
[From process to workflow](03-from-process-to-workflow.md), [Preparing the audit](04-preparing-the-audit.md).
