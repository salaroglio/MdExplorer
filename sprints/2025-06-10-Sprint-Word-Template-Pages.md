# Sprint Word Template Pages

# Sistema di Pagine Predefinite per Export Word

## 📋 Obiettivo

Implementare un sistema che permetta di inserire pagine predefinite (copertine, disclaimer, appendici) nei documenti Word esportati, con supporto per tag dinamici sostituibili.

## 🎯 Requisiti Funzionali

* Inserire pagine predefinite all'inizio o alla fine del documento
* Supportare template di pagine configurabili via file Markdown
* Sostituire tag dinamici (es: `{{company}}`, `{{author}}`) con valori reali
* Configurare le pagine tramite metadati YAML
* **Estendere il sistema template esistente** (`.md/templates/word/pages/`)
* **Sfruttare il ProjectsManager esistente** per copiare template
* **Permettere personalizzazione dei template per progetto**
* Mantenere retrocompatibilità con export esistenti

## 🏗️ Architettura Esistente e Proposta

### Sistema Template Attuale

MdExplorer ha già un sistema template completo:

* **ProjectsManager.cs** gestisce la copia automatica dei template
* **Directory** **`.md/templates/word/`** contiene `reference.docx`, `project.docx`, `minute.docx`
* **MdExportController** usa i template per Pandoc con `--reference-doc`

### Estensione Proposta

```
Progetto/
├── .md/
│   ├── templates/
│   │   ├── word/
│   │   │   ├── reference.docx     [ESISTENTE]
│   │   │   ├── project.docx       [ESISTENTE]
│   │   │   ├── minute.docx        [ESISTENTE]
│   │   │   └── pages/             [NUOVO]
│   │   │       ├── covers/
│   │   │       ├── disclaimers/
│   │   │       └── appendices/
│   │   └── pdf/eisvogel.tex       [ESISTENTE]
│   └── [altri file esistenti]
```

## 📝 Implementazione Step-by-Step

### Step 1: Estendere ProjectsManager per Template Pages

**Obiettivo**: Modificare il sistema esistente per copiare anche i template di pagine Markdown

#### 1.1 Aggiungere template di default come embedded resources

**File**: `MdExplorer/templates/word/pages/covers/standard.md`

```Markdown
\newpage

<div style="text-align: center; margin-top: 100px;">

![logo]({{logo_path|./assets/logo.png}})

# {{title}}

## {{subtitle|}}

<div style="margin-top: 200px;">

**Autore:** {{author}}  
**Data:** {{date}}  
**Versione:** {{version|1.0}}  

</div>

<div style="position: absolute; bottom: 50px; left: 0; right: 0; text-align: center;">

**{{company|Nome Azienda}}**  
{{department|Dipartimento}}

</div>

</div>

\newpage
```

**File**: `MdExplorer/templates/word/pages/covers/project.md`

```Markdown
\newpage

# {{project_name}}

## {{document_type|Documento di Progetto}}

**Codice Progetto:** {{project_code}}  
**Cliente:** {{client_name}}  
**Data Inizio:** {{start_date}}  
**Data Fine:** {{end_date}}  

---

**Preparato da:**  
{{author}}  
{{author_role|Sviluppatore}}  

**Approvato da:**  
{{approver_name}}  
{{approver_role|Project Manager}}  

**Data Documento:** {{date}}  
**Versione:** {{version|1.0}}  
**Stato:** {{status|Draft}}  

\newpage
```

**File**: `MdExplorer/templates/word/pages/disclaimers/confidential.md`

```Markdown
\newpage

## Informazioni sulla Riservatezza

Questo documento contiene informazioni proprietarie e riservate di **{{company}}**. 

La distribuzione è limitata ai destinatari autorizzati. È vietata la riproduzione o distribuzione non autorizzata di questo documento o di qualsiasi sua parte.

**Classificazione:** {{classification|Confidential}}  
**Data di Emissione:** {{date}}  
**Valido fino a:** {{expiry_date}}  

Per informazioni contattare: {{contact_email}}

\newpage
```

**File**: `MdExplorer/templates/word/pages/appendices/signatures.md`

