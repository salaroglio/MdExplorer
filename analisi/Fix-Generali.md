# Fix Generali

## Problema: Visualizzazione documenti dopo modifica

### Descrizione del problema

Quando viene editato un documento all'interno dell'applicazione Angular di MdExplorer, il documento modificato non viene visualizzato immediatamente nell'area di visualizzazione principale. È necessario cliccare nuovamente sul nodo del documento nel md-tree per aggiornare la visualizzazione.

**Stesso problema si verifica anche con i nuovi documenti**: Quando viene creato un nuovo documento, questo appare correttamente nell'md-tree ma non viene generato automaticamente nel corpo principale dell'applicazione. Anche in questo caso è necessario un click manuale sul nodo nel tree per visualizzare il contenuto.

## Analisi Completa

### Componenti coinvolti

* **md-tree.component.ts** - Gestione tree e selezione nodi
* **main-content.component.ts** - Visualizzazione contenuto e gestione eventi SignalR
* **md-file.service.ts** - Servizio per gestione file e selezioni
* **md-navigation.service.ts** - Gestione navigazione e cronologia
* **server-messages.service.ts** - Gestione eventi SignalR
* **MonitorMDHostedService.cs** - Monitoraggio file system (backend)

### Flusso Normale di Navigazione (Funziona Correttamente)

1. **Click su file nel tree** → `md-tree.component.ts:getNode()`
2. **Naviga a route** → `/main/navigation/document`
3. **Aggiorna servizi**:
   * `mdFileService.setSelectedMdFileFromSideNav(node)` ✅
   * `navService.setNewNavigation(node)` ✅
4. **Aggiorna tree state**: `activeNode` e `selectedNode` ✅
5. **main-content** si sottoscrive e aggiorna iframe ✅

### Flusso di Modifica File (NON Funziona Completamente)

1. **File modificato** → `FileSystemWatcher` rileva cambiamento
2. **Backend invia SignalR** → evento `markdownfileischanged`
3. **server-messages.service** riceve evento
4. **main-content.markdownFileIsChanged()** viene chiamato
5. **PROBLEMA**: Chiama solo `setSelectedMdFileFromServer()` ❌
6. **Iframe viene aggiornato** ✅ ma **tree non sa del cambiamento** ❌

### Flusso di Creazione Nuovo File (STESSO PROBLEMA)

1. **Nuovo file creato** → `FileSystemWatcher` rileva creazione
2. **Backend invia SignalR** → evento simile a `markdownfileischanged`
3. **Tree viene aggiornato** → Il nuovo nodo appare nel tree ✅
4. **PROBLEMA**: Il nuovo file non viene automaticamente selezionato e visualizzato ❌
5. **Click necessario** → L'utente deve cliccare sul nuovo nodo per vederlo ❌

## Punto Critico del Problema

**File**: `main-content.component.ts:63-81`

Il metodo `markdownFileIsChanged()` manca delle chiamate essenziali:

```TypeScript
// ❌ CURRENT (Incompleto)
objectThis.service.setSelectedMdFileFromServer(data);

// ✅ NEEDED (Completo)
objectThis.service.setSelectedMdFileFromServer(data);
objectThis.service.setSelectedMdFileFromSideNav(data);  // MANCANTE
// objectThis.navService.setNewNavigation(data);        // MANCANTE (se accessibile)
```

### Conseguenze del Problema

1. **Iframe aggiornato** - Il contenuto nuovo viene mostrato (solo per file modificati)
2. **Tree non sincronizzato** - Non sa che il file corrente è cambiato
3. **Stato inconsistente** - `activeNode`/`selectedNode` mantengono riferimenti vecchi
4. **Click necessario** - Solo il click sul tree re-sincronizza tutto
5. **UX degradata** - Per i nuovi file, l'utente vede il nodo ma non il contenuto automaticamente

## Soluzioni Proposte

### Soluzione 1: Fix Immediato

