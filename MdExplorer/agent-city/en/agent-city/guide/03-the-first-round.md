---
title: The first round
---

# The first round

## TL;DR

The quickest way to understand the city is to run it once. The city brought the «Hello world» workflow into the project
(`agent-city/hello.md`): a single step, where the agent `city-hello` writes «hello world» in
`agent-city/hello/hello-world.md` and you approve the delivery.

- First take the role «City owner» and trust the agent.
- Then launch the agent: the delivery reaches your mail, to approve.
- The round stays in the round log: you have seen the whole path, from start to finish.

## What is already there

| File | What it is |
|---|---|
| `agent-city/hello.md` | the workflow document, with the drawing of the round |
| `agent-city/hello.workflow.json` | the workflow itself, which MdExplorer runs |
| `agent-city/agents/city-hello.agent.md` | the card of the example agent |
| `agent-city/hello/` | the folder where the agent delivers |
| `responsibilities.md` | the responsibilities document, with the open role «City owner» |

## Step 1: take the role

Open the «City of agents» panel. The agent `city-hello` shows as «Nobody answers for it: it does not start». Press «It is
mine»: MdExplorer writes your name and your git email next to the role in the responsibilities document. Commit the
document, so your colleagues know the role is yours too.

## Step 2: trust the agent

Still in the panel, press «Trust» on the agent. Read what it does and what it can do on your computer: this agent reads
and writes files of the project, nothing else. Press «I trust it». The panel now shows it as «Trusted».

## Step 3: launch it

Launch the agent (the «Launch agent» window, button «Launch now»). The workflow says this is the first step of the round
and that it starts by hand: MdExplorer opens a new round and sends the agent its assignment. The agent works in its own
copy of the project, so it does not touch the files you are working on.

## Step 4: approve the delivery

When the agent is done, the «Agents' mail» shows work «To approve»: the file `agent-city/hello/hello-world.md`, with
«hello world», the date and the round. Open it, look at it and press «Approve». The work enters the project.

If instead you press «Reject», write the reason: the work stops, and starts again only when you press «Restart». The
agent receives your reason together with the assignment.

## Step 5: look at the round

In the «Rounds» section of the mail you find the round you just ran, with its steps and its messages: «finished: 1 of 1
steps». It is also written in the round log in git. When you no longer need it, «Archive the round» takes it off the
list: you find it in the «Archive».

## If it does not start

| What you see | What it means | What to do |
|---|---|---|
| «Nobody answers for it: it does not start» | nobody took the role | press «It is mine» (step 1) |
| «Not trusted» or «Trust expired» | trust is missing, or the card changed | trust it again (step 2) |
| queued, «it works on its owner's computer» | the role belongs to a colleague | the colleague launches it, from their computer |
| queued, «the workflow cannot be read» | a workflow of the project has an error | ask the MarkAgent to check it with `check_workflow` |
| no engine | the project has no AI engine | choose one in the project settings |

## And then

Once the round works, the «Hello world» workflow has done its job: you can remove it from the project settings («Remove
this workflow») and delete its files (`agent-city/hello.md`, `agent-city/hello.workflow.json`, the card
`agent-city/agents/city-hello.agent.md` and the folder `agent-city/hello/`), leaving this guide where it is. Then write
your own: [Writing a workflow](04-writing-a-workflow.md).
