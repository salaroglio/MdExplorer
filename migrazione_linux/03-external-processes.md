# Analisi Processi Esterni e Shell Commands per Linux

## 📊 Tabella Riassuntiva Processi Esterni

| Processo/Comando | Severità | Occorrenze | Azione | Stato | Test | Effort |
|------------------|----------|------------|--------|-------|------|--------|
| **cmd.exe /c** | 🔴 Critico | 5+ | Sostituire con /bin/sh -c | ❌ Da fare | ❌ | 1 giorno |
| **explorer.exe** | 🔴 Critico | 2 | Sostituire con xdg-open | ⚠️ Parziale | ❌ | 0.5 giorni |
| **javaw.exe hardcoded** | ⚠️ Alto | 3 | Auto-detect Java | ❌ Da fare | ❌ | 1 giorno |
| **VS Code path Windows** | ⚠️ Medio | 2 | Usare 'code' command | ❌ Da fare | ❌ | 0.5 giorni |
| **dot.exe GraphViz** | ⚠️ Medio | 1 | Rimuovere .exe | ❌ Da fare | ❌ | 0.5 giorni |
| **Pandoc via cmd** | ⚠️ Alto | 1 | Esecuzione diretta | ❌ Da fare | ❌ | 1 giorno |
| **Git commands** | ✅ OK | Multiple | Già cross-platform | ✅ Completo | ⚠️ | - |
| **SSH chmod** | ✅ OK | 1 | Già compatibile | ✅ Completo | ⚠️ | - |
| **secret-tool Linux** | ✅ OK | 1 | Già implementato | ✅ Completo | ⚠️ | - |

### Totale Effort Stimato: 5-7 giorni sviluppo + 2-3 giorni testing

## 🔴 Problemi Critici (Blockers)

### 1. CMD.EXE Shell Commands

**Locations:**
```
MdExplorer/Controllers/AppSettingsController.cs:95
MdExplorer/Controllers/MdExportController.cs:260
MdExplorer/Controllers/MdFiles/MdFilesController.cs:145,217,335
```

**Codice Problematico:**
```csharp
// ❌ ERRATO - Windows specific
new ProcessStartInfo("cmd.exe", $"/c \"{path}\"")
new ProcessStartInfo("cmd", finalCommand)
```

**Soluzione Cross-Platform:**
```csharp
// ✅ CORRETTO - Cross-platform shell command
public static ProcessStartInfo CreateShellCommand(string command)
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        return new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c \"{command}\"",
            UseShellExecute = false
        };
    }
    else
    {
        return new ProcessStartInfo
        {
            FileName = "/bin/sh",
            Arguments = $"-c \"{command}\"",
            UseShellExecute = false
        };
    }
}
```

**Test di Verifica:**
```bash
# Test 1: Verificare assenza cmd.exe hardcoded
grep -r "cmd\.exe" --include="*.cs" .
# Risultato atteso: Solo in helper class

# Test 2: Test shell command execution Linux
echo 'echo "Test"' | /bin/sh -c "$(cat)"
# Risultato atteso: "Test" stampato

# Test 3: Test cross-platform shell wrapper
dotnet test --filter "FullyQualifiedName~ShellCommandTests"
# Risultato atteso: Test passano su Linux e Windows

# Test 4: Verifica comando complesso
dotnet run --project TestHelpers/TestShellCommand.csproj -- "ls -la | grep .cs | wc -l"
# Risultato atteso: Numero di file .cs nella directory
```

### 2. Explorer.exe File Manager

**Locations:**
```
MdExplorer/Controllers/AppSettingsController.cs:88
MdExplorer/Controllers/MdFiles/MdFilesController.cs:388
```

**Codice Problematico:**
```csharp
// ❌ ERRATO - Windows only
Process.Start("explorer.exe", pathToOpen);
new ProcessStartInfo { FileName = "explorer.exe", Arguments = $"\"{processPath}\"" }
```

