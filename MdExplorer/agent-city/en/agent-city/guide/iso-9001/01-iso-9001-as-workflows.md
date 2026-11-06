---
title: ISO 9001 as workflows
---

# ISO 9001 as workflows

## TL;DR

ISO 9001 asks to manage work by processes: each with its input and output, who answers for it, and the evidence that it
was done as planned. A workflow of the city already holds these three things, and git keeps the evidence at no extra
cost. What no workflow can say by itself stays out: whether the processes are the right ones.

- Process = workflow: the steps are the activities, the roles are the responsibilities, the outcomes are the decisions.
- The evidence the auditor asks for is the round log and the approved commits, not a form filled in on the side.
- Context, leadership and improvement remain people's work: the workflow makes them visible, it does not do them.

## The idea

A quality system answers three questions for every activity that matters:

1. **What goes in and what comes out?** The complaint goes in; the reply to the customer and any corrective action come out.
2. **Who answers for it?** Not «the office», but a precise role, with a person behind it.
3. **How do we show it went that way?** With something written, dated, attributable to someone.

A workflow of the city answers the same questions, in a form that runs:

| Question | In the workflow |
|---|---|
| What goes in and what comes out | the inputs and outputs of each step: project files, with their paths |
| Who answers for it | the role of each person's step and of each approval, tied to real people in `responsibilities.md` |
| How we show it | every approved delivery is a commit; every decision is an outcome recorded with who, when and on which commit |

That is why one can say that **an ISO 9001 system is, largely, a set of well-made workflows**.

## What a workflow is made of

A workflow is a markdown document with `mde_type: workflow`, which explains it to people, plus a JSON file that
MdExplorer runs. The JSON lists the steps:

- **an agent's step**: the agent prepares a document; whoever answers for the agent, or another role, approves it;
- **a person's step**: a role reads what they receive and closes the step with an outcome, that is a button
  («Solved» / «A corrective action is needed»);
- **repeated rounds**: a rejected document is redone until it is approved.

Agents **prepare**, people **approve and decide**. MdExplorer passes the work from one step to the next with fixed rules:
an agent never decides by itself who comes next.

## Why the auditor finds what they look for

An auditor does not want to see well-written procedures: they want to see that the work **went** the way the procedures
say. With the city, the evidence is born while working:

- the delivered document, in its approved version, with its commit;
- who approved it, and when;
- the outcome of every decision, with the person who pressed the button;
- rejections and redos, with the reason written by whoever rejected;
- the version of the workflow the round started with: a round keeps the workflow it had at the start.

This is the documented information that 7.5 asks to retain: identified, dated, attributed, protected from unauthorised
changes because every change stays in the git history.

## What a workflow does not cover

These remain with people:

- **context and interested parties** (clause 4): which processes are really needed, for whom, with which boundaries;
- **leadership** (clause 5): the quality policy, the objectives, management's commitment;
- **resources and competence** (7.1, 7.2): that people can do what the role asks;
- **judgement**: a review approved in a hurry is still a review done badly, even if it is recorded.

The workflow makes these points visible: a role without a person, a step approved in a minute, a process without a
workflow. It does not solve them.

## The starting point

The first step is not writing a workflow: it is describing the processes the group already has, one per row, in
`quality/processes.md`. How to do it, and how to get to the workflow, is in
[From process to workflow](03-from-process-to-workflow.md).

See also: [The clauses that matter](02-the-clauses-that-matter.md).
