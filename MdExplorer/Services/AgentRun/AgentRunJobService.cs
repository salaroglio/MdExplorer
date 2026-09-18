using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Abstractions.Models.AI;
using MdExplorer.Abstractions.Services;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Execution;
using MdExplorer.Features.Services.AI;
using MdExplorer.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.AgentRun
{
    /// <summary>
    /// Singleton executor for <c>*.agent.md</c> agents. Fire-and-forget, one run per agent
    /// file; progress is streamed via the <c>agentJobProgress</c> event on MonitorMDHub
    /// (same plumbing the Mark folder job uses).
    /// </summary>
    public class AgentRunJobService : IAgentRunJobService
    {
        private const string ProgressEvent = "agentJobProgress";
        private const int OutputTailChars = 2000;

        private readonly ILogger<AgentRunJobService> _logger;
        private readonly IHubContext<MonitorMDHub> _hubContext;
        private readonly IAgentTurnRunner _turnRunner;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly AgentRegistry.IAgentRegistryService _agentRegistry;
        private readonly MdExplorer.Features.Agents.IRunTokenStore _tokens;
        private readonly IAgentWorktreeManager _worktree;
        private readonly IAgentWorktreePreference _worktreePreference;
        private readonly IAgentMergeRequestService _mergeRequests;

        private readonly ConcurrentDictionary<string, CancellationTokenSource> _running = new();

        public AgentRunJobService(
            ILogger<AgentRunJobService> logger,
            IHubContext<MonitorMDHub> hubContext,
            IAgentTurnRunner turnRunner,
            IServiceScopeFactory scopeFactory,
            AgentRegistry.IAgentRegistryService agentRegistry,
            MdExplorer.Features.Agents.IRunTokenStore tokens,
            IAgentWorktreeManager worktree,
            IAgentWorktreePreference worktreePreference,
            IAgentMergeRequestService mergeRequests)
        {
            _worktree = worktree;
            _worktreePreference = worktreePreference;
            _mergeRequests = mergeRequests;
            _logger = logger;
            _hubContext = hubContext;
            _turnRunner = turnRunner;
            _scopeFactory = scopeFactory;
            _agentRegistry = agentRegistry;
            _tokens = tokens;
        }

        /// <summary>
        /// Costruisce la rubrica dei colleghi (§6): i cittadini <b>trusted</b> del
        /// progetto, escluso l'agente stesso. Fail-soft con warning: la rubrica è
        /// contesto opzionale, un intoppo del registry non deve impedire il run.
        /// </summary>
        private IReadOnlyList<AgentRosterEntry> BuildRoster(string projectPath, string currentAgentFilePath)
        {
            try
            {
                var catalog = _agentRegistry.GetCatalog(projectPath);
                return catalog
                    .Where(e => e.IsCitizen && e.Trusted)
                    .Where(e => !PathEquals(e.AgentFilePath, currentAgentFilePath))
                    .Select(e => new AgentRosterEntry
                    {
                        Name = e.Name,
                        Role = e.Role,
                        Skills = e.Skills?
                            .Select(s => s.Id)
                            .Where(id => !string.IsNullOrWhiteSpace(id))
                            .ToList() ?? new List<string>(),
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[AgentRun] Rubrica non disponibile per {Project}: proseguo senza.", projectPath);
                return null;
            }
        }

        /// <summary>
        /// Il nome con cui firmare i commit del run (§10). Preferisce il nome a2a del
        /// cittadino (identità stabile dalla card); se l'agente non è cittadino, ripiega sul
        /// nome derivato dal file. Fail-soft: un intoppo del registry non deve impedire il run,
        /// si degrada al nome-da-file.
        /// </summary>
        /// <summary>
        /// Il default dell'isolamento per questo progetto. Fail-soft verso il <b>progetto</b>: se
        /// la preferenza non è leggibile, meglio un agente che lavora dove hai sempre visto
        /// lavorare che un run rifiutato per una lettura andata storta.
        /// </summary>
        private bool SafeIsolationDefault(string projectPath)
        {
            try { return _worktreePreference.IsEnabled(projectPath); }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[AgentRun] preferenza di isolamento non leggibile per '{Path}': lavoro nel progetto.", projectPath);
                return false;
            }
        }

        /// <summary>
        /// Consegna il lavoro di un run isolato: commit firmato dall'agente, branch pubblicato,
        /// richiesta di merge. Senza questo il lavoro resterebbe in un posto che il primo agente
        /// successivo ripulisce con <c>reset --hard</c>.
        /// <para>Fail-soft: un intoppo qui non trasforma un turno riuscito in un fallimento — ma
        /// si vede nel log, non si finge che sia andata.</para>
        /// </summary>
        private async Task PublishIsolatedWorkAsync(AgentRunRequestModel request, string agentName, CancellationToken ct)
        {
            try
            {
                var pushed = await _worktree.CommitAndPushBranchAsync(
                    request.ProjectPath, agentName, $"lavoro di {agentName}", ct);
                if (pushed == null)
                {
                    // Nessun commit = l'agente non ha toccato niente: è un esito, non un errore.
                    _logger.LogInformation("[AgentRun] '{Agent}': niente da consegnare dal posto di lavoro.", agentName);
                    return;
                }

                var changed = await _worktree.ChangedFilesAsync(request.ProjectPath, agentName, ct);
                _mergeRequests.Open(request.ProjectPath, agentName,
                    pushed.Branch, pushed.LocalBranch, pushed.HeadSha, changed);

                // La UI si accende: c'è qualcosa da decidere.
                await _hubContext.Clients.All.SendAsync("agentMergeRequested", new
                {
                    projectPath = request.ProjectPath,
                    agentName,
                    branch = pushed.Branch,
                    files = changed.Count,
                }, ct);

                _logger.LogInformation("[AgentRun] '{Agent}': lavoro consegnato su '{Branch}', {N} file.",
                    agentName, pushed.Branch, changed.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[AgentRun] consegna del lavoro isolato di '{Agent}' fallita: il lavoro è sul suo ramo, ma nessuno te l'ha chiesto di approvare.",
                    agentName);
            }
        }

        private string ResolveGitSignatureName(string projectPath, string agentFilePath)
        {
            try
            {
                var entry = _agentRegistry.GetCatalog(projectPath)
                    .FirstOrDefault(e => e.IsCitizen && PathEquals(e.AgentFilePath, agentFilePath));
                if (entry != null && !string.IsNullOrWhiteSpace(entry.Name))
                    return entry.Name;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[AgentRun] Nome a2a non risolvibile per {File}: firmo col nome-da-file.", agentFilePath);
            }
            return AgentGitIdentity.NameFromAgentFile(agentFilePath);
        }

        private static bool PathEquals(string a, string b)
        {
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
                return false;
            try
            {
                return string.Equals(
                    Path.GetFullPath(a).TrimEnd('/', '\\'),
                    Path.GetFullPath(b).TrimEnd('/', '\\'),
                    StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
            }
        }

        // NOTE: deliberately NOT async — the registry check must run synchronously so the
        // controller can catch the "already running" exception before the request returns.
        public Task RunAsync(AgentRunRequestModel request, CancellationToken ct = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.ProjectPath) || !Directory.Exists(request.ProjectPath))
                throw new ArgumentException($"Project path does not exist: '{request.ProjectPath}'");
            if (string.IsNullOrWhiteSpace(request.AgentFilePath) || !File.Exists(request.AgentFilePath))
                throw new ArgumentException($"Agent file does not exist: '{request.AgentFilePath}'");
            if (string.IsNullOrWhiteSpace(request.PreparedPrompt))
                throw new ArgumentException("Prepared prompt is empty");

            var key = RunKey(request.AgentFilePath);
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            if (!_running.TryAdd(key, cts))
            {
                cts.Dispose();
                throw new InvalidOperationException(
                    $"Agent '{Path.GetFileName(request.AgentFilePath)}' is already running.");
            }

            return RunInternalAsync(request, cts);
        }

        public void Cancel(string agentFilePath)
        {
            if (!string.IsNullOrWhiteSpace(agentFilePath)
                && _running.TryGetValue(RunKey(agentFilePath), out var cts))
            {
                try { cts.Cancel(); }
                catch (ObjectDisposedException) { /* job already finished */ }
            }
        }

        public bool IsRunning(string agentFilePath)
        {
            return !string.IsNullOrWhiteSpace(agentFilePath)
                && _running.ContainsKey(RunKey(agentFilePath));
        }

        private async Task RunInternalAsync(AgentRunRequestModel request, CancellationTokenSource cts)
        {
            var agentName = Path.GetFileName(request.AgentFilePath);
            var logId = InsertLogRow(request);
            try
            {
                _logger.LogInformation(
                    "[AgentRun] STARTED agent='{Agent}' trigger='{Trigger}' runId={RunId}",
                    agentName, request.TriggerSource, request.RunId);

                await SendAsync(request, new
                {
                    runId = request.RunId,
                    scheduleId = request.ScheduleId,
                    agentName,
                    agentFilePath = request.AgentFilePath,
                    triggerSource = request.TriggerSource,
                    phase = "started"
                });

                var unresolved = AgentPromptComposer.FindUnresolvedPlaceholders(request.PreparedPrompt);
                if (unresolved.Count > 0)
                {
                    throw new InvalidOperationException(
                        $"Prompt still contains unresolved placeholders: {string.Join(", ", unresolved)}");
                }

                var agentContent = await File.ReadAllTextAsync(request.AgentFilePath, cts.Token);
                // Rubrica (§6): i colleghi fidati del progetto, escluso sé stesso. Contesto
                // opzionale — il satellite Scheduler NON la inietta (non ha il registry).
                var roster = BuildRoster(request.ProjectPath, request.AgentFilePath);
                var composedPrompt = AgentPromptComposer.ComposeRunPrompt(agentContent, request.PreparedPrompt, roster);

                // Firma git per-agente (§10) anche per i run schedulati/manuali: l'agente che
                // scrive nel workspace committa con la propria identità, non con quella dell'umano.
                var gitName = ResolveGitSignatureName(request.ProjectPath, request.AgentFilePath);

                // Attraverso il seam provider-agnostico (IAgentTurnRunner): il runner reale
                // (CopilotTurnRunner) verifica la disponibilità e lancia Copilot; una fake lo
                // sostituisce nei test. I run schedulati/manuali non passano un RunToken.
                // Un agente lanciato a mano è un cittadino come gli altri: senza RunToken non
                // potrebbe chiamare un collega, chiedere un intervento su un ambito altrui né
                // leggere la rubrica — tutti i tool della città si autenticano con questo. Prima
                // mancava, e un agente lanciato dalla UI si trovava «RunToken assente o non
                // valido» su ogni tentativo di parlare con qualcuno.
                // gitName arriva dal registry ed E' gia' il nome a2a del cittadino (ripiega sul
                // nome-da-file solo se il catalogo non lo conosce): e' la stessa identita' con
                // cui firma i commit, quindi il token non puo' dire una cosa e la firma un'altra.
                var a2aName = gitName;
                var runToken = _tokens.Mint(new MdExplorer.Features.Agents.RunTokenClaims
                {
                    RunId = request.RunId,
                    AgentName = a2aName,
                    ProjectPath = request.ProjectPath,
                    ConversationId = null,
                });

                // DOVE lavora: la spunta del dialogo decide il singolo lancio, l'impostazione del
                // progetto fa il default. Senza isolamento l'agente scrive sul TUO ramo, nella tua
                // working tree, e il suo lavoro diventa indistinguibile dal tuo.
                var isolate = request.UseWorktree ?? SafeIsolationDefault(request.ProjectPath);
                var workDir = request.ProjectPath;
                if (isolate)
                {
                    var prep = await _worktree.PrepareForRunAsync(
                        request.ProjectPath, a2aName, request.RunId.ToString("N"), ct: cts.Token);
                    if (!prep.Success)
                    {
                        // Non ripiego sul progetto: chi ha chiesto l'isolamento non deve
                        // ritrovarsi l'agente dentro il proprio ramo perché un git è andato storto.
                        _tokens.Revoke(runToken);
                        throw new InvalidOperationException(
                            $"Posto di lavoro non preparabile per '{a2aName}': {prep.Error}");
                    }
                    workDir = prep.WorktreePath;
                }

                var env = new Dictionary<string, string>
                {
                    [MdExplorer.Features.Agents.LlmAgentWaker.EnvRunToken] = runToken,
                    [MdExplorer.Features.Agents.LlmAgentWaker.EnvAgentName] = a2aName ?? string.Empty,
                    [MdExplorer.Features.Agents.LlmAgentWaker.EnvProjectPath] = request.ProjectPath ?? string.Empty,
                };
                foreach (var kv in AgentGitIdentity.EnvFor(gitName))
                    env[kv.Key] = kv.Value;

                AgentTurnResult turn;
                try
                {
                    turn = await _turnRunner.RunTurnAsync(new AgentTurnRequest
                    {
                        ComposedPrompt = composedPrompt,
                        AgentName = a2aName,
                        ProjectPath = request.ProjectPath,
                        // Solo il cwd cambia: le claim del token e l'ambiente restano sul
                        // progetto vero, altrimenti l'agente parlerebbe a nome di un worktree.
                        WorkingDirectory = workDir,
                        Environment = env,
                    }, cts.Token);
                }
                finally
                {
                    // Il token vale un run: finito il turno non deve restare spendibile.
                    _tokens.Revoke(runToken);
                    if (isolate) _worktree.ReleaseSlot(workDir);
                }

                // Isolato: il lavoro non può restare in un posto che verrà riciclato. Si committa,
                // si pubblica e si chiede il permesso — lo stesso trattamento di un agente svegliato
                // da un messaggio, perché è la stessa cosa: lavoro di una macchina che aspetta un sì.
                if (isolate && turn.IsSuccess)
                    await PublishIsolatedWorkAsync(request, a2aName, cts.Token);

                // Un turno può concludersi male SENZA sollevare (tetto di iterazioni, uscita
                // non-zero): registrarlo come "success" mentirebbe allo storico dell'agente.
                if (!turn.IsSuccess)
                {
                    var why = turn.Diagnostic ?? $"turno concluso come {turn.Outcome}";
                    _logger.LogWarning(
                        "[AgentRun] FAILED agent='{Agent}' runId={RunId} outcome={Outcome}: {Why}",
                        agentName, request.RunId, turn.Outcome, why);
                    CompleteLogRow(logId, request, "error", Tail(turn.Text), why);
                    await SendAsync(request, new
                    {
                        runId = request.RunId,
                        scheduleId = request.ScheduleId,
                        agentName,
                        agentFilePath = request.AgentFilePath,
                        triggerSource = request.TriggerSource,
                        phase = "error",
                        error = why
                    });
                    return;
                }

                var output = turn.Text;

                _logger.LogInformation(
                    "[AgentRun] COMPLETED agent='{Agent}' runId={RunId} outputChars={Chars}",
                    agentName, request.RunId, output?.Length ?? 0);

                CompleteLogRow(logId, request, "success", Tail(output), null);
                await SendAsync(request, new
                {
                    runId = request.RunId,
                    scheduleId = request.ScheduleId,
                    agentName,
                    agentFilePath = request.AgentFilePath,
                    triggerSource = request.TriggerSource,
                    phase = "completed",
                    outputTail = Tail(output)
                });
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("[AgentRun] CANCELLED agent='{Agent}' runId={RunId}", agentName, request.RunId);
                CompleteLogRow(logId, request, "cancelled", null, null);
                await SendAsync(request, new
                {
                    runId = request.RunId,
                    scheduleId = request.ScheduleId,
                    agentName,
                    agentFilePath = request.AgentFilePath,
                    triggerSource = request.TriggerSource,
                    phase = "cancelled"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AgentRun] FAILED agent='{Agent}' runId={RunId}", agentName, request.RunId);
                CompleteLogRow(logId, request, ex is TimeoutException ? "timeout" : "error", null, ex.Message);
                await SendAsync(request, new
                {
                    runId = request.RunId,
                    scheduleId = request.ScheduleId,
                    agentName,
                    agentFilePath = request.AgentFilePath,
                    triggerSource = request.TriggerSource,
                    phase = "failed",
                    error = ex.Message
                });
            }
            finally
            {
                _running.TryRemove(RunKey(request.AgentFilePath), out _);
                cts.Dispose();
            }
        }

        /// <summary>
        /// Inserts the "running" row for this run and returns its generated Id.
        /// The Id is NOT set to RunId: the mapping uses GuidComb, and a pre-assigned Id
        /// makes NHibernate treat the entity as detached → UPDATE on a missing row →
        /// StaleObjectStateException. IUserSettingsDB is a shared NHibernate session —
        /// even in background work it must be resolved from a short-lived scope and used
        /// inside an explicit transaction (see MarkFolderJobService pattern).
        /// Log failures are non-fatal: the run itself matters more than its bookkeeping.
        /// </summary>
        private Guid? InsertLogRow(AgentRunRequestModel request)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetService<IUserSettingsDB>();
                if (db == null) return null;
                db.BeginTransaction();
                var row = new AgentExecutionLog
                {
                    ScheduleId = request.ScheduleId,
                    ProjectPath = request.ProjectPath,
                    AgentFilePath = request.AgentFilePath,
                    TriggerSource = request.TriggerSource,
                    ExecutedBy = "service",
                    StartedAt = DateTime.UtcNow,
                    Status = "running"
                };
                db.GetDal<AgentExecutionLog>().Save(row);
                db.Commit();
                return row.Id;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[AgentRun] Could not insert execution log row (non-fatal)");
                return null;
            }
        }

        private void CompleteLogRow(Guid? logId, AgentRunRequestModel request, string status, string outputSummary, string error)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetService<IUserSettingsDB>();
                if (db == null) return;

                db.BeginTransaction();
                var logDal = db.GetDal<AgentExecutionLog>();
                var row = logId.HasValue ? logDal.GetList().FirstOrDefault(l => l.Id == logId.Value) : null;
                if (row != null)
                {
                    row.FinishedAt = DateTime.UtcNow;
                    row.Status = status;
                    row.OutputSummary = outputSummary;
                    row.Error = error;
                    logDal.Save(row);
                }

                if (request.ScheduleId.HasValue)
                {
                    var scheduleDal = db.GetDal<AgentSchedule>();
                    var schedule = scheduleDal.GetList().FirstOrDefault(s => s.Id == request.ScheduleId.Value);
                    if (schedule != null)
                    {
                        schedule.LastRunAt = DateTime.UtcNow;
                        schedule.LastRunStatus = status;
                        schedule.LastRunError = error;
                        schedule.UpdatedAt = DateTime.UtcNow;
                        scheduleDal.Save(schedule);
                    }
                }
                db.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[AgentRun] Could not update execution log row (non-fatal)");
            }
        }

        private static string RunKey(string agentFilePath)
        {
            return Path.GetFullPath(agentFilePath).ToLowerInvariant();
        }

        private static string Tail(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text.Length <= OutputTailChars ? text : text.Substring(text.Length - OutputTailChars);
        }

        private async Task SendAsync(AgentRunRequestModel request, object payload)
        {
            try
            {
                // Always broadcast: scheduled/hook runs have no originating connection, and
                // for manual runs every open window should see the outcome toast. Clients
                // that care about a specific run filter by runId.
                await _hubContext.Clients.All.SendAsync(ProgressEvent, payload);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[AgentRun] SignalR send failed");
            }
        }
    }
}
