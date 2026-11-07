using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Features.Configuration;
using MdExplorer.Features.Services;
using MdExplorer.Hubs;
using MdExplorer.Utilities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services
{
    /// <summary>Lo stato di <c>.development.yml</c> per il client: l'avviso nella lingua dell'interfaccia e i gruppi spenti.</summary>
    public sealed record DevelopmentConfigState(string ProjectPath, string Notice, string[] Off, bool WrittenByNewerMde, int FileSchema, int Schema);

    /// <summary>
    /// L'avviso all'utente sui gruppi spenti di <c>.development.yml</c> (F4 dello sprint
    /// docs-internal/Sprints/2026-11-07-Schema-Development-Yml.md, D4): a ogni apertura del progetto, dopo un pull e quando il file
    /// cambia. Va a tutte le finestre con l'evento <see cref="Event"/>; lo mostra quella del progetto. All'apertura si ripete sempre;
    /// dal pull e dal watcher solo se è cambiato, così i salvataggi di MDE e gli eventi doppi del watcher non lo ripetono. Quando il
    /// file torna a posto parte un avviso vuoto, che chiude quello aperto.
    /// </summary>
    public sealed class DevelopmentConfigNotices
    {
        public const string Event = "developmentConfigOff";

        private readonly IHubContext<MonitorMDHub> _hub;
        private readonly ILogger<DevelopmentConfigNotices> _logger;
        private readonly ConcurrentDictionary<string, string> _last = new(StringComparer.OrdinalIgnoreCase);

        public DevelopmentConfigNotices(IHubContext<MonitorMDHub> hub, ILogger<DevelopmentConfigNotices> logger)
        {
            _hub = hub;
            _logger = logger;
        }

        /// <summary>
        /// Lo stato del file del progetto. Il testo usa <c>UiText</c>: chiamalo fuori da ogni transazione del DB (03/11).
        /// </summary>
        public static DevelopmentConfigState StateOf(string projectPath)
        {
            var check = DevelopmentConfigText.ReadFile(DevelopmentConfigFile.PathIn(projectPath)).Check;
            return new DevelopmentConfigState(projectPath, check.Describe(UiText.Say), check.SwitchedOff.Select(o => o.Path).ToArray(),
                check.WrittenByNewerMde, check.FileSchema, DevelopmentSchemaSnapshots.Current);
        }

        /// <summary>
        /// Lo stato chiesto dal client (all'apertura del progetto e dalle schermate delle impostazioni): è anche l'ultimo avviso dato,
        /// così il watcher non lo ripete subito dopo.
        /// </summary>
        public DevelopmentConfigState Current(string projectPath)
        {
            var state = StateOf(projectPath);
            _last[Key(projectPath)] = state.Notice;
            return state;
        }

        private static string Key(string projectPath) => Path.GetFullPath(projectPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        /// <summary>
        /// Manda l'avviso. <paramref name="always"/>: all'apertura del progetto si ripete anche se è uguale all'ultimo (D4). Non lancia:
        /// un avviso che non parte si scrive nel log, il lavoro che l'ha chiesto prosegue.
        /// </summary>
        public void Announce(string projectPath, bool always, string why)
        {
            if (string.IsNullOrWhiteSpace(projectPath) || !Directory.Exists(projectPath)) return;
            // Fuori dal chiamante: la lingua si legge dal DB, e chi annuncia può essere in mezzo a un'apertura o a un pull.
            _ = Task.Run(async () =>
            {
                try
                {
                    var state = StateOf(projectPath);
                    var key = Key(projectPath);
                    var previous = _last.TryGetValue(key, out var p) ? p : string.Empty;
                    _last[key] = state.Notice;
                    if (state.Notice.Length == 0 && previous.Length == 0) return;          // tutto a posto, e lo era
                    if (!always && state.Notice == previous) return;                        // già detto
                    if (state.Notice.Length > 0)
                        _logger.LogWarning("[DevelopmentConfig] {Project} ({Why}): {Notice}", projectPath, why, state.Notice);
                    await _hub.Clients.All.SendAsync(Event, state);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[DevelopmentConfig] avviso non partito per {Project} ({Why})", projectPath, why);
                }
            });
        }
    }
}
