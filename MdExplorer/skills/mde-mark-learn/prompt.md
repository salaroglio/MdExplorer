---
description: "Prompt del pulsante «Impara da questa attività» di MarkAgent: rilegge l'attività appena conclusa e propone skill nuove o correzioni a quelle del progetto, con le prove tratte dalla chat."
mde:
  origin: mdexplorer
  version: 1
  updatePolicy: replace
---

<!--
MdExplorer-managed prompt, letto SOLO dal servizio (risorsa incorporata): il blocco della risposta lo legge il codice
(sprint 2026-11-24-Skill-Che-Migliorano). Segnaposto: {{harness}}, {{skills}}, {{rejected}}, {{activity}}, {{thread}}, {{from}}, {{to}}.
-->

[Istruzione di MdExplorer] L'utente ha premuto «Impara da questa attività». Rileggi l'attività appena conclusa e proponi le skill che renderebbero **la prossima attività dello stesso tipo** più veloce o più corretta: skill nuove, o correzioni a quelle che il progetto ha già. Non rifare l'attività e non modificare file: qui si propone soltanto, l'utente approva una proposta alla volta e MdExplorer scrive.

## Che cosa cercare nell'attività

- **Correzioni dell'utente**: «no, così», «non quello», un'istruzione ripetuta. Sono le prove più forti: la prossima volta l'agente deve saperlo prima.
- **Errori risolti dopo vari tentativi**: un comando sbagliato, un'opzione che non esiste, un percorso che non c'era, e la strada che alla fine ha funzionato.
- **Procedure ripetute**: gli stessi passi fatti in un ordine preciso (comandi, controlli, file da toccare insieme).
- **Convenzioni del progetto scoperte strada facendo**: nomi, cartelle, formati, regole che non stanno scritte da nessuna parte.

## Che cosa NON diventa una skill

- Lo stato del momento (che cosa è stato fatto oggi, a che punto è un lavoro): sta nella chat e nei commit, non in una skill.
- Quello che il codice o la documentazione del progetto dicono già, e quello che le skill `mde-*` dicono già.
- Una cosa successa una volta sola e senza un motivo per ripetersi.
- Dati sensibili: password, chiavi, nomi di persone o di clienti, indirizzi interni. Mai in una skill: finisce nel repository.
- Una proposta uguale a una già rifiutata (elenco sotto), a meno che l'attività non porti una prova nuova: dilla in `why`.

Meglio **nessuna proposta** che una proposta debole: se l'attività non insegna niente di durevole, rispondi con `{"proposals": []}` e dillo in una frase.

## Come scrivere una skill

- Una skill per argomento, con un nome breve in minuscolo con i trattini (per esempio `rilascio-pacchetto-linux`). **Mai** un nome che comincia con `mde-`: quelli sono di MdExplorer e verrebbero sovrascritti.
- Il testo completo del file `SKILL.md`, frontmatter compreso:

```yaml
---
name: <nome>
description: <che cosa fa, in una frase>. Use when: <parole e situazioni che devono farla scegliere>.
---
```

  Niente blocco `mde:` (lo aggiunge MdExplorer quando scrive). Poi istruzioni operative, brevi e verificabili, nella lingua dell'utente: passi, comandi esatti, trappole note con il modo di evitarle.
- Per **correggere** una skill esistente, riscrivi il file intero cambiando solo quello che l'attività ha insegnato; tutto il resto resta com'è.

## Le skill del progetto (cartella del motore `{{harness}}`)

{{skills}}

## Proposte rifiutate in passato su questo progetto

{{rejected}}

## L'attività (chat `{{thread}}`, eventi da {{from}} a {{to}})

Qui sotto la vista compatta: domande, risposte, una riga per strumento. Il numero fra parentesi quadre è il progressivo dell'evento. Per leggere un evento per intero (argomenti e risultato completi di uno strumento) usa lo strumento `chat_read` di MdExplorer con la chat e l'intervallo che ti servono.

{{activity}}

## La tua risposta

Prima, in poche frasi, che cosa hai imparato dall'attività. Poi, in fondo, su righe sue e senza niente dopo, questo blocco, che MdExplorer toglie dalla risposta e mostra all'utente come proposte da approvare:

<<<mde-learn
{"proposals": [
  {"action": "create", "skill": "nome-della-skill", "why": "una riga: che cosa evita o accelera la prossima volta", "evidence": [12, 15], "content": "---\nname: …\ndescription: …\n---\n\n…"}
]}
mde-learn>>>

- `action`: `create` per una skill nuova, `update` per una che c'è già (con lo stesso `skill` dell'elenco).
- `evidence`: i progressivi degli eventi da cui nasce la proposta; almeno uno.
- `content`: il testo intero di `SKILL.md`, con gli a capo scritti come `\n`.
- In questa risposta **non** aggiungere il blocco `mde-thread` delle altre risposte: qui c'è solo `mde-learn`.
