using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Ad.Tools.Dal
{
    /// <summary>
    /// Sonda delle attese sui database SQLite (03/11/2026, indagine «apertura del progetto lenta»).
    /// Spenta finché nessuno assegna <see cref="Log"/>: il servizio la accende in sviluppo o con
    /// <c>MDE_PERF_TRACE=1</c>. Accesa, scrive due cose, solo quando superano <see cref="SlowMs"/>:
    /// <list type="bullet">
    /// <item><c>[DB-WAIT]</c>: quanto un <c>BeginTransaction</c> è rimasto fermo ad aspettare il lock
    ///   (System.Data.SQLite apre con BEGIN IMMEDIATE: l'attesa sta tutta lì);</item>
    /// <item><c>[DB-HOLD]</c>: quanto una transazione ha tenuto il lock, da Begin a Commit/Rollback.</item>
    /// </list>
    /// Con il chiamante (i primi frame di MdExplorer), così una riga dice chi aspetta e l'altra chi tiene.
    /// </summary>
    public static class DalDiagnostics
    {
        /// <summary>Dove scrivere. <c>null</c> = sonda spenta (costo: un confronto).</summary>
        public static Action<string> Log { get; set; }

        /// <summary>Sotto questa soglia non si scrive niente.</summary>
        public static int SlowMs { get; set; } = 150;

        internal static bool Enabled => Log != null;

        /// <summary>Le transazioni aperte adesso: chi le ha aperte, su quale DB, da quando. Serve a dire CHI tiene il lock.</summary>
        private static readonly ConcurrentDictionary<object, (string Database, string Opener, long Start, Func<bool> IsActive)> _open
            = new ConcurrentDictionary<object, (string, string, long, Func<bool>)>();

        internal static void Opened(object session, string database, Func<bool> isActive)
            => _open[session] = (database, Caller(), Stopwatch.GetTimestamp(), isActive);

        internal static void Closed(object session) => _open.TryRemove(session, out _);

        private static string OpenTransactions(object except)
        {
            var lines = _open
                .Where(kv => !ReferenceEquals(kv.Key, except))
                .Select(kv => kv.Value)
                .Where(v => { try { return v.IsActive(); } catch { return false; } })
                .OrderBy(v => v.Start)
                .Select(v => $"{v.Database} da {ElapsedMs(v.Start):F0} ms ← {v.Opener}")
                .ToList();
            return lines.Count == 0
                ? "nessuna transazione NHibernate aperta: il lock lo tiene qualcosa fuori da NHibernate (migrazioni, Scheduler, altro processo)"
                : string.Join(" ‖ ", lines);
        }

        internal static double ElapsedMs(long startTimestamp)
            => (Stopwatch.GetTimestamp() - startTimestamp) * 1000.0 / Stopwatch.Frequency;

        /// <summary>
        /// Una attesa di BeginTransaction in corso: se dura più di <see cref="SlowMs"/> un timer fotografa le
        /// transazioni aperte IN QUEL MOMENTO (a attesa finita chi teneva il lock l'avrebbe già lasciato).
        /// </summary>
        internal sealed class WaitProbe : IDisposable
        {
            private readonly object _session;
            private readonly long _start = Stopwatch.GetTimestamp();
            private readonly System.Threading.Timer _timer;
            private volatile string _snapshot;

            internal WaitProbe(object session)
            {
                _session = session;
                _timer = new System.Threading.Timer(_ => { try { _snapshot = OpenTransactions(_session); } catch { } },
                    null, SlowMs, System.Threading.Timeout.Infinite);
            }

            internal void Done(string database)
            {
                _timer.Dispose();
                Report("DB-WAIT", database, ElapsedMs(_start), _snapshot ?? "(fotografia non presa)");
            }

            public void Dispose() => _timer.Dispose();
        }

        internal static void Report(string kind, string database, double ms, string heldBy = null)
        {
            var log = Log;
            if (log == null || ms < SlowMs) return;
            try
            {
                var line = $"[{kind}] {database}: {ms:F0} ms | {Caller()}";
                if (heldBy != null) line += $" | aperte durante l'attesa: {heldBy}";
                log(line);
            }
            catch
            {
                // la sonda non deve mai rompere una transazione
            }
        }

        /// <summary>Una riga già composta, con il chiamante.</summary>
        internal static void Write(string text)
        {
            var log = Log;
            if (log == null) return;
            try { log($"{text} | {Caller()}"); } catch { }
        }

        internal static string DatabaseName(System.Data.Common.DbConnection connection)
        {
            try
            {
                var cs = connection?.ConnectionString;
                if (string.IsNullOrEmpty(cs)) return "?";
                var part = cs.Split(';')
                    .Select(p => p.Split(new[] { '=' }, 2))
                    .FirstOrDefault(kv => kv.Length == 2 && kv[0].Trim().Equals("Data Source", StringComparison.OrdinalIgnoreCase));
                return part == null ? "?" : Path.GetFileName(part[1].Trim());
            }
            catch
            {
                return "?";
            }
        }

        /// <summary>I primi metodi di MdExplorer sullo stack: chi ha aperto o chiuso la transazione.</summary>
        private static string Caller()
        {
            var frames = new StackTrace(3, false).GetFrames()
                .Select(f => f.GetMethod())
                .Where(m => m?.DeclaringType != null)
                .Select(m =>
                {
                    var type = m.DeclaringType;
                    // metodi async: il frame è MoveNext della macchina a stati <Nome>d__N, annidata nel tipo vero
                    if (type.Name.StartsWith("<") && type.DeclaringType != null)
                    {
                        var close = type.Name.IndexOf('>');
                        var name = close > 1 ? type.Name.Substring(1, close - 1) : m.Name;
                        type = type.DeclaringType;
                        return (Ns: type.Namespace ?? string.Empty, Name: $"{type.Name}.{name}");
                    }
                    return (Ns: type.Namespace ?? string.Empty, Name: $"{type.Name}.{m.Name}");
                })
                .ToList();
            var ours = frames
                .Where(x => x.Ns.StartsWith("MdExplorer", StringComparison.Ordinal))
                .Select(x => x.Name)
                .Distinct()
                .Take(3)
                .ToList();
            return string.Join(" < ", ours.Count > 0 ? ours : frames.Select(x => x.Name).Take(3));
        }
    }
}
