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
# Sprint Chat AI con Qwen3-8B e Download Manager

## 🐛 BUG FIXING - Problemi Critici da Risolvere

### Tabella Riepilogo Bug

| Bug ID | Nome | Problema | Soluzione Proposta | Status |
|--------|------|----------|-------------------|--------|
| BUG-001 | Model Loading Instability | Il pulsante Load nel download manager non carica il modello. Bug risolto 3 volte ma riappare ad ogni riavvio | ✅ RISOLTO: Conflitto versioni LLamaSharp (0.9.1 vs 0.18.0) e librerie native incompatibili | ✅ Risolto |
| BUG-002 | HTTP 416 Range Not Satisfiable | Errore 416 quando si tenta di riprendere download di file già completi o corrotti | ✅ RISOLTO: Aggiunta gestione file completi/corrotti prima del resume e gestione esplicita errore 416 | ✅ Risolto |
| BUG-003 | File Lock on Windows | Errore "file in use" durante File.Move dopo download completato su Windows | ✅ RISOLTO: Chiusura esplicita stream, flush, delay e retry logic con 5 tentativi | ✅ Risolto |

### BUG-001: Model Loading Instability

#### Sintomi
- Premendo il pulsante "Load" nel download manager, il modello non viene caricato
- Il problema è stato "risolto" 3 volte ma riappare sistematicamente al riavvio
- Indica un problema strutturale nella soluzione implementata

#### Analisi del Problema Ricorrente

**Pattern osservato**: Il bug viene risolto temporaneamente ma ritorna al riavvio, suggerendo che:

1. **Le modifiche non vengono salvate correttamente** 
   - Possibile modifica di file temporanei invece che sorgenti
   - Build cache che sovrascrive le modifiche

2. **Race condition all'inizializzazione**
   - Servizi che si inizializzano in ordine sbagliato
   - SignalR hub che parte prima del servizio AI

3. **Stato non persistente**
   - Il modello caricato non viene salvato in configurazione
   - Al riavvio si perde il riferimento al modello attivo

4. **Problema di dependency injection**
   - Istanze multiple del servizio AI
   - Singleton vs Scoped lifecycle errato

#### Investigazione Completata

**Risultati dell'analisi del codice:**

1. **Dependency Injection**: ✅ Corretto
   - `AiChatService` registrato come Singleton in `Startup.cs:67`
   - `ModelDownloadService` registrato come Singleton in `Startup.cs:66`

2. **Flusso di caricamento modello**:
   - Frontend: `model-manager.component.ts` → `ai-chat.service.ts`
   - SignalR: `hubConnection.invoke('LoadModel', modelId)`
   - Backend: `AiChatHub.LoadModel()` → `AiChatService.LoadModelAsync()`

3. **Problemi identificati nel codice**:
   - ❌ **Nessuna persistenza del modello attivo**: Al riavvio non c'è memoria di quale modello era caricato
   - ❌ **Mancanza di retry logic**: Se il caricamento fallisce, non ci sono tentativi automatici
   - ⚠️ **Possibile race condition**: Il servizio potrebbe essere chiamato prima che SignalR sia connesso
   - ⚠️ **Lock con timeout infinito**: `_modelLock.WaitAsync()` senza timeout può bloccare indefinitamente

#### Soluzione Definitiva: Allineamento Versioni Librerie

**Causa del problema identificata (14/08/2025)**: 
- Conflitto tra versioni LLamaSharp nei diversi progetti
- MdExplorer.Features.csproj aveva LLamaSharp 0.9.1
- MdExplorer.Service.csproj aveva LLamaSharp 0.18.0
- Le librerie native (.so/.dll) della versione 0.9.1 non contenevano la funzione `llama_supports_mmap` richiesta dalla 0.18.0

**Soluzione implementata con successo**:

1. **Allineamento versioni** - Aggiornati tutti i progetti a LLamaSharp 0.18.0:
   - MdExplorer.Features.csproj: LLamaSharp 0.18.0 + LLamaSharp.Backend.Cpu 0.18.0
   - Aggiornate dipendenze Microsoft.Extensions a 8.0.2 per compatibilità

2. **Rimozione proprietà incompatibili**:
   - Rimossa proprietà `Seed` da ModelParams (non disponibile in 0.18.0)
   - Mantenute solo le proprietà compatibili: ContextSize, GpuLayerCount

