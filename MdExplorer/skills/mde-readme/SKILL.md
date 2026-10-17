---
name: mde-readme
description: Write README sections that include runnable script examples MdExplorer can execute interactively. Use whenever you document a CLI tool, build/deploy script, dev task, or any command-line invocation in a README, sprint note, or how-to doc. Each example must declare its parameters in a way MdExplorer's runner can detect, so the user can fill them in a dialog and click ▶ Run.
mde:
  origin: mdexplorer
  version: 8
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
<comment-prefix> @param <NAME> [— description]  [<val> | <val> | …]  [default: <value>]  [secret]  [type: file|dir|out-file]
```

Rules:
- `<NAME>` is `[A-Za-z][A-Za-z0-9_-]*` (letters, digits, underscore, dash).
- The `—` (em dash) or a simple `-` or `:` separates the name from the description. Anything
  after the name on the same line is the description.
- `default: <value>` (anywhere on the line, in parentheses or after the description) sets the
  default value pre-filled in the dialog.
- **A list of admissible values, written as an alternation, renders a dropdown** instead of a free
  text field — the user picks, they cannot mistype. Two accepted forms:
  - the description **is** the list: `# @param DIALECT — cobol | pli (default: pli)`
  - the list is labelled inside a longer description: `# @param ENV — target environment, options: dev|staging|prod`
    (`options`, `values`, `choices`, `one of` are all accepted).

  Each value must be a **single word** (letters, digits, `.` `_` `-` `+` `/`) and there must be at
  least two of them — a pipe inside ordinary prose is left alone, so a description like
  `command to pipe into grep | wc` stays free text. Declare the `default:` as one of the listed
  values so the dropdown opens on it.
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
underscores allowed) — case-insensitive match.

## Quoting — YOU own the delimiters

The runner substitutes a placeholder with the value **exactly as the user typed it**, verbatim.
It adds **no quotes and no escaping**. A `<TOKEN>` stands for *text*, not for a shell word — so
whatever you write around the token in the markdown is exactly what the shell will see.

The practical consequence: **if the value could contain spaces or shell metacharacters, you must
quote the placeholder yourself.**

| Situation                                | Write this                        |
| ---------------------------------------- | --------------------------------- |
| Value may contain spaces (paths, titles) | `$dest = "<target_dir>"`          |
| Command argument, free text              | `./deploy.sh --msg "<message>"`   |
| Token spliced inside a longer literal    | `$uri = "http://host:3030/<dataset>/query"` |
| Numeric / boolean / known-safe token     | `--port <port>`                   |
| Value that must NOT be interpolated (pwsh) | `$raw = '<literal_text>'`       |

Pick the delimiter that suits the shell and the value:

