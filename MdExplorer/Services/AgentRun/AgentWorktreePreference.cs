using System;
using System.IO;
using System.Linq;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.AgentRun
{
    /// <summary>
    /// Se gli agenti di un progetto lavorano in un worktree isolato, <b>su questa macchina</b>.
    /// <para>
    /// Sta in UserDB e non nel <c>.development.yml</c> per una ragione precisa: non cambia
    /// <i>cosa</i> fanno gli agenti, cambia <i>dove</i> lavorano, e costa spazio disco locale.
    /// Metterlo in git significherebbe imporre al collega col portatile pieno una scelta che non
    /// ha fatto. L'auto-merge invece resta in git, perché quello decide se il ramo principale può
    /// cambiare da solo — ed è una regola del repo, non della macchina.
    /// </para>
    /// </summary>
    public interface IAgentWorktreePreference
    {
        /// <summary>Valore effettivo: scelta locale se c'è, altrimenti il default.</summary>
        bool IsEnabled(string projectPath);

        /// <summary>Scelta locale grezza (<c>null</c> = non decisa, vale il default).</summary>
        bool? GetRaw(string projectPath);

        /// <summary>Default per questo progetto: git con remoto <c>origin</c>.</summary>
        bool DefaultFor(string projectPath);

        /// <summary>Imposta (o azzera, con <c>null</c>) la scelta locale.</summary>
        void Set(string projectPath, bool? enabled);
    }

    public class AgentWorktreePreference : IAgentWorktreePreference
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly MdExplorer.Services.IProjectMetadataService _metadata;
        private readonly ILogger<AgentWorktreePreference> _logger;

        public AgentWorktreePreference(
            IServiceScopeFactory scopeFactory,
            MdExplorer.Services.IProjectMetadataService metadata,
            ILogger<AgentWorktreePreference> logger)
        {
            _scopeFactory = scopeFactory;
            _metadata = metadata;
            _logger = logger;
        }

        public bool IsEnabled(string projectPath) => GetRaw(projectPath) ?? DefaultFor(projectPath);

        /// <summary>Progetti per cui l'import dal yml è già stato tentato (una sola volta per avvio).</summary>
        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, byte> _importAttempted
            = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Import una-tantum dalla sede precedente: se qui non è mai stato deciso nulla ma il
        /// <c>.development.yml</c> porta ancora un valore esplicito, quel valore diventa la
        /// preferenza locale. Una scelta già espressa non deve essere ignorata in silenzio solo
        /// perché l'impostazione ha cambiato casa.
        /// <para>
        /// Vive <b>qui</b> e non nell'endpoint della UI: se lo facesse solo la UI, il dispatcher
        /// vedrebbe il default e la UI il valore importato — due verità diverse a seconda di chi
        /// guarda per primo.
        /// </para>
        /// </summary>
        private bool? ImportLegacyIfAny(string projectPath)
        {
            if (!_importAttempted.TryAdd(projectPath, 0)) return null;
            try
            {
                var legacy = _metadata.GetAgentCity(projectPath)?.UseAgentWorktrees;
                if (legacy == null) return null;
                Set(projectPath, legacy);
                _logger.LogInformation(
                    "[Worktree] preferenza importata dal .development.yml per '{Path}': {Value}", projectPath, legacy);
                return legacy;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Worktree] import dal .development.yml fallito per '{Path}'", projectPath);
                return null;
            }
        }

        public bool? GetRaw(string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath)) return null;
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                // Clear() prima di leggere: la sessione e' condivisa e la cache di primo livello
                // restituirebbe l'entita' com'era prima di una scrittura fatta altrove — e' il
                // motivo per cui tutti i lettori per-progetto in UserDB lo fanno.
                db.Clear();
                db.BeginTransaction();
                var project = FindProject(db, projectPath);
                var value = project?.UseAgentWorktrees;
                db.Commit();
                return value ?? ImportLegacyIfAny(projectPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Worktree] lettura preferenza fallita per '{Path}'", projectPath);
                return null;
            }
        }

        public void Set(string projectPath, bool? enabled)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                throw new ArgumentException("projectPath è obbligatorio", nameof(projectPath));

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.Clear();
            db.BeginTransaction();
            try
            {
                var project = FindProject(db, projectPath)
                    ?? throw new InvalidOperationException(
                        $"Nessun progetto registrato per '{projectPath}': aprilo prima di configurarne l'isolamento.");
                project.UseAgentWorktrees = enabled;
                db.GetDal<Project>().Save(project);
                db.Commit();
            }
            catch
            {
                db.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Git <b>con remoto <c>origin</c></b>. Non basta <c>.git</c>: il worktree si prepara con
        /// un <c>fetch</c> e prende il branch base da <c>origin/HEAD</c>. Su un repo solo locale
        /// un default acceso farebbe fallire ogni run al prepare — e un default che rompe non è
        /// un default. In dubbio, spento.
        /// </summary>
        public bool DefaultFor(string projectPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(projectPath)) return false;
                var gitPath = Path.Combine(projectPath, ".git");
                string configPath;

                if (Directory.Exists(gitPath))
                {
                    configPath = Path.Combine(gitPath, "config");
                }
                else if (File.Exists(gitPath))
                {
                    // worktree/submodule: '.git' è un file con "gitdir: <percorso>"
                    var line = File.ReadAllText(gitPath).Trim();
                    const string prefix = "gitdir:";
                    if (!line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return false;
                    var dir = line.Substring(prefix.Length).Trim();
                    if (!Path.IsPathRooted(dir)) dir = Path.GetFullPath(Path.Combine(projectPath, dir));
                    configPath = Path.Combine(dir, "config");
                }
                else return false;

                return File.Exists(configPath)
                       && File.ReadAllText(configPath).Contains("[remote \"origin\"]", StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
        }

        private static Project FindProject(IUserSettingsDB db, string projectPath)
            => db.GetDal<Project>().GetList().ToList()
                 .FirstOrDefault(p => string.Equals(p.Path, projectPath, StringComparison.OrdinalIgnoreCase));
    }
}
