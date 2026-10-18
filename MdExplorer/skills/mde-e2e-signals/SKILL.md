---
name: mde-e2e-signals
description: "Inserisce nel codice sorgente di un sito i segnali di MdExplorer per i test end-to-end: il sito annuncia quando ha finito di caricare (start, ready, error), così gli script dei test aspettano il momento esatto invece di indovinarlo. Vale per qualunque tecnologia (JavaScript, Angular, React, Vue, pagine generate dal server, htmx, Blazor). Use when: segnali e2e, strumentare il sito per i test, il test non aspetta il caricamento, script e2e fragili, rigioco che fallisce per i tempi, attesa esatta, mdeE2e, data-mde, mde-e2e-signals."
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

# I segnali dei test end-to-end

Il browser non sa quando un sito ha finito quello che stava facendo: una chiamata di rete che porta i dati e una
che controlla lo stato ogni secondo sono uguali, e dopo la risposta il sito può lavorare ancora a lungo. Lo sa
solo il sito. Con questa skill **il sito lo dice**: annuncia l'inizio e la fine di ogni caricamento importante,
e gli script dei test (skill `mde-e2e`) aspettano esattamente quel momento.

Qui modifichi il **codice sorgente del sito**, che deve essere nel progetto. Ogni modifica passa dall'utente
come qualunque altra modifica al codice: descrivila, poi applicala.

## Il contratto

Un **segnale** ha un **nome** (cosa si carica: `grafo`, `elenco-ordini`, `dettaglio-cliente`) e una **chiave**
(quale: il programma scelto, il numero dell'ordine). Ha tre stati:

| Stato | Quando | Dove |
|---|---|---|
| `start` | all'inizio dell'azione, **subito**, prima di qualunque attesa | nel gestore dell'evento (clic, selezione, cambio di vista) |
| `ready` | quando la pagina ha **finito**: dati arrivati **e** pagina aggiornata | dopo l'ultimo aggiornamento della pagina |
| `error` | quando il caricamento fallisce | nel ramo d'errore |

Il segnale si vede in due modi, e servono tutti e due:

- un **attributo** nella pagina, `data-mde-<nome>="<stato>:<chiave>"` (`data-mde-grafo="ready:cob:bs522"`): lo
  aspetta lo script del test, con un solo selettore che lo trova su qualunque elemento;
- una **riga in console**, `[mde] <stato> <nome> <chiave>`: la vede l'LLM che esegue il test (lo snapshot della
  pagina non mostra gli attributi `data-*`).

Perché la chiave: se la pagina passa da un programma all'altro, «ho finito» non basta, lo script prenderebbe per
buono il `ready` del programma precedente. `start` cancella subito il `ready` vecchio, e lo script aspetta il
`ready` **di quella chiave**.

La chiave la deve poter conoscere **chi scrive il test** guardando l'azione: il valore scelto nella select,
l'identificativo nel link. Se non c'è niente del genere (un pulsante «Aggiorna»), usa una chiave fissa
(`corrente`): funziona perché `start` toglie il `ready` precedente.

## L'aiuto JavaScript

Un file senza dipendenze, da aggiungere al sito **così com'è** con il nome `mde-e2e-signals.js`:

```js esempio=aiuto
// MdExplorer, convenzione dei segnali e2e (skill mde-e2e-signals). Nessuna dipendenza. Non modificare.
window.mdeE2e = window.mdeE2e || {
  on: (function () { try { return localStorage.getItem('mde-e2e') === '1'; } catch (e) { return false; } })(),
  signal: function (state, name, key, reason) {       // state: 'start' | 'ready' | 'error'
    if (!this.on) return;
    document.documentElement.setAttribute('data-mde-' + name, state + ':' + key);
    console.info('[mde] ' + state + ' ' + name + ' ' + key + (reason ? ' — ' + reason : ''));
  }
};
```

Nel codice del sito si chiama `window.mdeE2e?.signal('start', 'grafo', programma)`. Il `?.` fa sì che il sito
funzioni anche senza il file.

**Accensione.** Prima di modificare, **chiedi all'utente** come vuole i segnali:

- **solo durante i test** (consigliato): l'aiuto sopra, così com'è. Si accendono con la chiave `mde-e2e` in
  `localStorage`, che imposta lo script del test; per chi usa il sito non cambia niente, costa un controllo
  vero/falso;
- **sempre accesi**: cambia solo la riga `on:` in `on: true,`. In console compaiono le righe `[mde]` anche in
  produzione.

**Dove metterlo.** Carica il file **prima** del codice del sito, come file e non come script in linea (una
Content-Security-Policy rifiuta gli script in linea):

| Tecnologia | Come |
|---|---|
| HTML e JavaScript | il file accanto alle pagine, `<script src="mde-e2e-signals.js"></script>` nel `<head>` |
| Angular | il file in `src/` (o `public/`) e il percorso nell'elenco `scripts` del progetto in `angular.json` |
| React, Vue, Svelte con Vite | il file in `public/` e `<script src="/mde-e2e-signals.js"></script>` nel `<head>` di `index.html` |
| Blazor | il file in `wwwroot/`, `<script src="mde-e2e-signals.js"></script>` nella pagina ospite; da C# `await JS.InvokeVoidAsync("mdeE2e.signal", "ready", "grafo", chiave);` |
| Pagine generate dal server (Razor, JSP, PHP, Django), htmx | anche **senza JavaScript**: il server scrive l'attributo nell'HTML che manda, per esempio sul contenitore del frammento: `<div data-mde-grafo="ready:cob:bs522">` |

