---
title: La ISO 9001 come workflow
---

# La ISO 9001 come workflow

## TL;DR

La ISO 9001 chiede di gestire il lavoro per processi: ognuno con i suoi input e output, chi ne risponde e le prove che è
stato fatto come previsto. Un workflow della città contiene già queste tre cose, e git conserva le prove senza lavoro in
più. Resta fuori ciò che nessun workflow dice da solo: se i processi sono quelli giusti.

- Processo = workflow: i passi sono le attività, i ruoli sono le responsabilità, gli esiti sono le decisioni.
- L'evidenza che l'auditor chiede è il registro dei giri e i commit approvati, non un modulo compilato a parte.
- Contesto, direzione e miglioramento restano lavoro delle persone: il workflow li rende visibili, non li fa.

## L'idea

Un sistema qualità risponde a tre domande per ogni attività che conta:

1. **Che cosa entra e che cosa esce?** Il reclamo entra, la risposta al cliente e l'eventuale azione correttiva escono.
2. **Chi ne risponde?** Non «l'ufficio», ma un ruolo preciso, con una persona dietro.
3. **Come si dimostra che è andata così?** Con qualcosa di scritto, datato, attribuibile a qualcuno.

Un workflow della città risponde alle stesse domande, in un formato che si esegue:

| Domanda | Nel workflow |
|---|---|
| Che cosa entra e che cosa esce | gli ingressi e le uscite di ogni passo: file del progetto, con il loro percorso |
| Chi ne risponde | il ruolo di ogni passo di una persona e di ogni approvazione, legato a persone vere in `responsabilita.md` |
| Come si dimostra | ogni consegna approvata è un commit; ogni decisione è un esito registrato con chi, quando e su quale commit |

Per questo si può dire che **un sistema ISO 9001 è, in gran parte, un insieme di workflow fatti bene**.

## Com'è fatto un workflow

Un workflow è un documento markdown con `mde_type: workflow`, che lo spiega alle persone, più un file JSON che
MdExplorer esegue. Il JSON elenca i passi:

- **il passo di un agente**: l'agente prepara un documento; chi risponde dell'agente, o un altro ruolo, lo approva;
- **il passo di una persona**: un ruolo legge ciò che riceve e chiude il passo con un esito, cioè un pulsante
  («Risolto» / «Serve un'azione correttiva»);
- **i giri ripetuti**: un documento rifiutato si rifà, finché non è approvato.

Gli agenti **preparano**, le persone **approvano e decidono**. MdExplorer passa il lavoro da un passo all'altro con
regole fisse: un agente non decide mai da solo chi viene dopo.

## Perché l'auditor ci trova quello che cerca

Un auditor non vuole vedere procedure scritte bene: vuole vedere che il lavoro **è andato** come le procedure dicono.
Con la città le prove nascono mentre si lavora:

- il documento consegnato, nella versione approvata, con il suo commit;
- chi lo ha approvato, e quando;
- l'esito di ogni decisione, con la persona che ha premuto il pulsante;
- i rifiuti e i rifacimenti, con il motivo scritto da chi ha rifiutato;
- la versione del workflow con cui il giro è nato: un giro tiene il workflow che aveva all'inizio.

È l'informazione documentata che la 7.5 chiede di conservare: identificata, datata, attribuita, protetta dalle modifiche
non autorizzate perché ogni cambiamento resta nella storia di git.

## Che cosa un workflow non copre

Restano alle persone:

- **il contesto e le parti interessate** (clausola 4): quali processi servono davvero, per chi, con quali confini;
- **la leadership** (clausola 5): la politica per la qualità, gli obiettivi, l'impegno della direzione;
- **le risorse e le competenze** (7.1, 7.2): che le persone sappiano fare ciò che il ruolo chiede;
- **il giudizio**: un riesame approvato in fretta resta un riesame fatto male, anche se è registrato.

Il workflow rende visibili questi punti: un ruolo senza persona, un passo approvato in un minuto, un processo senza
workflow. Non li risolve.

## Il punto di partenza

Il primo passo non è scrivere un workflow: è descrivere i processi che il gruppo ha già, uno per riga, in
`qualita/processi.md`. Come farlo, e come arrivare al workflow, lo spiega
[Dal processo al workflow](03-dal-processo-al-workflow.md).

Vedi anche: [Le clausole che contano](02-le-clausole-che-contano.md).