3. **Pulizia e rebuild**:
   - Pulite directory bin/obj di tutti i progetti
   - Rebuild completo della soluzione
   - Verificata presenza corretta librerie native 0.18.0

**Verifica funzionamento**:
- Backend avviato correttamente senza errori
- Caricamento modello completato con successo
- Chat AI funzionante con streaming responses

### BUG-002: HTTP 416 Range Not Satisfiable (Windows)

#### Sintomi
- Errore "Response status code does not indicate success: 416 (Range Not Satisfiable)" durante il download
- Si verifica quando si tenta di riprendere un download con file temporaneo già completo o corrotto
- Specifico per Windows, non si manifestava su Linux

#### Causa Identificata (14/08/2025)
- Il file temporaneo `.download` aveva dimensione uguale o maggiore del file completo
- Il server HTTP restituisce 416 quando riceve una richiesta Range con byte di partenza >= dimensione file
- Mancava gestione del caso in cui il download era già completo ma non rinominato

#### Soluzione Implementata
**File modificato**: `MdExplorer.bll/Services/ModelDownloadService.cs`

1. **Controllo pre-download**:
   - Verifica se il modello è già installato prima di iniziare
   - Controlla dimensione del file temporaneo vs dimensione attesa

2. **Gestione file completi**:
   - Se il file temp ha la stessa dimensione del file atteso, lo rinomina direttamente
   - Se è più grande (corrotto), lo elimina e ricomincia

3. **Gestione errore 416**:
   - Intercetta specificamente l'errore 416
   - Esegue richiesta HEAD per ottenere la dimensione reale dal server
   - Se il file è completo, lo sposta nella posizione finale
   - Altrimenti elimina e riprova da zero

### BUG-003: File Lock su Windows

#### Sintomi
- Errore "The process cannot access the file because it is being used by another process" 
- Si verifica durante `File.Move` dopo il completamento del download
- I pulsanti "Load" non si attivano dopo il download
- Specifico per Windows (gestione file handle diversa da Linux)

#### Causa Identificata (14/08/2025)
- Gli stream del file non venivano rilasciati immediatamente su Windows
- Windows mantiene i file handle aperti per un breve periodo anche dopo il dispose
- Il `File.Move` veniva eseguito troppo rapidamente dopo la chiusura degli stream

#### Soluzione Implementata
**File modificato**: `MdExplorer.bll/Services/ModelDownloadService.cs`

1. **Chiusura esplicita stream**:
   - Uso di blocchi `using` standard invece di `using var`
   - Aggiunto `FlushAsync()` per garantire scrittura su disco
   - Stream chiusi esplicitamente prima del move

2. **Delay per Windows**:
   - Aggiunto delay di 100ms dopo chiusura stream
   - Permette a Windows di rilasciare completamente i file handle

3. **Retry logic robusta**:
   - Implementati 5 tentativi con delay progressivo (500ms, 1s, 1.5s, 2s, 2.5s)
   - Log dei tentativi falliti per debugging
   - Gestione graceful degli errori IOException

4. **Notifica completamento**:
   - Aggiunto report finale con status "Complete"
   - Garantisce aggiornamento UI e attivazione pulsanti

## 🎯 EVOLUZIONE: Da Chat a AI Agent con RAG (Approccio Graduale)

### 📊 STATO ATTUALE IMPLEMENTAZIONE

#### ✅ Completato (Sprint 1)
- **Backend AI Chat**: `AiChatService` con LLamaSharp funzionante
- **Download Manager**: `ModelDownloadService` per modelli HuggingFace  
- **SignalR Streaming**: `AiChatHub` per chat real-time
- **Controller API**: `AiModelsController` per gestione modelli
- **Frontend Angular**: Componente `ai-chat` con sidebar integrata
- **Modelli supportati**: Qwen3-8B, Qwen2.5-7B, Phi3-mini

### 🚀 Sprint 2.1 - RAG per Documento Corrente (IN CORSO - 3-4 giorni)

#### Obiettivo
Implementare un sistema RAG (Retrieval Augmented Generation) che permetta all'AI di comprendere e rispondere a domande sul documento markdown correntemente visualizzato, usando ricerca semantica vettoriale.

#### Architettura RAG Nativa

