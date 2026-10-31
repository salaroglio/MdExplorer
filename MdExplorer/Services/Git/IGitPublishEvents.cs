using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.Services.Git
{
    /// <summary>
    /// Chi vuole sapere dei push e dei pull fatti da MDE (oggi: gli avvisi del bot di progetto, Telecomando D5). Lo chiama
    /// <see cref="NativeGitTransport"/>, l'unico punto da cui MDE parla con un remoto. Le chiamate non devono mai cambiare
    /// l'esito dell'operazione git né farla aspettare la rete: chi implementa lavora in locale e manda il resto in sfondo.
    /// </summary>
    public interface IGitPublishEvents
    {
        /// <summary>Prima del push, in locale: che cosa sta per partire. Il risultato torna a <see cref="AfterPush"/>; null = niente da dire.</summary>
        Task<object> BeforePushAsync(string repositoryPath, string remoteName, string branchName, CancellationToken ct);

        /// <summary>Il push è finito. <paramref name="before"/> è quanto restituito da <see cref="BeforePushAsync"/>.</summary>
        void AfterPush(object before, bool ok);

        /// <summary>Un pull è fallito: se ha lasciato conflitti, va detto.</summary>
        Task PullFailedAsync(string repositoryPath, CancellationToken ct);
    }
}
