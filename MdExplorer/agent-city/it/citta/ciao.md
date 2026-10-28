---
mde_type: workflow
title: Ciao mondo
workflow: ciao.workflow.json
---

# Ciao mondo

## TL;DR

Il giro più piccolo della città degli agenti, per vederla funzionare dall'inizio alla fine: chi risponde della città
lancia l'agente d'esempio, l'agente scrive «hello world» in `citta/ciao/ciao-mondo.md`, e la persona approva la consegna.

- Prima: prendi il ruolo «Responsabile della città» nel documento delle responsabilità e dichiara tuo l'agente («È mio»).
- Poi lancia l'agente dalla città degli agenti: la consegna arriva come richiesta da approvare.
- Quando funziona, togli pure questo workflow dalle impostazioni del progetto e scrivi i tuoi.

## Il giro

```plantuml(@workflow, ./ciao.workflow.json)
```

## Chi fa che cosa

| Passo | Chi lavora | Che cosa consegna | Chi approva |
|---|---|---|---|
| Scrivi «hello world» | agente `citta-ciao` | `citta/ciao/ciao-mondo.md` | il responsabile della città |
