using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// D43 F3, i punti ciechi: le chiamate esterne di cui si sa solo il nome, e dove nel repository c'è quello che il grafo
    /// non ha analizzato (sorgente fuori perimetro, jar, .class, configurazione).
    /// </summary>
    [TestClass]
    public class CodeBlindSpotsShould
    {
        private string _root;

        [TestInitialize]
        public void Setup() { _root = Path.Combine(Path.GetTempPath(), "mde-blind-tests", Guid.NewGuid().ToString("N")); Directory.CreateDirectory(_root); }

        [TestCleanup]
        public void Cleanup() { try { Directory.Delete(_root, true); } catch { } }

        [TestMethod]
        public void Group_external_calls_under_their_type_and_count_the_ones_without_a_type()
        {
            var types = CodeExternals.Group(new[]
            {
                new CodeExternalUse { QualifiedName = "com.lib.Session", Relation = "references", Uses = 2, Callers = 2, Example = "p/A.java:3" },
                new CodeExternalUse { QualifiedName = "com.lib.Session.close", Relation = "calls", Uses = 5, Callers = 3, Example = "p/A.java:9" },
                new CodeExternalUse { QualifiedName = "com.lib.Session.open", Relation = "calls", Uses = 7, Callers = 4, Example = "p/B.java:1" },
                new CodeExternalUse { QualifiedName = "?.contains", Relation = "calls", Uses = 4, Callers = 2, Example = "p/C.java:2" },
                new CodeExternalUse { QualifiedName = "com.lib.util.Strings", Relation = "imports", Uses = 1, Callers = 1, Example = "p/A.java:1" },
            });
            var session = types[0];
            Assert.AreEqual(("com.lib.Session", "com.lib", 14, 4), (session.Type, session.Package, session.Uses, session.Callers));
            CollectionAssert.AreEqual(new[] { "open 7", "close 5" }, session.Members.Select(m => $"{m.Name} {m.Uses}").ToArray());
            Assert.AreEqual("p/A.java:3", session.Example, "il primo punto in ordine");
            Assert.AreEqual((CodeExternals.UnknownOwner, (string)null, 4), (types[1].Type, types[1].Package, types[1].Uses), "le chiamate di cui non si sa nemmeno il tipo");
        }

        private string Write(string relative, string text = "x")
        {
            var path = Path.Combine(_root, relative.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, text);
            return path;
        }

        [TestMethod]
        public void Find_in_the_repository_what_the_graph_did_not_analyse_with_the_remedy()
        {
            Write("program/src/com/acme/App.java");
            Write("shared/src/main/java/com/lib/Session.java", "package com.lib; public class Session {}");      // sorgente fuori dalle cartelle program
            Write("program/old/Session.java");                                                                     // stesso nome, altro package
            Write("program/build/classes/com/lib/Pool$Entry.class");                                              // compilato (build si guarda)
            var jar = Path.Combine(_root, "lib", "vendor.jar");
            Directory.CreateDirectory(Path.GetDirectoryName(jar));
            using (var zip = ZipFile.Open(jar, ZipArchiveMode.Create)) { zip.CreateEntry("com/lib/Pool.class"); zip.CreateEntry("META-INF/MANIFEST.MF"); }
            Write("program/conf/beans.xml", "<beans>\n  <bean class=\"com.lib.Session\"/>\n</beans>");
            Write(".git/objects/Session.java");                                                                   // mai dentro .git
            Write("node_modules/x/Session.js");

            var program = Path.Combine(_root, "program");
            var hits = CodeRepositorySearch.Search(new[] { _root }, new[] { "com.lib.Session", "com.lib.Pool", "com.lib.Pool.Entry", "com.lib.Missing", "?" },
                f => Path.GetFullPath(f).StartsWith(program + Path.DirectorySeparatorChar, StringComparison.Ordinal));
            string Of(string type, string kind) => string.Join("|", hits.Where(h => h.Type == type && h.Kind == kind).Select(h => Path.GetRelativePath(_root, h.File).Replace('\\', '/')));

            Assert.AreEqual("shared/src/main/java/com/lib/Session.java", Of("com.lib.Session", "source"));
            Assert.IsFalse(hits.Single(h => h.Kind == "source").InProgram);
            StringAssert.Contains(hits.Single(h => h.Kind == "source").Remedy, "@alias");
            Assert.AreEqual("program/old/Session.java", Of("com.lib.Session", "source-same-name"));
            Assert.AreEqual("program/conf/beans.xml", Of("com.lib.Session", "config"));
            StringAssert.StartsWith(hits.Single(h => h.Kind == "config").Detail, "2:");
            Assert.AreEqual("lib/vendor.jar", Of("com.lib.Pool", "jar"));
            Assert.AreEqual("com/lib/Pool.class", hits.Single(h => h.Kind == "jar").Detail);
            Assert.AreEqual("program/build/classes/com/lib/Pool$Entry.class", Of("com.lib.Pool.Entry", "compiled"), "la classe interna col $");
            Assert.IsFalse(hits.Any(h => h.Type == "com.lib.Missing"), "non c'è: lo dirà chi chiama");
            Assert.IsFalse(hits.Any(h => h.File.Contains(".git") || h.File.Contains("node_modules")));
            Assert.AreEqual("source", hits.First(h => h.Type == "com.lib.Session").Kind, "prima il sorgente vero");
        }
    }
}
