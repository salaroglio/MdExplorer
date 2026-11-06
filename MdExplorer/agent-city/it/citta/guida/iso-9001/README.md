---
title: La ISO 9001 nella città degli agenti
---

# La ISO 9001 nella città degli agenti

## TL;DR

Questo capitolo della guida spiega come mettere in piedi, con il MarkAgent, i workflow che gestiscono un sistema
qualità ISO 9001 nella città degli agenti. Non c'è un metodo da installare: i processi, gli agenti e i workflow li
costruisce il gruppo, partendo da come lavora oggi.

- Un processo che passa di mano diventa un workflow: chi prepara, chi approva, che cosa resta scritto.
- Le prove per l'audit nascono mentre si lavora: documenti approvati, esiti, registro dei giri, storia in git.
- È una guida, non un sostituto della norma né del giudizio di un ente di certificazione.

## Da dove partire

Se è la prima volta che usi la città, leggi prima la [guida della città](../README.md) e fai il giro «Ciao mondo»:
qui si dà per scontato che tu sappia che cos'è un workflow, un agente e il documento delle responsabilità.

## L'ordine di lettura

1. [La ISO 9001 come workflow](01-la-iso-9001-come-workflow.md): l'idea, e che cosa un workflow non copre.
2. [Le clausole che contano](02-le-clausole-che-contano.md): la mappa fra le clausole e le cose di MdExplorer.
3. [Dal processo al workflow](03-dal-processo-al-workflow.md): passo per passo, con la gestione dei reclami.
4. [Prepararsi all'audit](04-prepararsi-all-audit.md): l'audit interno, le prove, le azioni correttive.
5. [Con il MarkAgent](05-con-il-markagent.md): che cosa chiedergli, che cosa scrivere tu.

## Che cosa ti serve nel progetto

Niente arriva già fatto. Il capitolo propone una convenzione semplice, da creare con il MarkAgent quando serve:

| Che cosa | Dove | A che cosa serve |
|---|---|---|
| L'elenco dei processi | `qualita/processi.md` | un processo per riga, con chi ne risponde e il suo workflow |
| Gli audit interni | `qualita/audit/` | il piano, i rapporti, le azioni decise |
| I ruoli | `responsabilita.md` | chi risponde di che cosa, con nome ed email |
| I workflow | dove il gruppo preferisce | un documento con `mde_type: workflow` e il suo `.workflow.json` |

Per scrivere gli agenti si usa la skill `mde-agent`, per i workflow la skill `mde-workflow`; prima di aggiungere un
workflow al progetto lo si controlla con lo strumento `check_workflow`.

## Un modello da guardare

La demo di MdExplorer contiene il giro della gara, dal bando al riesame dei requisiti (8.2.3): quattro agenti preparano
le schede, l'account manager decide se partecipare. È un buon esempio di processo già diventato workflow, da leggere
prima di scrivere il primo dei tuoi.

## Le parole

| Parola | Che cos'è |
|---|---|
| Processo | un'attività che il gruppo ripete, con input, output e un responsabile |
| Workflow | il processo scritto in modo che MdExplorer lo esegua: passi, ruoli, esiti |
| Giro | una volta che il workflow viene eseguito, dall'inizio alla fine |
| Registro dei giri | la storia dei giri, nel ramo git `mde/giri` |
| Evidenza | ciò che resta scritto e dimostra che il lavoro è andato come previsto |
