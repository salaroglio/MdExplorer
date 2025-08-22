---
author: Carlo Salaroglio
document_type: Document
email: salaroglio@hotmail.com
title: 
date: 22/08/2025
word_section:
  write_toc: false
  document_header: ''
  template_section:
    inherit_from_template: ''
    custom_template: ''
    template_type: default
  predefined_pages: 
---
# Sprint: Implementazione funzionalità "Toc Directory"

## Obiettivo
Aggiungere una nuova voce nel context menu delle directory chiamata "Toc Directory" che permette di aprire e visualizzare un file con estensione `.md.directory` che contiene il sommario della directory.

## Analisi del flusso esistente

### Flusso attuale per apertura documenti
Il sistema segue questo percorso quando un utente clicca su un file markdown:
1. L'utente clicca su un file nel componente md-tree
2. Il metodo getNode() naviga alla route /main/navigation/document e chiama mdFileService.setSelectedMdFileFromSideNav()
3. MainContentComponent ascolta l'observable selectedMdFileFromSideNav e costruisce l'URL /api/mdexplorer/{relativePath}
4. MdExplorerController processa la richiesta e renderizza il markdown (accetta solo estensioni .md)

### Flusso per "Open directory on File Explorer"
Il sistema attuale gestisce l'apertura delle directory nel file explorer attraverso:
1. Context menu visualizza l'opzione per le directory
2. Il metodo openFolderOn() chiama mdFileService.openFolderOnFileExplorer()
3. MdFileService invia POST a /api/mdfiles/OpenFolderOnFileExplorer

## Implementazione richiesta

### 1. Modifica del template HTML
Nel file md-tree.component.html, aggiungere il nuovo pulsante dopo "Open directory on File Explorer":
- Condizione di visualizzazione per directory e root
- Testo del pulsante: "Toc Directory"
- Handler del click che invoca il metodo dedicato

### 2. Aggiunta metodo nel componente TypeScript
Creare il metodo openTocDirectory() in md-tree.component.ts che:
- Costruisce un oggetto MdFile con nome directory + .md.directory
- Imposta il percorso relativo corretto (percorso della directory + nome directory + .md.directory)
- Naviga alla route /main/navigation/document
- Chiama mdFileService.setSelectedMdFileFromSideNav() con il file

### 3. Modifica del controller backend
Modificare MdExplorerController.cs per accettare l'estensione .md.directory:
- Aggiornare la condizione di controllo estensioni per includere .md.directory
- Il controller leggerà il file .md.directory come un normale file markdown

### 4. Gestione del contenuto .md.directory
Il backend dovrà:
- Riconoscere l'estensione speciale .md.directory
- Se il file non esiste, crearlo automaticamente con il titolo corrispondente al nome della directory
- Una volta che il file esiste, trattarlo esattamente come un file .md normale
- Leggere il contenuto del file .md.directory dal file system
- Processare e restituire il contenuto come farebbe con qualsiasi file markdown

## File da modificare

### Frontend Angular
- MdExplorer/client2/src/app/md-explorer/components/md-tree/md-tree.component.html
- MdExplorer/client2/src/app/md-explorer/components/md-tree/md-tree.component.ts

### Backend .NET
- MdExplorer/Controllers/MdExplorerController.cs

## Flusso finale atteso

1. Utente fa click destro su una directory
2. Seleziona "Toc Directory" dal context menu
3. Il sistema costruisce il percorso al file .md.directory della directory
4. Il file viene aperto come un normale documento markdown
5. Il backend legge il file .md.directory dal file system
6. Il contenuto viene visualizzato nell'iframe di MainContentComponent

### Esempio pratico
Se l'utente fa click destro sulla directory `Documentation` che si trova in `/project/Documentation/`:
- Il sistema costruirà il percorso: `/project/Documentation/Documentation.md.directory`
- Se il file non esiste, verrà creato automaticamente con il titolo `# Documentation`
- Questo file verrà aperto e visualizzato come un normale markdown
- Il contenuto del file `Documentation.md.directory` apparirà nell'iframe

## Note tecniche

- L'estensione .md.directory è composta e richiede gestione speciale nel backend
- Se il file .md.directory non esiste, viene creato automaticamente con contenuto iniziale
- Il contenuto iniziale del file creato sarà un titolo H1 con il nome della directory
- Il percorso deve mantenere la struttura: {directory_path}/{directory_name}.md.directory
- La visualizzazione utilizza lo stesso meccanismo dei file markdown normali

## Testing

1. Verificare che il pulsante appaia solo per le directory
2. Testare che il click costruisca correttamente il percorso al file .md.directory
3. Confermare che il backend accetti l'estensione .md.directory
4. Validare che il contenuto del file .md.directory venga letto e visualizzato correttamente
5. Testare su directory con nomi speciali (spazi, caratteri speciali)

## Considerazioni sulla compatibilità multipiattaforma

- Utilizzare Path.Combine() per costruire i percorsi nel backend
- Mantenere forward slash per i percorsi nell'URL HTTP
- Testare su Windows, Linux e macOS