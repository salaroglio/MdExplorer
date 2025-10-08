---
author: Carlo Salaroglio
document_type: Document
email: developer@mdexplorer.net
title: 
date: 11/08/2025
word_section:
  write_toc: false
  document_header: ''
  template_section:
    inherit_from_template: ''
    custom_template: ''
    template_type: default
  predefined_pages: 
---
# MdExplorer Bootstrapper - Asset Requirements

## Modifiche Implementate

Il file `Bundle.wxs` è stato aggiornato per risolvere i seguenti problemi:

1. **Rilevamento Java**: Aggiunta la ricerca nel registro per verificare la presenza di Java 8 (sia 32 che 64 bit)
2. **Rilevamento .NET Core 3.1**: Aggiunta la verifica della presenza di .NET Core 3.1
3. **Installazione silenziosa**: Aggiunti i comandi per installazione, riparazione e disinstallazione silenziosa
4. **Rimozione VSCode**: Rimosso il package VSCode dal bootstrapper

## File Richiesti nella cartella `assets`

### IMPORTANTE: Cambiamento del runtime .NET

**PRIMA** (non corretto):
- `aspnetcore-runtime-3.1.32-win-x64.exe`

**ORA** (corretto):
- `dotnet-hosting-3.1.32-win.exe`

Il file `dotnet-hosting-3.1.32-win.exe` include:
- .NET Core Runtime
- ASP.NET Core Runtime
- IIS support

Questo è necessario perché MdExplorer.Service è un'applicazione self-contained che richiede il runtime completo .NET Core, non solo ASP.NET Core.

### Lista completa degli asset richiesti:

1. **dotnet-hosting-3.1.32-win.exe**
   - Download: https://dotnet.microsoft.com/download/dotnet/3.1
   - Cerca ".NET Core 3.1 - Windows Hosting Bundle"

2. **jre-8u311-windows-i586-iftw.exe**
   - Java Runtime Environment 8
   - Download: https://www.oracle.com/java/technologies/javase/javase8-archive-downloads.html

3. **pandoc-2.18-windows-x86_64.msi**
   - Pandoc per la conversione dei documenti
   - Download: https://github.com/jgm/pandoc/releases/tag/2.18

## Compilazione del Bootstrapper

Dopo aver aggiornato gli asset, compilare il progetto WiX per generare il nuovo bootstrapper con le correzioni implementate.