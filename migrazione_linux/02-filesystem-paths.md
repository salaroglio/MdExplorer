# Analisi Path e Filesystem Operations per Linux

## 📊 Tabella Riassuntiva Problemi Path

| Problema | Severità | Occorrenze | Azione | Stato | Test | Effort |
|----------|----------|------------|--------|-------|------|--------|
| **Drive Letters (C:\\, D:\\)** | 🔴 Critico | 7 | Rimuovere/Sostituire | ❌ Da fare | ❌ | 2-3 giorni |
| **LocalAppData Variable** | 🔴 Critico | 2 | Usare SpecialFolder | ❌ Da fare | ❌ | 1 giorno |
| **Backslash Hardcoded (\\)** | ⚠️ Alto | 35+ | Usare Path.DirectorySeparatorChar | ❌ Da fare | ❌ | 3-4 giorni |
| **Path Manipulation Manuale** | ⚠️ Alto | 12 | Usare Path API | ❌ Da fare | ❌ | 2 giorni |
| **Path Concatenation (+, $)** | ⚠️ Medio | 20+ | Usare Path.Combine | ❌ Da fare | ❌ | 2 giorni |
| **Windows Path Patterns** | ⚠️ Medio | 8 | Aggiornare Regex | ❌ Da fare | ❌ | 1 giorno |
| **Case Sensitivity** | ✅ Basso | 10+ | Verificare comparazioni | ❌ Da fare | ❌ | 1 giorno |

### Totale Effort Stimato: 11-16 giorni

## 🔴 Problemi Critici (Blockers)

### 1. Drive Letters Hardcoded

**Locations:**
```
GitCredentialTest.cs:16
MdExplorer.Migrations/Version00.00.01/M2021_07_09_001.cs:16
MdExplorer.Migrations/Version00.00.01/M2021_07_09_002.cs:19,25
MdExplorer.Migrations/Version00.00.01/M2021_07_12_001.cs:19
MdExplorer/Controllers/MdFiles/MdFilesController.cs:463,506
```

**Codice Problematico:**
```csharp
// ❌ ERRATO - Drive letter hardcoded
var currentPath = path == "root" ? @"C:\" : path;
ValueString = @"D:\InstallBinaries\plantuml.jar"
ValueString = @"C:\Program Files\Java\jre1.8.0_311\bin\javaw.exe"
```

**Soluzione:**
```csharp
// ✅ CORRETTO - Cross-platform
var currentPath = path == "root" ? Path.GetPathRoot(Directory.GetCurrentDirectory()) : path;
ValueString = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MdExplorer", "Binaries", "plantuml.jar")
ValueString = "java" // Use system Java
```

**Test di Verifica:**
```bash
# Test 1: Verificare assenza drive letters
grep -r '[A-Z]:[\\/]' --include="*.cs" .
# Risultato atteso: Nessun risultato

# Test 2: Verificare path root funzionante
dotnet test --filter "FullyQualifiedName~MdFilesController.GetRootPath"
# Risultato atteso: Test passa su Linux

# Test 3: Verificare PlantUML path
java -jar $(dotnet run --project TestHelpers/GetPlantUmlPath.csproj)
# Risultato atteso: PlantUML version displayed
```

### 2. LocalAppData Environment Variable

**Locations:**
```
MdExplorer/ProjectsManager.cs:34,65
```

**Codice Problematico:**
```csharp
// ❌ ERRATO - LocalAppData non esiste su Linux
var appdata = Environment.GetEnvironmentVariable("LocalAppData");
var databasePath = $@"Data Source = {appdata + Path.DirectorySeparatorChar}MdExplorer.db";
```

**Soluzione:**
```csharp
// ✅ CORRETTO - Cross-platform
var appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
var databasePath = $"Data Source={Path.Combine(appdata, "MdExplorer", "MdExplorer.db")}";
```

