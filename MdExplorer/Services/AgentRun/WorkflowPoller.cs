using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Agents.Workflow;
using MdExplorer.Services.Federation;
using MdExplorer.Features.Federation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.AgentRun
{
    /// <summary>
    /// Più computer (S5): ognuno scarica il registro dei giri e fa ciò che tocca ai suoi passi. Lo fa ogni
    /// <c>Workflow:PollSeconds</c> (60 di base, 0 = mai) e subito quando suona il campanello di un collega. Se niente è
    /// cambiato (registro, responsabilità, workflow) non rifà i conti.
    /// </summary>
    public interface IWorkflowPoller
    {
        /// <summary>Scarica il registro del progetto e, se qualcosa è cambiato (o <paramref name="force"/>), riprende i passi.</summary>
        Task<bool> PollAsync(string projectPath, bool force = false);

        /// <summary>Il campanello: guarda adesso, senza aspettare il prossimo giro. Più campanelli insieme valgono uno.</summary>
        void Ring(string projectPath);
    }

    public sealed class WorkflowPoller : BackgroundService, IWorkflowPoller
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IRoundStore _store;
        private readonly IAgentWorkflowExecutor _executor;
        private readonly IProjectMetadataService _metadata;
        private readonly ILogger<WorkflowPoller> _logger;
        private readonly TimeSpan _interval;

        private readonly ConcurrentDictionary<string, string> _seen = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _gates = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, byte> _rungAgain = new(StringComparer.OrdinalIgnoreCase);

        public WorkflowPoller(IServiceScopeFactory scopeFactory, IRoundStore store, IAgentWorkflowExecutor executor,
            IProjectMetadataService metadata, IConfiguration configuration, ILogger<WorkflowPoller> logger)
        {
            _scopeFactory = scopeFactory;
            _store = store;
            _executor = executor;
            _metadata = metadata;
            _logger = logger;
            var seconds = configuration.GetValue("Workflow:PollSeconds", 60);
            _interval = seconds > 0 ? TimeSpan.FromSeconds(seconds) : TimeSpan.Zero;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_interval == TimeSpan.Zero) return;
            try { await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken); } catch (OperationCanceledException) { return; }
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var path in Projects())
                {
                    if (stoppingToken.IsCancellationRequested) break;
                    try { await PollAsync(path); }
                    catch (Exception ex) { _logger.LogError(ex, "[Giri] il giro di controllo di {Project} è fallito", path); }
                }
                try { await Task.Delay(_interval, stoppingToken); } catch (OperationCanceledException) { return; }
            }
        }

        public void Ring(string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath)) return;
            _ = Task.Run(async () =>
            {
                try { await PollAsync(projectPath, force: true); }
                catch (Exception ex) { _logger.LogError(ex, "[Giri] il campanello per {Project} non ha fatto il controllo", projectPath); }
            });
        }

        public async Task<bool> PollAsync(string projectPath, bool force = false)
        {
            if (!_executor.IsActive(projectPath) || !_store.IsAvailable(projectPath)) return false;
            var gate = _gates.GetOrAdd(projectPath, _ => new SemaphoreSlim(1, 1));
            if (!await gate.WaitAsync(0))
            {
                // Un controllo è già in corso: lo si rifà appena finisce, così la novità arrivata adesso non si perde.
                _rungAgain[projectPath] = 0;
                return false;
            }
            try
            {
                var reconciled = false;
                do
                {
                    _rungAgain.TryRemove(projectPath, out _);
                    _store.Refresh(projectPath);
                    var key = Fingerprint(projectPath);
                    if (force || !_seen.TryGetValue(projectPath, out var last) || last != key)
                    {
                        _executor.Reconcile(projectPath, refresh: false);
                        // Dopo, non prima: la ripresa scrive essa stessa nel registro («da avviare», «partito»).
                        _seen[projectPath] = Fingerprint(projectPath);
                        reconciled = true;
                    }
                    force = false;
                } while (_rungAgain.ContainsKey(projectPath));
                return reconciled;
            }
            finally { gate.Release(); }
        }

        /// <summary>Ciò che, se cambia, può cambiare quali passi sono miei: il registro, il documento delle responsabilità, il workflow.</summary>
        private string Fingerprint(string projectPath)
        {
            var head = GitCli.Run(_store.Root(projectPath), "rev-parse", "HEAD").Out.Trim();
            var city = _metadata.GetAgentCity(projectPath);
            var sb = new StringBuilder(head);
            foreach (var relative in new[] { city?.OwnershipDoc, city?.WorkflowDoc, WorkflowDocument.JsonPathOf(projectPath, city?.WorkflowDoc) })
            {
                if (string.IsNullOrWhiteSpace(relative)) continue;
                var path = Path.Combine(projectPath, relative.Replace('/', Path.DirectorySeparatorChar));
                sb.Append('|').Append(File.Exists(path) ? Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))) : "-");
            }
            return sb.ToString();
        }

        /// <summary>I progetti con la città accesa e un workflow: gli unici dove c'è qualcosa da scaricare.</summary>
        private IEnumerable<string> Projects()
        {
            List<string> paths;
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                db.BeginTransaction();
                paths = db.GetDal<Project>().GetList().Select(p => p.Path).ToList();
                db.Commit();
            }
            return paths.Where(p => !string.IsNullOrWhiteSpace(p) && Directory.Exists(p))
                        .Where(p => _metadata.GetAgentCity(p) is { Enabled: true } c && !string.IsNullOrWhiteSpace(c.WorkflowDoc));
        }
    }

    /// <summary>
    /// Il campanello (W16): dopo una pubblicazione del registro avvisa gli altri computer del progetto, attraverso la stanza
    /// federata, che ci sono novità. Senza stanza (federazione non configurata) non suona: resta il controllo periodico.
    /// Più pubblicazioni ravvicinate (tre «da avviare» insieme) fanno un campanello solo.
    /// </summary>
    public interface IWorkflowBell
    {
        void Ring(string projectPath, string roundId);
    }

    public sealed class WorkflowBell : IWorkflowBell
    {
        private static readonly TimeSpan Coalesce = TimeSpan.FromMilliseconds(1500);
        private readonly IFederationSender _sender;
        private readonly IProjectOwnershipService _ownership;
        private readonly IEffectiveOwnerIdentity _identity;
        private readonly IProjectMetadataService _metadata;
        private readonly ILogger<WorkflowBell> _logger;
        private readonly ConcurrentDictionary<string, string> _pending = new(StringComparer.OrdinalIgnoreCase);

        public WorkflowBell(IFederationSender sender, IProjectOwnershipService ownership, IEffectiveOwnerIdentity identity,
            IProjectMetadataService metadata, ILogger<WorkflowBell> logger)
        {
            _sender = sender;
            _ownership = ownership;
            _identity = identity;
            _metadata = metadata;
            _logger = logger;
        }

        public void Ring(string projectPath, string roundId)
        {
            if (string.IsNullOrWhiteSpace(_metadata.GetAgentCity(projectPath)?.RoomSecret)) return;   // niente stanza: c'è il controllo periodico
            if (!_pending.TryAdd(projectPath, roundId)) { _pending[projectPath] = roundId; return; }
            _ = Task.Run(async () =>
            {
                await Task.Delay(Coalesce);
                _pending.TryRemove(projectPath, out var round);
                try { await SendAsync(projectPath, round); }
                catch (Exception ex) { _logger.LogWarning(ex, "[Giri] campanello per {Project} non suonato: gli altri lo vedranno al prossimo controllo", projectPath); }
            });
        }

        private async Task SendAsync(string projectPath, string roundId)
        {
            var me = _identity.ResolveEmail(projectPath);
            var colleagues = (_ownership.GetActiveOwnership(projectPath) ?? Array.Empty<OwnershipEntry>())
                .Select(e => e.GitEmail?.Trim().ToLowerInvariant())
                .Where(e => !string.IsNullOrWhiteSpace(e) && !string.Equals(e, me, StringComparison.OrdinalIgnoreCase))
                .Distinct().ToList();
            foreach (var email in colleagues)
            {
                var ok = await _sender.SendWorkflowNewsAsync(projectPath, FederationRoom.ComputeUserId(email),
                    new WorkflowNewsPayload { Round = roundId, FromOwner = me });
                if (!ok) _logger.LogInformation("[Giri] campanello per {Email} non consegnato: lo vedrà al prossimo controllo", email);
            }
        }
    }
}
