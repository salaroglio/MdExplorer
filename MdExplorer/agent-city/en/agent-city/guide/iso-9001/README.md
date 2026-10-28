---
title: ISO 9001 in the agent city
---

# ISO 9001 in the agent city

## TL;DR

This chapter of the guide explains how to set up, with the MarkAgent, the workflows that manage an ISO 9001 quality
system in the agent city. There is no method to install: the group builds the processes, agents and workflows itself,
starting from how it works today.

- A process that changes hands becomes a workflow: who prepares, who approves, what stays written.
- The evidence for the audit is born while working: approved documents, outcomes, round log, git history.
- It is a guide, not a substitute for the standard or for the judgement of a certification body.

## Where to start

If it is the first time you use the city, read the [city guide](../README.md) first and do the «Hello world» round: here
we take for granted that you know what a workflow, an agent and the responsibilities document are.

## Reading order

1. [ISO 9001 as workflows](01-iso-9001-as-workflows.md): the idea, and what a workflow does not cover.
2. [The clauses that matter](02-the-clauses-that-matter.md): the map between clauses and MdExplorer things.
3. [From process to workflow](03-from-process-to-workflow.md): step by step, with complaints handling.
4. [Preparing the audit](04-preparing-the-audit.md): the internal audit, the evidence, corrective actions.
5. [With the MarkAgent](05-with-the-markagent.md): what to ask it, what to write yourself.

## What you need in the project

Nothing comes ready-made. The chapter suggests a simple convention, to create with the MarkAgent when needed:

| What | Where | What it is for |
|---|---|---|
| The list of processes | `quality/processes.md` | one process per row, with who answers for it and its workflow |
| Internal audits | `quality/audit/` | the plan, the reports, the actions decided |
| Roles | `responsibilities.md` | who answers for what, with name and email |
| Workflows | wherever the group prefers | a document with `mde_type: workflow` and its `.workflow.json` |

Agents are written with the skill `mde-agent`, workflows with the skill `mde-workflow`; before adding a workflow to the
project, check it with the tool `check_workflow`.

## A model to look at

The MdExplorer demo contains the tender round, from the call for tenders to the review of the requirements (8.2.3): four
agents prepare the sheets, the account manager decides whether to bid. It is a good example of a process already turned
into a workflow, worth reading before you write your first one.

## Words

| Word | What it is |
|---|---|
| Process | an activity the group repeats, with input, output and someone responsible |
| Workflow | the process written so that MdExplorer runs it: steps, roles, outcomes |
| Round | one run of the workflow, from start to finish |
| Round log | the history of the rounds, in the git branch `mde/giri` |
| Evidence | what stays written and shows that the work went as planned |
