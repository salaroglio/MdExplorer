# mde-slide — Ricette per l'aspetto

Parte della skill `mde-slide` (sfondi e video, transizioni, elementi che si spostano, immagini sovrapposte, opzioni della presentazione, tema, stile di MdExplorer). Le regole che valgono sempre (scheletro, separatori, `Note:`, errori, checklist) sono in `SKILL.md`.

### «Questa slide deve avere uno sfondo diverso»

Un commento `.slide` nella slide. Può stare in qualunque punto della slide (in cima è più leggibile) e può
portare più attributi insieme, per esempio sfondo e transizione:

````markdown esempio=sfondo
<!-- .slide: data-background-color="#1b2a3a" -->

## Una sezione nuova

---

<!-- .slide: data-background-image="img/cantiere.jpg" data-background-opacity="0.4" -->

## Il cantiere oggi
````

Il percorso dell'immagine è relativo al file, come in un documento. Esiste anche
`data-background-gradient="linear-gradient(to bottom, #283b95, #17b2c3)"`.

- Su uno sfondo scuro reveal.js schiarisce da solo il testo della slide, qualunque sia il tema.
- Uno sfondo scuro «che si nota» si nota solo su un tema chiaro: se conta, scrivi `reveal.theme: white`,
  altrimenti con MdExplorer in tema scuro la presentazione diventa scura tutta.
- Non mettere un diagramma PlantUML su una slide a sfondo scuro: il diagramma resta chiaro, un riquadro bianco.

### «Come sfondo voglio un video, anche di YouTube, che si muove»

Un video di YouTube come sfondo vivo, non un'immagine ferma: `data-background-iframe` con l'indirizzo
**embed** del video (`https://www.youtube.com/embed/<ID>`, non `watch?v=`). `<ID>` è la parte dopo `watch?v=`.

````markdown esempio=sfondo-video
<!-- .slide: data-background-iframe="https://www.youtube.com/embed/aqz-KE-bpKQ?autoplay=1&mute=1&loop=1&playlist=aqz-KE-bpKQ&controls=0&playsinline=1" data-background-interactive -->

# Il titolo sopra il video
````

- `autoplay=1&mute=1`: parte da solo. I browser lo permettono **solo senza audio**; per l'audio togli
  `mute=1` e il video lo fa partire chi presenta, con un clic.
- `loop=1&playlist=<ID>`: ricomincia alla fine. YouTube vuole l'ID ripetuto in `playlist`.
- `controls=0`: nessuna barra di comandi sopra lo sfondo.
- `data-background-interactive`: il video risponde ai clic (pausa, avvio). Senza, è solo sfondo.
- Serve la **rete**: senza, lo sfondo resta nero.
- Il video è 16:9: per non avere bande nere dai alla presentazione le stesse proporzioni, `width: 1280` e
  `height: 720` in `reveal.config`.
- Il testo va scelto per il video: su un video chiaro il tema `white`, su uno scuro `black`.
- Nel **PDF** un video non c'è: compare al più un fotogramma. Se il PDF conta, per quella slide usa
  un'immagine.

Un file video del progetto (mp4, webm) si mette con `data-background-video="video/intro.mp4"`, anche in più
formati separati da virgola (`"video/intro.mp4, video/intro.webm"`), più `data-background-video-loop` e
`data-background-video-muted`. Il percorso è relativo al file, come per le immagini, e funziona senza rete.

### «Questa slide deve entrare in un altro modo»

````markdown esempio=transizione
<!-- .slide: data-transition="zoom" -->

## Il numero dell'anno

**+12%**
````

Transizioni: `none`, `fade`, `slide`, `convex`, `concave`, `zoom`. Per cambiarle a tutta la presentazione
usa `transition` in `reveal.config`.

Ogni slide comanda **come entra e come esce lei**: non esiste «la transizione fra A e B», le due slide che si
danno il cambio si animano insieme, ognuna con la propria. Il valore scritto su una slide **vale più** di quello del
deck; una slide dentro una pila verticale (`--`) lo usa per i passaggi in verticale, mentre quello in orizzontale è
della pila e segue il deck.

L'utente non deve scriverlo a mano: nella vista di MdExplorer la **barra delle slide** ha un pulsante 🎬 (visibile
con «Modifica» acceso) con l'elenco delle scelte e un'anteprima animata di ognuna, per «questa slide» o per «tutto il
deck»; MdExplorer scrive il commento `<!-- .slide: data-transition="…" -->` sulla prima riga della slide, o
`transition` in `reveal.config`, e rifiuta con un motivo se il file è scritto in un modo che non sa modificare in
sicurezza (per esempio un commento `.slide:` su più righe). La transizione scelta per ultima viene riproposta, a un
clic, sulla slide successiva.

**Quando aggiungi una slide a una presentazione che c'è già**, falle seguire lo stile delle vicine: se la slide che la
precede ha un suo `<!-- .slide: data-transition="…" -->`, scrivi lo **stesso** sulla nuova (la scelta dell'utente è nel
file, è lì che si legge: quella ricordata dal browser tu non la vedi). Se la precedente non ne ha, **non aggiungerne**:
la nuova segue il deck, come le altre. Non inventare una transizione che il resto della presentazione non usa.

### «Un elemento che si sposta da una slide alla successiva»

L'**auto-animate**: due slide consecutive con `data-auto-animate`; gli elementi con lo stesso `data-id` si
spostano e cambiano forma da una posizione all'altra.

