using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Commands.Markdown
{
    public class FromHtmlCodeBlockToPreviewMD : FromHtmlCodeBlockToPreview, ICommandMD
    {
        public FromHtmlCodeBlockToPreviewMD(ILogger<FromHtmlCodeBlockToPreview> logger, IHelper helper)
            : base(logger, helper)
        {
        }
    }
}
