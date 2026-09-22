using System.Linq;
using System.Text.Json;
using MdExplorer.Features.Services.AI.CopilotChat;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    /// <summary>
    /// The numbers next to MarkAgent's model choice. The quota JSON is the one measured with the
    /// real CLI on 13/09/2026 (free plan), plus the shape of a paid plan.
    /// </summary>
    [TestClass]
    public class CopilotUsage_Should
    {
        private const string FreePlan = @"{
            ""chat"": { ""entitlementRequests"": 200, ""isUnlimitedEntitlement"": false, ""overage"": 0, ""remainingPercentage"": 89.7, ""resetDate"": ""2026-10-13T14:14:05.09+00:00"", ""usedRequests"": 21 },
            ""completions"": { ""entitlementRequests"": 2000, ""isUnlimitedEntitlement"": false, ""remainingPercentage"": 100, ""usedRequests"": 0 },
            ""premium_interactions"": { ""entitlementRequests"": 0, ""isUnlimitedEntitlement"": false, ""remainingPercentage"": 0, ""usedRequests"": 0 } }";

        private const string PaidPlan = @"{
            ""chat"": { ""entitlementRequests"": -1, ""isUnlimitedEntitlement"": true, ""remainingPercentage"": 100, ""usedRequests"": 0 },
            ""premium_interactions"": { ""entitlementRequests"": 300, ""isUnlimitedEntitlement"": false, ""remainingPercentage"": 75, ""usedRequests"": 75 } }";

        private static System.Collections.Generic.IReadOnlyList<CopilotQuotaBucket> Parse(string json)
            => CopilotUsage.BucketsFromJson(JsonDocument.Parse(json).RootElement);

        [TestMethod]
        public void ReadTheQuotaAsGitHubSendsIt()
        {
            var chat = Parse(FreePlan).Single(b => b.Type == "chat");

            Assert.AreEqual(200, chat.Entitlement);
            Assert.AreEqual(21, chat.Used);
            Assert.AreEqual(89.7, chat.RemainingPercent, 0.0001);
            Assert.IsFalse(chat.Unlimited);
            Assert.AreEqual(2026, chat.ResetDate?.Year);
        }

        [TestMethod]
        public void ChooseTheQuotaAChatConsumes()
        {
            Assert.AreEqual("chat", CopilotUsage.ChooseBucket(Parse(FreePlan)).Type, "free plan: premium interactions are 0, completions is the editor");
            Assert.AreEqual("premium_interactions", CopilotUsage.ChooseBucket(Parse(PaidPlan)).Type, "paid plan: chat is unlimited, premium requests are what runs out");
            Assert.AreEqual("chat", CopilotUsage.ChooseBucket(Parse(@"{ ""chat"": { ""isUnlimitedEntitlement"": true } }")).Type);
            Assert.IsNull(CopilotUsage.ChooseBucket(Parse(@"{ ""completions"": { ""entitlementRequests"": 2000 } }")));
        }

        [TestMethod]
        public void ComputeTheTwoPercentages()
        {
            var usage = CopilotUsage.Build(Parse(FreePlan), null, contextTokens: 26277, contextLimit: 128000);

            Assert.AreEqual(10.3, usage.QuotaUsedPercent, "100 - GitHub's 89.7, not 21/200");
            Assert.AreEqual(21L, usage.QuotaUsed);
            Assert.AreEqual(200L, usage.QuotaEntitlement);
            Assert.AreEqual(21, usage.ContextPercent, "26,277 of 128,000 = 20.5%");
        }

        [TestMethod]
        public void ShowNoPercentageOfAnUnlimitedQuota()
        {
            var usage = CopilotUsage.Build(Parse(@"{ ""chat"": { ""isUnlimitedEntitlement"": true, ""entitlementRequests"": -1 } }"), null, null, null);

            Assert.IsTrue(usage.QuotaUnlimited);
            Assert.IsNull(usage.QuotaUsedPercent);
            Assert.IsNull(usage.ContextPercent, "no context known yet");
        }

        [TestMethod]
        public void KeepTheContextWhenGitHubReportedNoQuota()
        {
            var usage = CopilotUsage.Build(Enumerable.Empty<CopilotQuotaBucket>(), null, 64000, 128000);

            Assert.IsNull(usage.QuotaType);
            Assert.AreEqual(50, usage.ContextPercent);
        }
    }
}
