# Analisi Electron per Linux

## 📊 Tabella Riassuntiva Electron

| Componente | Severità | Problema | Azione | Stato | Test | Effort |
|------------|----------|----------|---------|-------|------|--------|
| **Build config Windows-only** | 🔴 Critico | Solo target NSIS Windows | Aggiungere config Linux | ❌ Da fare | ❌ | 4-6 ore |
| **.exe hardcoded paths** | 🔴 Critico | MdExplorer.Service.exe | Gestione cross-platform | ❌ Da fare | ❌ | 3-4 ore |
| **RuntimeIdentifier win10-x64** | 🔴 Critico | .NET service Windows-only | Multi-target build | ❌ Da fare | ❌ | 4-6 ore |
| **UseWindowsForms** | 🔴 Critico | Dipendenza Windows Forms | Rimuovere per Linux | ❌ Da fare | ❌ | 2-3 ore |
| **Linux packaging missing** | ⚠️ Alto | No AppImage/deb/rpm | Configurare packaging | ❌ Da fare | ❌ | 4-6 ore |
| **Desktop integration** | ⚠️ Medio | No .desktop file | Creare desktop entry | ❌ Da fare | ❌ | 2 ore |
| **Post-build.bat** | ⚠️ Medio | Script Windows-only | Script cross-platform | ❌ Da fare | ❌ | 2 ore |
| **Icons** | ✅ Basso | PNG disponibile | Convertire formati | ❌ Da fare | ❌ | 1 ora |

### Totale Effort Stimato: 5-8 giorni (22-32 ore)

## 🔴 Problemi Critici (Blockers)

### 1. Executable Paths Hardcoded (.exe)

**Location:**
```
ElectronMdExplorer/index.js:115,123,131
```

**Codice Problematico:**
```javascript
// ❌ ERRATO - Windows executable hardcoded
serviceExecutablePath = path.join(baseResourcePath, 'app_service', 'MdExplorer.Service.exe');
serviceExecutablePath = path.join(devServicePayloadPath, 'MdExplorer.Service.exe');
serviceExecutablePath = path.join(devDistServicePath, 'MdExplorer.Service.exe');
```

**Soluzione Cross-Platform:**
```javascript
// ✅ CORRETTO - Cross-platform executable detection
const { platform } = process;

function getServiceExecutableName() {
  switch (platform) {
    case 'win32':
      return 'MdExplorer.Service.exe';
    case 'linux':
      return 'MdExplorer.Service';
    case 'darwin':
      return 'MdExplorer.Service';
    default:
      return 'MdExplorer.Service';
  }
}

function getServiceExecutablePath() {
  const executableName = getServiceExecutableName();
  
  if (app.isPackaged) {
    // Production path
    const baseResourcePath = process.resourcesPath;
    return path.join(baseResourcePath, 'app_service', executableName);
  } else {
    // Development paths
    const possiblePaths = [
      path.join(__dirname, 'dev_service_payload', executableName),
      path.join(__dirname, 'dev_dist_service', executableName),
      path.join(__dirname, '..', 'MdExplorer', 'bin', 'Debug', 'netcoreapp3.1', getRuntimeIdentifier(), executableName),
      path.join(__dirname, '..', 'MdExplorer', 'bin', 'Release', 'netcoreapp3.1', getRuntimeIdentifier(), executableName)
    ];
    
    for (const possiblePath of possiblePaths) {
      if (fs.existsSync(possiblePath)) {
        console.log(`Found service at: ${possiblePath}`);
        return possiblePath;
      }
    }
    
    throw new Error(`Service executable not found. Searched paths: ${possiblePaths.join(', ')}`);
  }
}

function getRuntimeIdentifier() {
  switch (process.platform) {
    case 'win32':
      return 'win10-x64';
    case 'linux':
      return 'linux-x64';
    case 'darwin':
      return 'osx-x64';
    default:
      return 'linux-x64';
  }
}

// Uso nel codice
const serviceExecutablePath = getServiceExecutablePath();

// Per spawn del processo
const serviceProcess = spawn(serviceExecutablePath, serviceArguments, {
  stdio: 'pipe',
  env: {
    ...process.env,
    ASPNETCORE_ENVIRONMENT: 'Production',
    ASPNETCORE_URLS: serviceUrl
  }
});
```

