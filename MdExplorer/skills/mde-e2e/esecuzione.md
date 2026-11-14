# mde-e2e — Eseguire i test

Parte della skill `mde-e2e`: le forme del file di test sono in `SKILL.md`, mappa del sito e schede di pagina in `mappa.md`.

## Le credenziali

Un file di testo con una riga `chiave=valore` per ogni credenziale; le righe che iniziano con `#` sono commenti.

```text esempio=credenziali
# credenziali per i test di the-internet
the-internet.utente=utente-di-esempio
the-internet.password=password-di-esempio
```

**Non leggere e non cercare il file delle credenziali.** È escluso da git, quindi le ricerche (`Glob`, `Grep`)
possono non vederlo: non vuol dire che manchi, e durante un'esecuzione non devi crearlo (in chat, per la prima
configurazione, vedi `preparare.md`). MdExplorer ha già controllato, prima di
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

Quello che hai scoperto va nella scheda della pagina (`mappa.md`), così il prossimo test lo sa già.

Se hai dovuto dedurre un'attesa (livello 2 o 3) e il **codice sorgente del sito è nel progetto**, alla fine
suggerisci all'utente di aggiungere i segnali a quelle azioni con la skill **`mde-e2e-signals`**: elenca i passi e
le pagine. Durante un'esecuzione non modificare il sito.

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
  `Test_T<n>` per test che eredita `E2eTest` (nel rigioco registra chiamate e console del test), un solo metodo `T<n>_<Titolo>` (così l'esito di
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
// generatore: mde-e2e v3
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
    public class Test_T1 : E2eTest
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


I file di supporto (`E2eTests.csproj`, `E2eSupport.cs`, `e2e.runsettings`) li scrive MdExplorer. Gli script si rigiocano dalla finestra dei test di MdExplorer («Rigioca gli script»), che esegue
`dotnet test E2eTests.csproj --no-restore --settings e2e.runsettings`. I pacchetti (circa 230 MB la prima volta) li
scarica MdExplorer solo quando l'utente lo chiede dalla stessa finestra: non lanciare `dotnet restore` né
`dotnet build` tu.
