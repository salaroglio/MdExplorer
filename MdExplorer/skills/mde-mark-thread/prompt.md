---
description: "Istruzioni che MdExplorer mette in fondo a ogni domanda del tab MarkAgent: il modello aggiorna il titolo della chat e aggiunge il punto nuovo del turno alla cronologia, con un blocco nascosto."
mde:
  origin: mdexplorer
  version: 2
  updatePolicy: replace
---

<!--
MdExplorer-managed prompt, letto SOLO dal servizio (risorsa incorporata), mai dal progetto: il formato del blocco lo
legge il codice (ChatThreadBlock), quindi una copia modificata a mano lo romperebbe. Sprint 2026-11-24-Chat-Memorizzate, F5.
Segnaposto: {{title}}, il titolo attuale; {{detail}}, gli ultimi punti del registro dal più vecchio (ChatThreadBlock.PromptPoints).
Dalla versione 2 (26/11/2026) il modello manda solo il punto nuovo: il registro intero lo tiene MDE.
-->

[Istruzione di MdExplorer, non rivolta all'utente] Alla fine della risposta, su righe sue e senza niente dopo, aggiungi questo blocco, che MdExplorer toglie prima di mostrarla:

<<<mde-thread
{"title": "…", "point": "…"}
mde-thread>>>

- "title": il titolo della chat, una riga, al massimo 120 caratteri, nella lingua dell'utente. Scrivilo solo se va cambiato (al primo turno sempre).
- "point": l'argomento di questo turno, in una riga, al massimo 200 caratteri, nella lingua dell'utente. Scrivilo al primo turno sempre, poi solo se è un argomento nuovo rispetto agli ultimi punti qui sotto; se il turno continua l'ultimo punto, non scriverlo. MdExplorer lo aggiunge in fondo alla cronologia della chat: non riscrivere i punti di prima.
- Se né il titolo cambia né c'è un punto nuovo, il blocco è {} .
- Niente altro dentro il blocco: solo questo JSON.

Titolo attuale: {{title}}
Ultimi punti della chat, dal più vecchio:
{{detail}}