**Test di Verifica:**
```bash
# Test 1: Verificare nome eseguibile corretto
node -e "console.log(process.platform)"
# Risultato atteso: "linux" su Linux

# Test 2: Test path resolution
node test-service-path.js
# Risultato atteso: Path corretto per il sistema

# Test 3: Test service spawn
npm run test:service-spawn
# Risultato atteso: Servizio avviato correttamente

# Test 4: Verificare permessi esecuzione
ls -la dev_service_payload/MdExplorer.Service
# Risultato atteso: -rwxr-xr-x (755)
```

### 2. Electron Builder Configuration

**Location:**
```
ElectronMdExplorer/package.json
```

**Codice Problematico:**
```json
// ❌ ERRATO - Solo Windows configuration
"build": {
  "appId": "com.mdexplorer.app",
  "directories": {
    "output": "./Binaries"
  },
  "win": {
    "target": "nsis"
  }
}
```

**Soluzione Cross-Platform:**
```json
// ✅ CORRETTO - Multi-platform configuration
"build": {
  "appId": "com.mdexplorer.app",
  "productName": "MdExplorer",
  "directories": {
    "output": "./dist"
  },
  "files": [
    "**/*",
    "!**/*.ts",
    "!**/*.map",
    "!src",
    "!.gitignore",
    "!README.md"
  ],
  "extraResources": [
    {
      "from": "../MdExplorer/bin/Release/netcoreapp3.1/${os}-${arch}/publish",
      "to": "app_service",
      "filter": ["**/*"]
    }
  ],
  "win": {
    "target": [
      {
        "target": "nsis",
        "arch": ["x64"]
      },
      {
        "target": "portable",
        "arch": ["x64"]
      }
    ],
    "icon": "assets/icon.ico"
  },
  "linux": {
    "target": [
      {
        "target": "AppImage",
        "arch": ["x64"]
      },
      {
        "target": "deb",
        "arch": ["x64"]
      },
      {
        "target": "rpm",
        "arch": ["x64"]
      },
      {
        "target": "snap",
        "arch": ["x64"]
      }
    ],
    "icon": "assets/icon.png",
    "category": "Office",
    "synopsis": "Markdown file explorer and editor",
    "description": "A comprehensive markdown file management and editing application",
    "desktop": {
      "Name": "MdExplorer",
      "Comment": "Explore and edit Markdown files",
      "Categories": "Office;TextEditor;Development;",
      "MimeType": "text/markdown;text/x-markdown;",
      "Keywords": "markdown;editor;explorer;documentation;"
    },
    "executableArgs": ["--no-sandbox"]
  },
  "mac": {
    "target": [
      {
        "target": "dmg",
        "arch": ["x64", "arm64"]
      }
    ],
    "icon": "assets/icon.icns",
    "category": "public.app-category.developer-tools"
  },
  "deb": {
    "depends": [
      "libgtk-3-0",
      "libnotify4",
      "libnss3",
      "libxss1",
      "libxtst6",
      "xdg-utils",
      "libatspi2.0-0",
      "libuuid1",
      "libsecret-1-0"
    ],
    "recommends": [
      "graphviz",
      "default-jre",
      "pandoc"
    ]
  },
  "rpm": {
    "depends": [
      "gtk3",
      "libnotify",
      "nss",
      "libXScrnSaver",
      "libXtst",
      "xdg-utils",
      "at-spi2-atk",
      "libuuid",
      "libsecret"
    ]
  },
  "snap": {
    "confinement": "classic",
    "grade": "stable",
    "summary": "Markdown Explorer and Editor"
  },
  "appImage": {
    "systemIntegration": "ask"
  }
}
```

**Test di Verifica:**
```bash
# Test 1: Build per Linux AppImage
npm run build:linux -- --linux AppImage
# Risultato atteso: AppImage creato in dist/

# Test 2: Build per Linux deb
npm run build:linux -- --linux deb
# Risultato atteso: .deb package creato

# Test 3: Test AppImage execution
chmod +x dist/MdExplorer-*.AppImage
./dist/MdExplorer-*.AppImage
# Risultato atteso: Applicazione avviata

# Test 4: Verificare desktop integration
desktop-file-validate dist/linux-unpacked/mdexplorer.desktop
# Risultato atteso: No errors
```

