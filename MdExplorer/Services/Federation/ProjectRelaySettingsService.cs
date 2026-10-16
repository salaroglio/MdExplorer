using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents;
using MdExplorer.Services.TeamChat;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.Federation
{
    /// <summary>Da dove arriva un valore risolto: serve alla UI per dire all'utente cosa sta usando.</summary>
    public enum RelaySettingSource
    {
        /// <summary>Nessun valore: la federazione resta dormiente.</summary>
        None,
        /// <summary>Impostazione di questo progetto (UserDB) — vince su tutto.</summary>
        Project,
        /// <summary>Blocco <c>agentCity.relayUrl</c> del <c>.development.yml</c> (condiviso col team via git).</summary>
        DevelopmentYml,
        /// <summary>Configurazione globale dell'applicazione (<c>MdChat:*</c>).</summary>
        Global,
    }

    /// <summary>Vista per la UI: mai la chiave in chiaro, solo se c'è e da dove viene.</summary>
    public sealed class RelaySettingsView
    {
        public string RelayUrl { get; init; }
        public RelaySettingSource RelayUrlSource { get; init; }
        public bool HasApiKey { get; init; }
        public RelaySettingSource ApiKeySource { get; init; }
        public DateTime? LastTestedAt { get; init; }
        public bool? LastTestSuccess { get; init; }
    }

    /// <summary>
    /// Impostazioni del relay <b>per progetto</b> (indirizzo + chiave master cifrata in UserDB).
    /// <para>
    /// La chiave del progetto è il <b>join token</b>, ricavato dal room secret che sta in git: chi ha
    /// il repo entra nella sua stanza senza altro. La <b>chiave master</b> del relay (questa) è
    /// facoltativa e serve una volta sola, per registrare sul relay una stanza nuova; resta una
    /// credenziale locale della macchina perché apre il relay intero. Catena: progetto → globale.
    /// </para>
    /// </summary>
    public interface IProjectRelaySettingsService
    {
        /// <summary>Stato per la UI (chiave mai esposta).</summary>
        RelaySettingsView Get(string projectPath, string relayUrlFromDevelopmentYml);

        /// <summary>Chiave master effettiva: progetto → globale. <c>null</c> ⇒ si entra col solo join token.</summary>
        string ResolveApiKey(string projectPath);

        /// <summary>URL relay effettivo: progetto → <c>.development.yml</c> → default globale.</summary>
        string ResolveRelayUrl(string projectPath, string relayUrlFromDevelopmentYml);

        /// <summary>
        /// Salva. <paramref name="apiKeyPlain"/> null/vuoto ⇒ chiave <b>invariata</b> (la UI non
        /// rimanda mai la chiave esistente); <paramref name="clearApiKey"/> ⇒ rimossa.
        /// </summary>
        void Save(string projectPath, string relayUrl, string apiKeyPlain, bool clearApiKey);

        /// <summary>Registra l'esito dell'ultima verifica di connessione.</summary>
        void RecordTest(string projectPath, bool success);

    }


    public class ProjectRelaySettingsService : IProjectRelaySettingsService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IRelayKeyProtector _keyProtector;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProjectRelaySettingsService> _logger;

        public ProjectRelaySettingsService(
            IServiceScopeFactory scopeFactory,
            IRelayKeyProtector keyProtector,
            IConfiguration configuration,
            ILogger<ProjectRelaySettingsService> logger)
        {
            _scopeFactory = scopeFactory;
            _keyProtector = keyProtector;
            _configuration = configuration;
            _logger = logger;
        }

        public RelaySettingsView Get(string projectPath, string relayUrlFromDevelopmentYml)
        {
            var settings = Read(projectPath);

            var (url, urlSource) = ResolveUrlWithSource(settings, relayUrlFromDevelopmentYml);
            var (key, keySource) = ResolveKeyWithSource(settings);

            return new RelaySettingsView
            {
                RelayUrl = url,
                RelayUrlSource = urlSource,
                HasApiKey = !string.IsNullOrWhiteSpace(key),
                ApiKeySource = keySource,
                LastTestedAt = settings?.LastTestedAt,
                LastTestSuccess = settings?.LastTestSuccess,
            };
        }

        public string ResolveApiKey(string projectPath)
            => ResolveKeyWithSource(Read(projectPath)).Key;

        public string ResolveRelayUrl(string projectPath, string relayUrlFromDevelopmentYml)
            => ResolveUrlWithSource(Read(projectPath), relayUrlFromDevelopmentYml).Url;

        public void Save(string projectPath, string relayUrl, string apiKeyPlain, bool clearApiKey)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                throw new ArgumentException("projectPath è obbligatorio", nameof(projectPath));

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.BeginTransaction();
            try
            {
                var project = FindProject(db, projectPath)
                    ?? throw new InvalidOperationException(
                        $"Nessun progetto registrato per il percorso '{projectPath}': apri il progetto prima di configurarne il relay.");

                var dal = db.GetDal<ProjectRelaySettings>();
                var settings = dal.GetList().FirstOrDefault(s => s.Project.Id == project.Id)
                               ?? new ProjectRelaySettings { Project = project };

                settings.RelayUrl = string.IsNullOrWhiteSpace(relayUrl) ? null : relayUrl.Trim();

                if (clearApiKey)
                    settings.ApiKeyEncrypted = null;
                else if (!string.IsNullOrWhiteSpace(apiKeyPlain))
                    settings.ApiKeyEncrypted = _keyProtector.Protect(apiKeyPlain.Trim());
                // altrimenti: chiave invariata

                dal.Save(settings);
                db.Commit();
            }
            catch
            {
                db.Rollback();
                throw;
            }
        }

        public void RecordTest(string projectPath, bool success)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.BeginTransaction();
            try
            {
                var project = FindProject(db, projectPath);
                if (project == null) { db.Commit(); return; }

                var dal = db.GetDal<ProjectRelaySettings>();
                var settings = dal.GetList().FirstOrDefault(s => s.Project.Id == project.Id);
                if (settings == null) { db.Commit(); return; }

                settings.LastTestedAt = DateTime.UtcNow;
                settings.LastTestSuccess = success;
                dal.Save(settings);
                db.Commit();
            }
            catch
            {
                db.Rollback();
                throw;
            }
        }

        // ---- risoluzione ---------------------------------------------------

        private (string Url, RelaySettingSource Source) ResolveUrlWithSource(
            ProjectRelaySettings settings, string fromDevelopmentYml)
        {
            if (!string.IsNullOrWhiteSpace(settings?.RelayUrl))
                return (settings.RelayUrl.Trim(), RelaySettingSource.Project);

            if (!string.IsNullOrWhiteSpace(fromDevelopmentYml))
                return (fromDevelopmentYml.Trim(), RelaySettingSource.DevelopmentYml);

            var global = _configuration["MdChat:WebSocketUrl"];
            return string.IsNullOrWhiteSpace(global)
                ? (FederationPresenceService.DefaultRelayUrl, RelaySettingSource.Global)
                : (global.Trim(), RelaySettingSource.Global);
        }

        private (string Key, RelaySettingSource Source) ResolveKeyWithSource(ProjectRelaySettings settings)
        {
            if (!string.IsNullOrWhiteSpace(settings?.ApiKeyEncrypted))
            {
                try
                {
                    var plain = _keyProtector.Unprotect(settings.ApiKeyEncrypted);
                    if (!string.IsNullOrWhiteSpace(plain))
                        return (plain, RelaySettingSource.Project);

                    // Decifrata a vuoto: è un dato corrotto, non un "non configurato".
                    _logger.LogWarning("[Relay] la API key salvata per il progetto si decifra a stringa vuota: reimpostala dalle impostazioni di progetto.");
                }
                catch (Exception ex)
                {
                    // Fail-loud: capita se il profilo utente/la macchina cambia (DPAPI è legata all'utente).
                    _logger.LogWarning(ex, "[Relay] impossibile decifrare la API key del progetto: reimpostala dalle impostazioni di progetto.");
                }
            }

            var global = MdChatConfig.ResolveApiKey(_configuration);
            return string.IsNullOrWhiteSpace(global)
                ? (null, RelaySettingSource.None)
                : (global, RelaySettingSource.Global);
        }

        private ProjectRelaySettings Read(string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath)) return null;

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                db.BeginTransaction();
                var project = FindProject(db, projectPath);
                var settings = project == null
                    ? null
                    : db.GetDal<ProjectRelaySettings>().GetList().FirstOrDefault(s => s.Project.Id == project.Id);
                db.Commit();
                return settings;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Relay] lettura impostazioni relay del progetto fallita per '{Path}'", projectPath);
                return null;
            }
        }

        /// <summary>
        /// Progetto per percorso. Materializza prima del confronto: <see cref="AgentPathComparer"/>
        /// non è traducibile in SQL da NHibernate (gotcha ricorrente).
        /// </summary>
        private static Project FindProject(IUserSettingsDB db, string projectPath)
            => db.GetDal<Project>().GetList().ToList()
                 .FirstOrDefault(p => AgentPathComparer.Equals(p.Path, projectPath));
    }
}
