# 🚀 Sprint: Word Template Pages - Sistema di Pagine Predefinite per Export Word

## 📋 Obiettivo
Implementare un sistema che permetta di inserire pagine predefinite (copertine, disclaimer, appendici) nei documenti Word esportati, con supporto per tag dinamici sostituibili.

## 🎯 Requisiti Funzionali
- Inserire pagine predefinite all'inizio o alla fine del documento
- Supportare template di pagine configurabili via file Markdown
- Sostituire tag dinamici (es: `{{company}}`, `{{author}}`) con valori reali
- Configurare le pagine tramite metadati YAML
- Mantenere retrocompatibilità con export esistenti

## 🏗️ Architettura Proposta

### Layer Structure
```
┌─────────────────────────────────────┐
│   Angular Client (UI Configuration)  │
├─────────────────────────────────────┤
│   API Controller (MdExportController)│
├─────────────────────────────────────┤
│   Business Layer (WordTemplateService)│
├─────────────────────────────────────┤
│   Data Layer (Template Files)        │
└─────────────────────────────────────┘
```

## 📝 Implementazione Step-by-Step

### Step 1: Creazione Struttura Template Files
**Obiettivo**: Creare la struttura di directory per i template di pagine

#### 1.1 Creare le directory
```bash
mkdir -p MdExplorer/templates/word/pages
mkdir -p MdExplorer/templates/word/pages/covers
mkdir -p MdExplorer/templates/word/pages/appendices
mkdir -p MdExplorer/templates/word/pages/disclaimers
```

#### 1.2 Creare template di esempio

**File**: `MdExplorer/templates/word/pages/covers/standard.md`
```markdown
\newpage

<div style="text-align: center; margin-top: 100px;">

![logo]({{logo_path}})

# {{title}}

## {{subtitle}}

<div style="margin-top: 200px;">

**Autore:** {{author}}  
**Data:** {{date}}  
**Versione:** {{version}}  

</div>

<div style="position: absolute; bottom: 50px; left: 0; right: 0; text-align: center;">

**{{company}}**  
{{department}}

</div>

</div>

\newpage
```

**File**: `MdExplorer/templates/word/pages/covers/project.md`
```markdown
\newpage

# {{project_name}}

## {{document_type}}

**Codice Progetto:** {{project_code}}  
**Cliente:** {{client_name}}  
**Data Inizio:** {{start_date}}  
**Data Fine:** {{end_date}}  

---

**Preparato da:**  
{{author}}  
{{author_role}}  

**Approvato da:**  
{{approver_name}}  
{{approver_role}}  

**Data Documento:** {{date}}  
**Versione:** {{version}}  
**Stato:** {{status}}  

\newpage
```

**File**: `MdExplorer/templates/word/pages/disclaimers/confidential.md`
```markdown
\newpage

## Informazioni sulla Riservatezza

Questo documento contiene informazioni proprietarie e riservate di **{{company}}**. 

La distribuzione è limitata ai destinatari autorizzati. È vietata la riproduzione o distribuzione non autorizzata di questo documento o di qualsiasi sua parte.

**Classificazione:** {{classification}}  
**Data di Emissione:** {{date}}  
**Valido fino a:** {{expiry_date}}  

Per informazioni contattare: {{contact_email}}

\newpage
```

**File**: `MdExplorer/templates/word/pages/appendices/signatures.md`
```markdown
\newpage

## Foglio Firme

### Approvazioni del Documento

| Ruolo | Nome | Firma | Data |
|-------|------|-------|------|
| {{role_1}} | {{name_1}} | _________________ | ____/____/____ |
| {{role_2}} | {{name_2}} | _________________ | ____/____/____ |
| {{role_3}} | {{name_3}} | _________________ | ____/____/____ |
| {{role_4}} | {{name_4}} | _________________ | ____/____/____ |

### Registro delle Modifiche

| Versione | Data | Autore | Descrizione |
|----------|------|---------|-------------|
| {{version}} | {{date}} | {{author}} | {{change_description}} |

\newpage
```

### Step 2: Estensione del Modello YAML
**Obiettivo**: Aggiungere supporto per pagine predefinite nel modello YAML

