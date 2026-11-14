---
name: mde-code
description: Answer questions about the project's source code through MdExplorer's code graph (the `code_*` tools). Use when the user asks what programs or files are in the graph, how far the scan or the description got, where a class/method/function is, who calls it, what breaks if it changes, how two pieces of code are connected, what a file is for, or to fix a description. Use it to check whether a document (design, analysis, specification) matches the code. Use it also BEFORE changing code or writing the analysis of a change — which files and methods to touch and what depends on them — including the external code roots `@alias/…`, instead of searching the files by hand. Triggers on "grafo del codice", "code graph", "programma", "scansione", "scan", "chi chiama", "who calls", "impatto", "impact", "cosa c'è nel grafo", "quali file modificare", "analisi della modifica", "verifica il documento", "document driven", "@alias", "rilevazione", "che cosa manca a MdExplorer", "findings for MdExplorer".
mde:
  origin: mdexplorer
  version: 32
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

## Why the graph exists

It is the map of the code, built once so that nobody has to read the code again to answer: thousands of files, legacy
encodings, languages you may not know. Every answer comes with `path:line`, so you can check it. The descriptions are
written from the bottom up: a caller's description already says what its callees do, so `code_describe` on a high-level
symbol tells what the whole chain does. Use it to answer questions about the code, to plan or analyse a change, and to
**check a document against the code** (a design, an analysis or a specification written by hand: does what it says
exist, is it really called, does it do that?).

The graph is a tool you read, not a deliverable to complete. What it does not have — files of a language with no parser
(configuration, generated classes, templates), calls made through a string, implementations chosen by a DI container —
is a limit of the scan: say so, and read the source for that part. Do not fill it on your own initiative; when the user
asks what the graph lacked, follow «What the graph lacked» below.

## The one rule: ask the `code_*` tools

The graph lives in a database **MdExplorer runs and manages itself**. Its address changes at every start and is
not for you. **Never** reach it with `curl`, an HTTP call, or a Fuseki URL such as `localhost:3030`; never read the
scripts under `.mde-code/` to answer. Use the `code_*` tools: they answer in milliseconds and give `path:line` to
open the source when you really need it.

In this order: the tool made for the question (table below); then a ready-made question (`code_query`); only when
**neither answers**, a SPARQL query you write yourself with **`code_sparql`** (see «When no tool answers»).

A Fuseki server on `localhost:3030` and the skills `mde-tbox` / `mde-abox` / `mde-shacl` are a **different thing**:
domain ontologies written from the project's documents. They are not the code graph — do not use them for code.

If no `code_*` tool is available, the code graph is off for this project: say so, and tell the user to choose the
languages of a program folder in «Ambiente di sviluppo» (Development environment). Do not look for another way in.

How the user turns the graph on, from the tree (right click): on a folder of the project, or on an external root `@alias`
or one of its folders, «Marca come ambiente di sviluppo» (Mark as development environment), then the languages in
«Ambiente di sviluppo…». A huge root is analysed in parts: either the whole root is a program (`program` in its `tags`) or
some of its subfolders are, listed in `programs` by `code_roots`, never one inside another.

A root that `code_roots` lists with no `languages` and no `programs` has no graph: its files are reachable (open
`absolutePath`), but the graph tools find nothing there. With `programs`, only the code inside them is in the graph. Say so
before searching the files with your own tools, and tell the user that marking the folder they need (as above) gives you
callers and impact.

## First: learn how the graph is made

Before the first `code_*` question of a conversation, call **`code_ontology`** once and read it. It returns the graph's
TBox — every class of node, every property with its domain, range and meaning, the relations, the allowed values — and,
per language of this project, which parser wrote the graph and which kinds and relations it really produced. From then
on you know exactly what a symbol, an edge, a call site or a description carries. Two things follow:

- do not look for what is not there: a relation absent from a language's `parsers` entry (`reads`/`writes` exist only
  where the parser reads SQL, `resolvesTo` only where registrations are read) cannot answer for that language — say
  so, and read the source;
- do not miss what is there: an edge has a `confidence`, a `line`, maybe a `rule` and a `guard`; a symbol has `bodyHash`,
  `signature`, `visibility`; an `external` has only a name. Use them in the answer.

