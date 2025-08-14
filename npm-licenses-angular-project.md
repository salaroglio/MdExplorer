---
author: Carlo Salaroglio
document_type: Document
email: salaroglio@hotmail.com
title: 
date: 13/08/2025
word_section:
  write_toc: false
  document_header: ''
  template_section:
    inherit_from_template: ''
    custom_template: ''
    template_type: default
  predefined_pages: 
---
# npm-licenses-angular-project

## Licenze delle librerie npm utilizzate nel progetto Angular

| Libreria | Versione | Licenza | Licenza a pagamento | Note |
|----------|----------|---------|---------------------|------|
| @angular/animations | ~11.2.6 | MIT | No | Parte del framework Angular, licenza MIT by Google LLC |
| @angular/cdk | ^11.2.8 | MIT | No | Component Dev Kit di Angular Material |
| @angular/common | ~11.2.6 | MIT | No | Parte del framework Angular core |
| @angular/compiler | ~11.2.6 | MIT | No | Compilatore Angular |
| @angular/core | ~11.2.6 | MIT | No | Core del framework Angular |
| @angular/flex-layout | ^11.0.0-beta.33 | MIT | No | **DEPRECATO** - Angular team ha terminato il supporto |
| @angular/forms | ~11.2.6 | MIT | No | Gestione form Angular |
| @angular/material | ^11.2.8 | MIT | No | Libreria componenti Material Design |
| @angular/platform-browser | ~11.2.6 | MIT | No | Supporto browser Angular |
| @angular/platform-browser-dynamic | ~11.2.6 | MIT | No | Compilazione JIT Angular |
| @angular/router | ~11.2.6 | MIT | No | Sistema di routing Angular |
| @aspnet/signalr | ^1.0.27 | Apache-2.0 | No | **DEPRECATO** dal 21/08/2021 - usare @microsoft/signalr |
| @microsoft/signalr | ^5.0.7 | MIT | No | Client SignalR attualmente supportato |
| angular-animations | ^0.11.0 | MIT | No | Libreria di animazioni di Chris Filipowski |
| angular-resizable | ^1.2.0 | MIT* | No | *Licenza non esplicitamente confermata, ma angular-resizable-element usa MIT |
| rxjs | ~6.6.0 | Apache-2.0 | No | Libreria reactive programming |
| tslib | ^2.0.0 | 0BSD | No | Zero-Clause BSD (più permissiva di MIT) |
| zone.js | ~0.11.3 | MIT | No | Gestione zone per Angular change detection |

## Riepilogo delle licenze

### Licenze utilizzate:
- **MIT**: 15 librerie (la maggioranza, tutte le librerie Angular ufficiali)
- **Apache-2.0**: 2 librerie (@aspnet/signalr - deprecata, rxjs)
- **0BSD**: 1 libreria (tslib - Zero-Clause BSD, estremamente permissiva)

### Note importanti:
1. **Nessuna libreria richiede licenze a pagamento** - tutte sono open source gratuite
2. **Librerie deprecate da sostituire:**
   - @angular/flex-layout → considerare CSS Grid o Flexbox nativo
   - @aspnet/signalr → migrare a @microsoft/signalr
3. **Licenze permissive**: tutte le licenze (MIT, Apache-2.0, 0BSD) permettono uso commerciale, modifica e distribuzione
4. **tslib** usa 0BSD che è ancora più permissiva di MIT (non richiede attribuzione)

### Compatibilità delle licenze:
Tutte le licenze presenti (MIT, Apache-2.0, 0BSD) sono compatibili tra loro e permettono l'uso in progetti commerciali senza restrizioni significative.