#### 2.1 Aggiornare il modello `MdExplorerDocumentDescriptor`

**File**: `MdExplorer.Features/Yaml/Models/MdExplorerDocumentDescriptor.cs`
```csharp
namespace MdExplorer.Features.Yaml.Models
{
    public class MdExplorerDocumentDescriptor
    {
        // ... proprietà esistenti ...
        
        public WordSectionDescriptor WordSection { get; set; }
    }
    
    public class WordSectionDescriptor
    {
        // ... proprietà esistenti ...
        
        public PredefinedPagesDescriptor PredefinedPages { get; set; }
    }
    
    public class PredefinedPagesDescriptor
    {
        public PageDescriptor Cover { get; set; }
        public List<PageDescriptor> Headers { get; set; }
        public List<PageDescriptor> Footers { get; set; }
        public PageDescriptor Disclaimer { get; set; }
        public PageDescriptor Appendix { get; set; }
    }
    
    public class PageDescriptor
    {
        public string Type { get; set; }
        public string Template { get; set; }
        public Dictionary<string, string> Tags { get; set; }
        public bool Enabled { get; set; } = true;
    }
}
```

#### 2.2 Esempio di YAML con pagine predefinite
```yaml
---
title: "Report Tecnico Sistema XYZ"
author: "Mario Rossi"
date: "2024-01-15"
email: "mario.rossi@azienda.it"
word_section:
  write_toc: true
  document_header: Project
  template_section:
    template_type: inherits
    inherit_from_template: project
  predefined_pages:
    cover:
      type: covers
      template: project
      tags:
        project_name: "Sistema XYZ"
        document_type: "Report Tecnico"
        project_code: "XYZ-2024-001"
        client_name: "Cliente ABC"
        start_date: "01/01/2024"
        end_date: "31/12/2024"
        author_role: "Technical Lead"
        approver_name: "Giuseppe Verdi"
        approver_role: "Project Manager"
        version: "1.0"
        status: "Draft"
    disclaimer:
      type: disclaimers
      template: confidential
      tags:
        company: "Azienda SpA"
        classification: "Confidential"
        expiry_date: "31/12/2025"
        contact_email: "info@azienda.it"
    appendix:
      type: appendices
      template: signatures
      tags:
        role_1: "Project Manager"
        name_1: "Giuseppe Verdi"
        role_2: "Technical Lead"
        name_2: "Mario Rossi"
        role_3: "Quality Assurance"
        name_3: "Anna Bianchi"
        role_4: "Cliente"
        name_4: "Paolo Neri"
        change_description: "Versione iniziale del documento"
---
```

### Step 3: Implementazione Business Layer
**Obiettivo**: Creare il servizio per gestire template e sostituzioni

#### 3.1 Creare l'interfaccia del servizio

**File**: `MdExplorer.Features/Exports/IWordTemplateService.cs`
```csharp
using MdExplorer.Features.Yaml.Models;
using System.Threading.Tasks;

namespace MdExplorer.Features.Exports
{
    public interface IWordTemplateService
    {
        /// <summary>
        /// Inserisce le pagine predefinite nel contenuto markdown
        /// </summary>
        /// <param name="markdownContent">Contenuto markdown originale</param>
        /// <param name="descriptor">Descrittore del documento con configurazione pagine</param>
        /// <param name="projectPath">Path del progetto per risolvere percorsi relativi</param>
        /// <returns>Markdown con pagine predefinite inserite</returns>
        Task<string> InsertPredefinedPagesAsync(
            string markdownContent, 
            MdExplorerDocumentDescriptor descriptor,
            string projectPath);
        
        /// <summary>
        /// Sostituisce i tag in un template
        /// </summary>
        /// <param name="template">Template con tag</param>
        /// <param name="tags">Dizionario dei tag da sostituire</param>
        /// <returns>Template con tag sostituiti</returns>
        string ReplaceTags(string template, Dictionary<string, string> tags);
        
        /// <summary>
        /// Valida che tutti i tag richiesti siano presenti
        /// </summary>
        /// <param name="template">Template da validare</param>
        /// <param name="tags">Tag disponibili</param>
        /// <returns>Lista di tag mancanti</returns>
        List<string> ValidateRequiredTags(string template, Dictionary<string, string> tags);
    }
}
```