A language added with a package (`.mde-languages/<id>/`) has its own TBox in `languagePackages`: which kinds and
relations its rules produce, each with the rule of `language.yml` it comes from. The same text is the package's
`tbox.ttl`: MdExplorer writes it from the rules (when a language proposal is approved, on import, on export) and ships
it in the package zip. Never edit it: change the rules (`code_language_try` → `code_language_propose`) and it follows.

`code_ontology` is the structure; `code_schema` is the content of this project (counts, facts for rules, active rules,
ready-made questions). The structure does not change during a conversation: once is enough.

## When no tool answers: `code_sparql`

`code_sparql` runs a SELECT (or ASK) **you write**, read-only, on this project's graph. It is the last resort: first
check that no `code_*` tool and no `code_query` question gives the answer — they are tested, faster and cheaper.

- Write it from the TBox of `code_ontology`, never from a guess: classes, properties and values are there. An edge is
  a node: `?e a code:Edge ; code:source ?s ; code:target ?t ; code:relation "calls" ; code:confidence ?c ; code:path ?p`.
- Only the patterns: the prefixes `code:`, `text:`, `rdf:`, `rdfs:`, `owl:`, `xsd:` are declared, and MdExplorer
  puts the block in the project's graph. `GRAPH`, `FROM`, `SERVICE` and any update are refused.
- For numbers use `GROUP BY` and `COUNT`; to stay in one folder `FILTER(STRSTARTS(?p, "folder/"))`. At most 60 seconds
  and 1000 rows (`truncated` says there were more): narrow the query rather than raise the limit.
- A refused query comes back with the reason: fix it and run it again; do not fall back to guessing the number.
- In the answer, say that the number comes from a query you wrote, and give the query when the user may want to check it.
- A question you had to answer with `code_sparql` is worth a finding («What the graph lacked»): it may become a ready-made question.

## Which tool

| The user asks | Tool |
|---|---|
| What is in the graph? Which programs? How far did the scan / description get? Is it done? | `code_inventory` |
| Which revision of the sources is the graph from? Is it behind? | `code_inventory`: `lastScan.sourceControl` — `kind` (git, svn, none, unknown), `revision` (commit or SVN revision, `1230:1240` = mixed working copy), `branch`, `localChanges` (files modified and not yet committed at scan time), `note` (why something is missing). Say it in the document you write |
| The shape of an unknown codebase, the most connected symbols, the biggest files | `code_overview` |
| How the graph is organised: classes of nodes, their properties, what each parser of this project produces | `code_ontology` (once, first) |
| What the graph contains and what each relation means; which facts a rule can read (X.class calls, annotations, strings), the active rules, the ready-made questions | `code_schema` |
| A ready-made question (the catalog is in `code_schema`): what the code uses that the graph knows only by name, where in the repository that information is although not analyzed, what is on disk but not in the graph | `code_query` |
| A question no tool above answers: a count across relations or languages, a combination of conditions | `code_sparql` (last resort, read-only) |
| Where is X? Find a class/method by name or by what it does | `code_find_symbol` |
| Does this document (design, analysis, specification) match the code? | see «Checking a document against the code» |
| Plan a change or write its analysis: which files and methods to touch, what else must change | `code_find_symbol` → `code_impact` (and `code_neighbors`), then read the source at `absolutePath` |
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
| Is this link of the graph TRUE? Is a rule, a package or a kind of relation right? How far can the graph be trusted? | the skill `mde-code-check`: a sample opened in the source, a verdict, and the correction of what made the link |
| The answer needs the PAGES (server-side HTML templates with directives, e.g. `.template`) and the graph has no node for them | read a few pages, then `code_language_try` → check the sample files → `code_language_propose` |

## Reading the answers right

- **Programs are separate.** Each program folder has its own symbols: the same qualified name in two programs is
  two symbols. A call from one program to another ends on an `external` node unless the build declares the
  dependency.
- **Confidence of an edge**: `extracted` = the parser is sure; `inferred` = deduced; `ambiguous` = more than one
  candidate, the parser did not choose; `rule` = added by an enrichment rule the user approved (relation `dispatches`:
  the method named by a string; `resolvesTo`: the implementation registered for an abstraction). Say which one when it
  matters to the answer. An edge made by a rule with a `describe` question may carry a `description` (in
  `code_neighbors`): the answer to that question for that point — use it, it is what the user wanted to know there.
