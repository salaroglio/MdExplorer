using System;
using System.IO;
using MdExplorer.Features.Services.AI.CopilotSdk;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.AI
{
    /// <summary>
    /// A Copilot CLI nobody logged into answers «Not authenticated»: MdExplorer showed no models and no reason
    /// (a whole day on a customer's machine, 23/11/2026). Now it is recognised and explained.
    /// </summary>
    [TestClass]
    public class CopilotNotAuthenticated_Should
    {
        // The chain the SDK really throws, as read in the service log.
        private static Exception AsTheSdkThrowsIt() => new InvalidOperationException(
            "Could not read the models from Copilot CLI: Communication error with Copilot CLI: Request models.list failed with message: Not authenticated. Please authenticate first.",
            new IOException("Communication error with Copilot CLI: Request models.list failed with message: Not authenticated. Please authenticate first.",
                new Exception("Request models.list failed with message: Not authenticated. Please authenticate first.")));

        [TestMethod]
        public void Recognise_the_cli_that_has_no_login()
        {
            Assert.IsTrue(CopilotCliCompatibility.IsNotAuthenticated(AsTheSdkThrowsIt()));
            Assert.IsTrue(CopilotCliCompatibility.IsNotAuthenticated(new Exception("outer", new Exception("not authenticated"))));
        }

        [TestMethod]
        public void Not_mistake_other_failures_for_a_missing_login()
        {
            Assert.IsFalse(CopilotCliCompatibility.IsNotAuthenticated(new InvalidOperationException("Copilot CLI did not answer the model request within 30 seconds.")));
            Assert.IsFalse(CopilotCliCompatibility.IsNotAuthenticated(new Exception("SDK protocol version mismatch: SDK supports versions 3-3, but server reports version 4.")));
            Assert.IsFalse(CopilotCliCompatibility.IsProtocolMismatch(AsTheSdkThrowsIt()));
        }

        [TestMethod]
        public void Tell_which_copy_to_log_into()
        {
            // A machine can have several: the login must be made with the one MdExplorer starts.
            var message = CopilotCliCompatibility.DescribeNotAuthenticated(@"C:\Program Files\Acme Editor\bin\copilot-shim\copilot.exe");
            StringAssert.Contains(message, @"C:\Program Files\Acme Editor\bin\copilot-shim\copilot.exe");
            StringAssert.Contains(message, "/login");

            var unknown = CopilotCliCompatibility.DescribeNotAuthenticated(null);
            StringAssert.Contains(unknown, "`copilot`");
            StringAssert.Contains(unknown, "/login");
        }
    }
}
