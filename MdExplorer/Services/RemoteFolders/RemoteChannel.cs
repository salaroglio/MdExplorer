using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services;
using MdExplorer.Features.Services.RemoteFolders;
using MdExplorer.RemoteProtocol;

namespace MdExplorer.Services.RemoteFolders
{
    /// <summary>
    /// Un serverino acceso su una cartella remota: il processo <c>ssh … mde-remote --root …</c> con una riga JSON per
    /// messaggio su stdin/stdout. Una richiesta alla volta, nell'ordine (il serverino non ne sovrappone). Se una risposta
    /// non arriva — processo morto, richiesta annullata a metà — il canale è guasto e non si riusa: le risposte
    /// successive non si accoppierebbero più alle richieste.
    /// </summary>
    public sealed class RemoteChannel : IDisposable
    {
        private readonly Process _process;
        private readonly StringBuilder _stderr = new();
        private readonly SemaphoreSlim _gate = new(1, 1);
        private int _nextId;
        private volatile bool _broken;

        public RemoteLocation Location { get; }
        public RemoteHello Hello { get; private set; }
        public bool IsAlive => !_broken && !HasExited();

        private RemoteChannel(Process process, RemoteLocation location)
        {
            _process = process; Location = location;
        }

        /// <summary>Avvia il processo e aspetta il saluto (D6): senza saluto non si manda nessuna richiesta.</summary>
        public static async Task<RemoteChannel> StartAsync(ProcessStartInfo psi, RemoteLocation location, TimeSpan helloTimeout, CancellationToken ct)
        {
            var process = RemoteDiagnosis.StartSsh(psi);
            var channel = new RemoteChannel(process, location);
            process.ErrorDataReceived += (_, e) => { if (e.Data != null) lock (channel._stderr) channel._stderr.AppendLine(e.Data); };
            process.BeginErrorReadLine();
            string line;
            using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct))
            {
                timeout.CancelAfter(helloTimeout);
                try { line = await process.StandardOutput.ReadLineAsync(timeout.Token); }
                catch (OperationCanceledException) when (!ct.IsCancellationRequested) { line = null; }
            }
            if (line == null)
            {
                var exited = process.WaitForExit(5000);
                var stderr = channel.Stderr();
                var exitCode = exited ? process.ExitCode : (int?)null;
                channel.Dispose();
                throw RemoteDiagnosis.StartFailure(location, exitCode, stderr);
            }
            RemoteResponse hello;
            try { hello = JsonSerializer.Deserialize(line, RemoteJson.Default.RemoteResponse); }
            catch (JsonException) { hello = null; }
            if (hello?.Hello == null)
            {
                channel.Dispose();
                throw new RemoteUnavailableException(RemoteGate.Channel, UiText.Say(
                    $"{location}: il serverino ha risposto in un modo inatteso al posto del saluto: {Trim(line)}",
                    $"{location}: the agent answered something unexpected instead of its greeting: {Trim(line)}"));
            }
            if (hello.Hello.Protocol != AgentProtocol.Version)
            {
                channel.Dispose();
                throw new RemoteUnavailableException(RemoteGate.Channel, UiText.Say(
                    $"{location}: il serverino parla il protocollo {hello.Hello.Protocol}, MDE il {AgentProtocol.Version}.",
                    $"{location}: the agent speaks protocol {hello.Hello.Protocol}, MDE speaks {AgentProtocol.Version}."));
            }
            channel.Hello = hello.Hello;
            return channel;
        }

        /// <summary>Manda una richiesta e aspetta la sua risposta; un rifiuto del serverino diventa <see cref="RemoteOperationException"/>.</summary>
        public async Task<RemoteResponse> SendAsync(RemoteRequest request, CancellationToken ct = default)
        {
            await _gate.WaitAsync(ct);
            try
            {
                if (!IsAlive) throw Interrupted();
                request.Id = Interlocked.Increment(ref _nextId);
                string line;
                try
                {
                    await _process.StandardInput.WriteLineAsync(JsonSerializer.Serialize(request, RemoteJson.Default.RemoteRequest).AsMemory(), ct);
                    await _process.StandardInput.FlushAsync();
                    line = await _process.StandardOutput.ReadLineAsync(ct);
                }
                catch (OperationCanceledException)
                {
                    Break();   // la risposta arriverebbe dopo, al posto di quella della richiesta seguente
                    throw;
                }
                catch (IOException)
                {
                    Break();
                    throw Interrupted();
                }
                if (line == null) { Break(); throw Interrupted(); }
                var response = JsonSerializer.Deserialize(line, RemoteJson.Default.RemoteResponse);
                if (response == null || response.Id != request.Id)
                {
                    Break();
                    throw new RemoteUnavailableException(RemoteGate.Channel, $"{Location}: risposta fuori ordine ({Trim(line)})");
                }
                if (!response.Ok) throw new RemoteOperationException(response.Error?.Code ?? RemoteErrorCodes.Internal, response.Error?.Message ?? "");
                return response;
            }
            finally { _gate.Release(); }
        }

        public void Dispose()
        {
            _broken = true;
            try
            {
                if (!HasExited())
                {
                    _process.StandardInput.Close();   // stdin chiuso: il serverino esce da solo, e con lui ssh
                    if (!_process.WaitForExit(2000)) _process.Kill(entireProcessTree: true);
                }
            }
            catch (Exception) { /* già uscito */ }
            _process.Dispose();
        }

        private void Break()
        {
            _broken = true;
            try { if (!HasExited()) _process.Kill(entireProcessTree: true); } catch (Exception) { }
        }

        private RemoteUnavailableException Interrupted()
        {
            var exit = HasExited() ? _process.ExitCode.ToString() : "?";
            return new RemoteUnavailableException(RemoteGate.Channel, UiText.Say(
                $"{Location}: il collegamento al serverino si è interrotto (uscita {exit}). {Stderr()}".Trim(),
                $"{Location}: the connection to the agent was interrupted (exit {exit}). {Stderr()}".Trim()));
        }

        private bool HasExited()
        {
            try { return _process.HasExited; } catch (InvalidOperationException) { return true; }
        }

        private string Stderr() { lock (_stderr) return _stderr.ToString().Trim(); }

        private static string Trim(string s) => s.Length > 300 ? s.Substring(0, 300) + "…" : s;
    }
}
