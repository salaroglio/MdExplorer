---
name: mde-abox
description: Convention for generating ABox (instance data) files inside an MDE project. Use when extracting instance triples from a source document (analyses, requirements, mapping tables) into `ontology/ABox/`: produces a `.ttl` with concrete instances conforming to a referenced TBox plus a `.md` companion with description + runnable PowerShell load + sample SPARQL query. Triggers on "extract instances", "generate ABox", "create RDF data from doc", "populate ontology with examples", "instance data for ontology".
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

# ABox generation convention

Convenzione per generare file di **dati istanziati** (ABox = Assertional Box) in un progetto MDE configurato per Apache Jena Fuseki. Si applica quando un AI agent estrae **istanze concrete** da un documento sorgente (analisi tecnica, requirement, tabella di mapping, conversazione) e le materializza come triple RDF conformi a una TBox preesistente.

## Cosa è una ABox

La **ABox** contiene i **dati**: istanze concrete di classi definite nella TBox. Esempio: se la TBox dichiara `stem:EntityStem` come classe, una ABox conterrà `<urn:mde:abox:...:CNT> a stem:EntityStem ; stem:stemId "CNT" ; ...`.

La ABox cresce continuamente con il progredire dell'analisi e va aggiornata in modo idempotente per ogni documento sorgente.

## Prerequisito

La **TBox di riferimento deve già essere caricata** sul dataset Fuseki target. Senza, il triplestore accetta comunque le triple ma non può ragionare su classi/proprietà. Verifica con SPARQL `ASK { ?c a owl:Class }` prima di generare la ABox.

Se la TBox non c'è, **fermarsi** e chiedere all'utente di caricarla via `ontology/TBox/load-all.md` (vedi [[mde-tbox]]).

## Layout obbligatorio dei file

```
ontology/ABox/<dataset-name>.ttl    ← Turtle con le istanze
ontology/ABox/<dataset-name>.md     ← descrizione + runnable di caricamento + sample query
```

`<dataset-name>` è kebab-case, descrittivo, identifica il **set di istanze** che il file contiene. Esempi: `example-stems`, `CWPGFG10-analysis`, `payment-domain-data`, `raiffeisen-poc-instances`.

## Struttura del file `.ttl`

### 1. Header di commento

```turtle
# ============================================================
#  <dataset-name>.ttl
#  <Descrizione di cosa rappresentano queste istanze>
#  Conformante a TBox: <lista dei layer TBox usati>
# ============================================================
```

### 2. Prefissi: TBox + namespace locale

Importare i **prefissi TBox** che si useranno (uno o più dei `https://<authority>/ontology/<domain>/<package>#`) e definire un **namespace locale** per le istanze:

```turtle
@prefix stem: <https://dedagroup.it/ontology/cobol2java/stem#> .
@prefix call: <https://dedagroup.it/ontology/cobol2java/cobol/callable#> .
@prefix :     <urn:mde:abox:<dataset-name>#> .
```

Il **namespace vuoto** (`@prefix : <urn:mde:abox:...#> .`) è convenzione per le istanze locali al file. `:CNT` diventa `<urn:mde:abox:<dataset-name>#CNT>`. Pulito e tracciabile.

### 3. Istanze tipizzate

Ogni istanza deve dichiarare la classe TBox di appartenenza con `a` (alias di `rdf:type`):

```turtle
:CNT  a  stem:EntityStem ;
    stem:stemId        "CNT" ;
    stem:canonicalName "Contatore" ;
    stem:usageCount    47 ;
    stem:keyRole       "PK_CANDIDATE" .
```

### 4. Letterali tipati XSD

Usare il tipo esplicito quando ha senso:

```turtle
:binding001  a  tb:TransformationBinding ;
    tb:multiplicity  "ONE_TO_ONE" ;
    tb:generatedAt   "2026-05-26T14:30:00Z"^^xsd:dateTime ;
    tb:justification "Mapping diretto, nessuna decomposizione necessaria"@it .
```

- **Stringhe in italiano**: `"..."@it`
- **Stringhe in inglese**: `"..."@en` (se rilevante)
- **Interi**: senza virgolette (`47`, non `"47"`) — Turtle inferisce `xsd:integer`
- **Decimali**: con punto (`3.14`) — inferito `xsd:decimal`
- **Boolean**: `true` / `false` — inferito `xsd:boolean`
- **DateTime ISO 8601**: `"2026-05-26T14:30:00Z"^^xsd:dateTime`
- **Date**: `"2026-05-26"^^xsd:date`

### 5. Relazioni tra istanze (Object properties)

Usare il namespace vuoto per riferimenti interni al file:

```turtle
:DOC  a  stem:EntityStem ;
    stem:stemId         "DOC" ;
    stem:keyRole        "FK_CANDIDATE" ;
    stem:referencesKey  :CNT .   # ← riferimento ad altra istanza dello stesso file
```

Per riferimenti **cross-file** (es. istanza di questo file che punta a istanza di un altro), usare il namespace completo dell'altro file:

```turtle
@prefix other: <urn:mde:abox:other-dataset#> .

:foo  a  some:Class ;
      some:relatedTo  other:bar .
```

## Struttura del file `.md` companion

### Frontmatter

