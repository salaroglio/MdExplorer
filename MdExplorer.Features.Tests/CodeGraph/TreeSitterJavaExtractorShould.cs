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

        [TestMethod]
        public void Mark_getters_setters_constants_and_empty_methods_but_not_delegates()
        {
            InstallGrammarFromNuGet();
            File.WriteAllText(Path.Combine(_src, "app", "Shapes.java"),
                "package com.acme.app;\n\npublic class Shapes {\n    private int width; private String name; private Shapes inner;\n" +
                "    public int getWidth() { return width; }\n" +
                "    public String getName() { /* il nome */ return this.name; }\n" +
                "    public void setWidth(int w) { this.width = w; }\n" +
                "    public void setName(String n) { name = n; }\n" +
                "    public int height() { return 20; }\n" +
                "    public String kind() { return \"box\"; }\n" +
                "    public java.util.List<String> none() { return java.util.Collections.EMPTY_LIST; }\n" +
                "    public void onClose() { }\n" +
                "    public void grow(int w) { this.width += w; }\n" +
                "    public int delegate() { return inner.getWidth(); }\n" +
                "    public int twice() { int x = width; return x * 2; }\n" +
                "    public Shapes(int w) { this.width = w; }\n" +
                "}\n");
            var script = new TreeSitterJavaExtractor(_store, NullLogger<TreeSitterJavaExtractor>.Instance).Extract(Request())
                .Scripts.Single(s => s.File.LogicalPath.EndsWith("/Shapes.java")).Script;
            (string, string) T(string name) { var m = script.Symbols.Single(x => x.Name == name && x.Kind != "field"); return (m.Trivial, m.TrivialOf); }

            Assert.AreEqual(("getter", "width"), T("getWidth"));
            Assert.AreEqual(("getter", "name"), T("getName"), "un commento non conta");
            Assert.AreEqual(("setter", "width"), T("setWidth"));
            Assert.AreEqual(("setter", "name"), T("setName"));
            Assert.AreEqual(("constant", "20"), T("height"));
            Assert.AreEqual(("constant", "\"box\""), T("kind"));
            Assert.AreEqual(("constant", "java.util.Collections.EMPTY_LIST"), T("none"));
            Assert.AreEqual(("empty", (string)null), T("onClose"));
            Assert.AreEqual(((string)null, (string)null), T("grow"), "+= non è un setter");
            Assert.AreEqual(((string)null, (string)null), T("delegate"), "un delegante lo descrive l'LLM");
            Assert.AreEqual(((string)null, (string)null), T("twice"), "due istruzioni");
            Assert.IsNull(script.Symbols.Single(x => x.Kind == "constructor" && x.Name == "Shapes").Trivial, "i costruttori no");
            Assert.AreEqual("6", script.Header.ExtractorVersion, "versione nuova: gli script vecchi si rifanno da soli");
        }

        [TestMethod]
        public void Record_call_sites_with_name_like_string_arguments_and_keep_them_through_turtle()
        {
            // D42: lo smistamento per stringa (setX("metodo")) è testo per il parser; il punto di chiamata lo conserva per le regole.
            InstallGrammarFromNuGet();
            File.WriteAllText(Path.Combine(_src, "app", "Dispatch.java"),
                "package com.acme.app;\n\npublic class Dispatch {\n" +
                "    private Svc svc;\n" +
                "    public void go(String x) {\n" +
                "        svc.setTarget(\"leggiDati\");\n" +
                "        log.info(\"messaggio con spazi\");\n" +
                "        send(x, \"alias.metodo\", 3); send(\"redirect:pagina\", x, 1);\n" +
                "        svc.setTarget(NOME);\n" +
                "    }\n" +
                "    void send(String a, String b, int n) { }\n" +
                "}\n");
            var script = new TreeSitterJavaExtractor(_store, NullLogger<TreeSitterJavaExtractor>.Instance).Extract(Request())
                .Scripts.Single(s => s.File.LogicalPath.EndsWith("/Dispatch.java")).Script;
            var go = script.Symbols.Single(s => s.Name == "go").Id;

            Assert.AreEqual(3, script.CallSites.Count, "le frasi (log) e le costanti non sono punti di chiamata con stringhe");
            var target = script.CallSites.Single(c => c.Callee == "setTarget");
            Assert.AreEqual((go, 6, "svc", 1, 0), (target.SourceId, target.Line, target.Receiver, target.ArgumentCount, target.Ordinal));
            Assert.AreEqual((0, "leggiDati"), (target.Arguments.Single().Position, target.Arguments.Single().Value));
            var sends = script.CallSites.Where(c => c.Callee == "send").OrderBy(c => c.Ordinal).ToList();
            CollectionAssert.AreEqual(new[] { 0, 1 }, sends.Select(c => c.Ordinal).ToArray(), "due chiamate sulla stessa riga");
            Assert.IsNull(sends[0].Receiver);
            Assert.AreEqual((1, "alias.metodo"), (sends[0].Arguments.Single().Position, sends[0].Arguments.Single().Value));
            Assert.AreEqual((0, "redirect:pagina"), (sends[1].Arguments.Single().Position, sends[1].Arguments.Single().Value));

            var back = CodeScriptParser.Parse(CodeScriptTurtle.Write(script), "x.ast.ttl");
            Assert.AreEqual(script.RecordCount, back.RecordCount);
            var b = back.CallSites.Single(c => c.Callee == "send" && c.Ordinal == 1);
            Assert.AreEqual((go, 8, 3, "redirect:pagina"), (b.SourceId, b.Line, b.ArgumentCount, b.Arguments.Single().Value));
            Assert.AreEqual("svc", back.CallSites.Single(c => c.Callee == "setTarget").Receiver);
        }

        /// <summary>
        /// D43 (30/10/2026): i fatti per le regole di registrazione, registrati e non interpretati. Un modulo Guice: gli
        /// argomenti X.class col tipo risolto, la catena bind → to (anche oltre annotatedWith), le annotazioni con gli
        /// argomenti, il tipo restituito risolto; tutto attraverso il Turtle.
        /// </summary>
        [TestMethod]
        public void Record_class_arguments_chains_annotations_and_return_types_for_registration_rules()
        {
            InstallGrammarFromNuGet();
            File.WriteAllText(Path.Combine(_src, "app", "AppModule.java"),
                "package com.acme.app;\n\n" +                                                              // 1-2
                "import com.google.inject.AbstractModule;\nimport com.google.inject.Provides;\nimport com.google.inject.name.Named;\n" +   // 3-5
                "import com.google.inject.name.Names;\nimport com.google.inject.multibindings.MapBinder;\nimport com.acme.util.Helper;\n\n" + // 6-9
                "public class AppModule extends AbstractModule {\n" +                                         // 10
                "    @Override\n    protected void configure() {\n" +                                        // 11-12
                "        bind(IFoo.class).annotatedWith(Names.named(\"main\")).to(Foo.class);\n" +           // 13
                "        bind(Helper.class).toInstance(new Helper());\n" +                                    // 14
                "        MapBinder<String, IFoo> m = MapBinder.newMapBinder(binder(), String.class, IFoo.class);\n" + // 15
                "        m.addBinding(\"a\").to(Foo.class);\n" +                                              // 16
                "    }\n\n" +                                                                                 // 17-18
                "    @Provides @Named(\"helper\")\n    Helper provideHelper() { return new Helper(); }\n" +   // 19-20
                "}\n");
            File.WriteAllText(Path.Combine(_src, "app", "Svc.java"),
                "package com.acme.app;\n\nimport com.google.inject.ImplementedBy;\nimport org.junit.jupiter.api.DisplayName;\n\n" +
                "@ImplementedBy(Foo.class) @DisplayName(\"una frase con spazi\")\npublic interface Svc { }\n");
            var scripts = new TreeSitterJavaExtractor(_store, NullLogger<TreeSitterJavaExtractor>.Instance).Extract(Request()).Scripts;
            var script = scripts.Single(s => s.File.LogicalPath.EndsWith("/AppModule.java")).Script;
            ScriptCallSite Site(string callee, int line) => script.CallSites.Single(c => c.Callee == callee && c.Line == line);

            var bind = Site("bind", 13);
            Assert.AreEqual(("com.acme.app.IFoo", "extracted", 0), (bind.TypeArguments.Single().TypeName, bind.TypeArguments.Single().Confidence, bind.TypeArguments.Single().Position));
            Assert.IsTrue(bind.Ordinal >= CodeCallSites.TypeOnlyOrdinalBase, "solo tipi: numerato a parte, i numeri dei punti con stringhe non cambiano");
            Assert.AreEqual(0, Site("named", 13).Ordinal, "il punto con la stringa tiene il numero di prima");
            var to = Site("to", 13);
            Assert.AreEqual("com.acme.app.Foo", to.TypeArguments.Single().TypeName);
            Assert.AreEqual((13, bind.Ordinal), (to.ReceiverLine, to.ReceiverOrdinal), "la catena oltre annotatedWith, che non è registrata");
            Assert.AreEqual("com.acme.util.Helper", Site("bind", 14).TypeArguments.Single().TypeName);
            Assert.IsFalse(script.CallSites.Any(c => c.Callee == "toInstance"), "new Helper() non è un argomento di tipo");
            var map = Site("newMapBinder", 15);
            CollectionAssert.AreEqual(new[] { "1 String platform", "2 com.acme.app.IFoo extracted" }, map.TypeArguments.Select(t => $"{t.Position} {t.TypeName} {t.Confidence}").ToArray());
            var add = Site("addBinding", 16);
            Assert.AreEqual(("m", "a", (int?)null), (add.Receiver, add.Arguments.Single().Value, add.ReceiverLine), "una variabile non è una catena");
            Assert.AreEqual((16, add.Ordinal), (Site("to", 16).ReceiverLine, Site("to", 16).ReceiverOrdinal));

            var provide = script.Symbols.Single(s => s.Name == "provideHelper");
            Assert.AreEqual("com.acme.util.Helper", provide.ReturnTypeName);
            Assert.IsNull(script.Symbols.Single(s => s.Name == "configure").ReturnTypeName, "void");
            var anns = script.Annotations.Where(a => a.SymbolId == provide.Id).OrderBy(a => a.Ordinal).ToList();
            CollectionAssert.AreEqual(new[] { "Provides com.google.inject.Provides", "Named com.google.inject.name.Named" }, anns.Select(a => $"{a.Name} {a.TypeName}").ToArray());
            Assert.AreEqual(("value", "helper"), (anns[1].Arguments.Single().Element, anns[1].Arguments.Single().Value));
            Assert.IsFalse(script.Annotations.Any(a => a.Name == "Override"), "@Override non dice niente al grafo");

            var svc = scripts.Single(s => s.File.LogicalPath.EndsWith("/Svc.java")).Script;
            var implementedBy = svc.Annotations.Single(a => a.Name == "ImplementedBy");
            Assert.AreEqual(("com.google.inject.ImplementedBy", "value", "com.acme.app.Foo"), (implementedBy.TypeName, implementedBy.Arguments.Single().Element, implementedBy.Arguments.Single().TypeName));
            Assert.AreEqual(0, svc.Annotations.Single(a => a.Name == "DisplayName").Arguments.Count, "le frasi non si registrano");

            foreach (var s in new[] { script, svc })
            {
                var text = CodeScriptTurtle.Write(s);
                var back = CodeScriptParser.Parse(text, "x.ast.ttl");
                Assert.AreEqual(text, CodeScriptTurtle.Write(back), "andata e ritorno identici");
            }
        }
    }
}
