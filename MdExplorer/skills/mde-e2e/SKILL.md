---
name: mde-e2e
description: "Scrive ed esegue test end-to-end di siti web descritti in markdown (file *.e2e.md) con MdExplorer, e ne registra esiti, screenshot e script Playwright rigiocabili. Use when: test e2e, end-to-end, test di un sito, test dell'interfaccia web, collaudo, verificare che il sito funzioni, file .e2e.md, eseguire i test, rilanciare i test, esito dei test, mappa del sito, credenziali di test, script Playwright, regressione, smoke test, login di prova."
mde:
  origin: mdexplorer
  version: 8
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
e ciò che scopri su come il sito carica resta scritto nelle **schede di pagina** («Le attese», in `esecuzione.md`).

## Che cosa leggere, secondo quello che fai

Questo file vale sempre. Il resto è in file accanto a questo, nella cartella della skill: **leggi quello della fase
in cui sei, prima di cominciare**, per intero.

| Stai… | Leggi |
|---|---|
| scrivendo un test, o preparando i test di un sito nuovo | `preparare.md` e `mappa.md` |
| eseguendo i test (MdExplorer ti ha lanciato: «Esegui i test di …») | `esecuzione.md` e `mappa.md` |
| analizzando un rigioco fallito («Analizza con MarkAgent») | `rigioco-fallito.md` e `mappa.md` |

`mappa.md` dice come sono fatte la mappa del sito e le schede di pagina, che servono in tutte e tre le fasi.

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
│       ├── 2026-09-27_10-30/          ← una cartella per esecuzione
│       │   ├── report.md
│       │   ├── T1-03-messaggio-login.png
│       │   └── T2-03-errore-password.png
│       └── 2026-09-28_09-15_script/   ← un rigioco degli script, scritto da MdExplorer
│           ├── registro.T1.md         ← chiamate di rete e console del test, con i tempi
│           └── T1-05-messaggio-login.png
├── E2eTests.csproj                    ← progetto che compila gli script (uno per cartella di test)
├── E2eSupport.cs                      ← supporto comune agli script
└── e2e.runsettings                    ← browser usato per rigiocare gli script
```

`E2eTests.csproj`, `E2eSupport.cs` ed `e2e.runsettings` li **scrive MdExplorer** prima di lanciarti, e li
aggiorna quando cambia: **non crearli e non modificarli**.
Ce n'è uno solo per albero di test: se in una cartella sopra quella del test c'è già un `E2eTests.csproj`, vale
quello.

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

