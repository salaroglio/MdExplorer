using System.Collections.Generic;
using MdExplorer.Features.Services.AI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    /// <summary>
    /// Blinda l'invariante che rende affidabile la "città degli agenti": l'ambiente del
    /// processo Copilot (canale del RunToken, R2) proviene <b>solo</b> dal
    /// <see cref="CopilotInvocation"/> per-chiamata, mai da stato condiviso del provider.
    /// Sostituisce alla radice la vecchia race sul singleton (set property → run → clear),
    /// dove un run poteva partire col token di un altro. Nessuno spawn di processo: si
    /// verifica l'helper puro che applica gli override.
    /// </summary>
    [TestClass]
    public class CopilotInvocationIsolation_Should
    {
        [TestMethod]
        public void Apply_only_the_invocation_environment_to_a_spawn()
        {
            var target = new Dictionary<string, string>();
            var invocation = new CopilotInvocation("/prj-a", new Dictionary<string, string>
            {
                ["MDE_RUN_TOKEN"] = "token-A",
                ["MDE_AGENT_NAME"] = "stem-curator",
            });

            CopilotCliProvider.ApplyEnvironmentOverrides(target, invocation);

            Assert.AreEqual("token-A", target["MDE_RUN_TOKEN"]);
            Assert.AreEqual("stem-curator", target["MDE_AGENT_NAME"]);
            Assert.AreEqual(2, target.Count);
        }

        [TestMethod]
        public void Keep_two_concurrent_invocations_isolated()
        {
            // Il cuore della robustezza: due run "in parallelo" con due target distinti non
            // condividono nulla; il token dell'uno non compare mai nell'ambiente dell'altro.
            var targetA = new Dictionary<string, string>();
            var targetB = new Dictionary<string, string>();
            var invA = new CopilotInvocation("/prj-a", new Dictionary<string, string> { ["MDE_RUN_TOKEN"] = "token-A" });
            var invB = new CopilotInvocation("/prj-b", new Dictionary<string, string> { ["MDE_RUN_TOKEN"] = "token-B" });

            CopilotCliProvider.ApplyEnvironmentOverrides(targetA, invA);
            CopilotCliProvider.ApplyEnvironmentOverrides(targetB, invB);

            Assert.AreEqual("token-A", targetA["MDE_RUN_TOKEN"]);
            Assert.AreEqual("token-B", targetB["MDE_RUN_TOKEN"]);
            Assert.IsFalse(targetA.ContainsValue("token-B"), "il token di B non deve trapelare in A");
            Assert.IsFalse(targetB.ContainsValue("token-A"), "il token di A non deve trapelare in B");
        }

        [TestMethod]
        public void Apply_nothing_when_the_invocation_has_no_environment()
        {
            var target = new Dictionary<string, string> { ["PRE_ESISTENTE"] = "x" };

            CopilotCliProvider.ApplyEnvironmentOverrides(target, CopilotInvocation.None);
            CopilotCliProvider.ApplyEnvironmentOverrides(target, new CopilotInvocation("/prj", null));
            CopilotCliProvider.ApplyEnvironmentOverrides(target, null);

            Assert.AreEqual(1, target.Count);
            Assert.AreEqual("x", target["PRE_ESISTENTE"]);
        }

        [TestMethod]
        public void Skip_blank_keys_and_normalize_null_values()
        {
            var target = new Dictionary<string, string>();
            var invocation = new CopilotInvocation("/prj", new Dictionary<string, string>
            {
                [""] = "ignorata",
                ["MDE_FROM_AGENT"] = null,
            });

            CopilotCliProvider.ApplyEnvironmentOverrides(target, invocation);

            Assert.IsFalse(target.ContainsKey(""), "la chiave vuota è scartata");
            Assert.AreEqual(string.Empty, target["MDE_FROM_AGENT"], "il valore null diventa stringa vuota");
            Assert.AreEqual(1, target.Count);
        }
    }
}
