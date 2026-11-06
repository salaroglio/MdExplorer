using System;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// L'estrattore PowerShell sul parser ufficiale: moduli, funzioni (anche annidate) con parametri e [OutputType],
    /// classi con base/interfacce, costruttore, metodi, property hidden/static, enum, variabili di script, export del
    /// .psm1, import (dot-sourcing, Import-Module, using module, #Requires), chiamate e istanze con la confidenza giusta.
    /// </summary>
    [TestClass]
    public class PowerShellExtractorShould
    {
        private string _root, _project, _src;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-ps-tests", Guid.NewGuid().ToString("N"));
            _project = Path.Combine(_root, "project");
            _src = Path.Combine(_project, "scripts");
            Directory.CreateDirectory(Path.Combine(_src, "lib"));
            Directory.CreateDirectory(Path.Combine(_src, "Store"));

            File.WriteAllText(Path.Combine(_src, "lib", "util.ps1"),
                "function Get-Greeting {\n    [OutputType([string])]\n    param(\n        [Parameter(Mandatory)][string]$Name,\n        [int]$Times = 2,\n        [AllowNull()]$Extra = $null,\n        [ref]$Out,\n        [Parameter(ValueFromRemainingArguments)]$Rest\n    )\n    Write-Output \"Hi $Name\"\n}\n");

            File.WriteAllText(Path.Combine(_src, "Store", "Store.psm1"),
                "enum Color { Red; Green = 3 }\n\n" +
                "class Item {\n    [string]$Name\n}\n\n" +
                "class Store : Item, System.IDisposable {\n" +
                "    hidden [int]$_count\n    static [Store]$Default\n\n" +
                "    Store([string]$name) { $this.Name = $name; $this.Reset() }\n" +
                "    [void] Reset() { $this._count = 0 }\n" +
                "    [Item] Add([Item]$item) { $this._count++; return $item }\n" +
                "    static [Store] Create() { return [Store]::new('x') }\n" +
                "    [void] Dispose() { }\n}\n\n" +
                "function Get-Store { [Store]::Create() }\nfunction Hidden-Helper { 1 }\n" +
                "Export-ModuleMember -Function Get-Store\n");

            File.WriteAllText(Path.Combine(_src, "main.ps1"),
                "#Requires -Modules Pester\nusing module ./Store/Store.psm1\nparam([string]$Env = 'dev', [switch]$DryRun)\n\n" +
                ". $PSScriptRoot\\lib\\util.ps1\nImport-Module (Join-Path $PSScriptRoot 'Store/Store.psd1')\nImport-Module Az.Accounts\n\n" +
                "$script:Config = Get-Content config.json | ConvertFrom-Json\n[int]$Retries = 3\n$env:FOO = 'bar'\n\n" +
                "function Invoke-Main {\n    param([string]$Path)\n    function Inner { Get-Greeting -Name 'x' }\n" +
                "    $s = [Store]::new('a')\n    $s.Add((New-Object Item))\n    $s.Dispose()\n    Inner\n    Get-ChildItem $Path\n" +
                "    $Maps.Moves.ContainsKey('a'); $other.ContainsKey('b')\n}\n\n" +
                "Invoke-Main -Path .\n& \"$PSScriptRoot\\lib\\util.ps1\" -Name y\n");
        }

        [TestCleanup]
        public void Cleanup() { try { Directory.Delete(_root, true); } catch { } }

        private CodeExtractionRequest Request()
        {
            var files = Directory.GetFiles(_src, "*.ps*1", SearchOption.AllDirectories).OrderBy(f => f)
                .Select(f => new CodeSourceFile(f, CodeGraphPaths.ToProjectRelative(_project, f))).ToList();
            return new CodeExtractionRequest { ProjectPath = _project, AllFiles = files, FilesToEmit = files };
        }

        [TestMethod]
        public void Extract_modules_functions_classes_parameters_imports_calls_and_instances()
        {
            var extractor = new PowerShellExtractor(NullLogger<PowerShellExtractor>.Instance);
            var result = extractor.Extract(Request());
            Assert.AreEqual(0, result.Warnings.Count, string.Join("; ", result.Warnings));
            Assert.AreEqual(3, result.Scripts.Count);
            var main = result.Scripts.Single(s => s.File.LogicalPath == "scripts/main.ps1").Script;
            var util = result.Scripts.Single(s => s.File.LogicalPath == "scripts/lib/util.ps1").Script;
            var store = result.Scripts.Single(s => s.File.LogicalPath == "scripts/Store/Store.psm1").Script;
            foreach (var s in new[] { main, util, store }) Assert.AreEqual("powershell", s.Header.Language);

            // Moduli.
            var mainModule = main.Symbols.Single(s => s.Kind == "module");
            var utilModule = util.Symbols.Single(s => s.Kind == "module");
            var storeModule = store.Symbols.Single(s => s.Kind == "module");
            Assert.AreEqual("scripts/main", mainModule.QualifiedName);
            Assert.AreEqual("scripts/Store/Store", storeModule.QualifiedName);

            // Funzione con parametri: tipo, default, nullable, ref, remaining; [OutputType] come ritorno.
            var greet = util.Symbols.Single(s => s.Kind == "function");
            Assert.AreEqual("scripts/lib/util:Get-Greeting", greet.QualifiedName);
            Assert.AreEqual("string", greet.ReturnType);
            Assert.AreEqual("function Get-Greeting([string]$Name, [int]$Times = 2, $Extra = $null, [ref]$Out, $Rest)", greet.Signature);
            var gp = util.Parameters.Where(p => p.SymbolId == greet.Id).OrderBy(p => p.Position).ToList();
            CollectionAssert.AreEqual(new[] { "Name", "Times", "Extra", "Out", "Rest" }, gp.Select(p => p.Name).ToArray());
            Assert.AreEqual("string", gp[0].ParameterType); Assert.AreEqual("2", gp[1].DefaultValue);
            Assert.IsTrue(gp[2].IsNullable); Assert.AreEqual("ref", gp[3].Direction); Assert.AreEqual("params", gp[4].Direction);
            Assert.IsFalse(util.Edges.Any(e => e.Relation == "references"), "string/int non sono riferimenti del progetto");

            // Classi, enum, membri; nessuna funzione spuria dai metodi.
            var storeClass = store.Symbols.Single(s => s.Kind == "class" && s.Name == "Store");
            var item = store.Symbols.Single(s => s.Kind == "class" && s.Name == "Item");
            Assert.AreEqual("class Store : Item, System.IDisposable", storeClass.Signature);
            Assert.IsTrue(store.Edges.Any(e => e.SourceId == storeClass.Id && e.TargetId == item.Id && e.Relation == "inherits"));
            var idisp = store.Symbols.Single(s => s.Kind == "external" && s.Name == "System.IDisposable");
            Assert.IsTrue(store.Edges.Any(e => e.SourceId == storeClass.Id && e.TargetId == idisp.Id && e.Relation == "implements"));
            var color = store.Symbols.Single(s => s.Kind == "enum");
            CollectionAssert.AreEquivalent(new[] { "Red", "Green" }, store.Symbols.Where(s => s.Kind == "field" && s.ParentId == color.Id).Select(s => s.Name).ToArray());
            var ctor = store.Symbols.Single(s => s.Kind == "constructor");
            Assert.AreEqual("scripts/Store/Store:Store.Store", ctor.QualifiedName);
            Assert.AreEqual("name", store.Parameters.Single(p => p.SymbolId == ctor.Id).Name);
            var add = store.Symbols.Single(s => s.Kind == "method" && s.Name == "Add");
            Assert.AreEqual("[Item] Add([Item]$item)", add.Signature); Assert.AreEqual("Item", add.ReturnType);
            Assert.IsTrue(store.Edges.Any(e => e.SourceId == add.Id && e.TargetId == item.Id && e.Relation == "references"));
            Assert.IsTrue(store.Symbols.Single(s => s.Name == "Create").IsStatic);
            var count = store.Symbols.Single(s => s.Name == "_count");
            Assert.AreEqual("property", count.Kind); Assert.AreEqual("private", count.Visibility);
            Assert.IsTrue(store.Symbols.Single(s => s.Name == "Default").IsStatic);
            CollectionAssert.AreEquivalent(new[] { "Get-Store", "Hidden-Helper" }, store.Symbols.Where(s => s.Kind == "function").Select(s => s.Name).ToArray());

            // Export-ModuleMember: la funzione non esportata è privata.
            Assert.AreEqual("public", store.Symbols.Single(s => s.Name == "Get-Store").Visibility);
            Assert.AreEqual("private", store.Symbols.Single(s => s.Name == "Hidden-Helper").Visibility);

            // $this.Reset() nel costruttore → extracted; [Store]::new() → instantiates; [Store]::Create() → calls.
            var reset = store.Symbols.Single(s => s.Name == "Reset");
            Assert.IsTrue(store.Edges.Any(e => e.SourceId == ctor.Id && e.TargetId == reset.Id && e.Relation == "calls" && e.Confidence == "extracted"));
            var create = store.Symbols.Single(s => s.Name == "Create");
            Assert.IsTrue(store.Edges.Any(e => e.SourceId == create.Id && e.TargetId == storeClass.Id && e.Relation == "instantiates"));
            var getStore = store.Symbols.Single(s => s.Name == "Get-Store");
            Assert.IsTrue(store.Edges.Any(e => e.SourceId == getStore.Id && e.TargetId == create.Id && e.Relation == "calls" && e.Confidence == "inferred"));

            // Import: dot-sourcing, using module, Import-Module con Join-Path (psd1 → psm1 per nome), esterni.
            Assert.IsTrue(main.Edges.Any(e => e.SourceId == mainModule.Id && e.TargetId == utilModule.Id && e.Relation == "imports" && e.Confidence == "extracted"), "dot-sourcing");
            Assert.AreEqual(1, main.Edges.Count(e => e.SourceId == mainModule.Id && e.TargetId == storeModule.Id && e.Relation == "imports"), "using module + Import-Module → un arco");
            foreach (var ext in new[] { "Pester", "Az.Accounts" })
            {
                var m = main.Symbols.Single(s => s.Kind == "external" && s.Name == ext);
                Assert.IsTrue(main.Edges.Any(e => e.SourceId == mainModule.Id && e.TargetId == m.Id && e.Relation == "imports"), ext);
            }

            // Variabili di script: $script:Config e [int]$Retries, non $env:FOO.
            var fields = main.Symbols.Where(s => s.Kind == "field").ToList();
            CollectionAssert.AreEquivalent(new[] { "Config", "Retries" }, fields.Select(s => s.Name).ToArray());
            Assert.AreEqual("int", fields.Single(s => s.Name == "Retries").ReturnType);
            var config = fields.Single(s => s.Name == "Config");
            Assert.IsTrue(main.Edges.Any(e => e.SourceId == config.Id && e.Relation == "calls" && main.Symbols.Single(x => x.Id == e.TargetId).Name == "Get-Content"));

            // Funzione annidata, chiamate e istanze dentro Invoke-Main; codice di script attribuito al modulo.
            var invoke = main.Symbols.Single(s => s.Name == "Invoke-Main");
            var inner = main.Symbols.Single(s => s.Name == "Inner");
            Assert.AreEqual("scripts/main:Invoke-Main.Inner", inner.QualifiedName);
            Assert.AreEqual(invoke.Id, inner.ParentId);
            Assert.IsTrue(main.Edges.Any(e => e.SourceId == inner.Id && e.TargetId == greet.Id && e.Relation == "calls" && e.Confidence == "inferred"));
            Assert.IsTrue(main.Edges.Any(e => e.SourceId == invoke.Id && e.TargetId == inner.Id && e.Relation == "calls"));
            Assert.IsTrue(main.Edges.Any(e => e.SourceId == invoke.Id && e.TargetId == storeClass.Id && e.Relation == "instantiates"));
            Assert.IsTrue(main.Edges.Any(e => e.SourceId == invoke.Id && e.TargetId == item.Id && e.Relation == "instantiates"), "New-Object Item");
            Assert.IsTrue(main.Edges.Any(e => e.SourceId == invoke.Id && e.TargetId == add.Id && e.Relation == "calls" && e.Confidence == "ambiguous"), "$s.Add(): il tipo di $s non si sa");
            var containsKey = main.Symbols.Single(s => s.Kind == "external" && s.Name == "ContainsKey");
            Assert.AreEqual(".ContainsKey()", containsKey.QualifiedName, "un nodo per nome di metodo, non per espressione");
                        var gci = main.Symbols.Single(s => s.Kind == "external" && s.Name == "Get-ChildItem");
            Assert.IsTrue(main.Edges.Any(e => e.SourceId == invoke.Id && e.TargetId == gci.Id && e.Relation == "calls"));
            Assert.IsTrue(main.Edges.Any(e => e.SourceId == mainModule.Id && e.TargetId == invoke.Id && e.Relation == "calls"), "Invoke-Main al livello dello script");
            Assert.IsFalse(main.Edges.Any(e => e.SourceId == invoke.Id && e.TargetId == greet.Id), "la chiamata di Inner non è di Invoke-Main");

            // Il param() dello script sono i parametri del modulo; uno script lanciato per percorso è chiamato.
            var mp = main.Parameters.Where(p => p.SymbolId == mainModule.Id).OrderBy(p => p.Position).ToList();
            CollectionAssert.AreEqual(new[] { "Env", "DryRun" }, mp.Select(p => p.Name).ToArray());
            Assert.AreEqual("'dev'", mp[0].DefaultValue); Assert.AreEqual("switch", mp[1].ParameterType);
            Assert.IsTrue(main.Edges.Any(e => e.SourceId == mainModule.Id && e.TargetId == utilModule.Id && e.Relation == "calls" && e.Confidence == "extracted"), "& $PSScriptRoot\\lib\\util.ps1");

                        // Lo script si rilegge: tipi, relazioni e confidenze ammessi dal formato.
            foreach (var (file, script) in result.Scripts)
                CodeScriptParser.Parse(CodeScriptWriter.Serialize(script), file.LogicalPath);
        }

        [TestMethod]
        public void Emit_partial_tree_and_warn_on_syntax_errors()
        {
            File.WriteAllText(Path.Combine(_src, "broken.ps1"), "function Ok { 1 }\nfunction Broken {\n  if ($x {\n");
            var result = new PowerShellExtractor(NullLogger<PowerShellExtractor>.Instance).Extract(Request());
            Assert.IsTrue(result.Warnings.Any(w => w.StartsWith("scripts/broken.ps1:") && w.Contains("syntax errors")), string.Join("; ", result.Warnings));
            var broken = result.Scripts.Single(s => s.File.LogicalPath == "scripts/broken.ps1").Script;
            Assert.IsTrue(broken.Symbols.Any(s => s.Name == "Ok"));
        }

        [TestMethod]
        public void Catalog_maps_ps1_and_psm1_to_powershell()
        {
            Assert.AreEqual("powershell", CodeLanguages.ForFile("a/b.ps1").Id);
            Assert.AreEqual("powershell", CodeLanguages.ForFile("a/b.PSM1").Id);
            Assert.IsNull(CodeLanguages.ForFile("a/b.psd1"));
            var registry = new CodeExtractorRegistry(new ICodeExtractor[] { new PowerShellExtractor(NullLogger<PowerShellExtractor>.Instance) });
            Assert.AreEqual("ready", registry.Availability("powershell"));
        }
    }
}
