using System;
using System.Linq;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Services.KnowledgeGraph;
using Microsoft.Extensions.DependencyInjection;

namespace MdExplorer.Services.AgentMemory
{
    /// <summary>
    /// Risolve le coordinate Fuseki di un progetto dai suoi <see cref="ProjectFusekiSettings"/>
    /// (memoria, Fase 5). Punto unico: lo usano sia il <c>MemoryController</c> (assert/query
    /// dell'agente) sia il dispatcher (recupero al risveglio, 5c). Ritorna <c>null</c> quando la
    /// memoria non è abilitata per il progetto — il chiamante decide se è un errore (controller)
    /// o un semplice "niente memoria" (dispatcher).
    /// </summary>
    public interface IFusekiConnectionResolver
    {
        /// <summary>Coordinate Fuseki del progetto, o <c>null</c> se Fuseki è disabilitato/non configurato.</summary>
        FusekiConnection Resolve(string projectPath);
    }

    public class FusekiConnectionResolver : IFusekiConnectionResolver
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IPasswordProtector _passwordProtector;

        public FusekiConnectionResolver(IServiceScopeFactory scopeFactory, IPasswordProtector passwordProtector)
        {
            _scopeFactory = scopeFactory;
            _passwordProtector = passwordProtector;
        }

        public FusekiConnection Resolve(string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath)) return null;

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();

            // Materializza prima del confronto path (AgentPathComparer non traducibile in SQL).
            db.BeginTransaction();
            var project = db.GetDal<Project>().GetList().ToList()
                .FirstOrDefault(p => AgentPathComparer.Equals(p.Path, projectPath));
            var settings = project == null ? null : db.GetDal<ProjectFusekiSettings>().GetList()
                .FirstOrDefault(s => s.Project.Id == project.Id);
            db.Commit();

            if (settings == null || !settings.Enabled || string.IsNullOrWhiteSpace(settings.Dataset))
                return null;

            var password = string.IsNullOrEmpty(settings.PasswordEncrypted)
                ? string.Empty
                : _passwordProtector.Unprotect(settings.PasswordEncrypted);

            return new FusekiConnection
            {
                BaseUri = settings.Uri,
                Dataset = settings.Dataset,
                Username = settings.Username ?? string.Empty,
                Password = password,
            };
        }
    }
}
