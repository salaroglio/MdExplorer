using MdExplorer.Features.Interfaces;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Commands.pdf
{
    internal class ProtectEmojiInUrlsPdf : ProtectEmojiInUrls, ICommandPdf
    {
        public ProtectEmojiInUrlsPdf(ILogger<ProtectEmojiInUrls> logger) : base(logger)
        {
        }
    }
}
