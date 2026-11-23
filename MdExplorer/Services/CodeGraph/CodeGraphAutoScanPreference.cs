using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.CodeGraph
{
    public interface ICodeGraphAutoScanPreference
    {
        /// <summary>Le cartelle <c>program</c> si rianalizzano da sole (all'apertura del progetto e dopo ogni turno di MarkAgent)?</summary>
        bool IsEnabled(string projectPath);
        void Set(string projectPath, bool enabled);

        /// <summary>
        /// Gli script di <c>.mde-code</c> si caricano da soli in Fuseki (all'apertura del progetto e quando cambiano)?
        /// È il caricamento che avvia Java.
        /// </summary>
        bool IsLoadEnabled(string projectPath);
        void SetLoad(string projectPath, bool enabled);
    }

    /// <summary>
    /// Che cosa il grafo del codice fa da solo, per progetto e per QUESTA macchina (UserDB, non <c>.development.yml</c>):
    /// su un computer lento costa processore, memoria e disco, su uno veloce no, e la scelta non si impone al team via git.
    /// Due scelte separate (23/11/2026), tutte e due spente finché non le si accende:
    /// la scansione del codice, e il caricamento in Fuseki degli script già salvati. Spente, tutto resta a richiesta
    /// dalla pagina del grafo.
    /// </summary>
    public sealed class CodeGraphAutoScanPreference : ICodeGraphAutoScanPreference
    {
        public const string SettingPrefix = "CodeGraphAutoScan:";
        public const string LoadSettingPrefix = "CodeGraphAutoLoad:";

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CodeGraphAutoScanPreference> _logger;
        private readonly ConcurrentDictionary<string, bool> _known = new(StringComparer.Ordinal);

        public CodeGraphAutoScanPreference(IServiceScopeFactory scopeFactory, ILogger<CodeGraphAutoScanPreference> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        /// <summary>Il nome dell'impostazione: l'impronta del percorso, perché un percorso lungo non sta nel nome.</summary>
        public static string SettingName(string projectPath, string prefix = SettingPrefix)
        {
            var full = Path.GetFullPath(projectPath).TrimEnd('/', '\\');
            if (OperatingSystem.IsWindows()) full = full.ToLowerInvariant();
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(full));
            return prefix + Convert.ToHexString(hash, 0, 16).ToLowerInvariant();
        }

        public bool IsEnabled(string projectPath) => Read(projectPath, SettingPrefix);
        public void Set(string projectPath, bool enabled) => Write(projectPath, SettingPrefix, enabled, "Scansione automatica del codice per il grafo (per progetto, su questa macchina)");
        public bool IsLoadEnabled(string projectPath) => Read(projectPath, LoadSettingPrefix);
        public void SetLoad(string projectPath, bool enabled) => Write(projectPath, LoadSettingPrefix, enabled, "Caricamento automatico del grafo del codice in Fuseki (per progetto, su questa macchina)");

        private bool Read(string projectPath, string prefix)
        {
            if (string.IsNullOrWhiteSpace(projectPath)) return false;
            var name = SettingName(projectPath, prefix);
            if (_known.TryGetValue(name, out var known)) return known;
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                db.Clear();
                db.BeginTransaction();
                var setting = db.GetDal<Setting>().GetList().FirstOrDefault(x => x.Name == name);
                db.Commit();
                var enabled = setting?.ValueInt == 1;
                _known[name] = enabled;
                return enabled;
            }
            catch (Exception ex)
            {
                // Non si sa: non si fa. Un lavoro non chiesto su una macchina lenta è il danno da evitare.
                _logger.LogWarning(ex, "[CodeGraph] preferenza {Prefix} non leggibile: per ora resta spenta", prefix);
                return false;
            }
        }

        private void Write(string projectPath, string prefix, bool enabled, string description)
        {
            if (string.IsNullOrWhiteSpace(projectPath)) throw new ArgumentException("projectPath", nameof(projectPath));
            var name = SettingName(projectPath, prefix);
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.BeginTransaction();
            var dal = db.GetDal<Setting>();
            var setting = dal.GetList().FirstOrDefault(x => x.Name == name) ?? new Setting { Name = name, Description = description };
            setting.ValueInt = enabled ? 1 : 0;
            dal.Save(setting);
            db.Commit();
            _known[name] = enabled;
            _logger.LogInformation("[CodeGraph] {Prefix} = {Enabled}", prefix, enabled);
        }
    }
}
