using System;
using System.Threading;
using System.Threading.Tasks;
using SocketIOClient;

namespace MdExplorer.Services
{
    /// <summary>
    /// Tiene collegato un socket verso il relay (chat di gruppo, città degli agenti) e lo chiude davvero.
    /// <para>
    /// SocketIOClient 4 ha una riconnessione sua che NON si può fermare: né <c>DisconnectAsync</c> né <c>Dispose</c>
    /// fermano il ciclo partito dopo una caduta (misurato il 21/11/2026 e letto nel sorgente: nessun token, nessun
    /// flag). Un socket chiuso mentre riprovava si ricollegava da solo al ritorno del relay. Per questo il socket va
    /// creato con <c>Reconnection = false</c> e i tentativi li fa questa classe: a passo fisso, come la 3.1.1, e
    /// si fermano quando la si chiude.
    /// </para>
    /// <para>
    /// Come prima: dopo una caduta della rete o del relay si riprova per sempre; dopo un'espulsione decisa dal relay
    /// (<c>io server disconnect</c>) non si riprova.
    /// </para>
    /// </summary>
    internal sealed class RelaySocketKeeper : IDisposable
    {
        private readonly SocketIOClient.SocketIO _socket;
        private readonly TimeSpan _retryEvery;
        private readonly Action<Exception> _onConnectFailed;
        private readonly Action<int> _onRetry;
        private readonly CancellationTokenSource _cts = new();
        private volatile bool _closedByServer;
        private int _disposed;

        /// <param name="socket">Creato con <c>Reconnection = false</c>.</param>
        /// <param name="retryEvery">Ogni quanto si riprova quando il socket non è collegato.</param>
        /// <param name="onConnectFailed">Un tentativo non è riuscito (relay spento, rifiuto all'apertura).</param>
        /// <param name="onRetry">Sta per partire un nuovo tentativo dopo N falliti di fila.</param>
        public RelaySocketKeeper(SocketIOClient.SocketIO socket, TimeSpan retryEvery, Action<Exception> onConnectFailed, Action<int> onRetry = null)
        {
            if (socket.Options.Reconnection)
                throw new ArgumentException("Il socket va creato con Reconnection = false: la riconnessione di SocketIOClient 4 non si può fermare, la fa RelaySocketKeeper.", nameof(socket));
            _socket = socket;
            _retryEvery = retryEvery;
            _onConnectFailed = onConnectFailed;
            _onRetry = onRetry;
            _socket.OnConnected += (s, e) =>
            {
                _closedByServer = false;
                // Un tentativo già in volo al momento della chiusura può ancora riuscire: lo si richiude.
                if (_cts.IsCancellationRequested) _ = CloseSocketAsync();
            };
            _socket.OnDisconnected += (s, reason) =>
            {
                if (string.Equals(reason, DisconnectReason.IOServerDisconnect, StringComparison.OrdinalIgnoreCase)) _closedByServer = true;
            };
        }

        /// <summary>Parte il primo tentativo e, da lì, il presidio. Non aspetta la connessione.</summary>
        public void Start() => _ = RunAsync();

        private async Task RunAsync()
        {
            var ct = _cts.Token;
            var failures = 0;
            while (!ct.IsCancellationRequested)
            {
                if (!_socket.Connected && !_closedByServer)
                {
                    if (failures > 0) _onRetry?.Invoke(failures);
                    try
                    {
                        await _socket.ConnectAsync(ct).ConfigureAwait(false);
                        failures = 0;
                    }
                    catch (Exception) when (ct.IsCancellationRequested) { break; }
                    catch (Exception ex)
                    {
                        failures++;
                        try { _onConnectFailed?.Invoke(ex); } catch { /* chi ascolta non deve fermare il presidio */ }
                    }
                }
                try { await Task.Delay(_retryEvery, ct).ConfigureAwait(false); }
                catch (OperationCanceledException) { break; }
            }
        }

        /// <summary>Ferma i tentativi e chiude il socket. Da qui in poi non si ricollega più.</summary>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1) return;
            _cts.Cancel();
            _ = CloseSocketAsync();
        }

        private async Task CloseSocketAsync()
        {
            try { await _socket.DisconnectAsync().ConfigureAwait(false); } catch { /* best effort */ }
            try { _socket.Dispose(); } catch { /* best effort */ }
        }
    }
}