**Soluzione Cross-Platform:**
```csharp
// ✅ CORRETTO - Cross-platform file manager
public static void OpenFileManager(string path)
{
    ProcessStartInfo psi;
    
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        psi = new ProcessStartInfo("explorer.exe", $"\"{path}\"");
    }
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
        // xdg-open apre il file manager di default
        psi = new ProcessStartInfo("xdg-open", $"\"{path}\"");
    }
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
        psi = new ProcessStartInfo("open", $"\"{path}\"");
    }
    else
    {
        throw new PlatformNotSupportedException();
    }
    
    psi.UseShellExecute = true;
    Process.Start(psi);
}

// Alternativa: rilevare file manager specifico
public static string GetFileManager()
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
        // Controlla quale file manager è disponibile
        var managers = new[] { "nautilus", "dolphin", "thunar", "pcmanfm", "nemo" };
        foreach (var manager in managers)
        {
            if (IsCommandAvailable(manager))
                return manager;
        }
        return "xdg-open"; // Fallback
    }
    return "explorer.exe";
}
```

**Test di Verifica:**
```bash
# Test 1: Verificare xdg-open disponibile
which xdg-open
# Risultato atteso: /usr/bin/xdg-open o simile

# Test 2: Test apertura directory
xdg-open /tmp
# Risultato atteso: File manager si apre nella directory /tmp

# Test 3: Test funzione cross-platform
dotnet test --filter "FullyQualifiedName~OpenFileManagerTests"
# Risultato atteso: Test passano, file manager si apre

# Test 4: Test rilevamento file manager
dotnet run --project TestHelpers/DetectFileManager.csproj
# Risultato atteso: Nome del file manager rilevato (es. "nautilus")
```

## ⚠️ Problemi Alti

### 3. Java/Javaw.exe Path Hardcoded

**Locations:**
```
MdExplorer.Migrations/Version00.00.01/M2021_07_09_002.cs:19
MdExplorer.bll/Commands/PlantumlServer.cs:63
```

**Codice Problematico:**
```csharp
// ❌ ERRATO - Windows path hardcoded
ValueString = @"C:\Program Files\Java\jre1.8.0_311\bin\javaw.exe"

// ❌ ERRATO - Usa javaw che non esiste su Linux
processStartInfo.FileName = javaPath; // Assume javaw.exe
```

**Soluzione Cross-Platform:**
```csharp
// ✅ CORRETTO - Auto-detect Java
public static class JavaDetector
{
    public static string FindJavaExecutable()
    {
        // 1. Check JAVA_HOME
        var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
        if (!string.IsNullOrEmpty(javaHome))
        {
            var javaExe = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) 
                ? "javaw.exe" : "java";
            var javaPath = Path.Combine(javaHome, "bin", javaExe);
            if (File.Exists(javaPath))
                return javaPath;
        }
        
        // 2. Check PATH
        var pathVar = Environment.GetEnvironmentVariable("PATH");
        var paths = pathVar?.Split(Path.PathSeparator) ?? Array.Empty<string>();
        
        foreach (var path in paths)
        {
            var javaExe = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) 
                ? "javaw.exe" : "java";
            var fullPath = Path.Combine(path, javaExe);
            if (File.Exists(fullPath))
                return fullPath;
        }
        
        // 3. Try common locations
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var commonPaths = new[]
            {
                "/usr/bin/java",
                "/usr/lib/jvm/default/bin/java",
                "/usr/lib/jvm/java-11-openjdk/bin/java",
                "/usr/lib/jvm/java-8-openjdk/bin/java"
            };
            
            foreach (var path in commonPaths)
            {
                if (File.Exists(path))
                    return path;
            }
        }
        
        // 4. Fallback to command name
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "javaw" : "java";
    }
    
    public static string GetJavaVersion(string javaPath)
    {
        var psi = new ProcessStartInfo
        {
            FileName = javaPath,
            Arguments = "-version",
            RedirectStandardError = true,
            UseShellExecute = false
        };
        
        using var process = Process.Start(psi);
        var output = process.StandardError.ReadToEnd();
        process.WaitForExit();
        
        return output;
    }
}
```

