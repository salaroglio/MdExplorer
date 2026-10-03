using System;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Abstractions.DB;
using MdExplorer.Hubs;
using MdExplorer.Service.Controllers;
using MdExplorer.Service.Models;
using MdExplorer.Services.DatabaseManager;
using MdExplorer.Services.FileSystemWatcherManager;
using MdExplorer.Services.Git;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MdExplorer.Controllers.A2A
{
    /// <summary>
    /// Le azioni per riga dei pannelli git della toolbar: una per repository (il progetto o uno dei
    /// suoi submodule).
    /// <para>
    /// Un rifiuto <b>non è un errore del server</b>: è una condizione che l'utente può risolvere, e
    /// il motivo dice già come. Per questo torna 422 con lo stesso corpo di un successo.
    /// </para>
    /// </summary>
    [ApiController]
    [Route("api/RepoSync")]
    public class RepoSyncController : MdControllerBase<RepoSyncController>
    {
        private readonly IRepoSyncService _sync;

        public RepoSyncController(
            IRepoSyncService sync,
            ILogger<RepoSyncController> logger,
            IOptions<MdExplorerAppSettings> options,
            IHubContext<MonitorMDHub> hubContext,
            IUserSettingsDB userSettingsDB,
            IEngineDB engineDB,
            IDatabaseManager databaseManager = null,
            IFileSystemWatcherManager fileSystemWatcherManager = null)
            : base(logger, options, hubContext, userSettingsDB, engineDB,
                  databaseManager: databaseManager,
                  fileSystemWatcherManager: fileSystemWatcherManager)
        {
            _sync = sync;
        }

        /// <summary>DTO nullable di proposito: un <c>string</c> non-nullable sarebbe implicitamente obbligatorio → 400 su null.</summary>
        public sealed class RepoRequest
        {
            public string? ProjectPath { get; set; }
            /// <summary>Il posto di lavoro di un agente, in revisione. Vale solo per «pubblica».</summary>
            public string? Agent { get; set; }
            /// <summary>Vuoto = la radice del progetto; altrimenti il percorso del submodule.</summary>
            public string? Repo { get; set; }
            public string? ConnectionId { get; set; }
        }

        /// <summary>Chiede a ogni remoto cosa c'è di nuovo: solo dopo i submodule sanno di essere indietro.</summary>
        [HttpPost("fetch-all")]
        public async Task<IActionResult> FetchAll([FromBody] RepoRequest body)
        {
            if (string.IsNullOrWhiteSpace(body?.ProjectPath))
                return BadRequest(new { error = "Nessun progetto indicato." });
            return Ok(await _sync.FetchAllAsync(body.ProjectPath));
        }

        [HttpPost("push")]
        public async Task<IActionResult> Push([FromBody] RepoRequest body)
        {
            if (string.IsNullOrWhiteSpace(body?.ProjectPath))
                return BadRequest(new { error = "Nessun progetto indicato." });
            return Reply(await _sync.PushAsync(body.ProjectPath, EmptyToNull(body.Agent), body.Repo), "pubblica", body.Repo);
        }

        /// <summary>Sulla radice: scarica il progetto e allinea i submodule. Su un submodule: «Aggiorna all'ultima».</summary>
        [HttpPost("pull")]
        public Task<IActionResult> Pull([FromBody] RepoRequest body)
            => OnDiskAsync(body, "scarica", () => _sync.PullAsync(body.ProjectPath, body.Repo));

        /// <summary>Porta un submodule (o tutti, senza <c>repo</c>) alla versione che il progetto registra, solo in avanti.</summary>
        [HttpPost("align")]
        public Task<IActionResult> Align([FromBody] RepoRequest body)
            => OnDiskAsync(body, "allinea", () => _sync.AlignAsync(body.ProjectPath, body.Repo));

        [HttpPost("pull-all")]
        public Task<IActionResult> PullAll([FromBody] RepoRequest body)
            => OnDiskAsync(body, "scarica tutto", () => _sync.PullAllAsync(body.ProjectPath));

        /// <summary>Annulla un'unione rimasta a metà: si torna a prima dello scaricamento.</summary>
        [HttpPost("abort-merge")]
        public Task<IActionResult> AbortMerge([FromBody] RepoRequest body)
            => OnDiskAsync(body, "annulla unione", () => _sync.AbortMergeAsync(body.ProjectPath, body.Repo));

        /// <summary>
        /// Un'azione che riscrive file sul disco: il watcher resta spento per tutta la durata —
        /// stesso contratto di pull e checkout — e alla fine il client viene avvisato di cosa è
        /// cambiato, così ricarica l'albero e il documento aperto.
        /// </summary>
        private async Task<IActionResult> OnDiskAsync(RepoRequest body, string what, Func<Task<RepoActionResult>> action)
        {
            if (string.IsNullOrWhiteSpace(body?.ProjectPath))
                return BadRequest(new { error = "Nessun progetto indicato." });

            RepoActionResult result;
            SetFileSystemWatcherEnabled(false, body.ConnectionId);
            try { result = await action(); }
            finally { SetFileSystemWatcherEnabled(true, body.ConnectionId); }

            if (result.ContentChanged)
            {
                if (!string.IsNullOrEmpty(body.ConnectionId))
                {
                    await _hubContext.Clients.Client(body.ConnectionId).SendAsync("gitPullRefreshed", new
                    {
                        fileCount = result.ChangedFiles.Count,
                        changedFiles = result.ChangedFiles.Select(p => p.Replace('\\', '/')).ToList(),
                        message = result.Message,
                    });
                }
                else
                {
                    _logger.LogError("[Flusso] '{What}' ha cambiato file ma la richiesta non porta un ConnectionId: " +
                                     "l'albero del client non verrà aggiornato.", what);
                }
            }

            return Reply(result, what, body.Repo);
        }

        private IActionResult Reply(RepoActionResult result, string what, string? repo)
        {
            if (result.Refused != null)
            {
                _logger.LogInformation("[Flusso] '{What}' su '{Repo}' rifiutato: {Why}", what, repo ?? ".", result.Refused);
                return UnprocessableEntity(result);
            }

            _logger.LogInformation("[Flusso] '{What}' su '{Repo}': {Esito}. {Messaggio}",
                what, repo ?? ".", result.Success ? "riuscito" : "non riuscito", result.Message);
            return result.Success ? Ok(result) : StatusCode(502, result);
        }

        private static string? EmptyToNull(string? s) => string.IsNullOrWhiteSpace(s) ? null : s;
    }
}
