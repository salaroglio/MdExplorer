---
author: Carlo Salaroglio
document_type: Document
email: salaroglio@hotmail.com
title: 
date: 24/08/2025
word_section:
  write_toc: false
  document_header: ''
  template_section:
    inherit_from_template: ''
    custom_template: ''
    template_type: default
  predefined_pages: 
---
# Sprint TOC AI Generation

## Obiettivo

Estendere la funzionalità "Toc Directory" esistente per generare automaticamente descrizioni dei documenti markdown utilizzando l'AI locale (Llama 3.2), creando una tabella riassuntiva e cards dettagliate per ogni documento nella directory.

## Stato Attuale

La feature base "Toc Directory" è già implementata e funzionante:
- Frontend: pulsante nel context menu che crea file .md.directory
- Backend: accetta estensione .md.directory e crea file con titolo se non esiste
- Il file viene trattato come normale markdown per visualizzazione

## Requisiti Funzionali

### Generazione Contenuto con AI

Il sistema deve generare per ogni file markdown nella directory:
- Descrizione breve (massimo 50 parole) utilizzando l'AI locale
- Tabella riassuntiva in testata con nome file linkato e descrizione
- Sezione cards dettagliate con informazioni aggiuntive per ogni documento
- Gestione di tutti i file nella directory (no sottocartelle)
- Processing in batch da 10 file per non sovraccaricare il sistema

### Trigger di Generazione

La generazione AI deve avvenire:
- Al primo click su "Toc Directory" (se file non esiste)
- Con refresh esplicito tramite nuovo pulsante nel context menu
- Mai automaticamente per evitare overhead non richiesto

### Gestione Errori

- Se AI non disponibile: generare lista semplice con avviso nel documento
- Errori singoli file: mostrare messaggio errore nella descrizione
- Nessun retry automatico per evitare loop
- Tutti gli avvisi inseriti direttamente nel markdown generato

## Architettura Tecnica

### Backend - Servizi Richiesti

#### TocGenerationService

Servizio principale per orchestrare la generazione:
- Metodo GenerateTocWithAI(directoryPath, tocFilePath)
- Processing in batch con BATCH_SIZE = 10
- Integrazione con IAiChatService esistente
- Notifiche progress via SignalR esistente
- Utilizzo IUserSettingsDB per accesso dati

#### Prompt Configuration

Utilizzo tabella Setting esistente per configurazioni:
- Chiave "TOC_Generation_Prompt" per prompt personalizzato
- Chiave "TOC_Generation_EnableCache" (ValueBool) per abilitare cache
- Chiave "TOC_Generation_BatchSize" (ValueInt) per dimensione batch
- Template default in appsettings.json come fallback
- Accesso via IUserSettingsDB.GetDal<Setting>()

#### Cache System

Nuova entità e tabella TocDescriptionCache:
- Entità in MdExplorer.Abstractions/Entities/UserDB/
- Mapping FluentNHibernate in MdExplorer.Abstractions/Mappings/
- Migration FluentMigrator in MdExplorer.Migrations/
- Campi: Id (int, PK), FilePath, FileHash, Description, GeneratedAt, ModelUsed, ProjectId
- Accesso via IUserSettingsDB.GetDal<TocDescriptionCache>()

### Frontend - Componenti Angular

#### Estensione md-tree.component

Aggiungere al context menu:
- refreshTocDirectory(): rigenera con AI
- quickTocDirectory(): genera senza AI (lista semplice)
- Controllo esistenza file per mostrare refresh

#### Progress Notification

Utilizzare SignalR hub esistente (MonitorMDHub):
- Eventi: TocGenerationProgress, TocGenerationComplete
- Mostrare snackbar con progresso batch
- Aggiornamento real-time del documento mentre genera

### Formato Output .md.directory

Struttura del documento generato:
1. Header YAML standard esistente
2. Titolo directory
3. Status box con stato generazione e statistiche
4. Tabella markdown con colonne: Documento | Descrizione
5. Sezione cards dettagliate (usando heading H3 e separatori)

## Task di Implementazione

### Fase 1: Backend Core - Database e Entità (2 giorni)

- [ ] Creare entità TocDescriptionCache in MdExplorer.Abstractions/Entities/UserDB/
- [ ] Creare mapping FluentNHibernate TocDescriptionCacheMap in MdExplorer.Abstractions/Mappings/
- [ ] Creare migration FluentMigrator M2025_08_24_001 in MdExplorer.Migrations/
- [ ] Testare migration con dotnet run per verificare creazione tabella
- [ ] Creare TocGenerationService con dependency injection IUserSettingsDB
- [ ] Implementare metodi cache usando GetDal<TocDescriptionCache>()
- [ ] Integrare con IAiChatService esistente
- [ ] Implementare batch processing con feedback SignalR

### Fase 2: Configurazione e Settings (1 giorno)

- [ ] Aggiungere chiavi Setting nel database via seed o UI admin:
  - TOC_Generation_Prompt (ValueString)
  - TOC_Generation_EnableCache (ValueBool = true)
  - TOC_Generation_BatchSize (ValueInt = 10)
- [ ] Implementare metodi in TocGenerationService per leggere Setting
- [ ] Aggiungere default values in appsettings.json come fallback
- [ ] Creare endpoint API per gestione configurazioni TOC
- [ ] Testare lettura/scrittura configurazioni da tabella Setting

