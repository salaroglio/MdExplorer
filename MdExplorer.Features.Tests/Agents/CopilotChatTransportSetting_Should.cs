using System;
using MdExplorer.Features.Services.AI.CopilotChat;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    /// <summary>
    /// The switch between the two ways the chat talks to Copilot. The SDK is the normal road and
    /// must need no configuration; ACP is the way back, and asking for it must be unmistakable.
    /// </summary>
    [TestClass]
    public class CopilotChatTransportSetting_Should
    {
        [TestMethod]
        public void UseTheSdkWhenNothingIsConfigured()
        {
            Assert.AreEqual(CopilotChatTransport.Sdk, CopilotChatTransportSetting.Parse(null));
            Assert.AreEqual(CopilotChatTransport.Sdk, CopilotChatTransportSetting.Parse(""));
            Assert.AreEqual(CopilotChatTransport.Sdk, CopilotChatTransportSetting.Parse("   "));
        }

        [TestMethod]
        public void ReadBothValuesWhateverTheCaseAndSpacing()
        {
            Assert.AreEqual(CopilotChatTransport.Sdk, CopilotChatTransportSetting.Parse("sdk"));
            Assert.AreEqual(CopilotChatTransport.Acp, CopilotChatTransportSetting.Parse("acp"));
            Assert.AreEqual(CopilotChatTransport.Acp, CopilotChatTransportSetting.Parse(" ACP "));
        }

        [TestMethod]
        public void RefuseAValueItCannotReadInsteadOfFallingBackToTheSdk()
        {
            // Someone writes 'acpp' precisely because the SDK is broken: silently handing them the
            // SDK again would give back the very thing they were getting away from.
            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => CopilotChatTransportSetting.Parse("acpp"));

            Assert.IsTrue(ex.Message.Contains(CopilotChatTransportSetting.SettingName),
                "the message must name the setting to fix");
            Assert.IsTrue(ex.Message.Contains("'sdk'") && ex.Message.Contains("'acp'"),
                "the message must say which values are accepted");
        }
    }
}
