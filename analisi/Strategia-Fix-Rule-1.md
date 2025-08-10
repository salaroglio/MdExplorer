---
author: Carlo Salaroglio
document_type: Document
email: salaroglio@hotmail.com
title: 
date: 10/08/2025
word_section:
  write_toc: false
  document_header: ''
  template_section:
    inherit_from_template: ''
    custom_template: ''
    template_type: default
  predefined_pages: 
---
# Strategia Fix Rule 1

# File Rinominato Non Indicizzaton

## 📝 Problema Identificato

Quando la Rule #1 rileva che il nome file non corrisponde al titolo H1 e l'utente conferma la rinominazione:

1. ✅ Il backend rinomina correttamente il file
2. ✅ Il `MonitorMDHostedService` rileva il cambio (`Renamed` event)
3. ✅ Viene inviato evento `markdownFileCreated`
4. ✅ Il frontend aggiunge il nuovo file al tree
5. ❌ **PROBLEMA**: Il file appare non indicizzato e non cliccabile
6. ❌ **CAUSA**: `changeDataStoreMdFiles` non preserva `isIndexed`/`indexingStatus`

## 🎯 Strategia di Risoluzione

### Step 1: Fix del Servizio `changeDataStoreMdFiles` ⭐ PRIORITÀ ALTA

**File**: `MdExplorer/client2/src/app/md-explorer/services/md-file.service.ts`

**Problema**: Il metodo `changeDataStoreMdFiles` aggiorna solo nome/path ma non preserva le proprietà di indicizzazione.

**Fix**:

```TypeScript
changeDataStoreMdFiles(oldFile: MdFile, newFile: MdFile) {
  var returnFound = this.searchMdFileIntoDataStore(this.dataStore.mdFiles, oldFile);
  var leaf = returnFound[0];
  
  // PRESERVA le proprietà di indicizzazione esistenti
  const wasIndexed = leaf.isIndexed;
  const indexingStatus = leaf.indexingStatus;
  
  // Aggiorna le proprietà del file
  leaf.name = newFile.name;
  leaf.fullPath = newFile.fullPath;
  leaf.path = newFile.path;
  leaf.relativePath = newFile.relativePath;
  
  // IMPORTANTE: Preserva lo stato di indicizzazione
  leaf.isIndexed = wasIndexed ?? true; // Mantieni stato esistente o default true
  leaf.indexingStatus = indexingStatus ?? 'completed';
  
  this._mdFiles.next(Object.assign({}, this.dataStore).mdFiles);
  this._serverSelectedMdFile.next(returnFound);
}
```

### Step 2: Gestione Doppio Evento ⭐ PRIORITÀ MEDIA

**File**: `MdExplorer/client2/src/app/md-explorer/components/md-tree/md-tree.component.ts`

**Problema**: Quando viene rinominato un file, possono arrivare sia l'evento da `changeDataStoreMdFiles` che l'evento da `markdownFileCreated` da SignalR.

**Fix**: Aggiungere deduplicazione nell'handler `handleNewMarkdownFileCreated`:

```TypeScript
private handleNewMarkdownFileCreated(fileData: any): void {
  // Controlla se il file esiste già (caso rinominazione)
  const existingFile = this.findNodeByPath(fileData.fullPath);
  if (existingFile) {
    console.log('🔄 File già esistente, probabile rinominazione. Aggiornando proprietà...');
    // Aggiorna le proprietà di indicizzazione invece di aggiungere nuovo nodo
    existingFile.isIndexed = fileData.isIndexed ?? true;
    existingFile.indexingStatus = fileData.indexingStatus ?? 'completed';
    this.changeDetectorRef.markForCheck();
    return;
  }
  
  // Procedi con logica normale per nuovi file...
}
```

### Step 3: Tracking nel BehaviorSubject ⭐ PRIORITÀ BASSA

**File**: `MdExplorer/client2/src/app/md-explorer/services/md-file.service.ts`

**Problema**: Il `BehaviorSubject` per i file indicizzati potrebbe non essere aggiornato durante la rinominazione.

**Fix**: Aggiornare anche il tracking nel `changeDataStoreMdFiles`:

```TypeScript
changeDataStoreMdFiles(oldFile: MdFile, newFile: MdFile) {
  // ... existing code ...
  
  // Aggiorna anche il tracking se il file era indicizzato
  if (leaf.isIndexed) {
    // Questo può essere gestito tramite l'event system esistente
    console.log('📋 File rinominato ma mantiene indicizzazione:', newFile.name);
  }
}
```

