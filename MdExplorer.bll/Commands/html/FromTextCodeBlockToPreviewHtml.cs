using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Commands.html
{
    public class FromTextCodeBlockToPreviewHtml : FromTextCodeBlockToPreview, ICommandHtml
    {
        /// <summary>Runs on slide decks: text(file) includes a file as highlighted source, with no iframe.</summary>
        public bool WorksInSlides => true;

        public FromTextCodeBlockToPreviewHtml(ILogger<FromTextCodeBlockToPreview> logger, IHelper helper)
            : base(logger, helper)
        {
        }
    }
}
