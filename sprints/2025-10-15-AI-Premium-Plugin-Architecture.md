---
author: Carlo Salaroglio
document_type: Document
email: developer@mdexplorer.net
title:
date: 15/10/2025
word_section:
  write_toc: false
  document_header: ''
  template_section:
    inherit_from_template: ''
    custom_template: ''
    template_type: default
  predefined_pages:
---
# AI Premium Plugin Architecture

## Contesto

Questo documento descrive l'evoluzione dell'architettura AI Premium da un sistema solo backend a un **plugin completo** che include sia backend che frontend in un unico submodule git.

### Stato Attuale (Completato)
- Interfacce pubbliche in `MdExplorer.AI.Abstractions` (MIT)
- Implementazioni stub in `MdExplorer.AI.Stubs` (MIT)
- Implementazioni premium in submodule privato `MdExplorer.AI.Premium`
- Backend caricato dinamicamente via reflection
- Frontend ancora separato nel main repository

### Obiettivo
Creare un'architettura plugin completa dove il submodule Premium contiene:
- Backend: Controllers, Services, SignalR Hubs
- Frontend: Componenti Angular compilati
- Assets: Tutto in un unico DLL embedded
- Distribution: Self-contained, basta copiare la DLL

## Analisi Compatibilità Angular 11

### Module Federation - NON DISPONIBILE

**Module Federation** è una feature di Webpack 5 per micro-frontends che permette di caricare moduli JavaScript a runtime da URL diversi.

**Stato in Angular 11**:
- ❌ Webpack 5 è **sperimentale** in Angular 11
- ❌ Module Federation richiede Angular 12+ per essere production-ready
- ❌ Angular CLI 11 usa Webpack 4 di default
- ❌ Plugin `@angular-architects/module-federation` supporta Angular 12+

**Conclusione**: Module Federation NON è un'opzione per questo progetto.

### Approccio Alternativo: Embedded Resources ✅

**Questo approccio funziona con qualsiasi versione di Angular** e non richiede feature sperimentali.

**Come funziona**:
1. Compilare Angular separatamente nel submodule Premium
2. Embedare i file compilati come risorse nel DLL .NET
3. Estrarre i file a runtime in wwwroot quando il DLL è presente
4. Lazy loading Angular standard con fallback a stub

**Vantaggi**:
- ✅ Compatibile con Angular 11 (e qualsiasi versione)
- ✅ No modifiche a webpack necessarie
- ✅ Build standard Angular CLI
- ✅ Self-contained: tutto in un DLL
- ✅ Semplice da distribuire
- ✅ Production-ready e stabile

## Architettura Plugin Completo

### Struttura Submodule Premium

```
MdExplorer.AI.Premium/                    # Git submodule privato
├── Backend/
│   ├── Controllers/
│   │   ├── AiModelsController.cs        # Gestione modelli
│   │   ├── AiChatController.cs          # Chat API REST
│   │   ├── LicenseController.cs         # Attivazione licenze
│   │   └── RagController.cs             # RAG (futuro)
│   │
│   ├── Services/
│   │   ├── AiChatServicePremium.cs      # LLamaSharp integration
│   │   ├── ModelDownloadServicePremium.cs
│   │   ├── AiConfigurationService.cs
│   │   └── GpuDetectionService.cs
│   │
│   ├── SignalR/
│   │   └── AiChatHub.cs                 # Real-time streaming
│   │
│   └── Licensing/
│       ├── LicenseValidator.cs          # Validazione online/offline
│       ├── MachineFingerprint.cs        # Anti-piracy
│       └── ActivationService.cs
│
├── Frontend/                             # Angular 11 app
│   ├── src/
│   │   └── app/
│   │       └── ai-premium/
│   │           ├── ai-premium.module.ts
│   │           ├── ai-premium-routing.module.ts
│   │           │
│   │           ├── components/
│   │           │   ├── chat/
│   │           │   │   ├── chat.component.ts
│   │           │   │   ├── chat.component.html
│   │           │   │   └── chat.component.scss
│   │           │   │
│   │           │   ├── model-manager/
│   │           │   │   ├── model-manager.component.ts
│   │           │   │   ├── model-list/
│   │           │   │   ├── model-download/
│   │           │   │   └── gpu-info/
│   │           │   │
│   │           │   ├── license-manager/
│   │           │   │   ├── license-activation.component.ts
│   │           │   │   ├── license-status.component.ts
│   │           │   │   └── license-info.component.ts
│   │           │   │
│   │           │   └── settings/
│   │           │       ├── system-prompt.component.ts
│   │           │       └── gpu-settings.component.ts
│   │           │
│   │           ├── services/
│   │           │   ├── ai-chat-premium.service.ts
│   │           │   ├── model-download-premium.service.ts
│   │           │   └── license.service.ts
│   │           │
│   │           └── models/
│   │               ├── chat-message.ts
│   │               ├── model-info.ts
│   │               └── license-status.ts
│   │
│   ├── angular.json
│   ├── package.json
│   ├── tsconfig.json
│   └── tsconfig.app.json
│
├── DependencyInjection/
│   └── ServiceCollectionExtensions.cs   # Chiamato via reflection
│
├── MdExplorer.AI.Premium.csproj         # MSBuild con Angular build
├── LICENSE                              # Commercial license
└── README.md                            # Setup per clienti
```