````markdown esempio=auto-animate
<!-- .slide: data-auto-animate -->

## Il progetto

<div data-id="barra" style="background:#2a9d8f; width:200px; height:40px; margin:auto"></div>

---

<!-- .slide: data-auto-animate -->

## Il progetto, un anno dopo

<div data-id="barra" style="background:#e76f51; width:700px; height:40px; margin:auto"></div>
````

### «Più immagini una sopra l'altra, che si alternano»

Per le cose che il markdown non sa dire si scrive HTML. Lascia una riga vuota prima e dopo il blocco HTML.

````markdown esempio=pila
## Le tre versioni

<div class="r-stack">
<img class="fragment fade-out" data-fragment-index="0" src="img/v1.png" width="450">
<img class="fragment current-visible" data-fragment-index="0" src="img/v2.png" width="450">
<img class="fragment" src="img/v3.png" width="450">
</div>
````

### «La presentazione deve comportarsi in un certo modo»

Le opzioni di reveal.js vanno sotto `reveal.config`, scritte come le scrive reveal.js:

````markdown esempio=config
---
title: Kick-off
document_type: slides
reveal:
  config:
    transition: fade
    slideNumber: c/t
    controls: false
    autoSlide: 8000
    loop: true
    pdfSeparateFragments: false
---

# Kick-off
````

- `true`/`false` e i numeri **senza virgolette**: `'false'` fra virgolette è una scritta, e per reveal.js vale
  come vero.
- Due impostazioni le decide MdExplorer e il file non le può cambiare: i `plugins` e il percorso di KaTeX
  (`katex.local`). Scriverle dà un errore.
- `pdfSeparateFragments` cambia solo il PDF: presentando, gli effetti a comparsa restano.
- MdExplorer parte da `hash: true`, così la presentazione resta sulla slide corrente quando il file cambia, e
  da `scrollActivationWidth: null`, così resta a slide anche in un pannello stretto. Tutte e due si possono
  cambiare.

### «Un altro aspetto»

````markdown esempio=tema
---
title: Kick-off
document_type: slides
reveal:
  theme: dracula
  highlight_theme: zenburn
---

# Kick-off
````

- `theme`: `mde-space` (lo stile di MdExplorer, vedi sotto), `black`, `white`, `black-contrast`, `white-contrast`,
  `dracula`, `serif`, `beige`, `blood`, `league`, `moon`, `night`, `simple`, `sky`, `solarized`. **Senza rete** funzionano del tutto solo i primi
  sei; gli altri scaricano i caratteri da Google Fonts e altrimenti usano quelli di sistema.
- Senza `theme` la presentazione segue il tema di MdExplorer: `white` se è chiaro, `black` se è scuro.
- `highlight_theme` (colori del codice): `monokai` o `zenburn`.
- `theme` e `highlight_theme` stanno sotto `reveal:`, **non** sotto `config`.

### «Nello stile di MdExplorer»: lo spazio, i razzi, gli alieni

Il tema `mde-space` è quello di MdExplorer: cielo stellato, titoli con il gradiente delle card di MDE, grassetti
dorati, testo chiaro. Viene con un set di sfondi e icone a fumetto che il servizio serve sempre, in ogni progetto:

- **sfondi** (1920×1080, alcuni animati): `/mde-space/sfondi/stelle.svg` (quello del tema), `luna.svg`, `marte.svg`,
  `alieni.svg` (il paesaggio in basso), `orbite.svg`, `razzo.svg` (il disegno a destra);
- **icone** (`/mde-space/icone/<nome>.svg`): `razzo`, `ufo`, `alieno`, `astronauta`, `pianeta`, `luna`, `marte`,
  `terra`, `stella`, `cometa`, `satellite`, `telescopio`, `bandiera`, `base`.

````markdown esempio=mde-space
---
document_type: slides
reveal:
  theme: mde-space
---

<!-- .slide: data-background-image="/mde-space/sfondi/luna.svg" class="mde-cielo" -->
<img class="mde-icona-grande" src="/mde-space/icone/razzo.svg" alt="">

# Il lancio

---

## <img class="mde-icona" src="/mde-space/icone/telescopio.svg" alt=""> A che cosa serve

- un punto
- un altro punto
````

- Le icone del set **sempre in HTML** (`<img class="mde-icona" src="/mde-space/…">`), mai `![…](/mde-space/…)`: in
  markdown un percorso con `/` è un file del progetto, e MdExplorer lo riscrive (l'icona sparisce nel visualizzatore).
- **Mai `{.classe}` di Markdig in un titolo**: va sul titolo intero e il testo dopo le graffe si perde.
- `mde-icona` sta nel testo (nel titolo di una slide); `mde-icona-grande` sta da sola, sopra il titolo della prima slide.
- Con un paesaggio in basso (`luna`, `marte`, `alieni`) scrivi `class="mde-cielo"`: il testo sale e resta sopra
  l'orizzonte. `marte` ha l'orizzonte alto: va bene per un titolo, non per un elenco. Con `orbite` o `razzo` scrivi
  `class="mde-sinistra"`: il testo sta nella metà sinistra, il disegno a destra.
- Un paesaggio su una slide piena di testo si copre: mettilo sulla prima e sull'ultima slide, corte; le altre restano
  sul cielo del tema, con un'icona nel titolo.
