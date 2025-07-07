---
author: Carlo Salaroglio
document_type: Document
email: salaroglio@hotmail.com
title: 
date: 03/07/2025
word_section:
  write_toc: false
  document_header: ''
  template_section:
    inherit_from_template: ''
    custom_template: ''
    template_type: default
  predefined_pages: 
---
# librerie-licenze

Analisi delle licenze delle librerie utilizzate in MdExplorer

## Client2 (Angular) - Dipendenze NPM

| Libreria | Versione | Tipo di Licenza | Pagamento Richiesto | Note |
|----------|----------|-----------------|---------------------|------|
| @angular/animations | ~11.2.6 | MIT | No | Libreria ufficiale Angular |
| @angular/cdk | ^11.2.8 | MIT | No | Component Dev Kit Angular |
| @angular/common | ~11.2.6 | MIT | No | Libreria ufficiale Angular |
| @angular/compiler | ~11.2.6 | MIT | No | Libreria ufficiale Angular |
| @angular/core | ~11.2.6 | MIT | No | Libreria ufficiale Angular |
| @angular/flex-layout | ^11.0.0-beta.33 | MIT | No | **DEPRECATA** - Il team Angular ha terminato il supporto |
| @angular/forms | ~11.2.6 | MIT | No | Libreria ufficiale Angular |
| @angular/material | ^11.2.8 | MIT | No | Material Design per Angular |
| @angular/platform-browser | ~11.2.6 | MIT | No | Libreria ufficiale Angular |
| @angular/platform-browser-dynamic | ~11.2.6 | MIT | No | Libreria ufficiale Angular |
| @angular/router | ~11.2.6 | MIT | No | Libreria ufficiale Angular |
| @aspnet/signalr | ^1.0.27 | Apache-2.0 | No | **DEPRECATA** - Usare @microsoft/signalr |
| @microsoft/signalr | ^5.0.7 | MIT | No | Client SignalR ufficiale Microsoft |
| angular-animations | ^0.11.0 | MIT | No | Libreria di animazioni per Angular |
| angular-resizable | ^1.2.0 | MIT | No | Direttiva per elementi ridimensionabili |
| rxjs | ~6.6.0 | Apache-2.0 | No | Reactive Extensions per JavaScript |
| tslib | ^2.0.0 | 0BSD | No | Runtime library per TypeScript |
| zone.js | ~0.11.3 | MIT | No | Execution context per Angular |

## Riepilogo

### Distribuzione delle Licenze
- **MIT**: 15 librerie (83%)
- **Apache-2.0**: 2 librerie (11%)
- **0BSD**: 1 libreria (6%)

### Considerazioni Importanti

1. **Nessuna libreria richiede pagamento** - Tutte le dipendenze sono open source e gratuite per uso commerciale.

2. **Librerie Deprecate** che richiedono attenzione:
   - `@angular/flex-layout`: Il team Angular ha ufficialmente terminato il supporto. Si consiglia di migrare a CSS Grid o Flexbox nativo.
   - `@aspnet/signalr`: Deprecata in favore di `@microsoft/signalr` che è già presente nel progetto.

3. **Compatibilità delle Licenze**:
   - Tutte le licenze (MIT, Apache-2.0, 0BSD) sono compatibili tra loro
   - Permettono uso commerciale senza restrizioni
   - Non richiedono la pubblicazione del codice sorgente dell'applicazione

4. **Licenze Particolari**:
   - **0BSD** (tslib): È una licenza ancora più permissiva della MIT, praticamente equivalente al pubblico dominio
   - **Apache-2.0** (rxjs, @aspnet/signalr): Richiede di includere l'avviso di copyright e dichiarare le modifiche apportate

### Raccomandazioni

1. Considerare la rimozione di `@aspnet/signalr` dato che `@microsoft/signalr` è già presente
2. Pianificare la migrazione da `@angular/flex-layout` a soluzioni CSS native
3. Mantenere aggiornate le librerie Angular alla stessa versione per evitare conflitti
4. Tutte le licenze sono business-friendly e non presentano rischi legali per uso commerciale

## MdExplorer.Service - Dipendenze NuGet

