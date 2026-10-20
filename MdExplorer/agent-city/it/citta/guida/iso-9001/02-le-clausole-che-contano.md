---
title: Le clausole che contano
---

# Le clausole che contano

## TL;DR

La mappa fra le clausole della ISO 9001:2015 che toccano il lavoro di tutti i giorni e le cose della città che le
coprono: ruoli, documenti, workflow, storia in git. Le clausole sono descritte con parole nostre e con il loro numero:
per il testo esatto si legge la norma.

- Ruoli e responsabilità (5.3) stanno in `responsabilita.md`; le prove (7.5) nei documenti, in git e nel registro dei giri.
- Requisiti (8.2), modifiche (8.5.6), non conformità (8.7, 10.2) e audit (9.2) diventano workflow con esiti.
- Contesto, leadership e riesame di direzione restano lavoro delle persone, ma lasciano anch'essi documenti nel progetto.

## Come leggere le tabelle

Per ogni clausola: che cosa chiede, in breve, e dove sta in un progetto con la città. Una clausola non elencata non è
meno importante: è solo meno legata al modo in cui il gruppo passa il lavoro.

## Organizzazione e responsabilità

| Clausola | Che cosa chiede | Dove sta |
|---|---|---|
| 4.4 processi del sistema | conoscere i processi, i loro input e output, chi ne risponde, come si controllano | `qualita/processi.md`, una riga per processo; un workflow per ogni processo che passa di mano |
| 5.3 ruoli e responsabilità | assegnare e comunicare chi ha quale responsabilità | `responsabilita.md`: ruolo, persona, email, agenti |
| 7.2 competenza | persone competenti per il ruolo, con prove | `responsabilita.md` dice chi; la prova della competenza va tenuta a parte |

Il **ruolo** è il nome che i workflow usano; la persona è chi ne risponde oggi. Quando qualcuno cambia incarico si cambia
una riga, e la storia in git dice da quando.

## Rischi e informazioni documentate

| Clausola | Che cosa chiede | Dove sta |
|---|---|---|
| 6.1 rischi e opportunità | considerare i rischi che possono impedire i risultati, e agire | una sezione «Rischi» obbligatoria nei documenti su cui si decide |
| 7.5 informazioni documentate | tenere scritto ciò che serve, identificato, aggiornato, protetto | i documenti del progetto, la storia in git, il registro dei giri nel ramo `mde/giri` |

La 6.1 non chiede un registro dei rischi separato da tutto: chiede che i rischi siano considerati dove si decide. Una
sezione obbligatoria di un documento che un ruolo approva è il modo più semplice per dimostrarlo.

La 7.5 distingue ciò che si **mantiene** (le regole: procedure, workflow) da ciò che si **conserva** (le registrazioni:
riesami fatti, esiti, verbali). Nella città le prime sono documenti che cambiano con versioni approvate; le seconde sono
i documenti consegnati nei giri, che non si riscrivono.

## Attività operative

| Clausola | Che cosa chiede | Dove sta |
|---|---|---|
| 8.2.2 e 8.2.3 requisiti e loro riesame | capire che cosa chiede il cliente e riesaminarlo prima di impegnarsi, registrando l'esito | un workflow con schede, riesame e una decisione con esito, come la gara della demo |
| 8.5.6 controllo delle modifiche | riesaminare e controllare le modifiche, registrando chi le ha autorizzate | un workflow «modifica»: richiesta, analisi dell'impatto, approvazione di un ruolo |
| 8.7 output non conformi | identificare ciò che non va e decidere che cosa farne | il passo di una persona con esiti: correggere, rifare, accettare in deroga, scartare |

## Valutazione e miglioramento

| Clausola | Che cosa chiede | Dove sta |
|---|---|---|
| 9.1 monitoraggio e misura | decidere che cosa misurare e analizzarlo | il registro dei giri: quanti, quanto tempo, quanti rifiuti |
| 9.2 audit interno | controllare a intervalli pianificati che il sistema funzioni | un workflow di audit, con il piano e i rapporti in `qualita/audit/` |
| 9.3 riesame di direzione | la direzione riesamina il sistema e decide | il passo di una persona del ruolo Direzione, con il verbale come documento prodotto |
| 10.2 non conformità e azioni correttive | reagire, trovare la causa, correggere, verificare che abbia funzionato | un workflow con esiti: causa, azione, verifica dell'efficacia |

## Che cosa resta fuori dai workflow

- **La clausola 4** (contesto e parti interessate) e la **5.2** (politica per la qualità): documenti che la direzione
  scrive e approva. Nel progetto stanno in `qualita/`, come ogni altro documento, con la loro storia.
- **La 7.1** (risorse, infrastrutture, strumenti di misura): dipende dal lavoro dell'azienda.
- **L'applicabilità**: se una clausola non si applica, va detto e motivato. È una decisione delle persone.

Vedi anche: [La ISO 9001 come workflow](01-la-iso-9001-come-workflow.md),
[Dal processo al workflow](03-dal-processo-al-workflow.md), [Prepararsi all'audit](04-prepararsi-all-audit.md).
