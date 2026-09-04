---
name: mde-prompt-for-agents
description: Rewrite a free-text agent launch prompt into MdExplorer's normalized agent-prompt convention. The normalized prompt declares every input/output file as a parameter that MdExplorer can detect, so the user gets file-picker buttons in the Agent Launch dialog and the agent can later run unattended (scheduled) with those values substituted.
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


# Agent launch prompt — MdExplorer normalization convention

MdExplorer lets the user prepare a launch prompt for a `.agent.md` agent. The user writes the
prompt in free text; your job is to **rewrite it** into the normalized structure below. MdExplorer
parses the rewritten prompt for **parameters**, renders a file-picker (or text input) for each one,
substitutes the chosen values into the placeholders, and finally feeds the result to the agent —
interactively or on a schedule, with nobody watching.

## Target structure

The rewritten prompt MUST have exactly this shape:

1. `# <Task title>` — one short line.
2. Optional `## Context` — only if the original prompt contains background the agent needs. Keep it short.
3. `## Parameters` — a fenced code block with language tag `params`, containing **one line per
   parameter**, nothing else:

   ~~~markdown
   ```params
   # @param SOURCE_DOC — the markdown document to analyze, type: file, default: docs/spec.md
   # @param OUTPUT_REPORT — where to write the resulting report, type: out-file, default: docs/report.md
   # @param FOCUS — one-line description of what to focus on
   ```
   ~~~

   The leading `#` on every line is **mandatory** (it is what MdExplorer's parser keys on).
   The fence keeps the lines from rendering as markdown headings.
4. `## Task` — the imperative instructions for the agent. Reference each parameter **only** via a
   `<placeholder>` token; never repeat a literal path that a parameter already covers.

## Naming rules

- A parameter NAME starts with a letter, then letters/digits/`_`/`-`. Use `UPPER_SNAKE` in the
  `@param` declaration.
- The placeholder is the kebab-case form of the NAME wrapped in angle brackets:
  `@param SOURCE_DOC` → `<source-doc>`. Matching is case-insensitive and treats `-` and `_` as
  equivalent, but stick to this convention for readability.
- Every declared parameter MUST appear at least once as a placeholder in `## Task`, and every
  placeholder MUST be declared in `## Parameters`.

## Types

Classify each value the original prompt mentions or implies:

- Input files (markdown documents the agent must read, data files, …) → `type: file`
- Folders → `type: dir`
- The artifact the agent must produce → `type: out-file` (MdExplorer shows a Save-As picker)
- Plain text values (a focus, a language, a threshold, …) → omit `type` (plain text input)

If the original prompt names a concrete path, carry it into `default:` as a **project-relative**
path. `default:` values need no quotes.

## Headless-safety rules

The rewritten `## Task` must survive unattended execution:

- No questions to the user, no "ask me if unsure" — state the fallback behavior explicitly instead.
- The output location must be exactly the `out-file` parameter; the agent must not invent paths.
- No interactive git operations (no push, no credential prompts). Creating/modifying files in the
  project is fine.
- If a precondition fails (an input file is missing or empty), the agent must write the error into
  the output file rather than stopping silently.

## Output contract (for you, the rewriter)

Return **only** the rewritten prompt — no commentary, no explanations, and no surrounding code
fence. The response body IS the normalized prompt.

## Worked example

Original free-text prompt:

> Leggi la specifica in docs/pagamenti/spec-v2.md e confrontala con l'implementazione descritta in
> docs/pagamenti/stato-attuale.md. Scrivi un report con le differenze in docs/pagamenti/gap.md,
> concentrati sulla parte di riconciliazione.

Rewritten prompt:

~~~markdown
# Gap analysis tra specifica e stato attuale

## Parameters

```params
# @param SPEC_DOC — la specifica di riferimento da leggere, type: file, default: docs/pagamenti/spec-v2.md
# @param CURRENT_STATE_DOC — il documento che descrive l'implementazione attuale, type: file, default: docs/pagamenti/stato-attuale.md
# @param GAP_REPORT — il report delle differenze da produrre, type: out-file, default: docs/pagamenti/gap.md
# @param FOCUS — l'area su cui concentrare l'analisi, default: riconciliazione
```

## Task

Leggi <spec-doc> e confrontalo con <current-state-doc>. Produci in <gap-report> un report markdown
delle differenze, con una sezione per ogni scostamento (riferimenti puntuali a entrambe le fonti).
Dedica un'analisi approfondita all'area: <focus>. Se uno dei documenti di input non esiste o è
vuoto, scrivi in <gap-report> un report con la sola sezione "Errore" che spiega cosa manca.
~~~
