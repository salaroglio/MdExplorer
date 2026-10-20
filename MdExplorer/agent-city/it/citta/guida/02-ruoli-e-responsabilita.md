---
title: Ruoli e responsabilità
---

# Ruoli e responsabilità

## TL;DR

Il documento delle responsabilità (`responsabilita.md`) dice chi risponde di quale ruolo e di quali agenti. I workflow
parlano di ruoli, mai di persone: è questo documento a collegare il ruolo alla persona vera, riconosciuta dalla sua email
git.

- Una riga per persona e ruolo: Ruolo, Responsabile, Git Email, Agenti.
- Un ruolo senza persona è un ruolo aperto: i suoi agenti non partono finché qualcuno non lo prende.
- Più persone con lo stesso ruolo sono un team: chi fa partire il lavoro sceglie chi lo smaltisce.

## Il documento

È un normale file markdown del progetto, con un'intestazione che lo dichiara:

```markdown
---
mde_type: ownership
---
```

e una sola tabella. MdExplorer lo crea quando la città arriva nel progetto, e lo nomina nelle impostazioni (Impostazioni
del progetto → «Città degli agenti / Federazione» → «Documento di ownership»). Un progetto ne ha uno solo.

## Le colonne

| Colonna | Che cosa contiene | Obbligatoria |
|---|---|---|
| Ruolo | il nome del ruolo, lo stesso che usano i workflow | sì |
| Responsabile | il nome della persona, come lo leggete voi | no |
| Git Email | l'email con cui la persona fa i commit nel progetto | sì, per una persona |
| Agenti | i nomi degli agenti di cui risponde in quel ruolo | no |

Può esserci anche una colonna Descrizione, con una riga che spiega il ruolo.

## La persona è la sua email git

MdExplorer riconosce le persone dall'email git con cui lavorano nel progetto, non dal nome. Per questo la colonna Git
Email conta più del nome: deve essere esattamente l'email dei tuoi commit, e la persona deve essere un partecipante del
progetto.

## I ruoli aperti

Quando un metodo o la città portano un ruolo nuovo, MdExplorer lo aggiunge come **ruolo aperto**: una riga con il ruolo
e i suoi agenti, ma senza nome né email. Gli agenti di un ruolo aperto non partono: nel pannello «Città degli agenti»
compaiono come «Senza responsabile: non parte».

Per prendere un ruolo:

1. apri il pannello «Città degli agenti»;
2. premi «È mio» sull'agente (o «Sono tutti miei» per prenderli tutti): MdExplorer scrive la tua riga nel documento;
3. in alternativa scrivi tu nome ed email nella riga, e committa.

## I team

Lo stesso ruolo su più righe, con persone diverse, è un **team**: per esempio due responsabili tecnici che si dividono le
gare. Quando un passo tocca a un ruolo con più persone:

- chi preme il pulsante che fa partire il passo sceglie la persona;
- se nessuno l'ha scelta, va a chi del team ha già lavorato sui passi da cui viene;
- altrimenti MdExplorer chiede. Non sceglie mai a caso.

Chi ha un passo «Da avviare» può anche passarlo a un collega dello stesso ruolo, con «Passa a un collega» e una riga di
spiegazione.

## Un ruolo che decide e basta

Non tutti i ruoli hanno agenti. Una direzione che decide se partecipare a una gara, o che approva un rapporto, è una riga
con la colonna Agenti vuota: il workflow le manda la decisione con i suoi pulsanti.

## Che cosa non è

Il documento delle responsabilità non è un permesso: dice a MdExplorer a chi indirizzare il lavoro. La garanzia resta
sempre la persona, che approva dal proprio computer. Non scriverci password o token: il file viaggia in git con tutto il
progetto.

Adesso puoi provare: [Il primo giro](03-il-primo-giro.md).
