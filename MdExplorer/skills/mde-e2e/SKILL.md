---
name: mde-e2e
description: "Scrive ed esegue test end-to-end di siti web descritti in markdown (file *.e2e.md) con MdExplorer, e ne registra esiti, screenshot e script Playwright rigiocabili. Use when: test e2e, end-to-end, test di un sito, test dell'interfaccia web, collaudo, verificare che il sito funzioni, file .e2e.md, eseguire i test, rilanciare i test, esito dei test, mappa del sito, credenziali di test, script Playwright, regressione, smoke test, login di prova."
mde:
  origin: mdexplorer
  version: 3
  updatePolicy: replace
---

<!--
MdExplorer-managed skill.
The `mde:` block above marks this file as distributed by MdExplorer. When you
open a project, MdExplorer compares the embedded version with what is on disk
and will overwrite this file to keep it in sync with the current MdExplorer
features. To customize the skill while keeping your edits, remove the `mde:`
block (or change `origin` to something else) — MdExplorer will then leave the
file alone.
-->

# Test end-to-end in MdExplorer

Un test end-to-end è **un file markdown** che finisce in `.e2e.md`. Descrive, in italiano semplice, cosa fa un
utente su un sito e cosa deve vedere. Tu lo esegui su un browser vero con gli strumenti del server MCP
**playwright** (`browser_navigate`, `browser_snapshot`, `browser_click`, `browser_type`, `browser_fill_form`,
`browser_take_screenshot`, `browser_console_messages`, `browser_network_requests`, …), poi registri l'esito nel
file stesso.

Ogni esecuzione lascia tre cose, tutte collegate dal `.e2e.md`:

- una riga di **esito** per ogni test;
- gli **screenshot** e un **report** dell'esecuzione;
- uno **script Playwright C#** per ogni test, che si rigioca senza di te in pochi secondi.

Lo script va molto più veloce di te: dove tu, tra un passo e l'altro, vedi la pagina finire di caricare, lo
script prosegue subito. Per questo ogni passo che fa caricare qualcosa ha nello script un'**attesa** esplicita,
e ciò che scopri su come il sito carica resta scritto nelle **schede di pagina** (*«Le attese»*).

## I file

```text
test-e2e/                              ← la cartella dei test (il nome è libero)
├── mappa-sito-the-internet.md         ← mappa del sito: una per sito, condivisa dai test
├── mappa-sito-the-internet/           ← le schede di pagina: stesso nome della mappa, senza .md
│   └── login.md                       ← una scheda per pagina
├── credenziali-the-internet.txt       ← credenziali: una per sito, MAI in git
├── login.e2e.md                       ← un file di test
├── login.e2e/                         ← i suoi artefatti: stesso nome del test, senza .md
│   ├── scripts/
│   │   ├── login.T1.spec.cs
│   │   └── login.T2.spec.cs
│   └── esecuzioni/
│       └── 2026-09-27_10-30/          ← una cartella per esecuzione
│           ├── report.md
│           ├── T1-03-messaggio-login.png
│           └── T2-03-errore-password.png
├── E2eTests.csproj                    ← progetto che compila gli script (uno per cartella di test)
├── E2eSupport.cs                      ← supporto comune agli script
└── e2e.runsettings                    ← browser usato per rigiocare gli script
```

Se mancano `E2eTests.csproj`, `E2eSupport.cs` o `e2e.runsettings`, creali **esattamente** con il contenuto della
sezione *«I file di supporto»*. Se esistono, non toccarli, con un'eccezione: se `E2eSupport.cs` non ha la riga
`// versione-supporto: 2` della sezione, sostituiscilo con quello della sezione (gli script nuovi usano funzioni
che il vecchio non ha). **Uno solo per albero di test**: se in una cartella
sopra quella del test c'è già un `E2eTests.csproj`, non crearne un altro (compilerebbe due volte gli stessi script).

Nel `.gitignore` del progetto aggiungi, se mancano, le cartelle di compilazione:

```text
test-e2e/bin/
test-e2e/obj/
```

(adatta `test-e2e/` al nome vero della cartella dei test). La riga che esclude il file delle credenziali
(`credenziali-*.txt`) la controlla MdExplorer **prima** di lanciarti: se manca, l'utente la aggiunge dalla finestra
dei test.

**Esegui i test solo se hai gli strumenti del server playwright** (`browser_navigate`, `browser_snapshot`, …). Se
non li hai, non sei stato lanciato da MdExplorer: non eseguire niente con altri mezzi e di' all'utente di usare il
tasto destro sul file → «Test e2e…». Gli strumenti `browser_evaluate` e `browser_run_code_unsafe` non ci sono di
proposito: non cercarli.

