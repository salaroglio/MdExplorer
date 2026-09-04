using System;
using System.Linq;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.AgentRun
{
    /// <summary>Esito della chiusura di una sessione d'intervento.</summary>
    public sealed class InterventionCloseResult
    {
        public bool Closed { get; init; }
        /// <summary>Messaggi rimessi in coda perché l'umano ha annullato (0 se ha concluso).</summary>
        public int Requeued { get; init; }
        public string Message { get; init; }
    }

    /// <summary>
    /// La <b>sessione d'intervento manuale</b> sul worktree di un agente: si apre entrandoci, e
    /// ha una fine obbligata.
    /// <para>
    /// Non è un divieto ma una sessione, e la differenza conta. Un divieto è uno stato che
    /// qualcuno deve ricordarsi di togliere — e che quindi resta lì dimenticato. Una sessione è
    /// un lavoro che qualcuno deve <b>chiudere</b>, e finché è aperta l'attivazione dell'agente
    /// resta <b>in coda</b>: i suoi messaggi sono parcheggiati (<c>deferred:user</c>), non
    /// falliscono, e ripartono da dove sono quando esci.
    /// </para>
    /// <para>
    /// Le due uscite non sono simmetriche per caso:
    /// <list type="bullet">
    /// <item><b>Concludo</b> — le modifiche restano, la sessione si chiude, l'agente riprende.</item>
    /// <item><b>Annullo</b> — le modifiche si buttano <b>e la richiesta torna in coda</b>, così
    /// l'agente la rifà. È il punto: oggi un merge rifiutato lascia il ramo in un limbo che
    /// nessuno riprende; qui rifiutare significa rimettere la palla all'agente.</item>
    /// </list>
    /// </para>
    /// <para>
    /// ⚠️ Limite dichiarato: «una volta dentro non puoi lasciare a metà» non è imponibile —
    /// nessuno può impedire di chiudere MDE. Si garantisce che lo <b>stato resti aperto</b>:
    /// l'agente resta in coda e alla riapertura la sessione è lì che aspetta.
    /// </para>
    /// </summary>
    public interface IAgentWorktreeHoldService
    {
        /// <summary>true se c'è una sessione d'intervento aperta sul worktree di questo agente.</summary>
        bool IsHeld(string projectPath, string agentName);

        /// <summary>Motivo dell'intervento in corso, o null se non ce n'è uno.</summary>
        string ReasonFor(string projectPath, string agentName);

        /// <summary>Apre la sessione: l'agente va in coda. Idempotente.</summary>
        void Open(string projectPath, string agentName, string reason);

        /// <summary>
        /// Chiude la sessione. <paramref name="discardWork"/> = ho annullato: i messaggi conclusi
        /// da quell'agente su questo progetto tornano <c>pending</c> perché li rifaccia.
        /// </summary>
        InterventionCloseResult Close(string projectPath, string agentName, bool discardWork);
    }

    public class AgentWorktreeHoldService : IAgentWorktreeHoldService
    {
        /// <summary>
        /// Quanto indietro si guarda per rimettere in coda il lavoro annullato. Non si riapre
        /// tutta la storia dell'agente: solo ciò che ha prodotto il branch che l'umano stava
        /// revisionando.
        /// </summary>
        private static readonly TimeSpan RequeueWindow = TimeSpan.FromDays(7);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AgentWorktreeHoldService> _logger;

        public AgentWorktreeHoldService(IServiceScopeFactory scopeFactory, ILogger<AgentWorktreeHoldService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public bool IsHeld(string projectPath, string agentName) => Find(projectPath, agentName) != null;

        public string ReasonFor(string projectPath, string agentName) => Find(projectPath, agentName)?.Reason;

        public void Open(string projectPath, string agentName, string reason)
        {
            Require(projectPath, agentName);

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.Clear();
            db.BeginTransaction();
            try
            {
                if (FindIn(db, projectPath, agentName) == null)
                {
                    db.GetDal<AgentWorktreeHold>().Save(new AgentWorktreeHold
                    {
                        ProjectPath = projectPath,
                        AgentName = agentName,
                        CreatedAt = DateTime.UtcNow,
                        Reason = string.IsNullOrWhiteSpace(reason) ? "intervento manuale" : reason.Trim(),
                    });
                }

                // L'attivazione va in CODA: la pausa esistente fa già esattamente questo —
                // deferred:user, i messaggi restano parcheggiati e non consumano tentativi.
                var pauses = db.GetDal<AgentPause>();
                if (!pauses.GetList().ToList().Any(p => SameAgent(p.ProjectPath, p.AgentName, projectPath, agentName)))
                {
                    pauses.Save(new AgentPause
                    {
                        ProjectPath = projectPath,
                        AgentName = agentName,
                        CreatedAt = DateTime.UtcNow,
                    });
                }

                db.Commit();
                _logger.LogInformation(
                    "[Worktree] sessione d'intervento aperta su '{Agent}' ({Project}): attivazione in coda.",
                    agentName, projectPath);
            }
            catch
            {
                db.Rollback();
                throw;
            }
        }

        public InterventionCloseResult Close(string projectPath, string agentName, bool discardWork)
        {
            Require(projectPath, agentName);

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.Clear();
            db.BeginTransaction();
            try
            {
                var hold = FindIn(db, projectPath, agentName);
                if (hold == null)
                {
                    db.Commit();
                    return new InterventionCloseResult
                    {
                        Closed = false,
                        Message = $"Nessuna sessione d'intervento aperta su '{agentName}'.",
                    };
                }

                var requeued = 0;
                if (discardWork)
                {
                    // Annullare non è buttare: il lavoro torna all'agente. Senza questo, un
                    // rifiuto lascerebbe la richiesta in un limbo che nessuno riprende.
                    var since = DateTime.UtcNow - RequeueWindow;
                    var dal = db.GetDal<AgentMessage>();
                    var done = dal.GetList().ToList()
                        .Where(m => m.State == AgentMessage.StateEnum.Processed
                                    && string.Equals(m.ToAgent, agentName, StringComparison.OrdinalIgnoreCase)
                                    && AgentPathComparer.Equals(m.ProjectPath, projectPath)
                                    && m.ProcessedAt != null && m.ProcessedAt >= since)
                        .ToList();

                    foreach (var m in done)
                    {
                        m.State = AgentMessage.StateEnum.Pending;
                        m.ProcessedAt = null;
                        m.NextAttemptAt = null;
                        // Tentativi azzerati: non è un ritentativo dopo un errore, è lo stesso
                        // lavoro richiesto di nuovo. Farglielo pagare come fallimento sarebbe
                        // ingiusto e lo porterebbe a esaurire il budget.
                        m.Attempts = 0;
                        m.Error = null;
                        m.ForcedAt = null;
                        dal.Save(m);
                        requeued++;
                    }
                }

                db.GetDal<AgentWorktreeHold>().Delete(hold);

                var pauses = db.GetDal<AgentPause>();
                foreach (var p in pauses.GetList().ToList()
                             .Where(p => SameAgent(p.ProjectPath, p.AgentName, projectPath, agentName)))
                {
                    pauses.Delete(p);
                }

                db.Commit();

                _logger.LogInformation(
                    "[Worktree] sessione su '{Agent}' chiusa ({Mode}); messaggi rimessi in coda: {N}.",
                    agentName, discardWork ? "annullata" : "conclusa", requeued);

                return new InterventionCloseResult
                {
                    Closed = true,
                    Requeued = requeued,
                    Message = discardWork
                        ? $"Intervento annullato: {requeued} richiesta/e rimessa/e in coda per '{agentName}'."
                        : $"Intervento concluso: '{agentName}' riprende da dove era.",
                };
            }
            catch
            {
                db.Rollback();
                throw;
            }
        }

        private static bool SameAgent(string pathA, string agentA, string pathB, string agentB)
            => AgentPathComparer.Equals(pathA, pathB)
               && string.Equals(agentA, agentB, StringComparison.OrdinalIgnoreCase);

        private static void Require(string projectPath, string agentName)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                throw new ArgumentException("projectPath è obbligatorio", nameof(projectPath));
            if (string.IsNullOrWhiteSpace(agentName))
                throw new ArgumentException("agentName è obbligatorio", nameof(agentName));
        }

        private AgentWorktreeHold Find(string projectPath, string agentName)
        {
            if (string.IsNullOrWhiteSpace(projectPath) || string.IsNullOrWhiteSpace(agentName)) return null;
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                db.Clear();
                db.BeginTransaction();
                var hold = FindIn(db, projectPath, agentName);
                db.Commit();
                return hold;
            }
            catch (Exception ex)
            {
                // In dubbio si dichiara APERTA: se non sappiamo se un umano ci sta lavorando,
                // non è il momento di fare reset --hard.
                _logger.LogWarning(ex, "[Worktree] lettura della sessione fallita per '{Agent}': la tratto come aperta", agentName);
                return new AgentWorktreeHold
                {
                    ProjectPath = projectPath,
                    AgentName = agentName,
                    Reason = "stato della sessione non leggibile",
                };
            }
        }

        /// <summary>
        /// <see cref="AgentPathComparer"/> non è traducibile in SQL da NHibernate: si materializza
        /// prima e si confronta in memoria (gotcha ricorrente in questo progetto).
        /// </summary>
        private static AgentWorktreeHold FindIn(IUserSettingsDB db, string projectPath, string agentName)
            => db.GetDal<AgentWorktreeHold>().GetList().ToList()
                 .FirstOrDefault(h => SameAgent(h.ProjectPath, h.AgentName, projectPath, agentName));
    }
}
