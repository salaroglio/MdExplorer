using MdExplorer.Features.Interfaces;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Commands.html
{
    internal class ProtectEmojiInUrlsHtml : ProtectEmojiInUrls, ICommandHtml
    {
        /// <summary>Runs on slide decks: keeps an emoji shortcode inside a URL from breaking the link.</summary>
        public bool WorksInSlides => true;

        public ProtectEmojiInUrlsHtml(ILogger<ProtectEmojiInUrls> logger) : base(logger)
        {
        }
    }
}
