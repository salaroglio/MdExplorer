---
mde_type: workflow
title: Hello world
workflow: hello.workflow.json
---

# Hello world

## TL;DR

The smallest round of the agent city, to see it work from start to finish: whoever answers for the city launches the
example agent, the agent writes «hello world» in `agent-city/hello/hello-world.md`, and the person approves the delivery.

- First: take the role «City owner» in the responsibilities document and declare the agent yours («It is mine»).
- Then launch the agent from the agent city: the delivery arrives as a request to approve.
- Once it works, feel free to remove this workflow from the project settings and write your own.

## The round

```plantuml(@workflow, ./hello.workflow.json)
```

## Who does what

| Step | Who works | What they deliver | Who approves |
|---|---|---|---|
| Write «hello world» | agent `city-hello` | `agent-city/hello/hello-world.md` | the city owner |
