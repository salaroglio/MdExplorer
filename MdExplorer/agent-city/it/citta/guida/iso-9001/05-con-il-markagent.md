---
title: Con il MarkAgent
---

# Con il MarkAgent

## TL;DR

Il MarkAgent è l'assistente con cui si costruisce il sistema qualità nella città: descrive i processi con te, propone
agenti e workflow, li scrive con le skill di MdExplorer e li controlla. Tu decidi chi approva, che cosa resta scritto,
quando un processo è pronto: un'approvazione è sempre di una persona.

- Chiedigli una cosa per volta: un processo, un workflow, un audit.
- Le decisioni che spettano alle persone le lascia come domande: rispondi tu.
- Prima di aggiungere un workflow al progetto, fallo controllare con lo strumento `check_workflow`.

## Che cosa chiedergli

Qualche richiesta che funziona, nell'ordine in cui di solito servono:

- «Crea `qualita/processi.md` con le colonne di questa guida, e aiutami a descrivere i processi che abbiamo.»
- «Quali processi di `qualita/processi.md` non hanno ancora un responsabile, un'evidenza o un workflow?»
- «Trasforma la gestione dei reclami in un workflow: proponi i passi, chi prepara e chi approva, i file che restano.»
- «Scrivi le schede degli agenti che servono a questo workflow, con la skill `mde-agent`.»
- «Scrivi il workflow con la skill `mde-workflow` e controllalo con `check_workflow`.»
- «Quali clausole della ISO 9001 copre questo workflow, e quali no?»
- «Prepara il piano degli audit interni in `qualita/audit/README.md` per i prossimi dodici mesi.»
- «Leggi il registro dei giri di marzo e dimmi se ci sono approvazioni sospette o rifiuti senza motivo.»

## Che cosa scrivere tu

Alcune cose non si delegano, perché sono decisioni:

| Che cosa | Perché è tua |
|---|---|
| chi risponde di quale ruolo, in `responsabilita.md` | la 5.3 chiede che le responsabilità siano assegnate da chi ne ha l'autorità |
| quante volte si rifà un passo rifiutato | è una scelta del gruppo, non della norma |
| la politica per la qualità e gli obiettivi | sono della direzione (5.2, 6.2) |
| se una clausola non si applica, e perché | l'applicabilità va motivata da chi conosce l'azienda |
| l'approvazione di ogni consegna | il workflow la vuole da una persona, dal suo computer |

## Come lavora il MarkAgent con te

- **Propone, non decide**: quando un punto spetta a te lo scrive come domanda aperta.
- **Non inventa la norma**: descrive le clausole con parole sue e il loro numero; se un punto è dubbio, lo dice e
  rimanda al testo ufficiale.
- **Chiede prima di cambiare i file**: un workflow in uso si cambia solo dicendolo, e la modifica vale dal giro dopo.
- **Usa gli strumenti di MdExplorer**: `check_workflow` per i workflow, la ricerca nei documenti per trovare ciò che il
  gruppo ha già scritto.

## Un percorso per cominciare

1. Fai il giro «Ciao mondo» della [guida della città](../README.md), per vedere un giro intero.
2. Con il MarkAgent descrivi due o tre processi in `qualita/processi.md`, non tutti.
3. Scegline uno piccolo, che passa di mano spesso (i reclami vanno bene), e trasformalo in workflow.
4. Fai un giro vero, poi aggiusta: il primo workflow insegna come scrivere i successivi.
5. Quando ce ne sono alcuni in funzione, scrivi il piano degli audit e fai il primo audit interno.
6. Per un esempio completo guarda la gara nella demo di MdExplorer, dal bando al riesame dei requisiti (8.2.3).

Vedi anche: [Dal processo al workflow](03-dal-processo-al-workflow.md), [Prepararsi all'audit](04-prepararsi-all-audit.md).
