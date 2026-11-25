---
description: "Istruzioni che MdExplorer mette in fondo a ogni domanda del tab MarkAgent: il modello aggiorna titolo e dettaglio della chat nella cronologia con un blocco nascosto."
mde:
  origin: mdexplorer
  version: 1
  updatePolicy: replace
---

<!--
MdExplorer-managed prompt, letto SOLO dal servizio (risorsa incorporata), mai dal progetto: il formato del blocco lo
legge il codice (ChatThreadBlock), quindi una copia modificata a mano lo romperebbe. Sprint 2026-11-24-Chat-Memorizzate, F5.
Segnaposto: {{title}} e {{detail}}, il titolo e il dettaglio attuali della chat.
-->

[Istruzione di MdExplorer, non rivolta all'utente] Alla fine della risposta, su righe sue e senza niente dopo, aggiungi questo blocco, che MdExplorer toglie prima di mostrarla:

<<<mde-thread
{"title": "…", "detail": "- …\n- …"}
mde-thread>>>

- "title": il titolo della chat, una riga, al massimo 120 caratteri, nella lingua dell'utente. Scrivilo solo se va cambiato (al primo turno sempre).
- "detail": i punti trattati finora in tutta la chat, come elenco puntato markdown: un punto per riga, che comincia con "- ", al massimo 5 punti; riuniscili se sono di più. Scrivilo intero, ma solo se cambia.
- Se né il titolo né il dettaglio cambiano, il blocco è {} .
- Niente altro dentro il blocco: solo questo JSON.

Titolo attuale: {{title}}
Dettaglio attuale:
{{detail}}
