---
author: Carlo Salaroglio
document_type: Document
email: salaroglio@hotmail.com
title: 
date: 21/08/2025
word_section:
  write_toc: false
  document_header: ''
  template_section:
    inherit_from_template: ''
    custom_template: ''
    template_type: default
  predefined_pages: 
---
# 2025-08-21 Sprint AI RAG Implementation

## 📊 STATO ATTUALE

### ✅ Già Completato (Sprint precedente 2025-08-10)

* Sistema di chat AI con LLamaSharp funzionante
* Download manager per modelli da HuggingFace con resume support
* Streaming real-time delle risposte via SignalR
* UI Angular con componente chat dedicato
* Gestione modelli multipli (Qwen3-8B, Qwen2.5-7B, Phi3-mini)
* Risoluzione bug critici su Windows

## 🎯 OBIETTIVI SPRINT CORRENTE

### Sprint 2.1 - RAG per Documento Corrente (3-4 giorni)

#### Obiettivo Principale

Implementare un sistema RAG (Retrieval Augmented Generation) che permetta all'AI di comprendere e rispondere a domande sul documento markdown correntemente visualizzato, usando ricerca semantica vettoriale.

## 📋 TASK DA IMPLEMENTARE

### 1. Architettura RAG Nativa

#### 1.1 Setup Database Vettoriale

* [ ] Integrare SQLite-vss come estensione per ricerca vettoriale
* [ ] Creare database dedicato `MdExplorer.vectors.db` separato dal DB principale
* [ ] Configurare virtual tables per storage vettoriale
* [ ] Implementare migrations per struttura iniziale

#### 1.2 Integrazione Modello Embedding

* [ ] Integrare modello all-MiniLM-L6-v2 (ONNX, 22MB, 384 dimensioni)
* [ ] Implementare caricamento e gestione modello ONNX
* [ ] Creare wrapper per generazione embeddings
* [ ] Test performance generazione vettori

### 2. Servizi Backend RAG

#### 2.1 DocumentChunkerService

* [ ] Implementare chunking semantico del documento markdown
* [ ] Preservare contesto per sezioni (H1/H2)
* [ ] Non spezzare liste, tabelle o blocchi di codice
* [ ] Overlap di 50-100 token tra chunks
* [ ] Aggiungere metadata per ogni chunk (posizione, livello header, tipo)

#### 2.2 EmbeddingService

* [ ] Gestione modello ONNX per generare vettori da testo
* [ ] Caching dei vettori generati
* [ ] Batch processing per efficienza
* [ ] Normalizzazione vettori per similarità coseno

#### 2.3 VectorStoreService

* [ ] Interfaccia con SQLite-vss per storage vettoriale
* [ ] CRUD operations per chunks e embeddings
* [ ] Ricerca k-nearest neighbors
* [ ] Gestione indici e ottimizzazione query

#### 2.4 RAGService (Orchestratore)

* [ ] Coordinamento del processo completo
* [ ] Gestione query utente
* [ ] Retrieval chunks rilevanti
* [ ] Costruzione contesto per LLM
* [ ] Integrazione con AiChatService esistente

### 3. Frontend - UI/UX Chat con Documento

#### 3.1 Componente Selezione Documento

* [ ] Bottone "Aggiungi documento corrente" nella chat
* [ ] Indicatore visuale documento selezionato
* [ ] Stato indicizzazione (idle, in corso, completato)
* [ ] Opzione rimozione documento

#### 3.2 Stati e Feedback

* [ ] Gestione 4 stati documento (nessuno, selezionato, attivo, indicizzazione)
* [ ] Progress indicator durante indicizzazione
* [ ] Visualizzazione numero chunks indicizzati
* [ ] Warning per documento modificato

#### 3.3 Integrazione Chat

* [ ] Modifica ai-chat.component per supporto RAG
* [ ] Visualizzazione chunks recuperati nelle risposte
* [ ] Indicatore quando risposta usa contesto documento

### 4. Estensione Download Manager

#### 4.1 Supporto Modelli Embedding

* [ ] Aggiungere sezione "Modelli Embedding" nel download manager
* [ ] Lista modelli embedding disponibili
* [ ] Download all-MiniLM-L6-v2 ONNX
* [ ] Gestione separata da modelli chat

#### 4.2 UI Settings Migliorata

* [ ] Tab separati per "Modelli Chat" e "Modelli Embedding"
* [ ] Indicatori modello attivo per categoria
* [ ] Gestione indipendente download/attivazione

### 5. Performance e Caching

#### 5.1 Strategia Caching

* [ ] Cache in memoria per documenti sessione corrente
* [ ] Hash contenuto per verificare modifiche
* [ ] Lazy loading indicizzazione
* [ ] Pulizia automatica cache vecchia

#### 5.2 Ottimizzazioni

* [ ] Target indicizzazione < 2 secondi per documento medio
* [ ] Query RAG < 200ms
* [ ] Memory footprint < 50MB per documento
* [ ] Cambio documento con cache istantaneo

### 6. Testing e Validazione

#### 6.1 Test Unitari

* [ ] Test chunking semantico
* [ ] Test generazione embeddings
* [ ] Test ricerca vettoriale
* [ ] Test costruzione contesto

#### 6.2 Test Integrazione

* [ ] Test flusso completo RAG
* [ ] Test performance su documenti grandi
* [ ] Test cache e invalidazione
* [ ] Test UI/UX interazione

## 🚀 SPRINT FUTURI (Non in questo sprint)

### Sprint 2.2 - Tool System Base (1 settimana)

* SearchMarkdownTool: Ricerca semantica in tutti i markdown
* EditYamlHeaderTool: Modifica metadata YAML
* RefactorLinksTool: Aggiorna link automaticamente
* GeneratePlantUMLTool: Crea diagrammi dal contesto

### Sprint 2.3 - Agent Orchestrator (2 settimane)

* Analisi intent utente
* Selezione e coordinamento tools multipli
* Gestione memoria e contesto per task lunghi
* Error recovery e strategie alternative

## 📈 METRICHE DI SUCCESSO

### Performance Target

* Indicizzazione documento medio: < 2 secondi
* Query RAG completa: < 200ms
* Cambio documento con cache: istantaneo
* Memory footprint: < 50MB per documento indicizzato

### User Experience

* Indicizzazione on-demand solo quando richiesto
* Feedback visuale chiaro degli stati
* Nessun impatto performance su chat normale
* Risposte contestualizzate accurate

## 📅 TIMELINE STIMATA

* **Giorno 1-2**: Setup database vettoriale e servizi backend base
* **Giorno 3**: Integrazione frontend e UI/UX
* **Giorno 4**: Testing, ottimizzazioni e bug fixing

## 📝 NOTE TECNICHE

### Compatibilità Stack Esistente

* SQLite-vss non interferisce con NHibernate
* FluentMigration gestisce solo DB principale
* Virtual tables vettori in database separato
* Nessun conflitto con sistema esistente

### Indicizzazione On-Demand

* Chat funziona normalmente senza documento
* Utente seleziona esplicitamente documento
* Indicizzazione solo alla prima domanda
* Cache intelligente per evitare re-indicizzazioni

### Architettura Due Database

* **MdExplorer.db**: Metadata documenti (NHibernate/FluentMigration)
* **MdExplorer.vectors.db**: Chunks e embeddings (gestione diretta)

<br />
