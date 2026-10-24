---
name: mde-code
description: Answer questions about the project's source code through MdExplorer's code graph (the `code_*` tools). Use when the user asks what programs or files are in the graph, how far the scan or the description got, where a class/method/function is, who calls it, what breaks if it changes, how two pieces of code are connected, what a file is for, or to fix a description. Triggers on "grafo del codice", "code graph", "programma", "program folder", "scansione", "scan", "descrizione del codice", "chi chiama", "who calls", "impatto", "impact", "dove è definito", "cosa c'è nel grafo", "come è memorizzato il codice".
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

# The code graph of an MdExplorer project

MdExplorer reads the folders the user marked as **program** and keeps a graph of their code: files, symbols
(classes, methods, functions, components…), their parameters, and the edges between them (`calls`, `inherits`,
`implements`, `imports`, `instantiates`, `references`, `contains`, and `jumps` between the `fragment`s of a block too
long to describe in one go — a GOTO, or the code running on into the next piece). Two steps fill it:

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
| A chain stops at a STRING that names a method (`setX("method")`, `submit('alias.method')`): the graph cannot follow it | answer from the source, then `code_rule_try` → check the sample in the source → `code_rule_propose` |
| Which enrichment rules are active, which further analyses were proposed | `code_rules` |
| The answer needs the PAGES (server-side HTML templates with directives, e.g. `.template`) and the graph has no node for them | read a few pages, then `code_language_try` → check the sample files → `code_language_propose` |

## Reading the answers right

- **Programs are separate.** Each program folder has its own symbols: the same qualified name in two programs is
  two symbols. A call from one program to another ends on an `external` node unless the build declares the
  dependency.
- **Confidence of an edge**: `extracted` = the parser is sure; `inferred` = deduced; `ambiguous` = more than one
  candidate, the parser did not choose; `rule` = added by an enrichment rule the user approved (relation `dispatches`:
  the method named by a string). Say which one when it matters to the answer.
- **Pages** (when the project has a page dialect): a page is a `module`; its `control` symbols are the elements with an
  event (`onclick`…); `includes` links a page to the pages it includes; `guard` on a control or an `includes` edge is the
  condition in the page (`$if[ROLE==S]$`): the control exists only then. Visibility in the page is not authorisation on the
  server: say so when it matters.
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

## Enrichment rules: when the graph stops at a string

Some programs pick the method to run from a STRING: `setTarget("readData")`, `sendPage('ordersController.list')`,
a dispatcher with a map. The parser sees text, not a call, so the chain breaks there. Only for a concrete question of the
user (never a systematic search):

1. Answer the question anyway, reading the source; tell the user the graph stops at that point.
2. Write the convention as a rule and dry-run it with `code_rule_try`; read `missCounts` and `missExamples`, fix and retry
   until `accepted`.
3. Open every call of the returned `sample` in the source and check that the target is the method really executed.
4. `code_rule_propose` with the problem in plain words (the user may not be technical), the sample checks and, if the
   problem suggests them, further analyses. The user decides in the MarkAgent tab («Migliora il grafo»): tell them it is
   waiting there. Never write files under `.mde-code/` yourself.

## Page dialects: when the graph has no pages

MdExplorer does not know the extension of server-side templates (`.template`, `.tpl`…): it skips them, so the graph
stops before the user's click. Only for a concrete question of the user:

1. Answer anyway from the source; tell the user the graph has no pages.
2. Read a few pages and write the dialect. Directive pages (`$include[x]$`, `$if[X]$`): how a directive looks (a regex
   with `word` and `arg`), which words include another page, which open and close a block, which blocks are conditions.
   Pages that mix server code and HTML (Razor, JSP, Thymeleaf…): `recognizers`, one regex per role (include, open with
   `close: brace`, `close: element` or a close recognizer, else, case, call with `a0…a9`), `events` for attributes like
   Angular `(click)`, `dynamic` for server-written strings. Angular templates (`.html`): `*ngIf` → open with
   `close: element, guard: true`; child components → a call recognizer with `callee: selector`. Dry-run it with `code_language_try`; fix it until `accepted`, and explain every
   note (`unknownWords`, recognizers that never match).
3. Open every file of the returned `sample` and check the facts (controls, conditions, included pages).
4. `code_language_propose` with the problem in plain words. The user decides in the MarkAgent tab; MdExplorer writes
   `.mde-languages/<id>/` and re-analyzes. Never write that folder yourself.
5. After the scan MdExplorer links by itself a control to the function of a JS file the page loads (or of a page it
   includes); for Angular, the component to its template (`renders`), `(click)="save()"` to the component's method, and
   `<app-x>` to the component with that selector (`instantiates`). The rest are call sites for enrichment rules: a string naming a controller method → a rule as above; a string
   naming the page shown next (`nextPage("orders")`, `return View("Orders")`) → a rule with `owner.from: page`.
