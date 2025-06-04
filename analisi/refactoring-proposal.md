# Refactoring proposal

# Sistema di Indicizzazione

## Problemi Identificati

1. **Duplicazione della logica di stato**: Lo stato di indicizzazione è gestito in due posti
2. **Accoppiamento forte**: Il MdTreeComponent contiene troppa logica di business
3. **Complessità della sincronizzazione**: Difficile mantenere sincronizzati nodo + Set

## Soluzioni Proposte

### 1. IndexingStateService (✅ Implementato)

* Centralizza tutta la logica di stato di indicizzazione
* Elimina la duplicazione tra nodo.isIndexed e Set di tracking
* Metodi specifici per operazioni comuni (rename, mark as indexed)

### 2. Semplificazione MdTreeComponent

**Prima:**

```TypeScript
// Logica complessa nel component
isFileIndexed(node: MdFile): boolean {
  const nodeIndexed = node.isIndexed || false;
  const setIndexed = this.indexedFilesSubject.value.has(node.fullPath);
  return nodeIndexed || setIndexed;
}

handleRule1ForceUpdate(filePath: string): void {
  // Aggiorna nodo + Set + change detection
}
```

**Dopo:**

```TypeScript
// Logica delegata al servizio
isFileIndexed(node: MdFile): boolean {
  return this.indexingState.isFileIndexed(node.fullPath);
}

handleRule1ForceUpdate(filePath: string): void {
  this.indexingState.markAsIndexed(filePath);
  this.changeDetectorRef.detectChanges();
}
```

### 3. Miglioramento MdFileService

**Prima:**

```TypeScript
changeDataStoreMdFiles(oldFile: MdFile, newFile: MdFile) {
  // Logica complessa di aggiornamento nodo + notifiche multiple
}
```

**Dopo:**

```TypeScript
changeDataStoreMdFiles(oldFile: MdFile, newFile: MdFile) {
  // Aggiorna solo il datastore
  this.updateNodeInDatastore(oldFile, newFile);
  
  // Delega la gestione dello stato al servizio
  this.indexingState.handleFileRename(oldFile.fullPath, newFile.fullPath);
  this.indexingState.markAsIndexed(newFile.fullPath);
  
  // Unica notifica
  this._mdFiles.next([...this.dataStore.mdFiles]);
}
```

## Vantaggi del Refactoring

1. **Single Responsibility**: Ogni classe ha una responsabilità specifica
2. **Testabilità**: Più facile testare la logica di indicizzazione in isolamento
3. **Manutenibilità**: Meno duplicazione, meno rischio di inconsistenze
4. **Riusabilità**: Il servizio può essere usato da altri componenti

## Piano di Implementazione

### Fase 1: Graduale Migration

1. ✅ Creare IndexingStateService
2. Aggiornare MdTreeComponent per usare il servizio
3. Aggiornare MdFileService per usare il servizio
4. Rimuovere logica duplicata

### Fase 2: Testing

1. Test unitari per IndexingStateService
2. Test di integrazione per verificare che Rule #1 funzioni
3. Test di regressione per altre funzionalità

### Fase 3: Cleanup

1. Rimozione di codice obsoleto
2. Aggiornamento documentazione
3. Review del codice

## Note sulla Backward Compatibility

Il refactoring può essere fatto in modo incrementale senza rompere la funzionalità esistente:

* Il servizio può coesistere con la logica attuale
* La migrazione può essere fatta un metodo alla volta
* I test possono verificare che entrambi i sistemi producano gli stessi risultati

<br />