- **PowerShell** — `"<X>"` interpolates (`$`, backtick are live); `'<X>'` is literal.
- **bash / sh** — `"<X>"` interpolates (`$`, backtick, `\`); `'<X>'` is fully literal.
- **cmd** — `"<X>"`; note cmd has no escape for an embedded `"`.

Two consequences worth internalising:

1. **Never double-quote by habit and by template.** Because the runner no longer quotes, writing
   `"<X>"` is now *correct*, not a bug — this is the opposite of the pre-v7 convention. If you are
   updating an older README written against v6, the bare `$fuseki = <FUSEKI>` forms still work for
   single-token values with no spaces, but should be re-quoted as `"<FUSEKI>"` to stay safe.
2. **A value containing the delimiter still breaks the script.** `'<TITLE>'` with a value of
   `l'analisi` produces broken PowerShell. When the value is free-form prose, prefer the
   `export VAR="<x>"` / `param()` default forms below, which the runner rewrites *whole* — quotes
   and escaping included.

### The two forms the runner still quotes for you

`export VAR="default"` (bash) and `[string]$Var = 'default'` inside a PowerShell `param()` block
are **not** placeholder substitution: the runner replaces the entire right-hand side of the
assignment, adding correct quoting and escaping itself. Use these when the value is untrusted
free text — they are the escape-proof path.

```bash
export GREETING="hello"      # runner rewrites the whole RHS, quotes included
```

## Working directory — every command runs from the PROJECT ROOT

**Critical:** when the user clicks ▶ Run, MdExplorer executes the script with the working
directory set to the **project root** — the folder the user opened in MdExplorer — **not** the
folder that contains the README. This is true even when the README lives several levels deep in a
subfolder.

Therefore **every relative path in the command must be written relative to the project root**, not
relative to the README's own location. This applies to:
- the **script/program being invoked** (`python main.py`, `./build.sh`, `node cli.js`),
- any **helper files, config files, or relative output paths** the command references.

The trap: a README documenting a tool naturally describes commands as if you were standing *inside*
the tool's folder. That instinct produces a broken block. Example — a README at
`ai-tools-pli/analyze-pli-programs/README.md` whose `main.py` sits **next to it**:

| Wrong (relative to the README)        | Correct (relative to the project root)                          |
| ------------------------------------- | --------------------------------------------------------------- |
| `python main.py <pli_file>`           | `python ai-tools-pli/analyze-pli-programs/main.py <pli_file>`   |
| `./run.sh`                            | `./tools/run.sh`                                                 |

The wrong form fails with `can't open file '...\main.py': [Errno 2] No such file or directory`
because Python looks for `main.py` in the project root, where it does not exist.

Rules:
- **Prefix the invoked script with its path from the project root.** This is the simplest robust
  form and is unaffected by how parameter values are resolved.
- **Do not assume the README's folder is the cwd.** Don't write `python main.py` hoping the runner
  will `cd` next to the README — it won't.
- If you genuinely need a different working directory, `cd` **explicitly using a root-relative
  path** as the first line of the block (e.g. `cd ai-tools-pli/analyze-pli-programs`), then call
  the script. Prefer the path-prefix form above unless the tool truly requires its own cwd.
- `type: file` / `type: dir` pickers are also **rooted at the project root**, so picked paths share
  the same anchor as your root-relative command — they stay consistent, no conflict.

## Path separator — always `/`, never `\`

**Critical for cross-platform documents.** The same README is run on both Windows and Linux/macOS,
so every path you write inside a runnable block must use the **forward slash `/`** as separator —
**never** the Windows backslash `\`.

- Forward slash works on **all** platforms: .NET / Win32 accept `/` for filesystem paths on Windows
  too, and it is the native separator on Linux/macOS.
- Backslash works **only on Windows**. On Linux/macOS `\` is a literal filename character, not a
  separator, so a path like `Ontology\ABoxPL1\file.ttl` is read as one big filename and the script
  fails with *"No such file or directory"*.

MdExplorer cannot fix this for you at run time: since `\` is a legal character in a Linux filename,
the runner must pass your path through verbatim — rewriting it would corrupt paths that legitimately
contain a backslash. **Portability is your responsibility as the author: type `/`.**

| Wrong (Windows-only `\`)                  | Correct (portable `/`)                    |
| ----------------------------------------- | ----------------------------------------- |
| `$file = "Ontology\ABoxPL1\BS507.ttl"`    | `$file = "Ontology/ABoxPL1/BS507.ttl"`    |
| `dotnet publish .\src\MyApp.csproj`       | `dotnet publish src/MyApp.csproj`         |
| `python tools\foo\main.py`                | `python tools/foo/main.py`                |

This holds for **every** shell — `bash`, `powershell`, `cmd` alike. (PowerShell accepts `/` on
Windows for file paths, so a single `/`-form works in `pwsh` on every OS.) If a PowerShell script
genuinely needs the OS-native separator (e.g. to hand a path to a Windows-only external tool), build
it with `Join-Path` instead of hardcoding `\`:

```powershell
$file = Join-Path "Ontology" "ABoxPL1" "BS507.ttl"   # -> '\' on Windows, '/' on Linux
```

Also never hardcode an **absolute root** (`C:\sviluppo\...`, `/home/user/...`) or a drive letter:
those are machine-specific and break the moment the document moves. Keep paths **relative to the
project root** (see the previous section) and use `/`.

## Examples to copy when authoring a README

### 1. Bash — deploy script

```bash
# @param ENV       — dev | staging | prod (default: staging)
# @param VERSION   — git tag or branch to deploy
# @param API_KEY   — deployment API key (secret)
./deploy.sh --env <env> --version "<version>" --key "<api_key>"
```

`<env>` is picked from a dropdown (the three declared values), so it is a short safe token and can
stay bare. `<version>` and `<api_key>` are quoted because the user could paste anything into them
— the runner will not quote for you.

### 2. PowerShell — local build

```powershell
# @param Configuration — Debug | Release (default: Release)
# @param Runtime       — target RID, options: win-x64|linux-x64|osx-arm64 (default: win-x64)
dotnet publish src/MyApp.csproj -c <Configuration> -r <Runtime> --self-contained
```

Both parameters have a closed set of values, so they render as dropdowns: the user picks
`Release` or `Debug` instead of typing it. `Configuration` uses the bare form (the description is
the list), `Runtime` the labelled one (prose plus `options:`).

### 3. Bash with env-export style (also detected, legacy)

**Use this form for untrusted free text.** The `export VAR=...` form makes the runner rewrite the
whole right-hand side, so it adds correct quoting *and* escaping — a value containing quotes or
`$` cannot break out. Plain placeholder substitution elsewhere is verbatim, so there you carry the
quoting burden yourself.

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

### 7. Tool living in a subfolder (path is relative to the project root)

The README sits in `ai-tools-pli/analyze-pli-programs/`, and so does `main.py`. Because the block
runs from the **project root**, the call must spell out the path to `main.py` from the root — not
just `python main.py`.

```bash
# @param PLI_FILE — PL/I source to analyse (type: file)
python ai-tools-pli/analyze-pli-programs/main.py <pli_file>
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
- ❌ Don't leave a placeholder bare when its value can contain spaces or shell metacharacters —
  `$dest = <target_dir>`, `--msg <message>`. Substitution is verbatim, so a value like
  `My Documents` splits into two words and the command breaks. Quote it: `$dest = "<target_dir>"`.
- ❌ Don't rely on the runner to escape a value that contains your own delimiter (an apostrophe
  inside `'<X>'`). For free-form prose use the `export VAR="<x>"` / `param()` default forms, which
  the runner rewrites whole.
