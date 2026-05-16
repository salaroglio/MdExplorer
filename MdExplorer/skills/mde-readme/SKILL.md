---
name: mde-readme
description: Write README sections that include runnable script examples MdExplorer can execute interactively. Use whenever you document a CLI tool, build/deploy script, dev task, or any command-line invocation in a README, sprint note, or how-to doc. Each example must declare its parameters in a way MdExplorer's runner can detect, so the user can fill them in a dialog and click ▶ Run.
mde:
  origin: mdexplorer
  version: 3
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


# README with runnable examples — MdExplorer convention

MdExplorer renders fenced `bash`, `sh`, `powershell`, `pwsh`, `cmd` blocks with a **▶ Run** toolbar.
When the user clicks Run, MdExplorer parses the script for **parameters**, pops up a dialog so the
user can fill them in, and then executes the script with those values substituted.

For this to work, you must write the example following the convention below.

## What every runnable example must contain

1. A short **prose intro** explaining what the script does.
2. A **`@param` documentation header** inside the fenced block, one line per parameter.
3. **Placeholder tokens** of the form `<param-name>` wherever the user must supply a value.
4. Optionally, a `@description` line summarising the block.

Do **not** hardcode example values directly into the call — always use placeholders. The dialog
defaults to empty (or to a `default:` you declare); the user fills them in before running.

## Parameter declaration syntax

Use comments natural to the shell:

| Shell        | Comment prefix |
| ------------ | -------------- |
| bash / sh    | `#`            |
| powershell   | `#`            |
| cmd / bat    | `REM` or `::`  |

The grammar for each parameter line is:

```
<comment-prefix> @param <NAME> [— description]  [default: <value>]  [secret]  [type: file|dir|out-file]
```

Rules:
- `<NAME>` is `[A-Za-z][A-Za-z0-9_-]*` (letters, digits, underscore, dash).
- The `—` (em dash) or a simple `-` or `:` separates the name from the description. Anything
  after the name on the same line is the description.
- `default: <value>` (anywhere on the line, in parentheses or after the description) sets the
  default value pre-filled in the dialog.
- `secret` (or the name containing `KEY`/`TOKEN`/`SECRET`/`PASSWORD`/`PWD`) renders the field as
  a password input.
- `type: file` renders a **path-picker button** that opens MdExplorer's file browser **rooted at
  the project folder**. The chosen path is inserted as the parameter value when Run is clicked.
- `type: dir` is identical to `type: file` but lets the user pick a folder instead of a file.
- `type: out-file` is for files the script will **generate** (output files that don't exist yet).
  The picker opens in **Save-As mode**: the user navigates to the destination folder, types the
  filename, and the full path is composed for the script. Use this whenever the parameter
  represents an output / destination file. The `default:` value (if any) pre-fills the filename
  suggestion in the Save-As input. Synonyms accepted: `output-file`, `save-file`.
- Path pickers work identically in the browser and in the Electron build — they reuse the
  MdExplorer-managed file browser, no native OS dialog needed, so they behave consistently on
  Windows, macOS and Linux.

The same `<NAME>` is then referenced inside the script body as `<name>` (lower-cased and with
underscores allowed) — case-insensitive match. Placeholders **never quoted by you**; the runner
quotes them safely per shell.

## Examples to copy when authoring a README

### 1. Bash — deploy script

```bash
# @param ENV       — target environment (default: staging)
# @param VERSION   — git tag or branch to deploy
# @param API_KEY   — deployment API key (secret)
./deploy.sh --env <env> --version <version> --key <api_key>
```

### 2. PowerShell — local build

```powershell
# @param Configuration — Debug or Release (default: Release)
# @param Runtime       — RID like win-x64, linux-x64 (default: win-x64)
dotnet publish .\src\MyApp.csproj -c <Configuration> -r <Runtime> --self-contained
```

### 3. Bash with env-export style (also detected, legacy)

```bash
# @param GREETING — message to print (default: Hello)
export GREETING="<greeting>"
echo "$GREETING, world!"
```

### 4. Cmd / batch

```cmd
REM @param TARGET  — build target (default: all)
REM @param THREADS — parallel build threads (default: 4)
make <target> -j<threads>
```

### 5. Bash with path pickers

When a parameter takes a path on disk, mark it `type: file` (file picker), `type: dir`
(folder picker), or `type: out-file` (Save-As picker for files the script generates).
Clicking the parameter chip in MdExplorer opens the project-scoped file browser starting
at the project root.

```bash
# @param SOURCE — file to upload (type: file)
# @param TARGET_DIR — destination directory inside the project (type: dir)
rsync <source> <target_dir>/
```

### 6. Script that generates an output file

When the parameter is a file the script CREATES, the user cannot select it because it does
not yet exist. Use `type: out-file` — the picker opens in Save-As mode, so the user picks
the destination folder and types the filename.

```powershell
# @param INPUT       — input Excel file (type: file)
# @param OUTPUT_FILE — generated Markdown file (default: report.md, type: out-file)
python -m tools.excel_to_markdown.main <input> -o <output_file>
```

## When the AI generates a README

When you are asked to write or update a README that documents a runnable script:

1. **Start each runnable section with a heading** (e.g. `### Deploy`, `### Run locally`).
2. **Above the fenced block**, write 1–3 prose lines: what it does, when to use it, any
   side effects (writes to disk, hits production, etc.).
3. **Inside the fenced block**, put the `@param` documentation header first, blank line, then
   the actual call.
4. **Prefer one parameter per line** in the call so the placeholders are visually obvious; long
   lines are OK if needed.
5. **One block per scenario** — do not stuff multiple unrelated invocations into a single fence;
   MdExplorer treats each fence as one runnable cell.
6. **Don't add usage comments after `--help`-style lines** unless the script also exposes them at
   runtime; keep documentation in the `@param` header.

## What NOT to do

- ❌ Don't write `./deploy.sh --env staging` with hardcoded values — the user can't change them.
- ❌ Don't use bare `$VAR` references without an `@param` line above; the runner won't know they
  exist.
- ❌ Don't use angle brackets for anything other than parameter placeholders inside runnable
  blocks; the parser treats `<word>` as a parameter.
- ❌ Don't mix shells in a single fence (e.g. `bash` fence with PowerShell syntax inside).

## Quick checklist before committing a README

- [ ] Every runnable fence starts with a `@param` header (or has no parameters at all).
- [ ] Every placeholder `<name>` in the call has a matching `@param NAME` line above.
- [ ] Sensitive parameters are marked `secret` (or named with a secret-like suffix).
- [ ] Defaults are provided for non-secret parameters where a sensible default exists.
- [ ] Prose above the block explains the side effects.
