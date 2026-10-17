using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.AgentRun
{
    public interface IOverdueWorkWatcher
    {
        /// <summary>Le richieste a un altro computer senza esito da 24 ore: un avviso ciascuna, una volta. Quante ne ha avvisate.</summary>
        int CheckDispatches(DateTime now);

        /// <summary>
        /// Registra l'avviso <paramref name="key"/> del progetto e lo fa dare a <paramref name="tell"/>, se non era già
        /// stato dato. Falso se c'era già.
        /// </summary>
        bool NotifyOnce(string projectPath, string key, DateTime now, Action tell);
    }

    /// <summary>
    /// L'avviso di lavoro non processato (17/10/2026, piano omonimo): chi ha mandato lavoro a un agente di un altro computer
    /// e dopo 24 ore non ha l'esito lo sa, una volta, dalla posta. MDE non sa perché il lavoro non torna (computer spento,
    /// incarico mai avviato, agente fermo): lo dice, e da lì parte la verifica fra le persone. Le buste sul relay non scadono.
    /// I passi dei giri li controlla l'esecutore del workflow, con <see cref="NotifyOnce"/>.
    /// </summary>
    public sealed class OverdueWorkWatcher : BackgroundService, IOverdueWorkWatcher
    {
        public static readonly TimeSpan Threshold = TimeSpan.FromHours(24);
        private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(10);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IAgentMailbox _mailbox;
        private readonly ILogger<OverdueWorkWatcher> _logger;
        private readonly object _gate = new();

        public OverdueWorkWatcher(IServiceScopeFactory scopeFactory, IAgentMailbox mailbox, ILogger<OverdueWorkWatcher> logger)
        {
            _scopeFactory = scopeFactory;
            _mailbox = mailbox;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try { CheckDispatches(DateTime.UtcNow); }
                catch (Exception ex) { _logger.LogError(ex, "[Overdue] controllo delle richieste senza esito fallito"); }
                try { await Task.Delay(CheckInterval, stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }

        public int CheckDispatches(DateTime now)
        {
            var cutoff = now - Threshold;
            var told = 0;
            foreach (var d in Overdue(cutoff))
            {
                var who = d.Email ?? d.Dispatch.TargetAgent;
                var when = d.Dispatch.CreatedAt;
                if (NotifyOnce(d.Dispatch.ProjectPath, "dispatch:" + d.Dispatch.RequestId, now, () => _mailbox.Enqueue(new EnqueueRequest
                {
                    ProjectPath = d.Dispatch.ProjectPath,
                    FromAgent = d.Dispatch.OriginAgent,
                    ToAgent = ConversationHopGuard.UserRecipient,
                    Body = UiText.Say(
                        $"[ATTENZIONE] Il lavoro che «{d.Dispatch.OriginAgent}» ha mandato a «{d.Dispatch.TargetAgent}» di {who} il {when:dd/MM/yyyy HH:mm} (UTC) " +
                        $"non ha ancora un esito, dopo più di 24 ore. MdExplorer non sa perché: computer spento, incarico mai avviato o agente fermo. " +
                        $"Verifica con {who}. Quando hai chiarito, archivia questo messaggio.",
                        $"[ATTENTION] The work «{d.Dispatch.OriginAgent}» sent to «{d.Dispatch.TargetAgent}» of {who} on {when:yyyy-MM-dd HH:mm} (UTC) " +
                        $"still has no outcome, after more than 24 hours. MdExplorer does not know why: computer off, assignment never started or agent stuck. " +
                        $"Check with {who}. When it is clear, archive this message."),
                    ContextId = d.Dispatch.ConversationId.ToString(),
                })))
                {
                    told++;
                    _logger.LogWarning("[Overdue] richiesta {Req} a '{Agent}' ({Who}) senza esito dal {When:u}: avvisata la persona.",
                        d.Dispatch.RequestId, d.Dispatch.TargetAgent, who, when);
                }
            }
            return told;
        }

        public bool NotifyOnce(string projectPath, string key, DateTime now, Action tell)
        {
            if (string.IsNullOrWhiteSpace(projectPath)) throw new ArgumentException("projectPath obbligatorio", nameof(projectPath));
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("key obbligatoria", nameof(key));
            if (tell == null) throw new ArgumentNullException(nameof(tell));
            lock (_gate)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                db.BeginTransaction();
                try
                {
                    var dal = db.GetDal<OverdueWorkNotice>();
                    var already = dal.GetList().Where(n => n.Key == key).ToList()
                        .Any(n => AgentPathComparer.Equals(n.ProjectPath, projectPath));
                    if (already) { db.Commit(); return false; }
                    dal.Save(new OverdueWorkNotice { ProjectPath = projectPath, Key = key, NotifiedAt = now });
                    db.Commit();
                }
                catch
                {
                    db.Rollback();
                    throw;
                }
            }
            // Fuori dalla transazione: la posta apre la sua.
            tell();
            return true;
        }

        private sealed record OverdueDispatch(FederationDispatch Dispatch, string Email);

        private System.Collections.Generic.List<OverdueDispatch> Overdue(DateTime cutoff)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.BeginTransaction();
            var pending = db.GetDal<FederationDispatch>().GetList()
                .Where(d => d.Status == FederationDispatch.StatusEnum.Pending && d.CreatedAt <= cutoff)
                .ToList();
            var convIds = pending.Select(d => d.ConversationId).Distinct().ToList();
            var emails = db.GetDal<AgentConversation>().GetList()
                .Where(c => convIds.Contains(c.Id))
                .ToList()
                .ToDictionary(c => c.Id, c => string.IsNullOrWhiteSpace(c.RemoteOwner) ? null : c.RemoteOwner);
            db.Commit();
            return pending.Select(d => new OverdueDispatch(d, emails.TryGetValue(d.ConversationId, out var e) ? e : null)).ToList();
        }
    }
}
