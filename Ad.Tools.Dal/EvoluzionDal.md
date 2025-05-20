# Refactoring Evolutivo di Ad.Tools.Dal con Pattern Unit of Work

## Obiettivo

Refactoring del layer di accesso ai dati (`Ad.Tools.Dal`) per introdurre il pattern **Unit of Work (UoW)** e **Repository**, migliorando la gestione delle transazioni, la testabilità e l'allineamento con le pratiche moderne di NHibernate. Si mira inoltre a risolvere l'uso di API obsolete e aggiornare potenziali dipendenze vulnerabili.

## Approccio Scelto: Strangler Fig Pattern

Per minimizzare i rischi e permettere un'evoluzione graduale senza interrompere la funzionalità esistente durante il processo, adotteremo lo **Strangler Fig Pattern**.

Questo approccio prevede la creazione di una nuova implementazione del DAL (che chiameremo `Ad.Tools.Dal.Evo`) che coesisterà temporaneamente con quella vecchia (`Ad.Tools.Dal`). I componenti dell'applicazione (principalmente nel BLL - Business Logic Layer) verranno migrati incrementalmente per utilizzare la nuova implementazione. Man mano che più componenti usano il nuovo DAL, quello vecchio viene progressivamente "strangolato" fino a poter essere rimosso in sicurezza.

```mermaid
graph TD
    A[Applicazione / BLL] -->|Chiama| O(Ad.Tools.Dal - Vecchio);
    A -->|Chiama (gradualmente)| N(Ad.Tools.Dal.Evo - Nuovo);
    O --> DB[(Database)];
    N --> DB;

    subgraph "Fase Iniziale"
        A -- Utilizzo Esclusivo --> O;
    end

    subgraph "Fase Intermedia (Migrazione)"
        A -- Utilizzo Misto --> O;
        A -- Utilizzo Misto --> N;
    end

    subgraph "Fase Finale"
        A -- Utilizzo Esclusivo --> N;
        O -- Dismesso --> X((X));
    end

    style O fill:#f9f,stroke:#333,stroke-width:2px;
    style N fill:#9cf,stroke:#333,stroke-width:2px;
```

**Vantaggi di Questo Approccio:**

*   **Rischio Ridotto:** L'applicazione principale rimane funzionante durante il refactoring. Le modifiche sono isolate.
*   **Progresso Incrementale:** Le modifiche vengono introdotte e testate gradualmente, componente per componente.
*   **Rollback Facilitato:** Se la migrazione di un componente causa problemi, è più semplice isolare il problema e/o tornare alla vecchia implementazione per quel componente specifico senza bloccare l'intera applicazione.
*   **Nessun "Big Bang":** Si evita un unico grande rilascio con molte modifiche rischiose e difficili da testare contemporaneamente.
*   **Feedback Continuo:** Si ottiene feedback sul funzionamento del nuovo DAL man mano che viene utilizzato da più parti dell'applicazione.

## Piano Dettagliato

La tabella seguente descrive i passi incrementali pianificati. Aggiorneremo lo stato man mano che procediamo.

