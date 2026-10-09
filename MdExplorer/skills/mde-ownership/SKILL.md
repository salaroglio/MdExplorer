---
name: mde-ownership
description: Author or reorganize the project OWNERSHIP document for the agent city (federation). Use when a project enables the agent city and needs a machine-readable map of "who is responsible for which scope, with which agents". Produces a single markdown doc with `mde_type: ownership` and a well-formed table.
mde:
  origin: mdexplorer
  version: 1
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
*who is responsible for which scope, and with which agents*. It is a **routing hint** for
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
| Scope         | `Ambito`, `Scope`                | yes      | The area of responsibility (unique across the table). |
| Description   | `Descrizione`, `Description`     | no       | One-line explanation of the scope. |
| Responsible   | `Responsabile`, `Responsible`, `Owner` | no | Display name of the person. |
| Git Email     | `Git Email`, `GitEmail`, `Email` | yes      | Identity key — MUST match a project **participant** git email. |
| Agents        | `Agenti`, `Agents`               | no       | Agent names (`a2a.name`) that serve this scope, comma/space separated. |

### Example

```
---
mde_type: ownership
---
# Ownership

| Ambito     | Descrizione            | Responsabile | Git Email        | Agenti          |
|------------|------------------------|--------------|------------------|-----------------|
| WSAA-TOT   | Import TOT movements   | Carlo        | carlo@acme.it    | analyst, dev    |
| Batch      | Nightly jobs           | Marco        | marco@acme.it    | dev             |
```

## Validation rules (fail-loud)

The parser and validator reject a doc that breaks any of these — fix them, do not work around:

- **Unique scopes**: the same `Scope` may not appear twice.
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
