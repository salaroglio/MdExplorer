using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services.RemoteFolders;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.RemoteFolders
{
    /// <summary>I serverini accesi, uno per cartella remota (sprint 2026-10-21-Cartella-Remota-Via-SSH, F2).</summary>
    public interface IRemoteSessions
    {
        /// <summary>
        /// Il canale verso il serverino acceso su <paramref name="location"/>: quello già aperto se è vivo, altrimenti uno
        /// nuovo dopo aver attraversato le porte (P10) e, se serve, caricato il serverino. Eccezione con la porta chiusa e
        /// il rimedio (<see cref="RemoteUnavailableException"/>).
        /// </summary>
        Task<RemoteChannel> OpenAsync(RemoteLocation location, CancellationToken ct = default);
    }

    public sealed class RemoteSessions : IRemoteSessions, IDisposable
    {
        private static readonly TimeSpan CommandTimeout = TimeSpan.FromSeconds(60);
        private static readonly TimeSpan HelloTimeout = TimeSpan.FromSeconds(30);

        private readonly IRemoteTransport _transport;
        private readonly Lazy<RemoteAgentBinary> _binary;
        private readonly ILogger<RemoteSessions> _logger;
        private readonly ConcurrentDictionary<RemoteLocation, RemoteChannel> _channels = new();
        private readonly ConcurrentDictionary<RemoteLocation, SemaphoreSlim> _opening = new();

        public RemoteSessions(IRemoteTransport transport, Func<RemoteAgentBinary> binary, ILogger<RemoteSessions> logger)
        {
            _transport = transport;
            _binary = new Lazy<RemoteAgentBinary>(binary, LazyThreadSafetyMode.ExecutionAndPublication);
            _logger = logger;
        }

        public async Task<RemoteChannel> OpenAsync(RemoteLocation location, CancellationToken ct = default)
        {
            if (_channels.TryGetValue(location, out var open) && open.IsAlive) return open;
            var gate = _opening.GetOrAdd(location, _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync(ct);
            try
            {
                if (_channels.TryGetValue(location, out open))
                {
                    if (open.IsAlive) return open;
                    _channels.TryRemove(location, out _);
                    open.Dispose();
                }
                var binary = _binary.Value;
                await PrepareAsync(location, binary, ct);
                var command = $"\"$HOME\"/{RemoteShell.Quote(binary.InstallFolder + "/" + RemoteAgentBinary.FileName)} --root {RemoteShell.Quote(location.Path)}";
                var channel = await RemoteChannel.StartAsync(_transport.Command(location.Destination, command), location, HelloTimeout, ct);
                _channels[location] = channel;
                _logger.LogInformation("[Remote] serverino {Version} acceso su {Location}", channel.Hello.Version, location);
                return channel;
            }
            finally { gate.Release(); }
        }

        /// <summary>
        /// Porte 1-3 in un solo giro di ssh (accesso, comandi permessi, cartella del serverino scrivibile) e caricamento
        /// del serverino se nella sua cartella non c'è. Le porte 4-5 (esecuzione, cartella remota) le dice l'avvio.
        /// </summary>
        private async Task PrepareAsync(RemoteLocation location, RemoteAgentBinary binary, CancellationToken ct)
        {
            var dir = $"\"$HOME\"/{RemoteShell.Quote(binary.InstallFolder)}";
            var probe = $"echo mde-ok; mkdir -p {dir} && test -w {dir} && echo mde-writable; test -x {dir}/{RemoteAgentBinary.FileName} && echo mde-present; true";
            var (exit, stdout, stderr) = await RunAsync(_transport.Command(location.Destination, probe), null, ct);
            if (exit == RemoteDiagnosis.SshFailed) throw RemoteDiagnosis.AccessFailure(location.Destination, stderr);
            var lines = stdout.Split('\n').Select(l => l.Trim()).ToHashSet(StringComparer.Ordinal);
            if (!lines.Contains("mde-ok")) throw RemoteDiagnosis.CommandsRefused(location.Destination, (stdout + "\n" + stderr).Trim());
            if (!lines.Contains("mde-writable")) throw RemoteDiagnosis.AgentFolderNotWritable(location.Destination, binary.InstallFolder, stderr);
            if (lines.Contains("mde-present")) return;

            // P13: che cosa si carica e dove, nel log (la finestra che lo chiede all'utente arriva con la UI, F3)
            _logger.LogInformation("[Remote] carico il serverino su {Destination}: {Size} byte in ~/{Folder}", location.Destination, binary.Size, binary.InstallFolder);
            var tmp = $"{dir}/{RemoteAgentBinary.FileName}.tmp";
            var upload = $"cat > {tmp} && chmod +x {tmp} && mv -f {tmp} {dir}/{RemoteAgentBinary.FileName} && echo mde-uploaded";
            (exit, stdout, stderr) = await RunAsync(_transport.Command(location.Destination, upload), binary.LocalPath, ct);
            if (exit == RemoteDiagnosis.SshFailed) throw RemoteDiagnosis.AccessFailure(location.Destination, stderr);
            if (!stdout.Contains("mde-uploaded", StringComparison.Ordinal)) throw RemoteDiagnosis.AgentFolderNotWritable(location.Destination, binary.InstallFolder, stderr);
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
        }
    }
}