### Fase 3: SignalR e Feedback (1 giorno)

- [ ] Estendere MonitorMDHub con eventi TOC
- [ ] Implementare notifiche progress per batch
- [ ] Aggiungere gestione errori e timeout
- [ ] Test con file di grandi dimensioni

### Fase 4: Frontend Integration (2 giorni)

- [ ] Modificare md-tree.component.ts per nuovi metodi
- [ ] Aggiungere pulsanti refresh e quick nel menu
- [ ] Implementare listener SignalR per progress
- [ ] Aggiungere feedback visuale (snackbar/spinner)

### Fase 5: Testing e Ottimizzazione (1 giorno)

- [ ] Test con directory con 50+ file
- [ ] Verifica performance batch processing
- [ ] Test fallback senza AI
- [ ] Validazione cache hit/miss
- [ ] Test multipiattaforma (Windows/Linux/Mac)

## Configurazione

### appsettings.json

```json
{
  "TocGeneration": {
    "EnableAI": true,
    "BatchSize": 10,
    "TimeoutSeconds": 30,
    "MaxContentLength": 2000,
    "CacheDays": 30,
    "DefaultPrompt": "Analizza questo documento markdown e genera una descrizione concisa di massimo 50 parole. Focus su: scopo principale, contenuti chiave, target audience."
  }
}
```

### Settings Database

Utilizzo tabella Setting esistente con pattern standard del progetto:
- TOC_Generation_Prompt (Name) -> ValueString: prompt personalizzato
- TOC_Generation_EnableCache (Name) -> ValueBool: abilitazione cache
- TOC_Generation_BatchSize (Name) -> ValueInt: dimensione batch
- Accesso via IUserSettingsDB.GetDal<Setting>().GetList()

## API Endpoints

### Nuovi Endpoint

- POST /api/toc/generate/{directoryPath}: avvia generazione
- GET /api/toc/status/{directoryPath}: stato generazione corrente
- PUT /api/toc/settings: aggiorna configurazione TOC
- POST /api/toc/refresh/{filePath}: forza refresh singolo file

### Modifiche Esistenti

- MdExplorerController: nessuna modifica (già gestisce .md.directory)
- AiChatController: aggiungere endpoint per test prompt TOC

## Considerazioni Performance

### Ottimizzazioni

- Cache descrizioni per evitare rigenerazione
- Batch processing per non bloccare AI
- Truncate contenuto file oltre 2000 caratteri
- Skip file oltre 100KB automaticamente
- Parallel processing dei batch quando possibile

### Metriche Target

- Generazione singolo file: < 3 secondi
- Batch 10 file: < 30 secondi
- Directory 50 file: < 3 minuti
- Memory footprint: < 100MB aggiuntivi
- Cache hit rate: > 70% dopo primo run

## Testing

### Test Unitari

- TocGenerationService: mock IUserSettingsDB e IAiChatService
- Cache validation con mock GetDal<TocDescriptionCache>()
- Batch processing boundaries
- Error handling paths
- Verificare migration FluentMigrator applicata correttamente

### Test Integrazione

- End-to-end generation flow
- SignalR notification delivery
- Cache persistence
- Concurrent generation handling

### Test UI

- Context menu visibility logic
- Progress feedback accuracy
- Error message display
- Refresh button state management

## Note Tecniche

### Compatibilità

- Mantenere compatibilità con file .md.directory esistenti
- Path handling cross-platform (Path.Combine)
- Encoding UTF-8 per tutti i file generati
- SignalR compatibility con client esistenti
- Migration FluentMigrator compatibile con SQLite esistente
- Mapping NHibernate coerente con convenzioni progetto

### Sicurezza

- Sanitizzazione path per evitare directory traversal
- Limite timeout per evitare DOS con file enormi
- Rate limiting su refresh (max 1 per directory per minuto)
- Validazione prompt template per evitare injection

## Timeline

**Totale stimato: 7 giorni lavorativi**

- Giorni 1-2: Backend core e cache
- Giorno 3: Configurazione e settings
- Giorno 4: SignalR integration
- Giorni 5-6: Frontend e UI
- Giorno 7: Testing e bugfix

## Rischi e Mitigazioni

### Rischi Identificati

1. **Performance AI su grandi directory**
   - Mitigazione: batch processing e cache aggressiva

2. **Timeout su file molto grandi**
   - Mitigazione: truncate contenuto, skip file oltre soglia

3. **Concorrenza generazioni multiple**
   - Mitigazione: lock per directory, queue gestione

4. **Compatibilità modelli AI diversi**
   - Mitigazione: prompt generico, test con tutti i modelli

## Definition of Done

- [ ] Migration FluentMigrator eseguita e tabella TocDescriptionCache creata
- [ ] Entità e mapping NHibernate funzionanti con IUserSettingsDB
- [ ] Configurazioni lette correttamente da tabella Setting
- [ ] Generazione TOC con AI funzionante per tutti i file .md
- [ ] Batch processing con feedback real-time via SignalR
- [ ] Cache funzionante con invalidazione su modifica file
- [ ] Refresh esplicito da context menu
- [ ] Fallback senza AI con avviso utente
- [ ] Test coverage > 80% sul nuovo codice
- [ ] Documentazione API aggiornata
- [ ] Performance entro metriche target
- [ ] Compatibilità cross-platform verificata