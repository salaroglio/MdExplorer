using MdExplorer.Features.Interfaces;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Commands.html
{
    public class FromExternalDataToPlantumlHtml : FromExternalDataToPlantuml, ICommandHtml
    {
        public FromExternalDataToPlantumlHtml(ILogger<FromExternalDataToPlantuml> logger)
            : base(logger)
        {
        }
    }
}
