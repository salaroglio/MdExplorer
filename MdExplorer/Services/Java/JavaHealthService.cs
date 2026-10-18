using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ad.Tools.Dal.Abstractions.Interfaces;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.Java
{
    /// <summary>Un avviso nato all'avvio del servizio (o a un controllo successivo), che la UI mostra nella pagina dei progetti finché non viene chiuso.</summary>
    public sealed class StartupNotice
    {
        public string Id { get; init; }
        /// <summary>info · warning · error</summary>
        public string Level { get; init; }
        /// <summary>Chiave stabile che la UI traduce: <c>java-replaced</c>, <c>java-user-path-not-suitable</c>, <c>java-missing</c>.</summary>
        public string Kind { get; init; }
        public IReadOnlyDictionary<string, string> Params { get; init; }
        public DateTime At { get; init; }
    }

    /// <summary>Gli avvisi di avvio: un posto solo, in memoria, con una voce per tipo (un controllo nuovo sostituisce la precedente).</summary>
    public sealed class StartupNotices
    {
        private readonly object _gate = new();
        private readonly Dictionary<string, StartupNotice> _byKind = new(StringComparer.OrdinalIgnoreCase);

        public void Set(StartupNotice notice)
        {
            if (notice == null) throw new ArgumentNullException(nameof(notice));
            lock (_gate) _byKind[notice.Kind] = notice;
        }

        public void ClearKind(string kind) { lock (_gate) _byKind.Remove(kind); }

        public bool Dismiss(string id)
        {
            lock (_gate)
            {
                var key = _byKind.FirstOrDefault(kv => string.Equals(kv.Value.Id, id, StringComparison.OrdinalIgnoreCase)).Key;
                return key != null && _byKind.Remove(key);
            }
        }

        public IReadOnlyList<StartupNotice> All() { lock (_gate) return _byKind.Values.OrderBy(n => n.At).ToList(); }
    }

    /// <summary>Le impostazioni <c>JavaPath</c> e <c>JavaPathSource</c> nel DB utente, lette e scritte come fa <c>AppSettingsController</c> (SQL diretto in aggiornamento: le righe seminate dalle migrazioni hanno il GUID in testo e NHibernate non le aggiorna).</summary>
    public sealed class SessionJavaSettingsStore : IJavaSettingsStore
    {
        public const string PathName = "JavaPath";
        public const string SourceName = "JavaPathSource";
        private readonly ISessionDB _session;

        public SessionJavaSettingsStore(ISessionDB session) { _session = session ?? throw new ArgumentNullException(nameof(session)); }

        public (string Path, string Source) Read()
        {
            var all = _session.GetDal<Setting>().GetList().ToList();
            return (all.FirstOrDefault(s => s.Name == PathName)?.ValueString, all.FirstOrDefault(s => s.Name == SourceName)?.ValueString);
        }

        public void Write(string path, string source)
        {
            Upsert(PathName, path);
            Upsert(SourceName, source);
        }

        /// <summary>Scrive la sorgente senza toccare il percorso: quando il percorso lo salva l'utente dalle impostazioni.</summary>
        public void MarkSource(string source) => Upsert(SourceName, source);

        private void Upsert(string name, string value)
        {
            var dal = _session.GetDal<Setting>();
            var existing = dal.GetList().FirstOrDefault(s => s.Name == name);
            if (existing != null)
            {
                _session.Evict(existing);
                _session.CreateSQLQuery("UPDATE Setting SET ValueString = :val WHERE Name = :name")
                    .SetParameter("val", value ?? string.Empty).SetParameter("name", name).ExecuteUpdate();
            }
            else
            {
                dal.Save(new Setting { Name = name, ValueString = value });
            }
        }
    }

    /// <summary>
    /// Il controllo di salute di Java come servizio (12/10/2026): parte in sfondo APPENA il server è in ascolto (mai nel
    /// percorso di apertura di un progetto), e si può rilanciare quando l'utente salva il percorso. Il rapporto va nel log
    /// e negli <see cref="StartupNotices"/>.
    /// </summary>
    public sealed class JavaHealthService : IHostedService
    {
        public const int MinimumJavaMajor = 21;
        private readonly IServiceProvider _services;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly IJavaLocator _locator;
        private readonly StartupNotices _notices;
        private readonly ILogger<JavaHealthService> _logger;

        public JavaHealthService(IServiceProvider services, IHostApplicationLifetime lifetime, IJavaLocator locator, StartupNotices notices, ILogger<JavaHealthService> logger)
        {
            _services = services; _lifetime = lifetime; _locator = locator; _notices = notices; _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _lifetime.ApplicationStarted.Register(() => _ = Task.Run(() => RunSafelyAsync("avvio")));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        /// <summary>Il controllo, con una sessione propria sul DB utente. Restituisce il rapporto; eccezioni al chiamante.</summary>
        public JavaHealthReport Run()
        {
            using var scope = _services.CreateScope();
            var factory = scope.ServiceProvider.GetRequiredService<IDALFactory<IUserSettingsDB>>();
            using var session = factory.OpenSession();
            using var tx = session.BeginTransaction();
            var report = new JavaHealth(_locator, new SessionJavaSettingsStore(session)).Check(MinimumJavaMajor);
            tx.Commit();
            Publish(report);
            return report;
        }

        private async Task RunSafelyAsync(string when)
        {
            await Task.Yield();
            try
            {
                var report = Run();
                _logger.LogInformation("[JavaHealth] ({When}) esito {Outcome}: usa {Path}", when, report.Outcome, report.EffectivePath ?? "(nessuno)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[JavaHealth] ({When}) controllo fallito", when);
            }
        }

        private void Publish(JavaHealthReport r)
        {
            var p = new Dictionary<string, string>
            {
                ["old"] = r.ConfiguredPath ?? string.Empty,
                ["oldMajor"] = r.ConfiguredMajor?.ToString() ?? "?",
                ["problem"] = r.ConfiguredProblem ?? string.Empty,
                ["problemKind"] = r.ConfiguredProblemKind ?? string.Empty,
                ["new"] = r.BestPath ?? string.Empty,
                ["newMajor"] = r.BestMajor?.ToString() ?? "?",
                ["min"] = r.MinimumMajor.ToString(),
            };
            foreach (var kind in new[] { "java-replaced", "java-user-path-not-suitable", "java-missing" }) _notices.ClearKind(kind);
            switch (r.Outcome)
            {
                case JavaHealthOutcome.Ok:
                    _logger.LogInformation("[JavaHealth] Java {Major} in {Path}: va bene (minimo {Min})", r.ConfiguredMajor, r.ConfiguredPath, r.MinimumMajor);
                    break;
                case JavaHealthOutcome.Replaced:
                    _logger.LogWarning("[JavaHealth] 'JavaPath' = '{Old}' ({Problem}): sostituito con Java {New} in '{NewPath}'. Lo aveva scritto la scoperta automatica, non l'utente.",
                        r.ConfiguredPath, r.ConfiguredProblem, r.BestMajor, r.BestPath);
                    _notices.Set(new StartupNotice { Id = Guid.NewGuid().ToString("N"), Level = "info", Kind = "java-replaced", Params = p, At = DateTime.UtcNow });
                    break;
                case JavaHealthOutcome.UserPathNotSuitable:
                    _logger.LogWarning("[JavaHealth] 'JavaPath' = '{Old}' ({Problem}) è stato impostato dall'utente: non lo tocco. Sul computer c'è Java {New} in '{NewPath}'.",
                        r.ConfiguredPath, r.ConfiguredProblem, r.BestMajor, r.BestPath);
                    _notices.Set(new StartupNotice { Id = Guid.NewGuid().ToString("N"), Level = "warning", Kind = "java-user-path-not-suitable", Params = p, At = DateTime.UtcNow });
                    break;
                case JavaHealthOutcome.NoSuitableJava:
                    _logger.LogError("[JavaHealth] nessun Java {Min} sul computer ('JavaPath' = '{Old}': {Problem}). Grafo del codice e memoria degli agenti non partono finché non si installa Temurin {Min}.",
                        r.MinimumMajor, r.ConfiguredPath, r.ConfiguredProblem);
                    _notices.Set(new StartupNotice { Id = Guid.NewGuid().ToString("N"), Level = "error", Kind = "java-missing", Params = p, At = DateTime.UtcNow });
                    break;
            }
        }
    }
}
