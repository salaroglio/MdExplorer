using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services;
using MdExplorer.Features.Services.RemoteFolders;
using MdExplorer.RemoteProtocol;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.RemoteFolders
{
    /// <summary>I serverini accesi, uno per cartella remota (sprint 2026-10-21-Cartella-Remota-Via-SSH, F2-F3).</summary>
    public interface IRemoteSessions
    {
        /// <summary>
        /// Il canale verso il serverino acceso su <paramref name="location"/>: quello già aperto se è vivo, altrimenti uno
        /// nuovo dopo aver attraversato le porte (P10) e, se serve e l'utente l'ha permesso (P13), caricato il serverino.
        /// Eccezioni: <see cref="RemoteUnavailableException"/> (porta chiusa e rimedio),
        /// <see cref="RemoteUploadConsentRequiredException"/> (serve il consenso).
        /// </summary>
        Task<RemoteChannel> OpenAsync(RemoteLocation location, CancellationToken ct = default);

        /// <summary>
        /// Le sottocartelle di una cartella del server, per scegliere che cosa collegare (F3). Un serverino acceso solo per
        /// questo elenco e poi spento. <paramref name="path"/> null = la home dell'utente SSH.
        /// </summary>
        Task<RemoteBrowse> BrowseServerAsync(string host, string path, CancellationToken ct = default);
    }

    public sealed record RemoteBrowse(string Host, string Path, string Home, IReadOnlyList<RemoteEntry> Folders);

    public sealed class RemoteSessions : IRemoteSessions, IDisposable
    {
        private static readonly TimeSpan CommandTimeout = TimeSpan.FromSeconds(60);
        private static readonly TimeSpan HelloTimeout = TimeSpan.FromSeconds(30);

        private readonly IRemoteTransport _transport;
        private readonly Lazy<RemoteAgentBinary> _binary;
        private readonly IRemoteServerSettings _settings;
        private readonly ILogger<RemoteSessions> _logger;
        private readonly ConcurrentDictionary<RemoteLocation, RemoteChannel> _channels = new();
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _opening = new();
        /// <summary>Per destinatario: la home e la cartella del serverino già verificata (porte 1-3 e caricamento fatti).</summary>
        private readonly ConcurrentDictionary<string, (string Home, string Folder)> _prepared = new();

        public RemoteSessions(IRemoteTransport transport, Func<RemoteAgentBinary> binary, IRemoteServerSettings settings, ILogger<RemoteSessions> logger)
        {
            _transport = transport;
            _binary = new Lazy<RemoteAgentBinary>(binary, LazyThreadSafetyMode.ExecutionAndPublication);
            _settings = settings;
            _logger = logger;
        }

        public async Task<RemoteChannel> OpenAsync(RemoteLocation location, CancellationToken ct = default)
        {
            if (_channels.TryGetValue(location, out var open) && open.IsAlive) return open;
            var gate = _opening.GetOrAdd(location.ToString(), _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync(ct);
            try
            {
                if (_channels.TryGetValue(location, out open))
                {
                    if (open.IsAlive) return open;
                    _channels.TryRemove(location, out _);
                    open.Dispose();
                }
                var channel = await StartAsync(location, ct);
                _channels[location] = channel;
                _logger.LogInformation("[Remote] serverino {Version} acceso su {Location}", channel.Hello.Version, location);
                return channel;
            }
            finally { gate.Release(); }
        }

        public async Task<RemoteBrowse> BrowseServerAsync(string host, string path, CancellationToken ct = default)
        {
            var server = RemoteLocation.Of(host, "/");
            var (home, _) = await PrepareAsync(server, ct);
            var at = string.IsNullOrWhiteSpace(path) ? RemoteLocation.Of(host, home) : RemoteLocation.Of(host, path);
            using var channel = await StartAsync(at, ct);
            var entries = (await channel.SendAsync(new RemoteRequest { Op = RemoteOps.List, Path = "" }, ct)).Entries ?? new List<RemoteEntry>();
            return new RemoteBrowse(host, at.Path, home, entries.Where(e => e.IsFolder).ToList());
        }

        private async Task<RemoteChannel> StartAsync(RemoteLocation location, CancellationToken ct)
        {
            var (_, folder) = await PrepareAsync(location, ct);
            var command = $"{folder}/{RemoteAgentBinary.FileName} --root {RemoteShell.Quote(location.Path)}";
            try
            {
                return await RemoteChannel.StartAsync(_transport.Command(location.Destination, command), location, HelloTimeout, ct);
            }
            catch (RemoteUnavailableException ex) when (ex.Gate != RemoteGate.Folder)
            {
                _prepared.TryRemove(location.Destination, out _);   // la prossima volta si riguardano le porte da capo
                throw;
            }
        }

        /// <summary>
        /// Porte 1-3 in un solo giro di ssh (accesso, comandi permessi, cartella del serverino scrivibile), la home
        /// dell'utente e, se nella cartella il serverino non c'è, il caricamento — solo con il consenso (P13). Una volta
        /// per destinatario finché un avvio non fallisce.
        /// </summary>
        private async Task<(string Home, string Folder)> PrepareAsync(RemoteLocation location, CancellationToken ct)
        {
            var destination = location.Destination;
            var binary = _binary.Value;
            var (dir, display) = binary.InstallFolder(_settings.AgentFolder(destination));
            if (_prepared.TryGetValue(destination, out var ready) && ready.Folder == dir) return ready;

            var probe = $"echo mde-ok; printf 'mde-home:%s\\n' \"$HOME\"; mkdir -p {dir} && test -w {dir} && echo mde-writable; test -x {dir}/{RemoteAgentBinary.FileName} && echo mde-present; true";
            var (exit, stdout, stderr) = await RunAsync(_transport.Command(destination, probe), null, ct);
            if (exit == RemoteDiagnosis.SshFailed) throw RemoteDiagnosis.AccessFailure(destination, stderr);
            var lines = stdout.Split('\n').Select(l => l.Trim()).ToList();
            if (!lines.Contains("mde-ok")) throw RemoteDiagnosis.CommandsRefused(destination, (stdout + "\n" + stderr).Trim());
            var home = lines.FirstOrDefault(l => l.StartsWith("mde-home:", StringComparison.Ordinal))?.Substring("mde-home:".Length) ?? "/";
            if (!lines.Contains("mde-writable")) throw RemoteDiagnosis.AgentFolderNotWritable(destination, display, stderr);
            if (!lines.Contains("mde-present"))
            {
                if (!_settings.UploadAllowed(destination))
                    throw new RemoteUploadConsentRequiredException(destination, binary.Size, display, UiText.Say(
                        $"Per usare {destination} MDE deve caricare il suo serverino ({binary.Size / 1024 / 1024.0:0.0} MB, un solo file) in {display}. Nella cartella del progetto remoto non scrive niente.",
                        $"To use {destination} MDE must upload its agent ({binary.Size / 1024 / 1024.0:0.0} MB, a single file) to {display}. It writes nothing in the remote project folder."));
                _logger.LogInformation("[Remote] carico il serverino su {Destination}: {Size} byte in {Folder}", destination, binary.Size, display);
                var tmp = $"{dir}/{RemoteAgentBinary.FileName}.tmp";
                var upload = $"cat > {tmp} && chmod +x {tmp} && mv -f {tmp} {dir}/{RemoteAgentBinary.FileName} && echo mde-uploaded";
                (exit, stdout, stderr) = await RunAsync(_transport.Command(destination, upload), binary.LocalPath, ct);
                if (exit == RemoteDiagnosis.SshFailed) throw RemoteDiagnosis.AccessFailure(destination, stderr);
                if (!stdout.Contains("mde-uploaded", StringComparison.Ordinal)) throw RemoteDiagnosis.AgentFolderNotWritable(destination, display, stderr);
            }
            ready = (home, dir);
            _prepared[destination] = ready;
            return ready;
        }

        private static async Task<(int Exit, string Stdout, string Stderr)> RunAsync(ProcessStartInfo psi, string stdinFile, CancellationToken ct)
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(CommandTimeout);
            using var process = Process.Start(psi) ?? throw new RemoteUnavailableException(RemoteGate.Access, $"{psi.FileName}: process not started");
            var stdout = process.StandardOutput.ReadToEndAsync(timeout.Token);
            var stderr = process.StandardError.ReadToEndAsync(timeout.Token);
            try
            {
                if (stdinFile != null)
                {
                    await using var file = File.OpenRead(stdinFile);
                    await file.CopyToAsync(process.StandardInput.BaseStream, timeout.Token);
                }
                process.StandardInput.Close();
                await process.WaitForExitAsync(timeout.Token);
                return (process.ExitCode, await stdout, await stderr);
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                try { process.Kill(entireProcessTree: true); } catch (Exception) { }
                throw new RemoteUnavailableException(RemoteGate.Access, $"{psi.FileName} {string.Join(' ', psi.ArgumentList)}: nessuna risposta in {CommandTimeout.TotalSeconds:0} s");
            }
            catch (IOException ex)
            {
                // ssh è uscito mentre gli si scriveva (accesso negato prima del caricamento): conta come ha finito
                await process.WaitForExitAsync(CancellationToken.None);
                return (process.ExitCode, await stdout, (await stderr) + "\n" + ex.Message);
            }
        }

        public void Dispose()
        {
            foreach (var channel in _channels.Values) channel.Dispose();
            _channels.Clear();
            _prepared.Clear();
        }
    }
}
