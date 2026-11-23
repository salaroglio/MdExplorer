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
    }

    /// <summary>
    /// La scansione automatica del codice per il grafo, per progetto e per QUESTA macchina (UserDB, non
    /// <c>.development.yml</c>): su un computer lento costa processore e disco, su uno veloce no, e la scelta non si
    /// impone al team via git. Spenta finché non la si accende (23/11/2026): l'analisi resta a richiesta, dalla pagina
    /// del grafo. Gli script già salvati in <c>.mde-code</c> si caricano comunque all'apertura: quello non è una scansione.
    /// </summary>
    public sealed class CodeGraphAutoScanPreference : ICodeGraphAutoScanPreference
    {
        public const string SettingPrefix = "CodeGraphAutoScan:";

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CodeGraphAutoScanPreference> _logger;
        private readonly ConcurrentDictionary<string, bool> _known = new(StringComparer.Ordinal);

        public CodeGraphAutoScanPreference(IServiceScopeFactory scopeFactory, ILogger<CodeGraphAutoScanPreference> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        /// <summary>Il nome dell'impostazione: l'impronta del percorso, perché un percorso lungo non sta nel nome.</summary>
        public static string SettingName(string projectPath)
        {
            var full = Path.GetFullPath(projectPath).TrimEnd('/', '\\');
            if (OperatingSystem.IsWindows()) full = full.ToLowerInvariant();
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(full));
            return SettingPrefix + Convert.ToHexString(hash, 0, 16).ToLowerInvariant();
        }

        public bool IsEnabled(string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath)) return false;
            var name = SettingName(projectPath);
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
                // Non si sa: non si scansiona. Una scansione non chiesta su una macchina lenta è il danno da evitare.
                _logger.LogWarning(ex, "[CodeGraph] preferenza della scansione automatica non leggibile: per ora resta spenta");
                return false;
            }
        }

        public void Set(string projectPath, bool enabled)
        {
            if (string.IsNullOrWhiteSpace(projectPath)) throw new ArgumentException("projectPath", nameof(projectPath));
            var name = SettingName(projectPath);
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.BeginTransaction();
            var dal = db.GetDal<Setting>();
            var setting = dal.GetList().FirstOrDefault(x => x.Name == name)
                ?? new Setting { Name = name, Description = "Scansione automatica del codice per il grafo (per progetto, su questa macchina)" };
            setting.ValueInt = enabled ? 1 : 0;
            dal.Save(setting);
            db.Commit();
            _known[name] = enabled;
            _logger.LogInformation("[CodeGraph] scansione automatica del codice = {Enabled}", enabled);
        }
    }
}
