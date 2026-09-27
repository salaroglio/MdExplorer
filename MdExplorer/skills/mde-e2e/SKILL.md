---
name: mde-e2e
description: "Scrive ed esegue test end-to-end di siti web descritti in markdown (file *.e2e.md) con MdExplorer, e ne registra esiti, screenshot e script Playwright rigiocabili. Use when: test e2e, end-to-end, test di un sito, test dell'interfaccia web, collaudo, verificare che il sito funzioni, file .e2e.md, eseguire i test, rilanciare i test, esito dei test, mappa del sito, credenziali di test, script Playwright, regressione, smoke test, login di prova."
mde:
  origin: mdexplorer
  version: 1
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
`browser_take_screenshot`, …), poi registri l'esito nel file stesso.

Ogni esecuzione lascia tre cose, tutte collegate dal `.e2e.md`:

- una riga di **esito** per ogni test;
- gli **screenshot** e un **report** dell'esecuzione;
- uno **script Playwright C#** per ogni test, che si rigioca senza di te in pochi secondi.

## I file

```text
test-e2e/                              ← la cartella dei test (il nome è libero)
├── mappa-sito-the-internet.md         ← mappa del sito: una per sito, condivisa dai test
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
sezione *«I file di supporto»*. Se esistono, non toccarli.

Nel `.gitignore` del progetto devono esserci queste righe; aggiungile se mancano:

```text
credenziali-*.txt
test-e2e/bin/
test-e2e/obj/
```

(adatta `test-e2e/` al nome vero della cartella dei test).

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
| ✔ Compare il testo "…" | `Expect(Page.GetByText("…")).ToBeVisibleAsync()` |
| ✔ Non compare il testo "…" | `Expect(Page.GetByText("…")).ToBeHiddenAsync()` |
| ✔ L'URL contiene "…" | `Expect(Page).ToHaveURLAsync(new Regex(Regex.Escape("…")))` |
| ✔ Il titolo della pagina è "…" | `Expect(Page).ToHaveTitleAsync("…")` |
| ✔ Il campo "Etichetta" vale "…" | `Expect(Page.GetByLabel("Etichetta")).ToHaveValueAsync("…")` |
| ✔ Il bottone "…" è disabilitato | `Expect(Page.GetByRole(AriaRole.Button, new() { Name = "…" })).ToBeDisabledAsync()` |

Una verifica scritta in altro modo («✔ La pagina sembra in ordine») è una **verifica a giudizio**: la valuti tu
guardando la pagina, e nello script diventa un commento `// verifica a giudizio: …` senza asserzione.

Quando l'utente ti chiede di scrivere un test, usa queste forme. Se una frase è ambigua (quale bottone? quale
testo esatto?) chiedi, oppure apri il sito e guarda la mappa del sito prima di scrivere.

## La mappa del sito

La mappa del sito è la memoria di ciò che si sa del sito: la leggi **prima** di eseguire, la aggiorni **dopo**.
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

Aggiorna la mappa aggiungendo o correggendo, non riscrivendola da capo. Se una cosa scritta non è più vera,
correggila e scrivi accanto la data.

## Le credenziali

Un file di testo con una riga `chiave=valore` per ogni credenziale; le righe che iniziano con `#` sono commenti.

```text
# credenziali per i test di the-internet
the-internet.utente=tomsmith
the-internet.password=SuperSecretPassword!
```

Leggi il file solo per scrivere i valori nel browser. **Non copiare mai un valore** nel `.e2e.md`, nella mappa,
nel report, negli script o nella risposta all'utente: lì si scrive solo la chiave (`{{the-internet.password}}`).
Se il file o una chiave mancano, fermati e dillo all'utente: non inventare credenziali.

## Eseguire i test

1. Leggi il `.e2e.md`, la mappa del sito e il file delle credenziali indicati nel front matter.
2. La **cartella dell'esecuzione** è `<artifacts>/esecuzioni/<AAAA-MM-GG_hh-mm>/`: te la indica chi lancia i
   test; se nessuno la indica, usa data e ora correnti. Creala se non c'è.
3. Per ogni test, in ordine, esegui i passi con gli strumenti playwright. Prima di cliccare o scrivere, guarda
   la pagina con `browser_snapshot` e scegli l'elemento per **ruolo e nome**.
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
`Ran Playwright code` in C#: usalo per i passi.

Le regole, tutte obbligatorie:

- **segui lo scheletro qui sotto alla lettera**: stessi `using`, stesso namespace `MdeE2e.<NomeTest>`, una classe
  per test che eredita `PageTest`, un solo metodo `Esegui`;
- le credenziali si leggono **solo** con `E2e.Credential(Credentials, "<chiave>")`, mai scritte nello script;
- gli screenshot si fanno **solo** con `E2e.Screenshot(Page, Artifacts, "<nome>")`, mai con percorsi scritti a mano;
- ogni passo e ogni verifica hanno sopra un commento con la riga del test, numero compreso;
- le asserzioni usano il **testo atteso del test** (tabella delle verifiche), mai quello osservato;
- l'intestazione dice lo stato: `valido` (test ✅), `riproduce un difetto` (test ❌: l'asserzione che fallisce
  resta quella giusta), `incompleto` (test ⚠️: scrivi i passi fino a dove sei arrivato e un
  `Assert.Fail("esecuzione non riuscita al passo <n>: <motivo>")`);
- il campo `impronta-sorgente` lo calcola MdExplorer: scrivi `da calcolare`.

```csharp esempio=script
// stato: valido
// sorgente: login.e2e.md, T1 — Login riuscito
// impronta-sorgente: da calcolare
// generatore: mde-e2e v1
// data: 2026-09-27 10:30
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using MdeE2e;

namespace MdeE2e.Login
{
    [TestFixture]
    public class T1_LoginRiuscito : PageTest
    {
        private const string BaseUrl = "https://the-internet.herokuapp.com";
        private const string Artifacts = "login.e2e";
        private const string Credentials = "credenziali-the-internet.txt";

        [Test]
        public async Task Esegui()
        {
            // 1. Apri `/login`
            await Page.GotoAsync(BaseUrl + "/login");

            // 2. Scrivi {{the-internet.utente}} nel campo "Username"
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Username" }).FillAsync(E2e.Credential(Credentials, "the-internet.utente"));

            // 3. Scrivi {{the-internet.password}} nel campo "Password"
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync(E2e.Credential(Credentials, "the-internet.password"));

            // 4. Premi "Login"
            await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();

            // 5. ✔ Compare il testo "You logged into a secure area!"
            await Expect(Page.GetByText("You logged into a secure area!")).ToBeVisibleAsync();
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Playwright;

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
    }
}
```

Gli script si rigiocano dalla cartella dei test con:

```text
dotnet test E2eTests.csproj --settings e2e.runsettings
```

## Cosa non fare

- Non modificare il testo dei test, né per farli passare né per «migliorarli», a meno che l'utente non te lo
  chieda.
- Non scrivere mai il valore di una credenziale fuori dal file delle credenziali.
- Non usare comandi di shell per eseguire i test: il browser si guida solo con gli strumenti playwright.
- Non cancellare esecuzioni, righe di esito o script vecchi: si accumulano.