#### 3.2 Implementare il servizio

**File**: `MdExplorer.Features/Exports/WordTemplateService.cs`
```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MdExplorer.Features.Yaml.Models;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Exports
{
    public class WordTemplateService : IWordTemplateService
    {
        private readonly ILogger<WordTemplateService> _logger;
        private readonly string _templatesBasePath;
        private readonly Regex _tagRegex = new Regex(@"\{\{([^}]+)\}\}", RegexOptions.Compiled);

        public WordTemplateService(ILogger<WordTemplateService> logger)
        {
            _logger = logger;
            _templatesBasePath = Path.Combine(".", ".md", "templates", "word", "pages");
        }

        public async Task<string> InsertPredefinedPagesAsync(
            string markdownContent, 
            MdExplorerDocumentDescriptor descriptor,
            string projectPath)
        {
            if (descriptor?.WordSection?.PredefinedPages == null)
            {
                return markdownContent;
            }

            var result = new StringBuilder();
            var predefinedPages = descriptor.WordSection.PredefinedPages;

            // 1. Inserisci Cover Page
            if (predefinedPages.Cover?.Enabled == true)
            {
                var coverContent = await ProcessPageAsync(predefinedPages.Cover, descriptor, projectPath);
                if (!string.IsNullOrEmpty(coverContent))
                {
                    result.AppendLine(coverContent);
                }
            }

            // 2. Inserisci Headers
            if (predefinedPages.Headers?.Any() == true)
            {
                foreach (var header in predefinedPages.Headers.Where(h => h.Enabled))
                {
                    var headerContent = await ProcessPageAsync(header, descriptor, projectPath);
                    if (!string.IsNullOrEmpty(headerContent))
                    {
                        result.AppendLine(headerContent);
                    }
                }
            }

            // 3. Contenuto principale
            result.AppendLine(markdownContent);

            // 4. Inserisci Disclaimer
            if (predefinedPages.Disclaimer?.Enabled == true)
            {
                var disclaimerContent = await ProcessPageAsync(predefinedPages.Disclaimer, descriptor, projectPath);
                if (!string.IsNullOrEmpty(disclaimerContent))
                {
                    result.AppendLine(disclaimerContent);
                }
            }

            // 5. Inserisci Appendix
            if (predefinedPages.Appendix?.Enabled == true)
            {
                var appendixContent = await ProcessPageAsync(predefinedPages.Appendix, descriptor, projectPath);
                if (!string.IsNullOrEmpty(appendixContent))
                {
                    result.AppendLine(appendixContent);
                }
            }

            // 6. Inserisci Footers
            if (predefinedPages.Footers?.Any() == true)
            {
                foreach (var footer in predefinedPages.Footers.Where(f => f.Enabled))
                {
                    var footerContent = await ProcessPageAsync(footer, descriptor, projectPath);
                    if (!string.IsNullOrEmpty(footerContent))
                    {
                        result.AppendLine(footerContent);
                    }
                }
            }

            return result.ToString();
        }

        private async Task<string> ProcessPageAsync(
            PageDescriptor page, 
            MdExplorerDocumentDescriptor descriptor,
            string projectPath)
        {
            try
            {
                // Costruisci il percorso del template
                var templatePath = Path.Combine(_templatesBasePath, page.Type, $"{page.Template}.md");
                
                if (!File.Exists(templatePath))
                {
                    _logger.LogWarning($"Template non trovato: {templatePath}");
                    return string.Empty;
                }

                // Leggi il template
                var templateContent = await File.ReadAllTextAsync(templatePath);

                // Prepara i tag combinando quelli della pagina con quelli globali del documento
                var allTags = BuildTagsDictionary(page, descriptor, projectPath);

                // Valida i tag richiesti
                var missingTags = ValidateRequiredTags(templateContent, allTags);
                if (missingTags.Any())
                {
                    _logger.LogWarning($"Tag mancanti nel template {page.Template}: {string.Join(", ", missingTags)}");
                }

                // Sostituisci i tag
                var processedContent = ReplaceTags(templateContent, allTags);

                return processedContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nel processare la pagina {page.Template}");
                return string.Empty;
            }
        }

        private Dictionary<string, string> BuildTagsDictionary(
            PageDescriptor page,
            MdExplorerDocumentDescriptor descriptor,
            string projectPath)
        {
            var tags = new Dictionary<string, string>();

            // 1. Tag globali dal documento
            if (!string.IsNullOrEmpty(descriptor.Title))
                tags["title"] = descriptor.Title;
            if (!string.IsNullOrEmpty(descriptor.Author))
                tags["author"] = descriptor.Author;
            if (!string.IsNullOrEmpty(descriptor.Email))
                tags["email"] = descriptor.Email;
            if (!string.IsNullOrEmpty(descriptor.Date))
                tags["date"] = descriptor.Date;

            // 2. Tag di sistema
            tags["current_date"] = DateTime.Now.ToString("dd/MM/yyyy");
            tags["current_time"] = DateTime.Now.ToString("HH:mm:ss");
            tags["project_path"] = projectPath;

            // 3. Tag specifici della pagina (sovrascrivono quelli globali)
            if (page.Tags != null)
            {
                foreach (var tag in page.Tags)
                {
                    tags[tag.Key] = tag.Value;
                }
            }

            // 4. Risolvi percorsi relativi per immagini
            if (tags.ContainsKey("logo_path") && !Path.IsPathRooted(tags["logo_path"]))
            {
                tags["logo_path"] = Path.Combine(projectPath, tags["logo_path"]);
            }

            return tags;
        }

        public string ReplaceTags(string template, Dictionary<string, string> tags)
        {
            if (string.IsNullOrEmpty(template))
                return template;

            return _tagRegex.Replace(template, match =>
            {
                var tagName = match.Groups[1].Value.Trim();
                
                // Supporta tag con valore di default: {{tag|default}}
                var parts = tagName.Split('|');
                var actualTagName = parts[0].Trim();
                var defaultValue = parts.Length > 1 ? parts[1].Trim() : string.Empty;

                if (tags.ContainsKey(actualTagName))
                {
                    return tags[actualTagName];
                }
                else if (!string.IsNullOrEmpty(defaultValue))
                {
                    return defaultValue;
                }
                else
                {
                    _logger.LogDebug($"Tag non trovato: {actualTagName}");
                    return match.Value; // Mantieni il tag originale se non trovato
                }
            });
        }

        public List<string> ValidateRequiredTags(string template, Dictionary<string, string> tags)
        {
            var missingTags = new List<string>();
            var matches = _tagRegex.Matches(template);

            foreach (Match match in matches)
            {
                var tagName = match.Groups[1].Value.Trim();
                
                // Ignora tag con default value
                if (tagName.Contains('|'))
                {
                    tagName = tagName.Split('|')[0].Trim();
                }

                if (!tags.ContainsKey(tagName))
                {
                    missingTags.Add(tagName);
                }
            }

            return missingTags.Distinct().ToList();
        }
    }
}
```

