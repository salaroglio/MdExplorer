# Analisi Configurazioni e AppSettings per Linux

## 📊 Tabella Riassuntiva Configurazioni

| Configurazione | Severità | File | Azione | Stato | Test | Effort |
|----------------|----------|------|--------|-------|------|--------|
| **RuntimeIdentifier win10-x64** | 🔴 Critico | .csproj | Rendere condizionale | ❌ Da fare | ❌ | 30 min |
| **UseWindowsForms=true** | 🔴 Critico | .csproj | Rimuovere per Linux | ❌ Da fare | ❌ | 15 min |
| **LocalAppData paths** | 🔴 Critico | ProjectsManager.cs | Usare XDG paths | ❌ Da fare | ❌ | 2-3 ore |
| **Windows binaries embedded** | 🔴 Critico | .csproj | Escludere su Linux | ❌ Da fare | ❌ | 1-2 giorni |
| **Path Windows in launchSettings** | ⚠️ Alto | launchSettings.json | Creare profilo Linux | ❌ Da fare | ❌ | 1 ora |
| **IIS Express profile** | ⚠️ Medio | launchSettings.json | Rimuovere per Linux | ❌ Da fare | ❌ | 15 min |
| **PlantUML server IP** | ⚠️ Medio | appsettings.json | Configurabile | ❌ Da fare | ❌ | 30 min |
| **SSH key paths ~/** | ✅ OK | appsettings.json | Già compatibile | ✅ OK | ❌ | - |

### Totale Effort Stimato: 2-3 giorni

## 🔴 Problemi Critici (Blockers)

### 1. RuntimeIdentifier e UseWindowsForms

**Location:**
```
MdExplorer/MdExplorer.Service.csproj
```

**Codice Problematico:**
```xml
<!-- ❌ ERRATO - Hard-coded per Windows -->
<PropertyGroup>
  <RuntimeIdentifier>win10-x64</RuntimeIdentifier>
  <UseWindowsForms>true</UseWindowsForms>
</PropertyGroup>
```

**Soluzione Cross-Platform:**
```xml
<!-- ✅ CORRETTO - Condizionale per OS -->
<PropertyGroup>
  <!-- Configurazioni comuni -->
  <TargetFramework>netcoreapp3.1</TargetFramework>
  <AspNetCoreHostingModel>InProcess</AspNetCoreHostingModel>
</PropertyGroup>

<!-- Configurazioni Windows-specific -->
<PropertyGroup Condition="$([MSBuild]::IsOSPlatform('Windows'))">
  <RuntimeIdentifier>win10-x64</RuntimeIdentifier>
  <UseWindowsForms>true</UseWindowsForms>
</PropertyGroup>

<!-- Configurazioni Linux-specific -->
<PropertyGroup Condition="$([MSBuild]::IsOSPlatform('Linux'))">
  <RuntimeIdentifier>linux-x64</RuntimeIdentifier>
  <UseWindowsForms>false</UseWindowsForms>
</PropertyGroup>

<!-- Configurazioni macOS-specific -->
<PropertyGroup Condition="$([MSBuild]::IsOSPlatform('OSX'))">
  <RuntimeIdentifier>osx-x64</RuntimeIdentifier>
  <UseWindowsForms>false</UseWindowsForms>
</PropertyGroup>
```

**Test di Verifica:**
```bash
# Test 1: Build su Linux
dotnet build -r linux-x64
# Risultato atteso: Build successful

# Test 2: Verificare runtime identifier
dotnet msbuild -t:ShowRuntimeIdentifier /p:ShowRuntimeIdentifier=true
# Risultato atteso: linux-x64

# Test 3: Verificare assenza Windows Forms
dotnet build 2>&1 | grep -i "windowsforms"
# Risultato atteso: Nessun riferimento a Windows Forms

# Test 4: Build cross-platform
dotnet publish -r linux-x64 --self-contained
dotnet publish -r win10-x64 --self-contained
# Risultato atteso: Entrambi i build completati
```

### 2. LocalAppData Path Management

**Location:**
```
MdExplorer/ProjectsManager.cs:34,65
```

**Codice Problematico:**
```csharp
// ❌ ERRATO - Windows-specific
var appdata = Environment.GetEnvironmentVariable("LocalAppData");
var databasePath = $@"Data Source = {appdata + Path.DirectorySeparatorChar}MdExplorer.db";
```

**Soluzione Cross-Platform:**
```csharp
// ✅ CORRETTO - Cross-platform data directory
public static class DataDirectoryManager
{
    public static string GetDataDirectory()
    {
        string dataDir;
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Windows: %LocalAppData%
            dataDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Linux: Follow XDG Base Directory Specification
            dataDir = Environment.GetEnvironmentVariable("XDG_DATA_HOME") ??
                     Path.Combine(Environment.GetEnvironmentVariable("HOME"), ".local", "share");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // macOS: ~/Library/Application Support
            dataDir = Path.Combine(Environment.GetEnvironmentVariable("HOME"), 
                                   "Library", "Application Support");
        }
        else
        {
            // Fallback
            dataDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }
        
        return Path.Combine(dataDir, "MdExplorer");
    }
    
    public static string GetDatabasePath(string dbName = "MdExplorer.db")
    {
        var dataDir = GetDataDirectory();
        Directory.CreateDirectory(dataDir); // Ensure directory exists
        return Path.Combine(dataDir, dbName);
    }
    
    public static string GetLogDirectory()
    {
        var dataDir = GetDataDirectory();
        var logDir = Path.Combine(dataDir, "Logs");
        Directory.CreateDirectory(logDir);
        return logDir;
    }
    
    public static string GetTempDirectory()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "MdExplorer");
        Directory.CreateDirectory(tempDir);
        return tempDir;
    }
}

// Uso nel ProjectsManager
public class ProjectsManager
{
    private string GetConnectionString()
    {
        var dbPath = DataDirectoryManager.GetDatabasePath();
        return $"Data Source={dbPath}";
    }
}
```

**Test di Verifica:**
```bash
# Test 1: Verificare XDG_DATA_HOME
echo $XDG_DATA_HOME
echo $HOME/.local/share
# Risultato atteso: Path valido per data directory

# Test 2: Test creazione directory
dotnet run --project TestHelpers/TestDataDirectory.csproj
ls -la ~/.local/share/MdExplorer/
# Risultato atteso: Directory creata con permessi corretti

# Test 3: Test database creation
dotnet test --filter "FullyQualifiedName~DataDirectoryManager"
file ~/.local/share/MdExplorer/MdExplorer.db
# Risultato atteso: SQLite database

# Test 4: Verificare permessi
stat -c "%a" ~/.local/share/MdExplorer/
# Risultato atteso: 755 o 700
```

### 3. Windows Binaries Embedded

**Location:**
```
MdExplorer/MdExplorer.Service.csproj
```

**Codice Problematico:**
```xml
<!-- ❌ ERRATO - Binari Windows embedded -->
<ItemGroup>
  <EmbeddedResource Include="Binaries\GraphWiz\binaries\*.dll">
    <CopyToOutputDirectory>Always</CopyToOutputDirectory>
  </EmbeddedResource>
  <EmbeddedResource Include="Binaries\GraphWiz\binaries\dot.exe">
    <CopyToOutputDirectory>Always</CopyToOutputDirectory>
  </EmbeddedResource>
</ItemGroup>
```

**Soluzione Cross-Platform:**
```xml
<!-- ✅ CORRETTO - Binari condizionali per OS -->
<!-- Binari Windows -->
<ItemGroup Condition="$([MSBuild]::IsOSPlatform('Windows'))">
  <Content Include="Binaries\Windows\GraphViz\**\*">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>

<!-- Configurazione Linux (usa sistema) -->
<ItemGroup Condition="$([MSBuild]::IsOSPlatform('Linux'))">
  <!-- Linux usa GraphViz di sistema, non embedded -->
  <None Include="Scripts\Linux\install-dependencies.sh">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>

<!-- PlantUML JAR cross-platform -->
<ItemGroup>
  <Content Include="Binaries\plantuml.jar">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

**Script installazione dipendenze Linux:**
```bash
#!/bin/bash
# install-dependencies.sh

echo "Installing MdExplorer Linux dependencies..."

# Detect package manager
if command -v apt-get &> /dev/null; then
    # Debian/Ubuntu
    sudo apt-get update
    sudo apt-get install -y graphviz default-jre pandoc texlive-xetex
elif command -v dnf &> /dev/null; then
    # Fedora
    sudo dnf install -y graphviz java-latest-openjdk pandoc texlive-xetex
elif command -v pacman &> /dev/null; then
    # Arch
    sudo pacman -S --noconfirm graphviz jre-openjdk pandoc texlive-core
else
    echo "Unsupported package manager. Please install manually:"
    echo "- graphviz"
    echo "- Java Runtime (JRE)"
    echo "- pandoc"
    echo "- XeLaTeX (for PDF export)"
fi

echo "Dependencies installation complete."
```

**Test di Verifica:**
```bash
# Test 1: Verificare esclusione binari Windows
ls MdExplorer/bin/Debug/netcoreapp3.1/linux-x64/Binaries/*.dll 2>/dev/null | wc -l
# Risultato atteso: 0

# Test 2: Verificare PlantUML JAR presente
ls MdExplorer/bin/Debug/netcoreapp3.1/linux-x64/Binaries/plantuml.jar
# Risultato atteso: File exists

# Test 3: Test GraphViz di sistema
dot -V
# Risultato atteso: GraphViz version

# Test 4: Test script installazione
bash ./Scripts/Linux/install-dependencies.sh
# Risultato atteso: Dipendenze installate
```

## ⚠️ Problemi Alti

### 4. LaunchSettings con Path Windows

**Location:**
```
MdExplorer/Properties/launchSettings.json
```

**Codice Problematico:**
```json
{
  "profiles": {
    "MdExplorer": {
      "commandLineArgs": "C:\\Users\\Carlo\\Documents\\test.md"
    }
  }
}
```

**Soluzione - Profili Environment-Specific:**
```json
{
  "profiles": {
    "MdExplorer.Windows": {
      "commandName": "Project",
      "commandLineArgs": "C:\\Users\\Carlo\\Documents\\test.md",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "MdExplorer.Linux": {
      "commandName": "Project",
      "commandLineArgs": "/home/user/Documents/test.md",
      "launchBrowser": true,
      "launchUrl": "client2/index.html",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "ASPNETCORE_URLS": "http://localhost:5000"
      }
    },
    "MdExplorer.Docker": {
      "commandName": "Docker",
      "launchBrowser": true,
      "launchUrl": "{Scheme}://{ServiceHost}:{ServicePort}/client2",
      "publishAllPorts": true,
      "useSSL": false
    }
  }
}
```

**Test di Verifica:**
```bash
# Test 1: Launch con profilo Linux
dotnet run --launch-profile "MdExplorer.Linux"
# Risultato atteso: Applicazione avviata con path Linux

# Test 2: Verificare variabili ambiente
dotnet run --launch-profile "MdExplorer.Linux" -- --show-env
# Risultato atteso: ASPNETCORE_ENVIRONMENT=Development

# Test 3: Test porta binding
curl http://localhost:5000/health
# Risultato atteso: 200 OK

# Test 4: Test con file di test
touch /tmp/test.md
dotnet run -- /tmp/test.md
# Risultato atteso: File aperto correttamente
```

## ⚠️ Problemi Medi

### 5. PlantUML Server Configuration

**Location:**
```
MdExplorer/appsettings.json
```

**Codice Problematico:**
```json
{
  "MdExplorer": {
    "PlantumlServer": "172.19.243.45"
  }
}
```

**Soluzione - Configurazione Environment-Aware:**

**appsettings.json (default):**
```json
{
  "MdExplorer": {
    "PlantumlServer": "localhost:8080",
    "PlantumlServerType": "local"
  }
}
```

**appsettings.Development.json:**
```json
{
  "MdExplorer": {
    "PlantumlServer": "localhost:8080",
    "PlantumlServerType": "local",
    "PlantumlJarPath": "Binaries/plantuml.jar"
  }
}
```

**appsettings.Production.json:**
```json
{
  "MdExplorer": {
    "PlantumlServer": "${PLANTUML_SERVER}",
    "PlantumlServerType": "remote"
  }
}
```

**Docker Compose per PlantUML Server:**
```yaml
version: '3.8'
services:
  plantuml:
    image: plantuml/plantuml-server:jetty
    ports:
      - "8080:8080"
    environment:
      - PLANTUML_LIMIT_SIZE=8192
    restart: unless-stopped
```

**Test di Verifica:**
```bash
# Test 1: Avviare PlantUML server locale
docker-compose up -d plantuml
curl http://localhost:8080
# Risultato atteso: PlantUML server page

# Test 2: Test generazione diagramma
echo "@startuml\nA -> B\n@enduml" | curl -X POST --data-binary @- http://localhost:8080/svg
# Risultato atteso: SVG output

# Test 3: Test configurazione da environment
PLANTUML_SERVER=http://remote:8080 dotnet run
# Risultato atteso: Usa server remoto

# Test 4: Test fallback a JAR locale
dotnet test --filter "FullyQualifiedName~PlantUMLConfiguration"
# Risultato atteso: Fallback a JAR quando server non disponibile
```

## 🛠️ Template Configurazione Linux Completo

### appsettings.Linux.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    },
    "Console": {
      "FormatterName": "simple",
      "FormatterOptions": {
        "SingleLine": true,
        "TimestampFormat": "yyyy-MM-dd HH:mm:ss "
      }
    }
  },
  "AllowedHosts": "*",
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5000"
      }
    }
  },
  "MdExplorer": {
    "CreateStartupLog": false,
    "DataDirectory": "~/.local/share/MdExplorer",
    "TempDirectory": "/tmp/MdExplorer",
    "LogDirectory": "~/.local/share/MdExplorer/Logs",
    "PlantumlServer": "localhost:8080",
    "PlantumlServerType": "local",
    "PlantumlJarPath": "Binaries/plantuml.jar",
    "JavaExecutable": "java",
    "PandocExecutable": "pandoc",
    "GraphVizDot": "dot",
    "DefaultEditor": "code",
    "FileManager": "xdg-open",
    "JiraServer": "${JIRA_SERVER}"
  },
  "Git": {
    "Authentication": {
      "PreferredMethods": ["SSH", "GitCredentialHelper"],
      "AllowUserPrompt": false,
      "SSHKeySearchPaths": [
        "~/.ssh/id_ed25519",
        "~/.ssh/id_ecdsa",
        "~/.ssh/id_rsa",
        "~/.ssh/id_dsa"
      ],
      "CredentialHelper": "libsecret"
    }
  },
  "StaticFiles": {
    "ServeUnknownFileTypes": true,
    "DefaultContentType": "application/octet-stream"
  }
}
```

### Environment Variables (.env)
```bash
# MdExplorer Linux Environment Variables
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS=http://localhost:5000
export XDG_DATA_HOME=$HOME/.local/share
export MDEXPLORER_DATA=$XDG_DATA_HOME/MdExplorer
export PLANTUML_SERVER=http://localhost:8080
export JAVA_HOME=/usr/lib/jvm/default
export EDITOR=code
```

### Systemd Service (mdexplorer.service)
```ini
[Unit]
Description=MdExplorer Markdown Management Service
After=network.target

[Service]
Type=simple
User=mdexplorer
WorkingDirectory=/opt/mdexplorer
ExecStart=/usr/bin/dotnet /opt/mdexplorer/MdExplorer.Service.dll
Restart=on-failure
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=mdexplorer
Environment="ASPNETCORE_ENVIRONMENT=Production"
Environment="ASPNETCORE_URLS=http://localhost:5000"

[Install]
WantedBy=multi-user.target
```

**Test di Verifica Systemd:**
```bash
# Test 1: Installare servizio
sudo cp mdexplorer.service /etc/systemd/system/
sudo systemctl daemon-reload
# Risultato atteso: Servizio registrato

# Test 2: Avviare servizio
sudo systemctl start mdexplorer
sudo systemctl status mdexplorer
# Risultato atteso: Active (running)

# Test 3: Verificare logs
journalctl -u mdexplorer -f
# Risultato atteso: Logs senza errori

# Test 4: Test auto-start
sudo systemctl enable mdexplorer
sudo reboot
# Risultato atteso: Servizio avviato automaticamente
```

## 📝 Script di Migrazione Configurazioni

```bash
#!/bin/bash
# migrate-config.sh

echo "=== Migrating MdExplorer Configuration for Linux ==="

# 1. Create Linux config directory
CONFIG_DIR="$HOME/.config/MdExplorer"
mkdir -p "$CONFIG_DIR"

# 2. Create data directory
DATA_DIR="$HOME/.local/share/MdExplorer"
mkdir -p "$DATA_DIR"
mkdir -p "$DATA_DIR/Logs"
mkdir -p "$DATA_DIR/Temp"

# 3. Copy and adapt configuration
if [ -f "appsettings.json" ]; then
    cp appsettings.json "$CONFIG_DIR/appsettings.json"
    
    # Create Linux-specific settings
    cat > "$CONFIG_DIR/appsettings.Linux.json" << 'EOF'
{
  "MdExplorer": {
    "DataDirectory": "~/.local/share/MdExplorer",
    "PlantumlServer": "localhost:8080",
    "JavaExecutable": "java",
    "FileManager": "xdg-open"
  }
}
EOF
fi

# 4. Set permissions
chmod 700 "$DATA_DIR"
chmod 755 "$CONFIG_DIR"

# 5. Create environment file
cat > "$CONFIG_DIR/mdexplorer.env" << EOF
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://localhost:5000
MDEXPLORER_DATA=$DATA_DIR
EOF

echo "Configuration migration complete!"
echo "Data directory: $DATA_DIR"
echo "Config directory: $CONFIG_DIR"
```

## 📋 Checklist Migrazione Configurazioni

### Fase 1: Project Files (30 min)
- [ ] Rendere RuntimeIdentifier condizionale
- [ ] Rimuovere UseWindowsForms per Linux
- [ ] Escludere binari Windows su Linux
- [ ] Test: Build su Linux funziona

### Fase 2: Data Directories (2-3 ore)
- [ ] Implementare DataDirectoryManager
- [ ] Migrare ProjectsManager
- [ ] Seguire XDG Base Directory spec
- [ ] Test: Database creato in path corretto

### Fase 3: Launch Profiles (1 ora)
- [ ] Creare profilo Linux in launchSettings
- [ ] Rimuovere path Windows hardcoded
- [ ] Configurare environment variables
- [ ] Test: Launch profile Linux funziona

### Fase 4: AppSettings (1 ora)
- [ ] Creare appsettings.Linux.json
- [ ] Configurare PlantUML per Linux
- [ ] Configurare path eseguibili
- [ ] Test: Configurazioni caricate correttamente

### Fase 5: Deployment (2 ore)
- [ ] Creare systemd service
- [ ] Configurare Docker compose
- [ ] Script installazione dipendenze
- [ ] Test: Servizio production-ready

## 🎯 Metriche di Successo

- **Zero hard-coded Windows paths** in configurazioni
- **100% configurazioni** environment-specific
- **XDG compliance** per directory Linux
- **Systemd service** funzionante
- **Docker deployment** disponibile
- **< 5 secondi** startup time su Linux