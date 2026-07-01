using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.Services.IndexingPipeline
{
    /// <summary>
    /// Pipeline asincrona di indicizzazione del progetto su apertura.
    ///
    /// Sostituisce la sequenza bloccante CleanupDatabaseDuplicates + IndexAllMarkdownFiles +
    /// IndexLinksInBackground (Task.Run + scope nuovo) di MdFilesController. Gira su una
    /// sessione NHibernate isolata (IsolatedEngineDB) creata da IDatabaseManager, così
    /// non collide col session-per-request del controller e non rischia il "virgin DB"
    /// del pattern ReplaceDalFeatures.
    ///
    /// FSW lock: il chiamante disabilita FSW prima di triggerare la pipeline; la pipeline
    /// lo riabilita al termine (finally).
    /// </summary>
    public interface IIndexingPipelineService
    {
        /// <summary>
        /// Esegue la pipeline INCREMENTALE. Va invocato fire-and-forget (NON awaited) dal caller HTTP.
        /// Solo i file nuovi/cambiati/cancellati (diff su fingerprint mtime+size→hash persistiti
        /// su MarkdownFile) vengono riprocessati; a regime un'apertura costa un walk FS + stat.
        /// </summary>
        /// <param name="connectionId">SignalR connection per emettere eventi al client</param>
        /// <param name="projectPath">Path assoluto del progetto</param>
        /// <param name="linkIndexingEnabled">Se false, salta parse link + embed</param>
        /// <param name="forceFullReindex">Se true, ignora i fingerprint e riprocessa tutto (gli Id restano stabili)</param>
        /// <param name="ct">Cancellation token</param>
        Task RunAsync(string connectionId, string projectPath, bool linkIndexingEnabled, bool forceFullReindex = false, CancellationToken ct = default);
    }
}
