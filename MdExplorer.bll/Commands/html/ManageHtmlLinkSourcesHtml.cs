using MdExplorer.Features.Interfaces;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Commands.html
{
    internal class ManageHtmlLinkSourcesHtml : ManageHtmlLinkSources, ICommandHtml
    {
        /// <summary>Anche nelle slide: un'immagine HTML deve vedersi come nella pagina.</summary>
        public bool WorksInSlides => true;

        public ManageHtmlLinkSourcesHtml(ILogger<ManageHtmlLinkSources> logger) : base(logger)
        {
        }
    }
}
