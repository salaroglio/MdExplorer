using System;
using System.IO;
using MdExplorer.Features.Agents.Workflow.Scheduler;
using MdExplorer.Features.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    /// <summary>Gli errori della città seguono la lingua dell'interfaccia, anche quelli sollevati nel bll (17/10/2026).</summary>
    [TestClass]
    public class UiText_Should
    {
        [TestCleanup]
        public void Reset() => UiText.Use(null);

        private static string NeverOpenedRoundError()
        {
            var root = Path.Combine(Path.GetTempPath(), "uitext-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                return Assert.ThrowsException<InvalidOperationException>(() =>
                    RoundLedger.Append(root, "2026-10-17-gara-000000", "tecnica", "t",
                        new RoundEvent { Type = RoundEventType.Held, At = DateTime.UtcNow })).Message;
            }
            finally { Directory.Delete(root, true); }
        }

        [TestMethod]
        public void Speak_english_when_no_language_was_given()
        {
            UiText.Use(null);
            Assert.AreEqual("en", UiText.Current);
            StringAssert.Contains(NeverOpenedRoundError(), "does not exist: it must be opened before");
        }

        [TestMethod]
        public void Speak_italian_when_the_interface_is_italian()
        {
            UiText.Use(new FixedUiLanguage("it"));
            var message = NeverOpenedRoundError();
            StringAssert.Contains(message, "non esiste: va aperto prima");
            Assert.IsFalse(message.Contains("does not exist"), message);
        }

        [TestMethod]
        public void Follow_a_language_change_without_restarting()
        {
            var language = new SwitchableLanguage { Current = "it" };
            UiText.Use(language);
            Assert.AreEqual("sì", UiText.Say("sì", "yes"));
            language.Current = "en";
            Assert.AreEqual("yes", UiText.Say("sì", "yes"));
        }

        private sealed class SwitchableLanguage : IUiLanguage
        {
            public string Current { get; set; }
        }
    }
}
