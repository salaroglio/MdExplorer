using System.Linq;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Yaml;
using MdExplorer.Features.Yaml.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    /// <summary>
    /// «Cosa può fare sul tuo computer»: lo calcola l'app dagli strumenti dichiarati, gli unici che fa rispettare.
    /// Il riassunto scritto dall'autore è un'altra cosa (dichiarazione) e ha le sue regole.
    /// </summary>
    [TestClass]
    public class AgentEffects_Should
    {
        private static AgentEffect Of(string[] tools, string id) => AgentEffects.For(tools).Single(e => e.Id == id);

        [TestMethod]
        public void Always_allow_reading_and_messaging_and_nothing_else_by_default()
        {
            var none = new string[0];
            Assert.IsTrue(Of(none, AgentEffects.ReadFiles).Granted);
            Assert.IsTrue(Of(none, AgentEffects.MessageOthers).Granted);
            Assert.IsFalse(Of(none, AgentEffects.WriteFiles).Granted);
            Assert.IsFalse(Of(none, AgentEffects.RunCommands).Granted);
            Assert.IsFalse(Of(none, AgentEffects.SearchDocuments).Granted);
            Assert.IsFalse(AgentEffects.For(none).Any(e => e.Danger), "senza strumenti non c'è niente di pericoloso");
        }

        [TestMethod]
        public void Treat_a_missing_manifest_like_an_empty_one()
        {
            Assert.IsFalse(AgentEffects.For(null).Any(e => e.Granted && e.Danger));
        }

        [DataTestMethod]
        [DataRow("write")]
        [DataRow("edit")]
        [DataRow("EDIT")]
        public void Flag_writing_as_dangerous_for_write_and_edit(string tool)
        {
            var e = Of(new[] { "read", tool }, AgentEffects.WriteFiles);
            Assert.IsTrue(e.Granted);
            Assert.IsTrue(e.Danger);
        }

        [DataTestMethod]
        [DataRow("shell")]
        [DataRow("execute")]
        public void Flag_running_commands_as_dangerous(string tool)
        {
            var e = Of(new[] { tool }, AgentEffects.RunCommands);
            Assert.IsTrue(e.Granted);
            Assert.IsTrue(e.Danger);
            Assert.IsFalse(Of(new[] { tool }, AgentEffects.WriteFiles).Granted, "eseguire comandi non implica scrivere file");
        }

        [TestMethod]
        public void Say_search_is_granted_only_when_declared_and_is_not_dangerous()
        {
            var e = Of(new[] { "read", "search" }, AgentEffects.SearchDocuments);
            Assert.IsTrue(e.Granted);
            Assert.IsFalse(e.Danger);
        }

        [TestMethod]
        public void Agree_with_what_the_engine_is_told_to_deny()
        {
            // Coerenza con l'enforcement: ciò che la finestra dice «non può» è ciò che NativeToolsToDeny toglie al motore.
            foreach (var tools in new[] { new string[0], new[] { "read" }, new[] { "edit" }, new[] { "shell" }, new[] { "read", "edit", "shell" } })
            {
                var denied = AgentToolCatalog.NativeToolsToDeny(tools);
                Assert.AreEqual(!denied.Contains("shell"), Of(tools, AgentEffects.RunCommands).Granted, string.Join(",", tools));
                Assert.AreEqual(!denied.Contains("write"), Of(tools, AgentEffects.WriteFiles).Granted, string.Join(",", tools));
            }
        }

        private static AgentCardParseResult Parse(string a2aExtra) => new YamlAgentCardParser().GetDescriptor(
            "---\ndescription: x\ntools: [read]\na2a:\n  name: contabile\n  role: Contabile\n" + a2aExtra + "---\ncorpo\n");

        [TestMethod]
        public void Read_the_summary_from_the_card_and_trim_it()
        {
            var p = Parse("  summary: \"  Controlla le fatture contro il contratto.  \"\n");
            Assert.IsTrue(p.IsValid);
            Assert.AreEqual("Controlla le fatture contro il contratto.", p.Card.Summary);
        }

        [TestMethod]
        public void Leave_the_summary_null_when_absent_or_blank()
        {
            Assert.IsNull(Parse("").Card.Summary);
            Assert.IsNull(Parse("  summary: \"   \"\n").Card.Summary);
        }

        [TestMethod]
        public void Refuse_a_summary_too_long_to_be_read_before_trusting()
        {
            var p = Parse("  summary: \"" + new string('a', AgentCardDescriptor.SummaryMaxLength + 1) + "\"\n");
            Assert.IsFalse(p.IsValid);
            StringAssert.Contains(p.RegistrationError, "'summary'");
            Assert.IsTrue(Parse("  summary: \"" + new string('a', AgentCardDescriptor.SummaryMaxLength) + "\"\n").IsValid);
        }

        [TestMethod]
        public void Make_trust_decay_when_the_summary_changes_but_not_when_it_is_absent()
        {
            var tools = new[] { "read" };
            var none = AgentTrustHasher.ComputeHash(new AgentCardDescriptor { Name = "a", Role = "r" }, tools);
            var one = AgentTrustHasher.ComputeHash(new AgentCardDescriptor { Name = "a", Role = "r", Summary = "Legge." }, tools);
            var two = AgentTrustHasher.ComputeHash(new AgentCardDescriptor { Name = "a", Role = "r", Summary = "Legge e scrive." }, tools);
            Assert.AreNotEqual(none, one);
            Assert.AreNotEqual(one, two, "la persona non può aver letto una descrizione e trovarsene un'altra");
        }
    }
}