Modificare `markdownFileIsChanged()` per includere la chiamata mancante. **Questo fix risolve sia il problema dei file modificati che quello dei nuovi file**:

```TypeScript
private markdownFileIsChanged(data: any, objectThis: MainContentComponent) {
    // ... codice esistente ...
    
    objectThis.service.setSelectedMdFileFromServer(data);
    objectThis.service.setSelectedMdFileFromSideNav(data);  // ✅ AGGIUNGERE
    
    // ... resto del codice ...
}
```

**Nota**: Verificare che il meccanismo per i nuovi file usi lo stesso evento SignalR o un evento simile che attivi lo stesso metodo.

### Soluzione 2: Miglioramento Architetturale

1. Creare un servizio centralizzato per la gestione dello stato del file corrente
2. Far sottoscrivere sia tree che main-content allo stesso observable
3. Garantire che tutti i componenti siano sempre sincronizzati

### Soluzione 3: Event Bus Pattern

Implementare un event bus per comunicazioni cross-component più robuste

## Evidenze dai Log del Browser

I log della console confermano l'analisi e mostrano ulteriori dettagli significativi:

### Console Log - Eventi SignalR

```log
📄 [MainContent] markdownFileIsChanged received: {
  "path": "Fix-Generali.md",
  "name": "Fix-Generali.md", 
  "relativePath": "Fix-Generali.md",
  "connectionId": null,
  "fullDirectoryPath": null
}
✅ [MainContent] Updated htmlSource to: ../api/mdexplorerFix-Generali.md?time=1749037319.498&connectionId=K7--8fK9irSIKFOLruhoLg
```

**Osservazioni critiche:**

1. **Eventi multipli**: Il metodo `markdownFileIsChanged()` viene chiamato ripetutamente (8+ volte per ogni modifica)
2. **Dati sufficienti**: I dati ricevuti da SignalR contengono `relativePath` e `path` che sono compatibili con `setSelectedMdFileFromSideNav()`
3. **Iframe aggiornato**: L'htmlSource viene correttamente aggiornato, ma il tree non viene sincronizzato

### Network Log - GET che vanno a vuoto

Dai network logs si nota che ci sono molte chiamate GET ripetitive:

```log
GET /api/gitservice/branches/feat/getdatatopull - Status 200 (ripetuta ~20+ volte)
GET /api/gitservice/branches/feat/getcurrentbranch - Status 200 (ripetuta ~15+ volte) 
```

**Problema identificato**: Queste chiamate Git vengono eseguite continuamente ogni pochi secondi, anche quando non necessario, indicando possibili:

* Polling inefficiente
* Memory leak nei timer/interval
* Mancanza di debouncing per le chiamate API

## Verifiche Necessarie

1. **Formato dati**: ✅ **CONFERMATO** - I dati da SignalR sono compatibili con `setSelectedMdFileFromSideNav()`
2. **Change Detection**: Controllare se servono trigger espliciti per Angular change detection
3. **Performance**: Valutare impatto di chiamate aggiuntive
4. **🚨 NUOVO**: Investigare il polling eccessivo delle API Git che potrebbe impattare le performance

## Priorità di Implementazione

### Problema Principale (Sincronizzazione Tree)

**ALTA** - Soluzione 1 (fix immediato) - Aggiungere `setSelectedMdFileFromSideNav()`
**MEDIA** - Test e validazione del fix
**BASSA** - Soluzioni architetturali avanzate

### Problema Secondario (Performance)

**MEDIA** - Investigare polling eccessivo API Git
**BASSA** - Implementare debouncing per le chiamate ripetitive
**BASSA** - Ottimizzare gestione eventi multipli SignalR

### Note Aggiuntive

* **Eventi multipli**: Considerare debouncing anche per `markdownFileIsChanged()` per evitare 8+ chiamate consecutive
* **Dati confermati**: Il fix proposto è tecnicamente fattibile poiché i dati SignalR contengono tutti i campi necessari

***

*Analisi completata - Pronto per implementazione*