- **Registrations** (`resolvesTo`): an interface — and each of its methods — resolves into the implementations
  *registered* elsewhere (today Java `META-INF/services`, read by MdExplorer at every scan; `path:line` is the
  registration file). `implements` says who could run; `resolvesTo` says who is registered to run: a class that
  implements but is not registered is not loaded by that mechanism. A call that ends on an abstract method continues
  through `resolvesTo`; `code_impact` of an implementation climbs back to whoever calls the interface.
- **Events** (C#, Visual Basic): an event is a node (kind `field`); who raises it (`RaiseEvent`, `X?.Invoke(…)`) `calls`
  it, and it `calls` its handlers (`Handles`, `AddHandler`, `+=`) with confidence `inferred`. In VB the `WithEvents` field
  (the control of a page or a form) also `calls` its handler, so «control → handler → …» can be followed. An event of a
  library (`Button.Click`) is ONE external node shared by every control of that type: follow the field, not the event.
  A handler wired only in the markup (`OnClick="…"` in an `.aspx`) or by naming convention (`Page_Load` with
  `AutoEventWireup`) has no such edge.
- **SQL written in the code** (C#, Visual Basic): the method (or constant) whose string holds SQL `reads` the tables
  after `FROM` / `JOIN`, `writes` those of `INSERT INTO`, `UPDATE`, `DELETE FROM`, `MERGE INTO`, `TRUNCATE`, and `calls`
  the procedures of `CALL x(…)`, `BEGIN x(…); END;`, `EXEC x`. It is where the SQL is WRITTEN, wherever the string then
  goes (a variable, a parameter, a `StringBuilder`); pieces of a statement count too (`" FROM ORDERS O "`). Tables and
  procedures are `external` nodes, one per name: «who writes ORDERS?» is `code_neighbors` on that node with `writes`,
  direction in. Confidence `inferred`: read from the text, not by an SQL parser. The node `?` (`ambiguous`) means the
  name comes from a variable (`"SELECT * FROM " & table`): the graph cannot know which table — read the source and say
  so. Not seen: SQL kept outside the code (configuration, XML files, the database), views and synonyms behind a name.
  When the database sources are in the graph too — a language package with `database: true` in its `language.yml`
  (PL/SQL) — these arrows go to the real procedure or table instead of the external node: MdExplorer relinks them
  whenever scripts change (after «Extract the structure», at project open, after a pull), in whichever order the two
  folders were analysed, matching the name without case and without the schema in front; a name that fits two symbols
  stays on the external node. The rewritten `.mde-code` scripts are changes to commit.
- **.NET projects**: C# and VB are read one project file at a time (`.csproj`, `.vbproj`: root namespace, project
  imports, referenced libraries and projects). For a .NET Framework project the framework libraries must be on the
  computer; when they are not, the scan warns and what the code inherits or calls from the framework stays an
  `ambiguous` external written as in the source — say so instead of treating those names as resolved types.
- **Pages** (when the project has a page dialect): a page is a `module`; its `control` symbols are the elements with an
  event (`onclick`…); `includes` links a page to the pages it includes; `guard` on a control or an `includes` edge is the
  condition in the page (`$if[ROLE==S]$`): the control exists only then. Visibility in the page is not authorisation on the
  server: say so when it matters. With a code-behind dialect (ASP.NET Web Forms) the page is also linked to its C# or VB
  class: the class `renders` the page, a control `calls` the method named in its `OnClick=`/`On…=` attribute, the page
  `calls` the `Page_…` methods (AutoEventWireup). The class must be in the graph (the folder marked with VB or C# too).
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

## Checking a document against the code

When the user asks whether a document is true to the code (document-driven development), the code is the reference
and the document is under review:

1. List the document's concrete claims: classes, methods, fields, flows («A calls B», «the page saves through X»).
2. For each one: `code_find_symbol`, then `code_describe` for what it does, `code_neighbors` / `code_path` for the flows.
3. Answer claim by claim: **confirmed** (with `path:line`), **different** (what the code really does), **not in the graph**
   (then search the files: it may be generated code or a file with no parser — say which).
4. Then what the code does and the document does not say: `code_outline` of the files involved, `code_impact` of the
   main symbols.

Do not propose changes to the graph or to its rules unless the user asks: the question is about the document.

## Writing the functional analysis of a program

When the user asks for a summary, an analysis or a document about what a program DOES, build it from the graph with the
ready-made questions (`code_query`, catalog in `code_schema`), then read the source only where they point:

1. `program-outline` (`folder`): the files with what each is for and the main types — the structure of the document.
2. `entry-points` (`folder`): where the program starts (called by a framework, a UI, a job, `main`) — one section each.
3. `flow-from` (`symbol`) for each entry point: what happens, step by step, with the summaries; `resolvesTo` steps are
   the implementations really registered, and a step's `description` (from a rule) is what that link means.
4. `cross-program`: what the program uses from the others, and who uses it — the boundaries.
5. The limits, always written in the document: `unknown-externals` + `where-is-it` (what is only known by name, and where
   it is in the repository), `uncertain` (where the graph guessed), `not-analyzed` (files and descriptions missing), and
   the revision of the sources from `code_inventory`.

`hotspots` (where a change weighs), `cycles` (files that depend on each other) and `unused` (candidates for dead code —
candidates only) are for change analyses and reviews. Cite every claim with `path:line`.

## Navigating the methods: from a page to its tables, and back

For «what does this screen touch», «which pages write table X», «how much of the data layer do the pages reach», use the
three navigation questions of `code_query` — one call each, exact counts, no SPARQL to write:

- `data-reach` (`symbol`: a page module, a control or a method): every table reached, with the method/query/procedure that
  holds the SQL and ONE shortest chain (`path:line` of every step, `weakest` confidence of the chain); then every point
  where the walk stops before the data, counted by cause — `externals` (library, program not analysed, unresolved name),
  `leaves` (calls nothing, no SQL), `interfaceMembers` (no body: what runs is an implementation) — and the uncertain
  edges crossed.
- `who-reaches` (`symbol`: a table — an external node, find it with `code_find_symbol` — a catalog query, a procedure or a
  method): the entry points (page controls, page modules, framework-called methods) that reach it, each with its chain.
- `reachability` (`to`: the layer's folder, `from`: the pages' folder): how many methods and queries of the layer the pages
  reach — a page runs its controls, its life-cycle events (`Page_Load`…) and its includes — and for the others why —
  `no-callers` or `callers-not-reached` (with an example caller).

A chain is a shortest path, not the only one. A `weakest` of `inferred`/`ambiguous` means at least one step was deduced:
open that `path:line` before stating the link as certain. `depthLimit` or `listsCut` in the answer: the walk or a list was
cut — raise `depth`/`limit`, never report a cut list as complete.

## Enrichment rules: when the graph stops at a string

Some programs pick the method to run from a STRING: `setTarget("readData")`, `sendPage('ordersController.list')`,
a dispatcher with a map. The parser sees text, not a call, so the chain breaks there. Only for a concrete question of the
user (never a systematic search):

In C# and Visual Basic a name is recorded as a string fact also when it is not an argument of a call: **assigned to a
property or a field** (`cmd.CommandText = "PKG_ORDERS.LIST"`, `With {.ReportPath = "orders.rdlc"}`) — it is a call site
whose callee is the property name and whose argument 0 is the string; **passed to a constructor**
(`New OleDbCommand("PKG_ORDERS.LIST", cn)`: callee = the type name); and through a **constant** (`Const SP = "…"` used as
the argument). Sentences (text with spaces) are never recorded as names; an SQL statement is read by MdExplorer itself
(«SQL written in the code», above), no rule is needed for it.

When the string names something of ANOTHER language — a database procedure, a query in a file, a report — the target is
`"owner": {"from": "symbol", "languages": ["<id>"], "ignoreCase": true}`: the whole string is searched among the symbols
of those languages, by name or by the qualified name after the file (`PKG_ORDERS.LIST`). A name the graph does not have
(the database sources are not analysed yet, or only partly) still gets its edge, to an `external` node carrying that
name: the preview counts these as `outsideGraph` and lists some in `outsideGraphNames`, and they do not lower the linked
share. When those sources enter the graph the rule is applied again and the edge moves to the real symbol. Read
`outsideGraphNames` before proposing: names that are not procedures (or a wrong language id in `languages`, which puts
every name outside) mean the rule is wrong.

1. Answer the question anyway, reading the source; tell the user the graph stops at that point.
2. Write the convention as a rule and dry-run it with `code_rule_try`; read `missCounts` and `missExamples`, fix and retry
   until `accepted`.
3. Open every call of the returned `sample` in the source and check that the target is the method really executed.
4. `code_rule_propose` with the problem in plain words (the user may not be technical), the sample checks and, if the
   problem suggests them, further analyses. The user decides in the MarkAgent tab («Migliora il grafo»): tell them it is
   waiting there. Never write files under `.mde-code/` yourself.

## What the graph lacked: «what would you have needed in the graph?»

When the user asks it — «che cosa ti sarebbe servito nel grafo per dare una risposta più precisa?», «cosa è mancato al
grafo?», «what would the graph need to answer better?» — the answer is a plan to improve the graph for THAT question,
not a list of wishes. At the end of an analysis where the graph stopped and you read the source, you may offer it in one
line; do not start it unasked.

0. `code_schema` first: which facts the graph really has (the callees with X.class or string arguments, the annotations)
   and which rules already exist — a rule can only read what is there.
1. **Each gap, concretely**: where it stopped (`path:line`), what you read in the source instead, which fact would have
   answered (a link, a property of a link, a revision…).
2. **Classify each gap**:
   - **The graph has the fact, a rule can use it** → write a rule (below): a STRING names the method → `dispatch`
     (section above); an ABSTRACTION is resolved elsewhere (DI binding, `@ImplementedBy`, `@Provides`, a registry built
     with `X.class`) → `registration`. If the user's question needs something the edge alone cannot say (qualifier,
     scope, condition, the key of a handler), add a `describe` question (below).
   - **The fact is not recorded** (the class is in a variable, `toInstance(new Y())`, a binder kept in a variable, a
     configuration file, the content of a jar) → it is a request for MdExplorer: put it in `furtherAnalyses` of
     `code_rule_propose`, or tell the user plainly.
   - **It is not in the code** (production configuration, plugins actually deployed, runtime data) → say so: no rule
     can help, it needs evidence from the deployment.
   - **The code is not analysed** (the chain goes into a caller, a library, a service, a header that no program folder
     or seed covers: `code_query not-analyzed`, `where-is-it`, an external you read in the source) → propose the files
     with `code_analysis_propose`: the 1-5 files that carry the answer, lowest level first, logical paths
     (`@alias/dir/file` or project-relative), and the reason in plain words. The user gets «Analyse these programs» in
     the MarkAgent tab: a preview of each file's dependency closure, where they prune and choose implementations, then
     the structure is extracted (parser, no LLM). You never analyse yourself; after they confirm, ask the same question
     again and compare.
3. For each rule: `code_rule_try` until `accepted` → open every `sample` point in the source → `code_rule_propose`, one
   proposal per rule. Tell the user the proposals wait in the MarkAgent tab («Migliora il grafo») and that the link
   descriptions are written by «Descrivi con Mark» on the folder (its preview counts them). Never write `.mde-code/`.

### Findings for who maintains MdExplorer: `.mde-code/_findings/`

Who maintains MdExplorer cannot see this code. What only MdExplorer can fix — a fact the extractors do not record, a
construct read wrongly, a convention no rule can express — goes back to them as a **finding**: a markdown file
`.mde-code/_findings/rilevazione-YYYY-MM-DD.md`. It is the ONE place under `.mde-code/` you write yourself (create the
folder if missing, `.md` only). Write it when the user asks («scrivi la rilevazione», «che cosa manca a MdExplorer?»),
or offer it in one line when step 2 found facts that are not recorded.

**Nothing of the customer goes in it**: no source code, no names of classes, methods, tables, servers, people or of
the customer; no queries, connection strings or configuration values. Describe SHAPES (a regular expression, or a 5-15
line skeleton with placeholders: `IServizio1`, `Classe1`, `Metodo1`) and NUMBERS. Names of public libraries and
frameworks (the DI container, the UI toolkit) are not the customer's: write them exactly, with the version.

What to put in it (TL;DR on top, as in every MdExplorer document), measured with `code_schema`, `code_query` and the
search tools, not estimated:

1. **Perimeter**: languages, number of files and lines per language, kinds of project (web, desktop, service, library).
2. **How objects are wired**: which dependency-injection container (name, version) or none; each form of registration
   with its count — generic arguments, `typeof`/`GetType`, chained calls, factories (a lambda that creates the object),
   attributes on classes, scanning by convention, configuration files (XML/JSON: the element and attribute names);
   which forms a registration rule already links (the rule, the numbers of `code_rule_try`) and which it cannot, and why.
3. **Events and callbacks**: the forms that connect a handler to an event (`Handles`, `AddHandler`, `+=`, designer
   files, markup attributes), each with its count; how many methods have no caller in the graph because of them.
4. **Attributes / annotations that carry meaning** (routing, injection, export, serialization, transactions): the
   attribute names as written, the count, what they connect.
5. **What the graph knows only by name**: the groups of `code_query unknown-externals` with their counts, and for the
   largest what they are (framework, generated code, another repository).
6. **Constructs read wrongly or not at all**: the shape, the count, what the graph says and what the source says.
7. **Open questions** for the maintainers.

Read it again before closing: if a real name, a query or a value slipped in, replace it. Then tell the user: «Invia
diagnostica» (in the list of projects), tick «Allega le rilevazioni di MarkAgent», check the preview, send — or
«Salva lo zip» there, if the network does not allow sending.

### Blind spots: what the graph knows only by name

Before writing that something «is not known», ask the graph where it stops — and say it in the answer or document:

- `code_query unknown-externals` (with `folder` for one program): the types and calls the code uses that the graph knows
  only by name, by type and package, with the members called. `?` = calls whose type is not even known.
- `code_query where-is-it` (with `type`, or the most used ones): WHERE in the repository the missing information is —
  a source outside the program folders (`source`), a file with the same name (`source-same-name`, to check), a compiled
  `.class` or a jar that contains the class (`compiled`, `jar`), a configuration file that names it (`config`) — each
  with the remedy (mark the folder as program, link an external root `@alias`, choose the language, a further analysis
  for bytecode). `notFound` = nowhere in the repository: an external library; say so.
- `code_query not-analyzed`: per program folder, the files on disk that are not in the graph (no parser, language not
  chosen, not extracted) and the symbols still without a description.

These are facts for the user, not tasks: propose the remedy, do not apply it.

### Registration rules (`kind: registration`)

The facts they read, recorded by the Java, C# and Visual Basic extractors: type arguments of calls — `X.class` in Java,
`typeof(X)` in C#, `GetType(X)` in VB (type resolved through the imports) — and, for C# and VB, the GENERIC arguments
written in the call (`AddScoped<IFoo, Foo>()`, `AddScoped(Of IFoo, Foo)()`), the chain of a call (`bind(X.class).annotatedWith(…).to(Y.class)`: `to` reaches `bind`), annotations with their
arguments (`@ImplementedBy(Y.class)`, `@Named("x")`), the resolved return type of a method. One rule = an `anchor` (a call
or an annotation) and where to read the two ends: `argument` (an `X.class` of the anchor call), `chain` (an `X.class` of a
call further down the chain), `annotated` (the annotated symbol), `returnType` (of the annotated method),
`annotationArgument` (an `X.class` of the annotation, `element` default `value`). In `argument` and `chain` the position
is `"argument": n` for a type between the parentheses, or `"generic": n` for a generic argument (one of the two). In C#
and VB the annotations are the ATTRIBUTES (`[Export(typeof(IFoo))]`, `<Export(GetType(IFoo))>`): the name is the last
word without `Attribute`; the first positional argument is the element `value`, the next ones `arg1`, `arg2`, a named
one has its name (`ContractType`). Guice is three rules:

```json
{"id": "guice-bind", "kind": "registration", "description": "bind(X).to(Y): Guice gives Y where X is asked",
 "anchor": {"call": "to"}, "abstraction": {"from": "chain", "callee": "bind", "argument": 0},
 "implementation": {"from": "argument", "argument": 0}}
{"id": "guice-implemented-by", "kind": "registration", "description": "@ImplementedBy(Y) on X: the default implementation",
 "anchor": {"annotation": "ImplementedBy"}, "abstraction": {"from": "annotated"}, "implementation": {"from": "annotationArgument"}}
{"id": "guice-provides", "kind": "registration", "description": "@Provides: the method provides the type it returns",
 "anchor": {"annotation": "Provides"}, "abstraction": {"from": "returnType"}, "implementation": {"from": "annotated"}}
```

.NET dependency injection is the same mechanism (one rule serves C# and VB; write one per registration method the code
really uses — `code_schema` lists the callees with type arguments):

```json
{"id": "di-add-scoped", "kind": "registration", "description": "AddScoped<I, C>(): the container gives C where I is asked",
 "anchor": {"call": "AddScoped"}, "abstraction": {"from": "argument", "generic": 0},
 "implementation": {"from": "argument", "generic": 1}}
{"id": "di-add-scoped-typeof", "kind": "registration", "description": "AddScoped(typeof(I), typeof(C))",
 "anchor": {"call": "AddScoped"}, "abstraction": {"from": "argument", "argument": 0},
 "implementation": {"from": "argument", "argument": 1}}
{"id": "autofac-as", "kind": "registration", "description": "RegisterType<C>().As<I>()",
 "anchor": {"call": "As"}, "abstraction": {"from": "argument", "generic": 0},
 "implementation": {"from": "chain", "callee": "RegisterType", "generic": 0}}
{"id": "mef-export", "kind": "registration", "description": "[Export(typeof(I))] on C: MEF gives C where I is imported",
 "anchor": {"annotation": "Export"}, "abstraction": {"from": "annotationArgument"}, "implementation": {"from": "annotated"}}
```

A registration through a factory (`AddScoped<IFoo>(sp => new Foo(…))`), by convention (assembly scanning) or in a
configuration file has no such fact: say so to the user instead of forcing a rule.

MdExplorer writes `resolvesTo` from the abstraction to the implementation and method → method with the same name; for
`@Provides` the implementation is the method itself. A point whose class is outside the graph (a library) is read
correctly and counted in `outsideGraph`, not against the rule. The same works for any registry written with class
literals (a Java `MapBinder`/`Multibinder` chain, a framework of the project): read the code, find the anchor.

### The `describe` question

Optional on any rule: one question MdExplorer asks the LLM for EACH point the rule links, with the lines around it and
the names (and summaries) of both ends; the answer becomes the link's `description`. Write it for the user's question,
answerable from those lines: «With which qualifier (`@Named`) and scope is the implementation bound?», «Which message
key leads to this handler, and under which condition?». Not «describe this binding»: the edge already says that.

## Page dialects: when the graph has no pages

MdExplorer does not know the extension of server-side templates (`.template`, `.tpl`…): it skips them, so the graph
stops before the user's click. Only for a concrete question of the user:

1. Answer anyway from the source; tell the user the graph has no pages.
2. Read a few pages and write the dialect. Directive pages (`$include[x]$`, `$if[X]$`): how a directive looks (a regex
   with `word` and `arg`), which words include another page, which open and close a block, which blocks are conditions.
   Pages that mix server code and HTML (Razor, JSP, Thymeleaf…): `recognizers`, one regex per role (include, open with
   `close: brace`, `close: element` or a close recognizer, else, case, call with `a0…a9`), `events` for attributes like
   Angular `(click)`, `dynamic` for server-written strings. Angular templates (`.html`): `*ngIf` → open with
   `close: element, guard: true`, with the groups `then`/`else` for `*ngIf="x; else other"`, and
   `<ng-template #other>` → a recognizer with `role: named` (group `name`, `close: element`); templates written in the
   `.ts` (`` template: `…` ``) → `inline: { extensions: [.ts], match: … (?<body>…) … }`, read as the page
   `<file>.ts#template` with the lines of the `.ts`; child components → a call recognizer with `callee: selector`. Dry-run it with `code_language_try`; fix it until `accepted`, and explain every
   note (`unknownWords`, recognizers that never match).
3. Open every file of the returned `sample` and check the facts (controls, conditions, included pages).
4. `code_language_propose` with the problem in plain words. The user decides in the MarkAgent tab; MdExplorer writes
   `.mde-languages/<id>/` and re-analyzes. Never write that folder yourself.
5. After the scan MdExplorer links by itself a control to the function of a JS file the page loads (or of a page it
   includes); for Angular, the component to its template (`renders`), `(click)="save()"` to the component's method, and
   `<app-x>` to the component with that selector (`instantiates`). The rest are call sites for enrichment rules: a string naming a controller method → a rule as above; a string
   naming the page shown next (`nextPage("orders")`, `return View("Orders")`) → a rule with `owner.from: page`.
