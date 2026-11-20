using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Utilities;

namespace MdExplorer.Features.Services.Fuseki
{
    /// <summary>
    /// Quanto aspettare che Fuseki risponda al ping dopo l'avvio (20/11/2026). Non un tempo fisso: un Fuseki lento ma vivo
    /// scrive righe nel suo log mentre parte, uno bloccato no. Su un PC aziendale (antivirus sul jar e sul TDB2) il primo
    /// avvio a freddo ha impiegato 31 secondi: con il vecchio limite fisso di 30 il servizio lo chiudeva un secondo prima che
    /// fosse pronto. Regola: si aspetta finché il processo è vivo e scrive; ci si ferma dopo <c>silence</c> senza una riga
    /// nuova né un ping riuscito, o comunque dopo <c>max</c>.
    /// </summary>
    internal static class FusekiStartupWait
    {
        /// <param name="sinceStart">L'orologio partito con il processo: su di esso <paramref name="lastOutputMs"/> segna
        /// il momento dell'ultima riga scritta (0 = niente ancora).</param>
        public static async Task WaitAsync(
            Stopwatch sinceStart,
            Func<CancellationToken, Task<bool>> ping,
            Func<int?> exitCode,
            Func<long> lastOutputMs,
            Func<string> lastLine,
            TimeSpan silence,
            TimeSpan max,
            string pingUri,
            CancellationToken ct,
            TimeSpan? pollInterval = null)
        {
            var poll = pollInterval ?? TimeSpan.FromMilliseconds(250);
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                var exited = exitCode();
                if (exited != null)
                    throw new InvalidOperationException(UiText.Say($"Fuseki è terminato durante l'avvio (exit {exited}).", $"Fuseki exited while starting (exit {exited})."));
                if (await ping(ct)) return;

                var elapsed = sinceStart.Elapsed;
                if (elapsed >= max)
                    throw new TimeoutException(UiText.Say(
                        $"Fuseki non è partito in {max.TotalSeconds:0} secondi: ha continuato a scrivere nel log ma {pingUri} non ha mai risposto.",
                        $"Fuseki did not start within {max.TotalSeconds:0} seconds: it kept writing to its log but {pingUri} never answered."));

                // L'ultima prova di vita: l'ultima riga scritta, o l'avvio stesso se non ha ancora scritto niente.
                var quiet = elapsed - TimeSpan.FromMilliseconds(Math.Max(0, lastOutputMs()));
                if (quiet >= silence)
                {
                    var last = lastLine();
                    throw new TimeoutException(UiText.Say(
                        $"Fuseki non ha risposto su {pingUri} e non ha scritto niente negli ultimi {silence.TotalSeconds:0} secondi (avviato da {elapsed.TotalSeconds:0} s; ultima riga: {last ?? "nessuna"}).",
                        $"Fuseki did not answer on {pingUri} and wrote nothing in the last {silence.TotalSeconds:0} seconds (started {elapsed.TotalSeconds:0} s ago; last line: {last ?? "none"})."));
                }
                await Task.Delay(poll, ct);
            }
        }
    }
}