### 3. .NET Service Multi-Platform Build

**Location:**
```
MdExplorer/MdExplorer.Service.csproj
```

**Soluzione - Build Script Cross-Platform:**
```json
// package.json scripts
{
  "scripts": {
    "build:service:win": "dotnet publish ../MdExplorer/MdExplorer.Service.csproj -c Release -r win10-x64 --self-contained -o dev_service_payload/win",
    "build:service:linux": "dotnet publish ../MdExplorer/MdExplorer.Service.csproj -c Release -r linux-x64 --self-contained -o dev_service_payload/linux",
    "build:service:mac": "dotnet publish ../MdExplorer/MdExplorer.Service.csproj -c Release -r osx-x64 --self-contained -o dev_service_payload/mac",
    "build:service:all": "npm run build:service:win && npm run build:service:linux && npm run build:service:mac",
    "prebuild": "npm run build:service:all",
    "build": "electron-builder",
    "build:win": "npm run build:service:win && electron-builder --win",
    "build:linux": "npm run build:service:linux && electron-builder --linux",
    "build:mac": "npm run build:service:mac && electron-builder --mac",
    "dist": "npm run build:service:all && electron-builder -mwl"
  }
}
```

**Build Script Shell (build-service.sh):**
```bash
#!/bin/bash
# build-service.sh - Cross-platform service build

CONFIGURATION=${1:-Release}
OUTPUT_DIR="dev_service_payload"

echo "Building MdExplorer Service for all platforms..."

# Detect current platform
case "$(uname -s)" in
    Linux*)     CURRENT_PLATFORM="linux-x64";;
    Darwin*)    CURRENT_PLATFORM="osx-x64";;
    MINGW*|CYGWIN*|MSYS*) CURRENT_PLATFORM="win10-x64";;
    *)          CURRENT_PLATFORM="linux-x64";;
esac

# Build for current platform
echo "Building for current platform: $CURRENT_PLATFORM"
dotnet publish ../MdExplorer/MdExplorer.Service.csproj \
    -c $CONFIGURATION \
    -r $CURRENT_PLATFORM \
    --self-contained \
    -p:PublishSingleFile=true \
    -p:UseWindowsForms=false \
    -o "$OUTPUT_DIR/$CURRENT_PLATFORM"

# Set executable permissions on Linux/Mac
if [[ "$CURRENT_PLATFORM" != "win10-x64" ]]; then
    chmod +x "$OUTPUT_DIR/$CURRENT_PLATFORM/MdExplorer.Service"
fi

echo "Build complete!"
```

**Test di Verifica:**
```bash
# Test 1: Build servizio per Linux
bash build-service.sh Release
# Risultato atteso: Servizio built in dev_service_payload/linux-x64/

# Test 2: Verificare eseguibile
file dev_service_payload/linux-x64/MdExplorer.Service
# Risultato atteso: ELF 64-bit executable

# Test 3: Test avvio servizio
./dev_service_payload/linux-x64/MdExplorer.Service --version
# Risultato atteso: Version info displayed

# Test 4: Test self-contained
ldd dev_service_payload/linux-x64/MdExplorer.Service
# Risultato atteso: Minimal external dependencies
```

## ⚠️ Problemi Alti

### 4. Desktop Entry File

**Nuovo File Richiesto:**
```
ElectronMdExplorer/assets/linux/mdexplorer.desktop
```

**Desktop Entry File:**
```ini
[Desktop Entry]
Name=MdExplorer
Comment=Markdown Explorer and Editor
Exec=mdexplorer %U
Terminal=false
Type=Application
Icon=mdexplorer
Categories=Office;TextEditor;Development;
MimeType=text/markdown;text/x-markdown;text/plain;
Keywords=markdown;editor;explorer;documentation;notes;
StartupNotify=true
StartupWMClass=MdExplorer
Actions=new-window;

[Desktop Action new-window]
Name=New Window
Exec=mdexplorer --new-window
```

