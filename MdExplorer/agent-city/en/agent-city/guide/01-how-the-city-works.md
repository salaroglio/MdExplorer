---
title: How the city works
---

# How the city works

## TL;DR

In the agent city every agent prepares a piece of work and delivers it to a person, who approves or rejects it. An agent
works only on the computer of the person who answers for it, and only after that person trusts it. Work that changes
hands follows a workflow, and every round stays written in git.

- Agents prepare, people approve and decide: no agent approves its own work.
- An agent nobody answers for does not start; an agent that is not trusted does not work.
- MdExplorer directs the play: it starts the steps, delivers the assignments, records the rounds.

## Agents and people

An **agent** is a `*.agent.md` file in the project: a card that says who it is, which jobs it can do, what it reads and
what it delivers. It works with the project's AI engine (Claude, Copilot or opencode).

A **person** answers for one or more agents. They are the one who:

- declares the agent theirs, with the «It is mine» button in the «City of agents» panel;
- trusts it, with «Trust» and then «I trust it», after reading what it does and what it can do on their computer;
- reads what the agent delivers, and approves or rejects it.

For every agent the panel shows whom it answers to: «Answers to you: it works on this computer», or «Nobody answers for
it: it does not start». An agent that answers to a colleague works on the colleague's computer, not on yours.

## Why an agent works on one computer only

The agent uses the tools and the credentials of the person who answers for it. That is why it works only there: if you
launch an agent a colleague answers for, the work waits in the queue until they start it. It is a safety choice, not a
technical limit: everyone is in charge of what happens on their own computer.

## Trust

Before working, an agent must be **trusted**. Trusting it means reading the agent's card and saying «I trust it».
MdExplorer shows two different things: what the agent says it does («written by the author, not verified») and what it
can really do on your computer («computed by the app»).

If someone changes the important part of the card (the tools, the jobs it can do), the trust expires: the panel shows
«Trust expired» and the agent waits until you trust it again.

## The agents' mail

Agents write to you in the **Agents' mail**. There you find:

| Section | What is there |
|---|---|
| Rounds | the work in progress of a workflow, step by step |
| To do | what is up to you: work «To approve», a step «To start», a decision to take |
| Messages | the agents' messages, with the answers you can give |

When an agent delivers, its work stays in its own copy of the project until you approve it. With «Approve» the work
enters the project; with «Reject» you write the reason and the work stops until you press «Restart».

## Rounds and the round log

A **workflow** describes work that changes hands: which steps there are, who prepares them, who approves. Every time the
work begins a **round** is born: today's round of the tender, this week's round of the review.

The **round log** says, for every round, what happened: which steps started, who delivered, who approved, which outcome a
person chose. It lives in git, in a separate branch (`mde/giri`), so it does not mix with the group's work and it travels
with the repository. It is the evidence of who did what, and when.

## What MdExplorer does

MdExplorer directs the play, with fixed rules:

- when a step ends, it starts the ones that come next, or puts them in the mail of whoever must start them;
- it writes every agent's assignment: what to do, what it receives, what to deliver;
- it asks people for their decisions, with the outcome buttons;
- it writes everything in the round log.

Agents only do their own job and write to the person: they never decide who comes next. The workflow says that.

The next chapter explains the document that ties people, roles and agents together:
[Roles and responsibilities](02-roles-and-responsibilities.md).
