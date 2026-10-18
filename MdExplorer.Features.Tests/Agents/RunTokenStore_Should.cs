using System;
using MdExplorer.Features.Agents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    [TestClass]
    public class RunTokenStore_Should
    {
        private readonly IRunTokenStore _store = new RunTokenStore();

        private static RunTokenClaims Claims(string name = "stem-curator") => new RunTokenClaims
        {
            RunId = Guid.NewGuid(),
            AgentName = name,
            ProjectPath = "/p",
            ConversationId = Guid.NewGuid().ToString(),
        };

        [TestMethod]
        public void Resolve_the_identity_of_a_minted_token()
        {
            var claims = Claims();
            var token = _store.Mint(claims);

            var resolved = _store.Validate(token);
            Assert.IsNotNull(resolved);
            Assert.AreEqual(claims.AgentName, resolved.AgentName);
            Assert.AreEqual(claims.RunId, resolved.RunId);
            Assert.AreEqual(claims.ConversationId, resolved.ConversationId);
        }

        [TestMethod]
        public void Return_null_for_an_unknown_token()
        {
            Assert.IsNull(_store.Validate("non-esiste"));
            Assert.IsNull(_store.Validate(null));
            Assert.IsNull(_store.Validate(""));
        }

        [TestMethod]
        public void Stop_validating_after_revocation()
        {
            var token = _store.Mint(Claims());
            Assert.IsNotNull(_store.Validate(token));
            _store.Revoke(token);
            Assert.IsNull(_store.Validate(token), "un token revocato a fine run non è più valido");
        }

        [TestMethod]
        public void Mint_distinct_tokens()
        {
            var t1 = _store.Mint(Claims("a"));
            var t2 = _store.Mint(Claims("b"));
            Assert.AreNotEqual(t1, t2);
            Assert.AreEqual("a", _store.Validate(t1).AgentName);
            Assert.AreEqual("b", _store.Validate(t2).AgentName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Refuse_to_mint_without_claims()
        {
            _store.Mint(null);
        }
    }
}
