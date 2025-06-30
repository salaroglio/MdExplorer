# Test della Dialog di Commit in Electron

## Problema Risolto
La messagebox per il commit Git non appariva quando l'applicazione Angular era compilata dentro Electron perché utilizzava `window.prompt()` nativo del browser, che non funziona correttamente in Electron.

## Soluzione Implementata
1. Creato un nuovo componente Angular Material Dialog: `CommitMessageDialogComponent`
2. Sostituito l'uso di `prompt()` con `MatDialog.open()` nel componente toolbar
3. Il componente dialog include:
   - Un campo textarea per il messaggio di commit
   - Pulsanti Cancel e Commit
   - Validazione per evitare commit con messaggi vuoti
   - Supporto per Ctrl+Enter per confermare rapidamente

## Test
Per testare la soluzione:
1. Compilare l'applicazione Angular: `npm run build` in MdExplorer/client2
2. Avviare Electron: `npm start` in ElectronMdExplorer
3. Provare a fare un commit Git cliccando sul pulsante di commit nella toolbar
4. Verificare che appaia la dialog Material invece del prompt nativo

## Vantaggi della Soluzione
- Funziona sia nel browser che in Electron
- Interfaccia coerente con il resto dell'applicazione
- Migliore esperienza utente con validazione e shortcuts
- Nessuna dipendenza da API native del browser problematiche in Electron