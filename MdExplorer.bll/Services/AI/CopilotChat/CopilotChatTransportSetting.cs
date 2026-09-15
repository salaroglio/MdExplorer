using System;
using System.Linq;
using Ad.Tools.Dal.Abstractions.Interfaces;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MdExplorer.Features.Services.AI.CopilotChat
{
    /// <summary>
    /// Which transport a NEW chat session is opened with.
    /// </summary>
    public interface ICopilotChatTransportSource
    {
        CopilotChatTransport Current();
    }

    /// <summary>
    /// Reads the <c>CopilotChatTransport</c> setting (<c>sdk</c> | <c>acp</c>) from the user DB.
    ///
    /// <para>
    /// Absent = <see cref="CopilotChatTransport.Sdk"/>: the SDK is the normal road and nobody
    /// should have to configure anything to be on it. ACP is the way back, written by hand in the
    /// DB when the SDK breaks — so the switch works without a new release.
    /// </para>
    ///
    /// <para>
    /// Read through an ISOLATED session (<c>OpenSession</c>), never through the shared
    /// <c>IUserSettingsDB</c>: a read on the shared session outside a transaction leaves an
    /// implicit one behind, and the next unrelated <c>Commit</c> fails with "Transaction not
    /// successfully started" — which is how opening a project broke in April. Same pattern as
    /// <c>PlantumlServer</c>.
    /// </para>
    /// </summary>
    public sealed class CopilotChatTransportSetting : ICopilotChatTransportSource
    {
        public const string SettingName = "CopilotChatTransport";

        private readonly IDALFactory<IUserSettingsDB> _dalFactory;

        public CopilotChatTransportSetting(IDALFactory<IUserSettingsDB> dalFactory)
        {
            _dalFactory = dalFactory;
        }

        public CopilotChatTransport Current()
        {
            string raw;
            using (var session = _dalFactory.OpenSession())
            {
                raw = session.GetDal<Setting>().GetList()
                    .Where(_ => _.Name == SettingName)
                    .Select(_ => _.ValueString)
                    .FirstOrDefault();
            }
            return Parse(raw);
        }

        /// <summary>
        /// A value that is there but unreadable is an error, not a default: silently picking the
        /// SDK for someone who wrote <c>acpp</c> precisely because the SDK was broken would give
        /// them back the thing they were trying to get away from, and no clue why.
        /// </summary>
        internal static CopilotChatTransport Parse(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return CopilotChatTransport.Sdk;

            switch (raw.Trim().ToLowerInvariant())
            {
                case "sdk": return CopilotChatTransport.Sdk;
                case "acp": return CopilotChatTransport.Acp;
                default:
                    throw new InvalidOperationException(
                        $"L'impostazione '{SettingName}' vale '{raw}', che non so interpretare: " +
                        "i valori ammessi sono 'sdk' (quello normale) e 'acp' (la strada vecchia, di riserva). " +
                        "Correggila, oppure cancellala per tornare a 'sdk'.");
            }
        }
    }
}
