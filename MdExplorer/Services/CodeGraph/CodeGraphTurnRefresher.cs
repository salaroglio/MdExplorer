using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.CodeGraph
{
    public interface ICodeGraphTurnRefresher
    {
        /// <summary>Prima di un turno di MarkAgent: aspetta l'aggiornamento del grafo lasciato dal turno prima.</summary>
        Task BeforeTurnAsync(string projectPath, CancellationToken ct = default);
        /// <summary>A fine turno: fa partire in sfondo l'aggiornamento del grafo del progetto (non rallenta la risposta).</summary>
        void AfterTurn(string projectPath);
    }

    /// <summary>
    /// Il grafo del codice segue le modifiche di MarkAgent (sprint 2026-10-12-Radici-Di-Codice-Esterne, F4, R8): a fine
    /// turno si rianalizzano le cartelle <c>program</c> e le radici collegate (git dice che cosa è cambiato, l'hash
    /// conferma, si caricano solo gli script toccati); il turno dopo aspetta che sia finito, così risponde sul codice di
    /// adesso. Un aggiornamento per progetto alla volta, in coda. Un errore non toglie la chat: si scrive nel log e il
    /// grafo resta quello di prima — lo dirà la pagina del grafo, e la prossima analisi ci riprova.
    /// </summary>
    public sealed class CodeGraphTurnRefresher : ICodeGraphTurnRefresher
    {
        /// <summary>Quanto il turno dopo aspetta al massimo: oltre, parte e lo dice nel log (un repository enorme non blocca la chat).</summary>
        public static readonly TimeSpan MaxWait = TimeSpan.FromSeconds(60);

        private readonly ICodeAnalysisService _analysis;
        private readonly ILogger<CodeGraphTurnRefresher> _logger;
        private readonly ConcurrentDictionary<string, Task> _pending = new(StringComparer.Ordinal);

        public CodeGraphTurnRefresher(ICodeAnalysisService analysis, ILogger<CodeGraphTurnRefresher> logger)
        {
            _analysis = analysis; _logger = logger;
        }

        public async Task BeforeTurnAsync(string projectPath, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(projectPath) || !_pending.TryGetValue(Key(projectPath), out var pending) || pending.IsCompleted) return;
            try { await pending.WaitAsync(MaxWait, ct); }
            catch (TimeoutException)
            {
                _logger.LogWarning("[CodeGraph] l'aggiornamento del grafo dopo il turno precedente dura più di {Seconds} s: il turno parte sul grafo di prima", MaxWait.TotalSeconds);
            }
        }

        public void AfterTurn(string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath)) return;
            _pending.AddOrUpdate(Key(projectPath), _ => RunAsync(projectPath, Task.CompletedTask), (_, previous) => RunAsync(projectPath, previous));
        }

        private async Task RunAsync(string projectPath, Task previous)
        {
            try { await previous; } catch { /* già scritto nel log da chi l'ha fatto */ }
            await Task.Yield();
            try
            {
                var outcomes = await _analysis.RefreshAfterTurnAsync(projectPath);
                var reparsed = outcomes.Sum(o => o.Reparsed);
                var removed = outcomes.Sum(o => o.ScriptsRemoved);
                foreach (var o in outcomes.Where(o => o.Error != null))
                    _logger.LogWarning("[CodeGraph] dopo il turno, '{Folder}' non aggiornata: {Error}", o.Folder, o.Error);
                if (reparsed + removed > 0)
                    _logger.LogInformation("[CodeGraph] dopo il turno di MarkAgent: {Reparsed} file rianalizzati, {Removed} script tolti ({Folders})",
                        reparsed, removed, string.Join(", ", outcomes.Where(o => o.Reparsed + o.ScriptsRemoved > 0).Select(o => o.Folder)));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[CodeGraph] aggiornamento del grafo dopo il turno fallito per {Project}", projectPath);
            }
        }

        private static string Key(string projectPath) => System.IO.Path.GetFullPath(projectPath).TrimEnd('/', '\\');
    }
}