### Step 4: Registrazione Dependency Injection
**Obiettivo**: Registrare il nuovo servizio nel container DI

#### 4.1 Aggiornare FeaturesDI.cs

**File**: `MdExplorer.Features/FeaturesDI.cs`
```csharp
// Aggiungi questa riga nel metodo AddFeatures
services.AddScoped<IWordTemplateService, WordTemplateService>();
```

### Step 5: Integrazione nel Controller
**Obiettivo**: Utilizzare il servizio nel controller di export

#### 5.1 Modificare MdExportController

**File**: `MdExplorer/Controllers/MdExportController.cs`

Aggiungi il campo:
```csharp
private readonly IWordTemplateService _wordTemplateService;
```

Aggiorna il costruttore:
```csharp
public MdExportController(
    // ... parametri esistenti ...
    IWordTemplateService wordTemplateService
) : base(/* ... */)
{
    // ... inizializzazioni esistenti ...
    _wordTemplateService = wordTemplateService;
}
```

Modifica il metodo `GetAsync` dopo la riga 129 (dopo auto-generazione YAML):
```csharp
// Inserisci pagine predefinite se configurate
if (docDesc?.WordSection?.PredefinedPages != null)
{
    readText = await _wordTemplateService.InsertPredefinedPagesAsync(
        readText, 
        docDesc, 
        _fileSystemWatcher.Path
    );
    
    // Log per debug
    _logger.LogInformation("Pagine predefinite inserite per il documento {0}", filePath);
}
```

