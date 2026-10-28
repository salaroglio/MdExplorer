---
name: mde-plantuml
description: "Disegna diagrammi PlantUML in MdExplorer con colore sobrio e le convenzioni proprie di ogni tipo di diagramma. Use when: blocco plantuml, diagramma di flusso, workflow, activity diagram, sequence diagram, diagramma di sequenza, class diagram, diagramma delle classi, schema del database, ER, entity relationship, mindmap, mappa mentale, colorare un diagramma, palette, skinparam, style, colore di una freccia, colore di un nodo, il diagramma non si vede in tema scuro."
mde:
  origin: mdexplorer
  version: 5
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


# Diagrammi PlantUML in MdExplorer

Un diagramma serve a far capire **una cosa sola**. Se dopo averlo disegnato non sai dire qual è, non è pronto: non aggiungere colore, togli elementi.

## Due vincoli di MdExplorer, prima dell'estetica

### 1. Dentro un blocco plantuml non ci va nessun backtick

MdExplorer riconosce i blocchi con un'espressione regolare che cattura il corpo con una classe di caratteri **negata sul backtick**. Il blocco quindi finisce al primo backtick che incontra, non alla chiusura del fence: basta un nome di metodo scritto fra backtick dentro una nota e il diagramma smette di comparire, senza nessun messaggio di errore.

(Questa regola vale anche per un documento che *parla* di PlantUML: scrivere per esteso la riga che apre un blocco, dentro un esempio, fa partire un rendering indesiderato lì dove non te lo aspetti.)

Per evidenziare un identificatore dentro il diagramma usa il **corsivo** di PlantUML (`//testo//`) o le doppie virgolette, mai i backtick.

### 2. In tema scuro il diagramma viene ribaltato

Se il tema è scuro e il progetto non ha attivato *Mantieni i colori originali*, MdExplorer applica all'SVG:

    filter: invert(0.88) hue-rotate(180deg)

Non è un dettaglio cosmetico, cambia come si progetta. Ecco cosa succede davvero ai colori (valori calcolati, non stimati):

| colore che scrivi | diventa in tema scuro | resta leggibile? |
|---|---|---|
| `#1A73E8` blu | `#5599F2` | sì, ancora blu |
| `#D93025` rosso | `#FF867E` | sì, ancora rosso |
| `#188038` verde | `#5AA972` | sì, ancora verde |
| `#F1F3F4` grigio pallido | `#272829` | sì, riempimento discreto |
| `#FFFFFF` bianco puro | `#1F1F1F` | **no**, sparisce nello sfondo |
| `#808080` grigio medio | `#7F7F7F` | invariato |

La regola che ne discende è una sola:

> **La tinta sopravvive, la luminosità si ribalta.** Codifica il significato nel *colore*, mai nel *chiaro/scuro*.

Un diagramma con "grigio chiaro = fatto, grigio scuro = da fare" si legge al contrario in tema scuro. Gli stessi due stati distinti come verde e ambra si leggono uguali in entrambi i temi.

## Prima di consegnare: verifica

Se hai a disposizione lo strumento **`CheckPlantuml`** (server MCP di MdExplorer), chiamalo su ogni diagramma prima di scriverlo nel documento. Non renderizza niente, costa poco, e vede cose che tu non puoi vedere.

Come si legge la risposta:

- **`ok: true`** — il diagramma si vede. Se ci sono `warning`, valuta se correggerli; sono difetti che si notano solo in tema scuro.
- **`ok: false`** — c'è almeno un `error`: il diagramma **non comparirà affatto**. Per ogni problema hai `line`, la riga stessa in `source`, il `meaning` e **una** correzione in `fix`. Applicala e richiama la verifica.
- **`toolUnavailable` valorizzato** — la verifica **non è stata eseguita**: manca qualcosa sulla macchina. Il tuo diagramma non è stato giudicato, quindi **non cambiarlo**: riferisci all'utente cosa manca.

Quest'ultimo punto è il più importante da rispettare. Correggere un diagramma sano perché la verifica non è partita è il modo più veloce di peggiorarlo.

Lo strumento non sostituisce le regole che seguono: controlla la sintassi e i due difetti tipici di MdExplorer, non se il diagramma si capisce. Quello resta compito tuo.

I due avvisi di colore che `CheckPlantuml` solleva, così li eviti in partenza:

- **`#FFFFFF` ovunque compaia**, anche come colore del testo: in tema scuro diventa lo sfondo.
- **Due o più grigi puri diversi** (R, G e B identici, come `#EEEEEE` e `#999999`): è il significato affidato al chiaro/scuro. I grigi della palette qui sotto hanno una punta di blu e non contano come grigi puri.

## Colore: le regole

**Il colore è un'informazione, non una decorazione.** Se togliendo tutti i colori il diagramma dice ancora la stessa cosa, quei colori erano rumore.