**Database Migration per Java Path:**
```csharp
[Migration(20250804001, "Update Java path for cross-platform")]
public class UpdateJavaPathCrossPlatform : Migration
{
    public override void Up()
    {
        var javaPath = JavaDetector.FindJavaExecutable();
        
        Update.Table("Setting")
            .Set(new { ValueString = javaPath })
            .Where(new { Name = "JavaPath" });
            
        // Log per debug
        Execute.Sql($"INSERT INTO MigrationLog (Message) VALUES ('Java path updated to: {javaPath}')");
    }
    
    public override void Down()
    {
        // Revert non necessario
    }
}
```

**Test di Verifica:**
```bash
# Test 1: Verificare JAVA_HOME
echo $JAVA_HOME
java -version
# Risultato atteso: Path Java e versione visualizzati

# Test 2: Test auto-detection
dotnet run --project TestHelpers/DetectJava.csproj
# Risultato atteso: Path Java trovato e versione stampata

# Test 3: Test PlantUML con Java rilevato
echo "@startuml\nA -> B\n@enduml" | java -jar plantuml.jar -pipe > test.png
file test.png
# Risultato atteso: PNG image data

# Test 4: Test migration database
dotnet ef migrations add UpdateJavaPath
dotnet ef database update
sqlite3 MdExplorer.db "SELECT * FROM Setting WHERE Name='JavaPath'"
# Risultato atteso: Path Java corretto per il sistema
```

### 4. Pandoc Export via CMD

**Location:**
```
MdExplorer/Controllers/MdExportController.cs:260,314
```

**Codice Problematico:**
```csharp
// ❌ ERRATO - Wrappato in cmd
var finalCommand = $"/c pandoc \"{mdFilePath}\" -o \"{outputPath}\" --pdf-engine=pdflatex";
new ProcessStartInfo("cmd", finalCommand);
```

**Soluzione Cross-Platform:**
```csharp
// ✅ CORRETTO - Esecuzione diretta Pandoc
public static ProcessStartInfo CreatePandocCommand(string mdFile, string outputFile, string pdfEngine = null)
{
    var psi = new ProcessStartInfo
    {
        FileName = "pandoc",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true
    };
    
    var args = new List<string>
    {
        $"\"{mdFile}\"",
        "-o", $"\"{outputFile}\""
    };
    
    if (!string.IsNullOrEmpty(pdfEngine))
    {
        args.Add($"--pdf-engine={pdfEngine}");
    }
    
    // Aggiungi template se disponibile
    var templatePath = GetPandocTemplatePath();
    if (File.Exists(templatePath))
    {
        args.Add($"--template=\"{templatePath}\"");
    }
    
    psi.Arguments = string.Join(" ", args);
    
    return psi;
}

private static string GetPandocTemplatePath()
{
    var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    return Path.Combine(appData, "MdExplorer", "templates", "eisvogel.tex");
}
```

**Test di Verifica:**
```bash
# Test 1: Verificare Pandoc installato
pandoc --version
# Risultato atteso: Pandoc version info

# Test 2: Test conversione MD to PDF
echo "# Test\nHello World" > test.md
pandoc test.md -o test.pdf --pdf-engine=xelatex
file test.pdf
# Risultato atteso: PDF document

# Test 3: Test con template
dotnet test --filter "FullyQualifiedName~PandocExportTests"
# Risultato atteso: Export PDF funzionante

# Test 4: Benchmark export performance
time pandoc large-doc.md -o output.pdf
# Risultato atteso: < 5 secondi per documento medio
```

## ⚠️ Problemi Medi

### 5. VS Code Path Windows

**Location:**
```
MdExplorer.Migrations/Version00.00.01/M2021_07_12_001.cs:19
MdExplorer.bll/Utilities/ProcessUtil.cs
```

**Codice Problematico:**
```csharp
// ❌ ERRATO - Path Windows hardcoded
ValueString = @"C:\Users\Carlo Salaroglio\AppData\Local\Programs\Microsoft VS Code\Code.exe"
```

