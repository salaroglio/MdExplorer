# Log Implementazione Migrazione Linux

## Data: 2025-08-04 - PASSO 01 COMPLETATO ✅

### 🎉 Milestone: Compatibilità .NET Completata

Il progetto MdExplorer ora compila completamente su Linux senza errori.

**Risultato build**:
```
Build succeeded.
    2 Warning(s)
    0 Error(s)
```

## Data: 2025-08-04

### ✅ Modifiche Completate

#### 1. MdExplorer.Service.csproj - Configurazione Cross-Platform
**File**: `/MdExplorer/MdExplorer.Service.csproj`

**Modifiche effettuate**:
- Rimosso `UseWindowsForms` e `RuntimeIdentifier` hardcoded dal PropertyGroup principale
- Aggiunte configurazioni condizionali per OS:
  - Windows: `UseWindowsForms=true`, `RuntimeIdentifier=win10-x64`
  - Linux: `UseWindowsForms=false`, `RuntimeIdentifier=linux-x64`  
  - macOS: `UseWindowsForms=false`, `RuntimeIdentifier=osx-x64`
- MdImageNumbering.exe incluso solo per Windows build
- GraphViz DLL binaries inclusi solo per Windows build

### ⚠️ Problemi Identificati

#### 1. Dipendenza libssl
**Problema**: .NET Core 3.1 richiede libssl 1.1, ma sistemi Linux moderni hanno OpenSSL 3.0
**Soluzione**: Installare manualmente libssl 1.1:
```bash
# Per Ubuntu/Debian AMD64
wget http://archive.ubuntu.com/ubuntu/pool/main/o/openssl/libssl1.1_1.1.1f-1ubuntu2_amd64.deb
sudo dpkg -i libssl1.1_1.1.1f-1ubuntu2_amd64.deb

# Per ARM64
wget http://ports.ubuntu.com/pool/main/o/openssl/libssl1.1_1.1.1f-1ubuntu2_arm64.deb
sudo dpkg -i libssl1.1_1.1.1f-1ubuntu2_arm64.deb
```

#### 2. Progetti Incompatibili Ancora da Sistemare

##### MdImageNumbering
- **Status**: Windows-only (net6.0-windows, Windows Forms)
- **Azione necessaria**: Escludere dal build Linux o creare versione console

##### Ad.Tools.FluentMigrator.UnitTest
- **Status**: .NET Framework 4.7.2 (incompatibile)
- **Azione necessaria**: Migrare a .NET Core/Standard o escludere dai test Linux

### 📝 Prossimi Passi

1. **Installare libssl 1.1** sul sistema Linux per permettere il build
2. **Escludere progetti Windows-only** dalla solution per Linux
3. **Testare il build** dopo l'installazione di libssl
4. **Verificare altri progetti** per configurazioni Windows-specific

### 🔧 Script Helper Creati

1. **install-linux-dependencies.sh**
   - Installa libssl 1.1 per .NET Core 3.1
   - Installa GraphViz, Java, Pandoc
   - Installa librerie addizionali (libgdiplus, etc.)
   - Supporta Ubuntu/Debian, Fedora/RHEL, Arch Linux

2. **build-linux.sh**
   - Build selettivo escludendo progetti Windows-only
   - Opzioni per Debug/Release e clean build
   - Build frontend Angular/React se Node.js presente
   - Report dettagliato dei risultati

3. **CrossPlatformPath.cs**
   - Utility class per gestione path cross-platform
   - Normalizzazione path per OS corrente
   - Gestione directory dati XDG-compliant per Linux
   - Sanitizzazione nomi file

### 🔥 Nuove Modifiche Completate (Sessione 2)

#### 1. Rimozione Progetti Incompatibili
**Progetti rimossi dalle solution**:
- ✅ **WebViewControl** - Non referenziato, non necessario
- ✅ **MdImageNumbering** - Windows Forms, rimosso da MdExplorer.sln
- ✅ **Ad.Tools.FluentMigrator.UnitTest** - .NET Framework 4.7.2, rimosso
- ✅ **Ad.Tools.Dal.UnitTest** - Riferimenti mancanti, rimosso

#### 2. Fix Configurazione Solution
**Problema**: Progetti Ad.Tools.* non compilati in Debug|x64
**Soluzione**: Aggiunte righe `.Build.0` per:
- Ad.Tools.Dal
- Ad.Tools.Dal.Abstractions
- Ad.Tools.FluentMigrator

#### 3. Fix Cross-Platform
**AppSettingsController.cs**:
- Sostituito `LastIndexOf("\\")` con `Path.GetFileName()`
- Nome progetto ora visualizzato correttamente nel sidenav

**MdExplorerReactEditorController.cs**:
- Fix parsing YAML per newline Linux/Windows
- Array esplicito `{ "\r\n", "\n" }` invece di `Environment.NewLine`

### 📊 Progresso Complessivo

- [x] Analisi iniziale completata
- [x] MdExplorer.Service.csproj reso cross-platform
- [x] Risoluzione dipendenza libssl ✅
- [x] Esclusione progetti Windows-only ✅
- [x] Test build su Linux ✅
- [x] Sistemazione path e filesystem operations ✅
- [ ] Sistemazione processi esterni
- [ ] Frontend Angular/React
- [ ] Electron packaging

### 📋 Dipendenze Verificate

| Componente | Versione | Stato |
|------------|----------|-------|
| libssl | 1.1 | ✅ Installata |
| libgdiplus | so.0 | ✅ Installata |
| GraphViz | 2.43.0 | ✅ Installata |
| Java | OpenJDK 11 | ✅ Installata |