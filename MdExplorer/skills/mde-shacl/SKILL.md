---
name: mde-shacl
description: Convention for generating SHACL constraint files (Shapes) inside an MDE project. Use when authoring validation rules for a TBox in `ontology/Shapes/`: produces a `.shapes.ttl` with NodeShape/PropertyShape definitions plus a `.md` companion with description + runnable PowerShell that loads the shapes into Apache Jena Fuseki and runs `shacl validate` against ABox data. Triggers on "add SHACL constraints", "validate ontology", "define cardinality rules", "enforce enum values", "model invariants", "constrain ABox", "shacl shape".
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

# SHACL shapes generation convention

Convenzione per generare file di **vincoli SHACL** che validano dati RDF contro lo schema definito in TBox. Si applica quando un AI agent deve produrre un nuovo file `.shapes.ttl` che esprime cardinalità, enum, invarianti, vincoli polimorfici.

## Cosa è SHACL

**SHACL** (Shapes Constraint Language, W3C 2017) descrive **forme** che i dati RDF devono rispettare. Una shape ha tre concetti chiave:

- **NodeShape** — vincolo applicato a un nodo (istanza)
- **PropertyShape** — vincolo applicato a una proprietà di un nodo
- **Constraint component** — il vincolo specifico (es. `sh:minCount`, `sh:datatype`, `sh:in`)

A differenza di OWL (che descrive *cosa esiste*), SHACL descrive *cosa è valido*. La differenza è critica:

- OWL: `stem:EntityStem a owl:Class` → "esiste una classe EntityStem"
- SHACL: `:EntityStemShape sh:targetClass stem:EntityStem ; sh:property [ sh:path stem:stemId ; sh:minCount 1 ]` → "ogni EntityStem deve avere almeno uno stemId"

## Layout obbligatorio dei file

```
ontology/Shapes/<layer-name>.shapes.ttl    ← le shape
ontology/Shapes/<layer-name>.shapes.md     ← descrizione + runnable load + runnable validate
```

`<layer-name>` matcha quello della TBox che si sta vincolando. Esempio: la TBox `ontology/TBox/layer0-stem.ttl` ha shape in `ontology/Shapes/layer0-stem.shapes.ttl`.

## Struttura del file `.shapes.ttl`

### 1. Header

```turtle
# ============================================================
#  <layer-name>.shapes.ttl
#  Vincoli SHACL per la TBox <ontology/TBox/<layer-name>.ttl>
# ============================================================
```

### 2. Prefissi

Sempre:

```turtle
@prefix sh:   <http://www.w3.org/ns/shacl#> .
@prefix rdf:  <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .
@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> .
@prefix xsd:  <http://www.w3.org/2001/XMLSchema#> .
```

Più i prefissi della TBox che si sta vincolando (es. `stem:`, `call:`, ecc.) e il namespace locale:

```turtle
@prefix stem: <https://dedagroup.it/ontology/cobol2java/stem#> .
@prefix :     <urn:mde:shapes:<layer-name>#> .
```

### 3. NodeShape — vincoli a livello di istanza

Pattern base:

```turtle
:EntityStemShape  a  sh:NodeShape ;
    sh:targetClass  stem:EntityStem ;
    sh:property [...] ;
    sh:property [...] .
```

`sh:targetClass` dice "applica questa shape a tutte le istanze di stem:EntityStem".

### 4. PropertyShape — vincoli su singole proprietà

Vincoli più comuni, con esempi pratici:

#### Cardinalità

```turtle
sh:property [
    sh:path     stem:stemId ;
    sh:minCount 1 ;       # obbligatorio
    sh:maxCount 1 ;       # massimo uno
] ;
```

Cardinalità tipiche:
- `0..1` = `sh:maxCount 1`
- `1..1` = `sh:minCount 1 ; sh:maxCount 1`
- `1..*` = `sh:minCount 1`
- `n..m` = `sh:minCount n ; sh:maxCount m`

#### Tipo del valore

```turtle
sh:property [
    sh:path     stem:usageCount ;
    sh:datatype xsd:integer ;
] ;
```

Per tipi misti (es. stringa con o senza language tag) usare `sh:nodeKind sh:Literal`:

