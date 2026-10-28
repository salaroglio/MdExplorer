using MdExplorer.Features.Interfaces;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Commands.html
{
    internal class ManageLinkAbsolutePathHtml : ManageLinkAbsolutePath, ICommandHtml
    {
        /// <summary>Runs on slide decks: links to other files resolved against the project.</summary>
        public bool WorksInSlides => true;

        public ManageLinkAbsolutePathHtml(ILogger<ManageLinkAbsolutePath> logger) : base(logger)
        {
        }
    }
}
