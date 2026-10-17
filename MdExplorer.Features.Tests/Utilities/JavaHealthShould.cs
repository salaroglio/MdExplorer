using System.Collections.Generic;
using System.IO;
using Ad.Tools.Dal.Abstractions.Interfaces;
using MdExplorer.Features.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Utilities
{
    /// <summary>
    /// Il controllo di salute di Java (12/10/2026), con un locator finto: nessun processo, nessun DB. Il caso di partenza è quello
    /// di Windows dell'11/10: JavaPath = JRE 8 di un altro programma scritto dalla scoperta automatica, Temurin 21 altrove.
    /// </summary>
    [TestClass]
    public class JavaHealthShould
    {
        private sealed class MemoryStore : IJavaSettingsStore
        {
            public string Path; public string Source; public int Writes;
            public (string Path, string Source) Read() => (Path, Source);
            public void Write(string path, string source) { Path = path; Source = source; Writes++; }
        }

        private sealed class FakeLocator : IJavaLocator
        {
            public readonly Dictionary<string, int?> Versions = new();
            public string Best;
            public string ResolveJavaPath() => null;
            public string ResolveJavaPath(ISessionDB session) => null;
            public JavaProbe Probe(string javaPath) => Versions.TryGetValue(javaPath, out var major)
                ? new JavaProbe { Path = javaPath, IsRunnable = true, Major = major, Raw = $"openjdk version \"{major}\"" }
                : new JavaProbe { Path = javaPath, IsRunnable = false, Error = "finto: non eseguibile" };
            public JavaProbe FindBest(int minimumMajor) => Best == null ? null : Probe(Best);
        }

        private static string Existing(string name)
        {
            var p = Path.Combine(Path.GetTempPath(), "mde-javahealth-" + name);
            File.WriteAllText(p, "x");
            return p;
        }

        [TestMethod]
        public void Keep_a_suitable_java()
        {
            var java21 = Existing("java21");
            var store = new MemoryStore { Path = java21, Source = JavaPathSources.Auto };
            var locator = new FakeLocator { Versions = { [java21] = 21 }, Best = java21 };
            var r = new JavaHealth(locator, store).Check(21);
            Assert.AreEqual(JavaHealthOutcome.Ok, r.Outcome);
            Assert.AreEqual(0, store.Writes);
            Assert.AreEqual(java21, r.EffectivePath);
        }

        [TestMethod]
        public void Replace_an_old_java_written_by_discovery_and_say_so()
        {
            var java8 = Existing("java8"); var java21 = Existing("java21b");
            var store = new MemoryStore { Path = java8, Source = null }; // senza sorgente = scritto da MdExplorer prima che esistesse la sorgente
            var locator = new FakeLocator { Versions = { [java8] = 8, [java21] = 21 }, Best = java21 };
            var r = new JavaHealth(locator, store).Check(21);
            Assert.AreEqual(JavaHealthOutcome.Replaced, r.Outcome);
            Assert.AreEqual(java21, store.Path);
            Assert.AreEqual(JavaPathSources.Auto, store.Source);
            Assert.AreEqual(8, r.ConfiguredMajor);
            StringAssert.Contains(r.ConfiguredProblem, "Java 8");
            Assert.AreEqual(21, r.BestMajor);
            Assert.AreEqual(java21, r.EffectivePath);
        }

        [TestMethod]
        public void Replace_a_java_whose_file_disappeared()
        {
            var gone = Path.Combine(Path.GetTempPath(), "mde-javahealth-gone-" + System.Guid.NewGuid().ToString("N"));
            var java21 = Existing("java21c");
            var store = new MemoryStore { Path = gone, Source = JavaPathSources.User }; // anche se l'ha messo l'utente: un file che non c'è più non si tiene
            var locator = new FakeLocator { Versions = { [java21] = 21 }, Best = java21 };
            var r = new JavaHealth(locator, store).Check(21);
            Assert.AreEqual(JavaHealthOutcome.Replaced, r.Outcome);
            StringAssert.Contains(r.ConfiguredProblem, "no longer exists");
            Assert.AreEqual(java21, store.Path);
        }

        [TestMethod]
        public void Warn_but_not_touch_an_old_java_chosen_by_the_user()
        {
            var java8 = Existing("java8d"); var java21 = Existing("java21d");
            var store = new MemoryStore { Path = java8, Source = JavaPathSources.User };
            var locator = new FakeLocator { Versions = { [java8] = 8, [java21] = 21 }, Best = java21 };
            var r = new JavaHealth(locator, store).Check(21);
            Assert.AreEqual(JavaHealthOutcome.UserPathNotSuitable, r.Outcome);
            Assert.AreEqual(0, store.Writes);
            Assert.AreEqual(java8, store.Path);
            Assert.AreEqual(java21, r.BestPath);
            Assert.AreEqual(java8, r.EffectivePath);
        }

        [TestMethod]
        public void Report_when_no_suitable_java_exists_and_keep_the_old_one_for_plantuml()
        {
            var java8 = Existing("java8e");
            var store = new MemoryStore { Path = java8, Source = JavaPathSources.Auto };
            var locator = new FakeLocator { Versions = { [java8] = 8 }, Best = null };
            var r = new JavaHealth(locator, store).Check(21);
            Assert.AreEqual(JavaHealthOutcome.NoSuitableJava, r.Outcome);
            Assert.AreEqual(0, store.Writes);
            Assert.AreEqual(java8, r.EffectivePath);
            Assert.AreEqual(21, r.MinimumMajor);
        }

        [TestMethod]
        public void Fill_an_empty_setting_with_the_best_java()
        {
            var java21 = Existing("java21f");
            var store = new MemoryStore { Path = null, Source = null };
            var locator = new FakeLocator { Versions = { [java21] = 21 }, Best = java21 };
            var r = new JavaHealth(locator, store).Check(21);
            Assert.AreEqual(JavaHealthOutcome.Replaced, r.Outcome);
            Assert.AreEqual(java21, store.Path);
            StringAssert.Contains(r.ConfiguredProblem, "no path set");
        }
    }
}
