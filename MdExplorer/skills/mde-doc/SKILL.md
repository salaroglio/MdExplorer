---
name: mde-doc
description: Write technical and analysis documents using MdExplorer conventions. Use whenever you author or update an analysis, design, sprint, how-to, or any narrative `.md` document (NOT a README — see the mde-readme skill for that). Every such document opens with a self-contained TL;DR header.
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


# MdExplorer documentation convention

Rule for AI agents producing markdown documents in an MdExplorer project. It applies to **analysis, design, sprint, how-to, and any narrative `.md` document**. It does NOT apply to README files (see the `mde-readme` skill for those).

## TL;DR header (mandatory)

Every document must start with a TL;DR section: **at most 3 lines of prose, followed by a bullet list of exactly 3 points**.

```
## TL;DR
Concise summary in at most three lines that explains what this document covers and why it matters.
- First key takeaway
- Second key takeaway
- Third key takeaway
```

The TL;DR must be **self-contained**: a reader who reads only the TL;DR should leave with the gist of the whole document. Write it last, once the body is finished, so it reflects what the document actually says.

## Checklist

- [ ] The document opens with a `## TL;DR` section, before any other content.
- [ ] TL;DR is at most 3 lines of prose, followed by exactly 3 bullet points.
- [ ] The TL;DR conveys the gist on its own, without the reader needing the body.
