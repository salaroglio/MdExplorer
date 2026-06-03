---
name: mde-tbox
description: Convention for generating TBox (schema/ontology) files inside an MDE project. Use when authoring a new OWL ontology layer in `ontology/TBox/`: produces a `.ttl` with class and property definitions plus a `.md` companion with description + runnable PowerShell that loads the file into Apache Jena Fuseki via Graph Store Protocol PUT. Triggers on "create new TBox layer", "generate OWL ontology", "define schema classes", "model domain in OWL", "new ontology layer".
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

# TBox generation convention

Convenzione per generare file di **schema OWL** (TBox = Terminological Box) all'interno di un progetto MDE configurato per Apache Jena Fuseki. Si applica quando un AI agent (Copilot, mde-skillcreator, qualunque altro) deve produrre un nuovo file `.ttl` che definisce classi e proprietà di un dominio.

## Cosa è una TBox

In semantic web, la **TBox** è la parte dell'ontologia che descrive lo **schema**: le classi, le proprietà, le gerarchie, i vincoli di tipo. Distinta dalla **ABox** che contiene le **istanze** (i dati concreti). La TBox cambia raramente e va caricata una volta sola; la ABox cresce continuamente.

## Layout obbligatorio dei file

Ogni TBox produce **due file** affiancati:

```
ontology/TBox/<layer-name>.ttl    ← il file Turtle con classi e proprietà
ontology/TBox/<layer-name>.md     ← descrizione + runnable di caricamento
```

`<layer-name>` deve essere **kebab-case**, descrittivo, breve. Esempi: `layer0-stem`, `layer1-cobol`, `payment-domain`, `cobol-workflow`.

## Struttura del file `.ttl`

### 1. Header di commento

```turtle
# ============================================================
#  <layer-name>.ttl
#  <Breve descrizione di cosa modella questa TBox>
#  Riferimento documento sorgente: <path al .md descrittivo o doc esterno>
# ============================================================
```

### 2. Prefissi standard

Sempre presenti:

```turtle
@prefix owl:   <http://www.w3.org/2002/07/owl#> .
@prefix rdf:   <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .
@prefix rdfs:  <http://www.w3.org/2000/01/rdf-schema#> .
@prefix xsd:   <http://www.w3.org/2001/XMLSchema#> .
```

### 3. Sub-namespace per classe

Quando lo schema sorgente ha più classi i cui attributi **collidono nei nomi** (es. `Callable.name` e `TypedParam.name` entrambi chiamati `name`), declarare **una sub-namespace per classe** così i local-name del documento UML/sorgente si preservano identici:

```turtle
@prefix call: <https://<authority>/ontology/<domain>/<package>/callable#> .
@prefix tp:   <https://<authority>/ontology/<domain>/<package>/typedparam#> .
@prefix wf:   <https://<authority>/ontology/<domain>/<package>/workflow#> .
```

Authority esempio: `dedagroup.it`. Domain esempio: `cobol2java`. Package esempio: `cobol`.

**Non rinominare proprietà** per disambiguare (es. NON usare `paramName` al posto di `name`); usa sub-namespaces.

### 4. Classe + label + comment

```turtle
<prefix>:<ClassName>  a  owl:Class ;
    rdfs:label    "Human-readable EN"@en , "Italiano leggibile"@it ;
    rdfs:comment  "Descrizione lunga di cosa rappresenta questa classe."@it .
```

Sempre `@en` e `@it` per le label se possibile (il dominio Deda è italiano ma standard semantic web è inglese).

### 5. Sottoclassi via `rdfs:subClassOf`

```turtle
<prefix>:<SubClass>  a  owl:Class ;
    rdfs:subClassOf  <prefix>:<SuperClass> ;
    rdfs:label       "Nome sottoclasse"@it .
```

### 6. Proprietà di valore (`owl:DatatypeProperty`)

```turtle
<prefix>:<propName>  a  owl:DatatypeProperty ;
    rdfs:domain  <prefix>:<ClassName> ;
    rdfs:range   xsd:string .
```

Range typical: `xsd:string`, `xsd:integer`, `xsd:decimal`, `xsd:boolean`, `xsd:dateTime`, `xsd:date`.

### 7. Proprietà di riferimento (`owl:ObjectProperty`)

```turtle
<prefix>:<relName>  a  owl:ObjectProperty ;
    rdfs:domain  <prefix>:<SourceClass> ;
    rdfs:range   <prefix>:<TargetClass> .
```

Per relazioni con **attributi propri** (es. "include con atLine"), usa la **reificazione**: crea una classe-cerniera che porta gli attributi:

```turtle
<prefix>:IncludeStatement  a  owl:Class ;
    rdfs:comment "Reificazione di 'CobolProgram include CopyBook' con atLine."@it .

<prefix>:includedBy   a owl:ObjectProperty ;
    rdfs:domain <prefix>:IncludeStatement ; rdfs:range <prefix>:CobolProgram .
<prefix>:includesCopy a owl:ObjectProperty ;
    rdfs:domain <prefix>:IncludeStatement ; rdfs:range <prefix>:CopyBook .
<prefix>:atLine       a owl:DatatypeProperty ;
    rdfs:domain <prefix>:IncludeStatement ; rdfs:range xsd:integer .
```

