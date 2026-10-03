using System.Threading.Tasks;

namespace MdExplorer.Services.Git.Interfaces
{
    /// <summary>
    /// Collega un repository locale a un remoto già esistente e fa il primo push. Nessuna
    /// credenziale passa di qui: l'autenticazione la fa il git di sistema col suo credential
    /// manager (<see cref="INativeGitTransport"/>). Se serve un login, lo fa git al primo push.
    /// </summary>
    public interface IGenericRemoteService
    {
        Task<SetupRemoteGenericResult> SetupRemoteGenericAsync(SetupRemoteGenericRequest request);
    }

    public class SetupRemoteGenericRequest
    {
        public string RepositoryPath { get; set; }
        public string RemoteUrl { get; set; }
        public string RemoteName { get; set; } = "origin";
        /// <summary>
        /// L'account git da usare per l'host del remoto (con più account sullo stesso host git
        /// deve saperlo). Finisce in <c>credential.&lt;scheme://host&gt;.username</c> del repository.
        /// Vuoto = non scrivere niente.
        /// </summary>
        public string AccountUsername { get; set; }
        public bool PushAfterAdd { get; set; } = true;
    }

    public class SetupRemoteGenericResult
    {
        /// <summary>Vero solo se il remote è configurato E, se richiesto, il push è riuscito. Mai «verde con dentro un fallimento».</summary>
        public bool Success { get; set; }
        public string Message { get; set; }
        /// <summary>Lo stderr di git così com'è: è ciò che l'utente deve leggere.</summary>
        public string Error { get; set; }
        public string RemoteUrl { get; set; }
        public bool PushAttempted { get; set; }
        public bool PushSucceeded { get; set; }
        public long DurationMs { get; set; }
    }
}
