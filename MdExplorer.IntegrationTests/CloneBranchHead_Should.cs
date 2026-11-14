using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.Git.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Chi clona un ramo preciso lavora su quel ramo: <c>origin/HEAD</c> deve seguirlo.
    /// <para>
    /// git lo lascia sul ramo predefinito del remoto, e da lì nascono i posti di lavoro degli agenti, la richiesta di
    /// revisione e l'«Autorizza». Il demo in inglese vive nel ramo <c>en</c> e il predefinito è <c>main</c> (italiano):
    /// gli agenti del clone inglese leggevano i documenti italiani, e il loro risultato citava
    /// <c>caso-studio/03-piano-del-pilota.md</c> in un progetto dove quel file non c'era.
    /// </para>
    /// <para>Richiede <c>git</c> nel PATH.</para>
    /// </summary>
    [TestClass]
    public class CloneBranchHead_Should
    {
        [TestMethod]
        public async Task Point_origin_HEAD_to_the_branch_that_was_asked_for()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var origin = MakeRemoteWithTwoBranches(ctx.Factory.DataDir, "remoto-due-rami");
            var target = Path.Combine(ctx.Factory.DataDir, "cloni", "clone-en");
            using var scope = ctx.Factory.Services.CreateScope();
            var git = scope.ServiceProvider.GetRequiredService<IModernGitService>();

            var result = await git.CloneAsync(origin, target, "en", useSavedToken: false);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.AreEqual("en", Git(target, "branch --show-current").Out.Trim(), "il clone è sul ramo chiesto");
            Assert.AreEqual("refs/remotes/origin/en", Git(target, "symbolic-ref refs/remotes/origin/HEAD").Out.Trim(),
                "il ramo di partenza degli agenti è quello su cui lavori, non quello predefinito del remoto");
        }

        [TestMethod]
        public async Task Leave_origin_HEAD_alone_when_no_branch_was_asked_for()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var origin = MakeRemoteWithTwoBranches(ctx.Factory.DataDir, "remoto-predefinito");
            var target = Path.Combine(ctx.Factory.DataDir, "cloni", "clone-default");
            using var scope = ctx.Factory.Services.CreateScope();
            var git = scope.ServiceProvider.GetRequiredService<IModernGitService>();

            var result = await git.CloneAsync(origin, target, null, useSavedToken: false);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.AreEqual("refs/remotes/origin/main", Git(target, "symbolic-ref refs/remotes/origin/HEAD").Out.Trim(),
                "senza un ramo esplicito resta il predefinito del remoto");
        }

        // ---- infrastruttura ----

        private static string MakeRemoteWithTwoBranches(string baseDir, string name)
        {
            var work = Path.Combine(baseDir, "sorgenti", name);
            Directory.CreateDirectory(work);
            Git(work, "init -b main");
            Git(work, "config user.email carlo@test.local");
            Git(work, "config user.name Test");
            Git(work, "config commit.gpgsign false");
            File.WriteAllText(Path.Combine(work, "README.md"), "# main\n");
            Git(work, "add -A");
            Git(work, "commit -m base");
            Git(work, "checkout -b en");
            File.WriteAllText(Path.Combine(work, "README.md"), "# en\n");
            Git(work, "add -A");
            Git(work, "commit -m english");

            var bare = Path.Combine(baseDir, "remoti", name + ".git");
            Directory.CreateDirectory(bare);
            Git(bare, "init --bare");
            Git(bare, "symbolic-ref HEAD refs/heads/main");
            Git(work, $"remote add origin \"{bare}\"");
            Git(work, "push origin main en");
            return bare;
        }

        private static (int Code, string Out) Git(string cwd, string args)
        {
            var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git", Arguments = args, WorkingDirectory = cwd,
                    UseShellExecute = false, RedirectStandardOutput = true,
                    RedirectStandardError = true, CreateNoWindow = true,
                }
            };
            p.StartInfo.EnvironmentVariables["GIT_TERMINAL_PROMPT"] = "0";
            p.Start();
            var o = p.StandardOutput.ReadToEnd();
            p.StandardError.ReadToEnd();
            p.WaitForExit(60000);
            return (p.ExitCode, o);
        }

        private static bool GitAvail() => Git(Path.GetTempPath(), "--version").Code == 0;
    }
}
