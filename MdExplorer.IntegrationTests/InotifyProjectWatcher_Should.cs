using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using MdExplorer.Services.FileSystemWatcherManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Sprint 2026-10-21-Watcher-Selettivo-E-Gitignore, W3: il watcher Linux sorveglia solo le cartelle che la regola
    /// accetta, con eventi veri del file system, e dà gli stessi eventi di FileSystemWatcher.
    /// </summary>
    [TestClass]
    public class InotifyProjectWatcher_Should
    {
        private string _root;
        private readonly HashSet<string> _excluded = new() { "node_modules", ".git", "bozze" };
        private InotifyProjectWatcher _watcher;
        private readonly ConcurrentQueue<string> _events = new();

        [TestInitialize]
        public void Setup()
        {
            if (!OperatingSystem.IsLinux()) Assert.Inconclusive("inotify è di Linux");
            _root = Path.Combine(Path.GetTempPath(), "mde-inotify", Guid.NewGuid().ToString("N"));
            foreach (var d in new[] { "docs/a", "docs/b", "node_modules/x/y/z", ".git/objects/aa", "bozze/vecchie" })
                Directory.CreateDirectory(Path.Combine(_root, d));
        }

        [TestCleanup]
        public void Cleanup()
        {
            _watcher?.Dispose();
            try { Directory.Delete(_root, true); } catch { }
        }

        private void Start()
        {
            _watcher = new InotifyProjectWatcher(_root, dir => !_excluded.Contains(Path.GetFileName(dir)));
            _watcher.Created += (s, e) => _events.Enqueue("created " + Rel(e.FullPath));
            _watcher.Changed += (s, e) => _events.Enqueue("changed " + Rel(e.FullPath));
            _watcher.Deleted += (s, e) => _events.Enqueue("deleted " + Rel(e.FullPath));
            _watcher.Renamed += (s, e) => _events.Enqueue($"renamed {Rel(e.OldFullPath)} -> {Rel(e.FullPath)}");
            _watcher.Error += (s, e) => _events.Enqueue("error " + e.GetException().Message);
            _watcher.Start();
        }

        private string Rel(string full) => Path.GetRelativePath(_root, full).Replace('\\', '/');

        /// <summary>Aspetta che arrivi l'evento (o scade e fallisce con quelli arrivati).</summary>
        private void Expect(string expected)
        {
            var until = DateTime.UtcNow.AddSeconds(5);
            while (DateTime.UtcNow < until)
            {
                if (_events.Contains(expected)) return;
                Thread.Sleep(20);
            }
            Assert.Fail($"atteso «{expected}», arrivati: {string.Join(" | ", _events)}");
        }

        private void Settle() => Thread.Sleep(400);

        [TestMethod]
        public void Watch_only_the_folders_the_rule_accepts()
        {
            Start();
            Assert.AreEqual(4, _watcher.WatchedDirectories, "radice, docs, docs/a, docs/b — niente node_modules, .git, bozze");
        }

        [TestMethod]
        public void Raise_created_changed_deleted_and_renamed_like_FileSystemWatcher()
        {
            Start();
            var f = Path.Combine(_root, "docs", "a", "nota.md");
            File.WriteAllText(f, "uno");
            Expect("created docs/a/nota.md");
            File.AppendAllText(f, " due");
            Expect("changed docs/a/nota.md");
            File.Move(f, Path.Combine(_root, "docs", "b", "nota2.md"));
            Expect("renamed docs/a/nota.md -> docs/b/nota2.md");
            File.Delete(Path.Combine(_root, "docs", "b", "nota2.md"));
            Expect("deleted docs/b/nota2.md");
        }

        [TestMethod]
        public void Say_nothing_inside_excluded_folders_and_treat_moves_across_the_border_as_created_or_deleted()
        {
            Start();
            File.WriteAllText(Path.Combine(_root, "node_modules", "x", "pkg.js"), "x");
            File.WriteAllText(Path.Combine(_root, "bozze", "vecchie", "b.md"), "x");
            Settle();
            Assert.IsFalse(_events.Any(e => e.Contains("node_modules/x") || e.Contains("bozze/vecchie")), string.Join(" | ", _events));

            File.Move(Path.Combine(_root, "bozze", "vecchie", "b.md"), Path.Combine(_root, "docs", "b.md"));
            Expect("created docs/b.md");
            File.Move(Path.Combine(_root, "docs", "b.md"), Path.Combine(_root, "bozze", "b.md"));
            Expect("deleted docs/b.md");
        }

        [TestMethod]
        public void Watch_new_folders_unless_excluded()
        {
            Start();
            var nuova = Path.Combine(_root, "docs", "nuova");
            Directory.CreateDirectory(nuova);
            Expect("created docs/nuova");
            Settle();
            File.WriteAllText(Path.Combine(nuova, "c.md"), "x");
            Expect("created docs/nuova/c.md");

            var esclusa = Path.Combine(_root, "docs", "node_modules");
            Directory.CreateDirectory(esclusa);
            Expect("created docs/node_modules");
            Settle();
            File.WriteAllText(Path.Combine(esclusa, "p.js"), "x");
            Settle();
            Assert.IsFalse(_events.Contains("created docs/node_modules/p.js"));
            Assert.AreEqual(5, _watcher.WatchedDirectories, "+ docs/nuova, non docs/node_modules");
        }

        [TestMethod]
        public void Follow_a_renamed_folder_with_its_files()
        {
            Start();
            Directory.Move(Path.Combine(_root, "docs", "a"), Path.Combine(_root, "docs", "alfa"));
            Expect("renamed docs/a -> docs/alfa");
            File.WriteAllText(Path.Combine(_root, "docs", "alfa", "d.md"), "x");
            Expect("created docs/alfa/d.md");
            Assert.AreEqual(4, _watcher.WatchedDirectories);
        }

        [TestMethod]
        public void Resync_when_the_rules_change_and_stop_on_dispose()
        {
            Start();
            _excluded.Add("b");
            _watcher.Resync();
            Assert.AreEqual(3, _watcher.WatchedDirectories, "docs/b ora è esclusa");
            File.WriteAllText(Path.Combine(_root, "docs", "b", "x.md"), "x");
            Settle();
            Assert.IsFalse(_events.Contains("created docs/b/x.md"));

            _excluded.Remove("bozze");
            _watcher.Resync();
            Assert.AreEqual(5, _watcher.WatchedDirectories, "+ bozze, bozze/vecchie");

            _watcher.Dispose();
            File.WriteAllText(Path.Combine(_root, "docs", "dopo.md"), "x");
            Settle();
            Assert.IsFalse(_events.Contains("created docs/dopo.md"));
        }
    }
}