### File di Progetto con Build Angular Integrato

Il file `.csproj` deve:
1. Compilare Angular prima del build .NET
2. Embedare gli output come risorse
3. Copiare il DLL nella directory output del main project

Esempio di struttura (riferimento in `MdExplorer.AI.Premium.csproj`):
- Target MSBuild `BeforeBuild` per eseguire `npm run build`
- EmbeddedResource per includere `Frontend/dist/**/*`
- ProjectReference al progetto Abstractions

### Caricamento Runtime nel Main App

**File**: `MdExplorer/Startup.cs`

Il metodo `ConfigureAiServices` deve:
1. Verificare presenza di `MdExplorer.AI.Premium.dll`
2. Se presente:
   - Caricare assembly via reflection
   - Invocare `ServiceCollectionExtensions.AddAiPremiumServices`
   - Chiamare metodo di estrazione assets embedded
   - Registrare controllers con `AddApplicationPart`
3. Se assente:
   - Registrare implementazioni stub
   - Frontend cadrà automaticamente sul modulo stub

### Estrazione Assets Embedded

Nel DLL Premium, implementare metodo statico per estrarre risorse:

**Logica**:
- Controllare se la directory `wwwroot/client2/premium/` esiste già
- Se non esiste o è outdated, estrarre risorse embedded
- Scrivere files nel filesystem
- Registrare percorso per static files middleware

**Chiamata**: Dal metodo `AddAiPremiumServices` dopo registrazione servizi.

## Frontend Integration

### Main App Lazy Loading

**File**: `MdExplorer/client2/src/app/app-routing.module.ts`

Route per AI Premium con fallback automatico:

```typescript
{
  path: 'ai-premium',
  loadChildren: () => import('./premium-loader').then(m => m.loadPremiumModule)
}
```

### Premium Loader con Fallback

**File**: `MdExplorer/client2/src/app/premium-loader.ts`

Logica:
1. Tentare caricamento da `/premium/ai-premium.module.js`
2. Se successo → ritornare AiPremiumModule
3. Se errore (404) → caricare stub module
4. Lo stub mostra UI "Premium Required"

Questo pattern permette:
- Zero configurazione utente
- Graceful degradation automatico
- Free users vedono stub senza errori console

### Stub Module

**File**: `MdExplorer/client2/src/app/ai-stub/ai-stub.module.ts`

Componente semplice che mostra:
- Icona lock
- Messaggio "AI Premium Required"
- Lista features disponibili
- Pricing tiers
- Bottoni: "Learn More" e "I Have a License"

Dialog per attivazione licenza:
- Input license key
- Chiamata API `/api/ai/license/activate`
- Progress indicator
- Reload app dopo attivazione riuscita

## Vantaggi dell'Architettura

### Rispetto a Module Federation

1. **Compatibilità**: Funziona con Angular 11 (no upgrade necessario)
2. **Stabilità**: Non usa feature sperimentali
3. **Semplicità**: Build standard, no custom webpack config
4. **Manutenibilità**: Meno complessità, meno punti di failure

### Rispetto a Build Separati

1. **Self-contained**: Un solo DLL contiene tutto
2. **Distribuzione**: Basta copiare il DLL, no file asset separati
3. **Versioning**: Frontend e backend sempre allineati
4. **Deploy**: Impossibile avere mismatch versioni

### Generale

