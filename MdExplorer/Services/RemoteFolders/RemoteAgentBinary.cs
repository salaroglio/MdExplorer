using System;
using System.IO;
using System.Security.Cryptography;
using MdExplorer.Features.Services;

namespace MdExplorer.Services.RemoteFolders
{
    /// <summary>
    /// Il serverino che MDE carica sui server (<c>Binaries/remote/linux-x64/mde-remote</c>, compilato da
    /// <c>MdExplorer.RemoteAgent/publish-linux-x64.sh</c>). Sul server sta in <c>~/.cache/mdexplorer/&lt;impronta&gt;/</c>:
    /// l'impronta è lo SHA256 del file (calcolato qui, dove OpenSSL c'è), così un binario cambiato si ricarica sempre
    /// anche se nessuno alza la versione, e due versioni di MDE sullo stesso server non si pestano i piedi.
    /// </summary>
    public sealed class RemoteAgentBinary
    {
        public const string FileName = "mde-remote";

        public string LocalPath { get; }
        public long Size { get; }
        /// <summary>Primi 16 caratteri esadecimali dello SHA256.</summary>
        public string Fingerprint { get; }

        private RemoteAgentBinary(string localPath, long size, string fingerprint)
        {
            LocalPath = localPath; Size = size; Fingerprint = fingerprint;
        }

        /// <summary>La cartella del serverino sul server, relativa alla home dell'utente SSH.</summary>
        public string InstallFolder => ".cache/mdexplorer/" + Fingerprint;

        public static RemoteAgentBinary Load(string localPath)
        {
            if (!File.Exists(localPath))
                throw new RemoteUnavailableException(RemoteGate.AgentFolder, UiText.Say(
                    $"Il serverino per le cartelle remote non è in questa installazione ({localPath}). Va compilato con MdExplorer.RemoteAgent/publish-linux-x64.sh.",
                    $"The agent for remote folders is not in this installation ({localPath}). Build it with MdExplorer.RemoteAgent/publish-linux-x64.sh."));
            using var stream = File.OpenRead(localPath);
            var hash = Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
            return new RemoteAgentBinary(localPath, new FileInfo(localPath).Length, hash.Substring(0, 16));
        }

        /// <summary>Dove lo mette il pacchetto: accanto agli altri binari del servizio.</summary>
        public static string DefaultLocalPath => Path.Combine(AppContext.BaseDirectory, "Binaries", "remote", "linux-x64", FileName);
    }
}
