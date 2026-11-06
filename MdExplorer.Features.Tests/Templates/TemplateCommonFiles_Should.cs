using System;
using System.Collections.Generic;
using System.Linq;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Templates;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Templates
{
    /// <summary>
    /// Ciò che MDE scrive per un metodo nei file comuni del progetto (P2 del piano dei template): il blocco delle istruzioni
    /// e i ruoli aperti. Il resto dei file è del gruppo e non si tocca.
    /// </summary>
    [TestClass]
    public class TemplateCommonFiles_Should
    {
        [TestMethod]
        public void Append_the_block_of_a_method_and_then_replace_only_it()
        {
            var first = TemplateInstructions.Apply("# Gruppo\n\nRispondi in italiano.\n", "ddd", "Usa il glossario.\n");
            Assert.AreEqual("# Gruppo\n\nRispondi in italiano.\n\n<!-- mde-template:ddd -->\nUsa il glossario.\n<!-- /mde-template:ddd -->\n", first);

            var both = TemplateInstructions.Apply(first, "iso", "Registra il riesame.");
            var updated = TemplateInstructions.Apply(both, "ddd", "Usa il glossario aggiornato.");
            StringAssert.Contains(updated, "<!-- mde-template:ddd -->\nUsa il glossario aggiornato.\n<!-- /mde-template:ddd -->");
            StringAssert.Contains(updated, "<!-- mde-template:iso -->\nRegistra il riesame.\n<!-- /mde-template:iso -->", "l'altro metodo resta");
            StringAssert.StartsWith(updated, "# Gruppo\n\nRispondi in italiano.\n");

            Assert.AreEqual("<!-- mde-template:ddd -->\nA\nB\n<!-- /mde-template:ddd -->\n", TemplateInstructions.Apply(null, "ddd", "A\r\nB"), "il file non c'era: solo il blocco");
            Assert.AreEqual("# G\r\n\r\n<!-- mde-template:ddd -->\r\nA\r\n<!-- /mde-template:ddd -->\r\n", TemplateInstructions.Apply("# G\r\n", "ddd", "A\n"), "gli a capo del file");
        }

        [TestMethod]
        public void Refuse_a_block_that_is_not_whole()
        {
            var ex = Assert.ThrowsException<InvalidOperationException>(() => TemplateInstructions.Apply("<!-- mde-template:ddd -->\ntesto\n", "ddd", "x"));
            StringAssert.Contains(ex.Message, "is not whole");
        }

        [TestMethod]
        public void Add_the_missing_roles_as_open_rows_and_leave_the_others()
        {
            const string doc = "---\nmde_type: ownership\n---\n# Responsabilità\n\n| Ruolo | Descrizione | Responsabile | Git Email | Agenti |\n|---|---|---|---|---|\n| Product owner | | Anna | anna@x.it | |\n\nTesto dopo.\n";
            var roles = new List<(string, string, IReadOnlyList<string>)>
            {
                ("Product owner", null, new[] { "ddd-po" }),
                ("Analista DDD", "modella il dominio", new[] { "ddd-analista", "ddd-revisore" }),
            };

            var text = OwnershipDocWriter.AddOpenRoles(doc, roles, out var added);

            CollectionAssert.AreEqual(new[] { "Analista DDD" }, added.ToList(), "un ruolo che c'è già resta com'è: il gruppo l'ha riempito");
            Assert.AreEqual("---\nmde_type: ownership\n---\n# Responsabilità\n\n| Ruolo | Descrizione | Responsabile | Git Email | Agenti |\n|---|---|---|---|---|\n| Product owner | | Anna | anna@x.it | |\n" +
                            "| Analista DDD | modella il dominio |  |  | ddd-analista, ddd-revisore |\n\nTesto dopo.\n", text);
            var parsed = OwnershipDocParser.Parse(text);
            Assert.IsTrue(parsed.OpenRoles.Any(r => r.Role == "Analista DDD"), "è un ruolo aperto");
        }
    }
}