```yaml
---
title: ABox — <breve descrizione>
date: DD/MM/YYYY
---
```

### Sezioni obbligatorie

1. **`# <Titolo>`** — h1
2. **`## Cosa contiene`** — sintesi del dataset: quante istanze, di che classi, da quale documento sorgente sono state estratte
3. **`## File`** — embed live del `.ttl`:

   ````markdown
   ```text(./<dataset-name>.ttl)
   ```
   ````
4. **`## Prerequisito`** — riferimento alle TBox che devono essere caricate prima:

   > Le TBox `layer0-stem` e `layer1-cobol` devono essere già su `urn:mde:tbox:...`. Vedi [`../TBox/load-all.md`](../TBox/load-all.md).
5. **`## Carica su Fuseki`** — runnable PowerShell:

   ````markdown
   ```powershell
   # @param DATASET — nome dataset Fuseki (default: <project-name>)
   # @param FUSEKI_URI — base URL del server Fuseki (default: http://localhost:3030)
   $dataset = "<DATASET>"
   $fuseki = "<FUSEKI_URI>"
   $file = "ontology\ABox\<dataset-name>.ttl"
   $graph = "urn:mde:abox:<dataset-name>"

   $body = Get-Content $file -Raw -Encoding UTF8
   $uri  = "$fuseki/$dataset/data?graph=$([uri]::EscapeDataString($graph))"

   Invoke-RestMethod -Uri $uri -Method PUT -ContentType "text/turtle; charset=utf-8" -Body $body
   Write-Output "OK: $file -> $graph"
   ```
   ````
6. **`## Verifica` (obbligatoria)** — runnable SPARQL SELECT/ASK che dimostra utilità del grafo: una query mirata sul caso d'uso del dataset. Esempio per stem PK:

   ````markdown
   ```powershell
   $dataset = "<DATASET>"
   $fuseki = "<FUSEKI_URI>"
   $query = @"
   PREFIX stem: <https://dedagroup.it/ontology/cobol2java/stem#>
   SELECT ?stem ?canonicalName ?usageCount
   FROM <urn:mde:abox:<dataset-name>>
   WHERE {
       ?stem a stem:EntityStem ;
             stem:keyRole ?role ;
             stem:canonicalName ?canonicalName ;
             stem:usageCount ?usageCount .
       FILTER(?role IN ("PK", "PK_CANDIDATE"))
   }
   ORDER BY DESC(?usageCount)
   "@
   $uri = "$fuseki/$dataset/query"
   $resp = Invoke-RestMethod -Uri $uri -Method POST -ContentType "application/sparql-query" -Body $query -Headers @{ Accept = "application/sparql-results+json" }
   foreach ($r in $resp.results.bindings) { ... }
   ```
   ````

## Named graph convention

Il named graph IRI è **sempre** `urn:mde:abox:<dataset-name>`. Stesso pattern del TBox, ma con prefisso `abox:` invece di `tbox:`.

L'uso del named graph garantisce:
- **Re-load idempotente** via `PUT`
- **Drop selettivo** se si vuole rigenerare un singolo dataset senza toccare gli altri
- **Filtraggio in SPARQL** via `FROM <urn:mde:abox:<dataset-name>>` o `GRAPH <urn:...> { ... }`

## Tracciabilità della sorgente — PROV-O opzionale

Per dataset estratti automaticamente da un documento, è raccomandato annotare la **provenance**:

```turtle
@prefix prov: <http://www.w3.org/ns/prov#> .

:CNT  prov:wasDerivedFrom  <file:///docs/analisi/CWPGFG10.md> ;
      prov:wasGeneratedBy  :extraction-activity-001 .

:extraction-activity-001 a prov:Activity ;
    prov:startedAtTime "2026-05-26T14:30:00Z"^^xsd:dateTime ;
    prov:wasAssociatedWith :copilot-llm .
```

Non obbligatorio nella prima iterazione; aggiungere quando si vorrà tracciare *quale* documento ha generato *quale* istanza.

## Validazione

Validare il `.ttl` prima del caricamento:

```bash
riot --validate ontology/ABox/<dataset-name>.ttl
```

Silenzio = OK.

## Cosa NON fare

- ❌ Non includere definizioni di classi/proprietà (`owl:Class`, `owl:DatatypeProperty`) in un file ABox — quelle vivono solo in TBox
- ❌ Non inventare nuove proprietà non dichiarate nella TBox (RDF lo accetta ma il modello diventa inconsistente)
- ❌ Non usare i namespace pieni (lunghi) inline — definisci sempre prefissi all'inizio del file
- ❌ Non scrivere letterali numerici tra virgolette (es. `stem:usageCount "47"`) — il range XSD non matcha
- ❌ Non saltare il namespace locale `:` — è quello che identifica le istanze come "appartenenti a questo file"

## Esempio completo

Per un esempio concreto di ABox ben formata che applica queste convenzioni, vedi `ontology/ABox/example-stems.{ttl,md}` nel progetto Ontologies (3 istanze di `EntityStem` con tipi misti).

Vedi anche [[mde-tbox]] per la convenzione speculare di schema, e [[mde-features]] per la sintassi MDE-specific dei runnable e dei `text(...)` embed.