**Stack Tecnologico:**
- **SQLite-vss**: Estensione per ricerca vettoriale in SQLite
- **Database dedicato**: `MdExplorer.vectors.db` separato dal DB principale
- **Modello embedding**: all-MiniLM-L6-v2 (ONNX, 22MB, 384 dimensioni)
- **Indicizzazione on-demand**: Solo quando l'utente seleziona esplicitamente un documento

#### Concetti Chiave del Sistema RAG

**1. Chunking Semantico del Documento**
Il documento markdown viene diviso in "chunks" (pezzi) intelligenti che preservano il contesto:
- Ogni sezione (H1/H2) con il suo contenuto diventa un chunk
- Mai spezzare liste, tabelle o blocchi di codice
- Overlap di 50-100 token tra chunks per mantenere continuità
- Metadata per ogni chunk: posizione, livello header, tipo contenuto

**2. Embeddings e Ricerca Vettoriale**
Ogni chunk di testo viene trasformato in un vettore numerico (embedding) che rappresenta il suo significato semantico:
- Vettore di 384 dimensioni che cattura il "significato" del testo
- Permette di trovare testi simili anche se usano parole diverse
- "Come installo?" e "Setup procedure" avranno vettori simili
- Ricerca per similarità coseno invece che keyword matching

**3. Indicizzazione On-Demand**
Nessuna indicizzazione automatica, tutto controllato dall'utente:
- La chat funziona normalmente senza documento
- L'utente seleziona esplicitamente un documento dalla UI
- L'indicizzazione avviene solo alla prima domanda sul documento
- Cache intelligente per evitare re-indicizzazioni inutili

**4. Flusso di Retrieval**
Quando l'utente fa una domanda con documento selezionato:
- La domanda diventa un embedding
- SQLite-vss trova i 5 chunks più simili semanticamente
- I chunks vengono assemblati come contesto per il LLM
- Il modello risponde basandosi sui chunks rilevanti del documento

#### Gestione Database con SQLite-vss

**Architettura a Due Database:**
- **MdExplorer.db** (principale): Contiene metadata sui documenti indicizzati, gestito da FluentMigration e NHibernate
- **MdExplorer.vectors.db** (vettoriale): Contiene chunks e embeddings, gestito direttamente dal servizio RAG

**Compatibilità con Stack Esistente:**
- SQLite-vss è un'estensione che non interferisce con NHibernate
- FluentMigration gestisce solo tabelle standard nel DB principale
- Le virtual tables per vettori sono in database separato
- Nessun conflitto o modifica al sistema esistente

#### Servizi Backend in MdExplorer.bll

**Organizzazione dei servizi RAG:**
- **EmbeddingService**: Gestisce il modello ONNX per generare vettori da testo
- **DocumentChunkerService**: Divide intelligentemente i documenti markdown
- **VectorStoreService**: Interfaccia con SQLite-vss per storage e ricerca vettoriale
- **RAGService**: Orchestratore principale che coordina tutto il processo

**Flusso di elaborazione:**
1. DocumentChunker divide il markdown in chunks semantici
2. EmbeddingService trasforma ogni chunk in vettore numerico
3. VectorStore salva chunks e vettori nel database dedicato
4. RAGService gestisce query, retrieval e costruzione contesto

#### UI/UX per Chat con Documento

**Design dell'interfaccia:**
- Bottone esplicito "Aggiungi documento corrente" nella chat
- Indicatore visuale del documento selezionato e stato indicizzazione
- Possibilità di rimuovere il documento per tornare a chat generica
- Visualizzazione dei chunks recuperati durante le risposte

**Stati del documento nella UI:**
1. **Nessun documento**: Chat generica senza contesto
2. **Documento selezionato**: Nome file visibile, non ancora indicizzato
3. **Documento attivo**: Indicizzato e pronto, mostra numero chunks
4. **Indicizzazione in corso**: Progress indicator durante elaborazione

**Comportamento smart:**
- Indicizzazione solo alla prima domanda che richiede il documento
- Cache per sessione per evitare re-indicizzazioni
- Warning se documento è stato modificato dall'ultima indicizzazione

#### Estensione Download Manager

