using System;

namespace MdExplorer.Services.RemoteFolders
{
    /// <summary>
    /// Il server non si raggiunge o il serverino non parte: il messaggio dice quale porta è chiusa e cosa fare (sprint
    /// 2026-10-21-Cartella-Remota-Via-SSH, P10). È un <see cref="InvalidOperationException"/>: i controller lo rendono 400.
    /// </summary>
    public sealed class RemoteUnavailableException : InvalidOperationException
    {
        public RemoteGate Gate { get; }

        public RemoteUnavailableException(RemoteGate gate, string message) : base(message) { Gate = gate; }
    }

    /// <summary>Il serverino ha rifiutato un'operazione (file cambiato, fuori dalla radice, …): il codice del protocollo.</summary>
    public sealed class RemoteOperationException : InvalidOperationException
    {
        public string Code { get; }

        public RemoteOperationException(string code, string message) : base(message) { Code = code; }
    }

    /// <summary>Le porte che il serverino attraversa per partire, nell'ordine (P10).</summary>
    public enum RemoteGate
    {
        /// <summary>Accesso SSH: chiave, host in known_hosts, rete.</summary>
        Access = 1,
        /// <summary>Comandi permessi: account solo SFTP, shell ristretta, ForceCommand.</summary>
        Commands = 2,
        /// <summary>Scrittura della cartella del serverino.</summary>
        AgentFolder = 3,
        /// <summary>Esecuzione del serverino: home noexec, SELinux, fapolicyd, EDR.</summary>
        Execution = 4,
        /// <summary>La cartella remota: non c'è o non è accessibile all'utente SSH.</summary>
        Folder = 5,
        /// <summary>Il canale si è interrotto dopo la partenza.</summary>
        Channel = 6,
    }
}
