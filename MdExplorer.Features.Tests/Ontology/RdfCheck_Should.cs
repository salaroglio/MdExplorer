using System;
using System.IO;
using System.Linq;
using MdExplorer.Features.Ontology;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Ontology
{
    /// <summary>La verifica di un'ontologia sui file del progetto: Turtle, regole SHACL, domande di competenza in SPARQL.</summary>
    [TestClass]
    public class RdfCheck_Should
    {
        private string _root;

        private const string Prefixes = "@prefix ex: <https://example.org/gara#> .\n@prefix owl: <http://www.w3.org/2002/07/owl#> .\n" +
                                        "@prefix xsd: <http://www.w3.org/2001/XMLSchema#> .\n@prefix sh: <http://www.w3.org/ns/shacl#> .\n";

        [TestInitialize]
        public void Project()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-rdfcheck-" + Guid.NewGuid().ToString("N"));
            Write("ontology/TBox/gara.ttl", Prefixes + "ex:Bando a owl:Class .\nex:codice a owl:DatatypeProperty ; owl:domain ex:Bando .\n");
            Write("ontology/ABox/bandi.ttl", Prefixes + "ex:b1 a ex:Bando ; ex:codice \"NC-2027-014\" .\nex:b2 a ex:Bando .\n");
            Write("ontology/Shapes/gara.shapes.ttl", Prefixes +
                  "ex:BandoShape a sh:NodeShape ; sh:targetClass ex:Bando ;\n  sh:property [ sh:path ex:codice ; sh:minCount 1 ; sh:message \"un bando ha il suo codice\" ] .\n");
            Write("ontology/domande/c1-ci-sono-bandi.rq", "PREFIX ex: <https://example.org/gara#>\nASK { ?b a ex:Bando }\n");
            Write("ontology/domande/c2-codici.rq", "PREFIX ex: <https://example.org/gara#>\nSELECT ?b ?codice WHERE { ?b ex:codice ?codice }\n");
            Write("ontology/domande/c3-quanti.rq", "PREFIX ex: <https://example.org/gara#>\nSELECT (COUNT(?b) AS ?n) WHERE { ?b a ex:Bando }\n");
        }

        [TestCleanup]
        public void Remove() { try { Directory.Delete(_root, true); } catch { } }

        private void Write(string file, string text)
        {
            var full = Path.Combine(_root, file);
            Directory.CreateDirectory(Path.GetDirectoryName(full));
            File.WriteAllText(full, text);
        }

        [TestMethod]
        public void Read_validate_and_answer_the_competency_questions()
        {
            var report = RdfCheck.Check(_root, new[] { "ontology" }, new[] { "ontology/Shapes" }, new[] { "ontology/domande" });

            CollectionAssert.AreEquivalent(new[] { "ontology/TBox/gara.ttl", "ontology/ABox/bandi.ttl", "ontology/Shapes/gara.shapes.ttl" },
                                           report.Files.Select(f => f.Path).ToArray(), "le regole non sono contate fra i dati");
            Assert.IsTrue(report.Files.All(f => f.Error == null));
            Assert.AreEqual("shapes", report.Files.Single(f => f.Path.EndsWith("shapes.ttl")).Role);

            Assert.IsFalse(report.Shacl.Conforms, "b2 non ha il codice");
            var violation = report.Shacl.Violations.Single();
            Assert.AreEqual("ex:b2", violation.FocusNode);
            Assert.AreEqual("un bando ha il suo codice", violation.Message);

            var ask = report.Queries.Single(q => q.Path == "ontology/domande/c1-ci-sono-bandi.rq");
            Assert.AreEqual("ask", ask.Kind);
            Assert.AreEqual(true, ask.Answer);
            var select = report.Queries.Single(q => q.Path == "ontology/domande/c2-codici.rq");
            Assert.AreEqual("select", select.Kind);
            Assert.AreEqual(1, select.RowCount);
            Assert.AreEqual("\"NC-2027-014\"", select.Rows.Single()["codice"]);
            Assert.AreEqual("2", report.Queries.Single(q => q.Path.EndsWith("c3-quanti.rq")).Rows.Single()["n"], "un numero senza lo spazio del formattatore");
            Assert.IsFalse(report.Ok, "le regole non sono rispettate");
        }

        [TestMethod]
        public void Stop_at_a_file_that_cannot_be_read_and_say_where()
        {
            Write("ontology/ABox/rotto.ttl", Prefixes + "ex:b3 a ex:Bando ;\n  ex:codice \"senza fine .\n");
            Write("ontology/domande/c3-rotta.rq", "SELECT ?x WHERE { ?x \n");

            var report = RdfCheck.Check(_root, new[] { "ontology/TBox", "ontology/ABox" }, new[] { "ontology/Shapes" }, new[] { "ontology/domande" });

            var broken = report.Files.Single(f => f.Path == "ontology/ABox/rotto.ttl");
            Assert.IsNotNull(broken.Error);
            StringAssert.Contains(broken.Error, "line");
            Assert.IsNull(report.Shacl, "con dati incompleti SHACL direbbe cose false");
            Assert.AreEqual(0, report.Queries.Count);
            Assert.IsNotNull(report.Note);

            File.Delete(Path.Combine(_root, "ontology/ABox/rotto.ttl"));
            var again = RdfCheck.Check(_root, new[] { "ontology/TBox", "ontology/ABox" }, null, new[] { "ontology/domande" });
            Assert.IsNotNull(again.Queries.Single(q => q.Path.EndsWith("c3-rotta.rq")).Error, "una query che non si legge è un errore suo");
            Assert.IsNull(again.Shacl, "senza regole niente SHACL");
        }

        [TestMethod]
        public void Refuse_paths_outside_the_project_or_missing_and_an_empty_check()
        {
            StringAssert.Contains(Assert.ThrowsException<ArgumentException>(() => RdfCheck.Check(_root, new[] { "../fuori" })).Message, "outside the project");
            StringAssert.Contains(Assert.ThrowsException<ArgumentException>(() => RdfCheck.Check(_root, new[] { "ontology/manca.ttl" })).Message, "is not in the project");
            StringAssert.Contains(Assert.ThrowsException<ArgumentException>(() => RdfCheck.Check(_root, new[] { "ontology/domande" })).Message, "No .ttl file");
        }
    }
}