**Test di Verifica:**
```bash
# Test 1: Validare desktop file
desktop-file-validate assets/linux/mdexplorer.desktop
# Risultato atteso: No output (valid)

# Test 2: Installare desktop file (test)
cp assets/linux/mdexplorer.desktop ~/.local/share/applications/
update-desktop-database ~/.local/share/applications/
# Risultato atteso: App appears in menu

# Test 3: Test MIME association
xdg-mime query default text/markdown
# Risultato atteso: mdexplorer.desktop (dopo installazione)

# Test 4: Test launch from desktop
gtk-launch mdexplorer
# Risultato atteso: App launches
```

### 5. Icon Preparation

**Script per preparare icone multi-platform:**
```bash
#!/bin/bash
# prepare-icons.sh

SOURCE_ICON="assets/IconReady.png"

if [ ! -f "$SOURCE_ICON" ]; then
    echo "Source icon not found: $SOURCE_ICON"
    exit 1
fi

echo "Preparing icons for all platforms..."

# Create icons directory
mkdir -p assets/icons

# Linux icons (PNG various sizes)
for size in 16 24 32 48 64 128 256 512 1024; do
    convert "$SOURCE_ICON" -resize ${size}x${size} "assets/icons/icon_${size}x${size}.png"
done

# Windows icon (ICO)
convert "$SOURCE_ICON" -resize 256x256 assets/icon.ico

# macOS icon (ICNS)
if command -v png2icns &> /dev/null; then
    png2icns assets/icon.icns assets/icons/icon_*.png
else
    echo "png2icns not found, skipping macOS icon"
fi

# Copy main icon for Linux
cp "$SOURCE_ICON" assets/icon.png

echo "Icons prepared successfully!"
```

**Test di Verifica:**
```bash
# Test 1: Generare icone
bash prepare-icons.sh
# Risultato atteso: Icons created in assets/

# Test 2: Verificare icone generate
ls -la assets/icons/
# Risultato atteso: Multiple PNG sizes

# Test 3: Verificare formato ICO
file assets/icon.ico
# Risultato atteso: MS Windows icon resource

# Test 4: Verificare PNG principale
identify assets/icon.png
# Risultato atteso: PNG image data, dimensions shown
```

## 🛠️ Helper Scripts

### Post-Build Script Cross-Platform

**post-build.js (sostituisce post-build.bat):**
```javascript
#!/usr/bin/env node

const fs = require('fs-extra');
const path = require('path');
const { execSync } = require('child_process');

const platform = process.platform;
const arch = process.arch;

console.log(`Post-build script running on ${platform}-${arch}`);

async function postBuild() {
  try {
    // Copy service binaries
    const servicePath = path.join(__dirname, '..', 'MdExplorer', 'bin', 'Release', 'netcoreapp3.1');
    const runtimeId = getRuntimeId(platform);
    const sourceService = path.join(servicePath, runtimeId, 'publish');
    const targetService = path.join(__dirname, 'resources', 'app_service');
    
    console.log(`Copying service from ${sourceService} to ${targetService}`);
    await fs.copy(sourceService, targetService);
    
    // Set executable permissions on Unix
    if (platform !== 'win32') {
      const executable = path.join(targetService, 'MdExplorer.Service');
      console.log(`Setting executable permissions for ${executable}`);
      execSync(`chmod +x "${executable}"`);
    }
    
    // Copy additional resources
    const resourcesToCopy = [
      { from: '../MdExplorer/templates', to: 'resources/templates' },
      { from: '../MdExplorer/Binaries/plantuml.jar', to: 'resources/plantuml.jar' }
    ];
    
    for (const resource of resourcesToCopy) {
      const source = path.join(__dirname, resource.from);
      const target = path.join(__dirname, resource.to);
      if (await fs.pathExists(source)) {
        console.log(`Copying ${source} to ${target}`);
        await fs.copy(source, target);
      }
    }
    
    console.log('Post-build completed successfully!');
  } catch (error) {
    console.error('Post-build failed:', error);
    process.exit(1);
  }
}

function getRuntimeId(platform) {
  switch (platform) {
    case 'win32': return 'win10-x64';
    case 'linux': return 'linux-x64';
    case 'darwin': return 'osx-x64';
    default: return 'linux-x64';
  }
}

postBuild();
```