**Soluzione Cross-Platform:**
```csharp
// ✅ CORRETTO - Rilevamento VS Code cross-platform
public static class EditorDetector
{
    public static string FindVSCode()
    {
        // 1. Try 'code' command (cross-platform)
        if (IsCommandAvailable("code"))
            return "code";
            
        // 2. Platform-specific paths
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var paths = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                    "Programs", "Microsoft VS Code", "Code.exe"),
                @"C:\Program Files\Microsoft VS Code\Code.exe",
                @"C:\Program Files (x86)\Microsoft VS Code\Code.exe"
            };
            
            return paths.FirstOrDefault(File.Exists) ?? "code";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var paths = new[]
            {
                "/usr/bin/code",
                "/usr/share/code/code",
                "/snap/bin/code",
                Path.Combine(Environment.GetEnvironmentVariable("HOME"), ".local/bin/code")
            };
            
            return paths.FirstOrDefault(File.Exists) ?? "code";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "/Applications/Visual Studio Code.app/Contents/Resources/app/bin/code";
        }
        
        return "code";
    }
    
    public static bool IsCommandAvailable(string command)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "where" : "which",
                Arguments = command,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            
            using var process = Process.Start(psi);
            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
```

**Test di Verifica:**
```bash
# Test 1: Verificare 'code' command
which code
code --version
# Risultato atteso: Path e versione VS Code

# Test 2: Test apertura file in VS Code
echo "test" > test.txt
code test.txt
# Risultato atteso: VS Code apre il file

# Test 3: Test rilevamento automatico
dotnet run --project TestHelpers/DetectVSCode.csproj
# Risultato atteso: Path VS Code trovato

# Test 4: Test fallback su altri editor
EDITOR=nano dotnet run --project TestHelpers/OpenInEditor.csproj test.txt
# Risultato atteso: File aperto nell'editor configurato
```

### 6. GraphViz Dot.exe

**Location:**
```
MdExplorer.Migrations/Version2022-01/M2022_01_02_001.cs:16
```

**Codice Problematico:**
```csharp
// ❌ ERRATO - .exe hardcoded
ValueString = @"Binaries\GraphWiz\binaries\dot.exe"
```

**Soluzione Cross-Platform:**
```csharp
// ✅ CORRETTO - GraphViz cross-platform
public static class GraphVizDetector
{
    public static string FindDotExecutable()
    {
        // 1. Check system installation
        var dotCommand = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "dot.exe" : "dot";
        
        if (IsCommandAvailable(dotCommand))
            return dotCommand;
            
        // 2. Check common locations
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var paths = new[] { "/usr/bin/dot", "/usr/local/bin/dot" };
            return paths.FirstOrDefault(File.Exists) ?? "dot";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Check bundled binaries
            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            var bundledPath = Path.Combine(appPath, "Binaries", "GraphWiz", "binaries", "dot.exe");
            if (File.Exists(bundledPath))
                return bundledPath;
        }
        
        return dotCommand;
    }
}
```

**Test di Verifica:**
```bash
# Test 1: Verificare GraphViz installato
dot -V
# Risultato atteso: GraphViz version

# Test 2: Test generazione grafico
echo "digraph G { A -> B }" | dot -Tpng > test.png
file test.png
# Risultato atteso: PNG image data

# Test 3: Test rilevamento automatico
dotnet run --project TestHelpers/DetectGraphViz.csproj
# Risultato atteso: Path dot trovato

# Test 4: Test PlantUML con GraphViz
java -jar plantuml.jar -testdot
# Risultato atteso: "Installation seems OK"
```

## ✅ Componenti Già Compatibili

### Git Integration
- **Status**: ✅ Funzionante
- **Note**: LibGit2Sharp e comandi git già cross-platform

### SSH Key Management
- **Status**: ✅ Funzionante
- **Note**: Usa `chmod` che è disponibile su Linux

### Linux Secret Service
- **Status**: ✅ Implementato
- **Note**: Supporto nativo per Linux credential store

## 🛠️ Helper Class Consigliata

