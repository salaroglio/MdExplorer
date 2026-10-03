using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.Demo
{
    /// <summary>Cosa è successo quando si è chiesto di preparare l'origin locale del demo.</summary>
    public enum DemoOriginStatus
    {
        /// <summary>Fatto: <c>origin</c> ora è un repository locale su cui chi prova può scrivere.</summary>
        Prepared,

        /// <summary>Già preparato da una apertura precedente: non si tocca di nuovo.</summary>
        AlreadyPrepared,

        /// <summary>Non è il clone del demo (o non è un repository git): <b>non si è toccato niente</b>.</summary>
        NotTheDemo,

        /// <summary>Doveva essere preparato e non lo è stato: <see cref="DemoOriginOutcome.Error"/> dice perché.</summary>
        Failed,
    }

    public sealed class DemoOriginOutcome
    {
        public DemoOriginStatus Status { get; init; }
        public string Origin { get; init; }
        public string Error { get; init; }
    }

    /// <summary>
    /// Il demo della città degli agenti ha bisogno di un <c>origin</c> su cui chi lo prova può scrivere:
    /// «Autorizza» pubblica sul ramo principale di <c>origin</c>, e il clone di GitHub è di sola lettura.
    /// <para>
    /// ⚠️ <b>Solo per il demo.</b> Questo non è un comportamento dei progetti: parte unicamente se la richiesta
    /// di apertura lo chiede (il tour «Crea progetto demo», legato al percorso del clone) <b>e</b> il progetto è
    /// davvero un clone del repository del demo (<c>origin</c> uguale all'indirizzo noto). In ogni altro caso
    /// non si legge né si scrive nulla di più di un <c>git remote get-url</c>.
    /// </para>
    /// <para>
    /// Cosa fa: copia in locale il repository in un fratello della cartella (<c>&lt;nome&gt;.origin.git</c>), rinomina
    /// l'<c>origin</c> di GitHub in <c>upstream</c> (non si perde: si può ancora aggiornare il demo) e punta
    /// <c>origin</c> alla copia. Lascia un segno in <c>.git/config</c> (<c>mde.demoOrigin</c>), fuori dai file
    /// tracciati, così non compare fra le modifiche da committare.
    /// </para>
    /// </summary>
    public interface IDemoOriginPreparer
    {
        DemoOriginOutcome Prepare(string projectPath);
    }

    public class DemoOriginPreparer : IDemoOriginPreparer
    {
        /// <summary>L'unico repository che si riconosce come «il demo».</summary>
        public const string DemoRepositoryPath = "salaroglio/mdexplorer-demo";

        public const string MarkerKey = "mde.demoOrigin";

        private readonly ILogger<DemoOriginPreparer> _logger;

        public DemoOriginPreparer(ILogger<DemoOriginPreparer> logger) => _logger = logger;

        /// <summary>Vero se <paramref name="originUrl"/> è il repository pubblico del demo (https o ssh, con o senza .git).</summary>
        public static bool IsDemoRepositoryUrl(string originUrl)
        {
            var u = (originUrl ?? string.Empty).Trim().ToLowerInvariant();
            if (u.EndsWith("/")) u = u.TrimEnd('/');
            if (u.EndsWith(".git")) u = u.Substring(0, u.Length - 4);
            return u == "https://github.com/" + DemoRepositoryPath
                || u == "git@github.com:" + DemoRepositoryPath
                || u == "ssh://git@github.com/" + DemoRepositoryPath;
        }

        public DemoOriginOutcome Prepare(string projectPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(projectPath) || !Directory.Exists(Path.Combine(projectPath, ".git")))
                    return Skip(projectPath, "non è un repository git");

                if (Git(projectPath, out var marker, "config", "--local", "--get", MarkerKey) == 0
                    && marker.Trim() == "true")
                    return new DemoOriginOutcome { Status = DemoOriginStatus.AlreadyPrepared, Origin = CurrentOrigin(projectPath) };

                var origin = CurrentOrigin(projectPath);
                if (!IsDemoRepositoryUrl(origin))
                    return Skip(projectPath, $"origin non è il repository del demo ({origin ?? "assente"})");

                var branch = RequireGit(projectPath, "rev-parse", "--abbrev-ref", "HEAD").Trim();
                if (string.IsNullOrEmpty(branch) || branch == "HEAD")
                    throw new InvalidOperationException("il clone non è su un ramo (HEAD staccato).");

                var full = Path.GetFullPath(projectPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var bare = full + ".origin.git";
                if (Directory.Exists(bare) || File.Exists(bare))
                    throw new InvalidOperationException(
                        $"esiste già '{bare}': non lo sovrascrivo. Spostalo o cancellalo e riapri il demo.");

                // Tutto locale: la copia parte dal clone che c'è, senza rete.
                RequireGit(null, "clone", "--bare", "--quiet", full, bare);
                RequireGit(full, "remote", "rename", "origin", "upstream");
                RequireGit(full, "remote", "add", "origin", bare);
                RequireGit(full, "fetch", "--quiet", "origin");
                RequireGit(full, "branch", "--set-upstream-to=origin/" + branch, branch);
                // I posti di lavoro degli agenti, la revisione e «Autorizza» partono da origin/HEAD.
                RequireGit(full, "remote", "set-head", "origin", branch);
                RequireGit(full, "config", "--local", MarkerKey, "true");

                _logger.LogInformation("[DemoOrigin] '{Path}': origin → '{Bare}', GitHub resta come 'upstream'.", full, bare);
                return new DemoOriginOutcome { Status = DemoOriginStatus.Prepared, Origin = bare };
            }
            catch (Exception ex)
            {
                // Mai silenzioso: la persona deve sapere che gli agenti non potranno consegnare.
                _logger.LogError(ex, "[DemoOrigin] preparazione dell'origin locale fallita per '{Path}'", projectPath);
                return new DemoOriginOutcome { Status = DemoOriginStatus.Failed, Error = ex.Message };
            }
        }

        private DemoOriginOutcome Skip(string projectPath, string why)
        {
            _logger.LogInformation("[DemoOrigin] '{Path}' non è il clone del demo ({Why}): nessuna modifica.", projectPath, why);
            return new DemoOriginOutcome { Status = DemoOriginStatus.NotTheDemo };
        }

        private static string CurrentOrigin(string path)
            => Git(path, out var url, "remote", "get-url", "origin") == 0 ? url.Trim() : null;

        private static string RequireGit(string workDir, params string[] args)
        {
            var code = Git(workDir, out var output, args);
            if (code != 0)
                throw new InvalidOperationException($"git {string.Join(' ', args)} → codice {code}: {output.Trim()}");
            return output;
        }

        private static int Git(string workDir, out string output, params string[] args)
        {
            var psi = new ProcessStartInfo("git")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };
            if (!string.IsNullOrEmpty(workDir)) psi.WorkingDirectory = workDir;
            foreach (var a in args) psi.ArgumentList.Add(a);
            psi.Environment["GIT_TERMINAL_PROMPT"] = "0";

            using var p = Process.Start(psi) ?? throw new InvalidOperationException("git non è partito: è installato?");
            var stderr = p.StandardError.ReadToEndAsync();
            var stdout = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            output = stdout + stderr.Result;
            return p.ExitCode;
        }
    }
}