1. **True Plugin**: Aggiungere/rimuovere = aggiungere/rimuovere DLL
2. **Zero Config**: Main app non sa nulla del Premium internals
3. **Graceful Fallback**: Free users non vedono errori
4. **Hot Swap**: Possibile aggiungere DLL senza rebuild main app

## Flusso Build Completo

### 1. Build Submodule Premium

```bash
cd MdExplorer.AI.Premium

# Step 1: Build Angular
cd Frontend
npm install
npm run build   # Output in Frontend/dist/

# Step 2: Build .NET (include embedding Angular output)
cd ..
dotnet build    # MSBuild BeforeBuild target compila Angular se necessario
                # EmbeddedResource include dist/**/*
                # Output: bin/Release/net8.0/MdExplorer.AI.Premium.dll
```

### 2. Build Main Repository

**Scenario A: Utente con Premium (submodule presente)**
```bash
cd mdExplorer

# Il submodule è già clonato e aggiornato
ls MdExplorer.AI.Premium/
# Services/  Frontend/  ...

# Build solution
dotnet build

# Il .csproj di MdExplorer.Service referenzia Premium submodule
# Il DLL Premium viene copiato in bin/Debug/net8.0/
# Al runtime:
# - ConfigureAiServices trova MdExplorer.AI.Premium.dll
# - Carica servizi premium via reflection
# - Estrae frontend assets in wwwroot/client2/premium/
# - Registra controllers
# ✅ Log: "AI Premium addon detected - loading premium services"
```

**Scenario B: Utente free (submodule assente)**
```bash
cd mdExplorer

# Submodule NON clonato
ls MdExplorer.AI.Premium/
# ls: cannot access 'MdExplorer.AI.Premium/': No such file or directory

# Build solution
dotnet build

# Il .csproj usa Condition="Exists(...)" per includere Premium
# Premium non viene buildato (non c'è)
# Main app compila normalmente
# Al runtime:
# - ConfigureAiServices NON trova MdExplorer.AI.Premium.dll
# - Registra implementazioni stub
# - Frontend carica stub module
# ℹ️ Log: "AI Premium addon not found - using stub implementation"
```

## Registrazione Dinamica Controllers

Nel metodo `ConfigureAiServices`, dopo aver caricato l'assembly Premium:

**Logica**:
1. Ottenere assembly già caricato
2. Chiamare `services.AddControllers().AddApplicationPart(premiumAssembly)`
3. I controller del Premium diventano disponibili automaticamente
4. SignalR hubs registrati tramite `AddSignalR().AddHubOptions`

**Vantaggi**:
- Main app non referenzia mai controller Premium
- Controller appaiono solo se DLL presente
- Zero breaking changes per free users

## Pattern di Accesso Frontend

### Dalla TitleBar o Menu

Quando utente clicca "AI Chat":

**Free User Flow**:
1. Router Angular risolve route `/ai-premium`
2. Lazy loading tenta import da `/premium/ai-premium.module.js`
3. 404 → fallback a stub module
4. UI mostra "Premium Required" card
5. User può cliccare "I Have a License" → dialog attivazione

**Premium User Flow**:
1. Router Angular risolve route `/ai-premium`
2. Lazy loading import da `/premium/ai-premium.module.js` → success
3. AiPremiumModule caricato
4. ChatComponent montato
5. Services chiamano `/api/ai/chat/*` endpoints (controller premium)
6. SignalR connection a `/hubs/aichat`

### API Calls

Tutti i services Angular Premium chiamano API REST:
- `/api/ai/models` → AiModelsController
- `/api/ai/chat` → AiChatController
- `/api/ai/license` → LicenseController

Se controller non registrati (DLL assente):
- Response: 404 Not Found
- Frontend gracefully fallback o mostra errore

## Database e Migrations

### Licensing Table

Migration per tabella `AiLicense` nel database utente:
- File: `MdExplorer.Migrations/M2025_10_15_001_AddAiLicense.cs`
- Colonne: Id, LicenseKey, Email, LicenseType, ActivatedAt, ExpiresAt, MachineFingerprint, IsActive

Mapping NHibernate:
- File: `MdExplorer.Abstractions/Entities/UserDB/AiLicense.cs`
- File: `MDExplorer.dal/Mapping/AiLicenseMap.cs`

**Importante**: Queste rimangono nel main repository (MIT) perché:
- La struttura DB è pubblica
- Il validator che usa questa entità è nel Premium
- Free users vedono la tabella ma non la usano

## Sistema Licensing

### Online Validation

