using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.Git
{
    /// <summary>
    /// Una riga di log per ogni git che va <b>in rete</b> (fetch, push, pull, clone, ls-remote, submodule),
    /// da qualunque punto parta. Serve a rispondere a «chi ha aperto il login del credential manager?»:
    /// la riga dice chi l'ha chiamato, verso dove, quanto è durato e com'è finito. Un comando di rete
    /// che dura decine di secondi è quasi sempre un git rimasto ad aspettare il login nel browser.
    /// <para>
    /// Statico perché anche <see cref="AgentRun.GitCli"/> è statico: il logger arriva da <c>Startup.Configure</c>.
    /// </para>
    /// </summary>
    public static class GitNetworkLog
    {
        private static readonly HashSet<string> NetworkVerbs = new(StringComparer.Ordinal)
            { "fetch", "push", "pull", "clone", "ls-remote", "submodule" };

        /// <summary>Oltre questa durata la riga diventa un avviso: probabile attesa di un login.</summary>
        private static readonly TimeSpan SlowThreshold = TimeSpan.FromSeconds(5);

        private static ILogger _logger;

        public static void SetLogger(ILogger logger) => _logger = logger;

        /// <summary>Il verbo git, saltando le opzioni globali (<c>-c chiave=valore</c>, <c>-C cartella</c>).</summary>
        public static bool IsNetwork(IReadOnlyList<string> args)
        {
            for (var i = 0; i < args.Count; i++)
            {
                var a = args[i];
                if (a == "-c" || a == "-C") { i++; continue; }
                if (a.StartsWith("-")) continue;
                return NetworkVerbs.Contains(a);
            }
            return false;
        }

        /// <summary>
        /// Da chiamare PRIMA del primo await del runner: finché il codice è sincrono lo stack contiene
        /// ancora chi ha chiesto il comando. Null se il comando non va in rete (niente costo per gli altri).
        /// </summary>
        public static string Caller(IReadOnlyList<string> args)
        {
            if (!IsNetwork(args)) return null;
            var frames = new StackTrace().GetFrames()
                .Select(f => f.GetMethod())
                .Where(m => m != null && m.DeclaringType != null)
                .Select(m => Describe(m.DeclaringType, m.Name))
                .Where(n => n != null)
                .Distinct()
                .Take(4);
            return string.Join(" ← ", frames);
        }

        public static void Done(string caller, string workingDirectory, IReadOnlyList<string> args, int exitCode, string stderr, TimeSpan elapsed)
        {
            if (caller == null || _logger == null) return;
            var firstError = (stderr ?? string.Empty).Split('\n').Select(l => l.Trim()).FirstOrDefault(l => l.Length > 0) ?? string.Empty;
            var level = elapsed >= SlowThreshold || exitCode != 0 ? LogLevel.Warning : LogLevel.Information;
            _logger.Log(level, "[git-rete] git {Args} in {Dir} → exit {Exit} in {Ms} ms {Err} | chiamato da {Caller}",
                string.Join(" ", args), workingDirectory, exitCode, (long)elapsed.TotalMilliseconds, firstError, caller);
        }

        /// <summary>
        /// «Tipo.Metodo» leggibile, togliendo la macchina a stati degli async (<c>&lt;FetchAsync&gt;d__5.MoveNext</c>
        /// → <c>Classe.FetchAsync</c>). Null per i frame che non dicono niente: framework, runner, questo file.
        /// </summary>
        private static string Describe(Type type, string method)
        {
            var owner = type;
            if (method == "MoveNext" && type.DeclaringType != null && type.Name.StartsWith("<"))
            {
                owner = type.DeclaringType;
                method = type.Name.Substring(1, type.Name.IndexOf('>') - 1);
            }
            while (owner.DeclaringType != null && owner.Name.StartsWith("<")) owner = owner.DeclaringType;
            var ns = owner.Namespace ?? string.Empty;
            if (!ns.StartsWith("MdExplorer")) return null;
            if (owner == typeof(GitNetworkLog) || owner == typeof(NativeGitRunner) || owner.Name == "GitCli") return null;
            return $"{owner.Name}.{method}";
        }
    }
}
