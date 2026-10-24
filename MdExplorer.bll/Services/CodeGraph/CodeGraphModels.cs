using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>L'esito del caricamento di uno script.</summary>
    public sealed class CodeScriptResult
    {
        public string ScriptPath { get; set; }
        public string Kind { get; set; }
        public bool Loaded { get; set; }
        public bool Skipped { get; set; }
        public bool Removed { get; set; }
        public int RecordCount { get; set; }
        public string Error { get; set; }
        public bool HasError => Error != null;
    }

    /// <summary>L'esito della «generazione della conoscenza» su un progetto.</summary>
    public sealed class CodeScriptLoadOutcome
    {
        public bool ScriptsFolderMissing { get; set; }
        /// <summary>Script di uno schema ritirato cancellati in questo giro (<see cref="ICodeGraphStore.RetireOldScriptsAsync"/>).</summary>
        public int Retired { get; set; }
        public List<CodeScriptResult> Results { get; } = new();
        public int Loaded => Results.Count(r => r.Loaded);
        public int Skipped => Results.Count(r => r.Skipped);
        public int Removed => Results.Count(r => r.Removed);
        public int Failed => Results.Count(r => r.HasError);
        public string FirstError => Results.FirstOrDefault(r => r.HasError)?.Error;
    }

    public sealed class CodeGraphStatus
    {
        public bool ScriptsFolderExists { get; set; }
        public int ScriptsOnDisk { get; set; }
        public int ScriptsLoaded { get; set; }
        /// <summary>Script su disco con hash diverso da quello caricato, o mai caricati: «conoscenza da generare».</summary>
        public int ScriptsPending { get; set; }
        public int Files { get; set; }
        public int Symbols { get; set; }
        public int ExternalSymbols { get; set; }
        public int Parameters { get; set; }
        public int Edges { get; set; }
        public int DescribedSymbols { get; set; }
        /// <summary>Dove sta il grafo: base URI del Fuseki gestito, dataset e grafo nominato del progetto.</summary>
        public string Endpoint { get; set; }
        public string Graph { get; set; }
    }

    public sealed class CodeSymbolHit
    {
        public string Id { get; set; }
        public string Kind { get; set; }
        public string Name { get; set; }
        public string QualifiedName { get; set; }
        public string Path { get; set; }
        public int? LineStart { get; set; }
        public string Signature { get; set; }
        public string Summary { get; set; }
    }

    /// <summary>
    /// L'ultima descrizione di una cartella <c>program</c> (punto 2c del 14/10/2026). Sta nel grafo di stato, non su Git:
    /// dice che cosa è successo su QUESTA macchina. Una traccia <see cref="Running"/> senza un lavoro vivo è una
    /// descrizione interrotta (MdExplorer chiuso o caduto): non riparte da sola, si chiede all'utente.
    /// </summary>
    public sealed class CodeDescriptionRun
    {
        public const string Running = "running";
        public const string Done = "done";
        public const string Cancelled = "cancelled";
        public const string Failed = "failed";

        public string ProgramFolder { get; set; }
        public string Status { get; set; }
        public string Engine { get; set; }
        public bool Force { get; set; }
        /// <summary>Rivede anche le descrizioni «da rivedere» (F12b): una ripresa lo ricorda.</summary>
        public bool Review { get; set; }
        public string StartedAt { get; set; }
        public string UpdatedAt { get; set; }
        /// <summary>Il file a cui era arrivata (1-based) sul totale; <see cref="CurrentFile"/> il suo percorso logico.</summary>
        public int FileIndex { get; set; }
        public int FileTotal { get; set; }
        public string CurrentFile { get; set; }
        public int FilesDescribed { get; set; }
        /// <summary>
        /// Le domande FINITE (riuscite o fallite): è il numero da mostrare. <see cref="FileIndex"/> è la prima non finita,
        /// che serve per riprendere; dal basso le domande finiscono fuori ordine, e una lenta fra le prime lo teneva fermo
        /// mentre il lavoro andava avanti (Windows, 22/10/2026: «vedo i file cambiare, ma non aumentano le domande»).
        /// </summary>
        public int QuestionsDone { get; set; }
        public string Error { get; set; }
        /// <summary>
        /// Solo in memoria, non nel grafo: quando e da quale file è partita QUESTA sessione (una ripresa riparte dal mezzo),
        /// per la velocità e il tempo che manca nel pannello dei lavori in sfondo (punto 2d).
        /// </summary>
        public string SessionStartedAt { get; set; }
        public int SessionStartIndex { get; set; }
    }

    /// <summary>
    /// La mappa per livelli del grafo del codice (vista guidata, passo 2 del 14/10/2026): invece di migliaia di nodi, le
    /// parti del livello corrente (i programmi, oppure le sottocartelle di una cartella) con le frecce SOMMATE fra loro.
    /// </summary>
    public sealed class CodeGraphMap
    {
        /// <summary><c>programs</c> (tutto il progetto) o <c>folders</c> (dentro una cartella).</summary>
        public string Level { get; set; }
        /// <summary>La cartella mostrata, dopo aver compattato le catene con un figlio solo; vuota = il progetto.</summary>
        public string Folder { get; set; }
        public List<CodeGraphMapGroup> Groups { get; set; } = new();
        public List<CodeGraphMapLink> Links { get; set; } = new();
        public int Files { get; set; }
        public int Symbols { get; set; }
        /// <summary>Le cartelle raccolte in «+N altre» (oltre le più grandi).</summary>
        public int HiddenGroups { get; set; }
        /// <summary>Quelle cartelle, dalla più grande: cliccando «+N altre» si vedono e ci si entra.</summary>
        public List<CodeGraphMapGroup> More { get; set; } = new();
    }

    public sealed class CodeGraphMapGroup
    {
        /// <summary>Id nel disegno: la cartella, oppure <c>__files</c> (i file sciolti qui), <c>__more</c>, <c>__out</c> (fuori da qui).</summary>
        public string Id { get; set; }
        public string Label { get; set; }
        /// <summary><c>program</c>, <c>folder</c>, <c>files</c>, <c>more</c>, <c>outside</c>.</summary>
        public string Kind { get; set; }
        /// <summary>Dove si entra cliccando (percorso logico); null se non ci si entra.</summary>
        public string Folder { get; set; }
        public int Files { get; set; }
        public int Symbols { get; set; }
    }

    public sealed class CodeGraphMapLink
    {
        public string From { get; set; }
        public string To { get; set; }
        public int Count { get; set; }
        public Dictionary<string, int> ByRelation { get; set; } = new();
        public Dictionary<string, int> ByConfidence { get; set; } = new();
    }

    /// <summary>I conteggi di una cartella <c>program</c> nel grafo (punto 4b del 14/10, <c>code_inventory</c>).</summary>
    public sealed class CodeProgramCounts
    {
        public int Files { get; set; }
        public int DescribedFiles { get; set; }
        /// <summary>I simboli del programma, senza gli <c>external</c>.</summary>
        public int Symbols { get; set; }
        public int DescribedSymbols { get; set; }
    }

    /// <summary>Lo stato dello scanner per una cartella <c>program</c> (D14): che commit ha analizzato e in che modo.</summary>
    public sealed class CodeScanState
    {
        public string ProgramFolder { get; set; }
        public string RepositoryRoot { get; set; }
        public string AnalyzedCommit { get; set; }
        /// <summary><c>git</c> oppure <c>hash</c> (cartella fuori da un repository: scansione completa, dichiarata).</summary>
        public string Mode { get; set; }
        public string ScannedAt { get; set; }
        public int Candidates { get; set; }
        public int Reparsed { get; set; }
        /// <summary>I ponti attivi all'ultima analisi («dipendenza@linguaggio;…», punto 3b): se cambiano, il programma si riscrive.</summary>
        public string Bridges { get; set; }
    }

    public sealed class CodeSymbolView
    {
        public string Id { get; set; }
        public string Kind { get; set; }
        public string Name { get; set; }
        public string QualifiedName { get; set; }
        public string Path { get; set; }
        public int? LineStart { get; set; }
        public int? LineEnd { get; set; }
        public string Signature { get; set; }
        public string ReturnType { get; set; }
        public string Visibility { get; set; }
        public bool IsStatic { get; set; }
        public bool IsAsync { get; set; }
        public string ParentId { get; set; }
        public string Language { get; set; }
        /// <summary>La condizione sotto cui il simbolo esiste (un controllo dentro <c>$if[…]$</c> di una pagina, D42 passo 2); null = sempre.</summary>
        public string Guard { get; set; }
        public string Summary { get; set; }
        public string ReturnsDescription { get; set; }
        public string Remarks { get; set; }
        public string WrittenBy { get; set; }
        /// <summary>true quando la descrizione è stata scritta per una firma diversa da quella attuale; null senza descrizione.</summary>
        public bool? DescriptionStale { get; set; }
        public List<CodeParameterView> Parameters { get; set; } = new();
    }

    public sealed class CodeParameterView
    {
        public int Position { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Direction { get; set; }
        public string DefaultValue { get; set; }
        public bool IsNullable { get; set; }
        public string Description { get; set; }
    }

    /// <summary>Un punto di chiamata con stringhe letto dal grafo (D42): chiamante, dove, ricevente e stringhe per posizione.</summary>
    public sealed class CodeCallSiteView
    {
        public string CallerId { get; set; }
        public string Callee { get; set; }
        public string Path { get; set; }
        public int Line { get; set; }
        public int Ordinal { get; set; }
        public string Receiver { get; set; }
        public int ArgumentCount { get; set; }
        public SortedDictionary<int, string> Arguments { get; } = new();
    }

    public sealed class CodeEdgeView
    {
        public string Relation { get; set; }
        public string Confidence { get; set; }
        public string Path { get; set; }
        public int? Line { get; set; }
        /// <summary>La condizione sotto cui l'arco vale (una pagina inclusa dentro <c>$if[…]$</c>); null = sempre.</summary>
        public string Guard { get; set; }
        /// <summary>L'altro capo dell'arco (chi chiama, o chi è chiamato, a seconda della direzione chiesta).</summary>
        public CodeSymbolView Other { get; set; }
        public int Depth { get; set; }
    }

    public sealed class CodeOverview
    {
        public int Files { get; set; }
        public int Symbols { get; set; }
        public int ExternalSymbols { get; set; }
        public int Edges { get; set; }
        public int DescribedSymbols { get; set; }
        /// <summary>I file con la loro code:FileDescription (a che cosa servono).</summary>
        public int DescribedFiles { get; set; }
        public Dictionary<string, int> SymbolsByLanguage { get; set; } = new();
        public Dictionary<string, int> SymbolsByKind { get; set; } = new();
        public Dictionary<string, int> EdgesByRelation { get; set; } = new();
        public List<(CodeSymbolView Symbol, int Degree)> MostConnected { get; set; } = new();
        public List<(string Path, int Symbols, string Summary)> LargestFiles { get; set; } = new();
    }

    /// <summary>Quello che serve al report: la panoramica più ciò che manca.</summary>
    public sealed class CodeReportData
    {
        public CodeOverview Overview { get; set; }
        public int Undescribed { get; set; }
        public List<string[]> UndescribedList { get; set; } = new();   // kind, qualifiedName, path, line
        public int Ambiguous { get; set; }
        /// <summary>Archi ambigui scelti dall'LLM (F12c): le loro candidate scartate non si contano più fra gli ambigui.</summary>
        public int LlmResolved { get; set; }
        public int Inferred { get; set; }
        public List<string[]> AmbiguousList { get; set; } = new();     // source qn, relation, target qn, path, line
        public List<string[]> StaleList { get; set; } = new();         // qualifiedName, path, line
    }

    /// <summary>Una chiamata di una pagina verso una funzione di un file JS che la pagina carica (D42 passo 2c).</summary>
    public sealed class CodePageScriptCall
    {
        public string CallerId { get; set; }
        public string Callee { get; set; }
        public int Line { get; set; }
        public string Path { get; set; }
        public string FunctionId { get; set; }
    }
}