**License Server** (deployato separatamente):
- Endpoint: `POST /api/license/validate`
- Input: LicenseKey, MachineFingerprint, ProductVersion
- Output: LicenseStatus (IsValid, Type, ExpiresAt, EnabledFeatures)
- Database: Licenze vendute, activations count, blacklist

### Offline Validation

**Fallback quando server irraggiungibile**:
- Verifica licenza nel DB locale
- Controlla ExpiresAt
- Grace period: 30 giorni senza validazione online
- Dopo 30 giorni: richiede connessione

### Machine Fingerprint

**Cross-platform identifier** basato su:
- CPU ID (Windows: WMI, Linux: /proc/cpuinfo, macOS: system_profiler)
- Primary MAC address
- SHA256 hash del combined string

**Anti-piracy**:
- Max activations per licenza (es: 2 computer)
- Fingerprint salvato nel DB locale
- Se fingerprint cambia: richiede riattivazione
- Deactivation API per liberare slot

## Distribution Workflow

### Per Sviluppatore (Tu)

**Sviluppo nuovo feature Premium**:
1. `cd MdExplorer.AI.Premium`
2. Crea branch: `git checkout -b feature/rag-documents`
3. Modifica backend e/o frontend
4. Test locali
5. Commit: `git commit -m "feat: add RAG for documents"`
6. Push: `git push origin feature/rag-documents`
7. Create PR nel repo premium
8. Merge PR
9. Update submodule ref nel main repo:
   - `cd ..` (root mdExplorer)
   - `git submodule update --remote MdExplorer.AI.Premium`
   - `git add MdExplorer.AI.Premium`
   - `git commit -m "chore: update Premium submodule with RAG feature"`
   - `git push`

### Per Cliente Premium

**Primo setup**:
1. Acquisto su Gumroad/Paddle
2. Riceve email con:
   - License key
   - SSH deploy key (private)
   - Script setup automatico
3. Esegue script che:
   - Configura SSH key
   - Clona main repo
   - Inizializza submodule premium
   - Build e run
4. Apre app, inserisce license key
5. App riavvia → Premium attivo

**Update**:
1. `git pull` nel main repo
2. `git submodule update --remote MdExplorer.AI.Premium`
3. `dotnet build`
4. Restart app

### Deploy Keys Management

**Automatizzazione**:
- Webhook Gumroad → License Server
- License Server genera SSH keypair
- Public key → GitHub API (add deploy key to repo)
- Private key → Encrypted in DB + Email customer
- Key naming: `customer-{email}`
- Revocation: Remove deploy key via API

## Testing Strategy

### Scenario Testing

**Test Case 1: Free User Build**
- Clone main repo (no submodule)
- `dotnet build` → success
- `dotnet run` → app starts
- Open `/ai-premium` → stub UI shown
- Verify log: "AI Premium addon not found"

**Test Case 2: Premium User Build**
- Clone main repo
- Setup SSH key
- Initialize submodule
- `dotnet build` → success
- `dotnet run` → app starts
- Verify log: "AI Premium addon detected"
- `/api/ai/models` → 200 OK

**Test Case 3: License Activation**
- Start as free user
- Navigate to AI Premium
- Click "I Have a License"
- Enter valid license key
- Verify API call success
- Verify DB entry created
- Restart app
- Verify Premium loaded

**Test Case 4: Offline Validation**
- Activate license online
- Disconnect internet
- Restart app
- Verify offline validation success
- Verify grace period message
- After 30 days mock: require online validation

### Unit Testing

**Backend (Premium project)**:
- Test services: AiChatService, ModelDownloadService
- Test license validator online/offline modes
- Test machine fingerprint generation
- Mock LLamaSharp per test veloci

**Frontend (Angular)**:
- Test components rendering
- Test service API calls
- Test error handling
- Test stub module fallback

## Performance Considerations

### Asset Extraction

**First Run**: Estrarre assets da embedded resources richiede tempo
- Soluzione: Cache timestamp dell'ultima estrazione
- Ri-estrazione solo se DLL Premium ha timestamp più recente

### Assembly Loading

**Reflection overhead**: Minimo, avviene solo al startup
- Single caricamento per lifetime app
- Services registrati nel DI container

### Angular Bundle Size

**Lazy Loading**: Premium module non viene caricato se non necessario
- Free users non scaricano mai il bundle premium
- Anche premium users: caricamento on-demand

## Security Considerations

### Embedded Resources

