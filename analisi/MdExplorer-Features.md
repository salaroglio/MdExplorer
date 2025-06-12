# MdExplorer Features

## Configurazioni YAML Implementate

### 1. File Change Ignore Configuration (`.mdchangeignore`)

**Descrizione**: Configurazione per escludere file e directory dal monitoraggio del file system watcher.

**File di configurazione**: `/MdExplorer/.mdchangeignore`

**Struttura YAML**:

```YAML
# Directories to ignore
ignoredDirectories:
  - ".md"  # Ignore files in .md directory

# File extensions to ignore
ignoredExtensions:
  - ".pptx"
  - ".docx"
  - ".xlsx"
  - ".xls"
  - ".ppt"
  - ".xlsb"
  - ".bmpr"
  - ".tmp"

# Specific file patterns to ignore
ignoredPatterns:
  - ".md"  # Files starting with .md
  - ".0.pdnSave"  # Paint.net temporary files

# Git-related files to ignore
gitIgnoredFiles:
  - "FETCH_HEAD"
  - "COMMIT_EDITMSG"
  - ".git/"  # Anything in .git directory

# Additional rules
ignoreFilesWithoutExtension: true
```

**Implementazione**:

* Classe: `MonitorMDHostedService`
* Servizio: Carica la configurazione all'avvio
* Fallback: Se il file non esiste, usa i valori hardcoded predefiniti

### 2. Application Extension Configuration (`.mdapplicationtoopen`)

**Descrizione**: Configurazione per definire quali estensioni di file devono essere aperte con applicazioni esterne quando cliccate nell'interfaccia.

**File di configurazione**: `/MdExplorer/.mdapplicationtoopen`

**Struttura YAML**:

```YAML
# Extensions that should open in external applications
supportedExtensions:
  - xlsx    # Microsoft Excel files
  - xls     # Microsoft Excel files (old format)
  - pdf     # PDF documents
  - docx    # Microsoft Word documents
  - pptx    # Microsoft PowerPoint presentations
  - ppt     # Microsoft PowerPoint presentations (old format)
  - bmpr    # Balsamiq mockup files
```

**Implementazione**:

* Interfaccia: `IApplicationExtensionConfiguration`
* Servizio: `ApplicationExtensionConfigurationService`
* Registrazione DI: Singleton in `FeaturesDI.cs`
* Utilizzo: `FromLinkToApplication` command
* Fallback: Se il file non esiste, usa le estensioni predefinite

## Architettura Implementata

### Organizzazione del Codice

```
MdExplorer.Features/
├── Configuration/
│   ├── Models/
│   │   └── ApplicationExtensionConfigurationModel.cs
│   ├── Interfaces/
│   │   └── IApplicationExtensionConfiguration.cs
│   └── Services/
│       └── ApplicationExtensionConfigurationService.cs
```

### Modifiche al Sistema Esistente

1. **MonitorMDHostedService.cs**:
   * Aggiunto caricamento configurazione YAML
   * Modificato metodo `ThereIsNoNeedToAlertClient` per usare configurazione dinamica

2. **FromLinkToApplication.cs**:
   * Iniettato `IApplicationExtensionConfiguration`
   * Rimosso array hardcoded `ExtensionArrayToOpenInApplication`
   * Usa configurazione dinamica per determinare le estensioni supportate

3. **CommandFactory.cs**:
   * Aggiunto supporto per risolvere `IApplicationExtensionConfiguration` dal DI container
   * Risolto problema di dependency injection

4. **GitService.cs**:
   * Risolto conflitto di namespace con `LibGit2Sharp.Configuration`

## Vantaggi dell'Implementazione

1. **Configurabilità**: Gli amministratori possono modificare il comportamento senza ricompilare
2. **Manutenibilità**: Configurazioni centralizzate e facilmente accessibili
3. **Estensibilità**: Facile aggiungere nuove estensioni o pattern
4. **Robustezza**: Fallback automatico ai valori predefiniti se i file YAML non esistono
5. **Separazione delle Responsabilità**: Segue l'architettura a 3 livelli (Presentation, Business, Data)

## Test

* Aggiornati i test esistenti per supportare le nuove dipendenze
* Creato mock `MockApplicationExtensionConfiguration` per i test unitari

## Note per gli Sviluppatori

* I file di configurazione YAML devono essere posizionati nella root del progetto MdExplorer
* Le configurazioni vengono caricate all'avvio dell'applicazione
* Per ricaricare le configurazioni a runtime, usare il metodo `ReloadConfiguration()` (per future implementazioni)
* YamlDotNet v11.2.1 è utilizzato per il parsing YAML

<br />