```csharp
public static class CrossPlatformProcess
{
    public static ProcessStartInfo CreateShellCommand(string command)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return new ProcessStartInfo("cmd.exe", $"/c \"{command}\"");
        else
            return new ProcessStartInfo("/bin/sh", $"-c \"{command}\"");
    }
    
    public static void OpenFileManager(string path)
    {
        string command;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            command = "explorer.exe";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            command = "xdg-open";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            command = "open";
        else
            throw new PlatformNotSupportedException();
            
        Process.Start(command, $"\"{path}\"");
    }
    
    public static void OpenInEditor(string filePath)
    {
        var editor = Environment.GetEnvironmentVariable("EDITOR") ?? 
                    EditorDetector.FindVSCode() ?? 
                    (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "notepad" : "nano");
                    
        Process.Start(editor, $"\"{filePath}\"");
    }
    
    public static bool IsCommandAvailable(string command)
    {
        var finder = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "where" : "which";
        
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = finder,
                Arguments = command,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            
            using var process = Process.Start(psi);
            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
```

## 📝 Script di Test Integrazione

```bash
#!/bin/bash
# test-external-processes.sh

echo "=== Testing External Processes on Linux ==="

# Test 1: Shell commands
echo "Test 1: Shell command execution"
/bin/sh -c "echo 'Shell test passed'"

# Test 2: File manager
echo "Test 2: File manager"
if which xdg-open > /dev/null; then
    echo "✅ xdg-open available"
else
    echo "❌ xdg-open not found"
fi

# Test 3: Java
echo "Test 3: Java detection"
if java -version 2>&1 | grep -q "version"; then
    echo "✅ Java available"
    java -version 2>&1 | head -1
else
    echo "❌ Java not found"
fi

# Test 4: Pandoc
echo "Test 4: Pandoc"
if which pandoc > /dev/null; then
    echo "✅ Pandoc available"
    pandoc --version | head -1
else
    echo "❌ Pandoc not found"
fi

# Test 5: VS Code
echo "Test 5: VS Code"
if which code > /dev/null; then
    echo "✅ VS Code available"
    code --version | head -1
else
    echo "⚠️ VS Code not found (optional)"
fi

# Test 6: GraphViz
echo "Test 6: GraphViz"
if which dot > /dev/null; then
    echo "✅ GraphViz available"
    dot -V 2>&1
else
    echo "❌ GraphViz not found"
fi

# Test 7: Git
echo "Test 7: Git"
if which git > /dev/null; then
    echo "✅ Git available"
    git --version
else
    echo "❌ Git not found"
fi

echo ""
echo "=== Summary ==="
echo "Run 'sudo apt-get install default-jre graphviz pandoc' to install missing dependencies"
```

## 📋 Checklist Migrazione Processi

### Fase 1: Shell Commands (1-2 giorni)
- [ ] Implementare CrossPlatformProcess helper
- [ ] Sostituire tutti i cmd.exe con shell wrapper
- [ ] Sostituire explorer.exe con xdg-open
- [ ] Test: Shell commands funzionano su Linux

### Fase 2: Auto-Detection (2-3 giorni)
- [ ] Implementare JavaDetector
- [ ] Implementare EditorDetector
- [ ] Implementare GraphVizDetector
- [ ] Test: Auto-detection trova programmi

### Fase 3: Database Migration (1 giorno)
- [ ] Creare migration per Java path
- [ ] Creare migration per VS Code path
- [ ] Creare migration per GraphViz path
- [ ] Test: Migration applicate correttamente

### Fase 4: Testing Completo (2-3 giorni)
- [ ] Test tutti i processi esterni su Linux
- [ ] Test export PDF con Pandoc
- [ ] Test diagrammi PlantUML
- [ ] Test apertura file in editor
- [ ] Test integrazione Git

## 🎯 Metriche di Successo

- **Zero cmd.exe references** nel codice
- **100% processi esterni** funzionanti su Linux
- **Auto-detection** trova tutti i programmi richiesti
- **Export PDF/Word** funzionante su Linux
- **PlantUML rendering** corretto
- **< 100ms overhead** per process detection