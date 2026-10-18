---
name: mde-ownership
description: Author or reorganize the project OWNERSHIP document (responsabilita.md) for the agent city. Use when a project enables the agent city and needs a machine-readable map of "who has which role, with which agents" — the roles that workflow steps of people (`person`) and approvals (`approval.by`) name. Produces a single markdown doc with `mde_type: ownership` and a well-formed table.
mde:
  origin: mdexplorer
  version: 2
  updatePolicy: replace
---

<!--
MdExplorer-managed skill.
The `mde:` block above marks this file as distributed by MdExplorer. When you
open a project, MdExplorer compares the embedded version with what is on disk
and will overwrite this file to keep it in sync with the current MdExplorer
features. To customize the skill while keeping your edits, remove the `mde:`
block (or change `origin` to something else) — MdExplorer will then leave the
file alone.
-->

# MdExplorer ownership document convention

The **ownership document** (§12.3 of the Agent-Harness-A2A design) makes machine-readable
*who has which role, and with which agents*. The **role** is also the name a workflow (`mde-workflow`) uses for the step of a person (`person`) and for who approves (`approval.by`). It is a **routing hint** for
the agent city / federation — **not a permission**: the human gate (§12.6) is always the
guardrail. One project has **at most one** ownership doc.

## When to use

Only when the project has the **agent city enabled** (Project Settings → Agent City, which
writes `agentCity.enabled: true` in `.development.yml`). The doc's relative path goes in the
`ownershipDoc` field of that section. Prefer **reorganizing existing information** (team
docs, README responsibilities, a participants list) into this format rather than inventing
owners.

## Format (mandatory)

A normal markdown file, indexed like any other. It MUST have:

1. A YAML front matter declaring the type:

```
---
mde_type: ownership
---
```

2. A single markdown table. Recognized columns (Italian **or** English headers):

| Column        | Aliases                          | Required | Meaning |
|---------------|----------------------------------|----------|---------|
| Role          | `Ruolo`, `Role`                  | yes      | The role. The **same role on more rows is a team**: who starts a piece of work chooses which of them takes it. |
| Description   | `Descrizione`, `Description`     | no       | One-line explanation of the role. |
| Responsible   | `Responsabile`, `Responsible`, `Owner` | no | Display name of the person. |
| Git Email     | `Git Email`, `GitEmail`, `Email` | yes      | Identity key — MUST match a project **participant** git email. |
| Agents        | `Agenti`, `Agents`               | no       | Agent names (`a2a.name`) of this person in this role, comma/space separated. A role with no agents (a person who only decides or approves) leaves it empty. |

### Example

```
---
mde_type: ownership
---
# Ownership

| Ruolo      | Descrizione            | Responsabile | Git Email        | Agenti          |
|------------|------------------------|--------------|------------------|-----------------|
| WSAA-TOT   | Import TOT movements   | Carlo        | carlo@acme.it    | analyst, dev    |
| Batch      | Nightly jobs           | Marco        | marco@acme.it    | dev             |
| Batch      | Nightly jobs           | Lia          | lia@acme.it      | dev             |
| Direzione  | Decides on offers      | Anna         | anna@acme.it     |                 |
```

## Validation rules (fail-loud)

The parser and validator reject a doc that breaks any of these — fix them, do not work around:

- **The column is `Ruolo` / `Role`**: the old `Ambito` / `Scope` header is an error that asks to rename it.
- **A repeated role is a team** (Batch above: Marco and Lia), not an error.
- **Git Email required** on every row, and it **must be a project participant** (the email
  appears in the merged participants of `.development.yml`). If the responsible person is
  not yet a participant, add them first (MdE Team panel).
- **Agents must exist**: every agent name cited must be a real citizen in the project
  registry (a `.agent.md` with an `a2a:` block). A typo or a not-yet-created agent is an error.
- **One table only**, and the file must contain the `mde_type: ownership` front matter, else
  it is ignored entirely.

## What NOT to do

- Do **not** treat ownership as an access rule — it only *suggests* routing; every federated
  request still requires the recipient's explicit human authorization.
- Do **not** create more than one ownership doc per project (multiple docs → all rejected).
- Do **not** put owners' secrets or tokens here — this file is committed via git.
