using System.Collections.Generic;
using MdExplorer.Services.TeamChat;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Il placeholder versionato dell'API key del relay dev'essere trattato come "non
    /// configurato": è ciò che tiene chat/federazione DORMIENTI su un checkout pulito invece di
    /// martellare il relay con una chiave finta.
    /// </summary>
    [TestClass]
    public class MdChatConfig_Should
    {
        private static IConfiguration Cfg(string apiKey)
            => new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> { ["MdChat:ApiKey"] = apiKey })
                .Build();

        [TestMethod]
        public void Treat_the_placeholder_as_not_configured()
        {
            var cfg = Cfg(MdChatConfig.PlaceholderApiKey);
            Assert.IsNull(MdChatConfig.ResolveApiKey(cfg));
            Assert.IsTrue(MdChatConfig.IsPlaceholderApiKey(cfg));
        }

        [TestMethod]
        public void Treat_empty_and_missing_as_not_configured()
        {
            Assert.IsNull(MdChatConfig.ResolveApiKey(Cfg("")));
            Assert.IsNull(MdChatConfig.ResolveApiKey(new ConfigurationBuilder().Build()));
        }

        [TestMethod]
        public void Return_a_real_key_unchanged()
        {
            var cfg = Cfg("una-chiave-vera-non-placeholder");
            Assert.AreEqual("una-chiave-vera-non-placeholder", MdChatConfig.ResolveApiKey(cfg));
            Assert.IsFalse(MdChatConfig.IsPlaceholderApiKey(cfg));
        }
    }
}