**Leggibilità**: Assets embedded sono estraibili dal DLL
- **Mitigazione**: Codice è comunque protected da copyright
- Obfuscation opzionale per JS bundle
- Terms of service proibiscono reverse engineering

### API Endpoints

**Controller disponibili solo se DLL presente**:
- Free users: 404 su `/api/ai/*`
- Premium users: Authentication via license validation
- Rate limiting su endpoints costosi

### License Keys

**Storage**: Encrypted nel database
- Machine fingerprint come salt
- No plaintext license keys in logs
- HTTPS only per communication con license server

## Troubleshooting Guide

### Build Failures

**Problem**: "Cannot find MdExplorer.AI.Premium"
- **Solution**: Il submodule non è stato clonato (normale per free users)
- **If premium user**: `git submodule update --init MdExplorer.AI.Premium`

**Problem**: "npm not found" durante build Premium
- **Solution**: Installare Node.js + npm
- **Check**: Angular build richiede Node.js 14.21.3

### Runtime Issues

**Problem**: "AI Premium addon detected" ma features non funzionano
- **Check**: License activated? Verify in DB o `/api/ai/license/status`
- **Check**: Assets estratti? Verify `wwwroot/client2/premium/` exists

**Problem**: Frontend stub anche con DLL presente
- **Check**: Assets extraction riuscita?
- **Check**: Browser cache? Hard refresh
- **Check**: Permissions su wwwroot folder?

### License Activation

**Problem**: "Invalid license key"
- **Check**: Typos nel key
- **Check**: License server reachable?
- **Check**: Key già usato su troppi computer? (max activations)

**Problem**: "Online validation required"
- **Cause**: Offline grace period scaduto
- **Solution**: Connessione internet + riavvio app

## Roadmap

### Fase 1: Refactoring Base ✅ (Completata)
- Abstractions project
- Stub implementations
- Conditional DI
- Premium submodule backend

### Fase 2: Frontend Integration (Questa Evolutiva)
- Angular app nel submodule
- MSBuild integration
- Embedded resources
- Asset extraction runtime
- Lazy loading con fallback

### Fase 3: Distribution Automation
- License server deployment
- Deploy keys automation
- Customer email automation
- Setup scripts

### Fase 4: Advanced Features
- RAG implementation (Sprint 2.1)
- Tool system (Sprint 2.2)
- Agent orchestration (Sprint 2.3)

### Fase 5: Polish & Launch
- Documentation complete
- Video tutorials
- Marketing page
- Beta testing
- Public launch

## Metriche di Successo

**Technical**:
- ✅ Build success rate 100% per entrambi scenari (free/premium)
- ✅ Zero breaking changes per utenti esistenti
- ✅ Asset extraction time < 2 secondi
- ✅ Lazy load time < 500ms

**User Experience**:
- ⭐ Setup time < 10 minuti per premium users
- ⭐ Activation success rate > 95%
- ⭐ Support tickets < 5% customers
- ⭐ Documentazione completa e chiara

**Business** (post-launch):
- 💰 Conversion rate free → premium: 2-5%
- 💰 Retention dopo 1 anno > 80%
- 💰 Customer satisfaction score > 4.5/5

## Riferimenti

### Codice Esistente
- `MdExplorer.AI.Abstractions/` - Interfacce pubbliche
- `MdExplorer.AI.Stubs/` - Implementazioni stub
- `MdExplorer.AI.Premium/` - Submodule premium (privato)
- `MdExplorer/Startup.cs:ConfigureAiServices()` - DI condizionale

### Documentazione
- `docs-internal/Sprints/2025-10-15-AI-Agents-Premium-Strategy.md` - Strategia business
- `CLAUDE.md` - Guidelines sviluppo
- `README.md` - Setup utenti

### Design Patterns Utilizzati
- **Plugin Architecture**: Dynamic loading di components
- **Strangler Fig**: Migrazione graduale da free a premium
- **Embedded Resources**: Self-contained distribution
- **Lazy Loading**: On-demand frontend loading
- **Dependency Injection**: Loose coupling con interfaces
- **Repository Pattern**: Data access abstraction
- **Strategy Pattern**: Offline/online validation

---

**Documento creato**: 15 Ottobre 2025
**Autore**: Carlo Salaroglio
**Status**: Pianificazione - Fase 2
**Versione Angular**: 11.2.6
**Approccio**: Embedded Resources + Runtime Extraction
