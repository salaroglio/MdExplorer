using System;
using System.IO;
using System.Linq;
using MdExplorer.Features.Diagnostics;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Diagnostics
{
    /// <summary>
    /// Le rilevazioni di MarkAgent allegate a «Invia diagnostica» (11/11/2026): quelle del grafo e i pacchetti di linguaggio
    /// che ne hanno, con i progetti numerati (il nome di un progetto è spesso quello del cliente).
    /// </summary>
    [TestClass]
    public class DiagnosticsFindings_Should
    {
        private string _root;

        [TestInitialize]
        public void Setup() => _root = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "mde-findings-tests", Guid.NewGuid().ToString("N"))).FullName;

        [TestCleanup]
        public void Cleanup() { try { Directory.Delete(_root, true); } catch (IOException) { } }

        private string Write(string project, string rel, string text)
        {
            var path = Path.Combine(_root, project, rel.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, text);
            return path;
        }

        [TestMethod]
        public void Take_the_graph_findings_and_the_packages_that_have_some_under_numbered_projects()
        {
            Write("ClienteRossi", ".mde-code/_findings/rilevazione-2026-11-12.md", "# Rilevazione\n");
            Write("ClienteRossi", ".mde-code/src/A.cs.ast.ttl", "non è una rilevazione");
            Write("ClienteRossi", ".mde-languages/toy/language.yml", "schema: 1\n");
            Write("ClienteRossi", ".mde-languages/toy/findings/rilevazione.md", "# Toy\n");
            Write("ClienteRossi", ".mde-languages/toy/skills/toy-code/SKILL.md", "skill\n");
            Write("ClienteRossi", ".mde-languages/altro/language.yml", "schema: 1\n");          // senza rilevazioni: resta dov'è
            Write("SenzaNiente", "README.md", "# niente\n");
            Write("ClienteBianchi", ".mde-code/_findings/a.md", "# A\n");
            Write("ClienteBianchi", ".mde-code/_findings/sorgente.cs", "class X { }");           // solo .md

            var files = DiagnosticsFindings.Collect(new[] { "ClienteRossi", "SenzaNiente", "NonEsiste", "ClienteBianchi" }.Select(p => Path.Combine(_root, p)));

            CollectionAssert.AreEqual(new[]
            {
                "rilevazioni/progetto-1/codice/rilevazione-2026-11-12.md",
                "rilevazioni/progetto-1/linguaggi/toy/findings/rilevazione.md",
                "rilevazioni/progetto-1/linguaggi/toy/language.yml",
                "rilevazioni/progetto-1/linguaggi/toy/skills/toy-code/SKILL.md",
                "rilevazioni/progetto-2/codice/a.md",
            }, files.Select(f => f.Name).ToArray());
            Assert.IsFalse(files.Any(f => f.Name.Contains("Cliente")), "il nome del progetto non parte");
        }

        [TestMethod]
        public void Leave_out_a_file_too_large_to_be_a_text()
        {
            Write("P", ".mde-code/_findings/enorme.md", new string('x', (int)DiagnosticsFindings.MaxFileBytes + 1));
            Assert.AreEqual(0, DiagnosticsFindings.Collect(new[] { Path.Combine(_root, "P") }).Count);
            Assert.AreEqual(0, DiagnosticsFindings.Collect(null).Count);
        }
    }
}
