using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Commands.html
{
    public class FromTextCodeBlockToPreviewHtml : FromTextCodeBlockToPreview, ICommandHtml
    {
        public FromTextCodeBlockToPreviewHtml(ILogger<FromTextCodeBlockToPreview> logger, IHelper helper)
            : base(logger, helper)
        {
        }
    }
}