## Il file di test

````markdown esempio=test
---
title: Login di the-internet
e2e:
  baseUrl: https://the-internet.herokuapp.com
  siteMap: mappa-sito-the-internet.md
  credentials: credenziali-the-internet.txt
  artifacts: login.e2e/
---
# Login di the-internet

## T1 — Login riuscito
1. Apri `/login`
2. Scrivi {{the-internet.utente}} nel campo "Username"
3. Scrivi {{the-internet.password}} nel campo "Password"
4. Premi "Login"
5. ✔ Compare il testo "You logged into a secure area!"
6. ✔ L'URL contiene "/secure"

## T2 — Password sbagliata
1. Apri `/login`
2. Scrivi {{the-internet.utente}} nel campo "Username"
3. Scrivi "sbagliata" nel campo "Password"
4. Premi "Login"
5. ✔ Compare il testo "Your password is invalid!"
6. ✔ L'URL contiene "/login"

## Artefatti

## Esiti
````

Le regole di scrittura, che valgono sia quando scrivi un test sia quando lo esegui:

| Forma | Significato |
|---|---|
| `## T<n> — <titolo>` | un test. I numeri non si riusano: un test tolto lascia il buco |
| riga numerata | un passo, eseguito in ordine |
| riga che inizia con **✔** | una **verifica**: se non torna, il test fallisce |
| `` `/percorso` `` | un indirizzo relativo a `baseUrl` |
| `"testo"` | testo **letterale**: da scrivere, da cercare o da confrontare così com'è |
| `{{chiave}}` | un valore del file delle credenziali. Non compare mai scritto altrove |

Le verifiche si scrivono in una di queste forme, così diventano asserzioni precise nello script:

| Verifica | Asserzione nello script |
|---|---|
| ✔ Compare il testo "…" | `Expect(Page.GetByText("…").Filter(new() { Visible = true }).First).ToBeVisibleAsync()` |
| ✔ Non compare il testo "…" | `Expect(Page.GetByText("…")).ToBeHiddenAsync()` |
| ✔ L'URL contiene "…" | `Expect(Page).ToHaveURLAsync(new Regex(Regex.Escape("…")))` |
| ✔ Il titolo della pagina è "…" | `Expect(Page).ToHaveTitleAsync("…")` |
| ✔ Il campo "Etichetta" vale "…" | `Expect(Page.GetByLabel("Etichetta")).ToHaveValueAsync("…")` |
| ✔ Il bottone "…" è disabilitato | `Expect(Page.GetByRole(AriaRole.Button, new() { Name = "…" })).ToBeDisabledAsync()` |
| ✔ Ci sono esattamente N bottoni "…" | `Expect(Page.GetByRole(AriaRole.Button, new() { Name = "…" })).ToHaveCountAsync(N)` |
| ✔ Ci sono esattamente N link "…" | `Expect(Page.GetByRole(AriaRole.Link, new() { Name = "…" })).ToHaveCountAsync(N)` |

«Compare il testo» vuol dire: **almeno un** elemento **visibile** contiene quel testo. Lo stesso testo si trova
spesso in più punti, anche nascosti (le opzioni di una select, le viste non attive di una pagina a linguette):
senza `.Filter(new() { Visible = true })` il primo trovato può essere nascosto e la verifica fallisce; senza
`.First` Playwright si ferma perché non sa quale scegliere.

Ogni test deve avere **almeno una verifica ✔**: senza, lo script rigiocato risulta sempre superato, anche quando
la pagina fa tutt'altro. Se esegui un test senza verifiche, scrivilo nei dettagli dell'esito
(«⚠️ nessuna verifica ✔: il rigioco non può accorgersi di un errore»).

Una verifica scritta in altro modo («✔ La pagina sembra in ordine») è una **verifica a giudizio**: la valuti tu
guardando la pagina, e nello script diventa un commento `// verifica a giudizio: …` senza asserzione.

Nel front matter `siteMap` e `baseUrl` sono obbligatori; `artifacts` se manca vale il nome del test senza `.md`
(`login.e2e.md` → `login.e2e/`); `credentials` serve solo se il test usa delle `{{chiavi}}`.

Quando l'utente ti chiede di scrivere un test, usa queste forme. Se una frase è ambigua (quale bottone? quale
testo esatto?) guarda la mappa del sito e il codice sorgente del sito, se è nel progetto; se non basta, chiedi.
In chat non hai il browser: gli strumenti playwright ci sono solo quando MdExplorer ti lancia per eseguire.

## Preparare i test di un sito (prima configurazione)

