using System;
using System.Linq;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Federation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.Federation
{
    /// <summary>L'identità-padrone effettiva di un progetto: email + ownerId, ed è impersonata?</summary>
    public sealed record OwnerIdentity(string Email, string OwnerId, bool Impersonated);

    /// <summary>
    /// Il <b>seam dell'identità-padrone</b> (test della città degli agenti). "Chi sei come padrone"
    /// nasce da un solo valore: la git email → ownerId (<see cref="FederationRoom.ComputeUserId"/>).
    /// Questo servizio è l'UNICO punto che lo risolve: default = git email reale del repo; override
    /// = utente impersonato (riga <see cref="ImpersonatedOwner"/>), ma SOLO se la modalità test
    /// identità è abilitata (fail-safe: in produzione l'override non ha effetto). Loopback + R12.
    /// </summary>
    public interface IEffectiveOwnerIdentity
    {
        /// <summary>Identità effettiva (impersonata se attiva, altrimenti reale).</summary>
        OwnerIdentity Resolve(string projectPath);

        /// <summary>Email effettiva (comodità per i call-site che vogliono solo l'email).</summary>
        string ResolveEmail(string projectPath);

        bool IsTestModeEnabled();
        void SetTestMode(bool enabled);
        void SetImpersonation(string projectPath, string email);
        void ClearImpersonation(string projectPath);
    }

    public class EffectiveOwnerIdentity : IEffectiveOwnerIdentity
    {
        public const string TestModeSetting = "ImpersonationTestMode";

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EffectiveOwnerIdentity> _logger;

        public EffectiveOwnerIdentity(IServiceScopeFactory scopeFactory, ILogger<EffectiveOwnerIdentity> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public OwnerIdentity Resolve(string projectPath)
        {
            // Override impersonato, ma solo se la modalità test è abilitata (fail-safe).
            if (IsTestModeEnabled())
            {
                var imp = ReadImpersonatedEmail(projectPath);
                if (!string.IsNullOrWhiteSpace(imp))
                    return new OwnerIdentity(imp, FederationRoom.ComputeUserId(imp), true);
            }

            var real = ReadGitEmail(projectPath);
            var ownerId = string.IsNullOrWhiteSpace(real) ? null : FederationRoom.ComputeUserId(real);
            return new OwnerIdentity(real, ownerId, false);
        }

        public string ResolveEmail(string projectPath) => Resolve(projectPath).Email;

        public bool IsTestModeEnabled()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                db.BeginTransaction();
                var s = db.GetDal<Setting>().GetList().FirstOrDefault(x => x.Name == TestModeSetting);
                db.Commit();
                return s?.ValueInt == 1;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Identity] lettura modalità test fallita");
                return false;
            }
        }

        public void SetTestMode(bool enabled)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.BeginTransaction();
            var dal = db.GetDal<Setting>();
            var s = dal.GetList().FirstOrDefault(x => x.Name == TestModeSetting) ?? new Setting { Name = TestModeSetting };
            s.ValueInt = enabled ? 1 : 0;
            dal.Save(s);
            db.Commit();
            _logger.LogInformation("[Identity] modalità test identità = {Enabled}", enabled);
        }

        public void SetImpersonation(string projectPath, string email)
        {
            var e = (email ?? string.Empty).Trim();
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.BeginTransaction();
            var dal = db.GetDal<ImpersonatedOwner>();
            // AgentPathComparer non è traducibile in SQL → materializza prima del confronto path.
            var existing = dal.GetList().ToList()
                .FirstOrDefault(x => AgentPathComparer.Equals(x.ProjectPath, projectPath));
            if (existing == null)
                existing = new ImpersonatedOwner { ProjectPath = projectPath, CreatedAt = DateTime.UtcNow };
            existing.Email = e;
            existing.CreatedAt = DateTime.UtcNow;
            dal.Save(existing);
            db.Commit();
            _logger.LogInformation("[Identity] progetto '{Project}' ora agisce come '{Email}'", projectPath, e);
        }

        public void ClearImpersonation(string projectPath)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.BeginTransaction();
            var dal = db.GetDal<ImpersonatedOwner>();
            var rows = dal.GetList().ToList()
                .Where(x => AgentPathComparer.Equals(x.ProjectPath, projectPath)).ToList();
            foreach (var r in rows) dal.Delete(r);
            db.Commit();
            if (rows.Count > 0)
                _logger.LogInformation("[Identity] progetto '{Project}' torna all'identità reale", projectPath);
        }

        private string ReadImpersonatedEmail(string projectPath)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                db.BeginTransaction();
                var row = db.GetDal<ImpersonatedOwner>().GetList().ToList()
                    .FirstOrDefault(x => AgentPathComparer.Equals(x.ProjectPath, projectPath));
                db.Commit();
                return row?.Email;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Identity] lettura impersonazione fallita per '{Project}'", projectPath);
                return null;
            }
        }

        // Lettura CENTRALIZZATA della git email reale del repo (prima duplicata in
        // A2AMessagingController.ResolveLocalGitEmail e FederationRelayService.ResolveGit).
        private string ReadGitEmail(string projectPath)
        {
            try
            {
                using var repo = new LibGit2Sharp.Repository(projectPath);
                return repo.Config.Get<string>("user.email")?.Value;
            }
            catch { return null; }
        }
    }
}
