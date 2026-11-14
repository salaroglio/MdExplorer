---
title: Scrivere un workflow
---

# Scrivere un workflow

## TL;DR

Un workflow è un lavoro che passa di mano, scritto in modo che MdExplorer lo possa eseguire: i passi, chi prepara ogni
passo (un agente) e chi lo approva o decide (una persona di un ruolo). Prima lo si descrive a parole, poi lo si scrive con
il MarkAgent, infine lo si controlla con lo strumento `check_workflow`.

- Due file: il documento per le persone (`mde_type: workflow`) e il `.workflow.json` che MdExplorer esegue.
- Ogni passo ha un innesco (un lancio, un pulsante, la fine di altri passi) e un modo di partire (a mano, dal responsabile, da solo).
- Le uscite sono file: un passo riceve i file approvati dei passi prima di lui.

## I due file

| File | Per chi | Che cosa dice |
|---|---|---|
| `<nome>.md` con `mde_type: workflow` | le persone | a che cosa serve, il disegno del giro, chi fa che cosa |
| `<nome>.workflow.json` | MdExplorer | i passi, gli inneschi, gli ingressi, chi approva, i giri di rifacimento |

Il documento nomina il suo file JSON e lo disegna con un blocco `plantuml(@workflow, ./<nome>.workflow.json)`.
MdExplorer lo esegue quando il documento è nell'elenco dei workflow del progetto (Impostazioni del progetto →
«Documenti dei workflow»).

## I passi

- **Il passo di un agente** fa uno dei lavori che la sua scheda dichiara. MdExplorer gli manda l'incarico: che cosa deve
  fare, che cosa riceve, che cosa consegna.
- **Il passo di una persona** arriva nella posta di chi ha quel ruolo: un compito, i documenti da leggere e gli **esiti**
  come pulsanti («Partecipiamo», «Rinunciamo»). Si chiude scegliendo un esito.

## Che cosa fa partire un passo

| Innesco | Quando | Come parte |
|---|---|---|
| lancio | una persona lancia l'agente a mano | a mano |
| un pulsante | qualcuno preme una risposta dell'agente, o l'esito di una persona | dal responsabile, o da solo |
| dopo altri passi | quei passi sono finiti (tutti, o uno qualsiasi) | dal responsabile, o da solo |

«Dal responsabile» vuol dire che il passo aspetta nella posta di chi risponde dell'agente, come «Da avviare»: è la scelta
giusta quando l'agente lavora per una persona diversa da quella che ha fatto scattare il passo. «Da solo» vuol dire che
parte appena può.

Ogni giro comincia da un lancio: ogni passo deve essere raggiungibile, di innesco in innesco, da un passo lanciato a mano.

## Chi approva

La consegna di un agente la approva chi risponde dell'agente. Se deve approvarla qualcun altro, il passo dice quale
**ruolo** approva: per esempio la direzione approva la scheda contrattuale. Due approvazioni sono due passi uno dopo
l'altro.

## Ingressi, uscite e variabili

- Le **uscite** di un passo sono file con un percorso (`gare/schede/tecnica.md`); un nome può avere un `*` quando i file
  sono più d'uno.
- Gli **ingressi** di un passo sono testi, oppure i file approvati di passi precedenti («l'uscita `scheda` del passo
  `tecnica`»).
- Le **variabili** sono i valori di un giro (il codice del bando): li porta un pulsante o un esito, e i testi li usano
  fra graffe, come `{codice}`.

Una cartella di uscita deve esistere già nel progetto, con un suo `README.md`: non tutti i motori creano le cartelle da
soli. `check_workflow` te lo ricorda.

## Rifare un passo

Un lavoro rifiutato si ferma finché chi ne risponde non preme «Fai ripartire». Il workflow può dire di più: quali passi si
rifanno insieme, se ripartono a mano o da soli, quante volte al massimo. Senza indicazioni, si può sempre far ripartire.

## La scheda dell'agente: la sua interfaccia

Il workflow collega i passi; la scheda di ogni agente dice che cosa sa fare ciascun suo lavoro:

| Parte della scheda | Che cosa dice |
|---|---|
| ingressi (`inputs`) | che cosa il lavoro riceve nell'incarico |
| letture (`reads`) | i documenti fissi del progetto che legge |
| uscite (`outputs`) | i file che consegna |
| risposte (`replies`) | i pulsanti che può offrire alla persona |

Una scheda non nomina mai un altro agente o una persona: dice «ricevo il documento di gara», non «il collega mi manda il
documento». È il workflow a dire da dove arriva.

## L'ordine di lavoro

1. **Descrivi** il lavoro a parole: dove comincia, dove finisce, che cosa produce.
2. **Dividilo in passi**, e metti in fila quelli che dipendono uno dall'altro.
3. Per ogni passo decidi **chi prepara** (quale agente) e **chi approva o decide** (quale ruolo).
4. Decidi le **uscite come file**, con i loro percorsi.
5. **Scrivilo**: le schede degli agenti nuovi, il workflow, il documento.
6. **Controllalo** con lo strumento `check_workflow` finché non dice che è valido, e leggi gli avvisi.
7. **Aggiungilo** ai workflow del progetto, e fai un primo giro di prova.

## Scriverlo con il MarkAgent

Non serve scrivere a mano il JSON. Racconta il lavoro al MarkAgent: lui usa le skill `mde-workflow` (il workflow),
`mde-agent` (le schede) e `mde-ownership` (i ruoli), ti propone i file e li controlla con `check_workflow` prima di
consegnarli. Tu leggi, correggi e approvi.

## Un piccolo esempio

La revisione di un contratto, in due passi:

| Passo | Chi | Che cosa consegna | Innesco |
|---|---|---|---|
| Bozza del parere | un agente «assistente legale» | `contratti/parere.md` | lanciato a mano dal responsabile legale |
| Decidi | la persona con il ruolo «Direzione» | un esito: «Firmiamo» o «Da rivedere» | dopo la bozza, approvata |

La direzione riceve il parere approvato come ingresso, e il suo esito resta nel registro dei giri. Con «Da rivedere» si
può aggiungere un passo che riparte da quell'esito.

Il capitolo successivo applica tutto questo a un sistema qualità:
[Mettere in piedi la ISO 9001 con i workflow](iso-9001/README.md).
