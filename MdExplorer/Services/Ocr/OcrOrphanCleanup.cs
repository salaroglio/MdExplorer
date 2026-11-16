using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services.Ocr;
using Microsoft.Extensions.Hosting;

namespace MdExplorer.Service.Services.Ocr
{
    /// <summary>
    /// All'avvio del servizio: se una sessione precedente è stata chiusa o è caduta a metà di una
    /// conversione, il suo <c>llama-server</c> può essere rimasto acceso con 1,6 GB di RAM. Qui lo
    /// si termina, prima che l'utente se ne accorga dal computer lento.
    /// </summary>
    public class OcrOrphanCleanup : IHostedService
    {
        private readonly IOcrEngine _engine;

        public OcrOrphanCleanup(IOcrEngine engine) => _engine = engine;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _engine.KillOrphan();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
