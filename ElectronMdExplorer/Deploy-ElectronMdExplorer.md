---
author: Carlo Salaroglio
document_type: Document
email: salaroglio@hotmail.com
title: 
date: 31/07/2025
word_section:
  write_toc: false
  document_header: ''
  template_section:
    inherit_from_template: ''
    custom_template: ''
    template_type: default
  predefined_pages: 
---
# Deploy ElectronMdExplorer

Applicazione desktop Electron per MdExplorer.

## TL;DR - Passi rapidi per aggiornare l'applicazione

1. **Pubblicare il servizio .NET aggiornato:**
   ```powershell
   cd C:\sviluppo\mdExplorer
   dotnet publish MdExplorer\MdExplorer.Service.csproj /p:PublishProfile=FolderProfile
   ```

2. **Creare l'installer Electron:**
   ```powershell
   cd ElectronMdExplorer
   npm run build
   ```

3. **Trovare l'installer in:** `ElectronMdExplorer\Binaries\mdexplorer Setup 1.0.0.exe`

## Processo di Build e Deploy

### 1. Aggiornamento del servizio MdExplorer

Prima di creare l'installer Electron, è necessario pubblicare l'ultima versione del servizio .NET nella cartella `service_payload`.

**Metodo 1: Usando il profilo di pubblicazione (RACCOMANDATO)**

Il progetto MdExplorer ha un profilo di pubblicazione configurato (`FolderProfile.pubxml`) che pubblica automaticamente in `service_payload`:

```PowerShell
# Da Visual Studio: 
# Tasto destro su MdExplorer > Pubblica > FolderProfile

# Da command line:
dotnet publish ..\MdExplorer\MdExplorer.Service.csproj /p:PublishProfile=FolderProfile
```

**Metodo 2: Pubblicazione manuale**

```PowerShell
# Compila il progetto in Release
dotnet publish ..\MdExplorer\MdExplorer.Service.csproj -c Release -r win10-x64 --self-contained true -p:PublishSingleFile=true

# Copia i file nella cartella service_payload
Copy-Item "..\MdExplorer\bin\Release\netcoreapp3.1\win10-x64\publish\*" -Destination ".\service_payload\" -Recurse -Force
```

### 2. Build dell'applicazione Electron

Una volta aggiornato il servizio in `service_payload`:

```PowerShell
# Installa le dipendenze (se necessario)
npm install

# Crea l'installer NSIS
npm run build
```

L'installer verrà creato in `./Binaries/mdexplorer Setup 1.0.0.exe`

### 3. Struttura dell'output

* `Binaries/mdexplorer Setup 1.0.0.exe` - Installer NSIS per Windows
* `Binaries/win-unpacked/` - Versione portable dell'applicazione
* `Binaries/win-unpacked/resources/app_service/` - Contiene il servizio MdExplorer e tutte le dipendenze

### Sviluppo e Test

Per testare l'applicazione in modalità sviluppo:

```PowerShell
# Avvia l'applicazione Electron in modalità sviluppo
npm start

# Con URL specifico
npm start "http://127.0.0.1:55820/client2/main/navigation/document"
```

### Note importanti

* La cartella `service_payload` contiene il servizio ASP.NET Core compilato che viene incluso nell'installer
* Il file `post-build.bat` può essere usato per riorganizzare i file dopo la build (opzionale)
* L'applicazione utilizza electron-builder con target NSIS per creare l'installer Windows
* Verificare sempre la data di `service_payload\MdExplorer.Service.exe` per assicurarsi che sia aggiornato prima di creare l'installer

<br />
