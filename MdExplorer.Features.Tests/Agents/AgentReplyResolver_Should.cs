using System.Collections.Generic;
using MdExplorer.Features.Agents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    /// <summary>
    /// Le risposte che un agente propone alla persona: dichiarate nella scheda, riempite dal messaggio. Niente si
    /// indovina: ciò che non torna è un errore che l'agente legge e corregge.
    /// </summary>
    [TestClass]
    public class AgentReplyResolver_Should
    {
        private static readonly List<AgentRegistryReply> Card = new()
        {
            new AgentRegistryReply
            {
                Id = "avvia-giro",
                Label = "Avvia il giro su {codice}",
                Description = "Incarico i tre responsabili di scrivere la scheda sul bando {codice}.",
                Message = "avvia {codice}",
            },
        };

        private static IDictionary<string, string> Proposal(params (string Key, string Value)[] values)
        {
            var d = new Dictionary<string, string>();
            foreach (var (k, v) in values) d[k] = v;
            return d;
        }

        [TestMethod]
        public void Fill_the_placeholders_of_a_declared_reply()
        {
            var resolved = AgentReplyResolver.Resolve(Card, new[] { Proposal(("id", "avvia-giro"), ("codice", "NC-2027-014")) }, out var error);

            Assert.IsNull(error);
            Assert.AreEqual(1, resolved.Count);
            Assert.AreEqual("Avvia il giro su NC-2027-014", resolved[0].Label);
            Assert.AreEqual("Incarico i tre responsabili di scrivere la scheda sul bando NC-2027-014.", resolved[0].Description);
            Assert.AreEqual("avvia NC-2027-014", resolved[0].Message, "è il testo che la scheda dell'agente sa trattare");
        }

        [TestMethod]
        public void Give_one_button_for_each_proposal()
        {
            var resolved = AgentReplyResolver.Resolve(Card, new[]
            {
                Proposal(("id", "avvia-giro"), ("codice", "NC-2027-014")),
                Proposal(("id", "AVVIA-GIRO"), ("CODICE", "NC-2027-020")),
            }, out var error);

            Assert.IsNull(error);
            Assert.AreEqual(2, resolved.Count);
            Assert.AreEqual("avvia NC-2027-020", resolved[1].Message, "id e nomi dei valori non dipendono dalle maiuscole");
        }

        [TestMethod]
        public void Refuse_a_reply_the_card_does_not_declare()
        {
            var resolved = AgentReplyResolver.Resolve(Card, new[] { Proposal(("id", "cancella-tutto")) }, out var error);

            Assert.IsNull(resolved, "un pulsante può inviare solo ciò che la scheda dichiara");
            StringAssert.Contains(error, "cancella-tutto");
            StringAssert.Contains(error, "avvia-giro", "l'errore dice quali risposte esistono, così l'agente si corregge");
        }

        [TestMethod]
        public void Refuse_a_placeholder_left_without_a_value()
        {
            var resolved = AgentReplyResolver.Resolve(Card, new[] { Proposal(("id", "avvia-giro")) }, out var error);

            Assert.IsNull(resolved, "un pulsante «Avvia il giro su {codice}» non deve mai arrivare alla persona");
            StringAssert.Contains(error, "codice");
        }

        [TestMethod]
        public void Refuse_a_declared_reply_that_says_nothing()
        {
            var card = new List<AgentRegistryReply> { new AgentRegistryReply { Id = "vai", Label = "Vai", Message = "vai" } };

            var resolved = AgentReplyResolver.Resolve(card, new[] { Proposal(("id", "vai")) }, out var error);

            Assert.IsNull(resolved);
            StringAssert.Contains(error, "description", "la descrizione è ciò che dice chi viene contattato e per fare cosa");
        }

        [TestMethod]
        public void Return_nothing_when_nothing_is_proposed()
        {
            var resolved = AgentReplyResolver.Resolve(Card, null, out var error);

            Assert.IsNull(error);
            Assert.AreEqual(0, resolved.Count);
        }

        [TestMethod]
        public void Refuse_a_message_without_replies_when_the_card_declares_them()
        {
            var reason = AgentReplyResolver.MissingProposal(Card, repliesPassed: false);

            Assert.IsNotNull(reason, "senza replies «niente da scegliere» e «dimenticato» sarebbero lo stesso messaggio");
            StringAssert.Contains(reason, "avvia-giro");
            StringAssert.Contains(reason, "[]");
        }

        [TestMethod]
        public void Accept_an_explicit_empty_list_or_a_card_without_replies()
        {
            Assert.IsNull(AgentReplyResolver.MissingProposal(Card, repliesPassed: true), "[] = la persona non ha niente da scegliere");
            Assert.IsNull(AgentReplyResolver.MissingProposal(new List<AgentRegistryReply>(), repliesPassed: false), "replies: [] nella scheda");
            Assert.IsNull(AgentReplyResolver.MissingProposal(null, repliesPassed: false), "scheda senza replies");
        }

        [TestMethod]
        public void Quote_the_agent_message_and_its_buttons_in_a_free_reply()
        {
            var offered = new[] { new ResolvedReply { Id = "avvia-giro", Label = "Avvia il giro su NC-2027-014", Message = "avvia NC-2027-014" } };

            var body = AgentReplyResolver.QuoteFreeReply("  perché non vedo i pulsanti?  ", "[ESITO] Tre procedure.\r\nNC-2027-014 compatibile", offered);

            StringAssert.StartsWith(body, "[RISPOSTA LIBERA] perché non vedo i pulsanti?");
            StringAssert.Contains(body, "> [ESITO] Tre procedure.\n> NC-2027-014 compatibile");
            StringAssert.Contains(body, "«Avvia il giro su NC-2027-014» (messaggio: avvia NC-2027-014)");
        }

        [TestMethod]
        public void Say_no_buttons_were_offered_when_there_were_none()
        {
            var body = AgentReplyResolver.QuoteFreeReply("che è successo?", "[ESITO] Niente di nuovo.", null);

            StringAssert.Contains(body, "Risposte che le avevi proposto come pulsanti: nessuna.");
        }
    }
}
