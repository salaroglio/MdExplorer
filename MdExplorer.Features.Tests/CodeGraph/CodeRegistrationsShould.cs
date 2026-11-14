using System.Collections.Generic;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>Le registrazioni (30/10/2026): i file ServiceLoader letti come li legge Java, e gli archi resolvesTo.</summary>
    [TestClass]
    public class CodeRegistrationsShould
    {
        [TestMethod]
        public void Read_a_service_loader_file_as_java_reads_it()
        {
            Assert.IsTrue(CodeRegistrations.IsServiceLoaderFile("@core/src/main/resources/META-INF/services/com.x.Exporter"));
            Assert.IsFalse(CodeRegistrations.IsServiceLoaderFile("src/META-INF/MANIFEST.MF"));
            Assert.IsFalse(CodeRegistrations.IsServiceLoaderFile("src/services/com.x.Exporter"));

            var list = CodeRegistrations.ParseServiceLoader("r/META-INF/services/com.x.Exporter$Kind", "﻿# commento\r\ncom.x.Pdf\r\n\r\n  com.x.Outer$Csv   # interna\r\n");
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(("com.x.Exporter.Kind", "com.x.Pdf", 2), (list[0].AbstractionQualifiedName, list[0].ImplementationQualifiedName, list[0].Line));
            Assert.AreEqual(("com.x.Outer.Csv", 4), (list[1].ImplementationQualifiedName, list[1].Line), "la classe interna col punto, come nel grafo");
        }

        private static CodeOwnerMember M(string owner, string ownerQn, string member = null, string name = null)
            => new() { OwnerId = owner, OwnerQualifiedName = ownerQn, OwnerName = ownerQn.Split('.').Last(), MemberId = member, MemberName = name };

        [TestMethod]
        public void Link_the_interface_and_each_method_to_the_registered_class_and_say_what_is_missing()
        {
            var regs = CodeRegistrations.ParseServiceLoader("p/META-INF/services/x.Exporter", "x.Pdf\nx.Over\nlib.Gone\n");
            var owners = new List<CodeOwnerMember>
            {
                M("I", "x.Exporter", "I.export", "export"), M("I", "x.Exporter", "I.close", "close"),
                M("P", "x.Pdf", "P.export", "export"),                                                   // close ereditato: niente arco
                M("O", "x.Over", "O.export1", "export"), M("O", "x.Over", "O.export2", "export"), M("O", "x.Over", "O.close", "close"),
            };
            var edges = CodeRegistrations.Edges(regs, owners, out var misses);
            string Of(string s, string t) => edges.Single(e => e.SourceId == s && e.TargetId == t).Confidence;
            Assert.AreEqual("extracted", Of("I", "P"));
            Assert.AreEqual("extracted", Of("I.export", "P.export"));
            Assert.IsFalse(edges.Any(e => e.SourceId == "I.close" && e.TargetId.StartsWith("P")), "un metodo che la classe non dichiara non si inventa");
            Assert.AreEqual("ambiguous", Of("I.export", "O.export1"), "overload: il nome non basta a scegliere");
            Assert.AreEqual("extracted", Of("I.close", "O.close"));
            Assert.IsTrue(edges.All(e => e.Relation == CodeRegistrations.Relation && e.Path == "p/META-INF/services/x.Exporter"));
            Assert.AreEqual(1, misses.Count);
            StringAssert.Contains(misses[0], "lib.Gone");
        }
    }
}
