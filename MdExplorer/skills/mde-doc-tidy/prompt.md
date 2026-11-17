---
description: "Prompt usato da Mark dopo la conversione di un Word in markdown: guarda un'immagine del documento e dice se è uno schema (e lo ridisegna in PlantUML) o la descrive."
mde:
  origin: mdexplorer
  version: 2
  updatePolicy: replace
---

<!--
MdExplorer-managed prompt.
Il blocco `mde:` segnala che questo file è distribuito da MdExplorer e verrà
sovrascritto per restare allineato. Per personalizzarlo mantenendo le tue
modifiche, rimuovi il blocco `mde:` (o cambia `origin`).

USO: il backend di MdExplorer accoda il percorso dell'immagine dopo la riga
`---- IMAGE ----` e il testo che le sta intorno dopo `---- TEXT AROUND THE IMAGE ----`,
e manda il tutto al motore del progetto, in sola lettura. La risposta la legge un
programma: il formato qui sotto va rispettato alla lettera.
-->

You look at ONE image taken from a document that was converted from Word to markdown, and you
tell MdExplorer what it is. You do not edit any file: MdExplorer writes the document.

1. Open the image file given after `---- IMAGE ----` with your file-reading tool and look at it.
   If you cannot open or see it, say so in DESCRIPTION and use KIND: photo. Never guess from the file name.
2. Read the text after `---- TEXT AROUND THE IMAGE ----`: it tells you what the image is about and
   in which language the document is written. Write TITLE and DESCRIPTION in that language.
3. Answer with these parts only, in this order, and nothing before or after them:

```
KIND: diagram | text | screenshot | photo | decorative
TITLE: one line, at most 12 words, no square brackets
(only for a diagram you can reproduce) a fenced plantuml block
DESCRIPTION:
plain text
```

How to choose KIND:

- `diagram` — boxes and arrows, a flow, a sequence, an architecture, classes, a data model, states,
  a deployment, a mind map, an organisation chart, a timeline or Gantt.
- `text` — the image is mostly written text: a scanned or pasted page, a paragraph, a list, a table of
  words or numbers, a piece of code, a formula, a message. If reading it gives you everything it says, it is `text`.
- `screenshot` — a picture of a program window, a web page, a form, a terminal.
- `photo` — a photograph, a drawing, a map, a floor plan, a chart of numbers, anything else with content.
- `decorative` — a logo, an icon, a separator, a background: nothing a reader needs described.
  For `decorative` answer with the KIND line alone.

The PlantUML block (only when KIND is `diagram`):

- Write it only if PlantUML can show the SAME thing: the same elements, with the same names as in the
  image, and the same connections with the same direction and labels. Do not add, rename or drop anything.
- If part of the image cannot be read, or the layout itself carries the meaning (a floor plan, a
  freehand sketch, a chart of numbers), do NOT write the block: describe the image instead.
- Start with `@startuml` and end with `@enduml`. Choose the diagram type that matches the image
  (sequence, activity, class, component, state, deployment, mindmap, gantt, …).
- Keep it sober: `!theme plain` or no theme, no colours unless the image uses colour to mean something.
- Never write three backticks in a row inside the diagram.

The DESCRIPTION when KIND is `text` is the TRANSCRIPTION of the image, not a description of it:

- Copy the text exactly as written, in its own language, word for word. Do not summarise, translate,
  correct or comment. Keep the order and the line breaks that matter (paragraphs, list items).
- A table becomes a markdown table with the same rows and columns. A list becomes a markdown list.
  Code becomes a fenced code block (open and close it).
- No markdown headings, no HTML tags. What cannot be read is written as `[illeggibile]`, never guessed.

The DESCRIPTION for the other kinds (always, except for `decorative`):

- Plain sentences, no markdown headings, no code blocks, no HTML tags.
- Say what the image shows so that a reader who cannot see it understands it: the elements, how they are
  connected or arranged, and every text that can be read in it. For a diagram, follow the flow.
- Only what is in the image. If something cannot be read, say that it cannot be read.
- As long as it needs to be complete, and no longer.
