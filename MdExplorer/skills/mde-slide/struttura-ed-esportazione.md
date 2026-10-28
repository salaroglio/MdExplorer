# mde-slide — Struttura ed esportazione

Parte della skill `mde-slide` (una presentazione che porta alle sezioni, il PDF, lo zip HTML per chi non ha MdExplorer). Le regole che valgono sempre (scheletro, separatori, `Note:`, errori, checklist) sono in `SKILL.md`.

### «Una presentazione principale che porta alle sezioni»

Più presentazioni collegate: una **principale** con un link per sezione, e una presentazione per sezione.
Il link è un normale link markdown a un altro file `.md` (percorso relativo al file); con `#/n` dopo il nome si
arriva direttamente alla slide `n+1` (si conta da 0).

````markdown esempio=sezioni
## Le sezioni

- [Vendite](sezioni/vendite.md)
- [Acquisti](sezioni/acquisti.md)
- [Acquisti: i costi](sezioni/acquisti.md#/3)
````

Di una sezione si possono mostrare **solo alcune pagine** con `?pages=` nel link: utile quando la prima slide è il
titolo (già detto dal link) o quando si compone un documento da pezzi di altri. Una «pagina» è una slide
orizzontale, numerata **da 1** come è scritta nel file (una pila verticale `--` conta come una sola pagina).

````markdown esempio=pagine
- [Vendite, senza il titolo](sezioni/vendite.md?pages=2-)
- [Vendite, solo il riepilogo](sezioni/vendite.md?pages=2,6-9)
````

- `2` una pagina · `6-9` da 6 a 9 · `2-` dalla 2 alla fine · si separano con la virgola;
- le altre pagine **non ci sono** per la presentazione: sono tolte dal server, e la frase «dalla slide `n+1`» di
  `#/n` conta le pagine *mostrate*;
- una pagina che non esiste (`pages=12` in un file di 9) è un errore mostrato al posto della presentazione;
- vale solo per il link: la stessa presentazione aperta dall'albero dei file si vede intera.

Cosa fa MdExplorer, senza che serva scrivere altro:

- in una sezione compare in alto a sinistra un **breadcrumb** (`Le sezioni: principale › Vendite`); un clic
  riporta alla presentazione principale **sulla slide da cui si era partiti**. Funziona su più livelli;
- anche le frecce **← →** della barra di MdExplorer riaprono ogni presentazione sulla slide dove era stata
  lasciata;
- il breadcrumb si azzera quando una presentazione si apre dall'albero dei file, e non compare nel PDF.

Un link a una **pagina HTML del progetto** (`[Il prototipo](../assets/prototipo.html)`, percorso relativo al file) si
apre dentro MdExplorer, come un clic nell'albero dei file: entra nella cronologia (le frecce **← →** riportano alla
presentazione, sulla slide dove era) e in alto a sinistra compare lo stesso breadcrumb
(`principale › Prototipo`), con un clic si torna alla presentazione. Il file `.html` su disco non viene modificato.
Un link a un sito esterno (`https://…`) si apre invece nel browser di sistema.

Ogni sezione è una presentazione completa (front matter con `document_type: slides`), con il suo `title`: è il
nome che compare nel breadcrumb.

### «Mi serve il PDF»

Il pulsante **PDF** della barra, visibile solo sulle presentazioni, chiede dove salvare (propone il nome del
file markdown) e produce una pagina per slide, con gli sfondi. Le slide verticali seguono la loro slide madre;
il titolo del PDF è il `title` del front matter. Un'AI non può premere il pulsante: prepara la configurazione
qui sotto e di' all'utente di premere **PDF**. Due cose da sapere:

- ogni passo degli effetti a comparsa diventa una pagina: una slide con tre punti a comparsa diventa quattro
  pagine. Per una pagina sola per slide: `pdfSeparateFragments: false` in `reveal.config`;
- le note del relatore nel PDF: `showNotes: separate-page` in `reveal.config`.

### «La devo mandare a qualcuno che non ha MdExplorer»

Il pulsante **Esporta in HTML** della barra (l'icona della cartella zip, accanto al PDF, solo sulle
presentazioni) produce **uno zip**: un sito web locale che si apre con un doppio clic su `index.html`, senza
MdExplorer e senza rete. Dentro ci sono la presentazione e **tutto ciò che i suoi link raggiungono, a qualsiasi
profondità**: gli altri deck (con `?pages=` e `#/3`), i documenti markdown (diventano pagine HTML), le pagine
HTML del progetto e i file che caricano, le immagini, gli sfondi, i video. Restano animazioni, transizioni,
formule, diagrammi interattivi, vista relatore, annotazioni e breadcrumb; la modifica no. Un'AI non può
premere il pulsante: prepara la presentazione così e di' all'utente di premere **Esporta in HTML**:

- **link relativi a file del progetto** (`costi.md`, `../docs/analisi.md`, `img/grafico.png`): un file fuori
  dalla cartella del progetto non entra nello zip;
- i **diagrammi PlantUML** entrano come l'SVG che MdExplorer ha già generato: se un diagramma è stato appena
  cambiato, l'utente apra la presentazione in MdExplorer prima di esportare;
- **niente mermaid**: non entra nello zip, il blocco resterebbe testo;
- un **video YouTube** incorporato (`<iframe src="https://www.youtube.com/embed/…">`) resta un video dentro la
  slide: da una pagina aperta dal disco YouTube lo rifiuterebbe (errore 153), quindi nello zip passa da
  `www.mdexplorer.net`. Serve internet; un link a YouTube resta un link;
- una risorsa su un altro sito (immagine, font) funziona solo con internet: meglio scaricarla nel progetto
  (vedi «Un'immagine o una GIF trovata in rete» in `contenuto.md`).

Quello che non è potuto entrare è scritto in `_mde/resoconto.html` dentro lo zip, e l'app lo dice al termine.
