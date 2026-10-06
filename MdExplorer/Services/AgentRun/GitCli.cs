using System.Diagnostics;
using System.Text;

namespace MdExplorer.Services.AgentRun
{
    /// <summary>Git nativo, una riga alla volta: il registro dei giri e il documento delle responsabilità si committano così.</summary>
    public static class GitCli
    {
        public static (int Code, string Out, string Err) Run(string cwd, params string[] args)
        {
            var psi = new ProcessStartInfo("git")
            {
                WorkingDirectory = cwd, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8,
            };
            foreach (var a in args) psi.ArgumentList.Add(a);
            psi.Environment["GIT_TERMINAL_PROMPT"] = "0";
            using var p = Process.Start(psi);
            var outTask = p.StandardOutput.ReadToEndAsync();
            var errTask = p.StandardError.ReadToEndAsync();
            if (!p.WaitForExit(60000)) { try { p.Kill(true); } catch { } return (-1, "", "git non ha risposto in 60 secondi"); }
            return (p.ExitCode, outTask.Result, errTask.Result);
        }

        /// <summary>Committa solo <paramref name="pathspec"/> (mai il resto del lavoro della persona) e prova a pubblicarlo.</summary>
        /// <returns>Null se committato e pubblicato; altrimenti cosa non è andato (il commit, se c'è, resta).</returns>
        public static string CommitOnlyAndPush(string projectPath, string pathspec, string message)
        {
            var add = Run(projectPath, "add", "--", pathspec);
            if (add.Code != 0) return "git add: " + add.Err.Trim();
            var commit = Run(projectPath, "commit", "-m", message, "--", pathspec);
            if (commit.Code != 0 && !commit.Out.Contains("nothing to commit") && !commit.Err.Contains("nothing to commit"))
                return "commit: " + (commit.Err.Trim().Length > 0 ? commit.Err.Trim() : commit.Out.Trim());
            var push = Run(projectPath, "push", "--quiet");
            return push.Code != 0 ? "committato ma non pubblicato: " + push.Err.Trim() : null;
        }
    }
}
