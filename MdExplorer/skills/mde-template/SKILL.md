---
name: mde-template
description: Write, change and publish an MdExplorer project template - a git repository with `.mde-template/<id>/mde-template.yml` that brings a method (agents, workflows, skills, starting files) into a project, or a whole starting project like the demo. Use when the user asks to create a template or a method, to package a way of working so that other projects can add it, to change, version or publish a template, or to fix what `check_template` reports.
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

# Writing an MdExplorer project template

A **template** is a git repository that MdExplorer merges into a project. A project is **created** from templates
(«Create new project» → «Start from a template») or gets a **method added** later (Project settings → «Project methods»,
with the repository's address). Later versions arrive as **updates**, with the CHANGELOG shown first. Think of it as a
NuGet package for a way of working: a manifest, a version, release notes, and files that land in a known place.

MdExplorer provides the mechanisms; **the process lives in the template**. A template never needs a change to MdExplorer
to say what it brings: the manifest says it, and MdExplorer writes the project's common files for it.

## Two kinds

| `kind` | What it brings | Combines |
|---|---|---|
| `method` (a way of working: a tender with ISO 9001, ontologies, DDD) | only **its zone**: `.mde-template/<id>/**` and `.claude/skills/<id>-*/**` | yes, with other methods and with a base |
| `project` (a whole starting project, like the demo) | files anywhere | it is the **base**: at most one, and only when the project is created |

Write a `method` unless the user wants a complete starting project. Everything below is for methods; for a `project`
the zone rule does not apply.

## The repository

```text
.mde-template/<id>/
├── mde-template.yml        the manifest
├── CHANGELOG.md            the versions, with the incompatibilities
├── README.md               what the method is, for whoever picks it
├── presentazione.md        the slides of the «Create new project» showcase, one file per language (mde-slide skill)
├── istruzioni.md           instructions for the project's agents (MDE puts them in CLAUDE.md / AGENTS.md / copilot-instructions.md)
├── agents/                 the agent cards  <id>-<name>.agent.md   (mde-agent skill)
├── workflows/              the workflow document + *.workflow.json  (mde-workflow skill)
└── semi/                   models of the starting files (seeds), copied once into the project
.claude/skills/<id>-<name>/SKILL.md     the method's skills: every engine reads .claude/skills
```

- **Zone rule** (zero tolerance): every file of every branch of a method is under `.mde-template/<id>/` or
  `.claude/skills/<id>-*/`. A file outside is an error and MDE does not merge the method. This is what makes two methods
  safe together: their zones never touch each other, nor the group's files.
- **Prefix**: the agents are `<id>-<name>` (file name and `a2a.name`), the skills `<id>-<name>` (folder and `name:`).
  Two agents with the same name exclude each other: the prefix keeps two methods from causing it.
- **No common files**: a method never writes `responsabilita.md`, `.development.yml`, `CLAUDE.md`, `AGENTS.md`. It
  declares in the manifest what it needs there, and MDE writes it.
- **Branches**: the **structure** on one branch per language (`main` = Italian, `en` = English); the **example**, a
  compiled case study, on its own branch per language (`esempio`, `example`) that **starts from the structure** and
  adds documents. Whoever creates the project picks structure or example. The example's documents are seeds too, so its
  files stay in the zone.

## The manifest `mde-template.yml`

```yaml
mde_template: 1                   # format version
id: ddd                           # kebab-case = zone folder name = prefix of agents and skills
kind: method                      # method | project
title: Sviluppo con DDD
description: "Il DDD strategico: linguaggio, contesti e mappa, confrontati con il codice."
version: 1.0.0                    # x.y.z = first CHANGELOG entry = git tag v1.0.0
requires:
  mde: 2026.10.18                 # minimum MdExplorer version
  templates: []                   # methods this one cannot stand without (they must be in the project first)
  features: [code]                # MDE features it uses: fuseki, code (see below)
languages:                        # language → branches; at least one
  it: { structure: main, example: esempio }
roles:                            # added to the project's responsibilities document as OPEN roles
  - { role: Architetto, description: "approva i contesti e accetta la mappa", agents: [ddd-architetto] }
workflows:                        # the workflow DOCUMENTS (mde_type: workflow) it brings
  - .mde-template/ddd/workflows/ddd.md
instructions: .mde-template/ddd/istruzioni.md
seeds:                            # starting files: copied once if missing, then they belong to the group
  - { from: .mde-template/ddd/semi/dominio.md, to: ddd/dominio.md }
programs:                         # code folders to mark for the code graph (after the seeds: the folder must exist)
  - { path: codice/helpdesk, languages: [csharp] }
slides:                           # the showcase presentation, one per language of the SLIDES (optional)
  it: .mde-template/ddd/presentazione.md
  en: .mde-template/ddd/presentation.md
```

- **Unknown keys are errors.** So is a typo: the reader is strict, like MDE's config reader.
- **Quote every value that contains `:` or `,`** (`description: "a: b, c"`). Unquoted, YAML reads something else or
  refuses the file. It is the most frequent mistake.
- `roles`: each role becomes a row with nobody yet; whoever answers for it writes their name there and declares the
  agents theirs («It's mine»). The role names are what the workflow's `person` and `approval.by` use.
- `workflows`: MDE adds them to the project's list (`agentCity.workflowDocs`) and turns the agent city on. They must
  read together with the project's other workflows: an agent opens the rounds of one workflow only.
- `seeds`: `from` is in the zone, `to` is a group path, never under `.mde-template/`, `.claude/skills/`, `.git/`, `.md/`.
  A file that is already there is never touched; on update a changed model is only a note. Use them for registers,
  profiles and folders the method's agents write in, and for the example's documents.
- `requires.features`: MDE never turns a feature on by itself. With `fuseki` off, or with `code` and no code folder,
  the person gets a note saying where to turn it on and what stays stopped.
- `slides`: what «Create new project» shows on the right while the person looks at the template, before choosing it.
  One markdown presentation per language (mde-slide skill), in the zone, with its images next to it. The language of
  the slides is not the method's working language: an Italian-only method brings the **English slides too**, on its
  `main` branch, because whoever picks another language sees the English ones. Without slides in the chosen language
  nor in English the showcase shows a stand-in slide made from title and description: not an error, but
  `check_template` warns. Say what the method is for, who does what, and what the project has once created; MDE's
  commands (PlantUML, `text()`…) do not run in the showcase, images do.
- `instructions`: a short text for every agent of the project (where the method's documents are, which skill to use).
  MDE writes it between `<!-- mde-template:<id> -->` and `<!-- /mde-template:<id> -->` and replaces only that block.

## The CHANGELOG

```markdown
## 2.0.0 — 2026-12-01
⚠️ incompatibile: lo stato «in valutazione» del registro si chiama «aperta». Cosa fare: rinomina le righe del registro.
- Nuovo modello del verbale.

## 1.0.0 — 2026-10-18
- Prima versione.
```

- Headings `## x.y.z — yyyy-mm-dd` (an em dash), newest first; the first one is the manifest's `version`.
- **Incompatible = major version**, and then the entry **must** have at least one line
  `⚠️ incompatibile: … Cosa fare: …` (or `⚠️ incompatible: … What to do: …`). A major without it, or such a line in a
  minor, is an error. MDE shows these lines before merging an update.
- Write for the person who updates: what changes for them and what they have to do.

## Procedure

1. **Ask what the method is for**, who the people are (roles) and what they produce. Write the `README.md` first, in the
   user's words: it keeps the rest honest.
2. **Manifest**, then the **agents** (mde-agent skill, names prefixed) and the **workflow** (mde-workflow skill), the
   **skills** (prefixed), the **seeds**, the **instructions**. A workflow's output folders must exist in a project made
   of the template alone: bring them with a seed (a `README.md` in the folder is enough).
3. **CHANGELOG** entry for the version.
4. Call **`check_template`** (MCP, group `agents`) with the repository's folder. It checks the branch you are on from
   its files as they are, and every other declared branch from its last commit, in one pass: the manifest, the zone,
   the CHANGELOG, the files the manifest names, the cards and skills with their prefix, and the workflows read in a
   project made of the template alone (seeds in place, the manifest's roles as open roles). Fix every `error` and call it
   again. Tell the user every `warning`. Do not use `check_workflow` on the template repository: its roles live in the
   manifest, not in a responsibilities document, so only `check_template` sees them.
5. **Try it for real**: create a project from the repository's folder (a local path works as an address), or add it to a
   test project from Project settings → «Project methods». Open the project and look at it.
6. **Example branch**: `git checkout -b esempio` from the structure, add the compiled case as seeds, commit. After each
   change to the structure, merge the structure into the example again (`check_template` warns when the example is behind).
7. **Publish**: commit, `git tag v<version>`, push the branches **and** the tag. The template's address is the repository's.
   To appear in MdExplorer's list of templates, it must be added to MdExplorer's catalog: tell the user.

## A new version

1. Change the files; raise `version` (patch: fixes; minor: additions; major: anything that asks the group to do something).
2. Add the CHANGELOG entry, with the `⚠️` line for a major.
3. Bring the example branch along (merge the structure, then the same `version`).
4. `check_template`, tag, push. Projects see the update the next time they look for one.

Languages may be at different versions: a project follows the branch of its language (an English-only fix raises only
the English branches). Structure and example of the same language should be at the same version.

## Checklist

- [ ] Every file is in the zone; agents and skills start with `<id>-`.
- [ ] Values with `:` or `,` are quoted in the manifest.
- [ ] `slides` has an English presentation (and one in each language the method speaks), and you looked at it.
- [ ] The CHANGELOG's first entry is the manifest's `version`; a major has its `⚠️ incompatibile: … Cosa fare: …` line.
- [ ] `check_template` has no errors, and you told the user its warnings.
- [ ] You created a project from it and looked at it.
- [ ] Tag `v<version>` and branches pushed.