- ❌ Don't mix shells in a single fence (e.g. `bash` fence with PowerShell syntax inside).
- ❌ Don't write a script path relative to the README's folder (`python main.py` when `main.py`
  lives beside the README in a subfolder). The block runs from the **project root**, so it fails
  with `can't open file`. Write the path from the root: `python tools/foo/main.py`.
- ❌ Don't use backslashes `\` in paths (`$file = "Ontology\ABoxPL1\file.ttl"`). They work only on
  Windows; on Linux/macOS `\` is a literal filename character and the script fails with `No such
  file or directory`. Always use forward slashes: `"Ontology/ABoxPL1/file.ttl"`.
- ❌ Don't hardcode an absolute root or drive letter (`C:\sviluppo\...`, `/home/user/...`); it is
  machine-specific. Keep paths relative to the project root.

## Quick checklist before committing a README

- [ ] Every runnable fence starts with a `@param` header (or has no parameters at all).
- [ ] Every placeholder `<name>` in the call has a matching `@param NAME` line above.
- [ ] Every relative path (the invoked script, helper/config/output files) is written **relative to the project root**, not to the README's folder — the block runs from the project root.
- [ ] Every path uses **forward slashes `/`**, never backslashes `\`, and no absolute root / drive letter — so the same document runs on both Windows and Linux/macOS.
- [ ] Every placeholder whose value could contain spaces or metacharacters is **quoted by you**
      (`$x = "<name>"`, `--key "<key>"`) — substitution is verbatim, the runner adds nothing.
- [ ] Every parameter with a closed set of values declares it as an alternation (`cobol | pli`,
      `options: dev|staging|prod`) so the user gets a dropdown instead of a free-text field.
- [ ] Sensitive parameters are marked `secret` (or named with a secret-like suffix).
- [ ] Defaults are provided for non-secret parameters where a sensible default exists.
- [ ] Prose above the block explains the side effects.
