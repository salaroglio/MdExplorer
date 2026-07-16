using System.Collections.Generic;
using MdExplorer.Features.Agents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    [TestClass]
    public class MessageAuthorization_Should
    {
        [TestMethod]
        public void Allow_anyone_when_the_wildcard_is_present()
        {
            Assert.IsTrue(MessageAuthorization.IsSenderAccepted(new[] { "*" }, "cobol-pipeline"));
            Assert.IsTrue(MessageAuthorization.IsSenderAccepted(new[] { "deploy-sentinel", "*" }, "chiunque"));
        }

        [TestMethod]
        public void Allow_a_sender_that_is_whitelisted_case_insensitive()
        {
            Assert.IsTrue(MessageAuthorization.IsSenderAccepted(new[] { "cobol-pipeline" }, "cobol-pipeline"));
            Assert.IsTrue(MessageAuthorization.IsSenderAccepted(new[] { "Cobol-Pipeline" }, "cobol-pipeline"));
        }

        [TestMethod]
        public void Deny_a_sender_that_is_not_whitelisted()
        {
            Assert.IsFalse(MessageAuthorization.IsSenderAccepted(new[] { "deploy-sentinel" }, "cobol-pipeline"));
        }

        [TestMethod]
        public void Deny_when_the_whitelist_is_empty_or_null_default_deny()
        {
            Assert.IsFalse(MessageAuthorization.IsSenderAccepted(new List<string>(), "cobol-pipeline"));
            Assert.IsFalse(MessageAuthorization.IsSenderAccepted(null, "cobol-pipeline"));
        }

        [TestMethod]
        public void Always_allow_the_human_user_regardless_of_the_whitelist()
        {
            Assert.IsTrue(MessageAuthorization.IsSenderAccepted(new List<string>(), "user"));
            Assert.IsTrue(MessageAuthorization.IsSenderAccepted(new[] { "deploy-sentinel" }, "USER"));
        }

        [TestMethod]
        public void Deny_an_empty_sender()
        {
            Assert.IsFalse(MessageAuthorization.IsSenderAccepted(new[] { "*" }, null));
            Assert.IsFalse(MessageAuthorization.IsSenderAccepted(new[] { "*" }, "   "));
        }
    }
}