Con TypeScript, per `window.mdeE2e` aggiungi una dichiarazione:
`declare global { interface Window { mdeE2e?: { signal(state: string, name: string, key: string, reason?: string): void } } }`.

## Dove vanno i segnali

**Quali azioni.** Solo quelle dopo le quali la pagina **lavora in modo asincrono**: carica dati, ricostruisce
elenchi, disegna grafici, cambia vista caricando qualcosa. Parti dalle schede di pagina della mappa del sito
(skill `mde-e2e`) e dai test che esistono: i passi con un'attesa «dedotta» (`// attesa: GET …`) o «nessuna
trovata» sono i primi candidati. Non servono per ciò che cambia subito (spuntare una casella senza effetti,
aprire un menu) né per le chiamate periodiche (controlli di stato, notifiche).

**`start`**: la prima istruzione del gestore, prima di ogni `await`, `then`, `subscribe`.

**`ready`**: dopo che la pagina è **aggiornata**, non quando arrivano i dati. È l'errore più facile: i dati
arrivano, il framework aggiorna la pagina un attimo dopo, e se il `ready` parte prima lo script riprende troppo
presto. Dove metterlo:

| Tecnologia | Dopo cosa |
|---|---|
| JavaScript | dopo l'ultima istruzione che modifica la pagina. Se il lavoro continua in un `setTimeout`, un `requestAnimationFrame` o un worker, il `ready` va **alla fine di quel lavoro**, non prima |
| Angular | la vista si aggiorna al giro di change detection successivo: `afterNextRender(() => window.mdeE2e?.signal('ready', …), { injector: this.injector })` (Angular 17+), oppure `this.cdr.detectChanges()` e poi il segnale |
| React | in un `useEffect` che dipende dai dati caricati: parte dopo che React ha aggiornato la pagina |
| Vue | `await nextTick()`, poi il segnale |
| Svelte | `await tick()`, poi il segnale |
| Blazor | in `OnAfterRenderAsync`, quando i dati caricati sono quelli della chiave |
| Pagine dal server, htmx | l'attributo è nell'HTML che arriva: è pronto quando è nella pagina |

Se dopo i dati c'è un componente che disegna per conto suo (un grafico, un grafo, una mappa), usa il suo evento
di fine disegno (`layoutstop`, `rendered`, `finished`, …): lì va il `ready`.

**`error`**: in ogni ramo d'errore del caricamento (`catch`, callback d'errore, risposta non riuscita), con il
motivo come quarto argomento. Senza, uno script che aspetta il `ready` resta fermo fino al tempo massimo invece
di fallire subito con il motivo.

Un esempio in JavaScript:

```js esempio=uso
document.getElementById('program-selector').onchange = async (e) => {
  const programma = e.target.value;
  window.mdeE2e?.signal('start', 'grafo', programma);          // subito, prima di ogni attesa
  try {
    const dati = await (await fetch('/api/graph/' + encodeURIComponent(programma))).json();
    disegnaFiltri(dati.kinds);
    disegnaGrafo(dati.nodes, () => {                            // il grafo finisce di disegnare più tardi
      window.mdeE2e?.signal('ready', 'grafo', programma);      // pagina aggiornata: ora è pronto
    });
  } catch (err) {
    window.mdeE2e?.signal('error', 'grafo', programma, String(err));
  }
};
```

## Dopo le modifiche

1. **Le schede di pagina** (skill `mde-e2e`): per ogni azione strumentata scrivi il segnale, con nome e da dove
   viene la chiave: «Attesa: segnale `grafo`, chiave = il valore scelto nel selettore (`cob:<programma>`)». Scrivi
   anche «dal sorgente, da confermare» finché un'esecuzione del test non vede il segnale in console.
2. **Gli script esistenti** non si toccano: si rigenerano rieseguendo i test con MarkAgent, che trova i segnali in
   console e li usa.
3. **Come provarlo a mano** (dillo all'utente): con i segnali solo per i test, nella console del browser
   `localStorage.setItem('mde-e2e', '1')`, ricarica la pagina, fai l'azione; devono comparire
   `[mde] start …` e poi `[mde] ready …` con la stessa chiave.
4. **Alla fine** di' all'utente, in poche righe: i file modificati, le azioni con il loro segnale (nome e chiave),
   come si accendono, come provarlo.

## Cosa non fare

- Non cambiare il comportamento del sito: i segnali si aggiungono, non sostituiscono niente.
- Non modificare l'aiuto `mde-e2e-signals.js`, salvo la riga `on:` se l'utente vuole i segnali sempre accesi.
- Non mettere il `ready` quando arrivano i dati se la pagina si aggiorna dopo.
- Non segnalare le chiamate periodiche o le azioni che non caricano niente.
- Non modificare i file dei test (`*.e2e.md`) né gli script.
