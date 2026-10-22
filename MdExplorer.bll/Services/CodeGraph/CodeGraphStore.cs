using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services.Fuseki;
using MdExplorer.Features.Services.KnowledgeGraph;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Il grafo del codice in Fuseki (D19): il passo «script → triple» della generazione della conoscenza (D9) e il
    /// modello di lettura dei tool (D15). Un dataset <c>code</c> per installazione, un grafo nominato per progetto
    /// (<c>urn:mde:code:{hash}:data</c>) più il grafo di stato (<c>…:state</c>: script caricati con hash, commit
    /// analizzato per cartella). I tool leggono SOLO da qui, mai dagli script.
    /// </summary>
    public interface ICodeGraphStore
    {
        /// <summary>Carica tutti gli script di <c>.mde-code/</c> cambiati dall'ultima volta e toglie quelli spariti.</summary>
        /// <param name="compact">false: niente compattazione anche dopo un carico grosso (i caricamenti a metà di una descrizione: si compatta alla fine).</param>
        Task<CodeScriptLoadOutcome> LoadProjectAsync(string projectPath, CancellationToken ct = default, bool compact = true);
        /// <summary>
        /// La ricarica DA CAPO di una cartella program (22/10/2026, quando l'analisi ne ha riscritto tutti gli script): via
        /// le sue triple in una transazione sola, poi il caricamento a lotti e la compattazione di <see cref="LoadProjectAsync"/>.
        /// Le altre cartelle e gli altri progetti restano come sono.
        /// </summary>
        Task<CodeScriptLoadOutcome> ReloadFolderAsync(string projectPath, string folderRelative, CancellationToken ct = default);
        /// <summary>Compatta il dataset del grafo del codice (TDB2 copy-on-write): alla fine di un giro che ha caricato a lotti senza compattare.</summary>
        Task CompactAsync(CancellationToken ct = default);
        /// <summary>Carica (o ricarica, se l'hash è diverso) un singolo script.</summary>
        Task<CodeScriptResult> LoadScriptAsync(string projectPath, string scriptAbsolutePath, CancellationToken ct = default);
        /// <summary>Toglie dal grafo tutto ciò che uno script aveva portato.</summary>
        Task<CodeScriptResult> RemoveScriptAsync(string projectPath, string scriptAbsolutePath, CancellationToken ct = default);
        Task<CodeGraphStatus> GetStatusAsync(string projectPath, CancellationToken ct = default);
        /// <summary>
        /// Le triple di una cartella program (quelle che la sua ricarica toglierebbe: i nodi dei suoi script e i loro archi
        /// diretti) e quelle di tutto il dataset, per stimare quanto della cartella sta nel database (22/10/2026).
        /// </summary>
        Task<(long Folder, long Total)> TripleCountsAsync(string projectPath, string folderRelative, CancellationToken ct = default);

        Task<CodeScanState> GetScanStateAsync(string projectPath, string programFolder, CancellationToken ct = default);
        Task UpsertScanStateAsync(string projectPath, CodeScanState state, CancellationToken ct = default);
        /// <summary>
        /// Cancella gli script di uno schema ritirato (Turtle più vecchio di <see cref="CodeScriptParser.CurrentSchema"/>,
        /// JSON Lines) e, se ce n'erano, lo stato dello scanner del progetto: l'analisi seguente è completa. Restituisce
        /// quanti script ha tolto.
        /// </summary>
        Task<int> RetireOldScriptsAsync(string projectPath, CancellationToken ct = default);
        /// <summary>La traccia dell'ultima descrizione di ogni cartella program (punto 2c del 14/10).</summary>
        Task<List<CodeDescriptionRun>> GetDescriptionRunsAsync(string projectPath, CancellationToken ct = default);
        Task UpsertDescriptionRunAsync(string projectPath, CodeDescriptionRun run, CancellationToken ct = default);
        /// <summary>«Lascia stare»: la traccia si toglie e l'interruzione non si chiede più.</summary>
        Task RemoveDescriptionRunAsync(string projectPath, string programFolder, CancellationToken ct = default);

        /// <summary>Ricerca a testo pieno (jena-text) su nomi, nomi qualificati, descrizioni e descrizioni dei parametri.</summary>
        Task<IReadOnlyList<CodeSymbolHit>> SearchSymbolsAsync(string projectPath, string query, int limit = 20, CancellationToken ct = default);
        /// <summary>Il simbolo per id, nome qualificato o nome; con parametri e descrizioni. Più candidati = ambiguità da sciogliere.</summary>
        Task<List<CodeSymbolView>> ResolveAsync(string projectPath, string idOrName, int limit = 10, CancellationToken ct = default);
        Task<CodeSymbolView> DescribeAsync(string projectPath, string symbolId, CancellationToken ct = default);
        Task<List<CodeSymbolView>> OutlineAsync(string projectPath, string path, CancellationToken ct = default);
        /// <summary>A che cosa serve il file (code:FileDescription); null se non è ancora descritto.</summary>
        Task<string> FileSummaryAsync(string projectPath, string path, CancellationToken ct = default);
        Task<List<CodeEdgeView>> NeighborsAsync(string projectPath, string symbolId, string relation, string direction, int depth, int limit, CancellationToken ct = default);
        Task<List<CodeEdgeView>> ImpactAsync(string projectPath, string symbolId, int depth, int limit, CancellationToken ct = default);
        Task<List<CodeEdgeView>> ShortestPathAsync(string projectPath, string fromId, string toId, CancellationToken ct = default);
        Task<CodeOverview> OverviewAsync(string projectPath, CancellationToken ct = default);
        /// <summary>Quanto di una cartella <c>program</c> è nel grafo: file e simboli scansionati, quanti con una descrizione.</summary>
        Task<CodeProgramCounts> ProgramCountsAsync(string projectPath, string programFolder, CancellationToken ct = default);
        /// <summary>
        /// La mappa per livelli (passo 2 della vista guidata): <paramref name="folder"/> vuota = i programmi
        /// (<paramref name="programs"/>), altrimenti le sottocartelle della cartella con le frecce sommate.
        /// </summary>
        Task<CodeGraphMap> MapAsync(string projectPath, string folder, IReadOnlyList<string> programs, CancellationToken ct = default);
        /// <summary>I simboli dichiarati (non external) da una cartella program: id, tipo, nome qualificato, linguaggio (ponti, 3b).</summary>
        Task<List<(string Id, string Kind, string QualifiedName, string Language)>> DeclaredSymbolsAsync(string projectPath, string programFolder, CancellationToken ct = default);
        Task<CodeGraphViewModel> BuildViewAsync(string projectPath, string folderPrefix, IReadOnlyCollection<string> expandedFiles, IReadOnlyCollection<string> openedSymbols, IReadOnlyCollection<string> hiddenSymbols, string focusSymbolId, CancellationToken ct = default);
        Task<CodeReportData> GetReportDataAsync(string projectPath, CancellationToken ct = default);
    }

    public sealed partial class CodeGraphStore : ICodeGraphStore
    {
        private readonly IFusekiProcessManager _fuseki;
        private readonly IFusekiClient _client;
        private readonly ILogger<CodeGraphStore> _logger;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new(StringComparer.OrdinalIgnoreCase);

        public CodeGraphStore(IFusekiProcessManager fuseki, IFusekiClient client, ILogger<CodeGraphStore> logger)
        {
            _fuseki = fuseki;
            _client = client;
            _logger = logger;
        }

        // ------------------------------------------------------------------ Fuseki

        private sealed class ProjectGraphs
        {
            public string Hash;
            public string Data;
            public string State;
            public string DataIri => "<" + Data + ">";
            public string StateIri => "<" + State + ">";
        }

        private static ProjectGraphs GraphsOf(string projectPath)
        {
            var hash = CodeGraphIris.ProjectHash(projectPath);
            return new ProjectGraphs { Hash = hash, Data = CodeGraphIris.DataGraph(hash), State = CodeGraphIris.StateGraph(hash) };
        }

        private async Task<string> QueryAsync(string sparql, CancellationToken ct)
        {
            var baseUri = await _fuseki.EnsureRunningAsync(ct);
            return await _client.QueryAsync(baseUri, CodeOntology.Dataset, CodeOntology.SparqlPrefixes + sparql, string.Empty, string.Empty);
        }

        private async Task<List<Dictionary<string, string>>> RowsAsync(string sparql, CancellationToken ct)
            => SparqlResults.Parse(await QueryAsync(sparql, ct));

        private async Task UpdateAsync(string sparqlUpdate, CancellationToken ct)
        {
            var baseUri = await _fuseki.EnsureRunningAsync(ct);
            // Gli update costruiti dal writer portano già i PREFIX; quelli di stato no.
            var body = sparqlUpdate.StartsWith("PREFIX", StringComparison.Ordinal) ? sparqlUpdate : CodeOntology.SparqlPrefixes + sparqlUpdate;
            await _client.UpdateAsync(baseUri, CodeOntology.Dataset, body, string.Empty, string.Empty);
        }

        private SemaphoreSlim GateFor(string projectPath) => _locks.GetOrAdd(Path.GetFullPath(projectPath), _ => new SemaphoreSlim(1, 1));

        public static string Sha256(string content)
        {
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(content))).ToLowerInvariant();
        }

        // ------------------------------------------------------------------ caricamento

        public async Task<CodeScriptLoadOutcome> LoadProjectAsync(string projectPath, CancellationToken ct = default, bool compact = true)
        {
            var outcome = new CodeScriptLoadOutcome();
            var root = CodeGraphPaths.GetScriptsRoot(projectPath);
            var g = GraphsOf(projectPath);
            var gate = GateFor(projectPath);
            await gate.WaitAsync(ct);
            try
            {
                // Gli script di uno schema ritirato spariscono dal disco: qui sotto le loro triple escono dal grafo.
                outcome.Retired = await RetireOldScriptsCoreAsync(projectPath, g, ct);
                var known = await ReadScriptStatesAsync(g, ct);

                var onDisk = Directory.Exists(root)
                    ? Directory.EnumerateFiles(root, "*.ttl", SearchOption.AllDirectories)
                        .Where(f => CodeGraphPaths.IsAstScript(f) || CodeGraphPaths.IsDocScript(f))
                        .Select(Path.GetFullPath)
                        .ToList()
                    : new List<string>();
                outcome.ScriptsFolderMissing = !Directory.Exists(root);

                // Script spariti dal disco: via dal grafo PRIMA di caricare i nuovi. I nodi hanno IRI stabili (l'id del
                // simbolo): uno script rinominato, o un JSON Lines convertito in Turtle, porta gli STESSI nodi; togliere il
                // vecchio dopo aver caricato il nuovo cancellava anche le triple appena caricate (12/10/2026).
                var onDiskRel = new HashSet<string>(onDisk.Select(f => CodeGraphPaths.ToProjectRelative(projectPath, f)), StringComparer.Ordinal);
                foreach (var gone in known.Keys.Where(k => !onDiskRel.Contains(k)).ToList())
                {
                    ct.ThrowIfCancellationRequested();
                    outcome.Results.Add(await RemoveOneAsync(g, gone, ct));
                }

                // Prima gli ast (i simboli), poi i doc (che vi si agganciano). Gli script si mandano a Fuseki in lotti:
                // una richiesta = una transazione TDB2, e TDB2 è copy-on-write: 3.000 transazioni piccole hanno fatto 11 GB
                // di blocchi morti e un file al secondo (PlantUML, 10/10). In lotto: decine di file al secondo.
                var ordered = onDisk.Where(CodeGraphPaths.IsAstScript).OrderBy(f => f, StringComparer.Ordinal)
                                    .Concat(onDisk.Where(CodeGraphPaths.IsDocScript).OrderBy(f => f, StringComparer.Ordinal)).ToList();
                var batch = new List<(CodeScriptResult Result, string Update)>();
                foreach (var file in ordered)
                {
                    ct.ThrowIfCancellationRequested();
                    var rel = CodeGraphPaths.ToProjectRelative(projectPath, file);
                    known.TryGetValue(rel, out var previousHash);
                    var (result, update) = PrepareOne(g, file, rel, previousHash);
                    if (update == null) { outcome.Results.Add(result); continue; }
                    batch.Add((result, update));
                    if (batch.Count >= BatchScripts || batch.Sum(b => b.Update.Length) >= BatchChars)
                        await FlushBatchAsync(batch, outcome, ct);
                }
                await FlushBatchAsync(batch, outcome, ct);


                _logger?.LogInformation("[CodeGraph] '{Project}': {Loaded} caricati, {Skipped} invariati, {Removed} tolti, {Failed} falliti",
                    projectPath, outcome.Loaded, outcome.Skipped, outcome.Removed, outcome.Failed);
                // TDB2 è copy-on-write: dopo un carico grosso i blocchi vecchi restano su disco finché non si compatta.
                if (compact && outcome.Loaded + outcome.Removed >= CompactAfterScripts) await _fuseki.CompactAsync(CodeOntology.Dataset, ct);
                return outcome;
            }
            finally { gate.Release(); }
        }

        public async Task<(long Folder, long Total)> TripleCountsAsync(string projectPath, string folderRelative, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            var prefix = CodeGraphIris.Lit(CodeGraphPaths.ScriptsFolderName + "/" + (folderRelative ?? string.Empty).Replace('\\', '/').Trim('/') + "/");
            // lo stesso perimetro di RemoveFolderUpdate: i nodi dei suoi script con tutte le loro triple, più l'arco diretto di ogni nodo arco
            var nodes = await RowsAsync($"SELECT (COUNT(*) AS ?n) WHERE {{ GRAPH {g.DataIri} {{ ?s code:fromScript ?sc ; ?p ?o . FILTER(STRSTARTS(?sc, {prefix})) }} }}", ct);
            var edges = await RowsAsync($"SELECT (COUNT(*) AS ?n) WHERE {{ GRAPH {g.DataIri} {{ ?e a code:Edge ; code:fromScript ?sc . FILTER(STRSTARTS(?sc, {prefix})) }} }}", ct);
            // tutto il dataset: ogni progetto, il grafo di default e gli stati
            var total = await RowsAsync("SELECT (COUNT(*) AS ?n) WHERE { { ?s ?p ?o } UNION { GRAPH ?g { ?s ?p ?o } } }", ct);
            static long N(List<Dictionary<string, string>> rows) => long.TryParse(rows.FirstOrDefault() is { } r ? SparqlResults.Str(r, "n") : null, out var v) ? v : 0;
            return (N(nodes) + N(edges), N(total));
        }

        public Task CompactAsync(CancellationToken ct = default) => _fuseki.CompactAsync(CodeOntology.Dataset, ct);

        public async Task<CodeScriptLoadOutcome> ReloadFolderAsync(string projectPath, string folderRelative, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(folderRelative))
                throw new ArgumentException(UiText.Say("La ricarica da capo vuole la cartella program: folderRelative è vuoto.", "A full reload needs the program folder: folderRelative is empty."), nameof(folderRelative));
            var g = GraphsOf(projectPath);
            var prefix = CodeGraphPaths.ScriptsFolderName + "/" + folderRelative.Replace('\\', '/').Trim('/') + "/";
            var gate = GateFor(projectPath);
            var started = DateTime.UtcNow;
            await gate.WaitAsync(ct);
            try { await UpdateAsync(CodeTripleWriter.RemoveFolderUpdate(prefix, g.Data, g.State), ct); }
            finally { gate.Release(); }
            _logger?.LogInformation("[CodeGraph] '{Project}': cartella '{Folder}' tolta dal grafo in {Seconds:0.0} s, si ricarica da capo", projectPath, folderRelative, (DateTime.UtcNow - started).TotalSeconds);
            // senza stato, gli script della cartella risultano tutti da caricare: lotti e compattazione come all'apertura
            return await LoadProjectAsync(projectPath, ct);
        }

        public async Task<CodeScriptResult> LoadScriptAsync(string projectPath, string scriptAbsolutePath, CancellationToken ct = default)
        {
            // Un JSON Lines (prima del 12/10/2026) non si carica: lo converte LoadProjectAsync all'apertura, e la sua
            // cancellazione toglie le triple vecchie (RemoveScriptAsync). Qui solo il .ttl.
            if (CodeGraphPaths.IsLegacyScript(scriptAbsolutePath))
                return new CodeScriptResult { ScriptPath = CodeGraphPaths.ToProjectRelative(projectPath, scriptAbsolutePath), Skipped = true };
            var g = GraphsOf(projectPath);
            var gate = GateFor(projectPath);
            await gate.WaitAsync(ct);
            try
            {
                var rel = CodeGraphPaths.ToProjectRelative(projectPath, scriptAbsolutePath);
                (await ReadScriptStatesAsync(g, ct)).TryGetValue(rel, out var previousHash);
                return await LoadOneAsync(g, Path.GetFullPath(scriptAbsolutePath), rel, previousHash, ct);
            }
            finally { gate.Release(); }
        }

        public async Task<CodeScriptResult> RemoveScriptAsync(string projectPath, string scriptAbsolutePath, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            var gate = GateFor(projectPath);
            await gate.WaitAsync(ct);
            try { return await RemoveOneAsync(g, CodeGraphPaths.ToProjectRelative(projectPath, scriptAbsolutePath), ct); }
            finally { gate.Release(); }
        }

        /// <summary>Quanti script al massimo in una richiesta a Fuseki, e quanti caratteri di update.</summary>
        public const int BatchScripts = 200;
        /// <summary>Dopo tanti script caricati in un giro si chiede a Fuseki di compattare il dataset.</summary>
        public const int CompactAfterScripts = 100;
        public const int BatchChars = 8_000_000;

        /// <summary>Legge, controlla e traduce uno script; l'update è null se non c'è niente da mandare (invariato o rifiutato).</summary>
        private (CodeScriptResult Result, string Update) PrepareOne(ProjectGraphs g, string absolutePath, string rel, string previousHash)
        {
            var result = new CodeScriptResult { ScriptPath = rel, Kind = CodeGraphPaths.IsAstScript(rel) ? "ast" : "doc" };
            try
            {
                var content = File.ReadAllText(absolutePath, Encoding.UTF8);
                var hash = Sha256(content);
                if (string.Equals(hash, previousHash, StringComparison.OrdinalIgnoreCase))
                {
                    result.Skipped = true;
                    return (result, null);
                }

                var script = CodeScriptParser.Parse(content, rel);
                var expectedKind = CodeGraphPaths.IsAstScript(rel) ? CodeScriptKind.Ast : CodeScriptKind.Doc;
                if (script.Header.Kind != expectedKind)
                    throw new CodeScriptFormatException(rel, 1, UiText.Say($"kind '{script.Header.Kind.ToString().ToLowerInvariant()}' ma il file si chiama *{(expectedKind == CodeScriptKind.Ast ? CodeGraphPaths.AstScriptSuffix : CodeGraphPaths.DocScriptSuffix)}", $"kind '{script.Header.Kind.ToString().ToLowerInvariant()}' but the file is named *{(expectedKind == CodeScriptKind.Ast ? CodeGraphPaths.AstScriptSuffix : CodeGraphPaths.DocScriptSuffix)}"));

                // Dati e stato nella stessa richiesta: insieme, o niente.
                var update = script.Header.Kind == CodeScriptKind.Ast
                    ? CodeTripleWriter.LoadAstUpdate(script, rel, g.Data)
                    : CodeTripleWriter.LoadDocUpdate(script, rel, g.Data);
                update += ";\n" + CodeTripleWriter.ScriptStateUpsert(g.State, CodeGraphIris.Script(g.Hash, rel), rel, result.Kind, hash,
                    script.Header.Schema, script.RecordCount, DateTime.UtcNow.ToString("o"));
                result.RecordCount = script.RecordCount;
                return (result, update);
            }
            catch (CodeScriptFormatException ex)
            {
                result.Error = ex.Message;
                _logger?.LogWarning("[CodeGraph] script rifiutato: {Why}", ex.Message);
                return (result, null);
            }
            catch (Exception ex)
            {
                result.Error = $"'{rel}': {ex.Message}";
                _logger?.LogError(ex, "[CodeGraph] lettura fallita per {Script}", rel);
                return (result, null);
            }
        }

        /// <summary>Manda il lotto in una richiesta sola (una transazione); se Fuseki la rifiuta, ogni script del lotto porta l'errore.</summary>
        private async Task FlushBatchAsync(List<(CodeScriptResult Result, string Update)> batch, CodeScriptLoadOutcome outcome, CancellationToken ct)
        {
            if (batch.Count == 0) return;
            var sb = new StringBuilder(CodeOntology.SparqlPrefixes);
            foreach (var (_, update) in batch)
            {
                // Ogni update del writer porta i suoi PREFIX: in un lotto li dichiara solo la testa.
                sb.Append(update.StartsWith("PREFIX", StringComparison.Ordinal) ? update.Substring(CodeOntology.SparqlPrefixes.Length) : update).Append(";\n");
            }
            try
            {
                await UpdateAsync(sb.ToString(), ct);
                foreach (var (result, _) in batch) result.Loaded = true;
            }
            catch (FusekiUnavailableException) { throw; }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[CodeGraph] lotto di {Count} script rifiutato da Fuseki", batch.Count);
                foreach (var (result, _) in batch) result.Error = UiText.Say($"'{result.ScriptPath}': lotto rifiutato da Fuseki — {ex.Message}", $"'{result.ScriptPath}': batch rejected by Fuseki — {ex.Message}");
            }
            finally
            {
                foreach (var (result, _) in batch) outcome.Results.Add(result);
                batch.Clear();
            }
        }

        private async Task<CodeScriptResult> LoadOneAsync(ProjectGraphs g, string absolutePath, string rel, string previousHash, CancellationToken ct)
        {
            var (result, update) = PrepareOne(g, absolutePath, rel, previousHash);
            if (update == null) return result;
            try
            {
                await UpdateAsync(update, ct);
                result.Loaded = true;
                return result;
            }
            catch (FusekiUnavailableException) { throw; }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                result.Error = $"'{rel}': {ex.Message}";
                _logger?.LogError(ex, "[CodeGraph] caricamento fallito per {Script}", rel);
                return result;
            }
        }

        private async Task<CodeScriptResult> RemoveOneAsync(ProjectGraphs g, string rel, CancellationToken ct)
        {
            var result = new CodeScriptResult { ScriptPath = rel, Kind = CodeGraphPaths.IsAstScript(rel) ? "ast" : "doc" };
            try
            {
                var update = CodeTripleWriter.RemoveUpdate(rel, g.Data) +
                             CodeTripleWriter.ScriptStateRemove(g.State, CodeGraphIris.Script(g.Hash, rel));
                await UpdateAsync(update, ct);
                result.Removed = true;
                return result;
            }
            catch (FusekiUnavailableException) { throw; }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                result.Error = $"'{rel}': {ex.Message}";
                _logger?.LogError(ex, "[CodeGraph] rimozione fallita per {Script}", rel);
                return result;
            }
        }

        // ------------------------------------------------------------------ stato

        private async Task<Dictionary<string, string>> ReadScriptStatesAsync(ProjectGraphs g, CancellationToken ct)
        {
            var rows = await RowsAsync($"SELECT ?p ?h WHERE {{ GRAPH {g.StateIri} {{ ?s a code:Script ; code:scriptPath ?p ; code:contentHash ?h }} }}", ct);
            var map = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var r in rows) map[r["p"]] = r["h"];
            return map;
        }

        public async Task<CodeGraphStatus> GetStatusAsync(string projectPath, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            var baseUri = await _fuseki.EnsureRunningAsync(ct);
            var status = new CodeGraphStatus { Endpoint = baseUri + "/" + CodeOntology.Dataset, Graph = g.Data };

            var counts = (await RowsAsync(
                $"SELECT (COUNT(DISTINCT ?f) AS ?files) (COUNT(DISTINCT ?sym) AS ?symbols) (COUNT(DISTINCT ?ext) AS ?externals) " +
                $"(COUNT(DISTINCT ?p) AS ?parameters) (COUNT(DISTINCT ?e) AS ?edges) (COUNT(DISTINCT ?d) AS ?described) WHERE {{ GRAPH {g.DataIri} {{ " +
                "{ ?f a code:File } UNION { ?sym a code:Symbol ; code:kind ?k . FILTER(?k != \"external\") } UNION { ?ext a code:Symbol ; code:kind \"external\" } " +
                "UNION { ?p a code:Parameter } UNION { ?e a code:Edge } UNION { ?d a code:Description } } }", ct)).FirstOrDefault();
            if (counts != null)
            {
                status.Files = SparqlResults.Count(counts, "files");
                status.Symbols = SparqlResults.Count(counts, "symbols");
                status.ExternalSymbols = SparqlResults.Count(counts, "externals");
                status.Parameters = SparqlResults.Count(counts, "parameters");
                status.Edges = SparqlResults.Count(counts, "edges");
                status.DescribedSymbols = SparqlResults.Count(counts, "described");
            }
            var loaded = await ReadScriptStatesAsync(g, ct);
            status.ScriptsLoaded = loaded.Count;

            var root = CodeGraphPaths.GetScriptsRoot(projectPath);
            status.ScriptsFolderExists = Directory.Exists(root);
            if (!status.ScriptsFolderExists) return status;
            foreach (var f in Directory.EnumerateFiles(root, "*.ttl", SearchOption.AllDirectories)
                                       .Where(f => CodeGraphPaths.IsAstScript(f) || CodeGraphPaths.IsDocScript(f)))
            {
                status.ScriptsOnDisk++;
                var rel = CodeGraphPaths.ToProjectRelative(projectPath, f);
                if (!loaded.TryGetValue(rel, out var hash) || !string.Equals(hash, Sha256(File.ReadAllText(f)), StringComparison.OrdinalIgnoreCase))
                    status.ScriptsPending++;
            }
            return status;
        }

        public async Task<CodeScanState> GetScanStateAsync(string projectPath, string programFolder, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            var iri = CodeGraphIris.Iri(CodeGraphIris.Scan(g.Hash, programFolder));
            var row = (await RowsAsync(
                $"SELECT ?folder ?root ?commit ?mode ?at ?cand ?rep ?bridges WHERE {{ GRAPH {g.StateIri} {{ {iri} a code:ScanState ; code:programFolder ?folder ; code:mode ?mode ; code:scannedAt ?at ; code:candidates ?cand ; code:reparsed ?rep . " +
                $"OPTIONAL {{ {iri} code:repositoryRoot ?root }} OPTIONAL {{ {iri} code:analyzedCommit ?commit }} OPTIONAL {{ {iri} code:bridges ?bridges }} }} }}", ct)).FirstOrDefault();
            if (row == null) return null;
            return new CodeScanState
            {
                ProgramFolder = SparqlResults.Str(row, "folder"), RepositoryRoot = SparqlResults.Str(row, "root"), AnalyzedCommit = SparqlResults.Str(row, "commit"),
                Mode = SparqlResults.Str(row, "mode"), ScannedAt = SparqlResults.Str(row, "at"),
                Candidates = SparqlResults.Count(row, "cand"), Reparsed = SparqlResults.Count(row, "rep"), Bridges = SparqlResults.Str(row, "bridges"),
            };
        }

        public async Task UpsertScanStateAsync(string projectPath, CodeScanState state, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            await UpdateAsync(CodeTripleWriter.ScanStateUpsert(g.State, CodeGraphIris.Scan(g.Hash, state.ProgramFolder), state), ct);
        }

        public async Task<List<CodeDescriptionRun>> GetDescriptionRunsAsync(string projectPath, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            var rows = await RowsAsync(
                $"SELECT ?folder ?status ?engine ?force ?review ?qd ?started ?updated ?idx ?tot ?done ?cur ?err WHERE {{ GRAPH {g.StateIri} {{ ?r a {CodeOntology.ClassDescriptionRun} ; " +
                "code:programFolder ?folder ; code:status ?status ; code:engine ?engine ; code:force ?force ; code:startedAt ?started ; code:updatedAt ?updated ; " +
                "code:fileIndex ?idx ; code:fileTotal ?tot ; code:filesDescribed ?done . OPTIONAL { ?r code:currentFile ?cur } OPTIONAL { ?r code:error ?err } OPTIONAL { ?r code:review ?review } OPTIONAL { ?r code:questionsDone ?qd } } } ORDER BY ?folder", ct);
            return rows.Select(r => new CodeDescriptionRun
            {
                ProgramFolder = SparqlResults.Str(r, "folder"), Status = SparqlResults.Str(r, "status"), Engine = SparqlResults.Str(r, "engine"),
                Force = SparqlResults.Bool(r, "force"), Review = SparqlResults.Bool(r, "review"), StartedAt = SparqlResults.Str(r, "started"), UpdatedAt = SparqlResults.Str(r, "updated"),
                FileIndex = SparqlResults.Count(r, "idx"), FileTotal = SparqlResults.Count(r, "tot"), FilesDescribed = SparqlResults.Count(r, "done"), QuestionsDone = SparqlResults.Count(r, "qd"),
                CurrentFile = SparqlResults.Str(r, "cur"), Error = SparqlResults.Str(r, "err"),
            }).ToList();
        }

        public async Task UpsertDescriptionRunAsync(string projectPath, CodeDescriptionRun run, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            await UpdateAsync(CodeTripleWriter.DescriptionRunUpsert(g.State, CodeGraphIris.DescriptionRun(g.Hash, run.ProgramFolder), run), ct);
        }

        public async Task RemoveDescriptionRunAsync(string projectPath, string programFolder, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            await UpdateAsync(CodeTripleWriter.DescriptionRunRemove(g.State, CodeGraphIris.DescriptionRun(g.Hash, programFolder)), ct);
        }

        public async Task<int> RetireOldScriptsAsync(string projectPath, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            var gate = GateFor(projectPath);
            await gate.WaitAsync(ct);
            try { return await RetireOldScriptsCoreAsync(projectPath, g, ct); }
            finally { gate.Release(); }
        }

        /// <summary>
        /// Schema 3 (14/10/2026): il programma è nell'id di ogni simbolo (<see cref="CodeProgramScope"/>). Gli script di
        /// prima hanno gli id vecchi e non si convertono: gli AST si rifanno dal sorgente, le descrizioni si rifanno con
        /// l'LLM (deciso dall'utente: niente migrazione). Lo stato dello scanner va via con loro, sennò il commit ricordato
        /// direbbe «niente di cambiato».
        /// </summary>
        private async Task<int> RetireOldScriptsCoreAsync(string projectPath, ProjectGraphs g, CancellationToken ct)
        {
            var root = CodeGraphPaths.GetScriptsRoot(projectPath);
            if (!Directory.Exists(root)) return 0;
            var retired = 0;
            foreach (var f in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories).ToList())
            {
                // Solo più VECCHI: uno schema più nuovo l'ha scritto un MdExplorer aggiornato (arriva con Git) e il lettore
                // lo dice («Aggiorna MdExplorer»); un'intestazione che non si legge la segnala il caricamento.
                var old = CodeGraphPaths.IsLegacyScript(f)
                    || ((CodeGraphPaths.IsAstScript(f) || CodeGraphPaths.IsDocScript(f)) && CodeScriptTurtle.ReadHeaderLine(f) is { } h && h.Schema < CodeScriptParser.CurrentSchema);
                if (!old) continue;
                File.Delete(f);
                retired++;
            }
            if (retired == 0) return 0;
            await UpdateAsync(CodeTripleWriter.ScanStatesRemoveAll(g.State), ct);
            _logger.LogInformation("[CodeGraph] '{Project}': {Count} script di uno schema ritirato cancellati (schema attuale {Schema}); stato dello scanner azzerato, la prossima analisi è completa",
                projectPath, retired, CodeScriptParser.CurrentSchema);
            return retired;
        }
    }
}