Quando l'utente ti chiede di preparare i test di un sito («configura i test e2e», «preparami un test del login di
http://localhost:4200»):

1. **Indirizzo e cartella.** `baseUrl` è l'indirizzo del sito, senza percorso finale; la cartella dei test è quella
   che dice l'utente, altrimenti `test-e2e/` nella radice del progetto. Nomi dei file: `mappa-sito-<sito>.md`,
   `credenziali-<sito>.txt`, `<argomento>.e2e.md`, con `<sito>` breve e minuscolo (es. `miosito`).
2. **Testi esatti, mai inventati.** Etichette dei campi, testi dei bottoni, messaggi e percorsi vanno scritti
   **come il sito li mostra**. Se il codice del sito è nel progetto, prendili da lì: route per i percorsi, template
   e componenti per etichette e bottoni, file di traduzione (nella lingua che il sito mostra) per i testi. Se non
   c'è, o un testo non si trova, chiedilo all'utente; se lui preferisce non rispondere, scrivi il testo più
   probabile ed **elencalo** alla fine tra quelli da confermare.
3. **Il file di test**, nelle forme di *«Il file di test»*: front matter completo, un `## T<n>` per ogni caso
   (almeno il caso che riesce e uno che fallisce, se ha senso), verifiche con **✔** nelle forme della tabella,
   `## Artefatti` e `## Esiti` vuote.
4. **La mappa del sito**: se hai il codice sorgente, puoi crearne una prima versione (pagine dalle route, campi e
   bottoni dai template) con le regole di *«La mappa del sito»*; altrimenti la crei al primo giro. Se il sito è
   un'**applicazione a pagina singola** (un solo indirizzo, le «pagine» sono viste, linguette, pannelli), **chiedi
   all'utente** quali sono le pagine, proponendo quelle che vedi: ogni pagina avrà la sua scheda
   (*«Le schede di pagina»*). Dal sorgente puoi anche anticipare nelle schede quali chiamate fa ogni azione,
   scritte come *«dal sorgente, da confermare»* finché un'esecuzione non le osserva.
5. **Le credenziali, senza valori.** Scegli le chiavi (`<sito>.utente`, `<sito>.password`, …). Prima aggiungi al
   `.gitignore` del progetto la riga `credenziali-*.txt`, se manca. Poi, **solo se il file non esiste**, crealo
   con le chiavi e i valori vuoti:

   ```text
   # credenziali per i test di miosito: compila i valori (solo account di prova)
   miosito.utente=
   miosito.password=
   ```

   Se esiste già non aprirlo: di' all'utente quali chiavi deve contenere. **Non chiedere mai i valori** e, se
   l'utente te ne scrive uno in chat, non ricopiarlo da nessuna parte: digli di scriverlo lui nel file. Finché un
   valore è vuoto MdExplorer non lancia i test e lo dice.
6. **Non creare** `E2eTests.csproj`, `E2eSupport.cs` ed `e2e.runsettings`: nascono alla prima esecuzione.
7. **Alla fine** di' all'utente, in poche righe: i file creati; i valori da scrivere nel file delle credenziali; i
   testi da confermare; come si lancia (tasto destro sul file o sulla cartella → «Test e2e…»).

## La mappa del sito

La mappa del sito è la memoria di ciò che si sa del sito: la leggi **prima** di eseguire, la aggiorni **dopo**
(se il file non esiste ancora, crealo tu al primo giro, nel percorso indicato da `siteMap`).
Ci va solo ciò che serve a eseguire e scrivere test futuri, e che resta vero nel tempo:

- per ogni **pagina**: indirizzo, titolo, a cosa serve;
- gli **elementi** con cui si interagisce, con **ruolo e nome accessibile** come li mostra `browser_snapshot`
  (`textbox "Username"`, `button "Login"`, `link "Logout"`): sono i nomi che usano gli script;