### 8. Domini/range polimorfici

Se una property serve più classi (es. `hasParam` valido sia su `Callable` sia su `ExternalCall`), **omettere `rdfs:domain`** (o `rdfs:range`) e annotare:

```turtle
<prefix>:hasParam  a  owl:ObjectProperty ;
    rdfs:range   <prefix>:TypedParam ;
    rdfs:comment "Dominio polimorfico: Callable | ExternalCall. Vincolo via SHACL."@it .
```

I vincoli polimorfici si esprimeranno via SHACL in una fase successiva.

### 9. Convenzioni di naming

- **Reserved keywords**: NON usare `then`, `else` come local-name di property (rompono parser/code-gen di alcuni tool). Usa `thenBranch`/`elseBranch` con `rdfs:label "then"@en` per preservare la denominazione semantica.
- **Enumerazioni**: per ora `xsd:string` libero + commento "Valori attesi: 'A' | 'B' | 'C'. Vincolo via SHACL." (le enum vere si esprimono in SHACL).

## Struttura del file `.md` companion

### Frontmatter

```yaml
---
title: TBox — <Breve descrizione>
date: DD/MM/YYYY
---
```

### Sezioni obbligatorie

1. **`# <Titolo descrittivo>`** — h1
2. **`## Cosa contiene`** — paragrafo che riassume classi e proprietà presenti, con conta delle triple totali (ottenuta con `riot --syntax=ttl --output=N-Triples`)
3. **`## File`** — embed live del `.ttl` via la feature MDE `text(...)`:

   ````markdown
   ```text(./<layer-name>.ttl)
   ```
   ````
4. **`## Carica su Fuseki`** — runnable PowerShell con `@param DATASET` e `@param FUSEKI_URI` che esegue PUT su Graph Store Protocol:

   ````markdown
   ```powershell
   # @param DATASET — nome dataset Fuseki (default: <project-name>)
   # @param FUSEKI_URI — base URL del server Fuseki (default: http://localhost:3030)
   $dataset = "<DATASET>"
   $fuseki = "<FUSEKI_URI>"
   $file = "ontology\TBox\<layer-name>.ttl"
   $graph = "urn:mde:tbox:<layer-name>"

   $body = Get-Content $file -Raw -Encoding UTF8
   $uri  = "$fuseki/$dataset/data?graph=$([uri]::EscapeDataString($graph))"

   Invoke-RestMethod -Uri $uri -Method PUT -ContentType "text/turtle; charset=utf-8" -Body $body
   Write-Output "OK: $file -> $graph"
   ```
   ````
5. **`## Verifica` (opzionale ma raccomandata)** — runnable SPARQL ASK o SELECT che conferma che le entità chiave (es. una classe specifica) siano effettivamente nel grafo.

## Named graph convention

Il named graph IRI è **sempre** `urn:mde:tbox:<layer-name>` dove `<layer-name>` è il basename del file senza estensione.

Rationale: usare `PUT` su Graph Store Protocol con questo IRI rende il caricamento **idempotente** — re-eseguire il runnable sostituisce completamente il contenuto del named graph senza accumulare duplicati o cross-contaminare altri layer.

## Validazione

Prima di marcare la TBox come completata, validare con `riot`:

```bash
riot --validate ontology/TBox/<layer-name>.ttl
```

Silenzio = OK. Errori = riportano riga/colonna del problema. Il file deve essere syntactically valido prima del caricamento.

## Cosa NON fare

- ❌ Non mettere file `.ttl` di esempio/test fuori da `ontology/TBox/` se sono parte dello schema del progetto
- ❌ Non usare lo stesso namespace per classi diverse che condividono nomi di proprietà (gestire via sub-namespace)
- ❌ Non includere istanze concrete in un file TBox (le istanze vanno in `ontology/ABox/`)
- ❌ Non usare `rdfs:Class` (è il pattern RDFS pre-OWL; usare `owl:Class` per chiarezza)
- ❌ Non saltare `rdfs:label` e `rdfs:comment` — sono fondamentali per la documentazione e per i tool di visualizzazione (Protégé, WebVOWL)

## Aggiornare il bootstrap `load-all.md`

Quando aggiungi un nuovo TBox layer, **aggiornare anche** `ontology/TBox/load-all.md` per includerlo nel bootstrap multipart. Pattern:

```powershell
$files = @(
    @{ File = "ontology\TBox\<layer-name>.ttl"; Graph = "urn:mde:tbox:<layer-name>" }
    # ... gli altri layer
)
```

## Esempio completo

Per un esempio concreto di TBox ben formata che applica tutte queste convenzioni, vedi i file `ontology/TBox/layer0-stem.{ttl,md}` ... `layer-bridges.{ttl,md}` nel progetto Ontologies (5 layer dell'ontologia COBOL → Java).