```turtle
sh:property [
    sh:path        stem:meaning ;
    sh:nodeKind    sh:Literal ;    # accetta sia "foo" sia "foo"@it
] ;
```

#### Enumerazione

```turtle
sh:property [
    sh:path stem:keyRole ;
    sh:in   ( "NONE" "PK_CANDIDATE" "FK_CANDIDATE" "PK" "FK" ) ;
] ;
```

#### Range numerico

```turtle
sh:property [
    sh:path stem:usageCount ;
    sh:minInclusive 0 ;
    # sh:maxInclusive 1000000 ;   # se serve un limite superiore
] ;
```

#### Pattern regex su stringa

```turtle
sh:property [
    sh:path jc:packageName ;
    sh:datatype xsd:string ;
    sh:pattern  "^[a-z][a-z0-9.]*$" ;
    sh:message  "Package name must be lowercase dot-separated"@en ;
] ;
```

#### Riferimento a istanza di classe

```turtle
sh:property [
    sh:path  stem:referencesKey ;
    sh:class stem:EntityStem ;
    sh:maxCount 1 ;
] ;
```

#### Confronto inter-proprietà (invariante)

```turtle
sh:property [
    sh:path                stem:confirmedCount ;
    sh:lessThanOrEquals    stem:usageCount ;
    sh:message             "confirmedCount non può superare usageCount"@it ;
] ;
```

Altri operatori: `sh:lessThan`, `sh:equals`, `sh:disjoint`.

### 5. Disgiunzioni (domain/range polimorfico)

Per property come `hasParam` (domain Callable OR ExternalCall) usare `sh:or`:

```turtle
:HasParamDomainShape  a  sh:NodeShape ;
    sh:targetSubjectsOf  call:hasParam ;
    sh:or (
        [ sh:class call:Callable ]
        [ sh:class ec:ExternalCall ]
    ) ;
    sh:message "hasParam può uscire solo da Callable o ExternalCall"@it .
```

`sh:targetSubjectsOf <property>` mira tutti i soggetti di triple con quella property — utile per polimorfismi su domain.

Per polimorfismi su range usare `sh:targetObjectsOf <property>` analogamente.

### 6. Vincoli condizionali (SPARQL-based)

Per regole come "se keyRole = FK, allora referencesKey deve esistere":

```turtle
:FkMustReferenceShape  a  sh:NodeShape ;
    sh:targetClass  stem:EntityStem ;
    sh:sparql [
        sh:message  "Se keyRole = FK, referencesKey è obbligatorio"@it ;
        sh:select """
            PREFIX stem: <https://dedagroup.it/ontology/cobol2java/stem#>
            SELECT $this WHERE {
                $this stem:keyRole "FK" .
                FILTER NOT EXISTS { $this stem:referencesKey ?ref }
            }
        """ ;
    ] .
```

`sh:select` è una query SPARQL SELECT che ritorna tutti i `$this` (le istanze) che violano la regola. La proprietà `sh:message` è il messaggio di errore mostrato dal validator.

### 7. Vietare istanze dirette di classi astratte

Per `Callable` e `WorkflowNode` (astratte per convenzione):

```turtle
:CallableNoDirectInstanceShape  a  sh:NodeShape ;
    sh:targetClass  call:Callable ;
    sh:not [
        sh:not [
            sh:or (
                [ sh:class call:CobolProgram ]
                [ sh:class call:CobolFunction ]
                [ sh:class call:CopyFunction ]
            )
        ]
    ] ;
    sh:message "Callable è astratta: usa CobolProgram, CobolFunction o CopyFunction"@it .
```

Doppio `sh:not` perché SHACL non ha "must be one of these subclasses" diretto.

## Struttura del file `.md` companion

### Frontmatter

```yaml
---
title: Shapes — <layer-name>
date: DD/MM/YYYY
---
```

### Sezioni obbligatorie

1. **`# <Titolo>`** — h1
2. **`## Cosa vincola`** — elenco breve dei constraint principali (cardinalità, enum, invarianti)
3. **`## File`** — embed live via `text(...)`:

   ````markdown
   ```text(./<layer-name>.shapes.ttl)
   ```
   ````
