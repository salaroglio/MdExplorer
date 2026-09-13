using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace MdExplorer.Features.Services.AI.CopilotChat
{
    /// <summary>One Copilot quota of the account (chat, premium_interactions, …), as GitHub reports it.</summary>
    public sealed class CopilotQuotaBucket
    {
        public string Type { get; set; }
        public long Entitlement { get; set; }
        public long Used { get; set; }
        public double RemainingPercent { get; set; }
        public bool Unlimited { get; set; }
        public DateTimeOffset? ResetDate { get; set; }
    }

    /// <summary>
    /// What the bar next to MarkAgent's model choice shows: how much of the account's quota is
    /// used, how much of it this conversation took, how full the conversation's context is.
    /// </summary>
    public sealed class CopilotUsageSnapshot
    {
        /// <summary>The quota shown (<c>premium_interactions</c> or <c>chat</c>); null when GitHub reported none.</summary>
        public string QuotaType { get; set; }
        public bool QuotaUnlimited { get; set; }
        /// <summary>100 − GitHub's remaining percentage, one decimal.</summary>
        public double? QuotaUsedPercent { get; set; }
        public long? QuotaUsed { get; set; }
        public long? QuotaEntitlement { get; set; }
        public DateTimeOffset? QuotaResetDate { get; set; }
        public DateTimeOffset? QuotaAsOf { get; set; }

        /// <summary>Requests this conversation counted against the quota.</summary>
        public double SessionRequests { get; set; }
        /// <summary><see cref="SessionRequests"/> over the quota's entitlement, one decimal; null when unlimited or unknown.</summary>
        public double? SessionPercent { get; set; }

        public long? ContextTokens { get; set; }
        public long? ContextLimit { get; set; }
        public int? ContextPercent { get; set; }
    }

    /// <summary>
    /// Turns what the Copilot SDK reports into <see cref="CopilotUsageSnapshot"/>. Pure, so the
    /// arithmetic and the choice of the quota are tested without a CLI.
    ///
    /// <para>
    /// Copilot counts the quota in REQUESTS, not tokens (measured 13/09/2026 on a free plan:
    /// "chat" 200 requests a month, one per user prompt; a model call the agent makes on its own,
    /// after a tool, does not count). Tokens only matter for the context window.
    /// </para>
    /// </summary>
    public static class CopilotUsage
    {
        public const string PremiumInteractions = "premium_interactions";
        public const string Chat = "chat";

        /// <summary>
        /// The quota a chat conversation consumes: premium interactions when the plan has a limit on
        /// them (paid plans, where "chat" is unlimited), otherwise "chat" (free plan, where premium
        /// interactions are 0). "completions" is code completion in the editor, never this chat.
        /// </summary>
        public static CopilotQuotaBucket ChooseBucket(IEnumerable<CopilotQuotaBucket> buckets)
        {
            var list = (buckets ?? Enumerable.Empty<CopilotQuotaBucket>()).ToList();
            CopilotQuotaBucket Find(string type) => list.FirstOrDefault(b => string.Equals(b.Type, type, StringComparison.OrdinalIgnoreCase));

            var premium = Find(PremiumInteractions);
            var chat = Find(Chat);
            if (premium != null && !premium.Unlimited && premium.Entitlement > 0) return premium;
            if (chat != null && !chat.Unlimited && chat.Entitlement > 0) return chat;
            if (premium != null && premium.Unlimited) return premium;
            if (chat != null && chat.Unlimited) return chat;
            return null;
        }

        public static CopilotUsageSnapshot Build(
            IEnumerable<CopilotQuotaBucket> buckets,
            DateTimeOffset? quotaAsOf,
            double sessionRequests,
            long? contextTokens,
            long? contextLimit)
        {
            var snapshot = new CopilotUsageSnapshot { SessionRequests = sessionRequests };

            var bucket = ChooseBucket(buckets);
            if (bucket != null)
            {
                snapshot.QuotaType = bucket.Type;
                snapshot.QuotaUnlimited = bucket.Unlimited;
                snapshot.QuotaResetDate = bucket.ResetDate;
                snapshot.QuotaAsOf = quotaAsOf;
                if (!bucket.Unlimited)
                {
                    snapshot.QuotaUsed = bucket.Used;
                    snapshot.QuotaEntitlement = bucket.Entitlement;
                    // GitHub's own figure, not Used/Entitlement: the two differ slightly (measured
                    // 21/200 with 89.7% remaining), and the number must match what GitHub shows.
                    snapshot.QuotaUsedPercent = Round1(Math.Clamp(100 - bucket.RemainingPercent, 0, 100));
                    snapshot.SessionPercent = Round1(sessionRequests / bucket.Entitlement * 100);
                }
            }

            if (contextTokens != null && contextLimit is > 0)
            {
                snapshot.ContextTokens = contextTokens;
                snapshot.ContextLimit = contextLimit;
                snapshot.ContextPercent = (int)Math.Round((double)contextTokens.Value / contextLimit.Value * 100, MidpointRounding.AwayFromZero);
            }
            return snapshot;
        }

        /// <summary>
        /// The <c>quotaSnapshots</c> object of an <c>assistant.usage</c> event or of
        /// <c>account.getQuota</c>: <c>{ "chat": { entitlementRequests, usedRequests,
        /// remainingPercentage, isUnlimitedEntitlement, resetDate }, … }</c>.
        /// </summary>
        public static IReadOnlyList<CopilotQuotaBucket> BucketsFromJson(JsonElement quotaSnapshots)
        {
            var buckets = new List<CopilotQuotaBucket>();
            if (quotaSnapshots.ValueKind != JsonValueKind.Object) return buckets;

            foreach (var property in quotaSnapshots.EnumerateObject())
            {
                var q = property.Value;
                if (q.ValueKind != JsonValueKind.Object) continue;
                buckets.Add(new CopilotQuotaBucket
                {
                    Type = property.Name,
                    Entitlement = Long(q, "entitlementRequests"),
                    Used = Long(q, "usedRequests"),
                    RemainingPercent = Double(q, "remainingPercentage"),
                    Unlimited = q.TryGetProperty("isUnlimitedEntitlement", out var u) && u.ValueKind == JsonValueKind.True,
                    ResetDate = q.TryGetProperty("resetDate", out var r) && r.ValueKind == JsonValueKind.String
                                && DateTimeOffset.TryParse(r.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var reset)
                        ? reset
                        : null,
                });
            }
            return buckets;
        }

        private static long Long(JsonElement o, string name)
            => o.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number ? (long)v.GetDouble() : 0;

        private static double Double(JsonElement o, string name)
            => o.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetDouble() : 0;

        private static double Round1(double value) => Math.Round(value, 1, MidpointRounding.AwayFromZero);
    }
}
