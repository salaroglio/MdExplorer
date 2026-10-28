using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MdExplorer.Services.RemoteFolders
{
    /// <summary>
    /// Come si lancia un comando sul server: in produzione <c>ssh</c> di sistema (P1), nei test una shell locale. Il
    /// comando è una riga per <c>sh</c> sul server, già quotata con <see cref="RemoteShell.Quote"/>.
    /// </summary>
    public interface IRemoteTransport
    {
        ProcessStartInfo Command(string destination, string remoteCommand);
    }

    /// <summary>
    /// <c>ssh</c> di sistema (OpenSSH, Linux e Windows 10+): legge da solo <c>~/.ssh/config</c>, le chiavi, <c>ssh-agent</c>,
    /// <c>ProxyJump</c> e <c>known_hosts</c>. <c>BatchMode</c>: niente password chieste da MDE (P2) — se servisse una
    /// password, o l'host non fosse conosciuto, ssh esce con 255 e il messaggio dice cosa fare.
    /// </summary>
    public sealed class SshTransport : IRemoteTransport
    {
        private readonly string _executable;
        private readonly IReadOnlyList<string> _extraArgs;

        /// <param name="extraArgs">Opzioni in più prima del destinatario (le prove usano <c>-F</c> con un ssh_config proprio).</param>
        public SshTransport(string executable = "ssh", IReadOnlyList<string> extraArgs = null)
        {
            _executable = executable;
            _extraArgs = extraArgs ?? new List<string>();
        }

        public ProcessStartInfo Command(string destination, string remoteCommand)
        {
            var psi = Redirected(_executable);
            foreach (var a in new[] { "-o", "BatchMode=yes", "-o", "ConnectTimeout=10", "-o", "ServerAliveInterval=30" }) psi.ArgumentList.Add(a);
            foreach (var a in _extraArgs) psi.ArgumentList.Add(a);
            psi.ArgumentList.Add(destination);
            psi.ArgumentList.Add(remoteCommand);
            return psi;
        }

        public static ProcessStartInfo Redirected(string fileName) => new(fileName)
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = new UTF8Encoding(false),
            StandardErrorEncoding = new UTF8Encoding(false),
            StandardInputEncoding = new UTF8Encoding(false),
        };
    }

    /// <summary>Le righe di comando per la shell POSIX del server.</summary>
    public static class RemoteShell
    {
        /// <summary>Tra apici singoli; un apice dentro diventa <c>'\''</c>. Niente si espande.</summary>
        public static string Quote(string value) => "'" + value.Replace("'", "'\\''") + "'";
    }
}
