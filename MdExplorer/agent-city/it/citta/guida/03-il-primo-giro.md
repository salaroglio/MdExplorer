---
title: Il primo giro
---

# Il primo giro

## TL;DR

Il modo più veloce per capire la città è farla girare una volta. La città ha portato nel progetto il workflow «Ciao
mondo» (`citta/ciao.md`): un solo passo, in cui l'agente `citta-ciao` scrive «hello world» in `citta/ciao/ciao-mondo.md`
e tu approvi la consegna.

- Prima prendi il ruolo «Responsabile della città» e concedi la fiducia all'agente.
- Poi lancia l'agente: la consegna arriva nella tua posta, da approvare.
- Il giro resta nel registro dei giri: hai visto tutto il percorso, dall'inizio alla fine.

## Che cosa c'è già

| File | Che cos'è |
|---|---|
| `citta/ciao.md` | il documento del workflow, con il disegno del giro |
| `citta/ciao.workflow.json` | il workflow vero e proprio, che MdExplorer esegue |
| `citta/agenti/citta-ciao.agent.md` | la scheda dell'agente d'esempio |
| `citta/ciao/` | la cartella in cui l'agente consegna |
| `responsabilita.md` | il documento delle responsabilità, con il ruolo aperto «Responsabile della città» |

## Passo 1: prendi il ruolo

Apri il pannello «Città degli agenti». L'agente `citta-ciao` compare come «Senza responsabile: non parte». Premi «È mio»:
MdExplorer scrive nel documento delle responsabilità il tuo nome e la tua email git accanto al ruolo. Committa il
documento, così anche i colleghi sanno che il ruolo è tuo.

## Passo 2: concedi la fiducia

Sempre nel pannello, premi «Concedi trust» sull'agente. Leggi che cosa fa e che cosa può fare sul tuo computer: questo
agente legge e scrive file del progetto, e basta. Premi «Mi fido». Il pannello ora lo mostra come «Fidato».

## Passo 3: lancialo

Lancia l'agente (finestra «Lancia agente», pulsante «Lancia ora»). Il workflow dice che questo è il primo passo del giro e
che parte a mano: MdExplorer apre un giro nuovo e manda all'agente il suo incarico. L'agente lavora in una copia sua del
progetto, così non tocca i file su cui stai lavorando tu.

## Passo 4: approva la consegna

Quando l'agente ha finito, nella «Posta degli agenti» compare un lavoro «Da approvare»: il file
`citta/ciao/ciao-mondo.md`, con «hello world», la data e il giro. Aprilo, guardalo e premi «Autorizza». Il lavoro entra
nel progetto.

Se invece premi «Rifiuta», scrivi il motivo: il lavoro si ferma, e riparte solo quando premi «Fai ripartire». L'agente
riceve il tuo motivo insieme all'incarico.

## Passo 5: guarda il giro

Nella sezione «Giri» della posta trovi il giro appena fatto, con i suoi passi e i suoi messaggi: «concluso: 1 su 1
passi». È scritto anche nel registro dei giri in git. Quando non ti serve più, «Archivia il giro» lo toglie dall'elenco:
lo ritrovi nell'«Archivio».

## Se non parte

| Che cosa vedi | Che cosa vuol dire | Che cosa fare |
|---|---|---|
| «Senza responsabile: non parte» | nessuno ha preso il ruolo | premi «È mio» (passo 1) |
| «Non fidato» o «Trust decaduto» | manca la fiducia, o la scheda è cambiata | concedi di nuovo la fiducia (passo 2) |
| in coda, «lavora sul computer del suo responsabile» | il ruolo è di un collega | lo lancia il collega, dal suo computer |
| in coda, «il workflow non si legge» | c'è un errore in un workflow del progetto | chiedi al MarkAgent di controllarlo con `check_workflow` |
| nessun motore | il progetto non ha un motore AI | sceglilo nelle impostazioni del progetto |

## E poi

Quando il giro funziona, il workflow «Ciao mondo» ha fatto il suo lavoro: puoi toglierlo dalle impostazioni del progetto
(«Togli questo workflow») e cancellare i suoi file (`citta/ciao.md`, `citta/ciao.workflow.json`, la scheda
`citta/agenti/citta-ciao.agent.md` e la cartella `citta/ciao/`), lasciando questa guida dov'è. Poi scrivi i tuoi: [Scrivere un workflow](04-scrivere-un-workflow.md).