**Test di Verifica:**
```bash
# Test 1: Verificare LocalApplicationData path
dotnet run --project TestHelpers/PrintAppDataPath.csproj
# Risultato atteso: Path valido (/home/user/.local/share su Linux)

# Test 2: Verificare creazione database
dotnet test --filter "FullyQualifiedName~ProjectsManager.CreateDatabase"
# Risultato atteso: Database creato correttamente

# Test 3: Verificare accesso database
sqlite3 ~/.local/share/MdExplorer/MdExplorer.db ".tables"
# Risultato atteso: Lista tabelle
```

## ⚠️ Problemi Alti

### 3. Backslash Hardcoded nei Path

**Files Principali Affetti:**
- `MdExplorer.bll/Utilities/Helper.cs` (8 occorrenze)
- `MdExplorer/Controllers/*` (15+ occorrenze)
- `MdExplorer.bll/Commands/*` (10+ occorrenze)

**NOTA IMPORTANTE (2025-01-05)**: Dopo analisi approfondita, questo problema richiede attenzione caso per caso perché:
1. Alcuni backslash potrebbero riferirsi a path che arrivano da API/web con convenzioni specifiche
2. Esiste già `CrossPlatformPath.NormalizePath` che gestisce correttamente la conversione
3. Alcuni path potrebbero essere URL o riferimenti non-filesystem che usano convenzioni diverse
4. Il rischio di regressioni è alto senza comprendere il contesto di ogni utilizzo

**Raccomandazione**: Rimandare questo refactoring a una fase successiva con:
- Analisi dettagliata di ogni caso d'uso
- Test specifici per verificare il comportamento su diversi OS
- Possibile centralizzazione in `CrossPlatformPath` esistente

**Codice Problematico:**
```csharp
// ❌ ERRATO - Backslash hardcoded
return string.Join("\\", dictionary.Select(_ => _.Value));
backPath += "\\..";
var splittedFullPath = fileMdo.FullPath.Split("\\", StringSplitOptions.RemoveEmptyEntries);
var projectName = _fileSystemWatcher.Path.Substring(_fileSystemWatcher.Path.LastIndexOf("\\") + 1);
```

**Soluzione:**
```csharp
// ✅ CORRETTO - Cross-platform
return string.Join(Path.DirectorySeparatorChar.ToString(), dictionary.Select(_ => _.Value));
backPath = Path.Combine(backPath, "..");
var splittedFullPath = fileMdo.FullPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
var projectName = Path.GetFileName(_fileSystemWatcher.Path);
```

**Test di Verifica:**
```bash
# Test 1: Verificare assenza backslash non-verbatim
grep -r '\\\\' --include="*.cs" . | grep -v '@"' | grep -v '\\n' | grep -v '\\t' | grep -v '\\r'
# Risultato atteso: Solo escape sequences legittime

# Test 2: Test path splitting
dotnet test --filter "Category=PathOperations"
# Risultato atteso: Tutti i test passano

# Test 3: Test path joining
echo "/home/user/docs" | dotnet run --project TestHelpers/TestPathJoin.csproj
# Risultato atteso: Path correttamente formattato per Linux
```

### 4. Path Manipulation Manuale

**Locations:**
```
MdExplorer/Controllers/AppCurrentFolderController.cs:26
MdExplorer/Controllers/MdProjects/MdProjectsController.cs:98,193
```

**Codice Problematico:**
```csharp
// ❌ ERRATO - Manipolazione manuale
string lastFolder = currentFolder.Substring(currentFolder.LastIndexOf("\\") + 1);
var parentDir = path.Substring(0, path.LastIndexOf("\\"));
```

**Soluzione:**
```csharp
// ✅ CORRETTO - Usa Path API
string lastFolder = Path.GetFileName(currentFolder);
var parentDir = Path.GetDirectoryName(path);
```

