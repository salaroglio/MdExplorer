using System.Linq;
using MdExplorer.Features.Agents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    [TestClass]
    public class OwnershipDocParser_Should
    {
        private const string ValidDoc = @"---
mde_type: ownership
---
# Ownership

| Ruolo | Descrizione | Responsabile | Git Email | Agenti |
|--------|-------------|--------------|-----------|--------|
| WSAA   | Import TOT  | Carlo        | carlo@x.it | analyst, dev |
| Batch  | Notturni    | Marco        | marco@y.it | dev |
";

        [TestMethod]
        public void Ignore_a_document_without_the_ownership_type()
        {
            var plain = "---\nmde_type: sprint-plan\n---\n# not ownership\n";
            var result = OwnershipDocParser.Parse(plain);
            Assert.IsFalse(result.IsOwnershipDoc);
            Assert.AreEqual(0, result.Entries.Count);
        }

        [TestMethod]
        public void Ignore_a_document_without_any_front_matter()
        {
            var result = OwnershipDocParser.Parse("# just markdown\n\nno yaml here");
            Assert.IsFalse(result.IsOwnershipDoc);
        }

        [TestMethod]
        public void Parse_role_responsible_email_and_agents()
        {
            var result = OwnershipDocParser.Parse(ValidDoc);

            Assert.IsTrue(result.IsOwnershipDoc);
            Assert.IsFalse(result.HasErrors, string.Join(" | ", result.Errors));
            Assert.AreEqual(2, result.Entries.Count);

            var wsaa = result.Entries.First(e => e.Role == "WSAA");
            Assert.AreEqual("Carlo", wsaa.Responsible);
            Assert.AreEqual("carlo@x.it", wsaa.GitEmail);
            CollectionAssert.AreEquivalent(new[] { "analyst", "dev" }, wsaa.Agents.ToArray());
        }

        [TestMethod]
        public void Lowercase_the_email_and_split_agents_on_separators()
        {
            var doc = @"---
mde_type: ownership
---
| Role | Git Email | Agents |
|------|-----------|--------|
| S1 | CARLO@X.IT | a; b, c |
";
            var e = OwnershipDocParser.Parse(doc).Entries.Single();
            Assert.AreEqual("carlo@x.it", e.GitEmail);
            CollectionAssert.AreEquivalent(new[] { "a", "b", "c" }, e.Agents.ToArray());
        }

        [TestMethod]
        public void Read_the_rows_of_one_role_as_its_team()
        {
            var doc = @"---
mde_type: ownership
---
| Ruolo | Git Email |
|-------|-----------|
| WSAA | a@x.it |
| WSAA | b@x.it |
";
            var result = OwnershipDocParser.Parse(doc);
            Assert.IsFalse(result.HasErrors, string.Join(" | ", result.Errors));
            CollectionAssert.AreEqual(new[] { "a@x.it", "b@x.it" }, result.Entries.Select(e => e.GitEmail).ToList());
        }

        [DataTestMethod]
        [DataRow("Ambito")]
        [DataRow("Scope")]
        public void Ask_to_rename_the_old_column(string header)
        {
            var doc = $"---\nmde_type: ownership\n---\n| {header} | Git Email |\n|--|--|\n| WSAA | a@x.it |\n";
            var result = OwnershipDocParser.Parse(doc);
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(0, result.Entries.Count, "a renamed column is never read");
            StringAssert.Contains(result.Errors.Single(), "«Ruolo»");
        }

        [TestMethod]
        public void Flag_a_missing_email_row()
        {
            // Una persona senza email non si riconosce: è un errore, non un ruolo aperto.
            var doc = @"---
mde_type: ownership
---
| Ruolo | Responsabile | Git Email |
|-------|--------------|-----------|
| WSAA | Anna |  |
";
            var result = OwnershipDocParser.Parse(doc);
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(0, result.Entries.Count);
            Assert.AreEqual(0, result.OpenRoles.Count);
        }

        [TestMethod]
        public void Read_a_row_without_a_person_as_a_role_nobody_took_yet()
        {
            var doc = @"---
mde_type: ownership
---
| Ruolo | Responsabile | Git Email | Agenti |
|-------|--------------|-----------|--------|
| Account manager |  |  | account-manager |
| Tecnica | Marco | marco@x.it | responsabile-tecnico |
| Direzione |  |  |  |
";
            var result = OwnershipDocParser.Parse(doc);
            Assert.IsFalse(result.HasErrors, string.Join(" | ", result.Errors));
            Assert.AreEqual("marco@x.it", result.Entries.Single().GitEmail, "in Entries solo le righe di una persona");
            CollectionAssert.AreEqual(new[] { "Account manager", "Direzione" }, result.OpenRoles.Select(r => r.Role).ToList());
            CollectionAssert.AreEqual(new[] { "account-manager" }, result.OpenRoles[0].Agents.ToList());
            Assert.IsNull(result.OpenRoles[0].GitEmail);
        }

        [TestMethod]
        public void Fail_loud_when_the_table_is_missing()
        {
            var doc = "---\nmde_type: ownership\n---\n# no table here\n";
            var result = OwnershipDocParser.Parse(doc);
            Assert.IsTrue(result.IsOwnershipDoc);
            Assert.IsTrue(result.HasErrors);
        }

        [TestMethod]
        public void Validate_email_against_participants_and_agents_against_registry()
        {
            var parsed = OwnershipDocParser.Parse(ValidDoc);

            // carlo è participant, marco no; 'analyst'/'dev' esistono, 'dev' per Marco ok.
            var errors = OwnershipValidator.Validate(
                parsed,
                mergedParticipantEmails: new[] { "carlo@x.it" },
                knownAgentNames: new[] { "analyst", "dev" });

            Assert.IsTrue(errors.Any(e => e.Contains("marco@y.it")), "email non-participant segnalata");
            Assert.IsFalse(errors.Any(e => e.Contains("analyst") || e.Contains("'dev'")), "agenti esistenti non segnalati");
        }

        [TestMethod]
        public void Flag_an_agent_absent_from_the_registry()
        {
            var parsed = OwnershipDocParser.Parse(ValidDoc);
            var errors = OwnershipValidator.Validate(
                parsed,
                mergedParticipantEmails: new[] { "carlo@x.it", "marco@y.it" },
                knownAgentNames: new[] { "analyst" });   // 'dev' assente

            Assert.IsTrue(errors.Any(e => e.Contains("'dev'")), "agente inesistente segnalato");
        }
    }
}
