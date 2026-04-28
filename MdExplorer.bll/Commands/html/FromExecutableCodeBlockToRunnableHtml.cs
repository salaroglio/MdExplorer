using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Commands.html
{
    public class FromExecutableCodeBlockToRunnableHtml : FromExecutableCodeBlockToRunnable, ICommandHtml
    {
        public FromExecutableCodeBlockToRunnableHtml(
            ILogger<FromExecutableCodeBlockToRunnable> logger,
            IHelper helper)
            : base(logger, helper)
        {
        }
    }
}