## 🧪 Piano di Test

### Test 1: Scenario Base Rule #1

1. ✅ Crea file `test-wrong.md` con contenuto `# Correct Title`
2. ✅ Verifica che Rule #1 si attivi e mostri dialog
3. ✅ Conferma rinominazione a `Correct-Title.md`
4. ✅ **VERIFICARE**: File rinominato deve essere immediatamente cliccabile
5. ✅ **VERIFICARE**: Nessuna opacità ridotta o cursore "not-allowed"

### Test 2: Scenario File in Sottocartella

1. ✅ Crea `docs/another-wrong.md` con `# Right Name`
2. ✅ Ripeti processo di rinominazione
3. ✅ **VERIFICARE**: Gerarchia mantenuta correttamente

### Test 3: Scenario Edge Cases

1. ✅ File con caratteri speciali nel titolo
2. ✅ File con titoli molto lunghi
3. ✅ Annullamento dell'operazione di rinominazione

## 📊 Criteri di Successo

### Must Have

* ✅ File rinominato tramite Rule #1 immediatamente cliccabile
* ✅ Stato `isIndexed = true` preservato durante rinominazione
* ✅ Nessun doppio inserimento nel tree view
* ✅ NavigationArray aggiornato correttamente

### Should Have

* ✅ Transizione visuale smooth durante rinominazione
* ✅ Notifica di successo per operazione completata
* ✅ Logging dettagliato per debug

### Could Have

* ✅ Undo operation per rinominazione
* ✅ Preview del nuovo nome prima della conferma

## 🚀 Implementazione Prioritizzata

### Sprint Day 1 (Oggi)

1. **Step 1**: Fix `changeDataStoreMdFiles` (30 min)
2. **Test 1**: Scenario base (15 min)
3. **Step 2**: Deduplicazione eventi (45 min)
4. **Test 2**: Scenario sottocartella (15 min)
5. **Test 3**: Edge cases (30 min)

### Sprint Day 2 (Se necessario)

1. **Step 3**: BehaviorSubject tracking (15 min)
2. **Refinement**: Miglioramenti UX (30 min)
3. **Documentation**: Aggiornare documentazione (15 min)

## 📋 Checklist Implementazione

### Preparazione

* [ ] Backup del file `md-file.service.ts`
* [ ] Setup ambiente di test con file di esempio
* [ ] Verifica funzionamento attuale Rule #1

### Implementazione Step 1

* [ ] Modificare `changeDataStoreMdFiles`
* [ ] Aggiungere preservazione `isIndexed`/`indexingStatus`
* [ ] Aggiungere logging per debug
* [ ] Test immediato con scenario base

### Implementazione Step 2

* [ ] Modificare `handleNewMarkdownFileCreated`
* [ ] Aggiungere deduplicazione eventi
* [ ] Test con rinominazione Rule #1
* [ ] Verificare nessun doppio inserimento

### Testing & Validation

* [ ] Test scenario base - funziona ✅
* [ ] Test scenario sottocartella - funziona ✅
* [ ] Test edge cases - funziona ✅
* [ ] Test regressione funzionalità esistenti

### Finalizzazione

* [ ] Commit con messaggio dettagliato
* [ ] Aggiornamento documentazione punti-aperti.md
* [ ] Aggiornamento sprint-2025-06-02.md
* [ ] Demo al team/stakeholder

## 💡 Note Tecniche

### Flusso Attuale (Broken)

```
File modificato → Rule #1 check → Dialog → Confirm → 
Backend rename → MonitorMD detect → SignalR event → 
Tree update → ❌ File non indicizzato
```

### Flusso Corretto (Target)

```
File modificato → Rule #1 check → Dialog → Confirm → 
Backend rename → changeDataStoreMdFiles → ✅ Preserva indicizzazione →
MonitorMD detect → SignalR event → Deduplicazione → 
✅ File cliccabile immediatamente
```

### Possibili Rischi

1. **Race condition**: Eventi SignalR vs aggiornamento locale
2. **Duplicazione**: File mostrato due volte nel tree
3. **Stato inconsistente**: BehaviorSubject non allineato

### Mitigazioni

1. **Deduplicazione**: Check esistenza prima di aggiungere
2. **Logging**: Tracciare tutti gli eventi per debug
3. **Validation**: Verificare stato finale dopo ogni operazione

<br />
