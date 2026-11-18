---
description: "Prompt usato da Mark dopo la conversione di un Word in markdown: uno schema che dall'immagine non si è riusciti a ridisegnare viene ridisegnato in PlantUML dalla sua descrizione, corretta con il testo del documento."
mde:
  origin: mdexplorer
  version: 1
  updatePolicy: replace
---

<!--
MdExplorer-managed prompt.
Il blocco `mde:` segnala che questo file è distribuito da MdExplorer e verrà
sovrascritto per restare allineato. Per personalizzarlo mantenendo le tue
modifiche, rimuovi il blocco `mde:` (o cambia `origin`).

USO: è il secondo passo di `mde-doc-tidy`, solo per gli schemi. Il backend di
MdExplorer accoda la descrizione dell'immagine dopo `---- DESCRIPTION OF THE IMAGE ----`,
i titoli del documento dopo `---- DOCUMENT OUTLINE ----` e il capitolo che contiene
l'immagine dopo `---- SECTION OF THE DOCUMENT ----`, e manda il tutto al motore del
progetto, in sola lettura. L'immagine NON viene passata. La risposta la legge un
programma: il formato qui sotto va rispettato alla lettera.
-->

A document was converted from Word to markdown. One of its images is a diagram. Someone looked at
the image and wrote a description of it, but could not redraw it. You do NOT see the image and you
must not look for it: you work from the description and from the document. You do not edit any
file: MdExplorer writes the document.

Do these steps in order.

1. Read the text after `---- DESCRIPTION OF THE IMAGE ----`. It is what was seen in the image.
   Its first line, `Title:`, is the title of the image: do not repeat it in your DESCRIPTION.
2. Read `---- DOCUMENT OUTLINE ----` and `---- SECTION OF THE DOCUMENT ----`. The outline tells you
   what the document is about; the section is the text around the image (`[THE IMAGE IS HERE]`
   marks its place). The text usually explains what the diagram shows: the names of the parts,
   who does what, in which order.
3. Check the description against the document and correct it:
   - a name the description spells approximately, and the document spells exactly, takes the
     spelling of the document;
   - a role, a direction or an order the description leaves vague, and the document states, is
     stated;
   - what the description says and the document does not mention STAYS as the description says:
     the image may show things the text does not repeat;
   - do not add elements that only the document mentions: the diagram shows what the image shows;
   - where the description and the document contradict each other, keep the description and write
     the contradiction under DOUBTS.
4. From the CORRECTED description — not from the document alone — list the elements and the
   connections of the diagram, then draw it in PlantUML.

Answer with these parts only, in this order, and nothing before or after them:

```
DESCRIPTION:
the corrected description, plain text, in the language of the document
ELEMENTS:
- one line per element of the diagram, with its exact name
RELATIONS:
- Name of one element -> Name of another element
DOUBTS:
- one line per contradiction or thing you are not sure of (or the single line: - none)
a fenced plantuml block
```

Rules for the parts:

- DESCRIPTION: plain sentences, no markdown headings, no code blocks, no HTML tags. It replaces the
  first description in the document, so it must be complete: every element and every connection.
- ELEMENTS: the boxes, actors, participants, steps, states, classes, nodes of the diagram. Each name
  written exactly as in the corrected description. No duplicates.
- RELATIONS: every connection, one per line, as `- A -> B`, where A and B are names copied from
  ELEMENTS. The arrow goes in the direction of the flow. Write the label of the connection, if
  any, in the diagram and in the description, not here. If the diagram has no connections, write
  `- none`.
- DOUBTS: only what a reader must go and check on the original image, in short lines in the
  language of the document: a contradiction between the description and the document, an element
  or a direction that stays uncertain even after reading the document. A name or a direction you
  took from the document because the document states it clearly is a correction, NOT a doubt: do
  not list it. Labels that could not be read in the image are not a doubt either: just leave those
  connections without a label. If nothing is left to check, write `- none`. Be honest: a wrong
  diagram that looks sure of itself is worse than a doubt.

Rules for the PlantUML block:

- Start with `@startuml` and end with `@enduml` (or `@startmindmap` … `@endmindmap` and the like).
  Choose the diagram type that matches what the description says (sequence, activity, class,
  component, state, deployment, mindmap, gantt, …).
- Show EVERY element of ELEMENTS with exactly that name, and EVERY connection of RELATIONS. If a
  name is long or has spaces, declare it once with an alias (`rectangle "Gestione ordini" as GO`)
  and use the alias in the connections.
- Do not show anything that is not in ELEMENTS: no extra boxes, no invented names, no notes, no title.
- Label a connection only with words the description gives for it.
- Keep it sober: `!theme plain` or no theme, no colours.
- Never write three backticks in a row inside the diagram.
- Write the block even if you have doubts: MdExplorer shows it together with the description and
  tells the reader what to check. Leave the block out only if the description does not contain a
  diagram at all (no elements that can be drawn).
