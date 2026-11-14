---
title: Com'è fatta la città
---

# Com'è fatta la città

## TL;DR

Nella città degli agenti ogni agente prepara un pezzo di lavoro e lo consegna a una persona, che lo approva o lo rifiuta.
Un agente lavora solo sul computer di chi ne risponde, e solo dopo che quella persona se ne fida. I lavori che passano di
mano seguono un workflow, e ogni giro resta scritto in git.

- Gli agenti preparano, le persone approvano e decidono: nessun agente approva il proprio lavoro.
- Un agente senza responsabile non parte; un agente non fidato non lavora.
- MdExplorer fa da regista: fa partire i passi, consegna gli incarichi, registra i giri.

## Agenti e persone

Un **agente** è un file `*.agent.md` nel progetto: una scheda che dice chi è, che lavori sa fare, che cosa legge e che
cosa consegna. Lavora con il motore AI del progetto (Claude, Copilot o opencode).

Una **persona** risponde di uno o più agenti. È lei che:

- dichiara suo l'agente, con il pulsante «È mio» nel pannello «Città degli agenti»;
- gli concede la fiducia, con «Concedi trust» e poi «Mi fido», dopo aver letto che cosa fa e che cosa può fare sul suo
  computer;
- legge quello che l'agente consegna, e lo autorizza o lo rifiuta.

Il pannello mostra per ogni agente a chi risponde: «Risponde a te: lavora su questo computer», oppure «Senza responsabile:
non parte». Un agente che risponde a un collega lavora sul computer del collega, non sul tuo.

## Perché un agente lavora su un solo computer

L'agente usa gli strumenti e le credenziali di chi ne risponde. Per questo lavora soltanto lì: se lanci un agente di cui
risponde un collega, il lavoro aspetta in coda finché non lo fa partire lui. È una scelta di sicurezza, non un limite
tecnico: ognuno è padrone di ciò che succede sul proprio computer.

## La fiducia

Prima di lavorare, un agente deve essere **fidato**. Concedere la fiducia vuol dire leggere la scheda dell'agente e dire
«Mi fido». MdExplorer mostra due cose diverse: che cosa l'agente dice di fare («scritto dall'autore, non verificato») e
che cosa può fare davvero sul tuo computer («calcolato dall'app»).

Se qualcuno cambia la parte importante della scheda (gli strumenti, i lavori che sa fare), la fiducia decade: il pannello
mostra «Trust decaduto» e l'agente aspetta che tu la conceda di nuovo.

## La posta degli agenti

Gli agenti ti scrivono nella **Posta degli agenti**. Lì trovi:

| Sezione | Che cosa c'è |
|---|---|
| Giri | i lavori in corso di un workflow, passo per passo |
| Da fare | ciò che tocca a te: un lavoro «Da approvare», un passo «Da avviare», una decisione da prendere |
| Messaggi | i messaggi degli agenti, con le risposte che puoi dare |

Quando un agente consegna, il suo lavoro resta nella sua copia del progetto finché non lo autorizzi. Con «Autorizza» il
lavoro entra nel progetto; con «Rifiuta» scrivi il motivo e il lavoro si ferma finché non premi «Fai ripartire».

## Giri e registro dei giri

Un **workflow** descrive un lavoro che passa di mano: quali passi ci sono, chi li prepara, chi approva. Ogni volta che il
lavoro comincia nasce un **giro**: il giro di oggi della gara, il giro di questa settimana della revisione.

Il **registro dei giri** dice, per ogni giro, che cosa è successo: quali passi sono partiti, chi ha consegnato, chi ha
approvato, quale esito ha scelto una persona. Vive in git, in un ramo a parte (`mde/giri`), così non si mescola con il
lavoro del gruppo e viaggia con il repository. È la prova di chi ha fatto che cosa, e quando.

## Che cosa fa MdExplorer

MdExplorer fa da regista, con regole fisse:

- quando un passo finisce, fa partire quelli che vengono dopo, o li mette nella posta di chi li deve avviare;
- scrive l'incarico di ogni agente: che cosa deve fare, che cosa riceve, che cosa deve consegnare;
- chiede alle persone le loro decisioni, con i pulsanti degli esiti;
- scrive tutto nel registro dei giri.

Gli agenti fanno solo il loro lavoro e scrivono alla persona: non decidono mai chi viene dopo. Quello lo dice il workflow.

Il capitolo successivo spiega il documento che tiene insieme persone, ruoli e agenti:
[Ruoli e responsabilità](02-ruoli-e-responsabilita.md).
