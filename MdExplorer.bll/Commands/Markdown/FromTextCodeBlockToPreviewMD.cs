using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Commands.Markdown
{
    public class FromTextCodeBlockToPreviewMD : FromTextCodeBlockToPreview, ICommandMD
    {
        public FromTextCodeBlockToPreviewMD(ILogger<FromTextCodeBlockToPreview> logger, IHelper helper)
            : base(logger, helper)
        {
        }
    }
}