4. **`## Prerequisito`** — link alla TBox e alla ABox da validare:

   > Carica `ontology/TBox/<layer-name>.ttl` e `ontology/ABox/<dataset>.ttl` prima di validare.
5. **`## Carica le shapes su Fuseki`** — runnable PowerShell (PUT su Graph Store Protocol):

   ````markdown
   ```powershell
   # @param DATASET — nome dataset Fuseki (default: <project-name>)
   # @param FUSEKI_URI — base URL del server Fuseki (default: http://localhost:3030)
   $dataset = "<DATASET>"
   $fuseki = "<FUSEKI_URI>"
   $file = "ontology\Shapes\<layer-name>.shapes.ttl"
   $graph = "urn:mde:shapes:<layer-name>"

   $body = Get-Content $file -Raw -Encoding UTF8
   $uri  = "$fuseki/$dataset/data?graph=$([uri]::EscapeDataString($graph))"
   Invoke-RestMethod -Uri $uri -Method PUT -ContentType "text/turtle; charset=utf-8" -Body $body
   "OK: $file -> $graph"
   ```
   ````
6. **`## Valida localmente (Jena CLI)`** — runnable PowerShell che esegue `shacl validate`. **Importante**: Jena 6 non accetta backslash negli IRI, normalizzare i path con `.Replace('\','/')`:

   ````markdown
   ```powershell
   # @param SHAPES — file delle shapes (default: ontology/Shapes/<layer-name>.shapes.ttl, type: file)
   # @param DATA — file dati ABox da validare (default: ontology/ABox/<dataset>.ttl, type: file)
   $shape = "<SHAPES>".Replace('\','/')
   $data  = "<DATA>".Replace('\','/')
   shacl validate --shapes $shape --data $data
   ```
   ````

   Output atteso quando i dati conformano: `Conforms: true`. Altrimenti, lista delle violazioni con path e messaggio.

## Named graph convention

Il named graph IRI è **sempre** `urn:mde:shapes:<layer-name>`. Stesso pattern di TBox/ABox.

## Validazione del file `.shapes.ttl` stesso

Prima di caricarlo, validare la sintassi Turtle:

```bash
riot --validate ontology/Shapes/<layer-name>.shapes.ttl
```

Silenzio = OK. SHACL è esso stesso espresso come triple RDF, quindi `riot` valida la sintassi del file (non il contenuto delle shape — quello è semantico).

## Cosa NON fare

- ❌ Non mettere shape SHACL dentro un file TBox `.ttl` — separare i due artefatti
- ❌ Non saltare `sh:message` sui vincoli che non sono auto-esplicativi — il messaggio è quello che l'utente vede quando una shape catch'a un'istanza non conforme
- ❌ Non usare `sh:targetClass` su classi astratte direttamente — usa `sh:targetClass` sulla classe astratta e poi sottoshape per le sottoclassi specifiche
- ❌ Non scrivere shape che duplicano vincoli OWL (es. `owl:DatatypeProperty` con `rdfs:range xsd:string` viene già "documentato" in TBox; SHACL serve per vincoli che OWL non esprime: cardinalità, enum, regex, invarianti inter-proprietà)
- ❌ Non dimenticare di aggiungere la nuova shape a `Shapes/load-all.md` quando ne crei una nuova

## Aggiornare `load-all.md`

Quando aggiungi un nuovo file `.shapes.ttl`, aggiungilo anche al bootstrap multipart in `ontology/Shapes/load-all.md`. Pattern:

```powershell
$files = @(
    @{ File = "ontology\Shapes\<layer-name>.shapes.ttl"; Graph = "urn:mde:shapes:<layer-name>" }
    # ... altre shape
)
```

## Esempio completo

Per esempi concreti che applicano tutte queste convenzioni, vedi i file `ontology/Shapes/layer*-*.shapes.{ttl,md}` nel progetto Ontologies.

Vedi anche [[mde-tbox]] per la convenzione speculare delle classi e [[mde-abox]] per le istanze, e [[mde-features]] per la sintassi MDE dei runnable e dei `text(...)` embed.
