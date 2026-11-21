---
description: "Agente di prova: legge, scrive e risponde"
tools: [read, write, "bash(git:*)", mcp__mdexplorer__search]
mde:
  version: 3
a2a:
  name: redattore-acme
  role: Redattore dei verbali
  summary: >-
    Legge gli appunti della riunione e scrive il verbale; se manca un nome chiede
    a chi ha convocato.
  max_hops: 3
  accepts_messages_from: [user, coordinatore-acme, "revisore: esterno"]
  on_approval_notify:
    - mario.rossi
    - "@tutti"
  skills:
    - id: scrivi-verbale
      description: "Scrive il verbale: titolo, presenti, decisioni"
      inputs:
        - id: appunti
          description: Il file degli appunti (percorso relativo)
      reads:
        - path: docs/riunioni/*.md
          section: "## Decisioni"
          note: solo l'ultima
      outputs:
        - id: verbale
          path: docs/verbali/{data}-verbale.md
          action: writes
          note: "una riga per decisione; mai più di 80 colonne quando si può, altrimenti si va a capo con criterio e senza spezzare le parole"
      replies: [fatto, serve-chiarimento]
  replies:
    - id: fatto
      label: Fatto
      description: Il verbale è scritto
      message: |
        Verbale scritto.
        Guarda {verbale}.
    - id: serve-chiarimento
      label: "Serve un chiarimento?"
      values: [sì, no, "forse: dipende"]
runtime:
  engine: claude
  model: sonnet
---

# Redattore

Corpo del documento.
