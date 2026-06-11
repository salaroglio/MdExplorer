using MdExplorer.Features.Services.KnowledgeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MdExplorer.Features.Tests.Atlassian
{
    /// <summary>
    /// The Atlassian API token is persisted via the same DPAPI protector used for
    /// Neo4j/Fuseki passwords. These tests pin the round-trip contract the token
    /// storage relies on. DPAPI is Windows-only, so they no-op elsewhere.
    /// </summary>
    [TestClass]
    public class DpapiPasswordProtectorShould
    {
        [TestMethod]
        public void RoundTripASecretBackToPlaintext()
        {
            if (!OperatingSystem.IsWindows()) { Assert.Inconclusive("DPAPI is Windows-only."); return; }

            var protector = new DpapiPasswordProtector();
            const string secret = "ATATT3xFfGF0-fake-api-token-9aZ";

            var cipher = protector.Protect(secret);

            Assert.AreNotEqual(secret, cipher, "ciphertext must not equal plaintext");
            Assert.AreEqual(secret, protector.Unprotect(cipher), "decrypt must recover the original token");
        }

        [TestMethod]
        public void ReturnNullForNullOrEmpty()
        {
            if (!OperatingSystem.IsWindows()) { Assert.Inconclusive("DPAPI is Windows-only."); return; }

            var protector = new DpapiPasswordProtector();
            Assert.IsNull(protector.Protect(null));
            Assert.IsNull(protector.Protect(string.Empty));
            Assert.IsNull(protector.Unprotect(null));
            Assert.IsNull(protector.Unprotect(string.Empty));
        }
    }
}