```Markdown
\newpage

## Foglio Firme

### Approvazioni del Documento

| Ruolo | Nome | Firma | Data |
|-------|------|-------|------|
| {{role_1|Project Manager}} | {{name_1}} | _________________ | ____/____/____ |
| {{role_2|Technical Lead}} | {{name_2}} | _________________ | ____/____/____ |
| {{role_3|Quality Assurance}} | {{name_3}} | _________________ | ____/____/____ |
| {{role_4|Cliente}} | {{name_4}} | _________________ | ____/____/____ |

### Registro delle Modifiche

| Versione | Data | Autore | Descrizione |
|----------|------|---------|-------------|
| {{version|1.0}} | {{date}} | {{author}} | {{change_description|Versione iniziale}} |

\newpage
```

#### 1.2 Modificare ProjectsManager.cs

**File**: `MdExplorer/ProjectsManager.cs`

Estendere il metodo `ConfigTemplates` (circa riga 166) per includere la copia dei template pages:

```C#
private static void ConfigTemplates(string mdPath, IServiceCollection services = null)
{
    // ... codice esistente per creare directory ...
    
    // NUOVO: Crea directory per template pages
    Directory.CreateDirectory($"{directory}{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}word{Path.DirectorySeparatorChar}pages");
    Directory.CreateDirectory($"{directory}{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}word{Path.DirectorySeparatorChar}pages{Path.DirectorySeparatorChar}covers");
    Directory.CreateDirectory($"{directory}{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}word{Path.DirectorySeparatorChar}pages{Path.DirectorySeparatorChar}disclaimers");
    Directory.CreateDirectory($"{directory}{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}word{Path.DirectorySeparatorChar}pages{Path.DirectorySeparatorChar}appendices");

    // ... codice esistente per eisvogel.tex e template .docx ...

    // NUOVO: Copia template pages da embedded resources
    CopyPageTemplates(directory);
}

private static void CopyPageTemplates(string mdDirectory)
{
    var pageTemplates = new[]
    {
        ("covers.standard.md", $@"templates{Path.DirectorySeparatorChar}word{Path.DirectorySeparatorChar}pages{Path.DirectorySeparatorChar}covers{Path.DirectorySeparatorChar}standard.md"),
        ("covers.project.md", $@"templates{Path.DirectorySeparatorChar}word{Path.DirectorySeparatorChar}pages{Path.DirectorySeparatorChar}covers{Path.DirectorySeparatorChar}project.md"),
        ("disclaimers.confidential.md", $@"templates{Path.DirectorySeparatorChar}word{Path.DirectorySeparatorChar}pages{Path.DirectorySeparatorChar}disclaimers{Path.DirectorySeparatorChar}confidential.md"),
        ("appendices.signatures.md", $@"templates{Path.DirectorySeparatorChar}word{Path.DirectorySeparatorChar}pages{Path.DirectorySeparatorChar}appendices{Path.DirectorySeparatorChar}signatures.md")
    };

    foreach (var (resourceName, destinationPath) in pageTemplates)
    {
        var fullPath = $"{mdDirectory}{Path.DirectorySeparatorChar}{destinationPath}";
        if (!File.Exists(fullPath))
        {
            FileUtil.ExtractResFile($"MdExplorer.Service.templates.word.pages.{resourceName}", fullPath);
        }
    }
}
```

### Step 2: Estensione del Modello YAML

**Obiettivo**: Aggiungere supporto per pagine predefinite nel modello YAML esistente

#### 2.1 Verificare/Aggiornare il modello esistente

**File**: `MdExplorer.Features/Yaml/Models/MdExplorerDocumentDescriptor.cs`

Estendere le classi esistenti:

```C#
public class WordSectionDescriptor
{
    // ... proprietà esistenti (WriteToc, DocumentHeader, TemplateSection) ...
    
    [YamlMember(Alias = "predefined_pages")]
    public PredefinedPagesDescriptor PredefinedPages { get; set; }
}

public class PredefinedPagesDescriptor
{
    [YamlMember(Alias = "cover")]
    public PageDescriptor Cover { get; set; }
    
    [YamlMember(Alias = "disclaimer")]
    public PageDescriptor Disclaimer { get; set; }
    
    [YamlMember(Alias = "appendix")]
    public PageDescriptor Appendix { get; set; }
}

public class PageDescriptor
{
    [YamlMember(Alias = "type")]
    public string Type { get; set; }
    
    [YamlMember(Alias = "template")]
    public string Template { get; set; }
    
    [YamlMember(Alias = "enabled")]
    public bool Enabled { get; set; } = true;
    
    [YamlMember(Alias = "tags")]
    public Dictionary<string, string> Tags { get; set; }
}
```