| Fase                      | Passo                                                                 | Descrizione                                                                                                                                                              | Stato     |
| :------------------------ | :-------------------------------------------------------------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :-------- |
| **1: Creazione Nuovo DAL** | 1.1 Creare `Ad.Tools.Dal.Evo.Abstractions`                            | Nuovo progetto libreria .NET Standard. Definire le interfacce `IRepository<T>` e `IUnitOfWork` (con `CommitAsync`, `GetRepository<T>`, `IDisposable`, ecc.).                 | `[X]`     |
|                           | 1.2 Creare `Ad.Tools.Dal.Evo`                                         | Nuovo progetto libreria .NET Standard. Referenziare `Abstractions`, NHibernate, FluentNHibernate. Implementare `Repository<T>` e `UnitOfWork`. Adattare/aggiornare config NHibernate. | `[X]`     |
|                           | 1.3 Configurazione DI per `Ad.Tools.Dal.Evo`                          | Creare metodo di estensione `AddDalEvoFeatures` per registrare `IUnitOfWork` (Scoped), `IRepository<T>` (Transient), `ISessionFactory` (Singleton).                         | `[X]`     |
| **2: Migrazione BLL/Svc** | 2.1 Identificare Componente Pilota                                    | Scegliere un componente BLL/Service (es. `PlantumlServer`) con dipendenze chiare dal vecchio DAL per iniziare la migrazione.                                               | `[ ]`     |
|                           | 2.2 Migrare Componente Pilota                                         | Modificare il componente per iniettare e usare `IUnitOfWork` dal nuovo DAL (`Ad.Tools.Dal.Evo.Abstractions`). Aggiornare logica per `GetRepository` e `CommitAsync`.       | `[ ]`     |
|                           | 2.3 Aggiornare DI per Componente Pilota                               | Modificare la registrazione DI principale per fornire al componente pilota le dipendenze dal nuovo `AddDalEvoFeatures`.                                                    | `[ ]`     |
|                           | 2.4 Testare Componente Pilota                                         | Verificare che il componente migrato funzioni correttamente con il nuovo DAL attraverso test unitari e/o funzionali.                                                      | `[ ]`     |
|                           | 2.5 Migrare Componenti Restanti                                       | Ripetere i passi 2.2-2.4 per gli altri componenti BLL/Service che usano il DAL, procedendo incrementalmente.                                                             | `[ ]`     |
|                           | 2.6 Test Funzionali Completi                                          | Eseguire test funzionali completi dell'applicazione con l'utilizzo misto dei DAL per assicurare che non ci siano regressioni.                                              | `[ ]`     |
| **3: Completamento**      | 3.1 Verifica Finale Migrazione                                        | Assicurarsi che tutti i componenti che necessitano accesso ai dati siano stati migrati e usino esclusivamente il nuovo DAL.                                                | `[ ]`     |
|                           | 3.2 Rimuovere Vecchio DAL (DI)                                        | Rimuovere la chiamata a `AddDalFeatures` (o equivalente) dalla configurazione della Dependency Injection.                                                                | `[ ]`     |
|                           | 3.3 Rimuovere Vecchio DAL (Riferimenti)                               | Rimuovere i riferimenti ai progetti `Ad.Tools.Dal` e `Ad.Tools.Dal.Abstractions` dagli altri progetti nella solution.                                                    | `[ ]`     |
|                           | 3.4 Rimuovere Vecchio DAL (Progetti)                                  | Eliminare i progetti `Ad.Tools.Dal` e `Ad.Tools.Dal.Abstractions` dalla solution.                                                                                        | `[ ]`     |
|                           | 3.5 Test Finali                                                       | Eseguire un'ultima sessione di test completi dell'applicazione assicurandosi che funzioni correttamente usando solo il nuovo DAL.                                            | `[ ]`     |
|                           | 3.6 (Opzionale) Rinominare `*.Evo`                                    | Se desiderato, rinominare i progetti `Ad.Tools.Dal.Evo.*` nei nomi originali (`Ad.Tools.Dal.*`) per pulizia finale.                                                         | `[ ]`     |
| **4: Miglioramenti**      | 4.1 Aggiornare API NHibernate Obsolete nel Nuovo DAL                  | Assicurarsi che `Ad.Tools.Dal.Evo` utilizzi le API NHibernate più recenti e non obsolete.                                                                                | `[ ]`     |
|                           | 4.2 (Opzionale) Aggiornare Dipendenze NuGet nel Nuovo DAL             | Valutare l'aggiornamento di NHibernate, FluentNHibernate e altre dipendenze a versioni più recenti/sicure, gestendo eventuali breaking changes.                             | `[ ]`     |

---

Questo documento serve come traccia condivisa per il refactoring. Aggiorneremo lo stato dei checkbox man mano che completiamo i vari passi.
