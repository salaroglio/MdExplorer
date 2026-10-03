using System.Collections.Generic;
using System.Linq;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Yaml;
using MdExplorer.Features.Yaml.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    /// <summary>
    /// La tabella delle corrispondenze di «Approva»: a chi può passare il lavoro la persona, e se il collega è
    /// raggiungibile. Il comportamento di un agente che non dichiara niente non deve cambiare.
    /// </summary>
    [TestClass]
    public class ApprovalRoute_Should
    {
        private static AgentRegistryEntry Agent(string name, bool trusted = true, string[] accepts = null, string[] notify = null)
            => new AgentRegistryEntry
            {
                Name = name,
                Role = "ruolo di " + name,
                Trusted = trusted,
                AcceptsMessagesFrom = (accepts ?? new[] { "user" }).ToList(),
                OnApprovalNotify = (notify ?? new string[0]).ToList(),
            };

        [TestMethod]
        public void Have_no_recipients_when_the_card_does_not_declare_any()
        {
            var catalog = new[] { Agent("tecnico"), Agent("account-manager") };
            Assert.AreEqual(0, ApprovalRoute.Candidates(catalog, "tecnico").Count);
        }

        [TestMethod]
        public void Have_no_recipients_for_an_unknown_producer()
        {
            Assert.AreEqual(0, ApprovalRoute.Candidates(new[] { Agent("tecnico") }, "sconosciuto").Count);
            Assert.AreEqual(0, ApprovalRoute.Candidates(null, "tecnico").Count);
        }

        [TestMethod]
        public void List_one_available_recipient()
        {
            var catalog = new[] { Agent("tecnico", notify: new[] { "account-manager" }), Agent("account-manager") };
            var c = ApprovalRoute.Candidates(catalog, "tecnico");
            Assert.AreEqual(1, c.Count);
            Assert.AreEqual("account-manager", c[0].Name);
            Assert.AreEqual("ruolo di account-manager", c[0].Role);
            Assert.IsTrue(c[0].Available);
            Assert.IsNull(c[0].Reason);
        }

        [TestMethod]
        public void List_several_recipients_in_the_declared_order()
        {
            var catalog = new[]
            {
                Agent("tecnico", notify: new[] { "account-manager", "legale" }),
                Agent("account-manager"), Agent("legale"),
            };
            CollectionAssert.AreEqual(new[] { "account-manager", "legale" },
                ApprovalRoute.Candidates(catalog, "tecnico").Select(c => c.Name).ToList());
        }

        [TestMethod]
        public void Say_why_a_recipient_is_not_reachable()
        {
            var catalog = new[]
            {
                Agent("tecnico", notify: new[] { "fantasma", "non-fidato" }),
                Agent("non-fidato", trusted: false),
            };
            var c = ApprovalRoute.Candidates(catalog, "tecnico").ToDictionary(x => x.Name);

            Assert.IsFalse(c["fantasma"].Available);
            StringAssert.Contains(c["fantasma"].Reason, "non esiste");
            Assert.IsFalse(c["non-fidato"].Available);
            StringAssert.Contains(c["non-fidato"].Reason, "non è fidato");
        }

        [TestMethod]
        public void Ignore_case_and_let_the_person_in_even_when_the_whitelist_names_only_agents()
        {
            // La persona è sempre ammessa dalla whitelist del destinatario: l'avviso non dipende da quella lista.
            var catalog = new[] { Agent("Tecnico", notify: new[] { "ACCOUNT-MANAGER" }), Agent("account-manager", accepts: new[] { "legale" }) };
            var c = ApprovalRoute.Candidates(catalog, "tecnico");
            Assert.AreEqual(1, c.Count);
            Assert.IsTrue(c[0].Available);
        }

        [TestMethod]
        public void Send_the_notice_as_the_person_not_as_an_agent()
        {
            Assert.AreEqual("user", ApprovalRoute.Sender);
        }

        [TestMethod]
        public void Compose_a_notice_that_names_the_producer_and_the_files()
        {
            var text = ApprovalRoute.ComposeMessage("tecnico", new[] { "gara/schede/tecnico.md", " " });
            StringAssert.StartsWith(text, "[APPROVATO]");
            StringAssert.Contains(text, "'tecnico'");
            StringAssert.Contains(text, "- gara/schede/tecnico.md");
            Assert.IsFalse(text.Contains("- \n"), "i percorsi vuoti non compaiono");
        }

        [TestMethod]
        public void Read_the_recipients_from_the_card()
        {
            var parsed = new YamlAgentCardParser().GetDescriptor(@"---
description: ""x""
tools: [read, edit]
a2a:
  name: tecnico
  role: Tecnico
  accepts_messages_from: [user]
  on_approval_notify: [account-manager, legale]
---
corpo
");
            Assert.IsTrue(parsed.IsValid);
            CollectionAssert.AreEqual(new[] { "account-manager", "legale" }, parsed.Card.OnApprovalNotify.ToList());
        }

        [TestMethod]
        public void Leave_the_field_null_when_the_card_does_not_declare_it()
        {
            var parsed = new YamlAgentCardParser().GetDescriptor(@"---
description: ""x""
tools: [read]
a2a:
  name: tecnico
  role: Tecnico
---
corpo
");
            Assert.IsTrue(parsed.IsValid);
            Assert.IsNull(parsed.Card.OnApprovalNotify);
        }

        [TestMethod]
        public void Make_trust_decay_when_the_destination_changes()
        {
            // Cambiare a chi va il lavoro è come allargare la whitelist: deve far decadere la fiducia (R3).
            var tools = new[] { "read" };
            AgentCardDescriptor Card(params string[] notify) => new AgentCardDescriptor
            {
                Name = "tecnico", Role = "Tecnico",
                OnApprovalNotify = notify.Length == 0 ? null : notify.ToList(),
            };
            var none = AgentTrustHasher.ComputeHash(Card(), tools);
            var one = AgentTrustHasher.ComputeHash(Card("account-manager"), tools);
            var other = AgentTrustHasher.ComputeHash(Card("legale"), tools);
            Assert.AreNotEqual(none, one);
            Assert.AreNotEqual(one, other);
        }
    }
}
