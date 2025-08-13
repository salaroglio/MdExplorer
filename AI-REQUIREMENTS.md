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
# Requisiti di Sistema per MdExplorer AI Chat

## Panoramica
MdExplorer include una funzionalità di chat AI che utilizza LLamaSharp per eseguire modelli di linguaggio localmente. Questo documento descrive i requisiti di sistema e le dipendenze necessarie.

## Requisiti Generali
- **.NET 8.0 SDK** o superiore
- **Node.js 14.21.3** per compilare il frontend Angular
- Almeno **8 GB di RAM** (16 GB consigliati per modelli più grandi)
- **4-8 GB di spazio disco** per i modelli AI

## Dipendenze per Linux

### Librerie di Sistema Richieste
Le seguenti librerie devono essere installate sul sistema:

| Libreria | Descrizione | Pacchetto Debian/Ubuntu | Pacchetto RedHat/Fedora |
|----------|-------------|------------------------|-------------------------|
| libgomp.so.1 | OpenMP runtime | libgomp1 | libgomp |
| libstdc++.so.6 | C++ standard library | libstdc++6 | libstdc++ |
| libm.so.6 | Math library | libc6 | glibc |
| libgcc_s.so.1 | GCC support library | libgcc-s1 | libgcc |
| libpthread.so.0 | POSIX threads | libc6 | glibc |
| libdl.so.2 | Dynamic linker | libc6 | glibc |
| librt.so.1 | Realtime extensions | libc6 | glibc |

### Installazione Automatica su Linux
Esegui lo script di installazione fornito:
```bash
chmod +x install-dependencies.sh
./install-dependencies.sh
```

### Installazione Manuale
#### Ubuntu/Debian:
```bash
sudo apt-get update
sudo apt-get install -y libgomp1 libstdc++6 libc6 build-essential
```

#### Fedora/RHEL:
```bash
sudo dnf install -y libgomp libstdc++ glibc gcc gcc-c++
```

#### Arch Linux:
```bash
sudo pacman -S base-devel gcc
```

### Configurazione LD_LIBRARY_PATH
Le librerie native di LLamaSharp richiedono che `LD_LIBRARY_PATH` sia configurato correttamente:

```bash
export LD_LIBRARY_PATH="/path/to/MdExplorer/bin/Debug/net8.0/linux-x64:$LD_LIBRARY_PATH"
```

Oppure usa lo script fornito:
```bash
./run-mdexplorer.sh
```

## Dipendenze per Windows

### Librerie Incluse
Il pacchetto NuGet `LLamaSharp.Backend.Cpu` include automaticamente tutte le librerie native necessarie per Windows:

- **libllama.dll** - Libreria principale LLaMA
- **libggml.dll** - Libreria di supporto per operazioni matematiche
- **Varianti AVX** - Versioni ottimizzate per CPU con supporto AVX/AVX2/AVX512

### Requisiti Windows
- **Visual C++ Redistributable 2019** o superiore
- **Windows 10/11** (64-bit)

Le librerie vengono automaticamente copiate nella directory di output durante la compilazione.

## Librerie Native Fornite

### Linux (linux-x64)
```
libggml.so       - Libreria matematica GPU/CPU
libllama.so      - Implementazione LLaMA
libllava_shared.so - Supporto multimodale (opzionale)
libSkiaSharp.so  - Rendering grafico
libgit2-*.so     - Operazioni Git
```

### Windows (win-x64)
```
libggml.dll      - Libreria matematica GPU/CPU
libllama.dll     - Implementazione LLaMA
└── avx/         - Versione ottimizzata AVX
└── avx2/        - Versione ottimizzata AVX2
└── avx512/      - Versione ottimizzata AVX512
libSkiaSharp.dll - Rendering grafico
git2-*.dll       - Operazioni Git
sni.dll          - SQL Native Interface
```

## Modelli AI Supportati

MdExplorer supporta modelli in formato GGUF (quantizzati):
- **Phi-3 Mini** (2.3 GB) - Consigliato per uso generale
- **LLaMA 2** (varie dimensioni)
- **Mistral** (varie dimensioni)
- **Qwen** (varie dimensioni)

## Risoluzione Problemi

### Linux: "The native library cannot be correctly loaded"
1. Verifica che tutte le dipendenze siano installate: `./install-dependencies.sh`
2. Assicurati che LD_LIBRARY_PATH sia configurato correttamente
3. Controlla le dipendenze mancanti: `ldd /path/to/libllama.so`

### Windows: Errore di caricamento DLL
1. Installa Visual C++ Redistributable 2019
2. Verifica che le DLL siano nella directory di output
3. Assicurati di usare Windows a 64-bit

### Errore: "NullReferenceException in InferAsync"
Questo è un problema noto risolto nel codice. Assicurati che `SamplingPipeline` sia configurato in `InferenceParams`.

## Performance

### Ottimizzazione CPU
- Il sistema seleziona automaticamente la versione ottimale delle librerie (AVX512 > AVX2 > AVX > baseline)
- Per Linux, libgomp fornisce parallelizzazione OpenMP

### Utilizzo Memoria
- I modelli vengono caricati completamente in RAM
- Phi-3 Mini richiede circa 3-4 GB di RAM disponibile
- Il context size predefinito è 4096 token

## Build da Sorgente

Se necessario compilare le librerie native da sorgente:

1. Clona llama.cpp: `git clone https://github.com/ggerganov/llama.cpp`
2. Checkout del commit compatibile con LLamaSharp 0.9.1
3. Compila: `make LLAMA_OPENBLAS=1` (Linux) o usa CMake (Windows)
4. Copia le librerie nella directory appropriata

## Supporto

Per problemi relativi all'integrazione AI:
1. Controlla i log in `MdExplorer/Logs/`
2. Verifica le dipendenze con `install-dependencies.sh`
3. Consulta la documentazione LLamaSharp: https://scisharp.github.io/LLamaSharp/