**Nuovi modelli da aggiungere:**
- **Modelli embedding** per RAG (separati dai modelli chat)
- all-MiniLM-L6-v2 ONNX (22MB) per embedding multilingua veloce
- Alternative più potenti per accuracy migliore (opzionali)

**UI Settings migliorata:**
- Sezione separata per "Modelli Chat" e "Modelli Embedding"
- Indicatori di quale modello è attivo per ogni categoria
- Download e gestione indipendenti

#### Gestione Cache e Performance

**Strategia di caching:**
- Cache in memoria per documenti della sessione corrente
- Hash del contenuto per verificare se documento è cambiato
- Lazy loading: indicizza solo quando serve
- Pulizia automatica documenti non usati da tempo

**Performance target:**
- Indicizzazione documento medio: < 2 secondi
- Query RAG completa: < 200ms
- Cambio documento con cache: istantaneo
- Memory footprint: < 50MB per documento indicizzato

### 🔄 Sprint 2.2 - Tool System Base (Futuro - 1 settimana)

#### Obiettivo
Aggiungere capacità di azione all'AI attraverso tools che permettono di interagire con il sistema MdExplorer.

**Tools prioritari:**
- **SearchMarkdownTool**: Ricerca semantica in tutti i markdown del progetto
- **EditYamlHeaderTool**: Modifica metadata YAML dei documenti
- **RefactorLinksTool**: Aggiorna automaticamente link quando sposti file
- **GeneratePlantUMLTool**: Crea diagrammi dal contesto

**Architettura Tool System:**
- Interfaccia comune per tutti i tools
- Parser per identificare quando l'AI vuole usare un tool
- Esecuzione controllata con feedback all'utente
- Risultati re-iniettati nel contesto per continuare la conversazione

### 🧠 Sprint 2.3 - Agent Orchestrator (Futuro - 2 settimane)

#### Obiettivo
Trasformare l'AI in un vero agente capace di pianificare e eseguire task complessi.

**Capabilities dell'orchestrator:**
- Analisi intent per capire cosa vuole l'utente
- Selezione e coordinamento di tools multipli
- Gestione memoria e contesto per task lunghi
- Error recovery e strategie alternative

**Evoluzione progressiva:**
1. Prima tools singoli su richiesta esplicita
2. Poi combinazioni semplici di 2-3 tools
3. Infine pianificazione autonoma di task complessi

---

## ✅ IMPLEMENTAZIONE COMPLETATA - SPRINT 1

*Tutto quanto segue è stato completato con successo nel primo sprint (10/08/2025)*

### File Implementati Sprint 1

#### Backend
- `MdExplorer.bll/Services/AiChatService.cs` - Servizio chat con LLamaSharp
- `MdExplorer.bll/Services/ModelDownloadService.cs` - Download manager modelli
- `MdExplorer/Controllers/AI/AiModelsController.cs` - API REST per gestione modelli
- `MdExplorer/Hubs/AiChatHub.cs` - SignalR hub per streaming chat

#### Frontend Angular
- `client2/src/app/ai-chat/` - Componente chat completo
- `client2/src/app/services/ai-chat.service.ts` - Servizio Angular per AI

## Obiettivo Sprint

Implementare una chat AI completa in MdExplorer con:

1. Download manager per scaricare modelli da HuggingFace
2. Gestione modelli AI nel componente Settings esistente
3. Chat con Qwen3-8B (5.03 GB)
4. Integrazione con editor per inserire risposte AI

## Scope Sprint

* ✅ Download manager con progress bar
* ✅ Settings component per gestione modelli
* ✅ Chat AI con streaming
* ✅ Pulsante copia/inserisci risposte
* ✅ Support per resume download
* ❌ NO sicurezza avanzata (per ora)
* ❌ NO ottimizzazioni complesse
* ❌ NO MCP (per ora)

## 📋 Riepilogo Sprint 1 Completato

Lo Sprint 1 ha implementato con successo:
- Sistema di chat AI con LLamaSharp integrato
- Download manager per modelli da HuggingFace con resume support
- Streaming real-time delle risposte via SignalR
- UI Angular con componente chat dedicato
- Gestione modelli multipli (Qwen3-8B, Qwen2.5-7B, Phi3-mini)
- Risoluzione bug critici su Windows (file lock, HTTP 416, conflitti librerie)

I dettagli tecnici dell'implementazione sono stati completati e testati con successo.