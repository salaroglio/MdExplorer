---
title: Prepararsi all'audit
---

# Prepararsi all'audit

## TL;DR

L'audit interno (9.2) è il controllo che il sistema fa su se stesso prima che lo faccia un ente esterno. Nella città lo
si prepara leggendo i processi, i workflow, il registro dei giri e la storia in git; il rapporto lo approva una persona
che non risponde dei processi controllati. Le non conformità trovate diventano azioni correttive (10.2).

- Le evidenze ci sono già: documenti approvati, esiti registrati, commit. L'audit controlla che dicano la cosa giusta.
- I rilievi tipici sono pochi e ricorrenti: ruoli senza persona, processi senza workflow, approvazioni troppo rapide.
- Un'azione correttiva si chiude solo quando qualcuno ha verificato che abbia funzionato.

## Che cosa chiede la norma

Audit interni a intervalli pianificati, con criteri e perimetro definiti, fatti da chi non controlla il proprio lavoro,
con i risultati conservati. Il riesame di direzione (9.3) usa poi quei risultati per decidere. Ogni quanto lo decide il
gruppo: scrivilo nel piano degli audit, per esempio in `qualita/audit/README.md`, una riga per audit con data, perimetro,
periodo e stato («da fare», «fatto»).

## Chi prepara l'audit

Puoi farlo con il MarkAgent, o scrivere un agente apposta (skill `mde-agent`) e un workflow d'audit (skill
`mde-workflow`): l'agente prepara il rapporto, chi ha il ruolo di auditor interno lo approva, la direzione decide le
azioni. Un agente che deve leggere la storia di git ha bisogno di poter lanciare comandi: scrivi nella sua scheda che li
usa solo per leggere, e chi se ne fida lo sappia.

In ogni caso **l'indipendenza la garantisce chi approva il rapporto**, non chi lo prepara.

## Le evidenze da mostrare

| L'auditor chiede | Dove si trova |
|---|---|
| chi ha fatto questa attività, con quale ruolo | il registro del giro e `responsabilita.md` alla data del giro |
| il documento su cui si è deciso | il commit approvato indicato nella decisione |
| che la procedura seguita fosse quella in vigore | la versione del workflow salvata dal giro quando è nato |
| che cosa è successo quando qualcosa non andava | i rifiuti con il loro motivo, e la versione rifatta |
| chi ha cambiato una regola, e quando | la storia in git del documento o del workflow |

## Come leggere la storia

- **Il registro dei giri** sta nel ramo git `mde/giri`: `git log mde/giri` dice i giri, `git show mde/giri:<file>` ne
  legge uno.
- **Un documento consegnato**: si parte dalla decisione nel registro, si segue il commit indicato fino al documento.
- **Una regola** (un workflow, una procedura): `git log -- <percorso>` dice ogni modifica, con autore e data. Una
  modifica senza una ragione scritta nel messaggio è già un piccolo rilievo.

## I rilievi tipici

| Rilievo | Che cosa significa | Clausola |
|---|---|---|
| un ruolo dei workflow senza persona | nessuno risponde di quel passo | 5.3 |
| un processo in `qualita/processi.md` senza workflow né evidenze | il processo non è sotto controllo | 4.4 |
| approvazioni in pochi secondi, sempre dalla stessa persona | il controllo forse è solo formale | 8.2.3 |
| un riesame senza la sezione dei rischi | i rischi non sono stati considerati dove si decide | 6.1 |
| un workflow cambiato senza ragione scritta | la modifica non è stata controllata | 7.5, 8.5.6 |
| un'azione correttiva chiusa senza verifica | non si sa se ha funzionato | 10.2 |

## Dalla non conformità all'azione correttiva

La 10.2 chiede di reagire, capire la causa, decidere un'azione, attuarla e verificarne l'efficacia. Conviene farne un
workflow:

1. **Registra** la non conformità, con la prova: un agente prepara, l'auditor approva.
2. **Analizza la causa**: perché è successo, non solo che cosa; il responsabile del processo approva.
3. **Decidi l'azione**: il passo di una persona, con l'azione e la data entro cui verificarla.
4. **Verifica l'efficacia**: esiti «Efficace» o «Non efficace»; nel secondo caso si torna all'analisi.

[Dal processo al workflow](03-dal-processo-al-workflow.md) spiega come costruirlo; il MarkAgent può proporne la prima
versione.

## Prima dell'audit esterno

- Un audit interno completo nel periodo, con il rapporto approvato e le azioni aperte in corso.
- Un riesame di direzione (9.3) che abbia letto quei risultati, con il suo verbale.
- `qualita/processi.md` aggiornato: ogni processo con il suo responsabile, il suo workflow o la ragione per cui non ne ha.

Questa guida aiuta a prepararsi; il giudizio sulla conformità lo dà l'ente di certificazione.

Vedi anche: [Le clausole che contano](02-le-clausole-che-contano.md), [Con il MarkAgent](05-con-il-markagent.md).
