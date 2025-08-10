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
# Sprint: Implementazione Ricerca nella TitleBar

**Data**: 2025-01-31  
**Obiettivo**: Aggiungere una barra di ricerca nella TitleBar per cercare documenti e link nel database SQLite

## Analisi Preliminare

### Database Structure
- **MdEngine.db** contiene due tabelle principali:
  - `MarkdownFile`: Id, FileName, Path, FileType
  - `LinkInsideMarkdown`: Id, Path, FullPath, Source, MdContext, MdTitle, HTMLTitle, LinkedCommand

### Pattern Esistenti
- Accesso al database tramite `IEngineDB.GetDal<T>()`
- DTO pattern per trasferimento dati tra backend e frontend
- Controller che estendono `MdControllerBase`

## Implementazione

### 1. Backend - Data Transfer Objects

#### SearchRequestDto
- `searchTerm`: string - termine di ricerca
- `searchType`: enum (All, Files, Links) - tipo di ricerca
- `maxResults`: int - numero massimo di risultati

#### SearchResultDto
- `files`: FileSearchResultDto[] - risultati file trovati
- `links`: LinkSearchResultDto[] - risultati link trovati
- `totalFiles`: int - totale file trovati
- `totalLinks`: int - totale link trovati

#### FileSearchResultDto
- `id`: Guid
- `fileName`: string
- `path`: string
- `fileType`: string
- `matchedField`: string - campo dove è stata trovata la corrispondenza

#### LinkSearchResultDto
- `id`: Guid
- `path`: string
- `fullPath`: string
- `mdTitle`: string
- `htmlTitle`: string
- `mdContext`: string
- `source`: string
- `linkedCommand`: string
- `markdownFileName`: string
- `matchedField`: string

### 2. Backend - API Controller

#### SearchController
- Route: `/api/search`
- Metodi:
  - `GET /api/search/quick?term={term}` - ricerca veloce
  - `POST /api/search/advanced` - ricerca avanzata con filtri

### 3. Backend - Service Layer

#### ISearchService
- `SearchAsync(searchTerm, searchType)`: Task<SearchResultDto>
- `SearchFilesAsync(searchTerm)`: Task<List<FileSearchResultDto>>
- `SearchLinksAsync(searchTerm)`: Task<List<LinkSearchResultDto>>

### 4. Frontend - Angular Components

#### SearchBoxComponent
- Input con icona lente
- Overlay con risultati
- Tabs per Files/Links
- Debounce di 300ms

#### SearchService (Angular)
- Observable per risultati real-time
- Gestione stato ricerca
- Cache risultati recenti

### 5. Integrazione TitleBar
- Posizionamento centrale
- Responsive design
- Keyboard shortcuts (Ctrl+F)

## Fasi di Sviluppo

1. ✅ Creazione DTOs
2. ✅ Implementazione SearchService backend
3. ✅ Creazione SearchController
4. ⏳ Test API con Postman
5. ✅ Creazione SearchBoxComponent
6. ✅ Implementazione SearchService Angular
7. ✅ Integrazione in TitleBar
8. ⏳ Styling e UX refinement
9. ⏳ Testing end-to-end

## Note Tecniche

- Ricerca case-insensitive usando `ToLower()` in LINQ
- Limite di 50 risultati per tipo per evitare problemi di performance
- Highlight del termine cercato nei risultati
- Gestione errori e stati di loading

## Rischi e Mitigazioni

- **Performance**: Implementare paginazione se necessario
- **UX**: Assicurarsi che l'overlay non copra contenuti importanti
- **Accessibilità**: Supporto keyboard navigation nei risultati