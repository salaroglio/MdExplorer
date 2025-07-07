# Workflow Sviluppo Dati Persistenti - MdExplorer

Questo documento descrive il workflow completo per aggiungere o modificare dati persistenti nell'applicazione MdExplorer.

## Indice

1. [Panoramica Architettura](#panoramica-architettura)
2. [Scenario 1: Aggiungere una Nuova Entità](#scenario-1-aggiungere-una-nuova-entità)
3. [Scenario 2: Modificare un'Entità Esistente](#scenario-2-modificare-unentità-esistente)
4. [Best Practices e Convenzioni](#best-practices-e-convenzioni)
5. [Checklist Rapida](#checklist-rapida)

## Panoramica Architettura

Il sistema utilizza un'architettura a layer con:
- **Database**: SQLite con tabelle gestite da FluentMigrator
- **ORM**: NHibernate con mapping FluentNHibernate
- **Backend**: ASP.NET Core con pattern Repository/DAL
- **Frontend**: Angular con servizi HTTP e componenti Material

## Scenario 1: Aggiungere una Nuova Entità

### 1.1 Creazione Migration Database

Creare una nuova migration in `MdExplorer.Migrations/`:

```csharp
using FluentMigrator;
using System;

namespace MdExplorer.Migrations.Version2024
{
    [Migration(20240101001, "Creazione tabella NuovaEntita")]
    public class M2024_01_01_001 : Migration
    {
        public override void Up()
        {
            Create.Table("NuovaEntita")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("Nome").AsString(255).NotNullable()
                .WithColumn("Descrizione").AsString().Nullable()
                .WithColumn("DataCreazione").AsDateTime().NotNullable()
                .WithColumn("Valore").AsDecimal().Nullable();
                
            // Inserimento dati iniziali se necessario
            Insert.IntoTable("NuovaEntita").Row(new 
            { 
                Id = Guid.NewGuid().ToByteArray(), 
                Nome = "Default",
                DataCreazione = DateTime.Now
            });
        }

        public override void Down()
        {
            Delete.Table("NuovaEntita");
        }
    }
}
```

### 1.2 Creazione Entità

Creare l'entità in `MdExplorer.Abstractions/Entities/UserDB/`:

```csharp
using System;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    public class NuovaEntita
    {
        public virtual Guid Id { get; set; }
        public virtual string Nome { get; set; }
        public virtual string Descrizione { get; set; }
        public virtual DateTime DataCreazione { get; set; }
        public virtual decimal? Valore { get; set; }
    }
}
```

### 1.3 Creazione Mapping NHibernate

Creare il mapping in `MDExplorer.dal/Mapping/`:

```csharp
using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MDExplorer.DataAccess.Mapping
{
    public class NuovaEntitaMap : ClassMap<NuovaEntita>
    {
        public NuovaEntitaMap()
        {
            Table("NuovaEntita");
            Id(x => x.Id).GeneratedBy.GuidComb();
            Map(x => x.Nome).Not.Nullable();
            Map(x => x.Descrizione).Nullable();
            Map(x => x.DataCreazione).Not.Nullable();
            Map(x => x.Valore).Nullable();
        }
    }
}
```

### 1.4 Creazione Model/DTO Angular

Creare l'interfaccia in `MdExplorer/client2/src/app/Models/`:

```typescript
export interface INuovaEntita {
  id: string;
  nome: string;
  descrizione?: string;
  dataCreazione: Date;
  valore?: number;
}
```

### 1.5 Creazione API Controller

Creare o estendere un controller in `MdExplorer/Controllers/`:

```csharp
using Ad.Tools.Dal.Abstractions.Interfaces;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace MdExplorer.Service.Controllers
{
    [ApiController]
    [Route("api/NuovaEntita/{action}")]
    public class NuovaEntitaController : ControllerBase
    {
        private readonly IUserSettingsDB _session;

        public NuovaEntitaController(IUserSettingsDB session)
        {
            _session = session;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var dal = _session.GetDal<NuovaEntita>();
            var entities = dal.GetList();
            return Ok(entities);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            var dal = _session.GetDal<NuovaEntita>();
            var entity = dal.GetList().FirstOrDefault(x => x.Id == id);
            if (entity == null)
                return NotFound();
            return Ok(entity);
        }

        [HttpPost]
        public IActionResult Create([FromBody] NuovaEntita entity)
        {
            entity.Id = Guid.NewGuid();
            entity.DataCreazione = DateTime.Now;
            
            var dal = _session.GetDal<NuovaEntita>();
            _session.BeginTransaction();
            dal.Save(entity);
            _session.Commit();
            
            return Ok(entity);
        }

        [HttpPut("{id}")]
        public IActionResult Update(Guid id, [FromBody] NuovaEntita entity)
        {
            var dal = _session.GetDal<NuovaEntita>();
            var existingEntity = dal.GetList().FirstOrDefault(x => x.Id == id);
            if (existingEntity == null)
                return NotFound();
                
            existingEntity.Nome = entity.Nome;
            existingEntity.Descrizione = entity.Descrizione;
            existingEntity.Valore = entity.Valore;
            
            _session.BeginTransaction();
            dal.Save(existingEntity);
            _session.Commit();
            
            return Ok(existingEntity);
        }
    }
}
```

### 1.6 Creazione Servizio Angular

Creare il servizio in `MdExplorer/client2/src/app/services/`:

```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { INuovaEntita } from '../Models/INuovaEntita';

@Injectable({
  providedIn: 'root'
})
export class NuovaEntitaService {
  private baseUrl = '../api/NuovaEntita';

  constructor(private http: HttpClient) { }

  getAll(): Observable<INuovaEntita[]> {
    return this.http.get<INuovaEntita[]>(`${this.baseUrl}/GetAll`);
  }

  getById(id: string): Observable<INuovaEntita> {
    return this.http.get<INuovaEntita>(`${this.baseUrl}/GetById/${id}`);
  }

  create(entity: INuovaEntita): Observable<INuovaEntita> {
    return this.http.post<INuovaEntita>(`${this.baseUrl}/Create`, entity);
  }

  update(id: string, entity: INuovaEntita): Observable<INuovaEntita> {
    return this.http.put<INuovaEntita>(`${this.baseUrl}/Update/${id}`, entity);
  }
}
```

### 1.7 Creazione Componente Angular

Creare il componente per gestire l'entità:

```typescript
import { Component, OnInit } from '@angular/core';
import { NuovaEntitaService } from '../../../services/nuova-entita.service';
import { INuovaEntita } from '../../../Models/INuovaEntita';

@Component({
  selector: 'app-nuova-entita',
  templateUrl: './nuova-entita.component.html',
  styleUrls: ['./nuova-entita.component.scss']
})
export class NuovaEntitaComponent implements OnInit {
  entities: INuovaEntita[] = [];
  selectedEntity: INuovaEntita;

  constructor(private nuovaEntitaService: NuovaEntitaService) { }

  ngOnInit(): void {
    this.loadEntities();
  }

  loadEntities(): void {
    this.nuovaEntitaService.getAll().subscribe(
      data => this.entities = data,
      error => console.error('Errore caricamento entità', error)
    );
  }

  save(): void {
    if (this.selectedEntity.id) {
      this.nuovaEntitaService.update(this.selectedEntity.id, this.selectedEntity)
        .subscribe(() => this.loadEntities());
    } else {
      this.nuovaEntitaService.create(this.selectedEntity)
        .subscribe(() => this.loadEntities());
    }
  }
}
```

## Scenario 2: Modificare un'Entità Esistente

### 2.1 Migration per Alterare Tabella

```csharp
[Migration(20240102001, "Aggiunta colonna a Setting")]
public class M2024_01_02_001 : Migration
{
    public override void Up()
    {
        Alter.Table("Setting")
            .AddColumn("NuovaColonna").AsString(100).Nullable();
            
        // Opzionale: popolare la nuova colonna
        Execute.Sql("UPDATE Setting SET NuovaColonna = 'valore_default' WHERE NuovaColonna IS NULL");
    }

    public override void Down()
    {
        Delete.Column("NuovaColonna").FromTable("Setting");
    }
}
```

### 2.2 Aggiornamento Entità

Aggiungere la nuova proprietà all'entità esistente:

```csharp
public class Setting
{
    // Proprietà esistenti...
    public virtual string NuovaColonna { get; set; }
}
```

### 2.3 Aggiornamento Mapping

Aggiungere il mapping per la nuova proprietà:

```csharp
public class SettingsMap : ClassMap<Setting>
{
    public SettingsMap()
    {
        // Mapping esistenti...
        Map(x => x.NuovaColonna).Nullable();
    }
}
```

### 2.4 Aggiornamento Model Angular

```typescript
export interface IMdSetting {
  // Proprietà esistenti...
  nuovaColonna?: string;
}
```

### 2.5 Aggiornamento Componente Angular

Aggiungere il nuovo campo nel template HTML:

```html
<mat-form-field appearance="outline">
  <mat-label>Nuova Colonna</mat-label>
  <input matInput [(ngModel)]="nuovaColonna" placeholder="Inserisci valore">
  <mat-hint>Descrizione del nuovo campo</mat-hint>
</mat-form-field>
```

E nel componente TypeScript:

```typescript
export class SettingsComponent implements OnInit {
  // Proprietà esistenti...
  nuovaColonna: string;

  ngOnInit(): void {
    this.appCurrentFolder.settings.subscribe((data: any) => {
      var settings = data.settings as IMdSetting[];
      if (settings != undefined) {
        // Codice esistente...
        this.nuovaColonna = settings.filter(_ => _.name === "NuovaColonna")[0]?.valueString || null;
      }
    });
  }

  save() {
    // Codice esistente...
    this._settings.filter(_ => _.name === "NuovaColonna")[0].valueString = this.nuovaColonna;
    // Resto del metodo save...
  }
}
```

## Best Practices e Convenzioni

### Naming Conventions

- **Tabelle DB**: PascalCase singolare (es. `Setting`, `Document`)
- **Colonne DB**: PascalCase (es. `ValueString`, `DataCreazione`)
- **Classi C#**: PascalCase (es. `NuovaEntita`, `SettingsMap`)
- **Proprietà C#**: PascalCase (es. `ValueString`, `Id`)
- **File TypeScript**: kebab-case (es. `nuova-entita.service.ts`)
- **Proprietà TypeScript**: camelCase (es. `valueString`, `dataCreazione`)

### Struttura Progetto

- **Migrations**: `MdExplorer.Migrations/VersionAAAA-MM/`
- **Entità**: `MdExplorer.Abstractions/Entities/UserDB/`
- **Mapping**: `MDExplorer.dal/Mapping/`
- **Controller**: `MdExplorer/Controllers/`
- **Model Angular**: `MdExplorer/client2/src/app/Models/`
- **Servizi Angular**: `MdExplorer/client2/src/app/services/`
- **Componenti Angular**: `MdExplorer/client2/src/app/md-explorer/components/`

### Pattern e Pratiche

1. **Transazioni**: Sempre usare transazioni per operazioni di scrittura
2. **Validazione**: Validare i dati sia lato client che server
3. **Error Handling**: Gestire errori sia nel backend che frontend
4. **Lazy Loading**: Utilizzare `virtual` per le proprietà delle entità NHibernate
5. **DTO Pattern**: Considerare l'uso di DTO separati per API pubbliche

## Checklist Rapida

### Per Nuova Entità
- [ ] Creare migration FluentMigrator
- [ ] Creare classe entità
- [ ] Creare mapping NHibernate
- [ ] Creare interfaccia TypeScript
- [ ] Creare/estendere API controller
- [ ] Creare servizio Angular
- [ ] Creare/aggiornare componente Angular
- [ ] Testare il flusso completo

### Per Modifica Entità
- [ ] Creare migration per alterare tabella
- [ ] Aggiornare classe entità
- [ ] Aggiornare mapping NHibernate
- [ ] Aggiornare interfaccia TypeScript
- [ ] Aggiornare API se necessario
- [ ] Aggiornare componente Angular
- [ ] Testare che i dati esistenti non siano compromessi

## Esempi dal Codice Esistente

### Setting (Esempio Completo)

1. **Migration**: `M2021_06_23_001.cs` - Creazione tabella Setting
2. **Entità**: `Setting.cs` in `MdExplorer.Abstractions/Entities/UserDB/`
3. **Mapping**: `SettingsMap.cs` in `MDExplorer.dal/Mapping/`
4. **Controller**: `AppSettingsController.cs` con metodi GetSettings/SetSettings
5. **Servizio Angular**: `AppCurrentMetadataService` con loadSettings/saveSettings
6. **Componente**: `SettingsComponent` con dialog per modifica settings

Questo workflow garantisce che tutti i layer dell'applicazione siano correttamente aggiornati quando si aggiungono o modificano dati persistenti.