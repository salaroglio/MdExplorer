using System;
using System.Linq;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents;
using MdExplorer.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.AgentRun
{
    /// <summary>
    /// Decide se un agente è <b>indisponibile adesso</b> per una causa <i>di politica</i>
    /// (non di risorsa): <c>maintenance</c> (WIP condiviso via git in <c>.development.yml</c>)
    /// o <c>user</c> (pausa temporanea locale in UserDB). È il primo cancello della coda
    /// differita (§12.5), valutato PRIMA del tetto risorse: se torna un motivo, la richiesta è
    /// parcheggiata senza nemmeno provare ad acquisire uno slot Copilot.
    /// </summary>
    public interface IAgentAvailabilityPolicy
    {
        /// <summary>Motivo di parcheggio (<see cref="AgentMessage.DeferredReasonEnum"/>) o <c>null</c> se disponibile.</summary>
        string CheckDeferral(string projectPath, string agentName);
    }

    public class AgentAvailabilityPolicy : IAgentAvailabilityPolicy
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IProjectMetadataService _metadata;
        private readonly ILogger<AgentAvailabilityPolicy> _logger;

        public AgentAvailabilityPolicy(
            IServiceScopeFactory scopeFactory,
            IProjectMetadataService metadata,
            ILogger<AgentAvailabilityPolicy> logger)
        {
            _scopeFactory = scopeFactory;
            _metadata = metadata;
            _logger = logger;
        }

        public string CheckDeferral(string projectPath, string agentName)
        {
            if (string.IsNullOrWhiteSpace(agentName)) return null;

            // 1) Manutenzione (team-wide, via git): ha precedenza — è una condizione condivisa.
            try
            {
                var maintenance = _metadata.GetAgentCity(projectPath)?.Maintenance;
                if (maintenance != null &&
                    maintenance.Any(n => string.Equals(n?.Trim(), agentName, StringComparison.OrdinalIgnoreCase)))
                    return AgentMessage.DeferredReasonEnum.Maintenance;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Availability] lettura manutenzione fallita per '{Project}'", projectPath);
            }

            // 2) Pausa utente locale (UserDB): esiste una riga progetto+agente?
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                db.BeginTransaction();
                var paused = db.GetDal<AgentPause>().GetList().ToList()
                    .Any(p => string.Equals(p.AgentName?.Trim(), agentName, StringComparison.OrdinalIgnoreCase)
                              && AgentPathComparer.Equals(p.ProjectPath, projectPath));
                db.Commit();
                if (paused) return AgentMessage.DeferredReasonEnum.User;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Availability] lettura pausa utente fallita per '{Agent}'", agentName);
            }

            return null;
        }
    }
}
