using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands
{
    /// <summary>
    /// Un PlantUML che resta acceso (<c>-pipe -pipedelimitor</c>): Java parte una volta, poi ogni diagramma entra da stdin
    /// ed esce su stdout seguito da un delimitatore. Avviare un Java per diagramma costa mezzo secondo su una macchina
    /// veloce, ma 6-29 secondi dove ogni processo nuovo viene ispezionato (misurato il 23/11/2026 su due PC: documenti
    /// con 3 e 7 diagrammi aperti in 75 e 67 secondi).
    /// <para>Un diagramma alla volta. Un errore del diagramma arriva su stderr («ERROR / riga / messaggio») prima del
    /// delimitatore, e diventa la stessa eccezione del lancio singolo. Dopo un po' di inattività il processo si spegne;
    /// se muore, il prossimo diagramma lo riavvia.</para>
    /// </summary>
    public sealed class PlantumlPipeWorker : IDisposable
    {
        /// <summary>Lungo e improbabile: deve poter stare anche in mezzo ai byte di un PNG senza capitarci per caso.</summary>
        public const string Delimiter = "@@MDE-PLANTUML-FINE-7f3a9c51e2b84d06@@";
        public static readonly TimeSpan IdleShutdown = TimeSpan.FromMinutes(10);
        public static readonly TimeSpan RenderTimeout = TimeSpan.FromSeconds(120);

        private static readonly byte[] DelimiterBytes = Encoding.ASCII.GetBytes(Delimiter);
        private static readonly Regex StartLine = new Regex(@"^[ \t]*@start\w*", RegexOptions.Multiline | RegexOptions.Compiled);
        private static readonly Regex EndLine = new Regex(@"^[ \t]*@end\w*", RegexOptions.Multiline | RegexOptions.Compiled);
        private static readonly Regex FileDirective = new Regex(@"^[ \t]*!(include\w*|import)\b", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);
        private readonly StringBuilder _stderr = new StringBuilder();
        private Process _process;
        private string _startedWith;
        private Timer _idle;
        private bool _disposed;

        /// <summary>Il processo acceso adesso, se c'è (per le prove).</summary>
        internal int? ProcessId { get { var p = _process; try { return p != null && !p.HasExited ? p.Id : (int?)null; } catch (InvalidOperationException) { return null; } } }

        /// <summary>
        /// Un diagramma che il processo acceso può rendere: esattamente un blocco <c>@start…/@end…</c> (con più blocchi
        /// uscirebbero più risultati e la conversazione si sfaserebbe), e nessun <c>!include</c>/<c>!import</c>, che
        /// cercano i file dalla cartella del documento — e quella il processo acceso non la segue.
        /// </summary>
        public static bool CanRender(string plantumlCode)
            => !string.IsNullOrWhiteSpace(plantumlCode)
               && StartLine.Matches(plantumlCode).Count == 1
               && EndLine.Matches(plantumlCode).Count == 1
               && !FileDirective.IsMatch(plantumlCode)
               && !plantumlCode.Contains(Delimiter);

        /// <param name="arguments">Gli argomenti di java senza il delimitatore (li aggiunge qui): se cambiano, il processo si riavvia.</param>
        /// <exception cref="InvalidOperationException">Il diagramma è sbagliato (messaggio di PlantUML), o il processo non ha risposto.</exception>
        public async Task<byte[]> RenderAsync(string javaPath, string arguments, string plantumlCode)
        {
            await _gate.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_disposed) throw new ObjectDisposedException(nameof(PlantumlPipeWorker));
                _idle?.Change(Timeout.Infinite, Timeout.Infinite);
                var key = javaPath + "\n" + arguments;
                if (_process == null || _process.HasExited || _startedWith != key) Start(javaPath, arguments, key);
                lock (_stderr) _stderr.Clear();

                var code = plantumlCode.EndsWith("\n", StringComparison.Ordinal) ? plantumlCode : plantumlCode + "\n";
                var input = new UTF8Encoding(false).GetBytes(code);
                using var timeout = new CancellationTokenSource(RenderTimeout);
                try
                {
                    await _process.StandardInput.BaseStream.WriteAsync(input, 0, input.Length, timeout.Token).ConfigureAwait(false);
                    await _process.StandardInput.BaseStream.FlushAsync(timeout.Token).ConfigureAwait(false);
                    var output = await ReadUntilDelimiterAsync(_process.StandardOutput.BaseStream, timeout.Token).ConfigureAwait(false);

                    // L'errore del diagramma è già su stderr quando arriva il delimitatore; chi lo legge è un altro
                    // filo, gli si lascia il tempo di consegnarlo.
                    var error = ErrorSoFar();
                    for (var i = 0; i < 5 && error == null; i++) { await Task.Delay(10).ConfigureAwait(false); error = ErrorSoFar(); }
                    if (error != null)
                        throw new InvalidOperationException($"PlantUML rendering failed (java exit code 200): {error}");
                    return output;
                }
                catch (OperationCanceledException)
                {
                    Kill();
                    throw new InvalidOperationException($"PlantUML non ha risposto entro {RenderTimeout.TotalSeconds:0} secondi: il processo è stato fermato. {ErrorSoFar(any: true)}");
                }
                catch (IOException ex)
                {
                    var stderr = ErrorSoFar(any: true);
                    Kill();
                    throw new InvalidOperationException($"PlantUML si è chiuso mentre rendeva il diagramma: {ex.Message} {stderr}", ex);
                }
            }
            finally
            {
                if (!_disposed) (_idle ??= new Timer(_ => ShutdownIfIdle())).Change(IdleShutdown, Timeout.InfiniteTimeSpan);
                _gate.Release();
            }
        }

        private void Start(string javaPath, string arguments, string key)
        {
            Kill();
            var startInfo = new ProcessStartInfo(javaPath)
            {
                Arguments = arguments + " -pipedelimitor " + Delimiter,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardErrorEncoding = Encoding.UTF8,
            };
            var process = new Process { StartInfo = startInfo };
            process.ErrorDataReceived += (_, e) => { if (e.Data != null) lock (_stderr) _stderr.AppendLine(e.Data); };
            process.Start();
            process.BeginErrorReadLine();
            _process = process;
            _startedWith = key;
        }

        private string ErrorSoFar(bool any = false)
        {
            string text;
            lock (_stderr) text = _stderr.ToString().Trim();
            if (text.Length == 0) return any ? string.Empty : null;
            return any || text.StartsWith("ERROR", StringComparison.Ordinal) ? text : null;
        }

        /// <summary>Tutto ciò che esce fino al delimitatore; la fine riga che PlantUML mette dopo si scarta.</summary>
        private async Task<byte[]> ReadUntilDelimiterAsync(Stream stdout, CancellationToken ct)
        {
            using var collected = new MemoryStream();
            var buffer = new byte[64 * 1024];
            var searchedUpTo = 0;
            while (true)
            {
                var read = await stdout.ReadAsync(buffer, 0, buffer.Length, ct).ConfigureAwait(false);
                if (read == 0) throw new IOException("lo stdout si è chiuso prima del delimitatore");
                collected.Write(buffer, 0, read);

                var bytes = collected.GetBuffer();
                var length = (int)collected.Length;
                var at = IndexOf(bytes, length, DelimiterBytes, Math.Max(0, searchedUpTo - DelimiterBytes.Length));
                if (at < 0) { searchedUpTo = length; continue; }

                // Dopo il delimitatore PlantUML scrive una fine riga: se non è ancora arrivata la si aspetta, così non
                // resta in testa al diagramma successivo.
                while (length < at + DelimiterBytes.Length + 1 || !EndsWithNewline(bytes, length))
                {
                    read = await stdout.ReadAsync(buffer, 0, buffer.Length, ct).ConfigureAwait(false);
                    if (read == 0) break;
                    collected.Write(buffer, 0, read);
                    bytes = collected.GetBuffer();
                    length = (int)collected.Length;
                }
                var result = new byte[at];
                Array.Copy(bytes, result, at);
                return result;
            }
        }

        private static bool EndsWithNewline(byte[] bytes, int length) => length > 0 && bytes[length - 1] == (byte)'\n';

        private static int IndexOf(byte[] haystack, int length, byte[] needle, int from)
        {
            for (var i = from; i <= length - needle.Length; i++)
            {
                var j = 0;
                while (j < needle.Length && haystack[i + j] == needle[j]) j++;
                if (j == needle.Length) return i;
            }
            return -1;
        }

        private void ShutdownIfIdle()
        {
            if (!_gate.Wait(0)) return;   // sta rendendo: non è inattivo
            try { Kill(); }
            finally { _gate.Release(); }
        }

        private void Kill()
        {
            var process = _process;
            _process = null;
            _startedWith = null;
            if (process == null) return;
            try
            {
                if (!process.HasExited)
                {
                    // Chiudere stdin basta a farlo uscire; se non esce, lo si ferma.
                    try { process.StandardInput.Close(); } catch (IOException) { } catch (InvalidOperationException) { }
                    if (!process.WaitForExit(1500)) process.Kill(entireProcessTree: true);
                }
            }
            catch (InvalidOperationException) { /* già uscito */ }
            catch (System.ComponentModel.Win32Exception) { /* già uscito */ }
            finally { process.Dispose(); }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _idle?.Dispose();
            _gate.Wait(2000);
            try { Kill(); } finally { try { _gate.Release(); } catch (SemaphoreFullException) { } }
        }
    }
}