**Test di Verifica:**
```bash
# Test 1: Test GetFileName su vari path
echo -e "/home/user/docs\n/var/log/\nC:\\Windows" | dotnet run --project TestHelpers/TestGetFileName.csproj
# Risultato atteso: Nomi corretti estratti

# Test 2: Test GetDirectoryName
dotnet test --filter "FullyQualifiedName~PathManipulation"
# Risultato atteso: Tutti i test passano

# Test 3: Benchmark performance
dotnet run --project Benchmarks/PathOperationsBenchmark.csproj
# Risultato atteso: Path API più veloce di manipolazione manuale
```

## ⚠️ Problemi Medi

### 5. Path Concatenation Manuale

**Codice Problematico:**
```csharp
// ❌ ERRATO - Concatenazione manuale
var currentFilePath = $".\\.md\\{currentGuid}.md";
var backPath = $"{upLevels}{Path.DirectorySeparatorChar}.md";
var fullPath = basePath + "/" + relativePath;
```

**Soluzione:**
```csharp
// ✅ CORRETTO - Path.Combine
var currentFilePath = Path.Combine(".", ".md", $"{currentGuid}.md");
var backPath = Path.Combine(upLevels, ".md");
var fullPath = Path.Combine(basePath, relativePath);
```

**Test di Verifica:**
```bash
# Test 1: Verificare Path.Combine usage
grep -r "Path.Combine" --include="*.cs" . | wc -l
# Risultato atteso: Numero significativamente aumentato

# Test 2: Test concatenazioni corrette
dotnet test --filter "Category=PathConcatenation"
# Risultato atteso: Tutti i test passano

# Test 3: Verificare path risultanti
find . -name "*.md" -path "*/.md/*" | head -5
# Risultato atteso: Path corretti con separatori Linux
```

### 6. Windows Path Pattern in Regex

**Location:**
```
MdExplorer.bll/Utilities/Helper.cs
```

**Codice Problematico:**
```csharp
// ❌ ERRATO - Pattern solo Windows
var ruleReg = new Regex("(^(PRN|AUX|NUL|CON|COM[1-9]|LPT[1-9]|(\\.+)$)(\\..*)?$)|(([\\x00-\\x1f\\\\?*:\";‌​|/<>])+)|([\\. ]+)");
```

**Soluzione:**
```csharp
// ✅ CORRETTO - Cross-platform
public static bool IsValidFileName(string fileName)
{
    if (string.IsNullOrWhiteSpace(fileName)) return false;
    
    // Caratteri non validi cross-platform
    var invalidChars = Path.GetInvalidFileNameChars();
    if (fileName.IndexOfAny(invalidChars) >= 0) return false;
    
    // Reserved names Windows (mantieni per compatibilità)
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        var windowsReserved = new[] { "PRN", "AUX", "NUL", "CON", "COM1", "LPT1" };
        if (windowsReserved.Contains(fileName.ToUpperInvariant())) return false;
    }
    
    return true;
}
```

**Test di Verifica:**
```bash
# Test 1: Test nomi file validi/invalidi
echo -e "test.txt\nCON\n/invalid\nvalid-file.md" | dotnet run --project TestHelpers/ValidateFileNames.csproj
# Risultato atteso: Validazione corretta per OS

# Test 2: Test caratteri invalidi
dotnet test --filter "FullyQualifiedName~FileNameValidation"
# Risultato atteso: Test passano su Linux e Windows

# Test 3: Creazione file con nomi edge-case
touch "test:file.txt" 2>/dev/null && echo "FAIL" || echo "PASS"
# Risultato atteso: PASS (creazione fallisce come previsto)
```

## ✅ Problemi Bassi

### 7. Case Sensitivity

**Considerazioni:**
- Linux filesystem è case-sensitive
- Windows filesystem è case-insensitive di default
- Il codice già usa `StringComparison.OrdinalIgnoreCase` in molti punti

**Test di Verifica:**
```bash
# Test 1: Verificare uso di OrdinalIgnoreCase
grep -r "OrdinalIgnoreCase" --include="*.cs" . | wc -l
# Risultato atteso: > 20 occorrenze

# Test 2: Test file operations con case diversi
touch test.txt TEST.txt 2>/dev/null && ls test.txt TEST.txt
# Risultato atteso: Due file distinti su Linux

# Test 3: Test applicazione con file case-sensitive
dotnet test --filter "Category=CaseSensitivity"
# Risultato atteso: Gestione corretta differenze OS
```

