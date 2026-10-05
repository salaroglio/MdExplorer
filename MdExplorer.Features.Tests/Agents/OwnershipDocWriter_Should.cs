using System;
using System.Linq;
using MdExplorer.Features.Agents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    [TestClass]
    public class OwnershipDocWriter_Should
    {
        [TestMethod]
        public void Start_a_valid_document_when_there_is_none()
        {
            var doc = OwnershipDocWriter.AddRow(null, "account-manager", "Anna", "Anna@Pentagroup.it");

            var parsed = OwnershipDocParser.Parse(doc);
            Assert.IsTrue(parsed.IsOwnershipDoc);
            Assert.IsFalse(parsed.HasErrors, string.Join(" | ", parsed.Errors));
            var row = parsed.Entries.Single();
            Assert.AreEqual("anna@pentagroup.it", row.GitEmail);
            Assert.AreEqual("Anna", row.Responsible);
            CollectionAssert.AreEqual(new[] { "account-manager" }, row.Agents.ToList());
            StringAssert.Contains(doc, "## TL;DR");
        }

        [TestMethod]
        public void Accept_a_table_that_has_no_rows_yet()
        {
            Assert.IsFalse(OwnershipDocParser.Parse(OwnershipDocWriter.NewDocument).HasErrors, "an empty table is a valid document");
        }

        [TestMethod]
        public void Add_a_row_after_the_last_one_and_leave_the_rest_alone()
        {
            const string doc = "---\nmde_type: ownership\n---\n# Chi fa cosa\n\nTesto del gruppo.\n\n" +
                               "| Agenti | Git Email | Ambito |\n|---|---|---|\n| tecnico | marco@x.it | Tecnica |\n\nNota in fondo.\n";

            var written = OwnershipDocWriter.AddRow(doc, "legale", "Anna", "anna@x.it");

            StringAssert.Contains(written, "| tecnico | marco@x.it | Tecnica |\n| legale | anna@x.it | legale |\n\nNota in fondo.");
            StringAssert.Contains(written, "Testo del gruppo.");
            var parsed = OwnershipDocParser.Parse(written);
            Assert.IsFalse(parsed.HasErrors, string.Join(" | ", parsed.Errors));
            Assert.AreEqual(2, parsed.Entries.Count, "the columns keep the order the team gave them");
            Assert.AreEqual("anna@x.it", parsed.Entries[1].GitEmail);
        }

        [TestMethod]
        public void Keep_the_ambit_unique()
        {
            var doc = OwnershipDocWriter.AddRow(null, "legale", "Anna", "anna@x.it");
            doc = OwnershipDocWriter.AddRow(doc, "legale", "Anna", "anna@x.it");

            var parsed = OwnershipDocParser.Parse(doc);
            Assert.IsFalse(parsed.HasErrors, string.Join(" | ", parsed.Errors));
            CollectionAssert.AreEqual(new[] { "legale", "legale (2)" }, parsed.Entries.Select(e => e.Scope).ToList());
        }

        [TestMethod]
        public void Keep_windows_line_endings()
        {
            var doc = OwnershipDocWriter.NewDocument.Replace("\n", "\r\n");

            var written = OwnershipDocWriter.AddRow(doc, "legale", "Anna", "anna@x.it");

            Assert.IsFalse(written.Replace("\r\n", "").Contains('\n'), "no bare line feed slips in");
            Assert.AreEqual(1, OwnershipDocParser.Parse(written).Entries.Count);
        }

        [TestMethod]
        public void Refuse_a_document_without_a_usable_table_and_say_what_to_add()
        {
            var noTable = Assert.ThrowsException<InvalidOperationException>(
                () => OwnershipDocWriter.AddRow("---\nmde_type: ownership\n---\nSolo testo.\n", "legale", "Anna", "anna@x.it"));
            StringAssert.Contains(noTable.Message, "non ha una tabella");

            var noAgents = Assert.ThrowsException<InvalidOperationException>(
                () => OwnershipDocWriter.AddRow("---\nmde_type: ownership\n---\n| Ambito | Git Email |\n|--|--|\n", "legale", "Anna", "anna@x.it"));
            StringAssert.Contains(noAgents.Message, "'Agenti'");
        }

        [TestMethod]
        public void Not_let_a_name_break_the_table()
        {
            var doc = OwnershipDocWriter.AddRow(null, "legale", "Anna | Legale\nUfficio", "anna@x.it");

            var parsed = OwnershipDocParser.Parse(doc);
            Assert.IsFalse(parsed.HasErrors, string.Join(" | ", parsed.Errors));
            Assert.AreEqual("Anna   Legale Ufficio", parsed.Entries.Single().Responsible);
        }
    }
}