#### 2.2 Esempio di YAML

```YAML
---
title: "Report Tecnico Sistema XYZ"
author: "Mario Rossi"
date: "2024-01-15"
word_section:
  write_toc: true
  template_section:
    template_type: inherits
    inherit_from_template: project
  predefined_pages:
    cover:
      type: covers
      template: project
      enabled: true
      tags:
        project_name: "Sistema XYZ"
        document_type: "Report Tecnico"
        project_code: "XYZ-2024-001"
        client_name: "Cliente ABC"
        version: "1.0"
    disclaimer:
      type: disclaimers
      template: confidential
      enabled: true
      tags:
        company: "Azienda SpA"
        classification: "Confidential"
    appendix:
      type: appendices
      template: signatures
      enabled: true
      tags:
        role_1: "Project Manager"
        name_1: "Giuseppe Verdi"
---
```

### Step 3: Implementazione Business Layer

**Obiettivo**: Creare il servizio per gestire template e sostituzioni

#### 3.1 Creare interfaccia servizio

**File**: `MdExplorer.Features/Exports/IWordTemplateService.cs`

```C#
using MdExplorer.Features.Yaml.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MdExplorer.Features.Exports
{
    public interface IWordTemplateService
    {
        /// <summary>
        /// Inserisce le pagine predefinite nel contenuto markdown
        /// </summary>
        Task<string> InsertPredefinedPagesAsync(
            string markdownContent, 
            MdExplorerDocumentDescriptor descriptor,
            string projectPath);
        
        /// <summary>
        /// Sostituisce i tag in un template
        /// </summary>
        string ReplaceTags(string template, Dictionary<string, string> tags);
    }
}
```

#### 3.2 Implementare servizio

**File**: `MdExplorer.Features/Exports/WordTemplateService.cs`

```C#
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
        private readonly Regex _tagRegex = new Regex(@"\{\{([^}]+)\}\}", RegexOptions.Compiled);

        public WordTemplateService(ILogger<WordTemplateService> logger)
        {
            _logger = logger;
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

            // 2. Contenuto principale
            result.AppendLine(markdownContent);

            // 3. Inserisci Disclaimer
            if (predefinedPages.Disclaimer?.Enabled == true)
            {
                var disclaimerContent = await ProcessPageAsync(predefinedPages.Disclaimer, descriptor, projectPath);
                if (!string.IsNullOrEmpty(disclaimerContent))
                {
                    result.AppendLine(disclaimerContent);
                }
            }

            // 4. Inserisci Appendix
            if (predefinedPages.Appendix?.Enabled == true)
            {
                var appendixContent = await ProcessPageAsync(predefinedPages.Appendix, descriptor, projectPath);
                if (!string.IsNullOrEmpty(appendixContent))
                {
                    result.AppendLine(appendixContent);
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
                // Usa la struttura esistente .md/templates/word/pages/
                var templatePath = Path.Combine(projectPath, ".md", "templates", "word", "pages", 
                                               page.Type, $"{page.Template}.md");
                
                if (!File.Exists(templatePath))
                {
                    _logger.LogWarning($"Template non trovato: {templatePath}");
                    return string.Empty;
                }

                var templateContent = await File.ReadAllTextAsync(templatePath);
                var allTags = BuildTagsDictionary(page, descriptor, projectPath);
                
                return ReplaceTags(templateContent, allTags);
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

            // Tag globali dal documento
            if (!string.IsNullOrEmpty(descriptor.Title))
                tags["title"] = descriptor.Title;
            if (!string.IsNullOrEmpty(descriptor.Author))
                tags["author"] = descriptor.Author;
            if (!string.IsNullOrEmpty(descriptor.Email))
                tags["email"] = descriptor.Email;
            if (!string.IsNullOrEmpty(descriptor.Date))
                tags["date"] = descriptor.Date;

            // Tag di sistema
            tags["current_date"] = DateTime.Now.ToString("dd/MM/yyyy");
            tags["current_time"] = DateTime.Now.ToString("HH:mm:ss");
            tags["project_path"] = projectPath;

            // Tag specifici della pagina (sovrascrivono quelli globali)
            if (page.Tags != null)
            {
                foreach (var tag in page.Tags)
                {
                    tags[tag.Key] = tag.Value;
                }
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
                    return match.Value; // Mantieni il tag originale se non trovato
                }
            });
        }
    }
}
```

