using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.E2e
{
    public sealed record E2eCommitResult(bool Committed, string Sha, string Message, string Reason);

    /// <summary>
    /// The commit after a test run (D21, D26: everything, screenshots included). Only what the run
    /// produced or touched is staged — the test file, its artifacts folder, the site map, the support files
    /// of the tests folder, <c>.gitignore</c> — never <c>git add</c> of everything: the user's own changes
    /// stay out. When the index already holds something else the commit is not made, and it is said.
    /// </summary>
    public static class E2eCommitter
    {
        private static readonly string[] SupportFiles = { "E2eTests.csproj", "E2eSupport.cs", "e2e.runsettings" };
        private static readonly Regex ReportHeading = new(@"^## T\d+ .*[—-]\s*(✅|❌|⚠️)", RegexOptions.Multiline);

        public static E2eCommitResult Commit(E2eRunItem item)
        {
            var gitDir = Repository.Discover(Path.GetDirectoryName(item.TestFile));
            if (gitDir == null)
                return new E2eCommitResult(false, null, null, "il progetto non è un repository git: nessun commit.");

            using var repo = new Repository(gitDir);
            var workdir = repo.Info.WorkingDirectory;
            string Relative(string path) => Path.GetRelativePath(workdir, path).Replace('\\', '/');

            // Something the user staged on their own would end up in this commit.
            var before = repo.RetrieveStatus(new StatusOptions { IncludeUntracked = false });
            var alreadyStaged = before.Where(e => e.State.HasFlag(FileStatus.NewInIndex) || e.State.HasFlag(FileStatus.ModifiedInIndex)
                                                  || e.State.HasFlag(FileStatus.DeletedFromIndex) || e.State.HasFlag(FileStatus.RenamedInIndex))
                                      .Select(e => e.FilePath).ToList();
            if (alreadyStaged.Count > 0)
                return new E2eCommitResult(false, null, null,
                    $"ci sono già modifiche in stage ({string.Join(", ", alreadyStaged.Take(5))}{(alreadyStaged.Count > 5 ? ", …" : "")}): commit dei test non fatto, per non mischiarle.");

            var signature = repo.Config.BuildSignature(DateTimeOffset.Now);
            if (signature == null)
                return new E2eCommitResult(false, null, null,
                    "git non ha un autore configurato: imposta user.name e user.email (git config) e riprova.");

            var testFolder = Path.GetDirectoryName(item.TestFile)!;
            var document = item.Preflight.Document;
            var paths = new List<string> { item.TestFile };
            if (document?.SiteMap != null) paths.Add(Path.Combine(testFolder, document.SiteMap));
            if (document?.Artifacts != null) paths.Add(Path.Combine(testFolder, document.Artifacts));
            paths.AddRange(SupportFiles.Select(f => Path.Combine(testFolder, f)));
            paths.Add(Path.Combine(workdir, ".gitignore"));

            var existing = paths.Select(Path.GetFullPath).Where(p => File.Exists(p) || Directory.Exists(p)).Select(Relative).Distinct().ToList();
            // Ignored files (credentials, bin/obj) are never staged: StageOptions default leaves them out.
            LibGit2Sharp.Commands.Stage(repo, existing);

            var staged = repo.RetrieveStatus(new StatusOptions()).Where(e =>
                e.State.HasFlag(FileStatus.NewInIndex) || e.State.HasFlag(FileStatus.ModifiedInIndex)
                || e.State.HasFlag(FileStatus.DeletedFromIndex) || e.State.HasFlag(FileStatus.RenamedInIndex)).ToList();
            if (staged.Count == 0)
                return new E2eCommitResult(false, null, null, "nessuna modifica da registrare.");

            var message = MessageFor(item);
            var commit = repo.Commit(message, signature, signature);
            return new E2eCommitResult(true, commit.Sha, message, null);
        }

        /// <summary>"test e2e: test-e2e/login.e2e.md — 1 ✅, 1 ❌ (esecuzione 2026-09-27_10-30)", counts from the run's report.md.</summary>
        public static string MessageFor(E2eRunItem item)
        {
            var report = item.RunFolder == null ? null : Path.Combine(item.RunFolder, "report.md");
            var counts = "";
            if (report != null && File.Exists(report))
            {
                var outcomes = ReportHeading.Matches(File.ReadAllText(report)).Select(m => m.Groups[1].Value).ToList();
                var parts = new[] { "✅", "❌", "⚠️" }.Select(o => (o, n: outcomes.Count(x => x == o))).Where(x => x.n > 0).Select(x => $"{x.n} {x.o}");
                counts = string.Join(", ", parts);
            }
            var run = item.RunFolder == null ? "" : $" (esecuzione {Path.GetFileName(item.RunFolder)})";
            return $"test e2e: {item.RelativeTestFile}{(counts.Length > 0 ? " — " + counts : "")}{run}";
        }
    }
}
