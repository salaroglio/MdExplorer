using System;
using System.Linq;
using System.Net.Http;
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
    /// Impostazioni del relay <b>per progetto</b> (indirizzo + API key cifrata in UserDB).
    /// <para>
    /// La catena di risoluzione è la stessa per entrambi i valori: <b>progetto → .development.yml
    /// → globale</b>. Il motivo per cui la chiave sta qui e non in git è di raggio: il room secret
    /// apre <i>una stanza</i> ed è giusto che viaggi col repo per tutto il team, mentre la API key
    /// apre <i>il relay</i> — tutte le stanze — quindi resta una credenziale locale della macchina.
    /// </para>
    /// </summary>
    public interface IProjectRelaySettingsService
    {
        /// <summary>Stato per la UI (chiave mai esposta).</summary>
        RelaySettingsView Get(string projectPath, string relayUrlFromDevelopmentYml);

        /// <summary>API key effettiva: progetto → globale. <c>null</c> ⇒ dormiente.</summary>
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

        /// <summary>
        /// Bussa davvero al relay con la chiave risolta e registra l'esito. Non deduce nulla dalla
        /// presenza del valore: o il relay risponde, o si sa perché no.
        /// </summary>
        Task<RelayTestResult> TestAsync(string projectPath, string relayUrlFromDevelopmentYml, CancellationToken ct = default);
    }

    /// <summary>Esito della verifica di connessione al relay.</summary>
    public sealed class RelayTestResult
    {
        public bool Success { get; init; }
        public int? StatusCode { get; init; }
        /// <summary>Messaggio già leggibile dall'utente (la UI lo mostra così com'è).</summary>
        public string Message { get; init; }
    }

    public class ProjectRelaySettingsService : IProjectRelaySettingsService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IRelayKeyProtector _keyProtector;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ProjectRelaySettingsService> _logger;

        public ProjectRelaySettingsService(
            IServiceScopeFactory scopeFactory,
            IRelayKeyProtector keyProtector,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<ProjectRelaySettingsService> logger)
        {
            _scopeFactory = scopeFactory;
            _keyProtector = keyProtector;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
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

        public async Task<RelayTestResult> TestAsync(
            string projectPath, string relayUrlFromDevelopmentYml, CancellationToken ct = default)
        {
            var settings = Read(projectPath);
            var (key, keySource) = ResolveKeyWithSource(settings);
            if (string.IsNullOrWhiteSpace(key))
                return new RelayTestResult
                {
                    Success = false,
                    Message = "Nessuna API key configurata: la federazione resta dormiente.",
                };

            var (url, _) = ResolveUrlWithSource(settings, relayUrlFromDevelopmentYml);
            var probe = BuildProbeUrl(url);
            if (probe == null)
                return new RelayTestResult
                {
                    Success = false,
                    Message = $"Indirizzo del relay non valido: '{url}'.",
                };

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(15);
                using var request = new HttpRequestMessage(HttpMethod.Get, probe);
                request.Headers.Add("X-API-Key", key);

                using var response = await client.SendAsync(request, ct);
                var code = (int)response.StatusCode;

                // Il relay risponde 401 senza chiave e 403 con chiave errata: distinguerli dice
                // all'utente COSA sbagliare. Qualsiasi altra risposta HTTP prova che il server
                // c'è e ci riconosce (una stanza mai vista può legittimamente non essere 200).
                var result = code switch
                {
                    401 => new RelayTestResult { Success = false, StatusCode = code, Message = "Il relay non ha ricevuto la chiave." },
                    403 => new RelayTestResult { Success = false, StatusCode = code, Message = "Chiave rifiutata dal relay: non è quella giusta." },
                    _ when code >= 500 => new RelayTestResult { Success = false, StatusCode = code, Message = $"Il relay ha risposto con un errore ({code})." },
                    _ => new RelayTestResult { Success = true, StatusCode = code, Message = $"Relay raggiunto e chiave accettata (fonte: {Describe(keySource)})." },
                };

                RecordTestSafe(projectPath, result.Success);
                return result;
            }
            catch (Exception ex)
            {
                RecordTestSafe(projectPath, false);
                return new RelayTestResult
                {
                    Success = false,
                    Message = $"Relay irraggiungibile: {ex.Message}",
                };
            }
        }

        private void RecordTestSafe(string projectPath, bool success)
        {
            try { RecordTest(projectPath, success); }
            catch (Exception ex) { _logger.LogWarning(ex, "[Relay] impossibile registrare l'esito della verifica"); }
        }

        private static string Describe(RelaySettingSource source) => source switch
        {
            RelaySettingSource.Project => "impostazioni di questo progetto",
            RelaySettingSource.Global => "configurazione globale dell'applicazione",
            RelaySettingSource.DevelopmentYml => ".development.yml",
            _ => "nessuna",
        };

        /// <summary>
        /// Dall'URL websocket del relay all'endpoint REST da interrogare. Il relay espone la sua
        /// API sotto lo stesso host/prefisso: <c>wss://host/mdchat</c> → <c>https://host/mdchat/api/…</c>.
        /// </summary>
        private static string BuildProbeUrl(string relayUrl)
        {
            if (string.IsNullOrWhiteSpace(relayUrl)) return null;

            var http = relayUrl.Trim()
                .Replace("wss://", "https://", StringComparison.OrdinalIgnoreCase)
                .Replace("ws://", "http://", StringComparison.OrdinalIgnoreCase)
                .TrimEnd('/');

            if (!Uri.TryCreate(http, UriKind.Absolute, out var uri)) return null;
            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) return null;

            // Prefisso vuoto (il relay sta sulla radice) o /mdchat: in entrambi i casi l'API sta
            // sotto <prefisso>/api. La stanza è fittizia: ci interessa solo il gate della chiave.
            return $"{http}/api/rooms/mde-relay-probe/presence";
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