### Step 4: Registrazione Dependency Injection

**Obiettivo**: Registrare il nuovo servizio nel container DI esistente

#### 4.1 Aggiornare FeaturesDI.cs

**File**: `MdExplorer.Features/FeaturesDI.cs`

```C#
// Aggiungi questa riga nel metodo AddFeatures esistente
services.AddScoped<IWordTemplateService, WordTemplateService>();
```

### Step 5: Integrazione nel Controller Esistente

**Obiettivo**: Estendere MdExportController per utilizzare il nuovo servizio

#### 5.1 Modificare MdExportController

**File**: `MdExplorer/Controllers/MdExportController.cs`

Aggiungi il campo al controller esistente:

```C#
private readonly IWordTemplateService _wordTemplateService;
```

Aggiorna il costruttore esistente:

```C#
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

```C#
// Inserisci pagine predefinite se configurate
if (docDesc?.WordSection?.PredefinedPages != null)
{
    readText = await _wordTemplateService.InsertPredefinedPagesAsync(
        readText, 
        docDesc, 
        _fileSystemWatcher.Path
    );
    
    _logger.LogInformation("Pagine predefinite inserite per il documento {0}", filePath);
}
```

### Step 6: Test Unitari

**Obiettivo**: Creare test per verificare il funzionamento

#### 6.1 Creare test per WordTemplateService

**File**: `MdExplorer.Features.Tests/Exports/WordTemplateServiceTests.cs`

```C#
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

## 📊 Test di Accettazione

### Test 1: Integrazione con Sistema Esistente

1. Seleziona un progetto esistente
2. Verifica che la directory `.md/templates/word/pages/` sia creata automaticamente
3. Verifica che i template di default siano copiati

### Test 2: Export con Cover Page

1. Crea un documento Markdown con YAML che include una cover page
2. Esporta in Word usando il sistema esistente
3. Verifica che la cover page sia presente con i tag sostituiti

### Test 3: Retrocompatibilità

1. Esporta un documento senza configurazione pagine predefinite
2. Verifica che l'export funzioni esattamente come prima
3. Verifica che tutti i template .docx esistenti funzionino ancora

## 🚀 Deploy e Configurazione

### 1. Aggiungere Template come Embedded Resources

I template Markdown devono essere aggiunti come embedded resources al progetto `MdExplorer.Service` nella stessa modalità dei template esistenti.

### 2. Verifica Sistema Esistente

Il sistema si integra perfettamente con l'infrastruttura esistente:

* **ProjectsManager** gestisce la copia automatica
* **MdExportController** gestisce l'export
* **FileUtil** gestisce l'estrazione delle risorse

## 📈 Metriche di Successo

* ✅ Tutti i test unitari passano
* ✅ Export Word funziona con e senza pagine predefinite
* ✅ Tag vengono sostituiti correttamente
* ✅ **Nessuna regressione sugli export esistenti**
* ✅ **Sistema template esistente non modificato**
* ✅ Performance: < 100ms overhead per inserimento pagine

## 📚 Documentazione per Utenti

### Esempio YAML Minimo

```YAML
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

### Personalizzazione Template

Gli utenti possono modificare i template nella directory del progetto:

```
MioProgetto/
├── .md/
│   └── templates/
│       └── word/
│           └── pages/
│               ├── covers/
│               │   ├── standard.md    [modificabile]
│               │   └── project.md     [modificabile]
│               ├── disclaimers/
│               └── appendices/
```

## ⚡ Comandi Utili per Development

```Shell
# Build del progetto
dotnet build MdExplorer.Features/MdExplorer.Features.csproj

# Run dei test
dotnet test MdExplorer.Features.Tests/MdExplorer.Features.Tests.csproj --filter "FullyQualifiedName~WordTemplateService"

# Debug del servizio
dotnet run --project MdExplorer/MdExplorer.Service.csproj
```

***

**Nota**: Questo sprint si integra perfettamente con il sistema esistente. Ogni step estende funzionalità esistenti senza modificare il comportamento attuale.
