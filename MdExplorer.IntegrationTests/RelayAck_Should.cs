using System.Text.Json;
using MdExplorer.Services.Federation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// La risposta del relay a <c>send</c> (16/10/2026): consegnata, accodata o non partita. Prima ogni «ok» valeva come
    /// consegna, anche una busta che aspettava un computer spento (che non scade).
    /// </summary>
    [TestClass]
    public class RelayAck_Should
    {
        private static RelayDelivery Read(string json) => RelayAck.ReadSend(JsonDocument.Parse(json).RootElement);

        [TestMethod]
        public void Tell_delivered_from_queued_from_not_sent()
        {
            Assert.AreEqual(RelayDelivery.Delivered, Read("{\"ok\":true,\"status\":\"delivered\",\"id\":\"a\"}"));
            Assert.AreEqual(RelayDelivery.Queued, Read("{\"ok\":true,\"status\":\"queued\",\"id\":\"a\"}"));
            Assert.AreEqual(RelayDelivery.NotSent, Read("{\"ok\":false,\"error\":\"coda non disponibile: riprova\"}"));
            Assert.AreEqual(RelayDelivery.NotSent, Read("\"strano\""));
        }

        [TestMethod]
        public void Never_claim_a_delivery_it_does_not_know()
        {
            Assert.AreEqual(RelayDelivery.Queued, Read("{\"ok\":true}"), "ok senza stato: la cosa più prudente è «accodata»");
            Assert.AreEqual(RelayDelivery.Queued, Read("{\"ok\":true,\"status\":\"altro\"}"));
        }
    }
}
