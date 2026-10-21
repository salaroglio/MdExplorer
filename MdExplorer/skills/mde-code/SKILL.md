---
name: mde-code
description: Answer questions about the project's source code through MdExplorer's code graph (the `code_*` tools). Use when the user asks what programs or files are in the graph, how far the scan or the description got, where a class/method/function is, who calls it, what breaks if it changes, how two pieces of code are connected, what a file is for, or to fix a description. Triggers on "grafo del codice", "code graph", "programma", "program folder", "scansione", "scan", "descrizione del codice", "chi chiama", "who calls", "impatto", "impact", "dove è definito", "cosa c'è nel grafo", "come è memorizzato il codice".
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

# The code graph of an MdExplorer project

MdExplorer reads the folders the user marked as **program** and keeps a graph of their code: files, symbols
(classes, methods, functions, components…), their parameters, and the edges between them (`calls`, `inherits`,
`implements`, `imports`, `instantiates`, `references`, `contains`). Two steps fill it:

- **scan** (*scansione*): a parser (Roslyn for C#, tree-sitter for the other languages) reads the source and writes
  the structure. Deterministic, no AI.
- **description** (*descrizione*): an LLM writes what each file and symbol is for. Slow; it may be partial.

## The one rule: ask the `code_*` tools

The graph lives in a database **MdExplorer runs and manages itself**. Its address changes at every start and is
not for you. **Never** query it with SPARQL, `curl`, an HTTP call, or a Fuseki URL such as `localhost:3030`; never
read the scripts under `.mde-code/` to answer. Use the `code_*` tools: they answer in milliseconds and give
`path:line` to open the source when you really need it.

A Fuseki server on `localhost:3030` and the skills `mde-tbox` / `mde-abox` / `mde-shacl` are a **different thing**:
domain ontologies written from the project's documents. They are not the code graph — do not use them for code.

If no `code_*` tool is available, the code graph is off for this project: say so, and tell the user to choose the
languages of a program folder in «Ambiente di sviluppo» (Development environment). Do not look for another way in.

## Which tool

| The user asks | Tool |
|---|---|
| What is in the graph? Which programs? How far did the scan / description get? Is it done? | `code_inventory` |
| The shape of an unknown codebase, the most connected symbols, the biggest files | `code_overview` |
| Where is X? Find a class/method by name or by what it does | `code_find_symbol` |
| Signature, parameters and description of one symbol | `code_describe` |
| What a file is for and what it contains | `code_outline` |
| What X calls / what calls X (one or a few hops) | `code_neighbors` |
| What breaks if X changes | `code_impact` |
| How A reaches B | `code_path` |
| Where the code is on this machine (external roots `@alias/…`) | `code_roots` |
| Read, search or edit a REMOTE root (`remote` set in `code_roots`, a folder on an SSH server) | `remote_list`, `remote_read`, `remote_grep`, `remote_edit`, `remote_write` |
| A description is wrong or missing | read the source, then `code_set_description` |

## Reading the answers right

- **Programs are separate.** Each program folder has its own symbols: the same qualified name in two programs is
  two symbols. A call from one program to another ends on an `external` node unless the build declares the
  dependency.
- **Confidence of an edge**: `extracted` = the parser is sure; `inferred` = deduced; `ambiguous` = more than one
  candidate, the parser did not choose. Say which one when it matters to the answer.
- **A missing description is not an error**: the description step may still be running or may have been run only on
  part of the code. `code_inventory` tells how many files and symbols are described.
- **Paths** are logical (`program/…`, `@alias/…`), the same on every machine; open `absolutePath`, which is where the
  file is on this machine.
- **Remote roots** (`code_roots` shows `remote: ssh://host/…`) are on an SSH server: there is no `absolutePath` and your own
  file tools cannot reach them. Use the `remote_*` tools with the logical path `@alias/…`; search with `remote_grep`, which
  runs on the server. They are not in the code graph yet.
- **Encodings.** Files of a code root are not always UTF-8: `code_roots` lists each root's `encodings` by extension
  (e.g. `.cbl` → `ibm850` = CP-850), and `remote_read` states the encoding of a non-UTF-8 file. Before changing such a file:
  edit through the `remote_*` tools when they are available (they keep the encoding); with your own tools (Edit, Write,
  sed, perl) write the new text in the file's encoding. Never convert it to UTF-8 and never add UTF-8 text to it: the
  file breaks for its compiler. If you are not sure, ask.
