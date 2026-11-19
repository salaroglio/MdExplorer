using System;
using MdExplorer.Features.Services.AI.CopilotSdk;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    /// <summary>
    /// 19/11/2026, Windows: Copilot CLI 1.0.114 reports protocol 4, the SDK accepts only 3, and MarkAgent answered «ciao»
    /// with the bare SDK error. The texts below are the real ones from that machine.
    /// </summary>
    [TestClass]
    public class CopilotCliCompatibility_Should
    {
        private const string RealSdkError = "SDK protocol version mismatch: SDK supports versions 3-3, but server reports version 4. Please update your SDK or server to ensure compatibility.";

        [TestMethod]
        public void Know_the_cli_the_sdk_package_is_built_for()
        {
            var expected = CopilotCliCompatibility.ExpectedCliVersion;
            Assert.IsNotNull(expected, "CopilotCliVersion manca nei metadati dell'assembly (MdExplorer.Features.csproj)");
            Assert.AreEqual(expected, CopilotCliCompatibility.ParseVersion(expected), "non è una versione x.y.z: " + expected);
        }

        [TestMethod]
        public void Recognise_the_sdk_protocol_error_also_inside_another_exception()
        {
            Assert.IsTrue(CopilotCliCompatibility.IsProtocolMismatch(new InvalidOperationException(RealSdkError)));
            Assert.IsTrue(CopilotCliCompatibility.IsProtocolMismatch(new AggregateException(new Exception(RealSdkError))));
            Assert.IsFalse(CopilotCliCompatibility.IsProtocolMismatch(new InvalidOperationException("Copilot CLI not found in the service PATH")));
        }

        [TestMethod]
        public void Read_the_version_from_the_real_output_of_copilot_version()
        {
            Assert.AreEqual("1.0.114", CopilotCliCompatibility.ParseVersion("GitHub Copilot CLI 1.0.114.\nRun 'copilot update' to check for updates.\n"));
            Assert.IsNull(CopilotCliCompatibility.ParseVersion("command not found"));
        }

        [TestMethod]
        public void Compare_versions_by_number_not_by_text()
        {
            Assert.IsTrue(CopilotCliCompatibility.CompareVersions("1.0.114", "1.0.112") > 0);
            Assert.IsTrue(CopilotCliCompatibility.CompareVersions("1.0.79", "1.0.112") < 0, "come testo 1.0.79 > 1.0.112: deve contare il numero");
            Assert.AreEqual(0, CopilotCliCompatibility.CompareVersions("1.0.112", "1.0.112"));
            Assert.IsNull(CopilotCliCompatibility.CompareVersions("?", "1.0.112"));
        }

        [TestMethod]
        public void Say_both_versions_and_the_command_that_fixes_it()
        {
            var text = CopilotCliCompatibility.DescribeMismatch("1.0.114", "1.0.112", RealSdkError);
            StringAssert.Contains(text, "1.0.114");
            StringAssert.Contains(text, "npm install -g @github/copilot@1.0.112");
            StringAssert.Contains(text, RealSdkError, "l'errore dell'SDK resta, per chi legge il log");
        }

        [TestMethod]
        public void Say_it_when_the_installed_version_cannot_be_read_instead_of_inventing_one()
        {
            var text = CopilotCliCompatibility.DescribeMismatch(null, "1.0.112", RealSdkError);
            StringAssert.Contains(text, "npm install -g @github/copilot@1.0.112");
            Assert.IsFalse(text.Contains("1.0.114"));
        }
    }
}