- **Massimo tre colori con significato** in un diagramma. Oltre, nessuno se li ricorda mentre legge. (L'unica eccezione motivata è la mindmap colorata per ramo: vedi `mindmap.md`.)
- Il **percorso normale non si colora**. Si colora l'eccezione: l'errore, il ramo che costa, il punto dove serve una decisione umana.
- **Riempimenti pallidi, tratti saturi.** Il riempimento è lo sfondo di una parola, non un evidenziatore.
- **Il bordo dello stesso colore del riempimento, ma saturo**: un riempimento verde con bordo grigio sembra un errore di stampa.
- **Se il significato di un colore non è ovvio dal testo, aggiungi una `legend`.** Rosso = errore si capisce da solo; verde = "introdotto in questa versione" no.
- Non fidarti dei colori di default di PlantUML: apri con `!theme plain` e decidi tu.

### Palette di lavoro

Tutti i valori sono già calcolati contro il filtro del tema scuro: la tinta resta riconoscibile, il riempimento diventa un fondo scuro discreto.

| ruolo | riempimento | tratto | in tema scuro (riemp. / tratto) |
|---|---|---|---|
| neutro, la maggioranza degli elementi | `#F1F3F4` | `#5F6368` | `#272829` / `#93969A` |
| esito positivo, percorso felice | `#E6F4EA` | `#188038` | `#222D25` / `#5AA972` |
| attenzione, decisione, costo | `#FEF7E0` | `#F29900` | `#2A2513` / `#A46000` |
| errore, percorso di fallimento | `#FCE8E6` | `#D93025` | `#392A28` / `#FF867E` |
| elemento in evidenza, il soggetto del diagramma | `#E8F0FE` | `#1A73E8` | `#252B36` / `#5599F2` |

Due tinte in più, **solo per categorie senza giudizio** (i rami di una mindmap, i livelli di un'architettura), mai per dire bene/male:

| ruolo | riempimento | tratto | in tema scuro (riemp. / tratto) |
|---|---|---|---|
| categoria viola | `#F3E8FD` | `#9334E6` | `#332A3A` / `#CF87FF` |
| categoria ciano | `#E4F7FB` | `#12B5CB` | `#1C2A2D` / `#0D899A` |

Il testo resta sempre del colore di default (quasi nero, che in tema scuro diventa quasi bianco). Se proprio devi colorarlo, usa un **tratto** della palette, mai un riempimento: un testo `#E6F4EA` non si legge in nessuno dei due temi.

### Dove si mette il colore: tre livelli

Dal più generale al più puntuale. **Scendi di livello solo quando quello sopra non basta.**

1. **Default per tipo di elemento** — `skinparam ClassBackgroundColor #F1F3F4`. Decide l'aspetto della maggioranza: il neutro.
2. **Categoria con significato, tramite stereotipo** — dichiari una volta cosa vuol dire `<<Nuovo>>` e lo applichi a tutti gli elementi di quella categoria. È il modo giusto per colorare **con un significato**: il colore sta in un posto solo, e l'etichetta dello stereotipo fa da legenda.

        skinparam class {
          BackgroundColor<<Nuovo>> #E6F4EA
          BorderColor<<Nuovo>> #188038
        }
        class Bookmark <<Nuovo>>

   Se l'etichetta «Nuovo» sul disegno non ti serve, `hide stereotype` la nasconde e il colore resta.
3. **Il singolo elemento, in linea** — `#FCE8E6:Rifiuta il salvataggio;`. Per l'eccezione che capita una volta. Se lo stesso colore in linea compare tre volte, è una categoria: torna al livello 2.

### Sintassi del colore

Quella di ogni tipo di diagramma è nel suo file (vedi «Ogni tipo di diagramma ha il suo file»). Tutte provate sul jar distribuito con MdExplorer (1.2026.1) e sui 1.2022.x. Dove una sintassi **non** funziona, è scritto.

**Testo colorato e legenda (tutti i diagrammi)**

    legend right
      <color:#188038>verde</color> = introdotto in questa versione
      <color:#D93025>rosso</color> = deprecato
    endlegend

`<color:#...>testo</color>` funziona anche dentro le etichette di azioni, note e frecce: usalo per **una parola**, non per una frase.

## Ogni tipo di diagramma ha il suo file

Le regole e la sintassi del colore di ogni tipo sono in un file accanto a questo, nella cartella della skill. **Prima
di disegnare, leggi quello del tipo che disegni**:

| Disegni… | Leggi |
|---|---|
| un workflow, un diagramma di flusso, un activity diagram | `workflow.md` |
| un sequence diagram | `sequence.md` |
| un class diagram, uno schema del database, un ER | `class-ed-er.md` |
| una mindmap | `mindmap.md` |

Per un altro tipo valgono le regole di questo file.

## Checklist

- [ ] Nessun backtick dentro il blocco plantuml.
- [ ] Il diagramma dice **una** cosa, e sai dire quale.
- [ ] Il significato sta nella tinta, non nel chiaro/scuro.
- [ ] Non più di tre colori con significato; il percorso normale non è colorato.
- [ ] Riempimenti pallidi, tratti saturi dello stesso colore; nessun `#FFFFFF`, nemmeno come testo.
- [ ] Un colore ripetuto è diventato uno stereotipo (o una classe `<<nome>>` nella mindmap), non tre colori in linea.
- [ ] Un colore dal significato non ovvio ha la sua `legend`.
- [ ] `!theme plain` in testa, così i default di PlantUML non decidono al posto tuo.
- [ ] Le regole del tipo di diagramma sono rispettate: attivazioni nei sequence, cardinalità nei class ed ER, un solo start negli activity.
- [ ] Nella mindmap: una sola strategia di colore (per stato *o* per ramo), classe su ogni nodo del ramo, `<style>` e non `skinparam`.
- [ ] Se `CheckPlantuml` è disponibile, la risposta è `ok: true` (o gli avvisi rimasti sono una scelta consapevole).