| Libreria | Versione | Tipo di Licenza | Pagamento Richiesto | Note |
|----------|----------|-----------------|---------------------|------|
| AutoMapper.Extensions.Microsoft.DependencyInjection | 8.1.1 | MIT | No | Deprecata - funzionalità ora in AutoMapper principale |
| HtmlAgilityPack | 1.11.34 | MIT | No | Parser HTML per .NET |
| LibGit2Sharp | 0.27.0-preview-0182 | MIT | No | Binding .NET per libgit2 |
| Markdig | 0.25.0 | BSD-2-Clause | No | Processore Markdown per .NET |
| Microsoft.Alm.Authentication | 4.3.0 | MIT | No | Parte di Git Credential Manager |
| System.Data.SQLite | 1.0.114.4 | Public Domain | No | ADO.NET provider per SQLite |
| System.IO.FileSystem.AccessControl | 5.0.0 | MIT | No | API Microsoft per controllo accessi file |
| YamlDotNet | 11.2.1 | MIT | No | Serializzatore YAML per .NET |

## MdExplorer.Service - Librerie JavaScript in wwwroot

| Libreria | Tipo di Licenza | Pagamento Richiesto | Note |
|----------|-----------------|---------------------|------|
| Bootstrap | MIT | No | Framework CSS |
| jQuery | MIT | No | Versione 3.6.0 |
| Bootstrap Datepicker | Apache-2.0 | No | Plugin datepicker per Bootstrap |
| jQuery UI | MIT | No | Componenti UI per jQuery |
| Highlight.js | BSD-3-Clause | No | Evidenziazione sintassi |
| Tocbot | MIT | No | Generatore table of contents |
| Reveal.js | MIT | No | Framework presentazioni HTML |
| Tippy.js | MIT | No | Tooltip library |
| Popper.js | MIT | No | Posizionamento elementi |
| jSpreadsheet CE | MIT | No | Community Edition - versione Pro a pagamento disponibile |
| jSuites | MIT | No | Componenti JavaScript |
| Angular Resizable | MIT | No | Plugin angular-resizable-element |
| Highlight Within Textarea | MIT | No | Evidenziazione in textarea |
| KaTeX | MIT | No | Rendering formule matematiche |
| CodeMirror | MIT | No | Editor di codice |
| Milkdown | MIT | No | Editor Markdown WYSIWYG |
| ProseMirror | MIT | No | Framework editor rich-text |

## Software Binari Inclusi

| Software | Tipo di Licenza | Pagamento Richiesto | Note |
|----------|-----------------|---------------------|------|
| PlantUML | GPL/LGPL/Apache/EPL/MIT | No | Multi-licenza - scegliere quella più adatta |
| GraphViz | Eclipse Public License (EPL) | No | Tool per generazione grafi |
| Electron | MIT | No | Framework per app desktop |
| Pandoc | GPL v2+ | No | Convertitore universale di documenti - usato per export PDF |

## Riepilogo Generale

### Distribuzione delle Licenze (tutte le librerie)
- **MIT**: 34 librerie (~68%)
- **Apache-2.0**: 3 librerie (~6%)
- **BSD** (2-Clause, 3-Clause): 2 librerie (~4%)
- **GPL v2+**: 1 software (~2%)
- **0BSD**: 1 libreria (~2%)
- **Public Domain**: 1 libreria (~2%)
- **EPL**: 1 libreria (~2%)
- **Multi-licenza**: 1 software (~2%)

### Considerazioni Finali

1. **Nessuna libreria richiede licenza a pagamento** - Tutte le dipendenze sono open source e gratuite
2. **Compatibilità licenze**: Tutte le licenze sono compatibili per uso commerciale
3. **PlantUML** offre flessibilità con multi-licenza
4. **jSpreadsheet** ha versione Pro a pagamento ma la CE inclusa è gratuita con MIT
5. **System.Data.SQLite** è l'unica in Public Domain (nessuna restrizione)
6. **Pandoc** usa GPL v2+ - importante notare che:
   - Viene eseguito come processo separato (non linkato), quindi non impone GPL all'applicazione
   - È gratuito ma ha licenza copyleft (modifiche devono rimanere open source)
   - Usato per generare PDF con template eisvogel
7. Tutte le licenze permettono uso, modifica e distribuzione commerciale rispettando i termini