- i **messaggi** che il sito mostra e quando (conferme, errori);
- la **navigazione**: dove porta un'azione (redirect, pagine intermedie);
- le **trappole**: nomi accessibili strani (un'icona nel nome del bottone), attese necessarie, errori di console
  che non sono difetti.

Non ci va: valori delle credenziali, dati che cambiano a ogni visita (orari, numeri d'ordine), l'esito dei test.

````markdown esempio=mappa
# Mappa del sito the-internet

Indirizzo: `https://the-internet.herokuapp.com`. Ultimo aggiornamento: 2026-09-27, da `login.e2e.md`.

## Elementi comuni
- Messaggi: un riquadro in cima alla pagina, senza ruolo ARIA; si trova per testo.
- La console riporta errori di risorse esterne a ogni caricamento: non sono difetti del sito.

## `/login` — Login Page
- textbox "Username", textbox "Password"
- button "Login" (il nome accessibile comincia con un'icona: cercarlo per nome parziale)
- Credenziali giuste → `/secure`, testo "You logged into a secure area!"
- Password sbagliata → resta su `/login`, testo "Your password is invalid!"

## `/secure` — Secure Area
- link "Logout" → `/login`, testo "You logged out of the secure area!"
````

Aggiorna la mappa aggiungendo o correggendo, non riscrivendola da capo. Ogni fatto va sotto la **pagina** a cui
appartiene; in «Elementi comuni» solo ciò che vale per tutte le pagine. Se una cosa scritta non è più vera,
correggila e scrivi accanto la data. Per ogni pagina che ha una scheda, la mappa ha il link alla scheda.

## Le schede di pagina

Accanto alla mappa c'è una cartella con lo stesso nome (senza `.md`) e dentro **una scheda per pagina**: dice
**come la pagina carica** e quindi cosa deve aspettare uno script. La leggi prima di eseguire un test che passa
da quella pagina, la aggiorni dopo. In un'applicazione a pagina singola la «pagina» è la vista che l'utente
percepisce come un posto diverso, e quali siano le pagine lo decide l'utente.

````markdown esempio=scheda
# Login

Si apre da: `/login`. Ultimo aggiornamento: 2026-09-28, da `login.e2e.md`.

## Azioni e chiamate

### Premi "Login"
- Attesa: navigazione a `/secure` (il modulo si invia con un POST e la pagina cambia).
- Osservato: `POST /authenticate` → 303 verso `/secure`, 180 ms, 1,2 KB.
- Tempo massimo: 10 s.
- Rumore da ignorare: nessuno.
- Fine del lavoro: compare `link "Logout"`.

## Da tenere d'occhio
- Il bottone "Login" ha un'icona nel nome accessibile: cercarlo per nome parziale.

## Storia dei problemi
- 2026-09-28: nessun problema.
````

Le regole della scheda:

- **Solo ciò che hai osservato**, con il dato che lo prova: durata e dimensione lette da `browser_network_request`,
  l'elemento visto nello snapshot, la riga della console. Un'ipotesi (perché è lenta, cosa fa il server) si
  scrive come tale: «ipotesi: …». Una scheda sbagliata fa sbagliare tutti gli script che verranno.
- Per ogni azione che carica: cosa aspettare (segnale, chiamata, navigazione), a cosa serve, dimensione e durata
  osservate, tempo massimo, rumore da ignorare, fine del lavoro.
- **Storia dei problemi**: quando un rigioco fallisce e se ne capisce il perché, una riga con la data, cosa è
  successo e cosa è cambiato nella scheda. Non cancellare le righe vecchie.
- Aggiorna aggiungendo o correggendo, non riscrivendo da capo; una cosa non più vera si corregge con la data.

## Le credenziali

Un file di testo con una riga `chiave=valore` per ogni credenziale; le righe che iniziano con `#` sono commenti.

```text esempio=credenziali
# credenziali per i test di the-internet
the-internet.utente=utente-di-esempio
the-internet.password=password-di-esempio
```

**Non leggere e non cercare il file delle credenziali.** È escluso da git, quindi le ricerche (`Glob`, `Grep`)
possono non vederlo: non vuol dire che manchi, e durante un'esecuzione non devi crearlo (in chat, per la prima
configurazione, vedi *«Preparare i test di un sito»*). MdExplorer ha già controllato, prima di
lanciarti, che esista e che contenga ogni chiave usata dal test. Il server playwright lo riceve da MdExplorer e
fa lui la sostituzione:
per un passo come «Scrivi {{the-internet.password}} nel campo "Password"», chiama `browser_type` con il testo
`the-internet.password` (il nome della chiave, **senza** graffe). Il server scrive nel campo il valore vero, e
nelle sue risposte ogni valore segreto compare come `<secret>the-internet.password</secret>`: tu non vedi mai i
valori, ed è voluto.

Nel `.e2e.md`, nella mappa, nel report e negli script si scrive solo la chiave (`{{the-internet.password}}`).
Se una chiave sembra non funzionare (il login fallisce, il campo contiene il nome della chiave), non provare
altri nomi e non chiedere il valore: dai l'esito ⚠️ e scrivi quale chiave hai usato.

## Eseguire i test

Quando MdExplorer ti lancia, nessuno risponde alle tue domande: se un passo è ambiguo non chiedere, dai l'esito
⚠️ e scrivi nel report cosa non era chiaro.

I percorsi: la cartella dell'esecuzione che MdExplorer ti dà è relativa **al progetto** (usala così per gli
screenshot); i link che scrivi nel `.e2e.md` sono relativi **al file di test**, quelli nel `report.md` relativi
**al report**.

1. Leggi il `.e2e.md` e la mappa del sito indicata nel front matter (il file delle credenziali no: vedi
   *«Le credenziali»*).
2. La **cartella dell'esecuzione** è `<artifacts>/esecuzioni/<AAAA-MM-GG_hh-mm>/`: te la indica chi lancia i
   test; se nessuno la indica, usa data e ora correnti. Creala se non c'è.
3. Per ogni test, in ordine, esegui i passi con gli strumenti playwright. Prima di cliccare o scrivere, guarda
   la pagina con `browser_snapshot` e scegli l'elemento per **ruolo e nome**. Prima di un test leggi le schede
   delle pagine che attraversa; dopo ogni passo che può far caricare qualcosa (apertura, selezione, clic che
   cambia vista o dati) scopri **cosa aspettare**, come dice *«Le attese»*.
4. Dopo ogni verifica ✔ fai uno screenshot con `browser_take_screenshot`, nome
   `T<n>-<nn>-<descrizione-breve>.png` (`<nn>` = numero del passo, a due cifre), nella cartella dell'esecuzione.
5. Confronta sempre con il **testo atteso scritto nel test**, mai con quello che ti aspetteresti tu. Se il sito
   mostra altro, la verifica fallisce: non correggere il test per farlo passare.
6. Dai a ogni test uno di questi tre esiti:

| Esito | Quando |
|---|---|
| ✅ superato | tutti i passi eseguiti, tutte le verifiche tornano |
| ❌ l'applicazione sbaglia | i passi sono stati eseguiti, ma una verifica non torna: è un **difetto del sito** |
| ⚠️ esecuzione non riuscita | non sei arrivato in fondo ai passi (elemento non trovato, pagina che non carica, credenziale mancante): **non** si sa se il sito sia giusto |

Un test ❌ o ⚠️ non ferma gli altri: passa al test successivo.

7. Chiudi il browser con `browser_close` alla fine.

## Le attese

Tre livelli, dal più sicuro. Per ogni passo che carica usa il primo che il sito permette.

1. **Il segnale del sito.** Alcuni siti annunciano da soli quando hanno finito (convenzione dei segnali di
   MdExplorer). Dopo il passo leggi `browser_console_messages`: righe come `[mde] start grafo cob:bs522` e poi
   `[mde] ready grafo cob:bs522` (o `[mde] error …`) sono il segnale `grafo` con chiave `cob:bs522`. Nello script:
   `await E2e.EnableSignals(Context);` prima di aprire il sito, e dopo l'azione
   `await E2e.Signal(Page, "grafo", "cob:bs522", TimeSpan.FromSeconds(<tempo massimo>));`.
2. **La chiamata e la fine del lavoro.** Senza segnale, leggi `browser_network_requests`: le chiamate sono
   numerate, quelle del passo sono quelle con il numero più alto dell'ultimo che avevi visto. Distingui la
   chiamata che porta i dati del passo dal **rumore** (chiamate periodiche, telemetria, pubblicità): il nome
   dell'indirizzo, il momento e il senso dell'azione ti dicono qual è. Leggi durata e dimensione con
   `browser_network_request` passando **solo il numero**, senza `part`: la durata sta nella sezione «General»
   (`duration`), la dimensione in `content-length`; con `part` la sezione «General» non c'è. Poi confronta lo snapshot di prima e di dopo e trova la **fine del lavoro**: qualcosa
   che esiste solo quando la pagina ha finito di usare quei dati (un elemento nuovo, un nome che cambia: un filtro
   che prima era "Tabella DB2" e dopo "Tabella DB2 (12)"). Arrivata la risposta, la pagina può lavorare ancora a
   lungo: la sola chiamata non basta. Nello script, l'attesa della chiamata si registra **prima** dell'azione:

   ```csharp
   await Page.RunAndWaitForResponseAsync(
       () => Page.Locator("#program-selector").SelectOptionAsync("cob:bs522"),
       r => r.Url.Contains("/api/graph/") && r.Ok, new() { Timeout = 15_000 });
   await Expect(Page.GetByRole(AriaRole.Checkbox, new() { NameRegex = new Regex(@"^Tabella DB2 \(\d+\)$") }))
       .ToBeVisibleAsync(new() { Timeout = 15_000 });
   ```
3. **Niente da sorvegliare.** Il passo non fa chiamate e non ha segnale, ma la pagina cambia (un calcolo solo nel
   browser, una vista già caricata): aspetta la fine del lavoro osservata, come al livello 2. Se non trovi
   nemmeno quella, scrivilo nel commento dell'attesa: `// attesa: nessuna trovata`.

Il **tempo massimo** lo stimi da durata e dimensione osservate, con ampio margine (il PC di chi rigioca può
essere più lento, i dati possono crescere): l'attesa finisce al primo che arriva tra l'evento e il tempo massimo.
Se un segnale non arriva entro il tempo massimo, il test fallisce (`E2e.Signal` lo fa da solo). **Mai pause
fisse**: `Task.Delay`, `WaitForTimeoutAsync` e simili non si usano.

Quello che hai scoperto va nella scheda della pagina (*«Le schede di pagina»*), così il prossimo test lo sa già.

## Registrare l'esito

**Il report dell'esecuzione**: `report.md` nella cartella dell'esecuzione.

````markdown esempio=report
# Esecuzione del 2026-09-27 10:30 — login.e2e.md

## T1 — Login riuscito — ✅ superato
1. ✅ Apri `/login`
2. ✅ Scrivi {{the-internet.utente}} nel campo "Username"
3. ✅ Scrivi {{the-internet.password}} nel campo "Password"
4. ✅ Premi "Login"
5. ✅ Compare il testo "You logged into a secure area!"

   ![T1-05](T1-05-messaggio-login.png)
6. ✅ L'URL contiene "/secure"

## T2 — Password sbagliata — ❌ l'applicazione sbaglia
1. ✅ Apri `/login`
2. ✅ Scrivi {{the-internet.utente}} nel campo "Username"
3. ✅ Scrivi "sbagliata" nel campo "Password"
4. ✅ Premi "Login"
5. ❌ Compare il testo "Your password is invalid!"
   - atteso: "Your password is invalid!"
   - trovato: "Your username is invalid!"

   ![T2-05](T2-05-errore-password.png)
6. ⏭️ non eseguito
````

**Nel `.e2e.md`**, e solo nelle sezioni `## Artefatti` ed `## Esiti` (il resto del file non si tocca):

- in `## Artefatti`: un link a ogni script e uno all'ultima esecuzione. Aggiorna i link, non accumularli;
- in `## Esiti`: una tabella, **la più recente in alto**. Non cancellare le righe vecchie.

````markdown esempio=esiti
## Artefatti
- Script: [T1](login.e2e/scripts/login.T1.spec.cs), [T2](login.e2e/scripts/login.T2.spec.cs)
- Ultima esecuzione: [2026-09-27 10:30](login.e2e/esecuzioni/2026-09-27_10-30/report.md)

## Esiti

| Data | Test | Esito | Dettagli |
|---|---|---|---|
| 2026-09-27 10:30 | T2 | ❌ l'applicazione sbaglia | passo 5: trovato "Your username is invalid!" — [report](login.e2e/esecuzioni/2026-09-27_10-30/report.md) |
| 2026-09-27 10:30 | T1 | ✅ superato | [report](login.e2e/esecuzioni/2026-09-27_10-30/report.md) |
````

Poi aggiorna la mappa del sito con ciò che hai imparato.

## Lo script di ogni test

Dopo l'esecuzione scrivi uno script per **ogni** test, anche se è fallito, in
`<artifacts>/scripts/<nome>.T<n>.spec.cs`. Il server playwright ti restituisce, per ogni azione, il blocco
`Ran Playwright code` in C#: usalo per i passi, e **copia il localizzatore esattamente com'è**. Non
accorciarlo e non riscriverlo a memoria: il server lo sceglie perché trovi un solo elemento, anche contando quelli
nascosti (una linguetta con lo stesso testo in un'altra vista); un localizzatore accorciato ne trova più di uno e
il rigioco si ferma. Aggiungi solo attese, credenziali e screenshot.

Le regole, tutte obbligatorie:

- **segui lo scheletro qui sotto alla lettera**: stessi `using`, stesso namespace `MdeE2e.<NomeFile>` (il nome del file di test senza `.e2e.md`, in PascalCase, senza trattini né
  punti: `login-admin.e2e.md` → `MdeE2e.LoginAdmin`), una classe
  `Test_T<n>` per test che eredita `PageTest`, un solo metodo `T<n>_<Titolo>` (così l'esito di
  `dotnet test` dice quale test è);
- le credenziali si leggono **solo** con `E2e.Credential(Credentials, "<chiave>")`, mai scritte nello script:
  il codice che il server restituisce per quei passi usa `Environment.GetEnvironmentVariable("<chiave>")`,
  sostituiscilo;
- gli screenshot si fanno **solo** con `E2e.Screenshot(Page, Artifacts, "<nome>")`, mai con percorsi scritti a mano;
- ogni passo e ogni verifica hanno sopra un commento con la riga del test, numero compreso;
- ogni passo che carica ha la sua **attesa** (*«Le attese»*), con sopra un commento
  `// attesa: segnale <nome>`, `// attesa: <METODO> <indirizzo> + fine lavoro <cosa>`,
  `// attesa: fine lavoro <cosa>` oppure `// attesa: nessuna trovata`, e la scheda da cui viene; mai pause fisse;
- ogni passo dello script deve fare **quello che dice il test**. Se non riesci a tradurlo fedelmente (un clic su un
  disegno, un trascinamento, un'azione che hai fatto in un altro modo), lo script è `incompleto` e al posto del
  passo c'è `Assert.Fail("passo <n> non riproducibile: <motivo>")`;
- le asserzioni usano il **testo atteso del test** (tabella delle verifiche), mai quello osservato;
- l'intestazione dice lo stato: `valido` (test ✅), `riproduce un difetto` (test ❌: l'asserzione che fallisce
  resta quella giusta), `incompleto` (test ⚠️, oppure un passo non riproducibile: scrivi i passi fino a dove sei
  arrivato e un `Assert.Fail("esecuzione non riuscita al passo <n>: <motivo>")`);
- il campo `impronta-sorgente` lo calcola MdExplorer: scrivi `da calcolare`.

```csharp esempio=script
// stato: valido
// sorgente: login.e2e.md, T1 — Login riuscito
// impronta-sorgente: da calcolare
// generatore: mde-e2e v2
// data: 2026-09-28 10:30
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using MdeE2e;

namespace MdeE2e.Login
{
    [TestFixture]
    public class Test_T1 : PageTest
    {
        private const string BaseUrl = "https://the-internet.herokuapp.com";
        private const string Artifacts = "login.e2e";
        private const string Credentials = "credenziali-the-internet.txt";

        [Test]
        public async Task T1_LoginRiuscito()
        {
            // 1. Apri `/login`
            await Page.GotoAsync(BaseUrl + "/login");

            // 2. Scrivi {{the-internet.utente}} nel campo "Username"
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Username" }).FillAsync(E2e.Credential(Credentials, "the-internet.utente"));

            // 3. Scrivi {{the-internet.password}} nel campo "Password"
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync(E2e.Credential(Credentials, "the-internet.password"));

            // 4. Premi "Login"
            // attesa: navigazione a /secure + fine lavoro link "Logout" (scheda mappa-sito-the-internet/login.md)
            await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();
            await Page.WaitForURLAsync(new Regex(Regex.Escape("/secure")), new() { Timeout = 10_000 });
            await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Logout" })).ToBeVisibleAsync(new() { Timeout = 10_000 });

            // 5. ✔ Compare il testo "You logged into a secure area!"
            await Expect(Page.GetByText("You logged into a secure area!").Filter(new() { Visible = true }).First).ToBeVisibleAsync();
            await E2e.Screenshot(Page, Artifacts, "T1-05-messaggio-login");

            // 6. ✔ L'URL contiene "/secure"
            await Expect(Page).ToHaveURLAsync(new Regex(Regex.Escape("/secure")));
            await E2e.Screenshot(Page, Artifacts, "T1-06-url-secure");
        }
    }
}
```

## I file di supporto

`E2eTests.csproj`:

```xml esempio=csproj
<Project Sdk="Microsoft.NET.Sdk">
  <!-- MdExplorer, skill mde-e2e: progetto che compila e rigioca gli script dei test *.e2e.md di questa cartella. -->
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>disable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
    <PackageReference Include="NUnit" Version="4.2.2" />
    <PackageReference Include="NUnit3TestAdapter" Version="4.6.0" />
    <PackageReference Include="Microsoft.Playwright.NUnit" Version="1.63.0" />
  </ItemGroup>
</Project>
```

`e2e.runsettings` (usa il Chrome installato; per Edge scrivi `msedge`):

```xml esempio=runsettings
<RunSettings>
  <Playwright>
    <BrowserName>chromium</BrowserName>
    <LaunchOptions>
      <Channel>chrome</Channel>
      <Headless>true</Headless>
    </LaunchOptions>
  </Playwright>
</RunSettings>
```

`E2eSupport.cs`:

```csharp esempio=supporto
// MdExplorer, skill mde-e2e: supporto comune agli script dei test *.e2e.md. Non modificare.
// versione-supporto: 2
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;

namespace MdeE2e
{
    public static class E2e
    {
        private static readonly string RunStamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");

        /// <summary>Cartella dei test (quella dei file .e2e.md): E2E_ROOT, oppure due livelli sopra lo script.</summary>
        public static string Root([CallerFilePath] string caller = "") =>
            Environment.GetEnvironmentVariable("E2E_ROOT")
            ?? Path.GetFullPath(Path.Combine(Path.GetDirectoryName(caller)!, "..", ".."));

        /// <summary>Valore di una chiave del file delle credenziali (righe chiave=valore, # per i commenti).</summary>
        public static string Credential(string file, string key, [CallerFilePath] string caller = "")
        {
            var path = Path.Combine(Root(caller), file);
            if (!File.Exists(path))
                throw new FileNotFoundException(
                    $"File delle credenziali non trovato: {path}. Crealo con righe chiave=valore, oppure imposta E2E_ROOT sulla cartella dei test.", path);
            foreach (var raw in File.ReadAllLines(path))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;
                var eq = line.IndexOf('=');
                if (eq > 0 && line.Substring(0, eq).Trim() == key) return line.Substring(eq + 1).Trim();
            }
            throw new KeyNotFoundException($"Chiave '{key}' assente nel file delle credenziali {path}.");
        }

        /// <summary>Cartella di questa esecuzione dello script: E2E_RUN_DIR, oppure &lt;artefatti&gt;/esecuzioni/&lt;data&gt;_script.</summary>
        public static string RunDir(string artifacts, [CallerFilePath] string caller = "")
        {
            var dir = Environment.GetEnvironmentVariable("E2E_RUN_DIR")
                ?? Path.Combine(Root(caller), artifacts, "esecuzioni", RunStamp + "_script");
            Directory.CreateDirectory(dir);
            return dir;
        }

        public static Task Screenshot(IPage page, string artifacts, string name, [CallerFilePath] string caller = "") =>
            page.ScreenshotAsync(new PageScreenshotOptions { Path = Path.Combine(RunDir(artifacts, caller), name + ".png") });

        /// <summary>Accende i segnali del sito (chiave mde-e2e in localStorage): prima di aprire il sito.</summary>
        public static Task EnableSignals(IBrowserContext context) =>
            context.AddInitScriptAsync("try { localStorage.setItem('mde-e2e', '1'); } catch (e) { }");

        /// <summary>
        /// Aspetta il segnale <paramref name="name"/> con chiave <paramref name="key"/>: «ready» → prosegue;
        /// «error» → il test fallisce; nessuno dei due entro <paramref name="max"/> → il test fallisce col motivo.
        /// </summary>
        public static async Task Signal(IPage page, string name, string key, TimeSpan max)
        {
            var attribute = "data-mde-" + name;
            var done = page.Locator($"[{attribute}=\"ready:{key}\"], [{attribute}=\"error:{key}\"]").First;
            try
            {
                await done.WaitForAsync(new() { State = WaitForSelectorState.Attached, Timeout = (float)max.TotalMilliseconds });
            }
            catch (TimeoutException)
            {
                Assert.Fail($"segnale {name} {key} non arrivato entro {max.TotalSeconds} s");
            }
            if ((await done.GetAttributeAsync(attribute))?.StartsWith("error:") == true)
                Assert.Fail($"il sito ha segnalato un errore su {name} {key}");
        }
    }
}
```

Gli script si rigiocano dalla finestra dei test di MdExplorer («Rigioca gli script»), che esegue
`dotnet test E2eTests.csproj --no-restore --settings e2e.runsettings`. I pacchetti (circa 230 MB la prima volta) li
scarica MdExplorer solo quando l'utente lo chiede dalla stessa finestra: non lanciare `dotnet restore` né
`dotnet build` tu.

## Cosa non fare

- Non modificare il testo dei test, né per farli passare né per «migliorarli», a meno che l'utente non te lo
  chieda.
- Non scrivere mai il valore di una credenziale fuori dal file delle credenziali.
- Non usare comandi di shell per eseguire i test: il browser si guida solo con gli strumenti playwright.
- Non mettere pause fisse negli script (`Task.Delay`, `WaitForTimeoutAsync`): si aspetta un evento, con un tempo
  massimo.
- Non scrivere nelle schede supposizioni come se fossero fatti.
- Non cancellare esecuzioni, righe di esito o script vecchi: si accumulano.
- Non toccare le righe di `## Esiti` marcate «(script)» né le cartelle `esecuzioni/<data>_script`: le scrive
  MdExplorer quando rigioca gli script senza di te.
- Non modificare la configurazione degli agenti (`.claude/`, `.github/`, `.opencode/`, `.vscode/`, `.md/`,
  `opencode.json`, `.mcp.json`, `CLAUDE.md`, `AGENTS.md`): durante i test la scrittura lì è vietata.
