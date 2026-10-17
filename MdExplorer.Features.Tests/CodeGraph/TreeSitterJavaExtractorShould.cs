using System;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.Features.Services.CodeGraph.Grammars;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// F8: Java con la grammatica dal NuGet in cache. Tipi qualificati dal package, overload distinti dai tipi dei
    /// parametri, stereotipi Spring come kind, record ed enum, campi, costruttori, import verso il perimetro,
    /// extends/implements, chiamate implicite/statiche/per nome e istanze con la confidenza giusta; java.lang e
    /// java.* non fanno nodi.
    /// </summary>
    [TestClass]
    public class TreeSitterJavaExtractorShould
    {
        private string _root, _project, _src;
        private GrammarStore _store;

        private static string NuGetGrammar(GrammarStore store)
        {
            var home = Environment.GetEnvironmentVariable("NUGET_PACKAGES") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
            return Path.Combine(home, "treesitter.dotnet", "1.3.0", "runtimes", store.Rid, "native", GrammarCatalog.ById("java").FileName(store.Rid));
        }

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-java-tests", Guid.NewGuid().ToString("N"));
            _project = Path.Combine(_root, "project");
            _src = Path.Combine(_project, "src", "main", "java", "com", "acme");
            Directory.CreateDirectory(Path.Combine(_src, "app"));
            Directory.CreateDirectory(Path.Combine(_src, "util"));
            _store = new GrammarStore(_root);
            Directory.CreateDirectory(_store.Folder);
            File.WriteAllText(Path.Combine(_src, "util", "Helper.java"),
                "package com.acme.util;\n\npublic class Helper {\n    public void run(String p) { }\n    public void run(String p, int n) { }\n    public static long now() { return 1L; }\n}\n");
            File.WriteAllText(Path.Combine(_src, "app", "IFoo.java"),
                "package com.acme.app;\n\nimport java.util.List;\n\npublic interface IFoo {\n    List<String> bar(String path, int... retries);\n}\n");
            File.WriteAllText(Path.Combine(_src, "app", "Base.java"),
                "package com.acme.app;\n\npublic abstract class Base {\n    protected int level;\n    static void staticOne() { }\n}\n");
            File.WriteAllText(Path.Combine(_src, "app", "Foo.java"),
                "package com.acme.app;\n\nimport java.util.List;\nimport java.io.IOException;\nimport com.acme.util.Helper;\nimport org.slf4j.Logger;\n\n" +
                "@Service\npublic class Foo extends Base implements IFoo, Runnable {\n" +
                "    private final Helper helper = new Helper();\n    public static int COUNT = 0;\n    private Logger log;\n\n" +
                "    public Foo(Helper h, int n) { this.helper = h; }\n\n" +
                "    @Override\n    public List<String> bar(String path, int... retries) throws IOException {\n" +
                "        helper.run(path);\n        this.run();\n        Helper.now();\n        Base.staticOne();\n        log.info(path);\n        return List.of(path);\n    }\n\n" +
                "    public void run() { }\n    void packagePrivate() { }\n\n" +
                "    enum Color { RED, GREEN }\n    record Point(int x, int y) {}\n    interface Inner { void go(); }\n}\n");
        }

        [TestCleanup]
        public void Cleanup() { try { Directory.Delete(_root, true); } catch { } }

        private CodeExtractionRequest Request()
        {
            var files = Directory.GetFiles(_src, "*.java", SearchOption.AllDirectories).OrderBy(f => f)
                .Select(f => new CodeSourceFile(f, CodeGraphPaths.ToProjectRelative(_project, f))).ToList();
            return new CodeExtractionRequest { ProjectPath = _project, AllFiles = files, FilesToEmit = files };
        }

        private void InstallGrammarFromNuGet()
        {
            var source = NuGetGrammar(_store);
            if (!File.Exists(source)) Assert.Inconclusive("grammatica java non trovata nel NuGet in cache: " + source);
            _store.Install(GrammarCatalog.ById("java"), source, "1.3.0", "nuget");
        }

        [TestMethod]
        public void Extract_qualified_types_overloads_members_imports_and_calls_with_confidence()
        {
            InstallGrammarFromNuGet();
            var extractor = new TreeSitterJavaExtractor(_store, NullLogger<TreeSitterJavaExtractor>.Instance);
            Assert.AreEqual("ready", extractor.Availability);

            var result = extractor.Extract(Request());
            Assert.AreEqual(0, result.Warnings.Count, string.Join("; ", result.Warnings));
            Assert.AreEqual(4, result.Scripts.Count);
            var foo = result.Scripts.Single(s => s.File.LogicalPath.EndsWith("/Foo.java")).Script;
            var helper = result.Scripts.Single(s => s.File.LogicalPath.EndsWith("/Helper.java")).Script;
            var ifoo = result.Scripts.Single(s => s.File.LogicalPath.EndsWith("/IFoo.java")).Script;
            var baseS = result.Scripts.Single(s => s.File.LogicalPath.EndsWith("/Base.java")).Script;

            // Tipi: qualificati dal package, stereotipo Spring come kind, annidati, record ed enum.
            var fooType = foo.Symbols.Single(s => s.QualifiedName == "com.acme.app.Foo");
            Assert.AreEqual("service", fooType.Kind, "@Service");
            Assert.AreEqual("public", fooType.Visibility);
            Assert.AreEqual("enum", foo.Symbols.Single(s => s.QualifiedName == "com.acme.app.Foo.Color").Kind);
            Assert.AreEqual("record", foo.Symbols.Single(s => s.QualifiedName == "com.acme.app.Foo.Point").Kind);
            Assert.AreEqual("interface", foo.Symbols.Single(s => s.QualifiedName == "com.acme.app.Foo.Inner").Kind);
            Assert.AreEqual(fooType.Id, foo.Symbols.Single(s => s.Name == "Color").ParentId);
            CollectionAssert.AreEquivalent(new[] { "RED", "GREEN" }, foo.Symbols.Where(s => s.Kind == "field" && s.ParentId == foo.Symbols.Single(x => x.Name == "Color").Id).Select(s => s.Name).ToArray());
            CollectionAssert.AreEquivalent(new[] { "x", "y" }, foo.Symbols.Where(s => s.Kind == "field" && s.ParentId == foo.Symbols.Single(x => x.Name == "Point").Id).Select(s => s.Name).ToArray());

            // Overload distinti dai tipi dei parametri; varargs come params.
            var runs = helper.Symbols.Where(s => s.Kind == "method" && s.Name == "run").Select(s => s.QualifiedName).OrderBy(q => q, StringComparer.Ordinal).ToArray();
            CollectionAssert.AreEqual(new[] { "com.acme.util.Helper.run(String)", "com.acme.util.Helper.run(String, int)" }, runs);
            var bar = foo.Symbols.Single(s => s.Kind == "method" && s.Name == "bar");
            Assert.AreEqual("com.acme.app.Foo.bar(String, int...)", bar.QualifiedName);
            Assert.AreEqual("List<String>", bar.ReturnType);
            StringAssert.Contains(bar.Signature, "public List<String> bar(String path, int... retries) throws IOException");
            var barParams = foo.Parameters.Where(p => p.SymbolId == bar.Id).OrderBy(p => p.Position).ToList();
            CollectionAssert.AreEqual(new[] { "path", "retries" }, barParams.Select(p => p.Name).ToArray());
            Assert.AreEqual("int...", barParams[1].ParameterType); Assert.AreEqual("params", barParams[1].Direction);
            Assert.AreEqual("package", foo.Symbols.Single(s => s.Name == "packagePrivate").Visibility);
            var now = helper.Symbols.Single(s => s.Name == "now");
            Assert.IsTrue(now.IsStatic); Assert.AreEqual("long", now.ReturnType);

            // Costruttore e campi.
            var ctor = foo.Symbols.Single(s => s.Kind == "constructor");
            Assert.AreEqual("com.acme.app.Foo.Foo(Helper, int)", ctor.QualifiedName);
            CollectionAssert.AreEquivalent(new[] { "helper", "COUNT", "log" }, foo.Symbols.Where(s => s.Kind == "field" && s.ParentId == fooType.Id).Select(s => s.Name).ToArray());
            Assert.IsTrue(foo.Symbols.Single(s => s.Name == "COUNT").IsStatic);

            // extends / implements: Base e IFoo nel perimetro (stesso package, extracted); Runnable è java.lang: niente nodo.
            var baseType = baseS.Symbols.Single(s => s.QualifiedName == "com.acme.app.Base");
            var ifooType = ifoo.Symbols.Single(s => s.QualifiedName == "com.acme.app.IFoo");
            Assert.IsTrue(foo.Edges.Any(e => e.SourceId == fooType.Id && e.TargetId == baseType.Id && e.Relation == "inherits" && e.Confidence == "extracted"));
            Assert.IsTrue(foo.Edges.Any(e => e.SourceId == fooType.Id && e.TargetId == ifooType.Id && e.Relation == "implements" && e.Confidence == "extracted"));
            Assert.IsFalse(foo.Symbols.Any(s => s.Kind == "external" && s.Name == "Runnable"), "java.lang non fa nodi");
            Assert.IsFalse(foo.Symbols.Any(s => s.Kind == "external" && s.Name == "List"), "java.util importato non fa nodi");

            // Import: Helper nel perimetro, Logger esterno con il fqn, java.* niente.
            var helperType = helper.Symbols.Single(s => s.QualifiedName == "com.acme.util.Helper");
            Assert.IsTrue(foo.Edges.Any(e => e.SourceId == fooType.Id && e.TargetId == helperType.Id && e.Relation == "imports"));
            var logger = foo.Symbols.Single(s => s.Kind == "external" && s.QualifiedName == "org.slf4j.Logger");
            Assert.IsTrue(foo.Edges.Any(e => e.SourceId == fooType.Id && e.TargetId == logger.Id && e.Relation == "imports"));
            Assert.IsTrue(foo.Edges.Any(e => e.SourceId == foo.Symbols.Single(s => s.Name == "log").Id && e.TargetId == logger.Id && e.Relation == "references"));

            // Chiamate (D38): this.run() extracted; helper.run(path) col tipo del campo (Helper) e un argomento → solo
            // run(String), extracted; Helper.now() statica extracted; Base.staticOne() extracted; log.info sul tipo del
            // campo (org.slf4j.Logger, libreria) → external Logger.info extracted; List.of è piattaforma: niente arco;
            // new Helper() instantiates extracted (importato).
            var runFoo = foo.Symbols.Single(s => s.Kind == "method" && s.Name == "run");
            Assert.IsTrue(foo.Edges.Any(e => e.SourceId == bar.Id && e.TargetId == runFoo.Id && e.Relation == "calls" && e.Confidence == "extracted"));
            var runString = helper.Symbols.Single(s => s.QualifiedName == "com.acme.util.Helper.run(String)");
            var runStringInt = helper.Symbols.Single(s => s.QualifiedName == "com.acme.util.Helper.run(String, int)");
            Assert.IsTrue(foo.Edges.Any(e => e.SourceId == bar.Id && e.TargetId == runString.Id && e.Relation == "calls" && e.Confidence == "extracted"), "helper.run(path): tipo del campo + un argomento");
            Assert.IsFalse(foo.Edges.Any(e => e.SourceId == bar.Id && e.TargetId == runStringInt.Id), "l'overload a due argomenti non è chiamato");
            Assert.IsTrue(foo.Edges.Any(e => e.SourceId == bar.Id && e.TargetId == now.Id && e.Relation == "calls" && e.Confidence == "extracted"));
            var staticOne = baseS.Symbols.Single(s => s.Name == "staticOne");
            Assert.IsTrue(foo.Edges.Any(e => e.SourceId == bar.Id && e.TargetId == staticOne.Id && e.Relation == "calls" && e.Confidence == "extracted"));
            var info = foo.Symbols.Single(s => s.Kind == "external" && s.QualifiedName == "org.slf4j.Logger.info");
            Assert.IsTrue(foo.Edges.Any(e => e.SourceId == bar.Id && e.TargetId == info.Id && e.Relation == "calls" && e.Confidence == "extracted"));
            Assert.IsFalse(foo.Symbols.Any(s => s.Kind == "external" && s.Name == "of"), "List.of: piattaforma, niente nodo");
            var helperField = foo.Symbols.Single(s => s.Name == "helper");
            Assert.IsTrue(foo.Edges.Any(e => e.SourceId == helperField.Id && e.TargetId == helperType.Id && e.Relation == "instantiates" && e.Confidence == "extracted"));

            foreach (var (file, script) in result.Scripts)
                Assert.AreEqual(script.RecordCount, CodeScriptParser.Parse(CodeScriptWriter.Serialize(script), file.LogicalPath).RecordCount);
        }

        /// <summary>D38: le chiamate si risolvono con quello che il codice scrive, prima del nome.</summary>
        [TestMethod]
        public void Resolve_calls_through_declared_types_hierarchy_imports_and_interfaces()
        {
            InstallGrammarFromNuGet();
            Directory.CreateDirectory(Path.Combine(_src, "repo"));
            Directory.CreateDirectory(Path.Combine(_src, "other"));
            File.WriteAllText(Path.Combine(_src, "repo", "Repo.java"),
                "package com.acme.repo;\n\npublic interface Repo {\n    void save(Object o);\n}\n");
            File.WriteAllText(Path.Combine(_src, "repo", "UserRepo.java"),
                "package com.acme.repo;\n\npublic class UserRepo implements Repo {\n    public void save(Object o) { }\n    public void flush() { }\n    public int size() { return 0; }\n}\n");
            File.WriteAllText(Path.Combine(_src, "repo", "Audit.java"),
                "package com.acme.repo;\n\npublic class Audit {\n    public static void trace(String s) { }\n}\n");
            File.WriteAllText(Path.Combine(_src, "repo", "Parent.java"),
                "package com.acme.repo;\n\npublic class Parent {\n    protected UserRepo users;\n    public Parent(int n) { }\n    protected void helperOfParent() { }\n}\n");
            // due tipi omonimi «Item» nel perimetro
            File.WriteAllText(Path.Combine(_src, "repo", "Item.java"), "package com.acme.repo;\n\npublic class Item { public void touch() { } }\n");
            File.WriteAllText(Path.Combine(_src, "other", "Item.java"), "package com.acme.other;\n\npublic class Item { public void touch() { } }\n");
            File.WriteAllText(Path.Combine(_src, "app", "Service.java"),
                "package com.acme.app;\n\n" +
                "import java.util.List;\nimport java.util.ArrayList;\nimport java.io.IOException;\n" +
                "import com.acme.repo.Repo;\nimport com.acme.repo.UserRepo;\nimport com.acme.repo.Parent;\nimport org.lib.Client;\nimport org.lib.User;\n" +
                "import static com.acme.repo.Audit.trace;\n\n" +
                "public class Service extends Parent {\n" +
                "    private Repo repo;\n\n" +
                "    public Service() { super(1); }\n" +
                "    public Service(int n) { this(); }\n\n" +
                "    public void work(UserRepo direct, Client client) {\n" +
                "        repo.save(this);\n" +
                "        direct.flush();\n" +
                "        var local = new UserRepo();\n" +
                "        local.flush();\n" +
                "        List<String> names = new ArrayList<>();\n" +
                "        names.add(\"x\");\n" +
                "        client.send(names);\n" +
                "        helperOfParent();\n" +
                "        this.users.flush();\n" +
                "        trace(\"done\");\n" +
                "        for (UserRepo r : others()) { r.size(); }\n" +
                "        try { } catch (IOException e) { e.printStackTrace(); }\n" +
                "        User u = null;\n" +
                "        unknownThing.mystery();\n" +
                "    }\n\n" +
                "    private List<UserRepo> others() { return null; }\n\n" +
                "    class Inner {\n        void go() { others(); }\n    }\n" +
                "}\n");

            var extractor = new TreeSitterJavaExtractor(_store, NullLogger<TreeSitterJavaExtractor>.Instance);
            var result = extractor.Extract(Request());
            Assert.AreEqual(0, result.Warnings.Count, string.Join("; ", result.Warnings));
            var all = result.Scripts.SelectMany(x => x.Script.Symbols).ToList();
            var svc = result.Scripts.Single(x => x.File.LogicalPath.EndsWith("/Service.java")).Script;
            string Id(string qn) => all.Single(x => x.QualifiedName == qn).Id;
            var work = Id("com.acme.app.Service.work(UserRepo, Client)");
            ScriptEdge Call(string from, string to) => svc.Edges.SingleOrDefault(e => e.SourceId == from && e.TargetId == to && e.Relation == "calls");

            // campo di tipo interfaccia: il metodo dell'interfaccia (extracted) e chi la implementa (inferred)
            Assert.AreEqual("extracted", Call(work, Id("com.acme.repo.Repo.save(Object)"))?.Confidence, "repo.save → Repo.save");
            Assert.AreEqual("inferred", Call(work, Id("com.acme.repo.UserRepo.save(Object)"))?.Confidence, "…e l'implementazione");
            // parametro, var = new T(), campo ereditato via this.campo: tutti verso UserRepo.flush, extracted
            Assert.AreEqual("extracted", Call(work, Id("com.acme.repo.UserRepo.flush()"))?.Confidence);
            // metodo ereditato da una madre del perimetro, senza ricevente
            Assert.AreEqual("extracted", Call(work, Id("com.acme.repo.Parent.helperOfParent()"))?.Confidence);
            // variabile del for-each
            Assert.AreEqual("extracted", Call(work, Id("com.acme.repo.UserRepo.size()"))?.Confidence);
            // import statico
            Assert.AreEqual("extracted", Call(work, Id("com.acme.repo.Audit.trace(String)"))?.Confidence);
            // libreria: il nodo external Tipo.metodo, mai il metodo omonimo del perimetro
            var send = svc.Symbols.Single(x => x.Kind == "external" && x.QualifiedName == "org.lib.Client.send");
            Assert.AreEqual("extracted", Call(work, send.Id)?.Confidence);
            // piattaforma: names.add, e.printStackTrace → niente archi, niente nodi
            Assert.IsFalse(svc.Symbols.Any(x => x.Kind == "external" && (x.Name == "add" || x.Name == "printStackTrace")), "java.* non fa nodi");
            // ricevente sconosciuto: per nome, con il nome normalizzato «?.mystery» (mai il testo della chiamata)
            Assert.IsTrue(svc.Symbols.Any(x => x.Kind == "external" && x.QualifiedName == "?.mystery"));
            Assert.IsFalse(svc.Symbols.Any(x => x.Kind == "external" && x.QualifiedName.Contains("unknownThing")));
            // this() e super(n): i costruttori
            var ctorDefault = Id("com.acme.app.Service.Service()");
            Assert.AreEqual("extracted", Call(Id("com.acme.app.Service.Service(int)"), ctorDefault)?.Confidence, "this()");
            Assert.AreEqual("extracted", Call(ctorDefault, Id("com.acme.repo.Parent.Parent(int)"))?.Confidence, "super(1)");
            // classe interna: others() è della classe che la contiene
            Assert.AreEqual("extracted", Call(Id("com.acme.app.Service.Inner.go()"), Id("com.acme.app.Service.others()"))?.Confidence);
            // import esterno vince sull'assenza nel perimetro: User è org.lib.User (external), non un tipo inventato
            Assert.IsTrue(svc.Symbols.Any(x => x.Kind == "external" && x.QualifiedName == "org.lib.User"));
        }

        /// <summary>
        /// D38, seconda passata (misurata su PlantUML): catene (tipo di ritorno), generici della piattaforma e del
        /// perimetro anche ereditati, piattaforma che resta piattaforma, try-with-resources, variabili omonime in blocchi
        /// diversi, classi anonime, costanti di enum, nomi qualificati per intero, metodi ereditati dalla piattaforma.
        /// </summary>
        [TestMethod]
        public void Resolve_chains_generics_scopes_and_anonymous_classes()
        {
            InstallGrammarFromNuGet();
            Directory.CreateDirectory(Path.Combine(_src, "g"));
            File.WriteAllText(Path.Combine(_src, "g", "Shape.java"),
                "package com.acme.g;\n\npublic interface Shape {\n    Style style();\n    void draw();\n}\n");
            File.WriteAllText(Path.Combine(_src, "g", "Style.java"),
                "package com.acme.g;\n\npublic class Style {\n    public Color color() { return null; }\n}\n");
            File.WriteAllText(Path.Combine(_src, "g", "Color.java"),
                "package com.acme.g;\n\npublic enum Color {\n    RED, GREEN;\n    public int rgb() { return 0; }\n}\n");
            File.WriteAllText(Path.Combine(_src, "g", "Box.java"),
                "package com.acme.g;\n\npublic class Box<T> {\n    public T get() { return null; }\n}\n");
            File.WriteAllText(Path.Combine(_src, "g", "Canvas.java"),
                "package com.acme.g;\n\npublic abstract class Canvas<O> {\n    protected O target() { return null; }\n}\n");
            File.WriteAllText(Path.Combine(_src, "g", "Pen.java"),
                "package com.acme.g;\n\npublic class Pen {\n    public void stroke() { }\n}\n");
            File.WriteAllText(Path.Combine(_src, "g", "Lines.java"),
                "package com.acme.g;\n\nimport java.util.ArrayList;\n\npublic class Lines extends ArrayList<String> {\n    void fill() { addAll(null); }\n}\n");
            File.WriteAllText(Path.Combine(_src, "g", "Painter.java"),
                "package com.acme.g;\n\n" +
                "import java.util.List;\nimport java.util.Map;\nimport java.io.InputStream;\nimport java.io.FileInputStream;\n\n" +
                "public class Painter extends Canvas<Pen> {\n" +
                "    private List<Shape> shapes;\n" +
                "    private Map<String, Shape> byName;\n\n" +
                "    void paint(Shape s, Box<Style> box, Object o) {\n" +
                "        s.style().color().rgb();\n" +
                "        shapes.get(0).draw();\n" +
                "        byName.get(\"x\").style();\n" +
                "        box.get().color();\n" +
                "        target().stroke();\n" +
                "        new StringBuilder().append(\"a\").append(\"b\").append(\"c\").append(\"d\").append(\"e\").append(\"f\").reverse();\n" +
                "        Color.RED.rgb();\n" +
                "        com.acme.g.Pen p = new com.acme.g.Pen();\n" +
                "        try (InputStream in = new FileInputStream(\"f\")) { in.read(); } catch (Exception e) { }\n" +
                "        if (o instanceof Shape) { final Shape same = (Shape) o; same.draw(); }\n" +
                "        else { final Style same = (Style) o; same.color(); }\n" +
                "        ((Shape) o).style();\n" +
                "        Shape anon = new Shape() {\n" +
                "            public Style style() { return null; }\n" +
                "            public void draw() { style(); }\n" +
                "        };\n" +
                "    }\n" +
                "}\n");

            var result = new TreeSitterJavaExtractor(_store, NullLogger<TreeSitterJavaExtractor>.Instance).Extract(Request());
            Assert.AreEqual(0, result.Warnings.Count, string.Join("; ", result.Warnings));
            var all = result.Scripts.SelectMany(x => x.Script.Symbols).ToList();
            var painter = result.Scripts.Single(x => x.File.LogicalPath.EndsWith("/Painter.java")).Script;
            string Id(string qn) => all.Single(x => x.QualifiedName == qn).Id;
            var paint = Id("com.acme.g.Painter.paint(Shape, Box<Style>, Object)");
            string Conf(string to) => painter.Edges.SingleOrDefault(e => e.SourceId == paint && e.TargetId == Id(to) && e.Relation == "calls")?.Confidence;

            Assert.AreEqual("extracted", Conf("com.acme.g.Shape.style()"), "s.style()");
            Assert.AreEqual("extracted", Conf("com.acme.g.Style.color()"), "s.style().color(): tipo di ritorno");
            Assert.AreEqual("extracted", Conf("com.acme.g.Color.rgb()"), "catena di tre e Color.RED.rgb()");
            Assert.AreEqual("extracted", Conf("com.acme.g.Shape.draw()"), "List<Shape>.get(0).draw(), cast in un blocco");
            Assert.AreEqual("extracted", Conf("com.acme.g.Box.get()"));
            Assert.AreEqual("extracted", Conf("com.acme.g.Pen.stroke()"), "O target() ereditato da Canvas<Pen>");
            Assert.AreEqual("extracted", Conf("com.acme.g.Canvas.target()"));
            // nessuna chiamata finisce «per nome»: niente ambiguous, niente ?.x dal metodo
            var targets = painter.Edges.Where(e => e.SourceId == paint && e.Relation == "calls").ToList();
            Assert.IsFalse(targets.Any(e => e.Confidence == "ambiguous"), string.Join(", ", targets.Where(e => e.Confidence == "ambiguous").Select(e => all.First(x => x.Id == e.TargetId).QualifiedName)));
            Assert.IsFalse(painter.Symbols.Any(x => x.Kind == "external" && x.QualifiedName.StartsWith("?.")), "piattaforma e perimetro: nessun nome indovinato");
            // il metodo della classe anonima chiama style() suo: niente arco per nome verso Shape.style da draw()
            Assert.IsFalse(painter.Symbols.Any(x => x.Name == "style" && x.Kind == "external"));
            // Lines estende ArrayList: addAll è della piattaforma, nessun arco verso omonimi
            var lines = result.Scripts.Single(x => x.File.LogicalPath.EndsWith("/Lines.java")).Script;
            Assert.IsFalse(lines.Edges.Any(e => e.Relation == "calls"), "addAll ereditato da ArrayList");
        }

        /// <summary>D38: due tipi omonimi senza import che scelga → un arco ambiguous per ciascuno, mai il primo a caso.</summary>
        [TestMethod]
        public void Link_every_same_named_type_as_ambiguous_instead_of_picking_one()
        {
            InstallGrammarFromNuGet();
            Directory.CreateDirectory(Path.Combine(_src, "a"));
            Directory.CreateDirectory(Path.Combine(_src, "b"));
            File.WriteAllText(Path.Combine(_src, "a", "Item.java"), "package com.acme.a;\n\npublic class Item { }\n");
            File.WriteAllText(Path.Combine(_src, "b", "Item.java"), "package com.acme.b;\n\npublic class Item { }\n");
            File.WriteAllText(Path.Combine(_src, "app", "Uses.java"), "package com.acme.app;\n\npublic class Uses {\n    void make() { Object o = new Item(); }\n}\n");

            var result = new TreeSitterJavaExtractor(_store, NullLogger<TreeSitterJavaExtractor>.Instance).Extract(Request());
            var uses = result.Scripts.Single(x => x.File.LogicalPath.EndsWith("/Uses.java")).Script;
            var items = result.Scripts.SelectMany(x => x.Script.Symbols).Where(x => x.Name == "Item" && x.Kind == "class").Select(x => x.Id).ToList();
            Assert.AreEqual(2, items.Count);
            var edges = uses.Edges.Where(e => e.Relation == "instantiates").ToList();
            CollectionAssert.AreEquivalent(items, edges.Select(e => e.TargetId).ToList());
            Assert.IsTrue(edges.All(e => e.Confidence == "ambiguous"));
        }

        [TestMethod]
        public void Declare_itself_not_ready_without_the_grammar()
        {
            var extractor = new TreeSitterJavaExtractor(_store, NullLogger<TreeSitterJavaExtractor>.Instance);
            StringAssert.Contains(extractor.Availability, "grammatica da scaricare");
            Assert.ThrowsException<InvalidOperationException>(() => extractor.Extract(Request()));
        }
    }
}