## 🛠️ Helper Class Consigliata

Creare una classe helper per centralizzare operazioni path:

```csharp
public static class CrossPlatformPath
{
    public static string GetAppDataPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(appData, "MdExplorer");
    }
    
    public static string GetDatabasePath(string dbName)
    {
        return Path.Combine(GetAppDataPath(), "Databases", dbName);
    }
    
    public static string GetBinariesPath()
    {
        return Path.Combine(GetAppDataPath(), "Binaries");
    }
    
    public static string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path)) return path;
        
        // Sostituisci backslash con separatore corretto
        path = path.Replace('\\', Path.DirectorySeparatorChar);
        path = path.Replace('/', Path.DirectorySeparatorChar);
        
        // Rimuovi drive letter se presente
        if (path.Length >= 2 && path[1] == ':')
        {
            path = path.Substring(2);
        }
        
        return Path.GetFullPath(path);
    }
    
    public static bool IsValidFileName(string fileName)
    {
        // Implementazione cross-platform
        return !string.IsNullOrWhiteSpace(fileName) && 
               fileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
    }
}
```

**Test Helper Class:**
```bash
# Test completo helper class
dotnet test --filter "FullyQualifiedName~CrossPlatformPath"
# Risultato atteso: 100% coverage, tutti test passano

# Test integrazione con applicazione
dotnet run --project MdExplorer.Service -- --test-paths
# Risultato atteso: Tutti i path validi e accessibili
```

## 📝 Script di Migrazione Automatica

```bash
#!/bin/bash
# fix-paths.sh - Script per automatizzare alcune correzioni

echo "=== Fixing Path Issues for Linux ==="

# 1. Sostituisci LocalAppData
find . -name "*.cs" -exec sed -i 's/Environment\.GetEnvironmentVariable("LocalAppData")/Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)/g' {} \;

# 2. Sostituisci backslash in Path.DirectorySeparatorChar (con cautela)
# find . -name "*.cs" -exec sed -i 's/"\\\\"/"" + Path.DirectorySeparatorChar + ""/g' {} \;

# 3. Cerca e segnala drive letters
echo "Files with drive letters:"
grep -r '[A-Z]:[\\/]' --include="*.cs" . | cut -d: -f1 | sort -u

# 4. Verifica build
dotnet build --no-restore

echo "=== Path fixing complete ==="
echo "Manual review required for drive letters and complex path operations"
```

## 📋 Checklist Migrazione Path

### Fase 1: Correzioni Critiche
- [ ] Rimuovere tutti i drive letters hardcoded
- [ ] Sostituire LocalAppData con SpecialFolder.LocalApplicationData
- [ ] Verificare creazione database con nuovo path
- [ ] Test: Applicazione si avvia su Linux

### Fase 2: Correzioni Path Operations
- [ ] Sostituire backslash con Path.DirectorySeparatorChar
- [ ] Sostituire manipolazioni manuali con Path API
- [ ] Implementare CrossPlatformPath helper
- [ ] Test: Tutte le operazioni file funzionano

### Fase 3: Ottimizzazioni
- [ ] Sostituire concatenazioni con Path.Combine
- [ ] Aggiornare regex per validazione cross-platform
- [ ] Verificare case sensitivity
- [ ] Test: Performance accettabili

### Fase 4: Validazione Finale
- [ ] Eseguire tutti i test su Linux
- [ ] Verificare funzionalità end-to-end
- [ ] Documentare path conventions
- [ ] Test: Nessuna regressione

## 🎯 Metriche di Successo

- **Zero drive letters** nel codice
- **Zero backslash hardcoded** (eccetto escape sequences)
- **100% test passano** su Linux e Windows
- **Performance path operations** < 10ms
- **Nessun errore** di path not found in produzione