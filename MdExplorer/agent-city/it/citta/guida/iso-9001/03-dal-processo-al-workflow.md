---
title: Dal processo al workflow
---

# Dal processo al workflow

## TL;DR

Un workflow si costruisce partendo dal processo com'è oggi, non da un modello ideale: lo si descrive in una riga di
`qualita/processi.md`, lo si divide in passi, si decide chi prepara e chi approva ogni passo, e solo alla fine lo si
scrive, con il MarkAgent. L'esempio che segue è la gestione dei reclami.

- Prima la riga del processo, poi i passi, poi gli agenti e il workflow.
- Un agente prepara, un ruolo approva: ogni passo consegna un file, ogni decisione è un esito con i suoi pulsanti.
- Agenti con la skill `mde-agent`, workflow con la skill `mde-workflow`, controllo con lo strumento `check_workflow`.

## 1. Descrivi il processo in una riga

Se `qualita/processi.md` non c'è ancora, chiedi al MarkAgent di crearlo: una tabella, una riga per processo.

| Campo | Che cosa scrivere | Reclami, per esempio |
|---|---|---|
| Processo, scopo e confini | perché esiste, dove comincia e dove finisce | dal reclamo ricevuto alla risposta al cliente |
| Input / output | che cosa lo fa partire, che cosa consegna | un reclamo / la risposta e, se serve, un'azione correttiva |
| Responsabile ed evidenze | il ruolo che ne risponde; che cosa deve restare scritto | Responsabile qualità; il reclamo, l'analisi, la risposta, la decisione |
| Workflow | il percorso del suo documento, «da progettare», o «nessuno» con il perché | da progettare |

Se una riga non si riesce a compilare, il problema non è il workflow: il processo non è ancora chiaro. Conviene fermarsi
e parlarne nel gruppo.

## 2. Dividilo in passi

Una sequenza di attività, ognuna con **un solo** responsabile e **un solo** risultato:

1. **Registra il reclamo**: chi lo riceve lo descrive, con cliente, data, prodotto o servizio.
2. **Analizza**: che cosa è successo, quale requisito non è stato rispettato, se è un caso isolato.
3. **Decidi**: basta una correzione, o serve un'azione correttiva sulla causa (10.2)?
4. **Rispondi al cliente**: la risposta, approvata da chi ne risponde.
5. **Verifica l'efficacia**, se c'è stata un'azione correttiva: il problema si è ripresentato?

Regola utile: se un passo ha due approvatori, sono due passi.

## 3. Decidi chi prepara e chi approva

- **Lo prepara un agente o lo fa una persona?** Un documento da scrivere (l'analisi, la bozza di risposta) lo prepara un
  agente; una scelta è il passo di una persona, con gli esiti come pulsanti.
- **Chi approva?** Di norma chi risponde dell'agente; quando serve un'altra firma, il workflow lo dice con
  l'approvazione di un ruolo.

| Passo | Chi lavora | Che cosa consegna | Chi approva o decide |
|---|---|---|---|
| Registra | agente del servizio clienti | `qualita/reclami/reclamo-*.md` | Servizio clienti |
| Analizza | agente del servizio clienti | `qualita/reclami/analisi-*.md` | Responsabile qualità |
| Decidi | Responsabile qualità | esito «Correzione» o «Azione correttiva» | è la decisione |
| Rispondi | agente del servizio clienti | `qualita/reclami/risposta-*.md` | Responsabile qualità |

I ruoli devono esistere in `responsabilita.md`, con una persona dietro.

## 4. Gli output sono file

Ogni consegna è un file in una cartella che esiste e ha il suo `README.md`: non tutti i motori creano le cartelle da
soli. Un `*` nel nome va bene quando ogni giro produce un file nuovo. I file sono la prova: devono dire che cosa è stato
fatto, non riassumerlo.

## 5. Che cosa succede quando qualcosa è rifiutato

Un'analisi rifiutata si rifà: è un ciclo «finché non è approvato». Decidi se si ripete senza limite (di solito la scelta
giusta) o al massimo un certo numero di volte, e chi la fa ripartire. È una decisione delle persone.

## 6. Scrivi gli agenti e il workflow

Solo adesso si scrive, con il MarkAgent:

1. le schede degli agenti che servono, con la skill `mde-agent`: ognuna dice che cosa riceve, che cosa legge, che cosa
   consegna;
2. il workflow, con la skill `mde-workflow`: il documento con `mde_type: workflow` e il suo `.workflow.json`;
3. lo strumento `check_workflow` deve rispondere che è valido; ogni avviso va letto;
4. il documento si aggiunge ai workflow del progetto (Impostazioni del progetto, «Documenti dei workflow»);
5. chi risponde del processo lo legge, con il disegno che MdExplorer ne fa, e lo approva;
6. un giro di prova su un caso vero e piccolo, e si aggiusta.

Un giro tiene la versione del workflow con cui è nato: una modifica vale dal giro successivo.

## 7. Aggiorna la riga del processo

In `qualita/processi.md`, al posto di «da progettare», scrivi il percorso del workflow. Così l'elenco dice quali
processi sono già workflow e quali no: è la prima cosa che l'audit interno guarderà.

Vedi anche: [Con il MarkAgent](05-con-il-markagent.md), [Le clausole che contano](02-le-clausole-che-contano.md).
