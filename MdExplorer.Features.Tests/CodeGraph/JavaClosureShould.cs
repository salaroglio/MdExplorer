using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.Features.Services.CodeGraph.Grammars;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>F2b (10/11/2026): la chiusura di un seme Java — tipi per convenzione di percorso, metodi per numero di argomenti.</summary>
    [TestClass]
    public class JavaClosureShould
    {
        private string _root, _project;
        private TreeSitterJavaExtractor _extractor;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-java-closure", Guid.NewGuid().ToString("N"));
            _project = Path.Combine(_root, "project");
            var store = new GrammarStore(_root);
            Directory.CreateDirectory(store.Folder);
            var home = Environment.GetEnvironmentVariable("NUGET_PACKAGES") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
            var grammar = Path.Combine(home, "treesitter.dotnet", "1.3.0", "runtimes", store.Rid, "native", GrammarCatalog.ById("java").FileName(store.Rid));
            if (!File.Exists(grammar)) Assert.Inconclusive("grammatica java non trovata nel NuGet in cache: " + grammar);
            store.Install(GrammarCatalog.ById("java"), grammar, "1.3.0", "nuget");
            _extractor = new TreeSitterJavaExtractor(store, NullLogger<TreeSitterJavaExtractor>.Instance);

            Write("util/src/main/java/com/acme/util/Root.java", "package com.acme.util;\n\npublic class Root {\n    public void ping() { }\n}\n");
            Write("util/src/main/java/com/acme/util/Helper.java", "package com.acme.util;\n\nimport com.acme.util.Deep;\nimport com.acme.util.Other;\n\npublic class Helper extends Root {\n" +
                  "    public void run(String p) { Deep.go(); }\n    public void run(String p, int n) { }\n    public void notUsed() { Other.x(); }\n}\n");
            Write("util/src/main/java/com/acme/util/Deep.java", "package com.acme.util;\n\npublic class Deep {\n    public static void go() { }\n}\n");
            Write("util/src/main/java/com/acme/util/Other.java", "package com.acme.util;\n\npublic class Other {\n    public static void x() { }\n}\n");
            Write("app/src/main/java/com/acme/app/Foo.java", "package com.acme.app;\n\nimport com.acme.util.Helper;\n\npublic class Foo {\n    private final Helper helper = new Helper();\n\n" +
                  "    void go() {\n        helper.run(\"a\");\n        helper.ping();\n    }\n}\n");
        }

        [TestCleanup]
        public void Cleanup() { try { Directory.Delete(_root, true); } catch { } }

        private void Write(string rel, string text)
        {
            var abs = Path.Combine(_project, rel.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(abs));
            File.WriteAllText(abs, text);
        }

        private CodeClosure Run(CodeClosureRequest request) => new JavaClosure(_extractor, new LocalCodeWorkspace(_project, "")).Compute(request);

        private const string FooPath = "app/src/main/java/com/acme/app/Foo.java";
        private static CodeClosureFile Of(CodeClosure c, string name) => c.Files.SingleOrDefault(f => f.Path.EndsWith("/" + name));

        [TestMethod]
        public void Find_the_types_by_path_and_follow_only_the_methods_that_are_called()
        {
            var c = Run(new CodeClosureRequest { Seeds = new[] { FooPath } });
            CollectionAssert.AreEquivalent(new[] { "Foo.java", "Helper.java", "Root.java", "Deep.java" }, c.Files.Select(f => Path.GetFileName(f.Path)).ToList(),
                "Other serve solo a notUsed(), che nessuno chiama. Mancati: " + string.Join("; ", c.Misses.Select(m => m.Kind + " " + m.Written)));
            var helper = Of(c, "Helper.java");
            Assert.AreEqual((1, "type:com.acme.util.Helper"), (helper.Depth, helper.Written), "dal campo e dall'import di Foo");
            Assert.IsTrue(helper.Symbols.Contains("run"), "run(\"a\") → run(String)");
            Assert.IsFalse(helper.Symbols.Contains("notUsed"));
            Assert.IsTrue(Of(c, "Root.java").Symbols.Contains("ping"), "ping() di Root, la madre: arriva dopo aver letto la madre");
            Assert.IsTrue(Of(c, "Deep.java").Symbols.Contains("go"), "dal corpo di run(String)");
        }

        /// <summary>F4b: un ricevente che viene da un altro file (a().b()) si deduce rileggendo insieme i file della chiusura.</summary>
        [TestMethod]
        public void Follow_a_call_on_a_receiver_that_comes_from_another_file()
        {
            Write("util/src/main/java/com/acme/util/Factory.java", "package com.acme.util;\n\npublic class Factory {\n    public static Builder builder() { return new Builder(); }\n}\n");
            Write("util/src/main/java/com/acme/util/Builder.java", "package com.acme.util;\n\npublic class Builder {\n    public void build() { Deep.go(); }\n    public void unused() { Other.x(); }\n}\n");
            Write("app/src/main/java/com/acme/app/Main.java", "package com.acme.app;\n\nimport com.acme.util.Factory;\n\npublic class Main {\n    void start() {\n        Factory.builder().build();\n    }\n}\n");
            var c = Run(new CodeClosureRequest { Seeds = new[] { "app/src/main/java/com/acme/app/Main.java" } });
            Assert.IsTrue(Of(c, "Builder.java")?.Symbols.Contains("build") == true, "build() su ciò che builder() restituisce. Mancati: " + string.Join("; ", c.Misses.Select(m => m.Written)));
            Assert.IsNotNull(Of(c, "Deep.java"), "e da lì Deep.go(). Mancati: " + string.Join("; ", c.Misses.Select(m => m.Kind + " " + m.Written)));
            Assert.IsNull(Of(c, "Other.java"), "Builder.unused() non lo chiama nessuno");
            Assert.IsFalse(c.Misses.Any(m => m.Written == "method:?.build"), "il ricevente non è più sconosciuto");
        }

        private void Implementations()
        {
            Write("lib/com/acme/api/Repository.java", "package com.acme.api;\n\npublic interface Repository {\n    void save(String x);\n}\n");
            Write("lib/com/acme/api/Notifier.java", "package com.acme.api;\n\npublic interface Notifier {\n    void send();\n}\n");
            Write("lib/com/acme/impl/JdbcRepository.java", "package com.acme.impl;\n\nimport com.acme.api.Repository;\n\npublic class JdbcRepository implements Repository {\n    public void save(String x) { Db.write(); }\n}\n");
            Write("lib/com/acme/impl/MemoryRepository.java", "package com.acme.impl;\n\nimport com.acme.api.Repository;\n\npublic class MemoryRepository implements Repository {\n    public void save(String x) { Cache.put(); }\n}\n");
            Write("lib/com/acme/impl/SmsNotifier.java", "package com.acme.impl;\n\nimport com.acme.api.Notifier;\n\npublic class SmsNotifier implements Notifier {\n    public void send() { Sms.go(); }\n}\n");
            foreach (var n in new[] { "Db", "Cache", "Sms" })
                Write($"lib/com/acme/impl/{n}.java", $"package com.acme.impl;\n\npublic class {n} {{\n    public static void write() {{ }}\n    public static void put() {{ }}\n    public static void go() {{ }}\n}}\n");
            Write("app/com/acme/app/Service.java", "package com.acme.app;\n\nimport com.acme.api.Repository;\nimport com.acme.api.Notifier;\n\npublic class Service {\n" +
                  "    private Repository repo;\n    private Notifier notifier;\n\n    void run() {\n        repo.save(\"a\");\n        notifier.send();\n    }\n}\n");
        }

        /// <summary>F4c: un'implementazione unica nella radice si segue da sola; con più d'una si propongono, e si segue quella scelta.</summary>
        [TestMethod]
        public void Follow_the_only_implementation_and_propose_the_others()
        {
            Implementations();
            var seed = "app/com/acme/app/Service.java";
            var c = Run(new CodeClosureRequest { Seeds = new[] { seed } });
            string Files(CodeClosure x) => string.Join(",", x.Files.Select(f => Path.GetFileNameWithoutExtension(f.Path)).OrderBy(n => n));
            Assert.IsNotNull(Of(c, "SmsNotifier.java"), "l'unica implementazione di Notifier entra da sola: " + Files(c));
            Assert.IsNotNull(Of(c, "Sms.java"), "con la sua catena");
            Assert.IsNull(Of(c, "JdbcRepository.java"), "due implementazioni di Repository: nessuna da sola");
            Assert.IsNull(Of(c, "MemoryRepository.java"));
            var repo = c.Implementations.Single(i => i.Contract == "com.acme.api.Repository.save");
            Assert.IsFalse(repo.Auto);
            CollectionAssert.AreEquivalent(new[] { "lib/com/acme/impl/JdbcRepository.java", "lib/com/acme/impl/MemoryRepository.java" }, repo.Candidates, "trovate nella radice col grep e confermate");
            Assert.IsTrue(c.Implementations.Single(i => i.Contract == "com.acme.api.Notifier.send").Auto);

            var chosen = Run(new CodeClosureRequest { Seeds = new[] { seed }, Included = new HashSet<string> { "lib/com/acme/impl/JdbcRepository.java" } });
            Assert.IsNotNull(Of(chosen, "JdbcRepository.java"), "scelta: entra");
            Assert.IsNotNull(Of(chosen, "Db.java"), "con la sua catena: " + Files(chosen));
            Assert.IsNull(Of(chosen, "MemoryRepository.java"), "l'altra no");
        }

        [TestMethod]
        public void Pick_the_overload_by_argument_count()
        {
            var c = Run(new CodeClosureRequest { Seeds = new[] { FooPath } });
            Assert.IsNotNull(Of(c, "Deep.java"), "run(String) raggiunto: il suo corpo chiama Deep");
            Write("app/src/main/java/com/acme/app/Foo.java", "package com.acme.app;\n\nimport com.acme.util.Helper;\n\npublic class Foo {\n    private final Helper helper = new Helper();\n\n" +
                  "    void go() {\n        helper.run(\"a\", 1);\n    }\n}\n");
            var two = Run(new CodeClosureRequest { Seeds = new[] { FooPath } });
            Assert.IsNull(Of(two, "Deep.java"), "run(\"a\", 1) va a run(String, int), che non chiama Deep");
        }
    }
}