### Step 6: Test Unitari
**Obiettivo**: Creare test per verificare il funzionamento

#### 6.1 Creare test per WordTemplateService

**File**: `MdExplorer.Features.Tests/Exports/WordTemplateServiceTests.cs`
```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using MdExplorer.Features.Exports;
using MdExplorer.Features.Yaml.Models;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MdExplorer.Features.Tests.Exports
{
    [TestClass]
    public class WordTemplateServiceTests
    {
        private WordTemplateService _service;
        private Mock<ILogger<WordTemplateService>> _loggerMock;

        [TestInitialize]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<WordTemplateService>>();
            _service = new WordTemplateService(_loggerMock.Object);
        }

        [TestMethod]
        public void ReplaceTags_Should_Replace_Simple_Tags()
        {
            // Arrange
            var template = "Hello {{name}}, welcome to {{company}}!";
            var tags = new Dictionary<string, string>
            {
                { "name", "Mario" },
                { "company", "Azienda SpA" }
            };

            // Act
            var result = _service.ReplaceTags(template, tags);

            // Assert
            Assert.AreEqual("Hello Mario, welcome to Azienda SpA!", result);
        }

        [TestMethod]
        public void ReplaceTags_Should_Handle_Default_Values()
        {
            // Arrange
            var template = "Version: {{version|1.0}}, Status: {{status|Draft}}";
            var tags = new Dictionary<string, string>
            {
                { "version", "2.0" }
            };

            // Act
            var result = _service.ReplaceTags(template, tags);

            // Assert
            Assert.AreEqual("Version: 2.0, Status: Draft", result);
        }

        [TestMethod]
        public void ValidateRequiredTags_Should_Find_Missing_Tags()
        {
            // Arrange
            var template = "Author: {{author}}, Date: {{date}}, Version: {{version}}";
            var tags = new Dictionary<string, string>
            {
                { "author", "Mario" }
            };

            // Act
            var missingTags = _service.ValidateRequiredTags(template, tags);

            // Assert
            Assert.AreEqual(2, missingTags.Count);
            Assert.IsTrue(missingTags.Contains("date"));
            Assert.IsTrue(missingTags.Contains("version"));
        }

        [TestMethod]
        public async Task InsertPredefinedPagesAsync_Should_Return_Original_If_No_Pages_Configured()
        {
            // Arrange
            var originalContent = "# My Document\n\nContent here";
            var descriptor = new MdExplorerDocumentDescriptor
            {
                WordSection = new WordSectionDescriptor()
            };

            // Act
            var result = await _service.InsertPredefinedPagesAsync(originalContent, descriptor, "/project");

            // Assert
            Assert.AreEqual(originalContent, result);
        }
    }
}
```

### Step 7: Aggiornamento UI Angular (Opzionale)
**Obiettivo**: Aggiungere configurazione UI per le pagine predefinite

#### 7.1 Aggiungere componente di configurazione

**File**: `MdExplorer/client2/src/app/md-explorer/components/predefined-pages-config/predefined-pages-config.component.ts`
```typescript
import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';

export interface PredefinedPageConfig {
  cover?: PageConfig;
  disclaimer?: PageConfig;
  appendix?: PageConfig;
}

export interface PageConfig {
  type: string;
  template: string;
  enabled: boolean;
  tags: { [key: string]: string };
}

@Component({
  selector: 'app-predefined-pages-config',
  templateUrl: './predefined-pages-config.component.html',
  styleUrls: ['./predefined-pages-config.component.scss']
})
export class PredefinedPagesConfigComponent implements OnInit {
  @Input() config: PredefinedPageConfig;
  @Output() configChange = new EventEmitter<PredefinedPageConfig>();

  availableTemplates = {
    covers: ['standard', 'project', 'report'],
    disclaimers: ['confidential', 'public', 'internal'],
    appendices: ['signatures', 'changelog', 'references']
  };

  ngOnInit(): void {
    if (!this.config) {
      this.config = {};
    }
  }

  onTemplateChange(pageType: string, template: string): void {
    if (!this.config[pageType]) {
      this.config[pageType] = {
        type: pageType,
        template: template,
        enabled: true,
        tags: {}
      };
    }
    this.config[pageType].template = template;
    this.configChange.emit(this.config);
  }

  onTagChange(pageType: string, tagName: string, value: string): void {
    if (!this.config[pageType].tags) {
      this.config[pageType].tags = {};
    }
    this.config[pageType].tags[tagName] = value;
    this.configChange.emit(this.config);
  }
}
```

