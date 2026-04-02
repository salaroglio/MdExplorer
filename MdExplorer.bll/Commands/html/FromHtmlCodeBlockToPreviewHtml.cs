using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Commands.html
{
    public class FromHtmlCodeBlockToPreviewHtml : FromHtmlCodeBlockToPreview, ICommandHtml
    {
        public FromHtmlCodeBlockToPreviewHtml(ILogger<FromHtmlCodeBlockToPreview> logger, IHelper helper)
            : base(logger, helper)
        {
        }
    }
}
