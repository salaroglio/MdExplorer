---
name: mde-code-check
description: Check whether the relations of MdExplorer's code graph are TRUE against the source, and correct what produced a wrong one. Use when the user asks a precise question whose answer rests on a link of the graph («is it true that X calls Y?», «does this button really reach this procedure?», «who really handles this event?»), doubts an answer taken from the graph, asks to validate an enrichment rule, a language package or a kind of relation (events, dependency injection, calls to the database, project references), or wants to know how far the graph of a new language or of a new MdExplorer feature can be trusted. Triggers on "è giusta questa relazione", "verifica la relazione", "verifica il grafo", "controlla la regola", "valida le regole", "validazione delle regole", "la regola è sbagliata", "il collegamento è sbagliato", "manca un collegamento", "posso fidarmi del grafo", "quanto è affidabile il grafo", "is this link right", "check the rule", "validate the rules", "validate the graph", "wrong edge", "missing edge", "can I trust the graph".
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

# Checking the code graph against the source

The graph is a **claim** about the code; the source is the **truth**. This skill is the method to compare the two for a
precise question, to say how far the claim holds, and to correct what produced a wrong link. How to read the graph, the
tools and the shape of rules are in the skill `mde-code`: read it first if you have not.

Two things can be wrong, and you look for both:

- a **false link**: the graph has an edge the source does not justify;
- a **missing link**: the source connects two things and the graph has no edge.

## The method

1. **The claim.** State in one line what is being checked (one link, one rule, one kind of relation in one folder). Get
   the edges from the graph with `code_neighbors`, `code_path` or `code_impact`: for each, the relation, the
   **confidence** and `path:line`.
2. **The sample.** One link asked by the user: that link. A rule, a package or a kind of relation: at least 10 points,
   or all of them when fewer, **spread over different files and folders** — never the first ten of one file. Take half
   from the graph (to find false links) and half from the source (to find missing ones: search the construct in the
   files with your search tools, then look for its edge in the graph).
3. **The source.** Open every point at `absolutePath` and `line` (`remote_read` for a remote root) and read enough
   around it to decide. Verdict for each: **right**, **wrong** (say what the source does instead), or **cannot be told
   from the code** (the target is chosen at run time: a variable, configuration, data).
4. **Who made the link** decides what you can do (table below).
5. **The report.** Answer the user's question from the source first. Then the numbers: checked, right, wrong, cannot be
   told; for each wrong one `path:line`, what the graph says, what the source says; and what you propose. Do not turn a
   sample into a percentage of the whole program: say «9 of 10 checked».

## Who made the link, and what to do when it is wrong

| The link | Who made it | What you do |
|---|---|---|
| confidence `rule` (`dispatches`, `resolvesTo`) | an enrichment rule (`code_rules` lists them) | Correct the rule and keep its `id`: `code_rule_try` until accepted, open its sample in the source, `code_rule_propose`. The user's «yes» replaces the old rule and its links. If no rule can be right, say so and, **when the user agrees**, `code_rule_remove`. |
| symbols and edges of a language of `.mde-languages/` | the language package (line rules) | Correct the rules of the package, raise its `version`, `code_language_try`, check the sample files, `code_language_propose`. |
| `extracted`, `inferred`, `ambiguous` of a built-in language (C#, Visual Basic, Java, TypeScript…) | MdExplorer's own parser | You cannot change it. If a rule can add a MISSING link, write the rule. Otherwise write a **finding** (section «Findings» of `mde-code`): the shape and the count, never the customer's names. Tell the user that this part of the graph is wrong and what the source says. |
| a description (`summary`) that says what the code does not do | the description step | `code_set_description` after reading the source. |
| nothing in the graph because the code is not analysed | nobody | `code_analysis_propose`, or say which folder to mark as program. |

Rules and packages are applied by the user's «yes», never by you; you never write under `.mde-code/` except a finding.

## Reading a rule's numbers

- `resolved` = the name was found in the graph. `outsideGraph` = the name is right but its target is not in the graph
  (a library class; for `owner.from: symbol`, a procedure of sources not analysed yet, linked to an `external` node).
  `unresolved` = read but not linked: `missCounts` says why.
- For a rule with `owner.from: symbol` MdExplorer links every name it reads, so it cannot reject a wrong one: **you**
  check. Read `outsideGraphNames`: every name must be what the rule says it is (a procedure, a query, a report). Names
  that are something else, or all the names outside because the language id in `languages` is wrong, mean the rule is
  wrong even though it is `accepted`.
- A rule accepted months ago can be wrong today: the code changed. Check the points, not the date.

## Where a graph of C# or Visual Basic deserves a check first

These are read by MdExplorer's parser and are recent: nobody has yet seen them on many real programs. When the user
asks how far the graph can be trusted, or an answer goes through one of them, check a sample:

- **Types of another project of the solution**: a call into a referenced project must end on the real symbol, not on an
  `external` with the same name. An `external` there means the project file or its reference was not read.
- **Types of .NET Framework** (`System.Web`, `System.Data`, Windows Forms): when the framework libraries are missing on
  the computer the scan warns and these stay `ambiguous`. Look at the scan's warnings before trusting the base classes.
- **Events**: `Handles`, `AddHandler`, `+=`, `RaiseEvent`; in VB the `WithEvents` field → its handler. Missing by
  design: a handler named only in the markup (`OnClick="…"` in an `.aspx`) or by convention (`Page_Load`).
- **Visual Basic forms that are not C#**: calls without parentheses, `x(i)` that is an index and not a call, members
  of a `Module` used without its name, `Declare Function`, `My.…`, default properties. With `Option Strict Off` a call
  on an `Object` is decided at run time: no edge is right, an invented one is wrong.
- **Names in strings**: assigned to a property, passed to a constructor, held in a constant (`code_schema` shows which
  callees have them).
- **SQL written in strings**: `reads`, `writes` and `calls` from the method to the tables and procedures its SQL names.
  Check both ways: a string for people taken for SQL («Select a file from…»), an alias or a `WITH` name taken for a
  table, a write taken for a read; and SQL the graph missed — built in pieces that do not start with an SQL word, read
  from a file or from configuration. The node `?` is right when the table's name really comes from a variable.
- **Dependency injection**: a `resolvesTo` edge says who is REGISTERED. A registration made with a factory (a lambda),
  by scanning assemblies or in a configuration file is not read: an interface with no `resolvesTo` is not «unused».

A false or missing link found here is for a finding: say the construct, the count, what the graph says and what the
source says.

## Never

- Never answer «the graph is right» without having opened the source of the sample.
- Never run the program, its tests or anything on its database to check a link: read the code.
- Never correct a rule so that its numbers look better: a rule that links less and links right is the better one.