## 📊 Test di Accettazione

### Test 1: Export con Cover Page
1. Crea un documento Markdown con YAML che include una cover page
2. Esporta in Word
3. Verifica che la cover page sia presente con i tag sostituiti

### Test 2: Export con Multiple Pagine
1. Configura cover, disclaimer e appendix
2. Esporta in Word
3. Verifica l'ordine corretto delle pagine

### Test 3: Tag con Valori Default
1. Usa template con tag che hanno valori di default
2. Non fornire alcuni tag nel YAML
3. Verifica che i valori di default siano utilizzati

### Test 4: Retrocompatibilità
1. Esporta un documento senza configurazione pagine predefinite
2. Verifica che l'export funzioni come prima

## 🚀 Deploy e Configurazione

### 1. Copiare i Template
```bash
cp -r templates/word/pages /path/to/deployment/.md/templates/word/
```

### 2. Verificare Permessi
```bash
chmod -R 755 /path/to/deployment/.md/templates/word/pages
```

### 3. Configurare appsettings.json (se necessario)
```json
{
  "MdExplorer": {
    "Templates": {
      "WordPagesPath": "./.md/templates/word/pages"
    }
  }
}
```

## 📈 Metriche di Successo
- ✅ Tutti i test unitari passano
- ✅ Export Word funziona con e senza pagine predefinite
- ✅ Tag vengono sostituiti correttamente
- ✅ Nessuna regressione sugli export esistenti
- ✅ Performance: < 100ms overhead per inserimento pagine

## 🔄 Estensioni Future
1. **Editor Visuale**: UI per creare/modificare template
2. **Condizioni**: Supporto per logica condizionale nei template
3. **Loop**: Supporto per iterazioni (es: lista autori multipli)
4. **Stili Custom**: Applicare stili Word specifici alle pagine
5. **Preview**: Anteprima delle pagine prima dell'export

## 📚 Documentazione per Utenti

### Esempio YAML Minimo
```yaml
---
title: "Il Mio Documento"
author: "Nome Autore"
word_section:
  predefined_pages:
    cover:
      type: covers
      template: standard
      tags:
        company: "La Mia Azienda"
        version: "1.0"
---
```

### Esempio YAML Completo
```yaml
---
title: "Report Completo"
author: "Team Development"
word_section:
  predefined_pages:
    cover:
      type: covers
      template: project
      enabled: true
      tags:
        project_name: "Progetto XYZ"
        client_name: "Cliente ABC"
        version: "2.0"
        status: "Final"
    disclaimer:
      type: disclaimers  
      template: confidential
      enabled: true
      tags:
        company: "Azienda SpA"
        classification: "Strictly Confidential"
    appendix:
      type: appendices
      template: signatures
      enabled: true
      tags:
        role_1: "Project Manager"
        name_1: "Mario Rossi"
---
```

## ⚡ Comandi Utili per Development

```bash
# Build del progetto
dotnet build MdExplorer.Features/MdExplorer.Features.csproj

# Run dei test
dotnet test MdExplorer.Features.Tests/MdExplorer.Features.Tests.csproj --filter "FullyQualifiedName~WordTemplateService"

# Debug del servizio
dotnet run --project MdExplorer/MdExplorer.Service.csproj --launch-profile "MdExplorer.Service"
```

---

**Nota**: Questo sprint è progettato per essere implementato incrementalmente. Ogni step è testabile indipendentemente e può essere completato usando Claude Sonnet 4.