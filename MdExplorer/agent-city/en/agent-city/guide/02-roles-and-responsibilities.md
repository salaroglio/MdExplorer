---
title: Roles and responsibilities
---

# Roles and responsibilities

## TL;DR

The responsibilities document (`responsibilities.md`) says who answers for which role and which agents. Workflows speak
of roles, never of people: this document links the role to the real person, recognised by their git email.

- One row per person and role: Role, Owner, Git Email, Agents.
- A role with no person is an open role: its agents do not start until someone takes it.
- Several people with the same role are a team: whoever starts the work chooses who handles it.

## The document

It is an ordinary markdown file of the project, with a header that declares it:

```markdown
---
mde_type: ownership
---
```

and a single table. MdExplorer creates it when the city comes into the project, and names it in the settings (Project
settings → «Agent City / Federation» → «Ownership document»). A project has only one.

## The columns

| Column | What it holds | Required |
|---|---|---|
| Role | the role's name, the same the workflows use | yes |
| Owner | the person's name, as you read it | no |
| Git Email | the email the person commits with in the project | yes, for a person |
| Agents | the names of the agents they answer for in that role | no |

There can also be a Description column, with one line explaining the role. The Italian headers (Ruolo, Responsabile,
Git Email, Agenti) are read too.

## The person is their git email

MdExplorer recognises people by the git email they work with in the project, not by their name. That is why the Git
Email column matters more than the name: it must be exactly the email of your commits, and the person must be a
participant of the project.

## Open roles

When a method or the city bring a new role, MdExplorer adds it as an **open role**: a row with the role and its agents,
but no name and no email. The agents of an open role do not start: in the «City of agents» panel they show as «Nobody
answers for it: it does not start».

To take a role:

1. open the «City of agents» panel;
2. press «It is mine» on the agent (or «They are all mine» to take them all): MdExplorer writes your row in the document;
3. or write your name and email in the row yourself, and commit.

## Teams

The same role on several rows, with different people, is a **team**: for instance two technical leads who share the
tenders. When a step is up to a role with several people:

- whoever presses the button that starts the step chooses the person;
- if nobody chose, it goes to whoever in the team already worked on the steps it comes from;
- otherwise MdExplorer asks. It never picks at random.

Whoever has a step «To start» can also hand it to a colleague with the same role, with «Pass to a colleague» and a line
of explanation.

## A role that only decides

Not every role has agents. A management that decides whether to bid for a tender, or that approves a report, is a row
with an empty Agents column: the workflow sends it the decision with its buttons.

## What it is not

The responsibilities document is not a permission: it tells MdExplorer where to route the work. The guarantee is always
the person, who approves from their own computer. Do not write passwords or tokens in it: the file travels in git with
the whole project.

Now you can try: [The first round](03-the-first-round.md).