**Test di Verifica:**
```bash
# Test 1: Run post-build script
node post-build.js
# Risultato atteso: Resources copied successfully

# Test 2: Verificare permessi eseguibile
ls -la resources/app_service/MdExplorer.Service
# Risultato atteso: -rwxr-xr-x

# Test 3: Verificare risorse copiate
ls -la resources/
# Risultato atteso: templates/, plantuml.jar presenti

# Test 4: Test completo build
npm run build:linux
# Risultato atteso: Build completo con post-build
```

## 📝 Installation Dependencies Script

```bash
#!/bin/bash
# install-linux-deps.sh

echo "Installing MdExplorer Electron dependencies for Linux..."

# Detect distribution
if [ -f /etc/debian_version ]; then
    # Debian/Ubuntu
    echo "Detected Debian/Ubuntu system"
    sudo apt-get update
    sudo apt-get install -y \
        build-essential \
        libgtk-3-0 \
        libnotify4 \
        libnss3 \
        libxss1 \
        libxtst6 \
        xdg-utils \
        libatspi2.0-0 \
        libuuid1 \
        libsecret-1-0 \
        graphviz \
        default-jre \
        pandoc \
        imagemagick
        
elif [ -f /etc/redhat-release ]; then
    # RedHat/CentOS/Fedora
    echo "Detected RedHat/Fedora system"
    sudo dnf install -y \
        gtk3 \
        libnotify \
        nss \
        libXScrnSaver \
        libXtst \
        xdg-utils \
        at-spi2-atk \
        libuuid \
        libsecret \
        graphviz \
        java-latest-openjdk \
        pandoc \
        ImageMagick
        
elif [ -f /etc/arch-release ]; then
    # Arch Linux
    echo "Detected Arch Linux system"
    sudo pacman -S --noconfirm \
        gtk3 \
        libnotify \
        nss \
        libxss \
        libxtst \
        xdg-utils \
        at-spi2-atk \
        libsecret \
        graphviz \
        jre-openjdk \
        pandoc \
        imagemagick
else
    echo "Unsupported distribution"
    echo "Please install dependencies manually"
    exit 1
fi

# Install .NET if not present
if ! command -v dotnet &> /dev/null; then
    echo "Installing .NET SDK..."
    wget https://dot.net/v1/dotnet-install.sh
    chmod +x dotnet-install.sh
    ./dotnet-install.sh --channel 3.1
    rm dotnet-install.sh
fi

echo "Dependencies installation complete!"
```

## 📋 Checklist Migrazione Electron

### Fase 1: Core Fixes (8-10 ore)
- [ ] Implementare getServiceExecutablePath() cross-platform
- [ ] Rimuovere hardcoded .exe references
- [ ] Testare spawn processo su Linux
- [ ] Verificare comunicazione IPC

### Fase 2: Build Configuration (6-8 ore)
- [ ] Aggiungere configurazione Linux in package.json
- [ ] Configurare targets (AppImage, deb, rpm)
- [ ] Preparare icone multi-platform
- [ ] Creare desktop entry file

### Fase 3: Service Build (6-8 ore)
- [ ] Configurare multi-target .NET build
- [ ] Rimuovere UseWindowsForms per Linux
- [ ] Script build cross-platform
- [ ] Test servizio su Linux

### Fase 4: Packaging & Distribution (4-6 ore)
- [ ] Test build AppImage
- [ ] Test build deb package
- [ ] Test build rpm package
- [ ] Verificare installazione e update

### Fase 5: Testing Completo (4-6 ore)
- [ ] Test su Ubuntu 20.04/22.04
- [ ] Test su Fedora
- [ ] Test su Arch Linux
- [ ] Test auto-update mechanism
- [ ] Performance testing

## 🎯 Metriche di Successo

- **Build multi-platform** funzionante
- **Zero hardcoded** .exe references
- **Packaging Linux** (AppImage, deb, rpm) funzionante
- **Desktop integration** completa
- **Performance** comparabile a Windows
- **< 50MB** dimensione AppImage
- **< 5 secondi** startup time