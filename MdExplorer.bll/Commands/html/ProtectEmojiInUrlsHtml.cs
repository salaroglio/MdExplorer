using MdExplorer.Features.Interfaces;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Commands.html
{
    internal class ProtectEmojiInUrlsHtml : ProtectEmojiInUrls, ICommandHtml
    {
        public ProtectEmojiInUrlsHtml(ILogger<ProtectEmojiInUrls> logger) : base(logger)
        {
        }
    }
}
