using System;
using System.Linq;
using MdExplorer.Features.Services;
using MdExplorer.Features.Services.RemoteFolders;
using MdExplorer.RemoteProtocol;

namespace MdExplorer.Services.RemoteFolders
{
    /// <summary>
    /// Da exit code e messaggi di ssh / del serverino alla porta chiusa e al rimedio (sprint 2026-10-21-Cartella-Remota-Via-SSH,
    /// P10, segnali verificati in F0): 255 = ssh non è entrato; 126 = il serverino non si può eseguire; 3 = la cartella
    /// non c'è. Puro, per i test.
    /// </summary>
    public static class RemoteDiagnosis
    {
        public const int SshFailed = 255;
        public const int CannotExecute = 126;
        public const int CommandNotFound = 127;

        /// <summary>ssh è uscito con 255: non è entrato nel server. Il messaggio di ssh dice perché.</summary>
        public static RemoteUnavailableException AccessFailure(string destination, string stderr)
        {
            var e = stderr ?? string.Empty;
            string it, en;
            if (Has(e, "REMOTE HOST IDENTIFICATION HAS CHANGED"))
            {
                it = $"la chiave dell'host di {destination} è CAMBIATA rispetto a known_hosts. Può essere un server reinstallato o qualcuno in mezzo: verifica con chi lo amministra, poi correggi known_hosts (ssh-keygen -R …).";
                en = $"the host key of {destination} has CHANGED compared to known_hosts. It may be a reinstalled server or someone in the middle: check with its administrator, then fix known_hosts (ssh-keygen -R …).";
            }
            else if (Has(e, "Host key verification failed"))
            {
                it = $"{destination} non è ancora tra gli host conosciuti (known_hosts). Collegati una volta dal terminale (ssh {destination}) e accetta la sua chiave.";
                en = $"{destination} is not among the known hosts yet (known_hosts). Connect once from the terminal (ssh {destination}) and accept its key.";
            }
            else if (Has(e, "Permission denied"))
            {
                it = $"accesso negato a {destination}. MDE non chiede password: serve una chiave SSH. Dal terminale: ssh-copy-id {destination} (o aggiungi la chiave a ssh-agent).";
                en = $"access denied to {destination}. MDE does not ask for passwords: an SSH key is needed. From the terminal: ssh-copy-id {destination} (or add the key to ssh-agent).";
            }
            else if (Has(e, "Could not resolve hostname"))
            {
                it = $"il nome «{destination}» non si risolve: controlla il nome o l'alias in ~/.ssh/config.";
                en = $"the name «{destination}» does not resolve: check the name or the alias in ~/.ssh/config.";
            }
            else if (Has(e, "Connection refused") || Has(e, "timed out") || Has(e, "No route to host") || Has(e, "Network is unreachable"))
            {
                it = $"{destination} non risponde (rete, VPN, porta o server spento).";
                en = $"{destination} does not answer (network, VPN, port or server down).";
            }
            else
            {
                it = $"ssh non è riuscito a entrare in {destination}.";
                en = $"ssh could not get into {destination}.";
            }
            return new RemoteUnavailableException(RemoteGate.Access, UiText.Say(it, en) + Detail(e));
        }

        /// <summary>Il serverino non ha salutato: perché, dall'exit code.</summary>
        public static RemoteUnavailableException StartFailure(RemoteLocation location, int? exitCode, string stderr)
        {
            switch (exitCode)
            {
                case SshFailed:
                    return AccessFailure(location.Destination, stderr);
                case CannotExecute:
                    return new RemoteUnavailableException(RemoteGate.Execution, UiText.Say(
                        $"su {location.Host} il serverino non si può eseguire: la home è probabilmente montata «noexec», oppure una regola di sicurezza (SELinux, fapolicyd, antivirus) blocca i programmi nuovi. Serve un'altra cartella per il serverino, o chiedi a chi amministra il server di autorizzarlo.",
                        $"on {location.Host} the agent cannot be executed: the home is probably mounted «noexec», or a security rule (SELinux, fapolicyd, antivirus) blocks new programs. Another folder for the agent is needed, or ask the server administrator to allow it.") + Detail(stderr));
                case CommandNotFound:
                    return new RemoteUnavailableException(RemoteGate.Execution, UiText.Say(
                        $"su {location.Host} il serverino non c'è dove MDE l'ha caricato.",
                        $"on {location.Host} the agent is not where MDE uploaded it.") + Detail(stderr));
                case AgentProtocol.ExitRootNotFound:
                    return new RemoteUnavailableException(RemoteGate.Folder, UiText.Say(
                        $"la cartella {location.Path} non c'è su {location.Host} (o l'utente SSH non la vede).",
                        $"the folder {location.Path} is not on {location.Host} (or the SSH user cannot see it).") + Detail(stderr));
                default:
                    return new RemoteUnavailableException(RemoteGate.Channel, UiText.Say(
                        $"{location}: il serverino non è partito (uscita {exitCode?.ToString() ?? "nessuna"}).",
                        $"{location}: the agent did not start (exit {exitCode?.ToString() ?? "none"}).") + Detail(stderr));
            }
        }

        /// <summary>L'account entra ma non esegue comandi (account solo SFTP, shell ristretta, ForceCommand).</summary>
        public static RemoteUnavailableException CommandsRefused(string destination, string output)
            => new(RemoteGate.Commands, UiText.Say(
                $"l'account su {destination} non permette di eseguire comandi (solo SFTP o shell ristretta): con questo account la cartella remota non può funzionare.",
                $"the account on {destination} does not allow running commands (SFTP only or restricted shell): the remote folder cannot work with this account.") + Detail(output));

        public static RemoteUnavailableException AgentFolderNotWritable(string destination, string folder, string output)
            => new(RemoteGate.AgentFolder, UiText.Say(
                $"su {destination} non si può scrivere nella cartella del serverino ~/{folder} (home in sola lettura, quota piena, account senza home).",
                $"on {destination} the agent folder ~/{folder} cannot be written (read-only home, full quota, account without a home).") + Detail(output));

        private static bool Has(string text, string what) => text.IndexOf(what, StringComparison.OrdinalIgnoreCase) >= 0;

        /// <summary>Le ultime righe del messaggio originale, per chi deve capire di più.</summary>
        private static string Detail(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            var lines = text.Trim().Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0).TakeLast(3);
            return " [" + string.Join(" | ", lines) + "]";
        }
    }
}
